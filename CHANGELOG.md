# Changelog

## Unreleased

- Added English/Danish Cold War records for the January 1962 civil-defence leaflet and the 1982–1988 footnote policy, four checked sources, two distinct fictional archive dossiers, and GOV-020/GOV-021.

- Added CLASSIFIED / DECLASSIFIED / RECLASSIFIED dossier stamps and event-specific status messages, with English/Danish REGAN Vest jokes and English fallback for missing message translations.

- Added sourced Danish Cold War records for the 1949 North Atlantic Treaty signing and REGAN Vest, preserving the sources' differing construction end years.
- Added an optional fictional archive dossier with reversible redactions, English/Danish text, accessible controls, and English developer output; expanded GOV-007 and added GOV-019.

- Added Danish public translations for all four Germany records, with English developer output preserved.
- Expanded Germany with sourced records for the 1949 Basic Law and the 2020 BER Terminal 1 opening, two matching fictional GOV standards, and updated source and lookup indexes.
- Added a small executable GOV-003 budget policy: reporting succeeds after rebasing while original spending and overruns remain visible, with regression tests and a source-reading guide.
- Made the Windows launcher reuse an existing World.GOV portal and report unrelated port conflicts before building; added an optional `-Port` argument.
- Added English-first public website language catalogs, Danish interface text, partial-translation fallback, native language names, reading direction, and contributor coverage information.
- Added `translations new` and `translations status` commands, language-file validation, and a translation contributor guide.
- Kept Developer view, code, diagnostic/compiler output, simulated commits, and GOV standards in English regardless of website language; legacy translated technical fields remain compatible but are ignored.
- Fixed portal timeline ordering to use structured historical dates instead of display text.
- Kept shared-country incidents unique in the portal while including them in every listed country's filters, counts, and timeline.
- Limited the language chooser to English and available incident translations, independent of source languages.
- Added portal data and rendered timeline regression tests and clarified conduct reporting through issues or the owner's profile contact link.
- Added a portal screenshot and a local introductory-tour link near the top of the README.
- Aligned incident records with the v1 design contract: country ID arrays, structured historical dates, and stable source IDs.
- Added per-country source registries and exported source metadata for the static portal.
- Added validation and regression coverage for date precision, source resolution, and separated source publication.
- Updated contributor templates and synchronized the GDD copies with the implemented v1 contract.

## 0.4.0 — First Government Operational — 2026-11-04

- Local first-release implementation: sourced Denmark reference records and a Germany data-only demonstration.
- Static Blazor portal, introductory tour, Public/Developer presentations, timeline, standards, sources, and GOV diagnostics.
- JSON Schema enforcement, semantic validation, source registry, CLI, and static export.
- Regression tests for malformed input, missing sources, references, country paths, virtual history, and truthful build reports.
- Contributor starter files, evidence definitions, correction workflow, governance, and MIT license.
- Reproducible release script, local launcher, and GitHub Actions workflows that test before publishing. Deployment permissions are isolated from the build job.

This version has not been tagged or published to GitHub. Public hosting requires a repository and Pages configuration.

### Historical content update — 2026-11-04

- Added sourced Denmark records for the 1920 reunification, the 1940 German invasion and occupation, the 1945 liberation with Bornholm's distinct experience, the 1972 European Communities referendum, and the 2000 euro referendum.
- Kept the satirical operational memos and code examples explicitly separate from sourced historical summaries; added links between related events.
- Preserved the original four-event introductory tour as a curated subset while expanding the full timeline through 2000.
- Added Danish (`da-DK`) versions of all Denmark incident titles, historical summaries, satire, diagnostics, and relationship labels; untranslated records fall back to English.

## Factual corrections

No factual corrections registered yet. Add a dated entry here when changing a published claim, with incident ID, the change, and supporting source.
