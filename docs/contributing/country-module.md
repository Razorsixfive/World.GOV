# Adding a country module

1. Create `countries/<slug>/country.json` using `schemas/country-v1.schema.json`.
2. Keep the stable country ID and slug unique.
3. Add sources to `countries/<slug>/sources.json`, assigning stable `SRC-CC-NNN` IDs and retaining original-language titles.
4. Add incident files using `schemas/incident-v1.schema.json`, list them in `incidentFiles`, and reference sources by ID.
5. Assign each incident one or more `countryIds` and geographic `regions` from the schema. Use `globalEventId` only when separate country records describe perspectives on the same historical event.
6. Record date precision, calendar, approximate dating, and ranges explicitly in the structured date fields; use `displayText` for readable historical wording.
7. Add localized historical text only when a reviewed translation exists. Keep `defaultLocale` among the actually available content locales.
8. Add standards as Markdown files with a first heading in the form `# GOV-001: Standard title`; list them in `standardFiles`.
9. Use typed relations for contextual, causal, or continuity links and write a short label describing the link.
10. Run `dotnet run --project src/World.GOV.Cli -- validate` and `dotnet build World.GOV.sln`.

Validation confirms structure and reference resolution. It does not replace source review or historical fact-checking.
