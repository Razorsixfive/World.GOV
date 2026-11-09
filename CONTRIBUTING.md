# Contributing

Start with the paired folders in `templates/NO_COUNTRY` and `templates/NO_COUNTRY.NET`. The copy-and-fill steps are below. Read [editorial policy](docs/editorial-policy.md) for evidence labels and corrections, and [governance](GOVERNANCE.md) for review and licensing. Run content validation and a solution build before submitting.

World.GOV welcomes small, reviewable contributions to shared tooling and country histories.

## Historical content

- Keep historical facts, interpretation, and fictional satire in distinct fields.
- Cite credible sources for factual claims. Mark disputed or uncertain details clearly.
- Do not invent quotations or present satirical text as historical evidence.
- Treat casualties and suffering respectfully; direct satire at institutional decisions and processes.
- Keep original-language source titles intact and include the source language.

## Country modules and incidents

1. Copy `templates/NO_COUNTRY` to `countries/<slug>`.
2. Copy `templates/NO_COUNTRY.NET` to `src/<CountryName>.NET`, then rename `NO_COUNTRY.NET.csproj` to `<CountryName>.NET.csproj`. This is ready-to-build boilerplate; contributors can leave its C# code unchanged while filling in the JSON data.
3. In `country.json`, replace `XX`, `new-country`, names, locales, and coverage. Add credible sources to `sources.json` with stable `SRC-CC-NNN` identifiers. Fill in `incidents/incident.json` with a stable ID, `countryIds`, correct `regions`, source IDs, and the factual and satirical text. Represent dates with explicit year/month/day fields, precision, calendar, and display text; set `circa` for approximate dates. For additional incidents, use `CC-YYYY-NNN.json` filenames and add each path to `incidentFiles`.
4. Add the starter project with `dotnet sln add src/<CountryName>.NET/<CountryName>.NET.csproj`. Its folder name supplies the assembly name and the lowercase country slug automatically for names such as `Brazil.NET`. For other slug formats, edit `CountrySlug` in the project file. Keep `extensionAssembly` null for data-only content, or set it to `<CountryName>.NET` when registering the extension.
5. Add a GOV standard only when an incident needs one; list it in `standardFiles`. Use typed relations with clear labels; relation targets may belong to another country.
6. Run `dotnet run --project src/World.GOV.Cli -- validate` and `dotnet build World.GOV.sln`.

The CLI validator checks local data structure and URL shape. Maintainers still review source quality and factual accuracy; the validator does not certify that a source is credible or reachable.

## Code and data

- Keep shared domain contracts in `World.GOV.Core`.
- Prefer data files for ordinary historical entries. Country-specific C# is optional.
- Preserve published incident IDs and slugs.
- Technical build failures must remain visible. Domain diagnostics do not block successful code builds.
- Do not add visitor-executed commands or arbitrary server-side code to the static site.

## Review and deployment

Pull requests run build and content validation with read-only permissions. Production deployment runs only from the reviewed `main` branch. Review covers technical correctness, source support, accurate uncertainty, accessibility, and satire boundaries.
