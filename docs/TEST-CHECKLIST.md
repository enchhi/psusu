# SCADA — Test lista (kompletna verifikacija)

Prođi redom u Visual Studio. Format: **akcija → očekivano**. Čekiraj `[x]` kad prođe.

## 0. Build & pokretanje
- [ ] Otvori `PSUSUproject.sln` → NuGet restore prođe (EF preko packages.config, MSTest preko PackageReference).
- [ ] **Build Solution** (Ctrl+Shift+B) → bez grešaka.
- [ ] **Test → Test Explorer → Run All** → **sve zeleno** (~55 testova logike).
- [ ] **F5** → otvori se **Login** prozor (custom ljubičasta title traka + ikonica).

## 1. Login / Uloge (F5)
- [ ] Login sa `admin` / `Admin.Lozinka123!` / **Admin** → otvara se glavni prozor, u sidebar-u piše `admin` + `[Citanje/Pisanje]`.
- [ ] Zatvori app → ponovo F5 → login istim adminom → prolazi (postojeći korisnik).
- [ ] Novi user + **kratka lozinka** (npr. `abc`) → **odbija** sa porukom (min 15, veliko/malo/specijalni).
- [ ] Novi user sa validnom lozinkom + uloga **Operater/Student/Teacher** → uđe, ali **write dugmad (Dodaj/Ukloni/Upiši/Scan/Ack/Trace/Import) su izbledela/onemogućena**; Read (Detalji/Report/Export/Pretraga/Opcije) rade.
- [ ] (Opciono, sporo) Kao admin ne diraj ništa **5 min** → auto-logout na login.

## 2. Bazni core — tagovi + validacija
- [ ] **Dodaj** → AI: naziv `T1`, adresa **ADDR001**, scan `500`, ✅ On scan, Low `0`, High `100`, Units `C`, Deadband `1`, Hysteresis `2` → Sačuvaj → red u tabeli.
- [ ] **Vrednost** kolone se menja uživo (ADDR001 = sinus 0→100).
- [ ] Probaj **nevalidan** unos: prazan naziv, ili Low `100` / High `50` → **poruka, ne dozvoli**.
- [ ] Dodaj i **AO** (ADDR005), **DI** (ADDR009), **DO** (ADDR010) → svi u tabeli.
- [ ] (Mock dugme u title traci brzo popuni polja.)

## 3. Alarmi + signalizacija bojom
- [ ] **Dodaj** → Alarm → AI `T1`, Granica `50`, Smer **Above** → Sačuvaj.
- [ ] Kad vrednost pređe 50 → red **pocrveni**, tekst **čitljiv (taman)** u obe teme.
- [ ] Selektuj red → **Acknowledge** → red **požuti**, tekst čitljiv.
- [ ] Kad vrednost padne ispod ~48 → boja se očisti (hysteresis).
- [ ] `system.log` (u `ScadaGUI\bin\Debug\`) sadrži liniju o alarmu sa vremenom.

## 4. Pisanje u izlaz, Details, Scan on/off, Report
- [ ] Selektuj **AO** → **Upiši vrednost** → npr. `42` → vrednost se upiše.
- [ ] Selektuj **DO** → Upiši `1` (proba `5` → odbija, samo 0/1).
- [ ] Selektuj `T1` → **Detalji** → lista alarma + grafik (vidi F2).
- [ ] Selektuj `T1` → **Scan on/off** → vrednost stane; opet → nastavi.
- [ ] **Report** → sačuvaj `.txt` → sadrži vrednosti oko sredine opsega.

## 5. Perzistencija (3+ tabele)
- [ ] Zatvori app → pokreni ponovo (login) → **tagovi i alarmi učitani iz baze**.
- [ ] (U SSMS/VS SQL: baza `ScadaDb` ima tabele Tags, Alarms, ActivatedAlarms, AnalogSamples, Users.)

## 6. F1 — Zvuk + Tema
- [ ] Alarm pocrveni → **čuje se zvuk**; **Acknowledge → tišina**.
- [ ] **Opcije** → slajder **jačine** menja glasnoću; **combo** menja zvučni signal; **Testiraj zvuk/Stop**.
- [ ] **Opcije → Dark** → ceo app postane taman **uživo** (glavni, tabela, dugmad, combo, scrollbar, tooltip).
- [ ] Otvori **novi** dijalog (Dodaj/Detalji/Pretraga) → **i on je taman** (ne beli).
- [ ] **OS/naslovna traka** je tamna u Dark (Win10 20H1+/Win11).
- [ ] Nazad na **Light** → sve svetlo; **tekst na ljubičastim dugmadima/sidebar-u beo**.

## 7. F2 — Grafik istorije
- [ ] Pusti AI da skenira par sekundi → **Detalji** → **grafik krive** + **crvene isprekidane linije** granica alarma + ispis **min / max / avg**.

## 8. F3 — Lokalizacija
- [ ] **Opcije → Jezik → English** → dugmad/kolone/naslov na engleskom (uživo).
- [ ] Pređi mišem preko dugmadi → **tooltip** (na izabranom jeziku); bez selekcije disabled dugme kaže „Izaberite signal".
- [ ] Promeni **Format datuma** i **Vremensku zonu** → vidljivo u Pretrazi (kolona Vreme).

## 9. F4 — Filtriranje iz baze + TXT
- [ ] Pusti AI da nakupi uzorke → **Pretraga** → npr. vrednost od `40` do `60` → grid rezultata.
- [ ] Prazna polja se ignorišu (uslov se ne primenjuje).
- [ ] **Generate TXT** → `.txt` sa imenima, vremenima, vrednostima.

## 10. F6 — Export / Import (JSON)
- [ ] **Export** → sačuvaj `.json` (sadrži sve tagove).
- [ ] Obriši neki tag → **Import** tog `.json` → tag se vrati; poruka „Uvezeno tagova: N".

## 11. F7 — Trace-bitovi
- [ ] **Trace log** → odčekiraj neku kategoriju → ta vrsta loga prestane da se piše u `system.log`.
- [ ] Restart app → izbor traceword-a **preživeo** (fajl `traceword.cfg`).

## 12. UI / UX
- [ ] Svi **dijalozi imaju custom title bar** (akcenat + ikonica + naziv + **X**); prevlačenje pomera, ivice resize.
- [ ] **App ikonica** (ljubičasti logo) u title bar-u i **taskbar-u**.
- [ ] **Mock** dugme gore-desno u title traci (Dodaj/Pretraga/Upiši); **Login nema Mock**.
- [ ] Disabled dugmad su **izbledela** (ista familija), sa tooltip-om zašto.

---
**Napomena:** logiku (alarmi, deadband, validacija, hysteresis, JSON, lozinka, filter, stats...) pokriva ~55 automatskih testova (MSTest). Ručno gore proveravaš UI + bazu + integraciju.
