namespace DataConcentrator
{
    // Digitalni ulaz: skenira se, vrednost 0/1.
    public class DigitalInput : Tag
    {
        public override TagType Type => TagType.DI;

        public int ScanTime { get; set; }   // interval skeniranja u ms
        public bool OnScan { get; set; }
    }
}
