using System.Collections.Generic;

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

        public virtual ICollection<Alarm> Alarms { get; set; } = new List<Alarm>();
    }
}
