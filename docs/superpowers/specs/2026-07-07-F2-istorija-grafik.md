# F2 — Vizuelni prikaz istorije (grafik + min/max/avg)

**Spec (Projektni zadatak):** "grafički prikaz istorije selektovanog analog input-a u vidu grafika sa strane sa linijom za alarme koji su mu prikačeni i ispis min, max i average vrednosti za tu promenljivu."

## Dizajn
- Koristi `AnalogSample` iz baze (uveden u F4). U `DetailsWindow` (koji već prikazuje alarme AI-a) dodat je grafik + statistika.
- **`SampleStats.Compute(values)`** (čista logika, testabilna) → `Count/Min/Max/Average`.
- **Grafik:** crta se WPF primitivama na `Canvas`-u (**bez chart biblioteke**): `Polyline` istorije + isprekidane crvene `Line` na granicama prikačenih alarma. Opseg y-ose obuhvata i uzorke i granice alarma da linije budu vidljive. Skalira se na veličinu Canvas-a (redraw na `SizeChanged`).
- **Ispis min/max/avg** iznad grafika.

## Checklist
- [x] `SampleStats` + test (**47/47 zeleno**)
- [x] `DetailsWindow`: grafik (Polyline + linije alarma) + min/max/avg
- [ ] Verifikacija (VS): pusti AI da skenira, otvori Detalji → grafik crta krivu + crvene linije alarma + min/max/avg

## Napomene
- Istorija se čita iz baze (`SearchSamples(ime, ...)`), sortirana po vremenu.
- Ako nema uzoraka, prikaže se poruka umesto grafika.
