# WORLD.GOV / DENMARK.NET
## Game & Experience Design Document (GDD)

> **Clean Code. Broken Politics.**  
> *Come for the bugs. Stay for the history.*  
> **Document version:** 0.3 — English-language GDD and technical implementation contract  
> **Initial release:** `v0.0.0 — Initial Bureaucracy`  
> **Status:** Design baseline; features and sourced historical content are delivered incrementally  
> **Initial implementation scope:** Denmark. **Architectural scope:** International.  
> **Project language:** English (`en-US`); local-language historical names and original sources are retained.

---

## 1. Executive Summary

**World.GOV** is an open-source C#/.NET project that represents countries and historical events as modules inside a highly professional-looking enterprise system. Its architecture, tests, documentation, and deployments must be real and carefully engineered. Its fictional government business logic is satirical: responsibility is delegated indefinitely, budget overruns become reported successes, and known problems persist under new standards identifiers.

**Denmark.NET** is the reference country implementation and the only country its creator commits to developing. Other countries can later be contributed by the community through a small, well-documented module contract. Historical events must have reliable sources, while characters, dialogue, technical metaphors, and jokes are explicitly fictional.

This document specifies shared systems, data contracts, acceptance criteria, and delivery milestones. It is an implementation plan, **not** completed source code or an exhaustive historical archive.

The project is simultaneously **a programming joke**, **a historical discovery experience**, and **an engineering portfolio/open-source project**. It is not a political party, a real government platform, or a conventional win/lose game. The term *GDD* describes the intended experience, systems, content, implementation, and production plan.

## 2. Vision and Design Principles

1. **Enterprise on the surface.** At first glance, the project should resemble a serious large-company software product: clear visual design, strict conventions, thoughtful documentation, CI/CD, and consistent naming.
2. **Satire beneath the surface.** The best jokes should be discovered by reading a service implementation, following a reference, inspecting a test, finding a GOV standard, or exploring a historical incident.
3. **History should teach.** Real events must include dates, context, uncertainty, and verifiable sources. Satire must never replace the factual explanation.
4. **The code must work.** Actual compiler errors must fail builds. Satirical `GOV` problems may appear as critical *domain diagnostics* but must not prevent technically correct builds.
5. **One country first.** Denmark is built through small, independent iterations. International extensibility is planned without burdening `v0.0.0` with an oversized framework.
6. **Contributions should be approachable.** A contributor should be able to add an event without knowing the entire C# codebase. Adding a country requires slightly more, but should follow a simple template.
7. **A hobby project, not another full-time job.** Small commits and excellent standalone jokes are valid progress. No artificial deadlines.

**The central paradox:** *The code must be clean. The government must not.*

## 3. Audience and Experience

| Audience | What they encounter | What makes it worthwhile |
|---|---|---|
| Developers | C#, Git history, diagnostics, clean architecture, tests, absurd comments | Hidden technical punchlines |
| History enthusiasts | Timelines, citations, related events | Real historical knowledge and context |
| General visitors | Official-looking status portal, country views, incidents, accessible explanations | Understandable satire without coding knowledge |
| Open-source contributors | Templates, guidelines, country modules, PR workflow | Opportunities to expand the simulated world |

**Experience progression:** Official portal → suspicious status indicator → absurd incident → historical facts → C# implementation → Git reference → next hidden joke.

## 4. Core Features

### 4.1 Countries as Modules

`World.GOV.Core` defines shared domain models and contracts. `Denmark.NET` is the reference implementation. A country provides at least metadata and historical event data; additional C# services and country-specific behavior are optional.

A country module contains:

- Stable country identity, display and native names, supported languages, and historical coverage.
- Historical events and incident datasets.
- Country-specific GOV standards and optional satirical services.
- References to internationally shared events.
- Sources and translations when available.

**Not required for the first release:** Implementing every country or providing a complex runtime assembly/plugin loader.

### 4.2 Historical Events and Incidents

Each event has **separate factual and satirical layers**:

**Factual layer:** What happened, when, relevant institutions/actors, context, evidence, and uncertainty.  
**Satirical layer:** Fictional commit message, diagnostics, technical metaphor, GOV references, and optional code examples.

Suggested categories: `Political`, `Economic`, `NaturalDisaster`, `War`, `PublicAdministration`, `Technology`, `Infrastructure`, `Social`, `Other`.

Historical evidence uses four qualitative labels, not numeric confidence scores: `Documented` (cited evidence supports the stated account), `Interpretive` (a named author's explanation or relationship is attributed and bounded), `Uncertain` (a material date, attribution, or detail remains unknown and is explained), and `Disputed` (credible accounts disagree and the disagreement is described with sources). Different historians' interpretations may be included with attribution.

**Example:** `DK-1849-001 — The June Constitution`

- **Historical fact:** Denmark adopted its constitution on June 5, 1849; absolute monarchy was replaced by constitutional monarchy.
- **Satirical commit:** `refactor(governance): replace AbsoluteMonarchy with ConstitutionalMonarchy`
- **Source display:** Verified archival material and historical scholarship, added and checked when the event is implemented.

For current cases, clearly distinguish allegations, charges, suspicions, convictions, and verified findings. Fictional people must not be presented as identical to actual individuals; unverified accusations must never be passed off as historical fact.

### 4.3 Two Histories, Without Rewriting Git

**Development history:** Real `git log` and `git blame` record actual contributor changes with their real dates. Adding a 1940 event in 2027 creates a genuine development commit in 2027.

**Simulated world history:** `worldgov history`, `worldgov blame`, and the website timeline sort events by the dates on which the events occurred and show **clearly labeled virtual commits**. They are not actual Git commits.

The website timeline can be scoped to all countries, a geographic region, or one country. Shared historical events use a stable event identifier to group country-specific perspectives into one timeline card. Regions describe where an event took place or had direct historical scope; they do not replace country modules.

Example:

```text
$ worldgov history DK
2026  chore: Review legacy administrative decisions
1949  feat: Install NATO security dependency
1945  fix: Restore national sovereignty
1940  security: Unauthorized foreign administration detected
1849  refactor: Constitutional governance introduced
```

`git notes` may eventually attach supplemental metadata, but they are **not required for the MVP**, and regular pushes do not automatically synchronize notes.

### 4.4 GOV Standards

GOV standards look like serious RFC/compliance documents, with `MUST`, `SHOULD`, and `MAY`, revision numbers, approval processes, and test references. Their internal governance requirements are intentionally absurd.

| ID | Name | Satirical premise |
|---|---|---|
| `GOV-000` | Initial Bureaucracy | A release does not require functionality |
| `GOV-001` | Engineering & Code Quality | Code quality is mandatory; usefulness is out of scope |
| `GOV-002` | Accountability Management | Responsibility is resolved when nobody owns it |
| `GOV-003` | Public Budget Allocation | Budget overruns can be reclassified |
| `GOV-005` | Committee Governance | Committees may create committees |
| `GOV-006` | Procurement & Vendor Selection | Lowest price is mistaken for best value |
| `GOV-007` | Historical Revision & Redaction | Documents may be marked `[REDACTED]` |
| `GOV-009` | Technical Debt Preservation | Legacy defects cannot be removed without approval |
| `GOV-014` | Mandatory Operational Continuity | Domain failures must not stop production |
| `GOV-HIST-001` | Historical Source Integrity | Factual claims require evidence |

Example:

```csharp
[CompliantWith("GOV-002")]
public sealed class AccountabilityService
{
    public Responsibility Assign(Incident incident)
        => Responsibility.None;
}
```

This example illustrates **fictional domain behavior**, not a recommended design for real accountability systems.

### 4.5 Compiler Diagnostics and Build Behavior

Every implemented country module must **genuinely compile**, assuming technically correct code. Satirical errors are a separate category.

**Standard build:** Actual `dotnet build`, strict checks; real compilation and test failures must fail the pipeline.  
**Government diagnostics:** A Roslyn Analyzer and/or a CLI/MSBuild reporting adapter generates `GOVxxxx` diagnostics. A problem may be labeled `CRITICAL` as a **domain severity**, while being emitted as warning/info or into a separate report when the technical build must still succeed. A genuine `DiagnosticSeverity.Error` cannot be ignored while claiming an otherwise ordinary successful build.

```text
$ dotnet build World.GOV.sln

GOV0001 CRITICAL (ignored): Accountability not implemented.
GOV0203 WARNING (acknowledged): Budget exceeded by 470%.
GOV0140 INFO: Operational continuity directive applied.

Build succeeded.
0 C# compiler errors
847 government incidents acknowledged (illustrative)
12,438 administrative warnings deferred (illustrative)
```

All counts in examples are **satirical placeholders** until computed from actual registered data. Normal CI must not hide real compilation, test, validation, or security failures.

### 4.6 Developer CLI

Proposed commands:

```text
worldgov countries list
worldgov history DK --year 1849
worldgov incident show DK-1849-001
worldgov blame DK/Government
worldgov trace DK-1849-001
worldgov standards list
worldgov diagnostics DK
worldgov export --format json
```

The CLI uses the same Core models and incident catalog as the site. The website's satirical terminal is a **read-only display / safe browser simulation**, never a facility for arbitrary remote shell execution.

## 5. Website

### 5.1 Visual Direction

Combine **GitHub, Microsoft Learn, an official government status portal, and an enterprise observability dashboard**. Typography, spacing, navigation, and component behavior should be consistent, responsive, and readable on mobile.

The landing page initially looks serious. Humor emerges gradually in metadata, notices, labels, details, and history. For example:

```text
WORLD.GOV / DENMARK.NET
Build: PASSING
Compliance: 100%
Unresolved incidents: 2,401
Responsible department: UNKNOWN
* Operational status does not imply correct behavior.
```

### 5.2 Pages

1. **Overview:** Country status, incidents, recent registrations, build output, rotating one-liner.
2. **Countries:** Available modules and metadata; eventually an interactive international map.
3. **Incident Explorer:** Filters for country, year, category, status, and evidence level.
4. **Incident Details:** Facts, sources, satirical code, GOV standard, historical relationships, and GitHub link.
5. **Historical Timeline:** Virtual history, `worldgov blame`, filtering, and sourced relationships.
6. **Build Console:** Authentic CI-generated diagnostic results plus clearly marked interactive demonstrations.
7. **GOV Standards:** Professionally formatted yet absurd compliance documentation.
8. **Contribute / About:** Onboarding, rules, and clear distinction between facts and fiction.

### 5.3 Two Reading Modes

- **Public View:** Readable summaries, historical context, dates, sources, and accessible jokes.
- **Developer View:** The same incident with C# snippets, diagnostics, tests, standards, GitHub references, and CLI details.

Both modes present **the same underlying incident data**.

## 6. Technical Architecture

**Preferred stack:** C#, .NET, Blazor WebAssembly on GitHub Pages, GitHub Actions, structured JSON, and automated tests. No permanently running backend is necessary for the initial release.

```text
World.GOV/
├── World.GOV.sln
├── src/
│   ├── World.GOV.Core/             # Domain models and interfaces
│   ├── World.GOV.Cli/              # History, trace, diagnostics
│   ├── World.GOV.Tooling/          # Validation and static export
│   ├── World.GOV.Diagnostics/      # Roslyn GOV analyzer (later)
│   ├── World.GOV.Web/              # Blazor WebAssembly
│   └── Denmark.NET/                # Reference country module
├── countries/
│   └── denmark/
│       ├── country.json
│       ├── incidents/
│       ├── history/
│       └── standards/
├── tests/
├── docs/
│   ├── architecture/
│   ├── standards/
│   └── contributing/
├── .github/
│   ├── ISSUE_TEMPLATE/
│   └── workflows/
│       ├── ci.yml
│       └── deploy-pages.yml
├── README.md
├── CONTRIBUTING.md
├── CODE_OF_CONDUCT.md
└── LICENSE
```

**Data flow:** `country JSON + optional country C#` → `.NET validator/exporter` → `static JSON/metadata/build reports` → `Blazor WASM on GitHub Pages`.

GitHub Actions builds and tests code, generates publication data, and deploys GitHub Pages. The website reads the published artifacts; it does not start an ASP.NET Core server. Contributor code undergoes review and CI instead of being loaded and executed live from visitors' browsers.

**Guiding principle:** Begin with statically referenced country modules and a small `ICountryModule` contract. Consider a sophisticated plugin loader only when real international contributions justify it.

## 7. Illustrative Data Contract

This example shows the implemented v1 shape. The JSON Schema files in `schemas/` are authoritative for required and optional fields:

```json
{
  "id": "DK-1849-001",
  "countryIds": ["DK"],
  "date": {
    "startYear": 1849,
    "month": 6,
    "day": 5,
    "endYear": null,
    "precision": "day",
    "calendar": "gregorian",
    "circa": false,
    "displayText": "5 June 1849"
  },
  "category": "Political",
  "title": "The June Constitution",
  "historical": {
    "summary": "Denmark's first free constitution replaced absolute monarchy with constitutional monarchy.",
    "evidenceStatus": "Documented",
    "sources": ["SRC-DK-015"]
  },
  "satire": {
    "publicSummary": "An invented administrative memo.",
    "commitMessage": "refactor(governance): introduce ConstitutionalMonarchy",
    "diagnosticId": "GOV1849",
    "diagnosticMessage": "Legacy absolute privileges deprecated.",
    "severity": "INFO",
    "acknowledged": true,
    "ignoredByPolicy": true,
    "standards": ["GOV-007"]
  },
  "relations": []
}
```

The country folder also contains a `sources.json` registry. The manifest lists incident and standard files; the source registry uses this fixed filename.

Before an event is published as documented historical content, its sources must be supplied and verified. Content must be version-controlled, schema-validated, and localizable.

## 8. International Open Source

**World.GOV** contains shared contracts and documentation. **Denmark.NET** is maintained as the reference. Contributors can independently implement `Germany.GOV`, `France.GOV`, `USA.GOV`, and other national modules.

Contributor workflow:

1. Select a country or event using the GitHub issue template.
2. Create module/data files from the official starter template.
3. Keep the factual account, source references, and satire separate.
4. Run validation and tests locally.
5. Submit a PR with sources, preview, and a short explanation.
6. Review historical accuracy, humor, safety, and architecture.
7. Merge → CI → new static website deployment.

Contributions can be sharp and satirical, but must not be racist, hateful, defamatory, or misleading. Fictional officials and companies must not conceal unsourced accusations against identifiable real individuals.

## 9. Historical Integrity and Satire Ethics

- Clearly distinguish **fact**, **interpretation**, and **fiction/satire**.
- Link at least one credible source for factual events, preferably several for contested matters.
- Track event date separately from publication and revision timestamps.
- For ongoing cases, state the exact legal status and update it as facts change.
- Satirize institutions, decisions, and bureaucracy; treat victims of war, disasters, and abuse respectfully.
- `[REDACTED]` in Cold War displays is a **satirical visual device** unless the source itself documents genuine redaction.
- Never portray invented fictional quotes as authentic historical quotations.
- Do not make source citations part of the joke: the evidence layer must remain trustworthy.

## 10. Production Principles and Tests

**Functional tests:** Schemas, unique incident IDs, valid categories and country codes, event relationships, standard references, historical sorting, and export.

**Engineering tests:** Correct builds, loading Blazor pages, links and basepaths under GitHub Pages, reproducible diagnostics, and failures for genuine technical defects.

**Content tests:** No published documented event without sources; uncertainty displayed; satire labeled; historical dates checked.

**UX tests:** Mobile readability, navigation without login, clear historical/technical modes, quick-loading static pages.

**Performance/security:** Static deployment, no execution of visitor-supplied code, PR reviews, dependency scanning, and least-privilege GitHub Actions permissions.

## 11. Release Roadmap

### `v0.0.0 — Initial Bureaucracy` (first push)

**Goal:** A real repository with a minimal, compilable skeleton.

- Solution, `World.GOV.Core`, `Denmark.NET`, and foundational tests.
- README explaining the concept, taglines, and clear satire designation.
- `CONTRIBUTING.md` and the first GOV standard (`GOV-000`).
- Simple Denmark country manifest and one clearly labeled illustrative incident.
- Green GitHub Actions build.
- Release notes: *Nothing Implemented. Everything Approved.*

**Definition of Done:** A newcomer can clone, build, understand the project, and encounter at least one functioning satirical detail.

### `v0.1 — First Government Operational`

- Incident schema, JSON validation, and historical sorting.
- CLI: `countries list`, `history`, `incident show`.
- 3–5 carefully written Danish historical incidents with verified sources.
- Static incident-data export.

### `v0.2 — Public Transparency Portal`

- Blazor WebAssembly and GitHub Pages.
- Overview, incident list/detail, and historical timeline.
- Shared C# models and data between CLI and web.

### `v0.3 — Administrative Diagnostics`

- GOV diagnostics and genuine CI-generated build reports.
- GOV standards library, tests, and hidden code jokes.
- Read-only Build Console and Developer View.

### `v0.4 — International Compatibility`

- Country template, contributor guides, documented module interface.
- One demonstration/community country module to verify extensibility.
- Cross-country relationships when supported by historical sources.

**No mandatory `v1.0` deadline.** Content and features grow according to interest, time, and contributions.

## 12. Risks and Scope Boundaries

| Risk | Mitigation |
|---|---|
| Scope creep: 195 countries from day one | Build Denmark only; other countries are an extensibility contract, not an immediate requirement |
| Architecture overshadows the satire | Every meaningful technical feature should enable a real joke or user experience |
| Historical misinformation | Sources, uncertainty labels, separated factual/satirical layers |
| Build console is only fabricated text | Actual CI-generated reports plus clearly labeled simulation |
| Git history becomes confusing | Actual development history and separate virtual history |
| Maintenance becomes a burden | Small independent incidents and no content deadlines |
| Political or interpersonal disputes | Neutral evidence practices, satire focused on systems, transparent moderation |

## 13. Humor Guidelines

**Desired voice:** Deadpan enterprise language, dry programming humor, technically credible punchlines, and subtle discoveries.  
**Avoid:** Random unrelated memes, overexplaining jokes, made-up historical facts, and personal attacks.

Recurring one-liners:

- **Clean Code. Broken Politics.**
- **Come for the bugs. Stay for the history.**
- **History repeats itself because nobody fixed the bug.**
- **Every country builds. Nothing works as intended.**
- **Government as Code. Problems as a Service.**
- **International cooperation works. Just not in production.**
- **No deadlines. History creates the backlog.**

Example micro-joke:

```csharp
// GOV-014: An acknowledged incident is no longer operationally relevant.
public bool IsHealthy => Incidents.All(i => i.IsAcknowledged);
```

## 14. Success Criteria

**The MVP succeeds when:**

1. `World.GOV` builds and tests without actual technical errors.
2. `Denmark.NET` can register at least one documented historical event.
3. The same event appears in factual and satirical forms.
4. The virtual history uses the correct event date without rewriting Git.
5. A newcomer can add an incident by following the documentation.
6. GOV-domain defects can be displayed without breaking a genuine build.

**Long-term success does not require popularity or dozens of countries.** It is also successful if it remains enjoyable to maintain, makes visitors laugh, and helps some people discover real history.

---

## 15. Initial Release Notes (Ready to Use)

### World.GOV `v0.0.0 — Initial Bureaucracy`

> Established the organizational framework required to begin evaluating the possibility of future development.  
> No functionality was implemented.  
> No incidents were resolved.  
> All stakeholders have been notified.

```text
Build:          PASSING
Features:       0
Countries:      Denmark (initial skeleton)
Documentation:  Under review
Technical debt: Pre-installed
Status:         PRODUCTION READY
```

**GOV-000:** *The absence of functionality SHALL NOT prevent a successful release.*

**Issue closed:** *Works as intended.*

---

# PART II — TECHNICAL IMPLEMENTATION CONTRACT

This section turns the vision from Sections 1–15 into implementation-level contracts. It defines **what** each component must accomplish and how responsibilities are separated, without dictating every method body, all historical prose, or finished artwork.

## 16. Architecture Decision Record (ADR) Baseline

| ID | Decision | Rationale |
|---|---|---|
| ADR-001 | One GitHub monorepo and one .NET solution | Easy to clone, test, review, and version |
| ADR-002 | `World.GOV.Core` owns shared domain types | CLI, export, and web must not have incompatible incident models |
| ADR-003 | JSON is the default medium for historical events | Contributions should not require C# knowledge |
| ADR-004 | Country modules may optionally include C# logic | Country-specific jokes without Core modifications |
| ADR-005 | C# country extensions are reviewed and compiled at build time | No unsafe execution of untrusted runtime assemblies on the site |
| ADR-006 | Blazor WebAssembly is statically hosted on GitHub Pages | No always-on database or backend required |
| ADR-007 | Validation/export tooling creates versioned static website data | One source of truth for CLI, site, and CI artifacts |
| ADR-008 | Satirical GOV failures are domain diagnostics, not C# compiler failures | `dotnet build` must only pass when technically correct |
| ADR-009 | Historical virtual commits derive from content records | Preserve the real Git log and authentic commit dates |
| ADR-010 | Factual, interpretive, and satirical layers remain separate | Historical credibility and clear satire |
| ADR-011 | Major features ship vertically, each with one real Denmark example | Avoid empty frameworks and endless planning |
| ADR-012 | No mandatory cloud services beyond GitHub | Low-cost hobby project, easy to maintain |

**Out of scope:** International modules must be architecturally possible from the beginning, but an advanced plugin host, user authentication, payments, live databases, and automated web scraping are **not** required for the initial implementation.

## 17. Data Model and Versioning

### 17.1 Country Manifest

Each country has `countries/<slug>/country.json`. The minimum manifest includes:

```json
{
  "schemaVersion": 1,
  "id": "DK",
  "slug": "denmark",
  "displayName": "Denmark",
  "nativeName": "Danmark",
  "defaultLocale": "en-US",
  "supportedLocales": ["en-US", "da-DK"],
  "historicalCoverage": {"startYear": 965, "endYear": null},
  "incidentFiles": ["incidents/1849-constitution.json"],
  "standardFiles": ["standards/GOV-DK-001.md"],
  "extensionAssembly": null
}
```

`extensionAssembly` is optional and refers to a **compile-time referenced** C# extension, not a DLL uploaded by a website visitor. A country's `id` and `slug` must remain stable. Former states and historical territories may later use dedicated identities; **do not hardcode a total of exactly 195 countries**.

**Language policy:** Shared documentation, API identifiers, code comments, GOV standards, and contributor instructions use English. Published historical summaries prefer `en-US`, with localized content where available. Preserve original titles, primary-source wording, and native names accurately. Missing translations use a clearly labeled fallback rather than inventing a translation.

### 17.2 Historical Event / Incident Contract

Use one stable event identifier, such as `DK-1849-001`, and maintain a strict separation between the historical record and its fictional technical interpretation.

```json
{
  "schemaVersion": 1,
  "id": "DK-1849-001",
  "countryIds": ["DK"],
  "globalEventId": null,
  "regions": ["Europe"],
  "category": "Political",
  "title": "The June Constitution",
  "date": {
    "startYear": 1849,
    "month": 6,
    "day": 5,
    "endYear": null,
    "precision": "day",
    "calendar": "gregorian",
    "circa": false,
    "displayText": "5 June 1849"
  },
  "historical": {
    "summary": "Frederik VII signed Denmark's first constitution on 5 June 1849.",
    "evidenceStatus": "Documented",
    "sources": ["SRC-DK-015"]
  },
  "satire": {
    "publicSummary": "An invented administrative memo.",
    "commitMessage": "feat(governance): replace absolute authority with a versioned constitutional interface",
    "diagnosticId": "GOV1849",
    "diagnosticMessage": "Legacy ruler privileges have been deprecated.",
    "severity": "CRITICAL",
    "standards": ["GOV-000", "GOV-014"],
    "acknowledged": true,
    "ignoredByPolicy": true
  },
  "relations": [],
  "translations": {},
  "tags": ["constitution", "democracy"],
  "sourcePath": "countries/denmark/incidents/DK-1849-001.json"
}
```

This is the implemented **version 1 contract**. The optional `translations` object uses locale keys such as `da-DK`. Missing required fields, unknown country/source/incident references, invalid IDs, or nonexistent standard references cause **real CI validation failures**, not ignorable GOV warnings.

### 17.3 Historical Dates and Uncertainty

Not every event can be represented by an exact date. Sources may only provide a year, range, approximate period, unknown date, or a date BCE. The v1 record stores these fields in `date` and uses `displayText` for a faithful human-readable form.

- `startYear` and `endYear` are optional integers from -9999 to 9999; negative values represent BCE **without a year zero**. Zero is invalid.
- `month` and `day` are optional when source precision is insufficient. `endYear` supports year-level ranges; `displayText` can preserve more precise endpoint wording.
- `precision` is one of `day`, `month`, `year`, `range`, `approximate`, or `unknown`. The validator checks that the available fields match the selected precision.
- `calendar` records the source calendar or project normalization; historical calendar differences must not be silently hidden.
- `circa` marks an approximate date. `displayText` is human-readable, while sorting uses normalized date fields and places unknown dates last.
- Historical dating can be corrected with supporting evidence and a normal new development commit.

### 17.4 Source Registry

Sources are stored separately in `countries/<slug>/sources.json`, allowing several incidents to reuse the same reference. Incident records contain source IDs, and validation fails when an ID cannot be resolved in a registered country source file.

```json
{
  "schemaVersion": 1,
  "sources": [
    {
      "id": "SRC-DK-015",
      "title": "Actual source title",
      "publisher": "Institution or publisher",
      "url": "https://example.org/source",
      "publishedDate": null,
      "accessedDate": null,
      "language": "da",
      "notes": "Primary source / context / limitations"
    }
  ]
}
```

The URL above is a **placeholder, not a verified historical source**. An event must not be published as documented historical content until real source records are added and reviewed. Broken external links should be flagged rather than silently deleting previously published historical entries.

### 17.5 Event Relationships and International Events

Events can refer to other stable IDs through relationships such as `precededBy`, `followedBy`, `influencedBy`, `relatedTo`, and `partOf`. Claims of causation need an explanation and credible sources. A global event (for example, a world war or pandemic) uses a shared `globalEventId`, while countries may supply separate sourced perspectives and technical jokes. Validate every cross-country reference during export.

## 18. C# Contracts and Component Responsibilities

### 18.1 Modules

```csharp
public interface ICountryModule
{
    CountryDefinition Definition { get; }
    IEnumerable<GovernmentIncident> GetIncidents();
}

public interface IIncidentCatalog
{
    IReadOnlyCollection<GovernmentIncident> GetByCountry(string countryId);
    GovernmentIncident? FindById(string incidentId);
}

public interface IHistoricalTimeline
{
    IReadOnlyList<VirtualCommit> GetHistory(string countryId);
    IReadOnlyList<VirtualCommit> GetBlame(string entityId);
}
```

These are **contract sketches**. Concrete record types, validation results, and synchronous/asynchronous choices are finalized in `Core` during implementation. The important boundaries are fixed: country files provide content, `Core` owns shared domain types, `Tooling` validates and exports, `Cli` reads the catalog, and `Web` displays published data.

### 18.2 Data First, Code Only When Needed

Most events consist of JSON and satirical code snippets. Unusual country-specific mechanics may justify a C# extension project. Such extensions follow ordinary PR review and compile in CI. Adding a country must not require changes to `World.GOV.Core`.

### 18.3 Export Contract and Website Data

`World.GOV.Tooling` generates deterministic static artifacts:

```text
publish-data/
  manifest.json             # schemaVersion, generatedAt, supported countries
  countries.json            # Summary country list
  incidents/DK.json         # Validated incident records
  timelines/DK.json         # Virtual historical commits
  standards/index.json      # GOV standard index and links
  diagnostics/DK.json       # Satirical GOV domain diagnostics
  build-report.json         # Last actual CI outcome
```

Do not create manual duplicate incident catalogs in web source code. The browser may load country datasets on demand. The UI consumes static artifacts and optionally shared serialization DTOs from `Core`.

### 18.4 Stable Identifiers and Revisions

- Incident IDs are unique, stable, and may never be reused for unrelated historical events.
- Validate `schemaVersion`; breaking schema changes require migration logic and fixtures.
- Event records can be revised through genuine current Git commits without modifying their historical event dates.
- Files may be reorganized, but preserve aliases/redirects for published deep links.
- Changes to historical fact-checking should appear in the normal changelog or content revision metadata.

## 19. Diagnostics, CI, and the Intended Build Joke

**Never conflate these categories:**

1. **Technical failures** (invalid C#, broken schema, failing tests, insecure pipeline): must fail CI.
2. **Satirical government-domain failures** (missing accountability, bureaucratic deadlocks, budget overruns): may be `CRITICAL` in a domain report, but must not be emitted as blocking compiler errors.

A common diagnostics generator produces `GovernmentDiagnostic` records with `Id`, `Country`, `Severity`, `Message`, `IncidentId`, `StandardId`, `SourceLocation`, `Acknowledged`, and `IgnoredByPolicy`. A later Roslyn adapter may emit warnings or informational messages during builds; the CLI may render a theatrical line such as `GOV0601 CRITICAL (ignored)`.

**CI contract:**

```text
checkout
  -> dotnet restore
  -> dotnet build (genuine technical build)
  -> dotnet test (genuine tests)
  -> validate incidents + sources + links
  -> generate virtual history + GOV diagnostics
  -> export static site data + authentic build report
  -> publish Blazor WebAssembly to GitHub Pages
```

GitHub Actions must use minimum required permissions. Do not run arbitrary contributor code in a privileged deployment job. PR workflows validate submissions; public deployment occurs only after review and merge. `build-report.json` reflects actual CI outcomes, while satirical counters are labeled separately and ideally derived from real registered entries.

**Non-negotiable technical rule:** `Build succeeded` and `0 compiler errors` must be true. Never fake success by suppressing genuine compiler failures with `ContinueOnError`, ignoring exit codes, or manipulating CI statuses.

## 20. Virtual Git and Historical Exploration

### 20.1 Three Views

- **Real Git:** The project's authentic `git log` and `git blame`.
- **Virtual History:** Events sorted by historical date and rendered as commits with clearly fictional hashes or labels.
- **Virtual Blame / Trace:** Interpretive dependencies, standards, and changes linked to historical events; not the actual authorship of source-code lines.

A historical event links to development commits through `sourcePath` and, optionally, `introducedInGitCommit` collected by build tooling. Virtual commits must not impersonate authentic GitHub commits; label them **Historical simulation**.

### 20.2 Classification and Redaction

`[REDACTED]` is primarily a **fictional presentation effect**. Real classified or later-declassified information must be described from sources; never manufacture a claim of hidden knowledge. A theatrical `--include-classified` CLI option may respond with a joke, but must not claim to reveal actual state secrets.

### 20.3 Historical Branches and Themes

Large themes such as World War I, World War II, the Cold War, pandemics, natural disasters, technology, and economic crises are represented by filters and event relationships. These need not be real Git branches with falsified historical commits.

## 21. Website Contract — GitHub Pages

**Stack:** Statically deployed Blazor WebAssembly, exported JSON data, CSS with a professional enterprise look. No always-running ASP.NET Core server is required.

**Navigation and required behavior:**

| Page | Function | Target version |
|---|---|---|
| Dashboard | Country health, CI, GOV diagnostics, rotating one-liner | v0.2 |
| Country index | Country list, metadata, later map | v0.4 |
| Incidents | Search, filters, pagination | v0.2 |
| Incident detail | Facts, sources, satire, code, related events | v0.2 |
| History | Virtual timeline and `worldgov blame` | v0.2–v0.3 |
| Build Console | Genuine CI data and clearly labeled simulated terminal | v0.3 |
| GOV Standards | Standards index and documents | v0.3 |
| About / Contribute | Project purpose, factual/satirical distinctions, contribution rules | v0.2 |

**Public View / Developer View:** These are alternative presentations of **one incident record**. Public View must not require C# knowledge. Developer View includes fictional services, standards, domain diagnostics, virtual commits, and links to genuine source code.

**GitHub Pages requirements:** Handle repository basepaths (commonly `/<repository>/`), correct asset paths, SPA routing/404 fallback, cache versioning of published JSON, and mobile layouts. Deploy through GitHub Actions. Client-side scripts must never execute server commands. An interactive terminal is either a harmless in-browser simulation or a read-only presentation of published build artifacts.

**Accessibility:** Legible mobile font sizes, keyboard navigation, semantic markup, sufficient contrast, respect for reduced motion, and deep-linkable incidents. The satirical nature of the project must remain discoverable from the footer/About page even when the visual style is deliberately official.

## 22. Content Quality and Moderation

Every incident must be presentable in three blocks: **Historical facts**, **Sources and uncertainty**, and **Satirical implementation**. Visitors may switch locales when translations exist; missing translations fall back to a clearly marked available language, preserving original-language source titles.

**New incident checklist:**

1. Valid country, stable ID, date/range, and category.
2. Supported factual account with sources and accurate legal status for contemporary allegations.
3. Satirical commit, at least one joke/diagnostic, and optional GOV standard.
4. Supported and validated cross-event and cross-country relationships.
5. No private information, unsupported personal accusations, or invented quotes presented as real.
6. Successful local validation and human contributor/maintainer review.

For recent public controversies, do not describe allegations as proven crimes. Fictional characters must clearly be fictional; avoid thinly disguised depictions of identifiable people carrying unverified allegations. Treat casualties and suffering from wars or disasters respectfully; focus satire on institutional responses and decision-making.

## 23. Implementation Order and Vertical Slices

### Slice A — `v0.0.0: Initial Bureaucracy`

- Create monorepo, solution, `Core`, `Denmark.NET`, test project, and CI.
- Add `country.json`, one incident fixture, `GOV-000.md`, README, and contributor guide.
- Code builds, and at least one real test passes.
- One satirical line is visible in a demonstration CLI or test output.
- Publish the `v0.0.0` tag without pretending the product is complete.

### Slice B — `v0.1: Incident Pipeline`

- Implement schemas, source registry, validation, and international references.
- Implement `worldgov history`, `incident show`, and `countries list`.
- Generate deterministic `publish-data` outputs.
- Add at least three sourced Denmark incidents from different historical periods.

### Slice C — `v0.2: Website MVP`

- Deploy Blazor WebAssembly to GitHub Pages.
- Implement dashboard, incidents, detail pages, sources, and timeline.
- Display factual and satirical representations of the same event.
- Verify navigation and readability on an Android phone.

### Slice D — `v0.3: GOV Diagnostics`

- Implement GOV-domain diagnostics separately from actual compiler errors.
- Add CLI diagnostics, standard index, and CI-generated build reports.
- Add hidden code jokes, satirical tests, simulated `worldgov blame`, and a read-only console.

### Slice E — `v0.4: Community and Countries`

- Provide a country starter template and documentation; support data-only contributions without C#.
- Add a second country as a contract test without requiring a full national history.
- Implement cross-country links, locale fallback, and contributor review rules.

## 24. Verifiable Acceptance Tests

| ID | Test | Expected result |
|---|---|---|
| T01 | Clone the repo and run `dotnet build` | Exit code 0 when code is valid |
| T02 | Introduce a real C# syntax error | Build and CI turn red |
| T03 | Register a satirical `GOV0601` diagnostic | Build stays green; domain diagnostic is visible |
| T04 | Add a 1940 historical event today | Virtual history places it in 1940; real Git log retains the actual commit date |
| T05 | Register an incident with a missing source reference | Validator and CI fail |
| T06 | Register a duplicate event ID | Validator and CI fail |
| T07 | Add a second data-only country | Country appears in CLI and static export without Core modifications |
| T08 | Link related events across countries | Cross-country link resolves and references validate |
| T09 | Open an incident on GitHub Pages using a phone | Text, sources, and code remain accessible and readable |
| T10 | Show `GOV-014` in the website build console | Clearly labeled satire; actual CI status remains accurate |
| T11 | Add an event with uncertain historical dating | Correct uncertainty label and documented ordering |
| T12 | Display `[REDACTED]` | Explicitly satirical effect; no fictional claims inserted into the factual layer |
| T13 | Submit a community PR | Review and CI before production deployment; no untrusted privileged execution |
| T14 | Follow incident links to a source and GitHub | Both point to their intended destinations |
| T15 | Switch between Public and Developer View | Same incident ID and factual layer; different presentation |

## 25. Open Decisions and Deliberate Omissions

The document specifies system **contracts**, but not each source file, all Danish or world history, or a fully finished design system. These choices can be resolved during implementation without changing the project vision:

- Exact .NET SDK release, CSS framework, test framework, and versioning policy.
- Serialization/validation libraries and final fine-grained directory names.
- CLI command-line argument syntax and exact website colors/animations.
- Which historical events are authored first and how many ship per release.
- Physical storage for auxiliary indices, caching, and localized strings.
- Whether an advanced Roslyn analyzer is needed or a genuine CLI/CI domain reporting layer is sufficient.
- Whether future virtual history needs a dependency graph UI or full-text search.

**Explicit non-goals:** No real public administration, no live government-system access, no login requirement, no automatic publication of unverified accusations, no alteration of authentic Git history, and no arbitrary visitor-executed code on the website.

**Definition of realizing the entire concept:** At least one fully sourced country module; a functioning C# content pipeline; genuinely successful builds with satirical GOV diagnostics; virtual historical timelines with separate real Git references; a public, mobile-friendly GitHub Pages portal with Public/Developer views; compatibility with a second country through the same contract; and a contributor workflow with source requirements and secure review. Individual historical collections and international content can continue growing indefinitely.

---

## Appendix A — Original Brainstorm Traceability

| Original concept | Document sections |
|---|---|
| Clean enterprise styling with hidden programming satire | 2, 3, 5, 13, 21 |
| C#/.NET, genuine tests, clear engineering guidelines | 4.5, 6, 10, 16, 18, 19 |
| Denmark.NET first; World.GOV international | 1, 4.1, 8, 16, 23 |
| Real events: nature, politics, economics, wars, etc. | 4.2, 7, 9, 17, 20 |
| Fictional characters/companies and source criticism | 4.2, 9, 22 |
| World wars, Cold War, and `[REDACTED]` | 9, 20 |
| Historical comments, virtual commits, and two kinds of blame | 4.3, 20 |
| GOV standards with RFC wording and hidden punchlines | 4.4, 13, 19 |
| Green builds despite satirical government-domain failures | 4.5, 19, 24 |
| GitHub Pages, Blazor WebAssembly, GitHub Actions | 5, 6, 18.3, 21 |
| Interactive terminal, Public View, Developer View | 5.2, 5.3, 21 |
| Open-source additions by developers in other countries | 8, 16, 18, 22, 23 |
| First tag `v0.0.0`, small releases without deadlines | 11, 15, 23 |
| Historical learning and documentation beneath jokes | 3, 4.2, 9, 17, 22 |
| Easy updates when new real-world events occur | 4.2, 8, 17, 18, 23 |

**Overall status:** This GDD defines the shared functionality, architecture, content rules, and intended experience well enough to begin incremental implementation. It is not a guarantee that future development requires no design decisions, and it intentionally does not contain complete historical datasets or finished source code.

---

## Appendix B — Language and Localization Policy (v0.3)

1. **English is the repository's official working language:** README, GDD, standards, code, comments, commit convention, issues, PR templates, and documentation.
2. **Historical accuracy takes precedence over translation convenience:** Native titles, names, quotations, and primary source text may remain in their original language.
3. **`en-US` is the default publishing locale:** A historical entry may also provide `da-DK`, `de-DE`, or other locales. The UI must label fallbacks and never silently paraphrase quotations.
4. **Country modules may have distinct native languages:** Their maintainers may provide additional translations while keeping English entry points for international contributors.
5. **Satire must remain identifiable:** Do not localize a fictional quote in a way that makes it appear to be a real quotation, and do not alter facts merely to preserve a punchline.
6. **Localization remains optional for contributors:** A high-quality English event plus original-language citations is sufficient for a first submission; community translations can follow.

**Project rule:** *One shared engineering language. Many local histories. No fabricated facts.*
