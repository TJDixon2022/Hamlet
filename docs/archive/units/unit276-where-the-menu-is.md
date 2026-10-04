# Where the menu actually is — work instruction 276, task 1

**Reading only. Nothing was changed to produce any of this.** Read from the tree
on 2026-09-07 at commit `1f8dfad`, version 1.12.139.

---

## The short answer

**One context menu exists in the whole application, and it is attached to the left
list.** The mine list has none. The `Log this contact...` item is gated on the
message being addressed to the operator — **so it is attached to the one list on
which no such row can ever appear, and it has never once been shown.**

**And the instruction's central premise is wrong**, which changes what this unit
is really about. It says nothing could have caught this and that no test opens a
context menu on a decoded row. **A test does, it is thorough, and it has been red
since unit 273.** Nobody ran it.

---

## Which list carries the menu

**Exactly one `ContextRequested` handler exists in the application's markup.** A
search of every `.axaml` under `src/` for `ContextMenu`, `ContextFlyout` and
`ContextRequested` returns **two hits, and one of them is a comment**:

| Where | What |
|---|---|
| `MainWindow.axaml:3754` | a comment explaining why there is deliberately no `ContextFlyout` |
| **`MainWindow.axaml:3762`** | `ContextRequested="OnDecodedRowContextRequested"` |

Line 3762 is on the row `Grid` inside **`DigitalDecodedRows`** — declared at
`MainWindow.axaml:3728` — which is the **left** list, everything-or-CQ.

**`DigitalMineRows` has no handler, no flyout and no menu of any kind.**

## What is on it

`MainWindow.axaml.cs:171`, `SendFlyoutFor`, builds one `MenuFlyout`:

- every option from `Ft8SendMenu.Options`, each a `MenuItem` carrying
  **`vm.SendMessageCommand`** with the message text as its parameter — the one
  entry point that arms;
- then each note from `menu.Absent`, which carries no command and cannot be
  clicked;
- then the licence line, where there is one, as another note;
- then, **where `vm.CanLogRow(row)` is true**, a `Separator` and a `MenuItem`
  reading `Log this contact...` carrying **`vm.LogContactCommand`** with the row.

## What Log is gated on, and why it can never have fired

`MainWindowViewModel.cs:8589`:

```csharp
public bool CanLogRow(DigitalDecodeRow? row)
    => row is not null
       && Ft8MessageSplit.IsAddressedTo(row.Message, _settings.Operator.Callsign);
```

`MainWindow.axaml.cs:217` gates the menu item on exactly that.

**And unit 273 put every row addressed to the operator on the other list.** The
same predicate decides both: `Ft8MessageSplit.IsAddressedTo` is what fills
`DigitalMineDecodes`, and `MainWindowViewModel.WantsRow` excludes those rows from
the left list explicitly. So the condition for showing Log and the condition for
being on the list that has the menu are **mutually exclusive by construction**.

**The Log item has never appeared on screen.** Not once, from the moment unit 274
added it. Unit 274's report said otherwise and that claim was untrue when it was
written.

## Whether any test opens a context menu

**The instruction expects none and asks to be checked. It should be: there are
three, and one of them is exactly the test that would have caught this.**

| Test | What it does |
|---|---|
| `Views/TheMenuIsUnderTheMouseTests.cs` | **Raises a real `ContextRequested` on the real row control in a real window and reads the flyout's items.** Six tests. |
| `ViewModels/TheWholeChainRunsFromOneRightClickTests.cs` | Same, through the whole send chain. |
| `ViewModels/TheLicenceGateHoldsFromTheClickTests.cs` | Calls `SendMenuFor` on the view model; does **not** open a menu. |

`TheMenuIsUnderTheMouseTests.RightClickRow` (`:506`) looks for a realized row
inside **`DigitalDecodedRows`** whose `DataContext` matches a predicate, and
`RightClick` (`:496`) supplies that predicate:

```csharp
=> RightClickRow(scene, r => r.Sender == station && r.Addressee == "KC3QIS");
```

**It asks for a row addressed to the operator, inside the list unit 273 stopped
putting them in.** Which means it went red the moment that split landed.

## It is red, and it has been for three units

Run against the tree exactly as the operator found it:

```
Failed  OutOfPrivilegesTheMenuSaysSoAndForbidsNothing
Failed  EveryStationsPredictedMenuAppearsUnderTheMouse
Failed  TheRepeatCountBelongsToTheClickAndNotToTheRow
Failed  WithNoGridTheReasonIsANoteAndTheRestStayClickable
Failed  ChoosingOneGoesThroughTheOneCommandThatArms

Failed! - Failed: 5, Passed: 1, Skipped: 0, Total: 6
```

and the message is the defect stated plainly:

```
no realized row matched. Rows on the table: 8;
realized grids with a row DataContext: 8
```

**Eight rows on the table and not one of them addressed to him** — because they
are all on the other list, and the menu is on this one.

## What this changes about the unit

**The gap is not a missing test.** The test exists, it opens the menu properly
rather than reading a property, it asserts the items, and it caught this the day
it broke.

**The gap is that nothing ran it for three units.** `TheMenuIsUnderTheMouseTests`
is in the `Views` namespace, whose stall unit 230 documented, and the standing rule
of HM-DEC-155 — *a unit runs only the test it constructs* — means no unit since 273
had licence to look. Units 273, 274 and 275 each named it or its neighbours as
*not run, and you should know which*, and each was right to, and the red sat there
through all three.

So task 3 is **extending a test that works**, not writing one that is missing, and
task 4's correction should say what actually made the false claim survive: **not
that nothing could contradict it, but that something did and nobody was allowed to
look.**
