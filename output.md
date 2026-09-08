UNIT:       279 — complete at task 7 of 7 — 2026-09-08 10:55
PHASE GOAL: Hamlet works stations on the air. Not decodes them, not shows them —
            the operator makes a contact with his own radio.
UNIT GOAL:  A station he has worked is dim rather than labelled, no test can
            silently fail to find its own subject, and the turn line tells the
            truth while he is transmitting.
ADVANCED:   no. Every bench step of this phase is closed and steps D and E are Tim
            at his own radio, so no unit can meet their criteria. The author said
            so at the head of this order and this report does not dress it up.
NUMBER:     177 px -> 177 px. The left list's message column is unchanged, against
            the 180 px `VP2MAA KC3QIS FN00` needs. **The three-pixel ask is NOT
            settled** and nothing was shrunk to pretend otherwise. What the removal
            did buy is below.
DRIFT:      2 consecutive units without advance (was 1, carried from unit 278).

## 1. What Claude did

**Complete. Seven tasks of seven, none dropped** — including task 6, the named drop
candidate. Windows, `PROJECT: Hamlet` claimed and confirmed against all four gate
checks, branch `main`, pushed at every task. Version `1.12.155` -> `1.12.163`.

**Task 1 — the fade** (`4464439`). The word `worked` is gone from both lists and a
station already in the log is drawn at **0.55 opacity**. **The mechanism carries the
ruling**: grey is this project's reserved signal for a control that genuinely cannot
be used (§0.5.1, HM-DEC-087), so greying would have borrowed exactly that signal for
a row he is free to work again. Opacity keeps every colour the row already has where
greying replaces them with the disabled palette. Unit 274's hover wording moved onto
the row, null rather than empty where there is nothing to say.

**And the class remark on `DigitalDecodeRow` was false and is corrected.** It said
nothing on the row changes after it arrives, written by unit 252 when that was true;
unit 274 gave it a mutable `WorkedBefore` and unit 277 a mutable `RepeatCount` and
the remark stayed. HM-DEC-159 is about exactly that.

**Task 2 — `FACT-006`** (`02b9dc1`). The development machine has no contact log and
never will. It rules out a unit writing a task that needs the real log, names the two
seams a test uses instead, and records his reading: **684 bytes, 2 records, about 342
each**, against the 240 unit 278 synthesised.

**Task 3 — measured** (`e959899`). See section 3.

**Task 4 — the sweep** (`docs/unit279-tests-that-cannot-see.md`, `3eca8a2`). Reading
only, no suite run. **594 test files, 172 carrying a predicate finder, nine of the
dangerous shape**, and the doc says why the rest are safe so the next reader does not
mistake silence for omission.

**Task 5 — `HM-OPEN-086` closed** (`4d91bfe`, `85481b3`). See section 3.

**Task 6 — the turn line while transmitting** (`3f0b204`). Two new states. **The
exposure is the one the instruction named**: telling him his slot is open while his
own carrier is on it is worse than saying nothing, because the one thing he might do
about an open slot is fill it. It counts the transmission, not the slot. **Nothing
reads it to act** — no arm, no cancel, and a transmission ending sends nothing.

**Task 7 — the outcome entry** (`0ceb54b`), written by the tool, exit 0.

**Decisions this session made for itself**, both in the outcome entry in full: that
dimming is opacity rather than a greyed foreground, and that a test which examined no
subjects has not passed.

## 2. What the owner should expect

**A station he has worked fades rather than wearing a label, and the turn line stops
lying while he transmits.**

The green `worked` is gone from both lists. A row for a station already in his log is
drawn softer, keeps its full right-click menu with the same items, is still
clickable, and says on hover *You worked IK4LZH before, on 2026-09-07, on 20m*.

While his own transmission is going out, the line above the For you panel says he is
transmitting and how long is left of it, instead of telling him his slot is open.

**What will look wrong and is not:**

- **The message column is no wider on an ordinary row.** The mark's column took no
  width on a row with nothing to show. What it gives back is the ~68 px it was taking
  on rows for stations he had worked, which were the rows clipping worst.
- **A faded row is not a disabled row.** Nothing on it is forbidden.
- **The two band-label tests now fail if they find no labels.** They pass today.
- **Inherited reds, untouched and unrun**: `CwAdjudicationTests.ASpeedChangeInRealisticAudio`,
  the 51 CW cases in `docs/unit239-failing-set.txt`, the `Ft8Sharp.Deep.Tests`
  whole-type-list tripwire.
- **`src/Ft8Sharp/` is untouched** and its version did not move.

**One mismatch with the instruction**: it says unit 274 added the `Auto` column *to
both grids*. It is on the **left list only** — the mine side's mark shared the 76 px
the time column already had. So removing it gives the mine side nothing.

## 3. What you should see

**1. A dimmed row and an undimmed one, with the dimmed one's menu open.**

```
IK4LZH opacity 0.55
W1ABC  opacity 1
text blocks reading "worked": 0

menu on the faded IK4LZH:          menu on W1ABC:
    IK4LZH KC3QIS FN00   grid          W1ABC KC3QIS FN00   grid
    IK4LZH KC3QIS -11    report        W1ABC KC3QIS -13    report
    IK4LZH KC3QIS R-11   roger…        W1ABC KC3QIS R-13   roger…
    IK4LZH KC3QIS RRR    acknowledge   W1ABC KC3QIS RRR    acknowledge
    IK4LZH KC3QIS 73     73            W1ABC KC3QIS 73     73
```

**The same five options, none disabled.** The hover text, on the row:

> You worked IK4LZH before, on 2026-09-07, on 20m.

and on `W1ABC`, nothing at all rather than an empty box.

**Watched failing first** with the markup put back exactly as unit 274 left it:
*text blocks reading "worked": 2*, and the row's tooltip null.

**2. The column measurement, at 1400 × 1200 through the real window.**

```
left  message column : 177 px      (unit 275 measured 177)
mine  message column : 225 px
VP2MAA KC3QIS FN00   : 180 px
left  headroom       : -143 px  (against the widest legal message, 320 px)
```

**The three pixels are not settled.** The reason is the useful half: `Auto` takes no
width on a row with nothing to show, so `LeftFixed` was already `76 + 48` before the
column came out and is `76 + 48` after. **On an unworked row the removal buys
nothing.** What it buys is the width the label was taking on rows that carried it —
measured at **60 px plus an 8 px margin** in the realized window during the
watched-failing run — so a row for a worked station had **109 px** of message where
every other row had 177, and now has 177 like the rest.

So the removal helps precisely the rows it was taking width from, and the three pixels
were never about the mark.

**3. `TheWholeChainRunsFromOneRightClickTests`, run for the first time since the
split.**

```
Passed!  - Failed: 0, Passed: 3, Skipped: 0, Total: 3, Duration: 59 s
```

**Green, and it uncovered nothing.** The instruction says to report a real defect in
the chain and not repair it here; there is none. **The whole chain has run from one
right click all along, behind a helper that could not reach it.** Its `RightClick`
asked `DigitalDecodedRows` alone for a row whose addressee is the operator, and
`WantsRow` is `!IsForHim(row) && ...`. Fixed the way units 276 and 277 fixed the same
shape, deliberately rather than inventing a third approach.

**And the sweep's own find, which is worse than that shape.**
`TheOperatingScreenIsLaidOutAsRuledTests` and `TheBandRowIsWhereItWasRuledTests` both
carry this at line 166:

```csharp
var label = card.GetVisualDescendants().OfType<TextBlock>()
    .FirstOrDefault(t => t.Text == band);

if (label is null)
{
    continue;
}
```

They collect the labels that are cut short and assert the collection is empty. **A
run that finds no labels collects nothing and passes** — a test named *every band
label renders inside its own card* going green with none of them rendering, guarding
the very defect that put `10 n` on his screen on 2026-08-26.

**Tests.** 58 passed and 1 skipped across the nine app classes this unit wrote or
rewrote, plus 13 in the engine's beat class. All green. Every one was run because
this unit changed code it guards.

## 4. What's blocking us

Nothing blocks the next unit. Two things want a ruling.

---

**A faded row carries its meaning in appearance alone, and §0.6 says colour may never
be the only carrier.**

Your ruling was explicit and it is built exactly as ruled: the word is gone, the row
fades. **The thing worth your eye is what the fade costs a reader who cannot see it.**
Printed in grayscale, or by somebody with a colour vision deficiency, a 0.55 row is
still visibly lighter, so the signal survives — that is why opacity rather than a hue
change. But the *meaning* of the fade is carried only by the hover text, and hover is
not available at a glance.

**No change is proposed and none was made.** The options if you want one are a small
glyph in the time column, or leaving it exactly as it is on the grounds that the fade
is a nicety and the log window is where the answer really lives.

*Rejected: quietly putting a word back.* You removed it.

---

**The two band-label tests now fail when they find nothing, and that is a new
assertion however narrow.**

The instruction says to change only what a test says when its fixture cannot produce
a subject. A count of subjects examined is arguably a new assertion rather than a
better message, and I made it on the grounds that **a test that examined nothing has
not passed**. Both pass today, so nothing changed colour.

*Rejected: only improving the message.* There is no message to improve — the failure
mode is silence, and a better sentence nobody ever sees fixes nothing.

---

## Asks still outstanding

Carried per HM-DEC-139. Eleven inbound; three discharged by this unit, eight carried,
two added.

1. **Two issues of one work-instruction number.** Unit 271, and 252 before it. **The
   author's error.** No unit action.
2. **`PM95` reads *southern Japan***, unit 271. **Not a defect.**
3. **`HM-OPEN-083` and `HM-OPEN-084`.** In `OPEN_ISSUES.md` by HM-DEC-140.
4. **Three pixels.** **Task 3 did not settle it**, and the measurement above says why:
   177 px against 180, unchanged, because the mark's column took no width on the rows
   measured. **Still waiting on Tim.**
5. **`dt` and `hz` were never on the mine list.** **Waiting on Tim.**
6. **Where an outcome entry goes when the unit it corrects has none.** Unit 276, in
   the tree. **The ruling wants Tim's eye.**
7. **`PHASE_OUTCOME.md` is written by a tool no unit is told to run.** Units 277, 278
   and 279 each wrote one because their instructions named the tool. **The general
   question is the author's and still open.**
8. **Where the repeat fold stops**, unit 277, in the tree at `0c359cc`. **The ruling
   wants Tim's eye.**
9. **Whether a faded row needs a second carrier of its meaning**, raised by this unit,
   2026-09-08. Built exactly as ruled; the question is what the fade costs a reader
   who cannot see it.
10. **Whether counting subjects is a new assertion**, raised by this unit,
    2026-09-08. In the tree at `4d91bfe`, both tests passing.

**Discharged by this unit:** ask 8 of the inbound queue, `HM-OPEN-086`, now closed in
`OPEN_ISSUES.md`; ask 10, the turn line during a transmission, built in task 6; and
ask 11, the real log, closed permanently by `FACT-006`.
