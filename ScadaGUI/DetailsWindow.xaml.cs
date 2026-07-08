using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using DataConcentrator;

namespace ScadaGUI
{
    // F2: grafik istorije selektovanog AI + linije alarma + min/max/avg.
    public partial class DetailsWindow : DialogWindow
    {
        private readonly AnalogInput ai;
        private List<AnalogSample> samples;
        private List<Alarm> alarms;

        public DetailsWindow(AnalogInput analogInput)
        {
            InitializeComponent();
            ai = analogInput;
            HeaderText.Text = "AI: " + ai.Name + " (" + ai.IOAddress + ")";

            alarms = ai.Alarms.ToList();
            AlarmsGrid.ItemsSource = alarms;

            samples = DataConcentratorService.Instance
                .SearchSamples(ai.Name, null, null, null, null)
                .OrderBy(s => s.Timestamp)
                .ToList();

            var stats = SampleStats.Compute(samples.Select(s => s.Value));
            StatsText.Text = stats.Count == 0
                ? "Nema uzoraka u bazi (pusti skeniranje pa otvori ponovo)."
                : string.Format(CultureInfo.InvariantCulture,
                    "Uzoraka: {0}    Min: {1:0.##}    Max: {2:0.##}    Avg: {3:0.##}",
                    stats.Count, stats.Min, stats.Max, stats.Average);

            ChartCanvas.SizeChanged += (s, e) => DrawChart();
            Loaded += (s, e) => DrawChart();
        }

        private void DrawChart()
        {
            ChartCanvas.Children.Clear();
            double w = ChartCanvas.ActualWidth, h = ChartCanvas.ActualHeight;
            if (w <= 0 || h <= 0 || samples.Count == 0) return;

            // opseg = uzorci + granice alarma (da linije alarma budu vidljive)
            var values = samples.Select(s => s.Value).ToList();
            double vmin = values.Min(), vmax = values.Max();
            foreach (var a in alarms)
            {
                vmin = Math.Min(vmin, a.LimitValue);
                vmax = Math.Max(vmax, a.LimitValue);
            }
            double range = vmax - vmin;
            if (range < 1e-9) { vmin -= 1; vmax += 1; range = vmax - vmin; }

            const double pad = 8;
            double plotW = Math.Max(1, w - 2 * pad);
            double plotH = Math.Max(1, h - 2 * pad);

            Func<int, double> xOf = i =>
                pad + (samples.Count == 1 ? 0 : (double)i / (samples.Count - 1) * plotW);
            Func<double, double> yOf = val =>
                pad + (1 - (val - vmin) / range) * plotH;

            // linija istorije
            var accent = (Application.Current.TryFindResource("AccentBrush") as Brush) ?? Brushes.SteelBlue;
            var poly = new Polyline { Stroke = accent, StrokeThickness = 1.8 };
            for (int i = 0; i < samples.Count; i++)
                poly.Points.Add(new Point(xOf(i), yOf(samples[i].Value)));
            ChartCanvas.Children.Add(poly);

            // linije alarma (isprekidane crvene)
            foreach (var a in alarms)
            {
                double y = yOf(a.LimitValue);
                var line = new Line
                {
                    X1 = pad,
                    X2 = pad + plotW,
                    Y1 = y,
                    Y2 = y,
                    Stroke = Brushes.Red,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 4, 2 }
                };
                ChartCanvas.Children.Add(line);
            }
        }
    }
}
