# ADR-003: Public translations and English developer output

**Status:** Accepted  
**Date:** 2026-11-12

## Decision

English is the canonical project and developer language. Public interface text uses source-text keys in versioned JSON language catalogs under `locales`. The English catalog must contain the exact source value for every key. Invalid formatting slots in active translations fail content validation. Unknown or outdated community keys are warnings and ignored at runtime; English changes can publish without waiting for other languages. Blank or missing translations use the English source; a contributor can publish a partial language without creating a country or changing C#.

The static exporter includes validated catalogs in `data/locales/index.json`. This is an additive manifest dataset. The browser remains compatible with older publications that do not advertise it, using the existing English interface fallback. Language names and reading direction are data, with English as the initial selection until a visitor explicitly chooses another language.

Historical translations remain in the incident v1 `translations` object. English incident fields take precedence over an `en-US` translation override. Technical satire is rendered in a separate English-only developer component. Build console, code, simulated commits, diagnostics, and standards do not use public translation catalogs and retain left-to-right English markup.

## Compatibility and consequences

The v1 incident `commitMessage` and `diagnosticMessage` translation fields remain readable for compatibility but are deprecated and ignored for presentation. Existing files need no destructive migration. New contributions translate only public fields. Source titles and machine identifiers retain their original spelling.

The maintainer supports English and Danish; other translations rely on community proposals and language review. Automated validation checks structure and fallback behavior, not linguistic accuracy. Website-string coverage and incident translation coverage are distinct.
