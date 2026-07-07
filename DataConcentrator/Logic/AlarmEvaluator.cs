namespace DataConcentrator
{
    // Cista logika prelaza stanja alarma sa hysteresis-om (bez treperenja oko granice).
    //
    // Above: pali se kad value > limit; gasi se kad value < limit - hysteresis.
    // Below: pali se kad value < limit; gasi se kad value > limit + hysteresis.
    // Acknowledged ostaje dok se ne vrati u normalu.
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
                case AlarmState.Inactive:
                    return inAlarm ? AlarmState.Active : AlarmState.Inactive;
                case AlarmState.Active:
                    return cleared ? AlarmState.Inactive : AlarmState.Active;
                case AlarmState.Acknowledged:
                    return cleared ? AlarmState.Inactive : AlarmState.Acknowledged;
                default:
                    return current;
            }
        }
    }
}
