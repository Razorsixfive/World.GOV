# ADR-001: Data-first country modules

**Status:** Accepted  
**Date:** 2026-11-04

## Context

Historical entries are mostly structured content. Requiring each country to ship custom executable code would raise the barrier to contribution and expand the runtime trust boundary.

## Decision

Country modules are described by versioned JSON manifests and data files. `World.GOV.Core` owns shared contracts. A country may provide a compile-time referenced C# extension for behavior that cannot be expressed as data. The static site consumes generated JSON and never loads arbitrary contributor assemblies.

## Consequences

- Data-only countries can use the same validator and exporter.
- Schema changes need a version and migration policy.
- Historical claims can be reviewed without compiling country-specific code.
- Contributor code remains subject to ordinary review and CI before publication.
