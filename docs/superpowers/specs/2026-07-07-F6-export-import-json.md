# F6 — Export/import konfiguracije (JSON)

**Spec (Projektni zadatak):** "omogućiti eksportovanje i importovanje konfiguracije svih tagova u json formatu."

## Dizajn
- **`ConfigSerializer`** (čista logika): `Export(IEnumerable<Tag>) -> string` i `Import(string) -> List<Tag>`.
  - Koristi **ugrađeni** `DataContractJsonSerializer` (bez spoljnog NuGet-a; radi i u net472 i u net8 pa je testabilno lokalno).
  - Polimorfizam rešen preko `TagDto` (ravan DTO sa poljem `Type` = AI/AO/DI/DO). `ToDto`/`FromDto` mapiranje po tipu.
- **Servis:** `ExportConfigJson()` (vrati JSON, loguj) i `ImportConfigJson(json)` (parsira, preskače postojeća imena, `AddTag` za svaki — validacija + DB + scan; loše preskoči i loguj; vrati broj dodatih).
- **GUI:** dugmad **Export** (SaveFileDialog → `.json`) i **Import** (OpenFileDialog → učita, osveži grid, poruka koliko dodato).

## Checklist
- [x] `ConfigSerializer.Export/Import` + `TagDto` + test (round-trip svih 4 tipa) — **39/39 zeleno**
- [x] Servis: `ExportConfigJson` / `ImportConfigJson`
- [x] GUI: Export/Import dugmad + handleri
- [x] csproj: `ConfigSerializer.cs` + referenca `System.Runtime.Serialization`
- [ ] Verifikacija (VS): Export → `.json` fajl sa svim tagovima; Import na praznoj bazi → tagovi se pojave

## Napomene
- Import preskače tagove čije ime već postoji (ne duplira ključ).
- Alarmi se ne exportuju (spec traži "konfiguraciju tagova"); mogu se dodati kasnije ako zatreba.
