# ADR-002: Virtual history is not Git history

**Status:** Accepted  
**Date:** 2026-11-04

## Context

Historical events can be presented as commit-like records, but real Git dates and authors must remain authentic.

## Decision

Virtual commits use stable IDs prefixed with `SIM-`, are labeled as historical simulation, and derive their displayed date from event data. Authentic `git log` and `git blame` are separate CLI commands operating on the local repository metadata.

## Consequences

- Simulated hashes cannot be mistaken for Git object IDs.
- Interpretive trace links do not claim authorship of source-code lines.
- A repository with no Git metadata can still build, validate, and publish historical data.
