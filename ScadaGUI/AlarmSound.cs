using System;
using System.Windows.Media;

namespace ScadaGUI
{
    // F1: zvuk alarma - loop dok ima aktivnog (neacknowledge-ovanog) alarma; jacina + izbor zvuka.
    public static class AlarmSound
    {
        private static readonly MediaPlayer player = new MediaPlayer();
        private static bool playing;

        public static double Volume { get; set; } = 0.7;                      // 0..1
        public static string SoundPath { get; set; } = @"C:\Windows\Media\Alarm01.wav";

        public static void Start()
        {
            if (playing) return;
            try
            {
                player.Open(new Uri(SoundPath));
                player.Volume = Volume;
                player.MediaEnded += Loop;
                player.Play();
                playing = true;
            }
            catch
            {
                // npr. zvucni fajl ne postoji - ignorisi
            }
        }

        private static void Loop(object s, EventArgs e)
        {
            player.Position = TimeSpan.Zero;
            player.Play();
        }

        public static void Stop()
        {
            if (!playing) return;
            player.MediaEnded -= Loop;
            player.Stop();
            playing = false;
        }

        public static void ApplyVolume()
        {
            player.Volume = Volume;
        }
    }
}
