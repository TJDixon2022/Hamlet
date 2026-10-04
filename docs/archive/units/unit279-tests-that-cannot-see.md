# Every test that cannot find its subject — work instruction 279, task 4

**Reading only. Nothing was changed to produce any of this**, and no suite was run:
the standing rule of HM-DEC-155 keeps a unit out of suites it did not write, so
every judgement below is from the source. Read from the tree on 2026-09-08 at
commit `e959899`, version 1.12.158.

---

## What was looked at, not only what was found

Unit 276's context-menu sweep recorded that eight of nine files had none, so the
next reader would not mistake silence for omission. The same here.

| | |
|---|---|
| Test source files, excluding `obj/` and `bin/` | **594** |
| Files containing any `First`, `Single` or `FirstOrDefault` with a predicate | **172** |
| Bare `.First(pred)` / `.Single(pred)` calls, `Hamlet.App.Tests` | 62 |
| the same, `Hamlet.RadioEngine.Tests` | 122 |
| the same, `Ft8Sharp.Tests` | 17 |
| the same, `Ft8Sharp.Deep.Tests` | 2 |

**The great majority of those finders are not the shape this unit is about**, and saying
why is the useful part. A finder is only dangerous where **the fixture might not
produce its subject**. Three groups are safe by construction and were set aside
deliberately:

- **`.FirstOrDefault(c => c.Name == "...")` against the visual tree.** The name is a
  literal in the markup. If it is renamed the finder returns null and the very next
  line dereferences it or asserts on it, so the failure is immediate. It cannot pass
  quietly. **The great majority of the 172 are this.**
- **Finders over a list the test itself just built.** `Hamlet.RadioEngine.Tests`'s
  122 are mostly of this kind: the test constructs a message list, then finds a
  message in it. The predicate and the fixture are three lines apart and cannot
  drift.
- **Finders over decoder output in `Ft8Sharp.Tests` and `Ft8Sharp.Deep.Tests`.**
  Those search for a decode by text. Where none matches, the assertion that follows
  is about the count and reports it. `src/Ft8Sharp/` is parked and none was touched.

**What is dangerous is a predicate about a row's *content* on a bound collection or
a realized visual tree**, because that is what unit 273's split broke: the rows
moved and the predicate stayed. Those are enumerated below and there are **nine**.

## The nine content predicates, and whether each can be satisfied

| # | Where | Predicate | Can the fixture satisfy it? | What it says when it cannot |
|---|---|---|---|---|
| 1 | `ViewModels/TheWholeChainRunsFromOneRightClickTests.cs:895` | a row in **`DigitalDecodedRows`** whose `Addressee` is the operator | **No.** `WantsRow` is `!IsForHim(row) && ...`, so that list never holds one | An `Assert.True` naming the station and the row count. **This is `HM-OPEN-086`; task 5 fixes it** |
| 2 | `Views/TheMenuIsUnderTheMouseTests.cs:548` | a realized row grid matching a caller's predicate | **Yes**, since unit 276 widened it to both lists | *no realized row matched. Left rows: n; mine rows: n; realized grids…* — the model for the rest |
| 3 | `Views/AWorkedStationIsDimTests.cs` | a row whose sender is one station | **Yes.** It bit twice on the first run and both finders were made to speak (task 1) | names the station, both list counts, and every sender seen |
| 4 | `Views/TheOperatingScreenIsLaidOutAsRuledTests.cs:162` | a `TextBlock` whose text is the band's name, inside its card | **Yes today** | **Nothing. It `continue`s.** See below |
| 5 | `Views/TheBandRowIsWhereItWasRuledTests.cs:161` | the same | **Yes today** | **Nothing. It `continue`s.** See below |
| 6 | `Views/TheDecodedColumnsLineUpTests.cs:88` | a `Grid` whose children include a `TextBlock` reading `utc` | **Yes** | `Assert.NotNull(header)` with no message: it says *Value is null*, naming neither the text nor the tree |
| 7 | `ViewModels/BothHalvesOfTheConversationTests.cs:195,196,276` | a row where `IsSent` is true, and one where it is false | **Yes**, the fixture places three of each | a bare LINQ `Sequence contains no matching element` |
| 8 | `ViewModels/TheContactStandsAfterHisLastTransmissionTests.cs:139` | a row in `DigitalDecodes` whose message equals a given string | **Yes** | a bare LINQ exception, naming neither the message nor the table |
| 9 | `Views/TheSendPanelComposesAndDoesNotKeyTests.cs:121,235` | a button whose content is a face, a text box by watermark | **Yes** | a bare LINQ exception |

**How I decided, in every row: by reading the fixture that builds the subject and
the predicate that looks for it, in the same file.** Where the two are in different
files — cases 1 and 2 — by reading `MainWindowViewModel.WantsRow` and `IsForHim`,
which is where unit 276 found the answer too.

## The two that are worse than the rest, and are a different failure

**Cases 4 and 5 do not fail obscurely. They pass.**

Both files carry the same loop, at line 166 of each:

```csharp
var label = card.GetVisualDescendants().OfType<TextBlock>()
    .FirstOrDefault(t => t.Text == band);

if (label is null)
{
    continue;
}
```

`EveryBandLabelRendersInsideItsOwnCard` collects the labels that are cut short and
asserts the collection is empty. **If no label is found for any card, nothing is
collected and the assertion passes** — a test named *every band label renders inside
its own card* going green with none of them rendering.

**That is not hypothetical for the reason it was written.** The operator read `10 n`
off his own screen on 2026-08-26 because a label was given less width than it asked
for. A template change that renamed the band text, or a virtualization change that
stopped realizing the cards, would put the test back to silent and the defect back
on his screen with a green suite behind it.

**The skip itself is defensible and the silence is not.** A card without a label may
be a legitimate state during layout. What is missing is the count: how many cards
were examined, and a failure when that is zero.

## What this changes for task 5

- **Case 1 is the fix task 5 names**, and it is the only one where the predicate is
  genuinely unsatisfiable today.
- **Cases 4 and 5 need a subject count**, not a change to what they assert. A test
  that examined no subjects has not passed.
- **Cases 6 to 9 need their finders to say what they were looking for and what was
  there.** None of them is broken; all of them would waste an evening if they broke.
- **Case 2 is the model** and is left exactly as unit 276 wrote it.

## What was deliberately not swept

- **`src/Ft8Sharp/` and its tests.** Parked, and nothing there searches a view.
- **The 51 inherited CW reds** and `CwAdjudicationTests`. Parked and never chased.
- **Assertions about counts and numbers**, which are not finders.
- **Anything in `tools/`.**
