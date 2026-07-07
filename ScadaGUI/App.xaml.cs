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

        // F5: login pre glavnog prozora; posle auto-logout-a se vraca na login.
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            while (true)
            {
                var login = new LoginWindow();
                if (login.ShowDialog() != true)
                {
                    Shutdown();   // korisnik odustao od prijave
                    return;
                }
                Session.Set(login.Username, login.SelectedRole);

                var main = new MainWindow();
                main.ShowDialog();

                if (!main.LoggedOutByTimeout)
                    break;        // normalno zatvaranje -> izlaz iz aplikacije
                // inace: auto-logout admina -> petlja nazad na login
            }

            Shutdown();
        }
    }
}
