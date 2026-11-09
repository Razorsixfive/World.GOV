# Country data folder template

Copy this folder to `countries/<slug>` and fill in `country.json`, `sources.json`, and `incidents/incident.json`.

Replace every example value in the incident and source registry, assign one or more correct `regions`, cite credible HTTPS sources by stable source ID, and include only reviewed translations. The example is intentionally incomplete and will fail content validation until the sample source is replaced and the fields are reviewed. Record date precision, calendar, approximate dates, and ranges explicitly. When adding more incidents, use stable ID-based filenames and add each path to `incidentFiles`.

Leave `extensionAssembly` null when contributing historical data. The paired `templates/NO_COUNTRY.NET` folder is ready for a country-specific C# extension when one is useful.
