using System;
using System.Collections.Generic;
using System.Threading;
using PLCSimulator;

namespace DataConcentrator
{
    // Omotac oko PLC simulatora + upravljanje scan nitima (jedna nit po input tagu).
    public class PLC
    {
        public static PLCSimulatorManager instance;

        // Niti skeniranja po imenu taga i njihove "running" zastavice.
        public static Dictionary<string, Thread> tagThreads = new Dictionary<string, Thread>();
        private static readonly Dictionary<string, bool> tagRunning = new Dictionary<string, bool>();
        private static readonly object scanLock = new object();

        public static PLCSimulatorManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PLCSimulatorManager();
                    instance.StartPLCSimulator();
                }
                return instance;
            }
        }

        // Pokrece skeniranje jednog input taga (AI ili DI).
        // scanTimeMs = interval; onSample(tag, value) se poziva na svaku procitanu vrednost.
        public static void StartScan(Tag inputTag, int scanTimeMs, Action<Tag, double> onSample)
        {
            lock (scanLock)
            {
                if (tagThreads.ContainsKey(inputTag.Name))
                {
                    return; // vec skenira
                }

                tagRunning[inputTag.Name] = true;

                var t = new Thread(() =>
                {
                    while (IsRunning(inputTag.Name))
                    {
                        Thread.Sleep(scanTimeMs > 0 ? scanTimeMs : 100);

                        double value = Instance.GetAnalogValue(inputTag.IOAddress); // isti izvor i za DI (0/1)
                        onSample(inputTag, value);
                    }
                })
                { IsBackground = true };

                tagThreads[inputTag.Name] = t;
                t.Start();
            }
        }

        public static void StopScan(string tagName)
        {
            lock (scanLock)
            {
                tagRunning[tagName] = false;
                tagThreads.Remove(tagName);
            }
        }

        public static void StopAll()
        {
            lock (scanLock)
            {
                foreach (var key in new List<string>(tagRunning.Keys))
                {
                    tagRunning[key] = false;
                }
                tagThreads.Clear();
            }

            if (instance != null)
            {
                instance.Abort();   // gasi generatorske niti simulatora
                instance = null;    // sledeci pristup pravi svez simulator (npr. posle re-login-a)
            }
        }

        private static bool IsRunning(string name)
        {
            lock (scanLock)
            {
                return tagRunning.TryGetValue(name, out var r) && r;
            }
        }
    }
}
