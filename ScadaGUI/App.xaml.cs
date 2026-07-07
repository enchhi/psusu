using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ScadaGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Putanja gde se cuva traceword (F7), relativno na app folder.
        public const string TraceWordPath = "traceword.cfg";
    }
}
