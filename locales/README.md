# Website languages

`en-US.json` is the required English source catalog. `da-DK.json` is the Danish website translation. New community languages use the same format; partial and blank entries fall back to English.

Start with the [translation contributor guide](../docs/contributing/translations.md). Use `worldgov translations new` to create a file and `worldgov translations status` to see progress. The normal `worldgov validate` command checks all language files before export.

Developer view, diagnostic/compiler output, code, simulated commits, and GOV standards remain English and are excluded from these catalogs.
