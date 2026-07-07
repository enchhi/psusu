using System;
using System.Windows;
using System.Windows.Controls;
using DataConcentrator;

namespace ScadaGUI
{
    // F5: login/registracija pre glavnog prozora.
    public partial class LoginWindow : Window
    {
        public string Username { get; private set; }
        public Role SelectedRole { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            RoleCombo.SelectedIndex = 0;

            // Dev pogodnost: pre-popunjena polja da se ne kuca svaki put pri testiranju.
            // Login i validacija ostaju netaknuti - samo pritisni Enter (dugme je IsDefault).
            // Obrisi ove tri linije kad zelis da demonstriras rucnu prijavu/registraciju.
            UsernameBox.Text = "admin";
            PassBox.Password = "Admin.Lozinka123!";
            RoleCombo.SelectedIndex = 0; // Admin
        }

        // Dev pomoc: nasumican NOV nalog (jedinstveno ime + validna jedinstvena lozinka + nasumicna uloga).
        // Zgodno za testiranje registracije i read-only uloga (Operater/Student/Teacher).
        private void Mock_Click(object sender, RoutedEventArgs e)
        {
            UsernameBox.Text = "user" + MockData.Int(1000, 9999);
            PassBox.Password = MockData.Password();
            RoleCombo.SelectedIndex = MockData.Int(0, RoleCombo.Items.Count - 1);
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var roleText = (RoleCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (!Enum.TryParse(roleText, out Role role))
            {
                MessageBox.Show("Izaberite ulogu.");
                return;
            }

            try
            {
                var result = AuthService.LoginOrRegister(UsernameBox.Text.Trim(), PassBox.Password, role);
                if (!result.Success)
                {
                    MessageBox.Show(string.Join("\n", result.Errors),
                        "Prijava neuspesna", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Username = UsernameBox.Text.Trim();
                SelectedRole = role;
                DialogResult = true; // zatvara prozor i vraca true u App.OnStartup
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greska pri prijavi (baza?):\n" + ex.Message,
                    "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
