# F3 — Lokalizacija + UI polish (task #13)

**Spec (F3):** promena vremenske zone, formata datuma i jezika (srpski/engleski) za tekst svih labela; tool tip za svaku kontrolu na odgovarajućem jeziku.
**Polish (zahtev enchhi):** boje, tooltipovi, dugmad rade samo kad su upotrebljiva (inače tooltip objašnjava, npr. "Izaberite signal").

## Dizajn
- **`Localizer`** (static): `Language` (sr/en), `TimeZone`, `DateFormat`, `T(key)` prevod, `FormatTime(dt)` (zona + format), `Changed` event. Kontrole se pretplate i pozovu `ApplyLanguage()`.
- **`TimeConverter`** (IValueConverter): prikaz `DateTime` u izabranoj zoni/formatu (FilterWindow kolona Vreme).
- **MainWindow:** `ApplyLanguage` postavlja tekst svih dugmadi, zaglavlja kolona, naslov + tooltipove. `UpdateUi` (na promenu selekcije/uloge): **dugme radi samo kad je upotrebljivo** (Ukloni/Detalji/Scan/Ack traže selekciju/tip; Write samo AO/DO; write dugmad samo admin), a tooltip (lokalizovan) kaže zašto (`ToolTipService.ShowOnDisabled`).
- **SettingsWindow (Opcije):** combo Jezik (Srpski/English), Format datuma, Vremenska zona (+ tema i zvuk iz F1).

## Checklist
- [x] `Localizer` (sr/en, TZ, format) + `TimeConverter`
- [x] MainWindow: jezik za dugmad/kolone/naslov + tooltip na svakom dugmetu (i disabled)
- [x] Kontekstualna dugmad (selekcija + uloga) — polish
- [x] SettingsWindow: jezik / format datuma / vremenska zona
- [x] FilterWindow: prikaz vremena kroz TimeConverter
- [ ] Verifikacija (VS): promena jezika menja glavni ekran + tooltipove; TZ/format menjaju prikaz vremena; disabled dugme ima tooltip "Izaberite signal"

## Napomene o obimu (pošteno)
- Lokalizacioni mehanizam je kompletan i primenjen na **glavni prozor** (primarna površina) + Opcije + prikaz vremena. Interni labeli po dijalozima (AddWindow, Login, ...) su na srpskom; njihova lokalizacija je mehaničko proširenje istim `Localizer.T(...)` — može se dovršiti po potrebi posle prve VS verifikacije.
