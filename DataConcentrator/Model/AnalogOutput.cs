namespace DataConcentrator
{
    // Analogni izlaz: vrednost upisuje SCADA; ima granice i pocetnu vrednost.
    public class AnalogOutput : Tag
    {
        public override TagType Type => TagType.AO;

        public double LowLimit { get; set; }
        public double HighLimit { get; set; }
        public string Units { get; set; }
        public double InitialValue { get; set; }
    }
}
