namespace DataConcentrator
{
    // Digitalni izlaz: vrednost upisuje SCADA (0/1).
    public class DigitalOutput : Tag
    {
        public override TagType Type => TagType.DO;

        public double InitialValue { get; set; }
    }
}
