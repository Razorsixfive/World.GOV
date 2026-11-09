# Country source module template

Copy this folder to `src/<CountryName>.NET` beside `src/World.GOV.Core`, then rename `NO_COUNTRY.NET.csproj` to `<CountryName>.NET.csproj`. The folder name supplies the assembly name and, for simple lowercase slugs such as `brazil`, the country data folder name automatically. The boilerplate module reads the manifest and incident JSON files from `countries/<slug>`; no country-specific C# edits are needed to contribute historical data.

Add the module to the solution with `dotnet sln add src/<CountryName>.NET/<CountryName>.NET.csproj`. For a slug that cannot be derived from the folder name, set `CountrySlug` in the project file. Keep `extensionAssembly` null for a data-only contribution, or set it to `<CountryName>.NET` when registering the extension.
