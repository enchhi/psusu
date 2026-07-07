# SCADA Bazni Core — Implementacioni Plan

> **Za agentske izvršioce:** OBAVEZNA SUB-SKILL: koristi `superpowers:subagent-driven-development` (preporučeno) ili `superpowers:executing-plans` za implementaciju task-po-task. Koraci koriste checkbox (`- [ ]`) sintaksu.

**Goal:** Implementirati osnovnu SCADA funkcionalnost (tagovi, validacija, skeniranje, alarmi, perzistencija u 3 tabele, signalizacija bojom, Report, system.log) na postojećem kosturu.

**Architecture:** Tri projekta — `PLCSimulator` (izvor vrednosti, thread-safe), `DataConcentrator` (model + logika + EF), `ScadaGUI` (WPF). Logika je izdvojena u čiste, testabilne klase; niti/DB/UI su tanki omotači oko nje. Novi test projekat `DataConcentrator.Tests` (MSTest) pokriva logiku; UI se verifikuje ručno u VS.

**Tech Stack:** C# / .NET Framework 4.7.2 · WPF · Entity Framework 6 (Code First, LocalDB) · MSTest (V2).

**Referentni spec:** `docs/superpowers/specs/2026-07-07-scada-core-design.md` (+ `docs/Projektni-zadatak.pdf`).

## Global Constraints

- Target framework: **.NET Framework 4.7.2** (sva tri projekta + test projekat).
- Perzistencija: **EntityFramework 6**, **tačno 3 tabele**: `Tags`, `Alarms`, `ActivatedAlarms`. Mapiranje tagova = **TPH** (jedna `Tags` tabela).
- PLC adrese: **ADDR001–ADDR016** (AI 001–004, AO 005–008, DI 009/011–013, DO 010/014–016). Svaki pristup PLC-u pod `lock`-om.
- Signalizacija: **crveno** = aktivan alarm koji nije acknowledge-ovan, **žuto** = acknowledged.
- Report opseg: **`(HighLimit + LowLimit) / 2 ± 5`**.
- `system.log`: svaka akcija sa **timestamp-om**; logger već ima **trace-word** masku (bit po kategoriji).
- Nazivi klasa/varijabli na engleskom; komentari na srpskom (latinica).
- Verifikacija: logika → MSTest u VS Test Explorer; UI → ručni scenario u VS. (Autor plana ne može da build-uje na Linux-u.)

---

## Mapa fajlova

**PLCSimulator/**
- `PLCSimulatorManager.cs` (modifikuj) — sve adrese, `GetDigitalValue`, čist prekid niti.

**DataConcentrator/**
- `Model/Tag.cs` (modifikuj) — dodati `IOAddress`, `abstract TagType`.
- `Model/AnalogInput.cs`, `Model/AnalogOutput.cs`, `Model/DigitalInput.cs`, `Model/DigitalOutput.cs` (create).
- `Model/Alarm.cs`, `Model/ActivatedAlarm.cs` (create).
- `Model/Enums.cs` (create) — `TagType`, `AlarmDirection`, `AlarmState`, `LogCategory`.
- `Logic/TagValidator.cs` (create) — validacija.
- `Logic/AlarmEvaluator.cs` (create) — prelaz stanja alarma (hysteresis).
- `Logic/DeadbandFilter.cs` (create) — filtriranje po deadband-u.
- `Logic/AiHistoryStore.cs` (create) — in-memory istorija AI.
- `Logic/ReportGenerator.cs` (create) — Report `.txt` string.
- `Logging/Logger.cs` (create) — system.log + trace-word.
- `ContextClass.cs` (modifikuj) — DbSet-ovi + TPH.
- `PLC.cs` (modifikuj) — start/stop scan niti po tagu.
- `DataConcentrator.cs` (create) — fasada/orkestracija.

**DataConcentrator.Tests/** (create, MSTest)
- `PlcSimulatorTests.cs`, `TagValidatorTests.cs`, `AlarmEvaluatorTests.cs`, `DeadbandFilterTests.cs`, `AiHistoryStoreTests.cs`, `ReportGeneratorTests.cs`, `LoggerTests.cs`.

**ScadaGUI/**
- `MainWindow.xaml(.cs)` (modifikuj), `AddWindow.xaml(.cs)` (create), `WriteValueWindow.xaml(.cs)` (create), `DetailsWindow.xaml(.cs)` (create).
- `Converters/AlarmStatusToBrushConverter.cs` (create).
- `Resources/Strings.resx` (create — hook za F3), `Themes/Light.xaml` (create — hook za F1).
- `App.xaml(.cs)` (modifikuj) — startup flow (hook za F5), cleanup.

## Interfejsi (tačni potpisi — taskovi se oslanjaju na ovo)

```csharp
// Model/Enums.cs
public enum TagType { AI, AO, DI, DO }
public enum AlarmDirection { Above, Below }
public enum AlarmState { Inactive, Active, Acknowledged }
[Flags] public enum LogCategory {
    None=0, Login=1, Acknowledge=2, AddTag=4, UpdateTag=8, RemoveTag=16,
    AddAlarm=32, RemoveAlarm=64, WriteValue=128, Scan=256, ImportExport=512, Error=1024
}

// Logic/DeadbandFilter.cs
public static class DeadbandFilter {
    public static bool IsSignificant(double oldValue, double newValue, double deadband);
}

// Logic/AlarmEvaluator.cs
public static class AlarmEvaluator {
    public static AlarmState NextState(AlarmDirection dir, double limit, double hysteresis,
                                       double value, AlarmState current);
}

// Logic/TagValidator.cs
public class ValidationResult { public bool IsValid => Errors.Count==0; public List<string> Errors; }
public static class TagValidator {
    public static ValidationResult Validate(Tag tag);
}

// Logic/AiHistoryStore.cs
public class ValueSample { public DateTime Timestamp; public double Value; }
public class AiHistory { public string TagName; public double LowLimit; public double HighLimit;
                         public IReadOnlyList<ValueSample> Samples; }
public class AiHistoryStore {
    public void Record(string tagName, DateTime ts, double value);
    public AiHistory Get(string tagName, double low, double high);
    public IEnumerable<AiHistory> All(IEnumerable<AnalogInput> ais);
}

// Logic/ReportGenerator.cs
public static class ReportGenerator {
    public static string Generate(IEnumerable<AiHistory> histories); // opseg (high+low)/2 ± 5
}

// Logging/Logger.cs
public class Logger {
    public Logger(TextWriter writer, Func<DateTime> clock, long traceWord);
    public long TraceWord { get; set; }
    public void Log(LogCategory category, string message);
    public static Logger Instance { get; } // default: system.log + DateTime.Now + sve kategorije
}
```

---

## FAZA 1 — Logika (TDD, MSTest)

### Task 1: Test projekat + kompletiranje PLCSimulator-a

**Files:**
- Create: `DataConcentrator.Tests/` (VS template) + `DataConcentrator.Tests/PlcSimulatorTests.cs`
- Modify: `PLCSimulator/PLCSimulatorManager.cs`

**Interfaces:**
- Produces: `PLCSimulatorManager` sa svim adresama `ADDR001–ADDR016`, `double GetAnalogValue(string)`, `double GetDigitalValue(string)`, `SetAnalogValue`, `SetDigitalValue`, `Abort()`.

- [ ] **Step 1: Napravi test projekat.** U VS: *Add > New Project > Unit Test Project (.NET Framework)*, ime `DataConcentrator.Tests`, target **4.7.2**. Dodaj *Project Reference* na `DataConcentrator` i `PLCSimulator`. (Ovo je jedini ručni setup korak.)

- [ ] **Step 2: Napiši padajući test** `DataConcentrator.Tests/PlcSimulatorTests.cs`:

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PLCSimulator;

namespace DataConcentrator.Tests
{
    [TestClass]
    public class PlcSimulatorTests
    {
        [TestMethod]
        public void AllAddresses_AreKnown_NoMinusOne()
        {
            var plc = new PLCSimulatorManager();
            for (int i = 1; i <= 16; i++)
            {
                string addr = "ADDR" + i.ToString("000"); // ADDR001..ADDR016
                Assert.AreNotEqual(-1, plc.GetAnalogValue(addr), addr + " nije poznata");
            }
        }

        [TestMethod]
        public void SetAndGet_AnalogOutput_RoundTrips()
        {
            var plc = new PLCSimulatorManager();
            plc.SetAnalogValue("ADDR005", 42.5);
            Assert.AreEqual(42.5, plc.GetAnalogValue("ADDR005"), 1e-9);
        }

        [TestMethod]
        public void UnknownAddress_ReturnsMinusOne()
        {
            var plc = new PLCSimulatorManager();
            Assert.AreEqual(-1, plc.GetAnalogValue("ADDR999"));
        }
    }
}
```

- [ ] **Step 3: Pokreni test — mora da padne.** VS Test Explorer > Run. Očekivano: `AllAddresses_AreKnown` pada (adrese 006–008, 011–016 fale / vraćaju -1).

- [ ] **Step 4: Dopuni `PLCSimulatorManager` konstruktor** — dodaj sve adrese u `addressValues`:

```csharp
// AI 001-004, AO 005-008, DI 009/011-013, DO 010/014-016
for (int i = 1; i <= 16; i++)
    addressValues["ADDR" + i.ToString("000")] = 0;
```

Dodaj i getter za digitalne (isti izvor kao analogni):

```csharp
public double GetDigitalValue(string address)
{
    lock (locker)
        return addressValues.ContainsKey(address) ? addressValues[address] : -1;
}
```

Obmotaj `GetAnalogValue` telom pod `lock (locker)` (čitanje mora biti zaključano). Zameni tvrdi `Abort()` niti flag-om za čist prekid:

```csharp
private volatile bool running = true;
// u petljama: while (running) { ... }
public void Abort() { running = false; }
```

- [ ] **Step 5: Pokreni testove — svi prolaze.** Test Explorer: sva 3 zelena.

- [ ] **Step 6: Commit.**
```bash
git add PLCSimulator DataConcentrator.Tests PSUSUproject.sln
git commit -m "PLC: sve adrese ADDR001-016, GetDigitalValue, lock i cist prekid + test projekat"
```

---

### Task 2: Enums + model tagova

**Files:**
- Create: `DataConcentrator/Model/Enums.cs`, `AnalogInput.cs`, `AnalogOutput.cs`, `DigitalInput.cs`, `DigitalOutput.cs`
- Modify: `DataConcentrator/Model/Tag.cs`

**Interfaces:**
- Produces: klase iz §Interfejsi. `Tag` dobija `IOAddress`, `[NotMapped] double CurrentValue`, `abstract TagType Type`.

- [ ] **Step 1: `Model/Enums.cs`** — tačno kao u §Interfejsi (TagType, AlarmDirection, AlarmState, `[Flags] LogCategory`).

- [ ] **Step 2: Proširi `Tag.cs`** — dodaj (uz postojeći INPC obrazac iz kostura):
```csharp
private string ioAddress;
public string IOAddress { get => ioAddress; set { ioAddress = value; OnPropertyChanged("IOAddress"); } }

private double currentValue;
[System.ComponentModel.DataAnnotations.Schema.NotMapped]
public double CurrentValue { get => currentValue; set { currentValue = value; OnPropertyChanged("CurrentValue"); } }

public abstract TagType Type { get; }
```
`Tag` postaje `public abstract class Tag`.

- [ ] **Step 3: `AnalogInput.cs`** — `: Tag`, `override TagType Type => TagType.AI`, polja `ScanTime (int, ms)`, `OnScan (bool)`, `LowLimit`, `HighLimit`, `Units (string)`, `Deadband`, `Hysteresis` (svako sa INPC), i `public virtual ICollection<Alarm> Alarms { get; set; } = new List<Alarm>();`.

- [ ] **Step 4: `AnalogOutput.cs`** — `override Type => TagType.AO`, polja `LowLimit`, `HighLimit`, `Units`, `InitialValue`.

- [ ] **Step 5: `DigitalInput.cs`** — `override Type => TagType.DI`, polja `ScanTime`, `OnScan`.

- [ ] **Step 6: `DigitalOutput.cs`** — `override Type => TagType.DO`, polje `InitialValue`.

- [ ] **Step 7: Build (VS).** Bez grešaka. (Nema zasebnog testa — čiste data klase; pokrivene su indirektno kroz Task 3–6.)

- [ ] **Step 8: Commit.**
```bash
git add DataConcentrator/Model
git commit -m "Model: enums + AnalogInput/Output, DigitalInput/Output, IOAddress, CurrentValue"
```

---

### Task 3: Alarm + ActivatedAlarm + AlarmEvaluator (hysteresis)

**Files:**
- Create: `DataConcentrator/Model/Alarm.cs`, `Model/ActivatedAlarm.cs`, `Logic/AlarmEvaluator.cs`, `DataConcentrator.Tests/AlarmEvaluatorTests.cs`

**Interfaces:**
- Consumes: `AlarmDirection`, `AlarmState`.
- Produces: `Alarm { int Id, string Name, double LimitValue, AlarmDirection Direction, string Message, AlarmState State, string TagName }`; `ActivatedAlarm { int Id, int AlarmId, string TagName, string Message, DateTime Timestamp }`; `AlarmEvaluator.NextState(...)`.

- [ ] **Step 1: Napiši `AlarmEvaluatorTests.cs`:**
```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataConcentrator;

namespace DataConcentrator.Tests
{
    [TestClass]
    public class AlarmEvaluatorTests
    {
        // Above, limit 100, hysteresis 5
        [TestMethod] public void Above_Activates_WhenExceeds()
            => Assert.AreEqual(AlarmState.Active,
               AlarmEvaluator.NextState(AlarmDirection.Above, 100, 5, 101, AlarmState.Inactive));

        [TestMethod] public void Above_StaysInactive_BelowLimit()
            => Assert.AreEqual(AlarmState.Inactive,
               AlarmEvaluator.NextState(AlarmDirection.Above, 100, 5, 99, AlarmState.Inactive));

        [TestMethod] public void Above_HysteresisHold_DoesNotClearJustBelowLimit()
            => Assert.AreEqual(AlarmState.Active,
               AlarmEvaluator.NextState(AlarmDirection.Above, 100, 5, 97, AlarmState.Active)); // 97 > 100-5

        [TestMethod] public void Above_Clears_BelowLimitMinusHysteresis()
            => Assert.AreEqual(AlarmState.Inactive,
               AlarmEvaluator.NextState(AlarmDirection.Above, 100, 5, 94, AlarmState.Active)); // 94 < 95

        [TestMethod] public void Acknowledged_Persists_UntilCleared()
            => Assert.AreEqual(AlarmState.Acknowledged,
               AlarmEvaluator.NextState(AlarmDirection.Above, 100, 5, 101, AlarmState.Acknowledged));

        [TestMethod] public void Acknowledged_Clears_WhenReturnsToNormal()
            => Assert.AreEqual(AlarmState.Inactive,
               AlarmEvaluator.NextState(AlarmDirection.Above, 100, 5, 90, AlarmState.Acknowledged));

        [TestMethod] public void Below_Activates_WhenUnder()
            => Assert.AreEqual(AlarmState.Active,
               AlarmEvaluator.NextState(AlarmDirection.Below, 20, 3, 19, AlarmState.Inactive));

        [TestMethod] public void Below_Clears_AboveLimitPlusHysteresis()
            => Assert.AreEqual(AlarmState.Inactive,
               AlarmEvaluator.NextState(AlarmDirection.Below, 20, 3, 24, AlarmState.Active)); // 24 > 23
    }
}
```

- [ ] **Step 2: Pokreni — pada** (klasa ne postoji).

- [ ] **Step 3: `AlarmEvaluator.cs`:**
```csharp
namespace DataConcentrator
{
    public static class AlarmEvaluator
    {
        public static AlarmState NextState(AlarmDirection dir, double limit, double hysteresis,
                                           double value, AlarmState current)
        {
            bool inAlarm = dir == AlarmDirection.Above ? value > limit : value < limit;
            bool cleared = dir == AlarmDirection.Above ? value < limit - hysteresis
                                                       : value > limit + hysteresis;
            switch (current)
            {
                case AlarmState.Inactive:     return inAlarm ? AlarmState.Active : AlarmState.Inactive;
                case AlarmState.Active:       return cleared ? AlarmState.Inactive : AlarmState.Active;
                case AlarmState.Acknowledged: return cleared ? AlarmState.Inactive : AlarmState.Acknowledged;
                default:                      return current;
            }
        }
    }
}
```

- [ ] **Step 4: `Alarm.cs` i `ActivatedAlarm.cs`** — POCO klase sa poljima iz §Interfejsi (`[Key]` na `Id`). `Alarm` opciono INPC za `State` (radi live boje u GUI).

- [ ] **Step 5: Pokreni testove — svih 8 prolazi.**

- [ ] **Step 6: Commit.**
```bash
git add DataConcentrator/Model/Alarm.cs DataConcentrator/Model/ActivatedAlarm.cs DataConcentrator/Logic/AlarmEvaluator.cs DataConcentrator.Tests/AlarmEvaluatorTests.cs
git commit -m "Alarmi: model + AlarmEvaluator (above/below + hysteresis) sa testovima"
```

---

### Task 4: DeadbandFilter

**Files:** Create `DataConcentrator/Logic/DeadbandFilter.cs`, `DataConcentrator.Tests/DeadbandFilterTests.cs`

- [ ] **Step 1: Testovi:**
```csharp
[TestClass] public class DeadbandFilterTests {
    [TestMethod] public void ChangeAboveDeadband_IsSignificant()
        => Assert.IsTrue(DeadbandFilter.IsSignificant(10, 13, 2));
    [TestMethod] public void ChangeBelowDeadband_IsNotSignificant()
        => Assert.IsFalse(DeadbandFilter.IsSignificant(10, 11, 2));
    [TestMethod] public void ChangeEqualsDeadband_IsSignificant()
        => Assert.IsTrue(DeadbandFilter.IsSignificant(10, 12, 2));
    [TestMethod] public void ZeroDeadband_AnyRegister()
        => Assert.IsTrue(DeadbandFilter.IsSignificant(10, 10.0001, 0));
}
```
- [ ] **Step 2: Pokreni — pada.**
- [ ] **Step 3: Implementacija:**
```csharp
public static class DeadbandFilter {
    public static bool IsSignificant(double oldValue, double newValue, double deadband)
        => System.Math.Abs(newValue - oldValue) >= deadband;
}
```
(Napomena za scan engine: prvo očitavanje se tretira kao značajno; `oldValue` inicijalizovati na `double.NaN` i tretirati NaN kao „uvek značajno".)
- [ ] **Step 4: Pokreni — prolazi.**
- [ ] **Step 5: Commit.** `git commit -m "DeadbandFilter + testovi"`

---

### Task 5: TagValidator

**Files:** Create `DataConcentrator/Logic/TagValidator.cs`, `DataConcentrator.Tests/TagValidatorTests.cs`

**Pravila (iz §3.3 spec-a):** Name obavezno; ScanTime>0 (AI/DI); Low<High (AI/AO); Units nije prazno za analogne; Deadband/Hysteresis ≥0 (AI); InitialValue za DO ∈ {0,1}; IOAddress obavezno.

- [ ] **Step 1: Testovi** (po jedan primer po pravilu):
```csharp
[TestClass] public class TagValidatorTests {
    [TestMethod] public void ValidAnalogInput_Passes() {
        var ai = new AnalogInput { Name="T1", IOAddress="ADDR001", ScanTime=100,
            LowLimit=0, HighLimit=100, Units="C", Deadband=1, Hysteresis=1 };
        Assert.IsTrue(TagValidator.Validate(ai).IsValid);
    }
    [TestMethod] public void MissingName_Fails() {
        var ai = new AnalogInput { Name="", IOAddress="ADDR001", ScanTime=100,
            LowLimit=0, HighLimit=100, Units="C" };
        Assert.IsFalse(TagValidator.Validate(ai).IsValid);
    }
    [TestMethod] public void LowNotLessThanHigh_Fails() {
        var ai = new AnalogInput { Name="T", IOAddress="ADDR001", ScanTime=100,
            LowLimit=100, HighLimit=50, Units="C" };
        Assert.IsFalse(TagValidator.Validate(ai).IsValid);
    }
    [TestMethod] public void NonPositiveScanTime_Fails() {
        var di = new DigitalInput { Name="D", IOAddress="ADDR009", ScanTime=0 };
        Assert.IsFalse(TagValidator.Validate(di).IsValid);
    }
    [TestMethod] public void DigitalOutput_InitialOutsideBinary_Fails() {
        var dof = new DigitalOutput { Name="D", IOAddress="ADDR010", InitialValue=5 };
        Assert.IsFalse(TagValidator.Validate(dof).IsValid);
    }
    [TestMethod] public void AnalogOutput_EmptyUnits_Fails() {
        var ao = new AnalogOutput { Name="A", IOAddress="ADDR005", LowLimit=0, HighLimit=10, Units="" };
        Assert.IsFalse(TagValidator.Validate(ao).IsValid);
    }
}
```
- [ ] **Step 2: Pokreni — pada.**
- [ ] **Step 3: Implementacija** `TagValidator.Validate(Tag)` sa `switch` po `tag.Type`/tipu, dodaje poruke u `ValidationResult.Errors`. (Puni kod se piše u ovom koraku; pokriva svako pravilo gore.)
- [ ] **Step 4: Pokreni — svih 6 prolazi.**
- [ ] **Step 5: Commit.** `git commit -m "TagValidator sa validacionim pravilima + testovi"`

---

### Task 6: Logger (system.log + trace-word)

**Files:** Create `DataConcentrator/Logging/Logger.cs`, `DataConcentrator.Tests/LoggerTests.cs`

- [ ] **Step 1: Testovi** (koriste `StringWriter` + fiksni sat):
```csharp
[TestClass] public class LoggerTests {
    [TestMethod] public void Log_Writes_Timestamp_Category_Message() {
        var sw = new System.IO.StringWriter();
        var t = new System.DateTime(2026,7,7,10,0,0);
        var log = new Logger(sw, () => t, long.MaxValue);
        log.Log(LogCategory.Login, "user admin");
        StringAssert.Contains(sw.ToString(), "2026-07-07 10:00:00");
        StringAssert.Contains(sw.ToString(), "Login");
        StringAssert.Contains(sw.ToString(), "user admin");
    }
    [TestMethod] public void Log_Skips_WhenTraceBitOff() {
        var sw = new System.IO.StringWriter();
        var log = new Logger(sw, () => System.DateTime.Now, (long)LogCategory.Login); // samo Login bit
        log.Log(LogCategory.Scan, "scan tick");
        Assert.AreEqual(0, sw.ToString().Length);
    }
}
```
- [ ] **Step 2: Pokreni — pada.**
- [ ] **Step 3: Implementacija:** konstruktor prima `TextWriter`, `Func<DateTime>`, `traceWord`; `Log` piše samo ako `(TraceWord & (long)category) != 0`; format `yyyy-MM-dd HH:mm:ss | {category} | {message}`. `Instance` singleton default-uje na `StreamWriter("system.log", append:true){AutoFlush=true}`, `() => DateTime.Now`, `TraceWord = long.MaxValue`.
- [ ] **Step 4: Pokreni — prolazi.**
- [ ] **Step 5: Commit.** `git commit -m "Logger: system.log sa timestamp-om i trace-word maskom + testovi"`

---

### Task 7: AiHistoryStore + ReportGenerator

**Files:** Create `DataConcentrator/Logic/AiHistoryStore.cs`, `Logic/ReportGenerator.cs`, `DataConcentrator.Tests/AiHistoryStoreTests.cs`, `ReportGeneratorTests.cs`

- [ ] **Step 1: Testovi:**
```csharp
[TestClass] public class ReportGeneratorTests {
    [TestMethod] public void OnlyValuesInBand_AreIncluded() {
        // low=0 high=100 -> sredina 50, opseg [45,55]
        var h = new AiHistory { TagName="T1", LowLimit=0, HighLimit=100, Samples = new[] {
            new ValueSample{ Timestamp=new System.DateTime(2026,7,7,10,0,0), Value=50 }, // u opsegu
            new ValueSample{ Timestamp=new System.DateTime(2026,7,7,10,0,1), Value=80 }, // van
            new ValueSample{ Timestamp=new System.DateTime(2026,7,7,10,0,2), Value=46 }, // u opsegu
        }};
        var txt = ReportGenerator.Generate(new[]{ h });
        StringAssert.Contains(txt, "T1");
        StringAssert.Contains(txt, "50");
        StringAssert.Contains(txt, "46");
        Assert.IsFalse(txt.Contains(" 80"));
    }
}
[TestClass] public class AiHistoryStoreTests {
    [TestMethod] public void Record_And_Get_ReturnsSamples() {
        var s = new AiHistoryStore();
        s.Record("T1", new System.DateTime(2026,7,7,10,0,0), 12.3);
        var h = s.Get("T1", 0, 100);
        Assert.AreEqual(1, h.Samples.Count);
        Assert.AreEqual(12.3, h.Samples[0].Value, 1e-9);
    }
}
```
- [ ] **Step 2: Pokreni — pada.**
- [ ] **Step 3: Implementacija** `AiHistoryStore` (`Dictionary<string,List<ValueSample>>`, thread-safe uz `lock`) i `ReportGenerator.Generate` (za svaki AI: `mid=(low+high)/2`, uključi uzorke gde `Math.Abs(value-mid) <= 5`; format red po red `TagName | timestamp | value`).
- [ ] **Step 4: Pokreni — prolazi.**
- [ ] **Step 5: Commit.** `git commit -m "AiHistoryStore + ReportGenerator ((high+low)/2 +-5) + testovi"`

**✅ Milestone Faza 1:** logika kompletna i zeleno pokrivena testovima. Nezavisno vredna (biblioteka + simulator + testovi).

---

## FAZA 2 — Perzistencija + orkestracija

### Task 8: ContextClass (EF, 3 tabele, TPH)

**Files:** Modify `DataConcentrator/ContextClass.cs`. (Verifikacija: build + ručno, EF6 se teško unit-testira bez baze.)

- [ ] **Step 1:** Dodaj `DbSet<Alarm> Alarms`, `DbSet<ActivatedAlarm> ActivatedAlarms` (Tags već postoji). Ostavi singleton.
- [ ] **Step 2:** `OnModelCreating` — eksplicitno TPH nije obavezan (EF6 podrazumeva TPH za nasleđe), ali registruj naslednike: `modelBuilder.Entity<AnalogInput>(); ...` da EF uključi sve tipove. Alarm ↔ AnalogInput veza preko `TagName`.
- [ ] **Step 3:** Dodaj connection string u `App.config` (LocalDB) ili se osloni na EF default. Preporuka — eksplicitno u `DataConcentrator/App.config` i `ScadaGUI/App.config`:
```xml
<connectionStrings>
  <add name="ContextClass"
       connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ScadaDb;Integrated Security=True;MultipleActiveResultSets=True"
       providerName="System.Data.SqlClient"/>
</connectionStrings>
```
- [ ] **Step 4: Build + ručna verifikacija:** pokreni app (posle Faze 3) → baza `ScadaDb` se kreira sa 3 tabele. (Za sad samo build.)
- [ ] **Step 5: Commit.** `git commit -m "EF ContextClass: Tags/Alarms/ActivatedAlarms (TPH) + connection string"`

---

### Task 9: PLC scan niti (start/stop po tagu)

**Files:** Modify `DataConcentrator/PLC.cs`.

**Interfaces:**
- Produces: `PLC.StartScan(Tag inputTag, Action<Tag,double> onSample)`, `PLC.StopScan(string tagName)`, `PLC.StopAll()`.

- [ ] **Step 1:** Implementiraj `StartScan`: napravi `Thread` koji dok traje petlja: `Thread.Sleep(ScanTime)`, pod `lock`-om pročitaj vrednost preko `IOAddress`, pozovi `onSample(tag, value)`. Čuvaj u `tagThreads[tagName]`. Koristi `volatile bool` flag po niti (ne `Abort`).
- [ ] **Step 2:** `StopScan(name)` — postavi flag, ukloni iz `tagThreads`. `StopAll()` — sve niti + `Instance.Abort()` (simulator).
- [ ] **Step 3: Verifikacija:** ručno kroz GUI (Task 12) — vrednost se menja kad je scan ON. (Bez zasebnog unit testa — čista nit/vreme.)
- [ ] **Step 4: Commit.** `git commit -m "PLC: scan nit po input tagu (start/stop), lock, flag prekid"`

---

### Task 10: DataConcentrator fasada (orkestracija)

**Files:** Create `DataConcentrator/DataConcentrator.cs`.

**Interfaces (Produces):**
```csharp
public class DataConcentratorService {   // singleton .Instance
    public ObservableCollection<Tag> Tags { get; }
    public event Action<int> AlarmActivated;         // alarmId
    public void LoadFromDb();
    public void AddTag(Tag t);                        // validacija + DB + log
    public void RemoveTag(string name);
    public void AddAlarm(Alarm a);                    // vezan za AI
    public void RemoveAlarm(int id);
    public void Acknowledge(int alarmId);             // State=Acknowledged + log
    public void WriteValue(Tag output, double value); // PLC set + log
    public void StartScan(Tag inputTag);
    public void StopScan(string name);
    public string GenerateReport();                   // preko ReportGenerator
    public void Shutdown();                            // StopAll + SaveChanges + Dispose
}
```

- [ ] **Step 1:** Implementiraj skeletno telo koje koristi već testirane komadiće: `onSample` handler radi Deadband → `CurrentValue` → `AiHistoryStore.Record` → za svaki `Alarm` na tom AI `AlarmEvaluator.NextState`; na prelaz u Active upiši `ActivatedAlarm` u bazu, `SaveChanges`, `AlarmActivated?.Invoke(alarm.Id)`; sve akcije loguj preko `Logger.Instance`.
- [ ] **Step 2:** `AddTag` poziva `TagValidator.Validate` (baci/vrati grešku ako nije validan), doda u `Tags` + `ContextClass`, `SaveChanges`, `Logger.Log(AddTag,...)`.
- [ ] **Step 3:** `GenerateReport` → `ReportGenerator.Generate(historyStore.All(ais))`.
- [ ] **Step 4: Verifikacija:** build; puna provera kroz GUI (Faza 3).
- [ ] **Step 5: Commit.** `git commit -m "DataConcentratorService: orkestracija (scan->deadband->alarm->DB->event), akcije, report"`

**✅ Milestone Faza 2:** ceo backend radi bez UI-a.

---

## FAZA 3 — GUI (ručna verifikacija u VS)

> Za svaki task: „Verifikacija" = build u VS + opisani scenario; javi rezultat/greške.

### Task 11: Resursi (hookovi za F1/F3) + startup/cleanup

**Files:** Create `ScadaGUI/Resources/Strings.resx`, `ScadaGUI/Themes/Light.xaml`; Modify `App.xaml`, `App.xaml.cs`, `MainWindow.xaml.cs` (Window_Closing).

- [ ] **Step 1:** `Strings.resx` sa ključevima labela (Add, Remove, WriteValue, Report, Acknowledge, Details, TagName, Description, ...). Sav tekst u GUI ide iz resursa (hook za F3).
- [ ] **Step 2:** `Themes/Light.xaml` `ResourceDictionary` sa bojama/stilovima; merge u `App.xaml`. XAML koristi `DynamicResource` (hook za F1).
- [ ] **Step 3:** `App.xaml`: privremeno `StartupUri="MainWindow.xaml"` (F5 kasnije ubacuje login pre toga — ostavi komentar-hook).
- [ ] **Step 4:** `Window_Closing` → `DataConcentratorService.Instance.Shutdown()`.
- [ ] **Step 5: Verifikacija:** app se pokreće, prazan glavni prozor, zatvaranje čisto gasi niti (nema „visećih" procesa).
- [ ] **Step 6: Commit.** `git commit -m "GUI: resursi (Strings.resx), Light tema, startup/cleanup hookovi"`

### Task 12: MainWindow (DataGrid + dugmad + binding)

**Files:** Modify `ScadaGUI/MainWindow.xaml(.cs)`.

- [ ] **Step 1:** `DataGrid` bindovan na `DataConcentratorService.Instance.Tags` (kolone Type, Name, IOAddress, CurrentValue, Units, Status). Dugmad Add/Remove/WriteValue/Details/StartStopScan/Acknowledge/Report.
- [ ] **Step 2:** `MainWindow` konstruktor: `LoadFromDb()`, pretplata na `AlarmActivated` (u handleru preko `Dispatcher` osveži grid/status).
- [ ] **Step 3:** Dugmad zovu odgovarajuće metode servisa; Remove/Ack rade nad selektovanim redom.
- [ ] **Step 4: Verifikacija:** postojeći tagovi iz baze se prikažu; scan ON menja `CurrentValue` uživo.
- [ ] **Step 5: Commit.** `git commit -m "GUI: MainWindow DataGrid + dugmad + live binding"`

### Task 13: AddWindow (combo AI/AO/DI/DO/Alarm + dinamička polja + validacija)

**Files:** Create `ScadaGUI/AddWindow.xaml(.cs)`.

- [ ] **Step 1:** Combo na vrhu (`AI/AO/DI/DO/Alarm`). Paneli polja po tipu; na `SelectionChanged` prikaži samo relevantni panel (§3.1). Za „Alarm": izbor ciljnog AI + LimitValue + Above/Below + Message.
- [ ] **Step 2:** IOAddress combo se puni adresama koje odgovaraju tipu (AI→001–004, ...).
- [ ] **Step 3:** Na *Save*: konstruiši objekat, `TagValidator.Validate` (ili za alarm osnovna provera), na grešku `MessageBox` sa porukama i prekid; na uspeh `AddTag`/`AddAlarm` i zatvori.
- [ ] **Step 4: Verifikacija:** biranje tipa menja polja; nevalidan unos (npr. Units za digitalni je sakriven; Low>High) je odbijen porukom; validan tag se pojavi u gridu i u bazi.
- [ ] **Step 5: Commit.** `git commit -m "GUI: AddWindow (combo, dinamicka polja, validacija)"`

### Task 14: Signalizacija bojom (konverter)

**Files:** Create `ScadaGUI/Converters/AlarmStatusToBrushConverter.cs`; Modify `MainWindow.xaml`.

- [ ] **Step 1:** `IValueConverter`: ulaz = status AI (ima aktivan alarm? ack?), izlaz `Brushes.Red` (aktivan, nije ack), `Brushes.Yellow` (ack), `Brushes.Transparent`/normalno inače.
- [ ] **Step 2:** Primeni na red/ćeliju statusa u `DataGrid` preko binding-a + konvertera; osvežava se na `INotifyPropertyChanged`.
- [ ] **Step 3: Verifikacija:** kad AI pređe granicu red pocrveni; posle Acknowledge požuti; povratkom u normalu se očisti.
- [ ] **Step 4: Commit.** `git commit -m "GUI: signalizacija alarma bojom (crveno/zuto) preko konvertera"`

### Task 15: WriteValueWindow + DetailsWindow + Report dugme

**Files:** Create `ScadaGUI/WriteValueWindow.xaml(.cs)`, `DetailsWindow.xaml(.cs)`; Modify `MainWindow.xaml.cs`.

- [ ] **Step 1: WriteValue:** dijalog za unos vrednosti u selektovani AO/DO (validacija opsega / 0-1) → `WriteValue(...)`.
- [ ] **Step 2: Details:** za selektovani AI prikaži listu njegovih `Alarms` sa stanjima. (F2 kasnije dodaje grafik ovde.)
- [ ] **Step 3: Report dugme:** `SaveFileDialog` → upiši `GenerateReport()` u `.txt`.
- [ ] **Step 4: Verifikacija:** upis u AO/DO se vidi; Details prikazuje alarme; Report pravi `.txt` sa vrednostima u opsegu.
- [ ] **Step 5: Commit.** `git commit -m "GUI: WriteValue, Details, Report (.txt) dugme"`

### Task 16: Integracija logovanja + završna ručna provera

**Files:** Modify (razne) — osiguraj da sve akcije loguju.

- [ ] **Step 1:** Proveri da login(placeholder)/ack/add/update/remove/write/scan/report loguju u `system.log`.
- [ ] **Step 2: Verifikacija — pun scenario (§10 spec dizajna):** dodaj po jedan AI/AO/DI/DO; scan; alarm crveno→ack žuto→normalno; write; off scan; report; restart→učitavanje iz baze; `system.log` sadrži sve akcije sa vremenima.
- [ ] **Step 3: Commit.** `git commit -m "Integracija logovanja + zavrsna verifikacija baznog core-a"`

**✅ Milestone Faza 3:** kompletan bazni SCADA po specifikaciji.

---

## Self-Review (autor plana)

**1. Spec coverage** — provereno protiv §2 tabele dizajna: tagovi+validacija (T2,T5,T13), alarmi (T3,T13), signalizacija (T14), pisanje izlaza (T15), on/off scan (T9,T12), 3 tabele (T8), system.log (T6,T16), GUI/AddWindow/Details (T11–T15), Report (T7,T15), DC na promenu→alarm→DB→event→GUI (T10,T12), PLC lock/mapiranje (T1,T9). Nema praznina.

**2. Placeholder scan** — nema „TBD/kasnije"; UI taskovi imaju konkretne kontrole + scenario verifikacije (kod UI-a se piše u tasku, nije placeholder).

**3. Type consistency** — `NextState`, `IsSignificant`, `Validate`, `Generate`, `Log`, `Record/Get`, `DataConcentratorService` potpisi konzistentni kroz taskove i §Interfejsi.

## Napomena o feature-ima (posle core-a)
F5 login se ubacuje pre `MainWindow` (hook u T11/App.xaml); F3 koristi `Strings.resx` (T11); F1 dodaje `Themes/Dark.xaml` uz Light (T11); F4 uvodi DB tabelu istorije (reuse u F2); F7 dodaje UI za `TraceWord` (Logger već spreman, T6); F6 (JSON) serijalizuje `Tags`.
