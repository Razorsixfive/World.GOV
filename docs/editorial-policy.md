# Editorial policy

World.GOV is a historical discovery experience for curious visitors. Developer details reward a second look. The first session is: open the short tour, read an account, inspect a source, recognize the fictional memo, and follow the next event. There is no score, simulated political power, or claim that administrative health measures a country's wellbeing.

## Selecting content

Prefer an event with a clear historical question, accessible credible evidence, and an institutional decision that can sustain a joke without distorting the account. Add variety across periods and categories as the collection grows. Coverage is selective; an absent event is not a judgment of significance. Country coverage describes the dates actually represented in the catalog.

## Evidence labels

- **Documented:** cited evidence directly supports the stated account. It does not imply universal agreement on wider interpretations.
- **Interpretive:** an explanation or relationship attributed to a named author, with its limits described in `uncertainty`.
- **Uncertain:** a material date, attribution, or detail remains unknown; explain what is uncertain.
- **Disputed:** credible accounts disagree; describe the disagreement and cite the competing accounts.

The last three labels require an explanatory note. These are the version 1 implementation labels; the earlier GDD's example labels WellSupported and Legendary are not accepted by this schema. Legendary accounts require an Uncertain label and a factual account about the tradition, not a claim that the legend occurred.

## Sources and claims

Each factual sentence must be supportable from the listed sources. Keep accounts short enough for readers to check; include a section, page, or document title in the source title when needed. Favor archives, original documents, institutional histories, and attributed scholarship. An institutional account alone does not settle contested interpretation. Use multiple independent sources for disputed claims.

The validator checks source structure and links between records; it cannot determine whether a source supports a claim. A reviewer opens each URL and checks the passage before merge. Retain source titles in their original language and identify translations. Do not copy source articles into the project. If a URL stops working, seek the original publisher's replacement or an identifiable archival copy and record the change.

## Corrections and review

1. File a correction issue with incident ID, precise claim, supporting source, and proposed wording.
2. The maintainer records a review decision with reasons. If a claim cannot be supported, remove or qualify it while further review occurs.
3. Update content in an ordinary present-day commit. Keep the incident ID and event date stable unless those facts themselves were incorrect.
4. Add a dated entry to CHANGELOG.md for factual corrections, describing the changed claim and evidence. Publish through normal CI.
5. A contributor may request reconsideration with additional evidence. Seek another reviewer for unresolved historical disputes when one is available. Until then, publish the uncertainty or defer the entry.

Humor targets institutions and decisions. Quotes, legal status, casualty information, and citations remain factual. Fictional memos are clearly labeled. Relationship labels explain context; they do not establish causality on their own.

## A small release check with readers

Ask a first-time visitor to find an event, identify the fictional portion, open its source, and explain one thing they learned. Ask a developer to locate the matching JSON and add an event using the template. Record observed confusion in issues. These are release review activities, not claims of completed user research or requirements for tracking analytics.
