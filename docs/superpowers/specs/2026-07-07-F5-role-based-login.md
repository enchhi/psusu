# F5 — Role-based access + login prozor

**Spec (Projektni zadatak):** uloge (admin, operater, student, teacher); login prozor pre glavnog (username, password, role); admin Read/Write, ostali Read; lozinka: 15 karaktera, bar veliko/malo slovo i specijalni karakter, jedinstvena u bazi, maskirana; auto-logout admina posle 5 min neaktivnosti.

## Dizajn
- **`Role`** enum (Admin/Operater/Student/Teacher). **`User`** model (Username, PasswordHash, Role) — 5. tabela (`ContextClass.Users`), uvedena F5-om.
- **Lozinka:** `PasswordPolicy.Validate` (≥15, veliko, malo, specijalni) + `PasswordHasher.Hash` (SHA-256, ne čuvamo plaintext) — obe **čista logika, testirane** (8 testova). Jedinstvenost = nema drugog usera sa istim hash-om.
- **`AuthService.LoginOrRegister`**: ako korisnik postoji → proveri hash + ulogu; ako ne → registruj uz pravila + jedinstvenost.
- **`LoginWindow`**: username, **PasswordBox** (maskirano), Role combo. Novo ime = registracija.
- **`Session`** (static): ko je ulogovan + `IsAdmin`.
- **App flow:** `App.OnStartup` prikaže login pa glavni prozor; posle auto-logout-a se vraća na login (petlja). Uklonjen `StartupUri`.
- **Access:** `MainWindow.ApplyRoleRestrictions` — non-admin: onemogućena write dugmad (Dodaj/Ukloni/Upiši/Scan/Ack/Trace/Import). Read (Detalji/Report/Export/Pretraga) uvek.
- **Auto-logout:** `DispatcherTimer` 5 min (samo admin); reset na miš/tastaturu; na isteku → logout → nazad na login. `ContextClass.Reset()` + `PLC.StopAll` nuluju kontekst/simulator da re-login radi.

## Checklist
- [x] Role, User, PasswordPolicy, PasswordHasher (+8 testova, 55/55), AuthService
- [x] ContextClass.Users; ContextClass.Reset + PLC null instance (re-login)
- [x] LoginWindow, Session, App.OnStartup flow
- [x] MainWindow role gating + inactivity auto-logout
- [ ] Verifikacija (VS): login (novo ime → registracija sa pravilima); admin=write, ostali=read; auto-logout posle 5 min

## Napomene
- Uloga se bira na login-u; za postojećeg korisnika mora da se poklopi sa upisanom.
- Auto-logout samo za admina (spec). Ostali nemaju write pa nema potrebe.
