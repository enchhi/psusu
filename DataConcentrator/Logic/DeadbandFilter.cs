using System;

namespace DataConcentrator
{
    // Deadband: promena se registruje samo ako je apsolutna razlika >= deadband.
    // (Prvo ocitavanje scan engine tretira kao znacajno tako sto stara vrednost bude NaN.)
    public static class DeadbandFilter
    {
        public static bool IsSignificant(double oldValue, double newValue, double deadband)
        {
            if (double.IsNaN(oldValue))
            {
                return true;
            }
            return Math.Abs(newValue - oldValue) >= deadband;
        }
    }
}
