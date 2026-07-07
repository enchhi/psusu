using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DataConcentrator;

namespace ScadaGUI
{
    // F7: checkbox po log kategoriji; svaki = jedan bit traceword-a.
    public partial class TraceSettingsWindow : Window
    {
        private readonly List<CheckBox> checks = new List<CheckBox>();

        public TraceSettingsWindow()
        {
            InitializeComponent();

            long current = Logger.Instance.TraceWord;
            foreach (LogCategory cat in Enum.GetValues(typeof(LogCategory)))
            {
                if (cat == LogCategory.None) continue;

                var cb = new CheckBox
                {
                    Content = cat.ToString(),
                    Tag = cat,
                    IsChecked = (current & (long)cat) != 0,
                    Margin = new Thickness(0, 3, 0, 3)
                };
                cb.Checked += (s, a) => UpdateTraceWordText();
                cb.Unchecked += (s, a) => UpdateTraceWordText();

                checks.Add(cb);
                ChecksPanel.Children.Add(cb);
            }

            UpdateTraceWordText();
        }

        private long ComputeTraceWord()
        {
            long tw = 0;
            foreach (var cb in checks)
                if (cb.IsChecked == true) tw |= (long)(LogCategory)cb.Tag;
            return tw;
        }

        private void UpdateTraceWordText()
        {
            TraceWordText.Text = "traceword = " + ComputeTraceWord();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            long tw = ComputeTraceWord();
            Logger.Instance.TraceWord = tw;
            TraceWordStore.Save(App.TraceWordPath, tw);
            Logger.Instance.Log(LogCategory.System, "Traceword promenjen na " + tw + ".");
            Close();
        }
    }
}
