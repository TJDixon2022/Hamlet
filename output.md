UNIT:       276 — complete at task 5 of 5 — 2026-09-07 21:44
PHASE GOAL: Hamlet works stations on the air. Not decodes them, not shows them —
            the operator makes a contact with his own radio.
UNIT GOAL:  Every decoded row he can act on has a menu offering what he can do
            with it, and a test opens that menu so the claim cannot be made
            untruthfully again.
ADVANCED:   no — this removed a blocker standing in front of step E's bench half
            and closed no criterion. The instruction said so itself and this
            report does not upgrade it. No decoder path was touched.
NUMBER:     none — and that is a finding. The instruction's goal block carries
            PHASE GOAL, UNIT GOAL and ADVANCES and **no figure at all**, so there
            is nothing to report a before and after for. A phase with a goal and
            no scoreboard cannot tell repair from thrash.
DRIFT:      unknown — the instruction carried no drift count for this session to
            increment, and it cannot be recovered from here, because output.md is
            overwritten and a session cannot read prior reports. The next author
            has to reseed it.

## 1. What Claude did

**Complete. Five tasks of five, none dropped** — including task 5, which the
instruction named as the drop candidate. Windows, `PROJECT: Hamlet` claimed and
confirmed, branch `main`, pushed at every task. Version `1.12.139` -> `1.12.143`.

**The unit's premise was right about the defect and wrong about why it survived,
and that changes what the unit is.**

**Task 1 — where the menu actually is.** Reading only, written up in
`docs/unit276-where-the-menu-is.md` (commit `c61d2c9`). One `ContextRequested`
handler existed in the whole application and it was on `DigitalDecodedRows`, the
**left** list. `DigitalMineRows` had none. The `Log this contact...` item unit 274
added is gated on `CanLogRow`, which is
`Ft8MessageSplit.IsAddressedTo(row.Message, callsign)` — **the same predicate that
decides a row belongs on the mine list.** So the item's condition and the item's
list were mutually exclusive by construction, and **it had never once been shown.**

**And the instruction's central claim, that nothing could have caught it, is not
true.** `Views/TheMenuIsUnderTheMouseTests` raises a real `ContextRequested` on a
real row control in a real window and reads the flyout's items. Its row finder asks
for a row addressed to the operator, inside the list unit 273 stopped putting them
in. **It went red the day that split landed and stood at 5 of 6 failing**, with

```
no realized row matched. Rows on the table: 8;
realized grids with a row DataContext: 8
```

**It stayed red for three units because nothing was allowed to run it.** It is in
the `Views` namespace whose stall unit 230 documented, and HM-DEC-155 rules that a
unit runs only the test it constructs. Units 273, 274 and 275 each reported it or
its neighbours as *not run, and you should know which*, and each was right to.

**Tasks 2 and 3 — the fix and the test, committed together at `c5bb1dc`**, because
the fix and the test that catches it are one change. The mine row's outer `Grid`
gained `ContextRequested="OnDecodedRowContextRequested"` — **the same handler, not a
copy.** The handler reads the row off its own control's `DataContext`, so it neither
knows nor cares which list drew it, and there is no second menu to drift.

The test's row finder now searches both lists by name; `Options()` discriminates a
send item by its `CommandParameter is string` rather than by having any command,
which had been sweeping up the Log item and blurring the very distinction that class
exists to police. Two tests added. **The new tests were watched failing first**, by
stashing the markup back to what the operator found. **8 of 8 green** — the first
time that class has been green since unit 273.

**Task 4 — the record, and a mismatch worth naming.** Committed at `50768aa`.

> **The instruction says to append the correction beneath unit 274's entry in
> `PHASE_OUTCOME.md`, because that is where the untrue claim survives. There is no
> unit 274 entry.** The file's last entry was `## UNIT 272 - STEP E`. **Units 273,
> 274 and 275 wrote none at all.**

So the correction went into this unit's own entry, quoting the claim in full so a
reader who goes looking finds it. Inventing a `UNIT 274` block to append beneath
would fabricate a record of a unit nobody here ran, which is worse than the gap it
papers over. The step header was not touched: **E stays `not started`.**

**What made the false claim possible, which the instruction asked for and got
wrong.** Not that nothing could contradict it. **Something did, and nobody was
allowed to look.** A report can describe a menu item from the code that declares it
without anything ever opening the menu, and that half is right. But the test that
opens it existed, was thorough, and was already red. **The gap is not a missing
test. It is a test that was red and unread.** No blame attaches to unit 274: its
instruction did not ask for the menu to be moved, and the one thing that would have
contradicted it was a test that unit was forbidden to run.

**Task 5 — the sweep, taken rather than dropped.**
`docs/unit276-every-context-menu.md`, commit `3986c50`. **Two context menus in the
whole application and they are the same menu.** Four hits across nine `.axaml`
files, two of them comments; eight of the nine files have no hit at all. No other
control in the app — band cards, spot cards, map dots, dial tape, waterfall, canvas
widgets, the six other windows — has ever declared one, recorded so the next sweep
does not read that as a list of omissions.

**One finding, reported and not repaired.**
`ViewModels/TheWholeChainRunsFromOneRightClickTests` has the **same defect this unit
fixed**, and task 3 did not reach it: its `RightClick` helper at `:895` asks the left
list for a row whose addressee is the operator, and `WantsRow` is
`!IsForHim(row) && ...`, so no such row is ever there. Both of that class's
menu-opening tests go through it. **Read from the code, not from a run** —
HM-DEC-155 again — and recorded as **HM-OPEN-086**, left alone under §12.6. The
repair looks like one line, the same list widening task 3 made, but it wants its own
unit, because a test nobody watched go green is how this survived three units.

**Decisions this session made for itself:** where a correction goes when the entry it
was aimed at does not exist (task 4 above, reproduced in `PHASE_OUTCOME.md` in full),
and two test-shape choices recorded in `c5bb1dc`. Nothing touching §0.0, what the
display asserts, or transmit.

## 2. What the owner should expect

**He can right-click a message addressed to him and answer it.** That is the whole
change on his screen. The right-hand list — the one carrying his own traffic, which
is the only place a reply is ever sent from — now opens the same menu the left list
has had since step B: the reply the ledger says comes next highlighted, the others
offered, a repeat showing its count, and beneath a rule, `Log this contact...`.

**Nothing else moved.** No column changed width, no row changed shape, no filter
behaves differently. The left list is exactly as unit 275 left it.

**What will look wrong and is not:**

- **`Log this contact...` appears on the right list only, never on the left.** That
  is correct and is now what the tests assert. It is gated on the message being
  addressed to him, and every such row is on the right.
- **A third-party exchange, two other stations working each other, still opens a
  menu with five send options and no Log.** Offering him a way to break in is the
  point; offering to log a contact he was not part of would be a false claim.
- **`PHASE_OUTCOME.md` has no unit 273, 274 or 275 entry.** That is the tree, not a
  deletion by this unit.
- **Inherited reds are untouched and unrun**:
  `CwAdjudicationTests.ASpeedChangeInRealisticAudio`, the 51 CW cases in
  `docs/unit239-failing-set.txt`, the `Ft8Sharp.Deep.Tests` whole-type-list
  tripwire, and now **HM-OPEN-086**, which this unit found by reading and
  deliberately did not fix.
- **No suite was run.** Per HM-DEC-155 this unit ran one filtered class, its own.

## 3. What you should see

**The menu opened on a mine row, its items in order.** Row `KC3QIS N5TT EM10`,
N5TT calling him:

```
    N5TT KC3QIS FN00   grid
    N5TT KC3QIS -10    report - the one that comes next
    N5TT KC3QIS R-10   roger and report
    N5TT KC3QIS RRR    acknowledge
    N5TT KC3QIS 73     73
    ----------------
    Log this contact...
```

Five send options, every one enabled, then a rule, then Log. The Log item carries
the **row** where a send item carries a **message string**, which is how the test
knows it transmits nothing rather than trusting its wording.

**The menu opened on a left row.** A CQ, `CQ G4XYZ IO91`:

```
    G4XYZ KC3QIS FN00   grid
    G4XYZ KC3QIS -10    report - the one that comes next
    G4XYZ KC3QIS R-10   roger and report
    G4XYZ KC3QIS RRR    acknowledge
    G4XYZ KC3QIS 73     73
```

and a third-party exchange, `K9TC KJ6IX RRR`, two other stations working each
other:

```
    KJ6IX KC3QIS FN00   grid - the one that comes next
    KJ6IX KC3QIS -10    report
    KJ6IX KC3QIS R-10   roger and report
    KJ6IX KC3QIS RRR    acknowledge
    KJ6IX KC3QIS 73     73
```

**Both offer the sends. Neither offers Log**, and no rule is drawn where there is
nothing under it. The highlight moves, too: `grid` comes next against a station he
has no exchange with, `report` against one that has already called him.

**The test watched failing first**, against the tree as the operator found it:

```
Failed  OutOfPrivilegesTheMenuSaysSoAndForbidsNothing
Failed  EveryStationsPredictedMenuAppearsUnderTheMouse
Failed  TheRepeatCountBelongsToTheClickAndNotToTheRow
Failed  WithNoGridTheReasonIsANoteAndTheRestStayClickable
Failed  ChoosingOneGoesThroughTheOneCommandThatArms

Failed! - Failed: 5, Passed: 1, Skipped: 0, Total: 6
```

and with the markup stashed back, the new test's own red said the defect in words:

```
right-clicking a row on the mine list produced no menu. The list carries every
message addressed to the operator, so it is the one place a reply is ever sent
from.
```

After tasks 2 and 3: **`Passed: 8, Failed: 0`**, 4.1 seconds.

## 4. What's blocking us

Nothing blocks the next unit. Two things want a ruling, neither urgent.

---

**`PHASE_OUTCOME.md` is written by a tool no unit is told to run, and three units in
a row did not run it.**

Units 273, 274 and 275 wrote no outcome entry. This unit only noticed because task 4
sent it looking for one. The file is the phase's only durable record, since
`output.md` is overwritten every unit, so three units' findings survive nowhere but
commit messages, and a correcting instruction aimed at one of them had nothing to
aim at.

*Rejected: making every unit write one by habit.* Habit is what failed here. Three
consecutive units had that habit and it produced nothing. The write should be a task
in the instruction, or the tool should run at the end of the loop where the caller
can see that it did not.

*Rejected: back-filling entries for 273, 274 and 275.* Nobody here ran them, and a
reconstructed entry is a fabricated record.

---

**A work instruction with no figure in its goal block cannot report a number.**

`CLAUDE_CODE.md` §8 requires `NUMBER: <before> -> <after>` and says `none` is a
finding. This instruction's block carries no figure, so `none` is what the header
says. That is the second half of the drift problem rather than a separate one:
**this report also cannot carry `DRIFT` forward**, because the instruction did not
carry a count in and `output.md` is overwritten, so the count cannot be recovered
from inside a session. It has to be reseeded by the next author.

*Rejected: inventing a plausible drift count.* §13.3 and §12.4 both forbid it, and a
laundered number is worse than an admitted gap.

---

## Asks still outstanding

Carried per HM-DEC-139. Five inbound, all still open, carried verbatim; one added.

1. **Two issues of one work-instruction number.** Raised by unit 271, and unit 252
   before it. **The author's error: an executed order must never be amended, only
   succeeded.** No unit action.
2. **`PM95` reads *southern Japan***, raised by unit 271. **Not a defect**; the
   table is where to argue with it.
3. **`HM-OPEN-083` and `HM-OPEN-084`**, raised 2026-09-05. By HM-DEC-140 they live
   in `OPEN_ISSUES.md` and not on this queue.
4. **Three pixels.** Unit 275 left `VP2MAA KC3QIS FN00` three pixels over on the
   left list, on a headless measurement whose own uncertainty is about that size,
   and proposed no change. **Tim is looking at it on his own screen.** No unit
   action until he says.
5. **`dt` and `hz` were never on the mine list.** Unit 275 reported the mismatch
   rather than repairing it. The mine list now has 224 px of message to spend if he
   wants them there. **No unit action until he says.**
6. **Where an outcome entry goes when the unit it corrects has none**, raised by
   this unit, 2026-09-07. Decided one way here and written into `PHASE_OUTCOME.md`
   under `## UNIT 276 - STEP E`; the ruling above is what wants his eye. The change
   is already in the tree at `50768aa`.
