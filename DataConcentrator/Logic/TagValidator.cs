using System.Collections.Generic;

namespace DataConcentrator
{
    // Rezultat validacije taga (namerno drugacije ime od DataAnnotations.ValidationResult).
    public class TagValidationResult
    {
        public List<string> Errors { get; } = new List<string>();
        public bool IsValid => Errors.Count == 0;
    }

    // Validacija unetih vrednosti taga po tipu (spec: onemogucavanje neadekvatnih podataka).
    public static class TagValidator
    {
        public static TagValidationResult Validate(Tag tag)
        {
            var r = new TagValidationResult();

            if (string.IsNullOrWhiteSpace(tag.Name))
                r.Errors.Add("Name je obavezan.");
            if (string.IsNullOrWhiteSpace(tag.IOAddress))
                r.Errors.Add("IOAddress je obavezan.");

            switch (tag)
            {
                case AnalogInput ai:
                    RequirePositiveScanTime(ai.ScanTime, r);
                    RequireLowLessThanHigh(ai.LowLimit, ai.HighLimit, r);
                    RequireUnits(ai.Units, r);
                    if (ai.Deadband < 0) r.Errors.Add("Deadband ne sme biti negativan.");
                    if (ai.Hysteresis < 0) r.Errors.Add("Hysteresis ne sme biti negativan.");
                    break;

                case AnalogOutput ao:
                    RequireLowLessThanHigh(ao.LowLimit, ao.HighLimit, r);
                    RequireUnits(ao.Units, r);
                    break;

                case DigitalInput di:
                    RequirePositiveScanTime(di.ScanTime, r);
                    break;

                case DigitalOutput dof:
                    if (dof.InitialValue != 0 && dof.InitialValue != 1)
                        r.Errors.Add("InitialValue digitalnog izlaza mora biti 0 ili 1.");
                    break;
            }

            return r;
        }

        private static void RequirePositiveScanTime(int scanTime, TagValidationResult r)
        {
            if (scanTime <= 0) r.Errors.Add("ScanTime mora biti pozitivan.");
        }

        private static void RequireLowLessThanHigh(double low, double high, TagValidationResult r)
        {
            if (low >= high) r.Errors.Add("LowLimit mora biti manji od HighLimit.");
        }

        private static void RequireUnits(string units, TagValidationResult r)
        {
            if (string.IsNullOrWhiteSpace(units)) r.Errors.Add("Units je obavezan za analogne tagove.");
        }
    }
}
