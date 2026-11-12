# ADR-004: Fictional archive presentation

**Status:** Accepted  
**Date:** 2026-11-12

## Decision

Incident v1 gains an optional `satire.archive` object: a `SIM-ARCHIVE-` identifier, title, and ordered lines with labels, text, and redaction flags. The incident detail page displays an explicitly fictional dossier, initially redacted. Visitors can reveal the full memo and restore its redactions. Navigating to another incident resets the presentation. Historical facts, uncertainty, and source links do not depend on that state.

The fictional stamp starts at `CLASSIFIED`, changes to `DECLASSIFIED` on reveal, and becomes `RECLASSIFIED` when redactions are restored. These stamps stay English; the public controls remain Redacted / Unredacted and can be translated. Optional `classifiedMessage`, `declassifiedMessage`, and `reclassifiedMessage` fields provide a different, incident-specific punchline for each state. The component remembers a reveal only for the current dossier; navigation resets it. Missing message fields retain the existing generic status text, while missing message translations fall back individually to canonical English with the fallback notice.

The full memo is public JSON in the static export. Redaction controls presentation only; it cannot protect sensitive information. Redacted lines omit their text from the rendered page until revealed, including attributes and accessible names. Screen readers receive a translated redaction label. No server, access request, real classification status, or simulated declassification date is introduced.

English is canonical. An optional `translations[locale].archive` translates the entire public memo while preserving its identifier, line order, and redaction flags. Validation checks the identifier, line count, and flags; a reviewer checks meaning and order. An absent archive translation uses English and triggers the existing fallback notice. Developer view and all code, diagnostics, simulated commits, standards, and reference identifiers remain English.

## Compatibility

This is an additive v1 change. Existing records need no migration and do not acquire an archive panel. Existing portal readers ignore the optional fields. Content tooling must use the updated strict schema before reading or writing the new fields. The exporter already serializes incident records and needs no separate archive dataset. The component is reusable by any country; ordinary records remain data-only contributions.

## Verification

Rendering tests cover redacted/full text, Danish presentation, English fallback, and English developer output. Schema and content tests reject malformed memos and translations that alter redaction behavior. Browser verification covers the actual reveal/restore controls and navigation reset.
