# Denmark.NET

Denmark's reference module embeds the historical incident data and contains small, executable fictional government policies. The implementations and tests are part of the satire. Historical accounts and citations remain in `countries/denmark`.

Follow a policy from its GOV standard into the code, then read the test that carefully preserves its absurd outcome:

| Fictional policy | Implementation | What the test protects |
| --- | --- | --- |
| [GOV-014: Operational Continuity](../../countries/denmark/standards/GOV-014.md) | [OperationalContinuityService.cs](OperationalContinuityService.cs) | Acknowledging a critical incident restores administrative health while its critical diagnostic survives. |
| [GOV-003: Public Budget Allocation](../../countries/denmark/standards/GOV-003.md) | [BudgetAllocationService.cs](BudgetAllocationService.cs) | Revising the reporting baseline clears the reported overrun while the original budget, spending, and overrun remain visible. |

The executable examples live in [GovernmentPolicyTests.cs](../../tests/World.GOV.Tests/GovernmentPolicyTests.cs). Run them from the repository root:

```text
dotnet test tests/World.GOV.Tests --filter FullyQualifiedName~GovernmentPolicyTests
```

Keep each policy small and give it a distinct institutional absurdity. Code, comments, test names, and GOV standards stay English. These fictional policies remain separate from technical build results and the portal's factual historical records.
