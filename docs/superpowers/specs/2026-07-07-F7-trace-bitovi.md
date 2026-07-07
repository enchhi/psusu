# F7 — Trace-bitovi za logove

**Spec (Projektni zadatak):** "dodati skup checkbox-ova za izbor logova koji će se upisati u system.log fajl. Svaki checkbox predstavlja jedan bit. Čuvati traceword u numeričkom formatu."

## Dizajn
- `Logger` već ima `TraceWord` masku (bit po `LogCategory`). F7 dodaje UI za izbor i **perzistenciju** traceword-a.
- **Perzistencija:** `TraceWordStore` čuva traceword kao broj u fajlu `traceword.cfg` (app folder). Numerički format po spec-u.
- **UI:** prozor `TraceSettingsWindow` sa po jednim checkbox-om za svaku `LogCategory` (osim `None`). Stanje checkbox-a = da li je bit uključen. Na "Sačuvaj": sklopi traceword iz čekiranih, primeni na `Logger.Instance.TraceWord`, upiši u fajl, loguj akciju.
- **Startup:** na pokretanju aplikacije učitaj traceword iz fajla (default = sve kategorije uključene) i postavi `Logger.Instance.TraceWord`.
- Dugme **Trace settings** u glavnom prozoru otvara taj prozor.

## Checklist
- [ ] `TraceWordStore.Load(path, default)` / `Save(path, value)` + test (round-trip, missing→default)
- [ ] Startup: primeni traceword iz fajla na Logger (App/MainWindow)
- [ ] `TraceSettingsWindow` (checkbox po kategoriji, Save računa i perzistira traceword)
- [ ] Dugme "Trace settings" u MainWindow
- [ ] Registrovati fajlove u csproj (DataConcentrator + ScadaGUI)
- [ ] Verifikacija (VS): odčekiraj kategoriju → ta vrsta loga prestane da se upisuje u system.log; traceword preživi restart

## Van scope-a
Ostaje kako jeste: format log linije, kategorije (Login/Acknowledge/AddTag/.../System).
