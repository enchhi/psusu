# F4 — Filtriranje AI vrednosti iz baze + Generate TXT

**Spec (Projektni zadatak):** "dugme za otvaranje novog prozora za pretragu vrednosti AI tagova iz baze podataka. Korisnik može izabrati ime taga, vremenske trenutke (od-do) i vrednosti (od-do). Ukoliko ne unese ništa u neko polje, ne uzeti u obzir taj uslov. Generate TXT → .txt sa imenima tagova, vremenima i vrednostima koji su zadovoljili uslove."

## Dizajn
- **`AnalogSample`** — 4. DB tabela (`Id, TagName, Value, Timestamp`). Uvedena F4-om jer spec izričito traži "iz baze". Skeniranje (`OnSample`) upisuje uzorak i u bazu (uz in-memory istoriju za Report). `ContextClass.AnalogSamples`.
- **`SampleFilter.Apply(samples, tagName, from, to, min, max)`** (čista logika, testabilna) — `null`/prazan uslov se ignoriše.
- **`SampleTxtGenerator.Generate(samples)`** — `.txt`: `Ime | Vreme | Vrednost`.
- **Servis:** `SearchSamples(...)` učita uzorke iz baze (pod dbLock) i primeni `SampleFilter`.
- **GUI:** `FilterWindow` (polja: ime, vreme od/do, vrednost od/do; prazno = bez uslova), dugme **Pretrazi** (grid rezultata) + **Generate TXT**. MainWindow dugme **Pretraga**.

## Checklist
- [x] `AnalogSample` model + `ContextClass.AnalogSamples`
- [x] `SampleFilter` + `SampleTxtGenerator` + testovi (**45/45 zeleno**)
- [x] `OnSample` upisuje uzorke u bazu; `SearchSamples` u servisu
- [x] `FilterWindow` + Generate TXT; MainWindow dugme
- [ ] Verifikacija (VS): pusti AI da skenira par sekundi → Pretraga (npr. vrednost od 40 do 60) → grid + Generate TXT `.txt`

## Napomene
- Uzorci se pišu na svaku značajnu (deadband) promenu → volumen kontrolišu Deadband/ScanTime.
- Filter se primenjuje u memoriji nad učitanim uzorcima (dovoljno za projekat; za ogromne količine bi se guralo u SQL).
