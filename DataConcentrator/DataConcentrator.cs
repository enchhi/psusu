using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;

namespace DataConcentrator
{
    // Fasada koja povezuje sve slojeve: model, PLC (skeniranje), alarme, istoriju, log i bazu.
    // Sve DB operacije su serijalizovane preko dbLock (EF DbContext nije thread-safe,
    // a alarmi se upisuju iz scan niti).
    public class DataConcentratorService
    {
        private static DataConcentratorService instance;

        public static DataConcentratorService Instance
        {
            get { return instance ?? (instance = new DataConcentratorService()); }
        }

        private readonly object dbLock = new object();
        private readonly AiHistoryStore history = new AiHistoryStore();
        private readonly ConcurrentDictionary<string, double> lastValue = new ConcurrentDictionary<string, double>();

        // Svi tagovi (za GUI binding).
        public ObservableCollection<Tag> Tags { get; } = new ObservableCollection<Tag>();

        // Podize se kad se alarm aktivira (prosledjuje se Alarm.Id).
        public event Action<int> AlarmActivated;

        private DataConcentratorService() { }

        #region Ucitavanje iz baze

        public void LoadFromDb()
        {
            lock (dbLock)
            {
                var ctx = ContextClass.Instance;
                Tags.Clear();
                foreach (var tag in ctx.Tags.ToList())
                {
                    Tags.Add(tag);
                }

                // Alarms je [NotMapped] -> rucno povezujemo alarme sa njihovim AI po TagName-u.
                var alarms = ctx.Alarms.ToList();
                foreach (var ai in Tags.OfType<AnalogInput>())
                {
                    ai.Alarms = alarms.Where(a => a.TagName == ai.Name).ToList();
                }
            }

            Logger.Instance.Log(LogCategory.System, "Ucitana konfiguracija iz baze.");

            // Pokreni skeniranje za input tagove koji su OnScan.
            foreach (var tag in Tags.ToList())
            {
                if (IsInputOnScan(tag))
                {
                    PLC.StartScan(tag, ScanTimeOf(tag), OnSample);
                }
            }
        }

        #endregion

        #region Tagovi

        public void AddTag(Tag tag)
        {
            var v = TagValidator.Validate(tag);
            if (!v.IsValid)
            {
                throw new ArgumentException(string.Join("; ", v.Errors));
            }

            lock (dbLock)
            {
                ContextClass.Instance.Tags.Add(tag);
                ContextClass.Instance.SaveChanges();
            }

            Tags.Add(tag);
            Logger.Instance.Log(LogCategory.AddTag, "Dodat tag " + tag.Name + " (" + tag.Type + ").");

            if (IsInputOnScan(tag))
            {
                StartScan(tag);
            }
        }

        public void RemoveTag(string name)
        {
            StopScan(name);

            var tag = Tags.FirstOrDefault(t => t.Name == name);
            if (tag == null) return;

            lock (dbLock)
            {
                var ctx = ContextClass.Instance;
                var db = ctx.Tags.Find(name);
                if (db != null) ctx.Tags.Remove(db);

                foreach (var a in ctx.Alarms.Where(x => x.TagName == name).ToList())
                {
                    ctx.Alarms.Remove(a);
                }
                ctx.SaveChanges();
            }

            Tags.Remove(tag);
            Logger.Instance.Log(LogCategory.RemoveTag, "Uklonjen tag " + name + ".");
        }

        #endregion

        #region Alarmi

        public void AddAlarm(Alarm alarm)
        {
            lock (dbLock)
            {
                ContextClass.Instance.Alarms.Add(alarm);
                ContextClass.Instance.SaveChanges();
            }

            var ai = Tags.OfType<AnalogInput>().FirstOrDefault(t => t.Name == alarm.TagName);
            ai?.Alarms.Add(alarm);
            Logger.Instance.Log(LogCategory.AddAlarm, "Dodat alarm '" + alarm.Name + "' na " + alarm.TagName + ".");
        }

        public void RemoveAlarm(int id)
        {
            lock (dbLock)
            {
                var ctx = ContextClass.Instance;
                var a = ctx.Alarms.Find(id);
                if (a != null)
                {
                    ctx.Alarms.Remove(a);
                    ctx.SaveChanges();
                }
            }

            foreach (var ai in Tags.OfType<AnalogInput>())
            {
                var toRemove = ai.Alarms.FirstOrDefault(a => a.Id == id);
                if (toRemove != null) ai.Alarms.Remove(toRemove);
            }
            Logger.Instance.Log(LogCategory.RemoveAlarm, "Uklonjen alarm " + id + ".");
        }

        public void Acknowledge(int alarmId)
        {
            foreach (var ai in Tags.OfType<AnalogInput>())
            {
                var a = ai.Alarms.FirstOrDefault(x => x.Id == alarmId);
                if (a != null && a.State == AlarmState.Active)
                {
                    a.State = AlarmState.Acknowledged;
                    Logger.Instance.Log(LogCategory.Acknowledge, "Acknowledge alarma " + alarmId + " (" + ai.Name + ").");
                }
            }
        }

        #endregion

        #region Pisanje u izlaze

        public void WriteValue(Tag output, double value)
        {
            if (output.Type == TagType.AO)
            {
                PLC.Instance.SetAnalogValue(output.IOAddress, value);
            }
            else if (output.Type == TagType.DO)
            {
                PLC.Instance.SetDigitalValue(output.IOAddress, value);
            }
            else
            {
                throw new InvalidOperationException("Moguce je pisati samo u izlazne (AO/DO) tagove.");
            }

            output.CurrentValue = value;
            Logger.Instance.Log(LogCategory.WriteValue,
                "Upis " + value.ToString(CultureInfo.InvariantCulture) + " u " + output.Name + ".");
        }

        #endregion

        #region Skeniranje + obrada uzorka

        public void StartScan(Tag inputTag)
        {
            PLC.StartScan(inputTag, ScanTimeOf(inputTag), OnSample);
            SetOnScan(inputTag, true);
            Logger.Instance.Log(LogCategory.Scan, "Ukljuceno skeniranje " + inputTag.Name + ".");
        }

        public void StopScan(string name)
        {
            PLC.StopScan(name);
            var tag = Tags.FirstOrDefault(t => t.Name == name);
            if (tag != null) SetOnScan(tag, false);
            Logger.Instance.Log(LogCategory.Scan, "Iskljuceno skeniranje " + name + ".");
        }

        // Poziva se iz scan niti na svaku procitanu vrednost.
        private void OnSample(Tag tag, double value)
        {
            double old = lastValue.TryGetValue(tag.Name, out var v) ? v : double.NaN;
            double deadband = tag is AnalogInput aiDb ? aiDb.Deadband : 0;

            if (!DeadbandFilter.IsSignificant(old, value, deadband))
            {
                return;
            }

            lastValue[tag.Name] = value;
            tag.CurrentValue = value; // WPF marshaluje binding za skalarni property

            if (tag is AnalogInput ai)
            {
                var now = DateTime.Now;
                history.Record(ai.Name, now, value); // in-memory (za Report)

                // F4/F2: uzorak i u bazu (AnalogSample tabela)
                lock (dbLock)
                {
                    ContextClass.Instance.AnalogSamples.Add(
                        new AnalogSample { TagName = ai.Name, Value = value, Timestamp = now });
                    ContextClass.Instance.SaveChanges();
                }

                CheckAlarms(ai, value);
            }
        }

        private void CheckAlarms(AnalogInput ai, double value)
        {
            // Snapshot da ne pukne ako korisnik u medjuvremenu doda/ukloni alarm.
            foreach (var alarm in ai.Alarms.ToList())
            {
                var prev = alarm.State;
                var next = AlarmEvaluator.NextState(alarm.Direction, alarm.LimitValue, ai.Hysteresis, value, prev);
                if (next == prev) continue;

                alarm.State = next;

                // prelaz Inactive -> Active: upisi ActivatedAlarm i podigni event
                if (next == AlarmState.Active && prev == AlarmState.Inactive)
                {
                    var activated = new ActivatedAlarm
                    {
                        AlarmId = alarm.Id,
                        TagName = ai.Name,
                        Message = alarm.Message,
                        Timestamp = DateTime.Now
                    };

                    lock (dbLock)
                    {
                        ContextClass.Instance.ActivatedAlarms.Add(activated);
                        ContextClass.Instance.SaveChanges();
                    }

                    Logger.Instance.Log(LogCategory.AlarmRaised,
                        "ALARM: " + alarm.Message + " (" + ai.Name + " = " +
                        value.ToString(CultureInfo.InvariantCulture) + ").");
                    AlarmActivated?.Invoke(alarm.Id);
                }
            }
        }

        #endregion

        #region Report + Shutdown

        public string GenerateReport()
        {
            var ais = Tags.OfType<AnalogInput>();
            var report = ReportGenerator.Generate(history.All(ais));
            Logger.Instance.Log(LogCategory.Report, "Generisan Report.");
            return report;
        }

        // F6: export/import konfiguracije svih tagova u JSON.
        public string ExportConfigJson()
        {
            var json = ConfigSerializer.Export(Tags);
            Logger.Instance.Log(LogCategory.ImportExport, "Export konfiguracije (" + Tags.Count + " tagova).");
            return json;
        }

        public int ImportConfigJson(string json)
        {
            var imported = ConfigSerializer.Import(json);
            int added = 0;
            foreach (var t in imported)
            {
                if (Tags.Any(x => x.Name == t.Name)) continue; // preskoci postojece
                try
                {
                    AddTag(t); // validira + DB + scan
                    added++;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(LogCategory.Error, "Import preskocio " + t.Name + ": " + ex.Message);
                }
            }
            Logger.Instance.Log(LogCategory.ImportExport,
                "Import konfiguracije: dodato " + added + " od " + imported.Count + ".");
            return added;
        }

        // F4: pretraga AI uzoraka iz baze (prazni uslovi se ignorisu).
        public List<AnalogSample> SearchSamples(string tagName, DateTime? from, DateTime? to, double? min, double? max)
        {
            List<AnalogSample> all;
            lock (dbLock)
            {
                all = ContextClass.Instance.AnalogSamples.ToList();
            }
            var result = SampleFilter.Apply(all, tagName, from, to, min, max);
            Logger.Instance.Log(LogCategory.ImportExport, "Pretraga uzoraka: " + result.Count + " rezultata.");
            return result;
        }

        public void Shutdown()
        {
            PLC.StopAll();
            lock (dbLock)
            {
                try { ContextClass.Instance.SaveChanges(); } catch { /* ignorisi pri zatvaranju */ }
                ContextClass.Instance.Dispose();
            }
            Logger.Instance.Log(LogCategory.System, "Aplikacija zatvorena.");
        }

        #endregion

        #region Pomocne

        private static bool IsInputOnScan(Tag tag)
        {
            return (tag is AnalogInput ai && ai.OnScan) || (tag is DigitalInput di && di.OnScan);
        }

        private static int ScanTimeOf(Tag tag)
        {
            if (tag is AnalogInput ai) return ai.ScanTime;
            if (tag is DigitalInput di) return di.ScanTime;
            return 100;
        }

        private static void SetOnScan(Tag tag, bool on)
        {
            if (tag is AnalogInput ai) ai.OnScan = on;
            else if (tag is DigitalInput di) di.OnScan = on;
        }

        #endregion
    }
}
