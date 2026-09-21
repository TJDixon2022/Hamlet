```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 done. Step 1 partial, three
   units spent, 1.1 and 1.2 banked by unit 372. Steps 2, 3, 4, 5 not started.
B. Step 1 - Hamlet opens whole. 1.3 met - the panels drew 0 px before this unit
   and draw 73 px each now at 900x620, scrolling inside themselves with their
   last content reachable. 1.4 met - the best-bet red is off, and the cause was
   the test asserting one of two ruled labels rather than the application.
   Both must-pass; 1.1 carried met and re-checked green, 1.2 carried met and
   re-checked NOT green - its guard is 3 of 4.
C. The report last. Section 4 raises 4 items on top of the carried eleven, and
   item 1 is in the way of a criterion in B - it is 1.2, whose guard went red on
   the floor that met 1.3. Step 1 does NOT close: 1.3 and 1.4 are met, 1.2 is
   back to partial, and no floor satisfies both.
```

```
UNIT:       373 - complete at task 4 of 5 - 2026-09-20 21:32
PHASE GOAL: Bank what Hamlet already has. Five steps of screen, record and test
            work that needs neither the radio nor the owner, run unattended.
UNIT GOAL:  Finish step 1 - stop the three working panels disappearing when the
            window gets short by giving their canvas a measured floor and a
            scroller of its own, without the send area moving a pixel, and take
            the last red off 1.4.
ADVANCED:   step 1, criteria 1.3 and 1.4 both met; 1.2 regressed to partial on
            its guard's sweep precondition, which is section 4 item 1
NUMBER:     the panel row's height at 900x620: 0 px -> 73 px
DRIFT:      none
```

## 1. What Claude did

**Complete, at task 4 of 5** — tasks 0 through 4, each committed on its own, on `main`, at
`C:\Source\HamLet`. The project gate passed against the tree: `SHACK_FACTS.md` and
`src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` both present, no `CoreHMI.sln`, no
`MURC.sln`. **Nothing was dropped.** Task 3 was the named drop candidate and was not dropped.

**Task 0.** Carry-forward before any change, both invocations, one build each: **app 206 of 206,
engine 146 of 146**, name for name identical to unit 372's runs — no finding. Version
**1.13.59 → 1.13.60** with its line in the log. `UNIT 373 - STEP 1` appended to `PHASE_OUTCOME.md`
with `ADVANCED: step 1`.

**Task 1 — measured both repairs before making either.** `Unit373TraceTests`, printed and asserting
nothing about new behavior; `git diff 54b85eec -- src/` read **empty** at that commit, so the
application was task 0's byte for byte. Two answers, both in section 3.

**Task 2 — criterion 1.3, and it is met.** A `ScrollViewer` and a measured `MinHeight` on the panel
canvas inside `WorkspaceBoundary`, at exactly the site §6 names. **The panel row goes from 0 px to
73 px at 900 x 620 and at 1100 x 620.** `TheWindowHoldsBelowItsMinimumTests` is **3 of 3** where
unit 372 left it 2 of 3 committed red.

**Task 3 — criterion 1.4, and it is met.** Task 1's measurement said a badge *is* drawn for 40 m
and is wearing the other of HM-DEC-046's two ruled labels, so under §6's second ruling this was the
§R12 case and not an application fault. Rewritten in its own commit. All four of 1.4's types green:
**`BindingHealthTests` 1 of 1, `TheTopRowTests` 15 of 15, `TheWorkingPanelsTests` 8 of 8,
`TheStopIsAlwaysOnScreenTests` 5 of 5.**

**Task 4.** Carry-forward after the last change: **app 206 of 206, engine 146 of 146**, name for
name identical to the entry run — **no regression**. One name added to
`docs\carry-forward-tests.txt`. Records written criterion by criterion.

### Decisions this session made for itself, reproduced in full

**The floor's number is 185 px, and it is measured rather than chosen.** §6 asked for *the smallest
floor at which all three panels report a working scroller at 900 x 620*. Measured on a ladder of
nine candidate floors at two widths: all three panels report a working scroller only while the panel
row is between **60 and 95 px**, because the decoded list's content measures 36 px and the panel's
own chrome takes 59 of the row — so the canvas floor's working band is **172 to 207 px**. The
arithmetic smallest is 172, where the decoded viewport reads **1 px**, which is a working scroller
only arithmetically. **185 is taken**: it is inside the band, and it is the canvas Hamlet already
draws at **1100 x 780, the size it opens at**, so nothing changes at the opening size. *The
session's own, overrulable.*

**One clause of `TheWindowHoldsBelowItsMinimumTests` was rewritten under §R12, in its own commit.**
Unit 372 wrote *each panel is whole on the window* while the canvas did not scroll, where it is the
right way to say *nothing is hidden*. **A panel taller than its viewport is never whole on the
window**, so no scrolling canvas can satisfy it and the clause would have had to be read as *the
canvas may not scroll* — the opposite of criterion 1.3. That is §R12's case exactly: a test written
while a door was shut, blocking the unit told to open the door. **What replaced it asserts more, not
less**: each panel's first row on the window with the canvas at rest, **and each panel's last row on
the window once the canvas is scrolled to its end** — §0.5's actual rule, which *whole on the window*
never checked, since a panel can be whole and still have unreachable content. Where the canvas does
not scroll the two readings are identical. **And one assertion was added that unit 372 did not
make**: each list panel's scroller must report `viewport > 0` in its own right, because the
inherited fault was a viewport of 0 with content behind it and `extent > viewport` alone does not
forbid it — 36 in a viewport of 0 satisfies that comparison.

**`TheWindowGivesUpHeightInOneOrderTests` was NOT modified**, per §10, and it is now **3 of 4**. That
is section 4 item 1 and it is the reason step 1 does not close.

### Mismatches with the instruction, reported and not repaired

- **§5's 138 px is 13 px high.** The chrome between `WorkspaceBoundary`'s outer edge and the first
  panel pixel measures **125 px** at width 1100, not 138 — 13 px of the border's own top edge and
  padding, plus 112 px of the mode strip and the readiness strip. Unit 372's 138 appears to count
  the border's *bottom* padding and edge, which is not between the outer edge and the first panel
  pixel. Every other line §5 named was correct, including the 2956 correction it took.
- **§5's claim that the rest of the canvas varies with width is refuted.** The chrome is **constant
  down the whole 780-to-620 sweep at each width** — 125, 125, 108 at widths 900, 1100, 1920. The
  17 px across widths is entirely `DigitalReadinessStrip` wrapping to two lines. **The floor is one
  number, not three.**
- **`tools/run-carry-forward.sh` is not the list.** Its `APP` set is an older, different set of
  names — it has no `TheSettingsSurviveAnUpgradeTests`, no `CallsignPrivacyTests`, no `TheOlivia*`
  names and no `TheStopIsAlwaysOnScreenTests`. The two invocations were run from
  `docs\carry-forward-tests.txt`'s own command lines, as §1 says. *Reported, not repaired* — the
  script is not this unit's.
- **`PROJECT_STATUS.md`'s `RULES_AT` reads `HM-DEC-165 (2026-09-19)` while `CLAUDE.md` §1 holds
  `CPS-DEC-0165`** — the id-scheme split §5 names, carried in `PHASE_PLAN.md` §7. *Reported,
  carried, not repaired*, as §9 says.
- **`PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` were modified and uncommitted against
  HEAD `9a6ba2de`, and the two `.run-unit\unit372-*.sh` files were untracked**, exactly as §5 said.
  They were committed with task 0 rather than left dirty.
- **No worktree was asked for and none was attempted.** Where task 1 had to show the application
  unchanged, `git diff 54b85eec -- src/` was used and read empty, as §2 directs.
- **`validate-output.bat` is not in the tree.** §12 says it refuses a report without the ordering
  block; there is no such file at the root or one level down, so nothing checked this report's shape
  but the instruction itself. The block is written as §12 specifies.

### Tool facts, confirmed and added

`;` and second operations in one command are refused — confirmed, and every multi-step action was
split. **Shell output redirection was *not* refused in this session**: `sed ... > file` inside
`.run-unit\` was refused only as part of a compound command, and files were written with the editor
regardless, as §2 directs. `-m` more than once for a multi-line commit worked throughout. **Python
was not needed and was not used.**

## 2. What the owner should expect

When you drag Hamlet's window small, the waterfall, the decoded text and For You **stop vanishing**.
Before this unit, at 900 x 620 — the smallest size Hamlet will open at, and the smallest you can drag
to — all three of those panels were drawn **zero pixels tall** with their contents still loaded
behind them and no way to reach any of it. Unit 372's report promised you this fault would be fixed,
and it is: each of the three is now **73 pixels tall** at that size, and the workspace area below the
mode tabs scrolls, so anything that does not fit is reached by scrolling rather than gone. **The CQ
button, the mode tabs, the send area and Stop do not move, do not shrink and are not inside the
scroller** — the send area measures the same 22 pixels at every height from 780 down to 620, as it
always has. The header and the status bar stay pinned. At **six of the nine screen sizes** this is
measured at, nothing scrolls at all and nothing has changed. At the two sizes the ruling expected to
*lose* panel height — 1280 x 720 and 1366 x 728 — the panels actually **gained** it, 50 → 90 px and
86 → 90 px, with the workspace scrolling 28 px and 2 px respectively. **What will look wrong but is
not:** a scrollbar now appears beside the workspace at short windows, and at the very smallest sizes
the mode strip is what you see first with the panels a short scroll below it. That is the ruled
design — everything but the header and the status bar scrolls (HM-DEC-051) — and it is the price of
the panels existing at all. **Every appearance claim here is computed from a headless window's
measured rectangles, not seen** (FACT-004); nothing in this unit is evidence about the radio, and
nothing in it transmits.

## 3. What you should see

### The chrome per width — the table that says whether the floor is one number or three

`Unit373TraceTests.ThePanelCanvasAtThreeWidths`, before any change. *Chrome* is the pixels between
`WorkspaceBoundary`'s outer edge and the first panel pixel.

| width | 780 | 740 | 700 | 660 | 620 | spread |
|---|---|---|---|---|---|---|
| 900 | 125 | 125 | 125 | 125 | 125 | **0** |
| 1100 | 125 | 125 | 125 | 125 | 125 | **0** |
| 1920 | 108 | 108 | 108 | 108 | 108 | **0** |

**The answer, plainly: the chrome is constant at every height at each width, and varies by 17 px
across the three widths.** The variation is entirely `DigitalReadinessStrip` — 60 px where its text
wraps to two lines, 43 px where it does not. So **the floor is one number, not three**, and unit
372's stated reason for calling this a redesign rather than a number does not hold. The 125 breaks
down as 13 px of the border's own top edge and padding plus 112 px of the mode strip and the
readiness strip with their margins; **unit 372's 138 is 13 px high.**

The boxes behind that table, before the change:

| size | `WorkspaceBoundary` | the `Panel` | panel row | decoded vp/ext | For You vp/ext |
|---|---|---|---|---|---|
| 900 x 780 | 868 x 218 | 842 x 192 | 80 | 21 / 36 | 32 / 371 |
| 900 x 620 | 868 x 58 | 842 x 32 | **0** | **0** / 36 | **0** / 371 |
| 1100 x 780 | 1068 x 211 | 1042 x 185 | 73 | 14 / 36 | 25 / 360 |
| 1100 x 620 | 1068 x 51 | 1042 x 25 | **0** | **0** / 36 | **0** / 360 |
| 1920 x 780 | 1888 x 313 | 1862 x 287 | 192 | 133 / 133 | 144 / 236 |
| 1920 x 620 | 1888 x 153 | 1862 x 127 | 32 | **0** / 36 | **0** / 236 |

And after the floor, at the two sizes the criterion names:

| size | canvas vp / ext | panel row | decoded vp/ext | For You vp/ext | scrolled to end |
|---|---|---|---|---|---|
| 900 x 620 | 32 / **185** | **73** | 14 / 36 | 25 / 371 | 36 of 36; 371 of 371 |
| 1100 x 620 | 25 / **185** | **73** | 14 / 36 | 25 / 360 | 36 of 36; 360 of 360 |

### The floor ladder — why the number is 185, and the one thing it costs

`Unit373TraceTests.WhatEachCandidateFloorBuysAndWhatItCosts`, at width 1100. *Shrinks* is the count
`TheWindowGivesUpHeightInOneOrderTests` reads, and it needs 2 or more.

| floor | panel row at 780, 740, 700, 660, 620 | decoded vp at 620 | shrinks |
|---|---|---|---|
| 0 (today) | 73, 33, 0, 0, 0 | **0** | 2 |
| 120 | 73, 33, 8, 8, 8 | **0** | 2 |
| 144 | 73, 33, 32, 32, 32 | **0** | 2 |
| 160 | 73, 48, 48, 48, 48 | **0** | 1 |
| 172 | 73, 60, 60, 60, 60 | 1 | 1 |
| **185** | 73, 73, 73, 73, 73 | **14** | **0** |
| 192 | 80, 80, 80, 80, 80 | 21 | 0 |
| 240 | 128, 128, 128, 128, 128 | 69 / ext 69 — no longer scrolls | 0 |
| 480 | 368, 368, 368, 368, 368 | 309 / ext 309 — no longer scrolls | 0 |

The same ladder at width 900 gives the same answer. **Every floor that satisfies 1.2's guard leaves
the decoded viewport at 0, and every floor that gives the panels a working scroller fails it.** That
is section 4 item 1.

### The panel row at unit 354's nine sizes, before and after

| size | unit 372 | unit 373 | canvas |
|---|---|---|---|
| 1920 x 1040 | 450 | 452 | fits |
| **900 x 620** | **0** | **73** | scrolls 153 px |
| 1100 x 780 | 71 | 73 | fits |
| **1280 x 720** | 50 | **90** | scrolls 28 px |
| **1366 x 728** | 86 | **90** | scrolls 2 px |
| 1536 x 824 | 228 | 230 | fits |
| 1400 x 1040 | 424 | 426 | fits |
| 1920 x 1017 | 427 | 429 | fits |
| 2560 x 1400 | 827 | 829 | fits |

Unit 372's row: **450, 0, 71, 50, 86, 228, 424, 427, 827.** Unit 373's: **452, 73, 73, 90, 90, 230,
426, 429, 829.** **Six of the nine fit with no scrollbar at all.** The change at 1280 x 720 and
1366 x 728 is expected and ruled — and it went the other way from the ruling's expectation: the
panels **gained** 40 px and 4 px. The send area measured **22 px at all nine** (23 at 900 x 620, as
it was before this unit).

### The badge table — the answer to 1.4's question, in one sentence

`Unit373TraceTests.TheBestBetBadgeAtBothWidths`, at 20:53 local, before anything changed. **A badge
IS drawn for 40 m, and it is wearing *likely, going on the hour*.**

| best bet set on | band | `IsBestBet` | `BestBetLabel` | badge Border | wearing |
|---|---|---|---|---|---|
| 20 m | 20 m | True | best bet now | **VISIBLE** | best bet now |
| 20 m | 40 m | False | likely, going on the hour | hidden | likely, going on the hour |
| **40 m** | **40 m** | **True** | **likely, going on the hour** | **VISIBLE** | **likely, going on the hour** |
| 40 m | 20 m | False | best bet now | hidden | best bet now |

Identical at 1920 and at 1400. The failing test's own text filter collects `[20 m]` in the first case
and **`[]`** in the third; the badge borders actually drawn are `[20 m]` and **`[40 m]`**. So the
application was never wrong — nothing is missing and nothing is hidden, and the badge says exactly
what HM-DEC-046 requires a clock guess to say. **And the flake is explained**: `RankBands` passes
`DateTime.Now.Hour` to `BandOpportunities.Rank`, so which band the fixture's startup reload stamps a
label onto is the wall clock's choice, and `ApplyBestBet` writes `BestBetLabel` only onto the band it
badges while the fixture clears `IsBestBet` without clearing the label. **Measured red at hour 20 and
green at hour 21 of the same day, on the same tree.** The rewrite exercises **both** ruled labels at
both bands and both widths — **8 cases where there were 4** — so it no longer turns on the clock.

### The test counts

| type | count | note |
|---|---|---|
| `TheWindowHoldsBelowItsMinimumTests` | **3 of 3** | was 2 of 3, committed red |
| `BindingHealthTests` | 1 of 1 | 1.4 |
| `TheTopRowTests` | **15 of 15** | was 14 of 15; 1.4 |
| `TheWorkingPanelsTests` | 8 of 8 | 1.4 |
| `TheStopIsAlwaysOnScreenTests` | 5 of 5 | 1.4; red in 1 of 4 runs on §5's known flake |
| `TheWindowGivesUpHeightInOneOrderTests` | **3 of 4**, unmodified | **not** 4 of 4 — section 4 item 1 |
| `Unit373TraceTests` | 4 of 4 | printed, asserts nothing about new behavior |

**§5's known flake showed up once.** `TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnThe
BarFiresTheAbortWhileItRuns` was red in **1 of 4** runs of its type, always the same way — the abort
pair on the wire twice, `FE FE 94 E0 17 FF FD` appearing at positions 1 and 3. Re-run twice, green
both times. **Not chased**; it is step 2's criterion 2.3.

### The carry-forward counts, before and after

| | app | engine |
|---|---|---|
| before any change (task 0) | **206 of 206** | **146 of 146** |
| after the last change (task 4) | **206 of 206** | **146 of 146** |

**Name for name identical — no regression, and nothing that was green is red.** The app invocation
was run twice at task 4: the first attempt died at
`TheStopIsAlwaysOnScreenTests.AtEachOf354sNineSizesStopIsInTheStatusBarAndOnTheWindow` after 1 ms
with Avalonia's headless `InvalidProgramException: You've caused dispatcher loop`, which is a test-
session fault rather than an assertion, and the re-run was 206 of 206.

**One name was added to `docs\carry-forward-tests.txt`**, by type and method:
`TheWindowHoldsBelowItsMinimumTests.TheWorkingPanelsScrollInsideThemselvesRatherThanCollapsing`.
**Why, against the default of adding nothing:** task 2's floor *is* the thing that keeps the panels
reachable, and it is one `MinHeight` and one `ScrollViewer` in `MainWindow.axaml` that a later layout
change could undo without meaning to — the fault it guards sat in the tree through two units before
anything measured it. **The cost is three seconds**, the whole type, opening no device and reaching
no network. It is green, not knowingly red. The type's other two names were **not** added, as §R14
asks.

## 4. What's blocking us

**One item is in the way of a criterion, and it is why step 1 does not close.** Four items from this
unit, most-blocking first, then the carried queue.

### Raised by this unit

**1. No floor satisfies both criterion 1.3 and `TheWindowGivesUpHeightInOneOrderTests`' sweep
precondition, and 1.2 is back to partial because of it.**

*Ruling wanted. It is the one thing standing between step 1 and done, and it is the mirror image of
what unit 372 faced.* §6 ruled the repair and §6 point 2 stated that *the panels still give up height
first and the top row second, which is 1.2, and `TheWindowGivesUpHeightInOneOrderTests` stays 4 of 4
unmodified as the proof of it.* **That premise is refuted by measurement.** The type's third name,
`TheWorkingPanelsLoseHeightBeforeTheTopRowLosesAny`, opens with a precondition — *something has to
give somewhere in the sweep, or this name proves nothing* — requiring the panels to shrink at **two
or more** of the four transitions in the sweep 780, 740, 700, 660, 620 at width 1100. **A floor is
exactly what stops them shrinking.** The arithmetic, from measured numbers and confirmed on a ladder
of nine candidate floors at two widths (the table in section 3): the canvas viewport at width 1100
descends **185, 145, 105, 65, 25**, so two shrinks require a floor **below 145**; 112 px of that
canvas is chrome and the decoded panel's own chrome is another 59, so all three panels have a
viewport above zero only above a floor of **172**. **The gap is 27 px and there is nothing in it.**
Floor 144 keeps the guard green and leaves the decoded viewport at **0** — 1.3 unmet. Floor 185 meets
1.3 and gives the guard **zero** shrinks. *What was done*: the floor that meets 1.3 was built, and
**`TheWindowGivesUpHeightInOneOrderTests` was not modified**, because §10 forbids it in terms. It is
**3 of 4**: the send area is the same height at every height in the sweep, `TopRow` never passes its
cap and gives up nothing, and the send area and Stop are whole at every height — **the order rule
itself still holds and is still green**. What is red is only the claim that the order is *observable
in this sweep*. *The repair, if the owner wants it, is one line*: extend that type's `Sweep` upward
past the floor — the panels shrink from 1040 down to 73 — which would keep all four rules and every
assertion unweakened and assert strictly more than today. **That is §R12's shape and this session
judged it outside §10's permission**, so it is put here rather than taken. **1.2 is reported partial
and step 1 partial. Not rounded up.**

**2. `ApplyBestBet` leaves a stale `BestBetLabel` on every band it un-badges.**

*A finding, and it is not a defect the operator can see.* `MainWindowViewModel.ApplyBestBet` writes
`BestBetLabel` and `BestBetTooltip` only inside `if (isBest)`, so a band that *was* the best bet keeps
the words it earned after the badge moves away. It is invisible in the application, because the badge
`Border` is bound to `IsBestBet` and is not drawn — HM-DEC-046 is not broken on screen. **It matters
because it made a test turn on the wall clock**: the fixture sets `IsBestBet` by hand without going
through the ranking, and a band carrying a stale label then wears it. *Not repaired* — it is not this
unit's criterion, and §R14 says a unit does not fix doors it is not building. The test no longer
depends on it either way.

**3. §5's 138 px of chrome is 13 px high, and its reason for calling 1.3 a redesign is refuted.**

*A finding.* The chrome measures **125 px** at width 1100, not 138 — 13 px of `WorkspaceBoundary`'s
own top edge and padding plus 112 px of the mode strip and the readiness strip. The 138 appears to
include the border's bottom padding and edge, which is not between the outer edge and the first panel
pixel. And the claim the redesign argument rested on — that *the rest of the canvas varies with width
as well as height* — **is false as stated**: the chrome is constant at every height at each width
(125, 125, 108) and the 17 px across widths is one strip wrapping. **The floor is one number.**

**4. `tools/run-carry-forward.sh` is not the carry-forward list and has not been for some units.**

*A finding, reported and not repaired.* Its `APP` set is an older, different set of names — no
`TheSettingsSurviveAnUpgradeTests`, no `CallsignPrivacyTests`, no `TheOlivia*` names, no
`TheStopIsAlwaysOnScreenTests`. A unit that ran the script believing it was running the list would
get a green it had not earned. The two invocations here were run from
`docs\carry-forward-tests.txt`'s own command lines, as §1 says, and the new name was added to that
file and not to the script. *Not repaired* — reconciling roughly forty names is not this unit's.

### Asks still outstanding — the carried queue, per HM-DEC-139

**Five came off the queue and are not reproduced below.** Unit 372's **items 1 and 2** were answered
in work instruction 373 §6 — item 1 by the ruling that criterion 1.3's repair is a measured minimum
height and a scroller on the panel canvas inside `WorkspaceBoundary`, item 2 by the ruling that the
inherited best-bet red is this unit's to repair. Unit 372's **items 3, 5 and 6** were absorbed into
this instruction's §2 and §5 — the headless `MinHeight` binding, the refused worktree, and the two
corrected tool facts. **The remaining eleven are verbatim below and this unit answers none of them.**

#### Carried from unit 372's section 4

**4. Step 2's flake is measured, named, and not chased.**

*A finding, recorded for the unit that does step 2.*
`TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns` was red
in **three of seven** runs of its type and **two of seven** runs of the name alone. It is the only
one of the type's five names that was ever red, and it was green in both carry-forward runs. It
always fails the same way: the abort pair is on the wire **twice** — expected key-on, abort, PTT off
and got those plus a second abort and a second PTT off. **The numbers are in
`docs\unit372-flake-measurement.md`** rather than only here, because this file is overwritten and
step 2 would otherwise inherit a rumor. Which of the click's own `StopNow` and the sequence's unkey
wrote the second pair **was not determined**, because the instruction said record and do not chase,
and choosing between them would be a guess presented as a finding (§R14).

**7. The live `PHASE_PLAN.md`'s criterion boxes are never ticked, by any unit.**

*A finding, reported and not repaired.* All four of step 1's boxes are still `- [ ]`, and so are all
five of step 0's, which unit 369 met four of. The state that is actually maintained is in
`PHASE_STATUS.md` and `PHASE_OUTCOME.md`, and this unit followed that convention rather than
starting a second record. **It is the same drift unit 369 raised about the archived Olivia plan**,
one phase earlier. *Ruling wanted only if the boxes are meant to be the record* — if they are, a
unit should be told to tick them.

#### Carried from unit 371's section 4

**2. A guessed answer with no speaker still opens no card.**

*A finding.* The card is keyed by station, so a parse Hamlet cannot put a name to opens nothing —
its row is on his side and marked a guess, which is where it was before. In the record, the parser
named the speaker; had it not, the card would still not appear.

**3. The turn indicator keeps its words rather than a question mark.**

*A finding, the author's call under the decision block.* The card says *Your turn, a guess*; the
order suggested *his turn?*. In grayscale a word survives and a question mark is easy to miss
(§0.6), and the word was already in the tree from unit 319.

**4. Six tests asserted the shut door, two of them on the carry-forward list.**

*A finding about coverage, not a defect.* The door was guarded in six places, which is why the
middle carry-forward run was 204 of 206. All six now guard the rule, each in a §R12 commit.

**5. `psk31_answer_taken` fires once per station per session, not per line.**

*A finding.* A station who answers, goes, and answers again writes one line. The card's own
history is what carries the rest, and `psk31_line_parsed` already writes every line.

**6. Python runs here, contrary to the order's tool facts.**

*A mismatch, reported for the next order.* Scripts written to the scratchpad and run as
`python file.py` worked throughout, as in units 361 and 362.

#### Carried from unit 369's section 4

**2. The `no_transmit_device` refusal was never wrong about the device — the
refusal Tim actually saw was `transmit_device_would_not_open`, and the split above
assumes his device id was still in the file at that moment.** If instead the file
had been reset to defaults, he would have seen `no_transmit_device`, and unit 362's
report says he saw the other. *That means the device id survived and the device
genuinely would not open* — which is a different fault from the settings loss, and
this unit repaired both without proving which one he hit. *Ruling wanted:* whether
that matters enough to chase. His telemetry from 2026-09-14 to 09-19 would settle
it in one read; nothing in this repository has it.

**3. `LearnedAlcReference.Ago()` counts only in seconds and whole minutes.** Now
that the reference survives a restart, a legitimate value is *4320 minutes ago*.
Honest but poor. *Ruling wanted:* whether to extend it to hours and days. *Not done
here* because `TheAlcSentenceTests` asserts the current forms and §10 said not to
touch wording beyond the refusal's.

**4. The archived Olivia plan's checkboxes say 25 of 40, not 38 of 40.**
`docs/phase-olivia-run/PHASE_PLAN.md` has 25 boxes ticked and 15 open, including
several the unit reports say were met (1.5, 4.1–4.8, 5.1–5.3). HM-DEC-166 records
38 of 40 as ruled, which comes from the reports. *Reported, not repaired* — §5 says
repair nothing but this unit's, and an archived phase's plan is not this unit's.

**5. No settings file in the tree was ever reconstructed before this unit, and the
three fixtures are mine.** They carry the right key set, but their *values* are
invented — nobody's real 1.13.30 file was available. They prove the shape loads,
not that Tim's particular file does. *Raised once, not a blocker.*
