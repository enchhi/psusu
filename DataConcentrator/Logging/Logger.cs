using System;
using System.Globalization;
using System.IO;

namespace DataConcentrator
{
    // Logovanje akcija u system.log sa vremenskim trenutkom.
    // TraceWord (numericka maska) odredjuje koje kategorije se upisuju - svaka je jedan bit (feature F7).
    public class Logger
    {
        private readonly TextWriter writer;
        private readonly Func<DateTime> clock;
        private readonly object sync = new object();

        public long TraceWord { get; set; }

        public Logger(TextWriter writer, Func<DateTime> clock, long traceWord)
        {
            this.writer = writer;
            this.clock = clock;
            TraceWord = traceWord;
        }

        public void Log(LogCategory category, string message)
        {
            // upisi samo ako je bit te kategorije ukljucen u masci
            if ((TraceWord & (long)category) == 0)
            {
                return;
            }

            lock (sync)
            {
                writer.WriteLine(
                    clock().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                    + " | " + category + " | " + message);
            }
        }

        #region Singleton (default: system.log, realno vreme, sve kategorije ukljucene)

        private static Logger instance;
        private static readonly object instanceLock = new object();

        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            var sw = new StreamWriter("system.log", append: true) { AutoFlush = true };
                            instance = new Logger(sw, () => DateTime.Now, long.MaxValue);
                        }
                    }
                }
                return instance;
            }
        }

        #endregion
    }
}
