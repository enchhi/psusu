using DataConcentrator;

namespace ScadaGUI
{
    // F5: trenutna sesija (ko je ulogovan i sa kojom ulogom).
    public static class Session
    {
        public static string Username { get; private set; }
        public static Role CurrentRole { get; private set; }

        // Samo admin ima Write; ostali samo Read.
        public static bool IsAdmin => CurrentRole == Role.Admin;

        public static void Set(string username, Role role)
        {
            Username = username;
            CurrentRole = role;
        }
    }
}
