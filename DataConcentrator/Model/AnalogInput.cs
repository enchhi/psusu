using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataConcentrator
{
    // Analogni ulaz: skenira se, ima granice, deadband, hysteresis i alarme.
    public class AnalogInput : Tag
    {
        public override TagType Type => TagType.AI;

        public int ScanTime { get; set; }        // interval skeniranja u ms
        public bool OnScan { get; set; }
        public double LowLimit { get; set; }
        public double HighLimit { get; set; }
        public string Units { get; set; }
        public double Deadband { get; set; }      // minimalna promena na koju se reaguje
        public double Hysteresis { get; set; }     // za paljenje/gasenje alarma bez treperenja

        // Alarmi se NE mapiraju kao EF navigacija (izbegavamo shadow FK kolonu).
        // Alarm entiteti se cuvaju u zasebnoj Alarms tabeli, a ova kolekcija se puni
        // u servisu po TagName-u (Alarm.TagName == AnalogInput.Name).
        [NotMapped]
        public ICollection<Alarm> Alarms { get; set; } = new List<Alarm>();
    }
}
