using System;
using System.Collections.Generic;
using System.Threading;

namespace PLCSimulator
{
    /// <summary>
    /// PLC Simulator
    ///
    /// 4 x ANALOG INPUT : ADDR001 - ADDR004
    /// 4 x ANALOG OUTPUT: ADDR005 - ADDR008
    /// DIGITAL INPUT    : ADDR009, ADDR011 - ADDR013
    /// DIGITAL OUTPUT   : ADDR010, ADDR014 - ADDR016
    /// </summary>
    public class PLCSimulatorManager
    {
        private Dictionary<string, double> addressValues;
        private object locker = new object();
        private Thread t1;
        private Thread t2;
        // Zastavica za cist prekid niti (umesto Thread.Abort koji je nepodrzan/obsolete).
        private volatile bool running = true;
        // Jedan Random za sve pozive (poziva se pod lock-om iz analogne niti).
        private static readonly Random random = new Random();

        public PLCSimulatorManager()
        {
            addressValues = new Dictionary<string, double>();

            // Sve adrese ADDR001 - ADDR016 (AI 001-004, AO 005-008, DI 009/011-013, DO 010/014-016).
            for (int i = 1; i <= 16; i++)
            {
                addressValues.Add("ADDR" + i.ToString("000"), 0);
            }
        }

        public void StartPLCSimulator()
        {
            t1 = new Thread(GeneratingAnalogInputs) { IsBackground = true };
            t1.Start();

            t2 = new Thread(GeneratingDigitalInputs) { IsBackground = true };
            t2.Start();
        }

        private void GeneratingAnalogInputs()
        {
            while (running)
            {
                Thread.Sleep(100);

                lock (locker)
                {
                    addressValues["ADDR001"] = 100 * Math.Sin((double)DateTime.Now.Second / 60 * Math.PI); // SINE
                    addressValues["ADDR002"] = 100 * DateTime.Now.Second / 60;                             // RAMP
                    addressValues["ADDR003"] = 50 * Math.Cos((double)DateTime.Now.Second / 60 * Math.PI);  // COS
                    addressValues["ADDR004"] = RandomNumberBetween(0, 50);                                 // RANDOM
                }
            }
        }

        private void GeneratingDigitalInputs()
        {
            while (running)
            {
                Thread.Sleep(1000);

                lock (locker)
                {
                    ToggleDigital("ADDR009");
                    ToggleDigital("ADDR011");
                    ToggleDigital("ADDR012");
                    ToggleDigital("ADDR013");
                }
            }
        }

        // Pomocna: 0 -> 1, 1 -> 0 (poziva se vec pod lock-om).
        private void ToggleDigital(string address)
        {
            addressValues[address] = addressValues[address] == 0 ? 1 : 0;
        }

        public double GetAnalogValue(string address)
        {
            lock (locker)
            {
                return addressValues.ContainsKey(address) ? addressValues[address] : -1;
            }
        }

        public double GetDigitalValue(string address)
        {
            lock (locker)
            {
                return addressValues.ContainsKey(address) ? addressValues[address] : -1;
            }
        }

        public void SetAnalogValue(string address, double value)
        {
            lock (locker)
            {
                if (addressValues.ContainsKey(address))
                {
                    addressValues[address] = value;
                }
            }
        }

        public void SetDigitalValue(string address, double value)
        {
            lock (locker)
            {
                if (addressValues.ContainsKey(address))
                {
                    addressValues[address] = value;
                }
            }
        }

        private static double RandomNumberBetween(double minValue, double maxValue)
        {
            var next = random.NextDouble();
            return minValue + (next * (maxValue - minValue));
        }

        public void Abort()
        {
            running = false;
        }
    }
}
