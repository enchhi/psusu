using System;

namespace DataConcentrator
{
    // Tip taga.
    public enum TagType { AI, AO, DI, DO }

    // Smer okidanja alarma u odnosu na granicu.
    public enum AlarmDirection { Above, Below }

    // Stanje alarma.
    public enum AlarmState { Inactive, Active, Acknowledged }

    // Kategorije logova; svaka je jedan bit u trace-word masci (feature F7).
    [Flags]
    public enum LogCategory
    {
        None = 0,
        Login = 1,
        Acknowledge = 2,
        AddTag = 4,
        UpdateTag = 8,
        RemoveTag = 16,
        AddAlarm = 32,
        RemoveAlarm = 64,
        WriteValue = 128,
        Scan = 256,
        ImportExport = 512,
        Error = 1024
    }
}
