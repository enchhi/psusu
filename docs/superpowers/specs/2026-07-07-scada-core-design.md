# SCADA — Dizajn baznog core-a

**Projekat:** PSUSU — SCADA aplikacija
**Dokument:** dizajn/spec baznog (osnovnog) dela, pre dodatnih funkcionalnosti (F1–F7)
**Datum:** 2026-07-07
**Stack:** C# / .NET Framework 4.7.2 · WPF · Entity Framework 6 (Code First) · SQL Server LocalDB

> Napomena o principu rada: osnovna specifikacija (`docs/Projektni-zadatak.pdf`) mora biti ispunjena **do tačke**. Dodaci su dozvoljeni samo kad su neophodni, objašnjivi i ne deluju "nabacano". Zato bazni core ima **tačno tri tabele** koje spec imenuje; istorija AI vrednosti u bazi uvodi se tek uz F4, koji je izričito traži.

---

## 1. Cilj

Napraviti SCADA sistem koji:
- dodaje/uklanja analogne i digitalne tagove (DI, DO, AI, AO) sa validacijom,
- dodaje/uklanja alarme nad AI tagovima,
- skenira ulazne tagove (on/off), čita vrednosti iz PLC simulatora,
- detektuje alarmna stanja i signalizira ih bojom (crveno/žuto),
- upisuje vrednosti u izlazne tagove,
- perzistira podatke (EF, 3 tabele),
- generiše Report `.txt`,
- loguje sve akcije korisnika u `system.log`.

## 2. Pokrivenost specifikacije (traceability)

| # | Zahtev iz spec-a | Gde se rešava u dizajnu |
|---|---|---|
| 1 | Dodavanje/uklanjanje AI/AO/DI/DO tagova sa svim osobinama | §3 Model, §6 GUI (AddWindow) |
| 2 | Osobine taga tačno po tipu (scan/onscan samo input; low/high/units samo analog; initial samo output; deadband/hysteresis samo AI; alarms samo AI) | §3.1 tabela polja, §3.3 validacija |
| 3 | Validacija i onemogućavanje neadekvatnih unosa | §3.3, §6.2 |
| 4 | Dodavanje/uklanjanje alarma nad AI (granica, above/below, poruka, stanje) | §3.2, §6 |
| 5 | Signalizacija AI u alarmnom stanju (crveno = nije ack, žuto = ack) | §5.3, §6.3 |
| 6 | Pisanje vrednosti u DO/AO izlaze | §5.4, §6.4 |
| 7 | Uključivanje/isključivanje skeniranja (on/off scan) | §5.1 |
| 8 | Perzistencija: 3 tabele (Tags, Alarms, ActivatedAlarms) preko EF | §3.4 |
| 9 | Logovanje svake akcije u system.log sa timestamp-om | §7 |
| 10 | GUI: jedan tab sa svim tagovima; Details za AI prikazuje alarme | §6.1, §6.5 |
| 11 | AddWindow: combo AI/AO/DI/DO/Alarm menja izgled prozora | §6.2 |
| 12 | Report dugme: `.txt` sa AI vrednostima u opsegu `(high+low)/2 ± 5` | §6.6 |
| 13 | Data Concentrator na svaku promenu proverava alarmnu zonu; upis alarma u bazu; event; GUI čita iz baze po ID-u i prikaže | §5.2, §5.3 |
| 14 | PLC mapiranje preko I/O address; zaključavanje pri čitanju/pisanju | §4, §5 |

## 3. Model podataka (`DataConcentrator`)

### 3.1 Hijerarhija tagova

`Tag` je apstraktna bazna klasa (već postoji u kosturu, `INotifyPropertyChanged`, `[Key] Name`). Iz nje izvodimo 4 klase. Polja po tipu:

| Polje | Tag(bazna) | AI | AO | DI | DO |
|---|:---:|:---:|:---:|:---:|:---:|
| `Name` (id, [Key]) | ✓ | ✓ | ✓ | ✓ | ✓ |
| `Description` | ✓ | ✓ | ✓ | ✓ | ✓ |
| `IOAddress` | ✓ | ✓ | ✓ | ✓ | ✓ |
| `ScanTime` (ms) | | ✓ | | ✓ | |
| `OnScan` (bool) | | ✓ | | ✓ | |
| `LowLimit` | | ✓ | ✓ | | |
| `HighLimit` | | ✓ | ✓ | | |
| `Units` | | ✓ | ✓ | | |
| `InitialValue` | | | ✓ | | ✓ |
| `Deadband` | | ✓ | | | |
| `Hysteresis` | | ✓ | | | |
| `Alarms` (lista) | | ✓ | | | |
| `CurrentValue` (runtime) | | ✓ | ✓ | ✓ | ✓ |

- `TagType` enum: `AI, AO, DI, DO` (za prikaz/filtriranje; sam tip je i C# klasa).
- `CurrentValue` je runtime vrednost (poslednja pročitana/upisana); prikazuje se u GUI preko binding-a.

### 3.2 Alarmi

`Alarm`:
- `Id` ([Key]) — jedinstven,
- `Name` / opis alarma,
- `LimitValue` (vrednost granice),
- `Direction` enum `Above` / `Below` (aktivira se kad pređe iznad/ispod granice),
- `Message` (poruka),
- `State` enum `Inactive` / `Active` / `Acknowledged`,
- veza (FK) na `AnalogInput` (`TagName`).

`ActivatedAlarm` (istorija odigranih alarma, po spec-u str. 2 — "id alarma, naziv veličine, poruka, vreme"):
- `Id` ([Key], auto),
- `AlarmId` (koji alarm),
- `TagName` (naziv veličine),
- `Message`,
- `Timestamp`.

### 3.3 Validaciona pravila (onemogućavanje neadekvatnih unosa)

- `Name`: obavezno, jedinstveno (primarni ključ).
- `IOAddress`: obavezno; bira se iz liste adresa koje **odgovaraju tipu** taga (AI→ADDR001–004, AO→ADDR005–008, DI→ADDR009/011–013, DO→ADDR010/014–016).
- `ScanTime`: pozitivan broj; polje aktivno **samo za AI/DI**.
- `OnScan`: samo AI/DI.
- `LowLimit`/`HighLimit`: brojevi, `Low < High`; samo AI/AO.
- `Units`: samo AI/AO (nikada za digitalne).
- `InitialValue`: broj; za DO samo `0` ili `1`; samo AO/DO.
- `Deadband`, `Hysteresis`: `>= 0`; samo AI.
- Nedozvoljena polja su u AddWindow-u **sakrivena/onemogućena** za tip koji ih ne podržava (ne samo validacija na submit).

### 3.4 EF mapiranje (perzistencija)

- **Table-Per-Hierarchy (TPH):** sve 4 klase tagova u jednoj tabeli **`Tags`** (EF sama pravi `Discriminator` kolonu). Poklapa se sa spec zahtevom "tabela tagovi" (jedna).
- Tabele: **`Tags`**, **`Alarms`**, **`ActivatedAlarms`** — tačno tri, kao u spec-u.
- `ContextClass : DbContext` (singleton, već u kosturu) dobija `DbSet<Tag> Tags`, `DbSet<Alarm> Alarms`, `DbSet<ActivatedAlarm> ActivatedAlarms`.
- LocalDB, baza se kreira automatski (Code First) pri prvom pokretanju.

## 4. PLCSimulator

- Popuniti **sve adrese ADDR001–ADDR016** u `addressValues` (kostur trenutno dodaje samo 001/005/009/010 → niti pišu u 002–004 i 011–013 i bacaju `KeyNotFoundException`). Ovo je popravka postojećeg bug-a u kosturu.
  - AI: ADDR001–004 (generišu se: sinus, ramp, kosinus, random),
  - AO: ADDR005–008 (upisuje ih SCADA),
  - DI: ADDR009, ADDR011–013 (generišu se — toggle),
  - DO: ADDR010, ADDR014–016 (upisuje ih SCADA).
- Sve čitanje/pisanje ide kroz `lock (locker)` (već postoji) — zaključavanje po spec-u.
- Metode: `GetAnalogValue`, `SetAnalogValue`, `SetDigitalValue`, `GetDigitalValue` (dodati getter za digitalne ako fali), `Abort`.

## 5. DataConcentrator — logika

### 5.1 Skeniranje (nit po input tagu)
- `PLC` singleton (kostur) drži `PLCSimulatorManager` i `Dictionary<string, Thread> tagThreads`.
- Za svaki **input** tag (AI, DI) sa `OnScan == true` pokreće se zasebna nit koja:
  1. spava `ScanTime`,
  2. pod `lock`-om čita vrednost iz PLC-a preko `IOAddress`,
  3. za AI: primeni **Deadband** — ako je `|nova − stara| < Deadband`, ignoriši; inače ažuriraj `CurrentValue`, upiši u **in-memory istoriju** (za Report), i pozovi proveru alarma,
  4. za DI: ažuriraj `CurrentValue` na promenu.
- **On/Off scan:** uključivanje pokreće nit, isključivanje je zaustavlja (uklanja iz `tagThreads`).

### 5.2 Detekcija alarma
- Pri svakoj relevantnoj promeni AI vrednosti, prođi kroz `Alarms` tog AI:
  - `Above`: aktivira se kad `value > LimitValue`; vraća se (deaktivira) kad `value < LimitValue − Hysteresis`.
  - `Below`: aktivira se kad `value < LimitValue`; vraća se kad `value > LimitValue + Hysteresis`.
  - **Hysteresis** sprečava treperenje oko granice.
- Kad alarm postane aktivan (a nije već bio): upiši `ActivatedAlarm` u bazu (`AlarmId`, `TagName`, `Message`, `Timestamp`), postavi `Alarm.State = Active`, i **podigni event `AlarmActivated(alarmId)`**.

### 5.3 Event ka GUI-u i signalizacija
- `DataConcentrator` izlaže event `AlarmActivated`. `ScadaGUI` se pretplati; u handleru (na UI niti preko `Dispatcher`) pročita `ActivatedAlarm`/`Alarm` iz baze po ID-u i osveži prikaz.
- Boje statusa AI:
  - **crveno** — postoji aktivan alarm koji **nije** acknowledge-ovan,
  - **žuto** — aktivan alarm je acknowledge-ovan (`Acknowledged`),
  - normalno — nema aktivnog alarma.
- Acknowledge (dugme u GUI): `Alarm.State = Acknowledged`, upis u log.

### 5.4 Pisanje u izlaze
- GUI poziva `DataConcentrator.WriteValue(tag, value)` → pod `lock`-om `PLC.SetAnalogValue/SetDigitalValue(IOAddress, value)` i ažurira `CurrentValue`.

### 5.5 In-memory istorija AI (za Report)
- `DataConcentrator` drži po AI tagu listu `(Timestamp, Value)` očitavanja **tokom rada aplikacije** (nije u bazi — bazni core ima tačno 3 tabele). Report je koristi. (F4/F2 kasnije uvode DB tabelu istorije.)

## 6. ScadaGUI (WPF)

### 6.1 MainWindow
- Jedan tab; `DataGrid` svih tagova: kolone Tip, Name, IOAddress, CurrentValue, Units, Status (boja).
- Dugmad: **Add**, **Remove**, **Write value**, **Details** (za AI), **On/Off scan**, **Acknowledge**, **Report**.
- Binding preko `ObservableCollection` + `INotifyPropertyChanged` (vrednosti se osvežavaju uživo).

### 6.2 AddWindow
- Combo box na vrhu: **AI / AO / DI / DO / Alarm**.
- Na promenu izbora dinamički se prikazuju/omogućavaju samo polja relevantna za taj tip (vidi §3.1/§3.3). Za "Alarm": bira se ciljni AI + granica + above/below + poruka.
- Submit radi validaciju; na grešku prikazuje poruku i ne dozvoljava unos.

### 6.3 Signalizacija
- Konverter boje (value/status → Brush) u `DataGrid` redu/statusnoj ćeliji: crveno/žuto/normalno (§5.3).

### 6.4 Write value
- Prozorčić za unos vrednosti u selektovani AO/DO (uz validaciju opsega/0-1).

### 6.5 Details (alarmi po AI)
- Klik na Details selektovanog AI otvara prikaz svih prikačenih alarma i njihovih stanja. (F2 kasnije ovde dodaje grafik.)

### 6.6 Report
- Generiše `.txt`: za svaki AI, sva očitavanja iz in-memory istorije koja su bila u opsegu `(HighLimit + LowLimit) / 2 ± 5`, sa vremenom i vrednošću.

## 7. Logovanje (`system.log`)

- `Logger` singleton (u `DataConcentrator` ili zaseban helper), piše u `system.log` (app folder) red po red: `timestamp | kategorija | poruka`.
- Loguju se: login, acknowledge, add/remove/update tag, add/remove alarm, write value, on/off scan, import/export, exception/error.
- **Trace-word hook (za F7):** logger ima masku (`traceword`, numerički) gde je svaka kategorija jedan bit; poruka se upisuje samo ako je bit uključen. U baznom core-u su svi bitovi uključeni (default), a F7 dodaje UI za izbor i čuva traceword.

## 8. Feature-svesni hook-ovi (bez implementacije feature-a sada)

Da kasnije ne prepravljamo sve:
- **F3 (lokalizacija):** tekst labela ide preko resource stringova (`.resx`) umesto hardkodovanja. U baznom core-u pravimo jedan podrazumevani resurs; F3 dodaje jezike/TZ/format.
- **F1 (teme):** stilovi preko `ResourceDictionary` (Light default) i `DynamicResource` u XAML-u; F1 dodaje Dark + zvuk.
- **F5 (login/role):** startni tok aplikacije ide preko login prozora pre `MainWindow` (`App.xaml` `StartupUri` → login). U baznom core-u placeholder login flow; F5 dodaje uloge, pravila lozinke, auto-logout.
- **F4/F2 (istorija u bazi):** biće dodata tabela AI uzoraka uz F4 (koji je eksplicitno traži), F2 je reuse-uje.

## 9. Rukovanje greškama

- Svaki pristup PLC-u pod `lock`-om.
- DB operacije u `try/catch`; greške se loguju u `system.log` i (gde je bitno) prikazuju korisniku.
- Zatvaranje aplikacije (`Window_Closing`): zaustaviti sve scan niti i simulator niti, `SaveChanges`, `Dispose` (kostur već ima zakomentarisan obrazac).
- Niti se čisto gase (flag/`Join` umesto tvrdog `Abort` gde je moguće).

## 10. Verifikacija (kako proveravamo da radi)

Pošto se build/run rade na Windows/VS strani (ne mogu ovde), verifikacija je ručna po scenarijima:
1. Build solution-a u VS (Debug) — bez grešaka.
2. Dodaj po jedan AI/AO/DI/DO tag → vidljivi u gridu; validacija odbija loše unose.
3. Uključi scan AI → `CurrentValue` se menja uživo.
4. Dodaj alarm na AI (npr. Above) → kad vrednost pređe granicu: red pocrveni, upiše se ActivatedAlarm, u system.log upis.
5. Acknowledge → red požuti.
6. Write value u AO/DO → vrednost se upisuje (vidljivo u simulatoru/gridu).
7. Off scan → vrednost prestaje da se menja.
8. Report → `.txt` sadrži očekivane vrednosti.
9. Restart → tagovi/alarmi učitani iz baze.

## 11. Van scope-a baznog core-a (radi se u feature-ima)

- Zvuk, Dark tema (F1); grafik istorije min/max/avg (F2); prevod/TZ/format/tooltipovi (F3); prozor za filtriranje iz baze + Generate TXT i DB tabela istorije (F4); uloge/login pravila/auto-logout (F5); JSON export/import (F6); checkbox trace-bitovi (F7).

## 12. Pretpostavke

- LocalDB je dostupan u VS okruženju; connection string podrazumevan (EF Code First).
- Digitalne vrednosti su `0/1` (double u simulatoru).
- Report koristi istoriju tekuće sesije (in-memory), što je u skladu sa spec-om (Report ne pominje bazu).
