UNIT:       050 — complete, 6 of 6 — 2026-08-29
UNIT GOAL:  Stop the app leaving the radio in the wrong mode for the block, and prove one read answers the question it asked.
ADVANCED:   yes — the dial now has to come to rest before data territory writes anything, and an unconfirmed write no longer leaves half a measured-looking answer on screen.
NUMBER:     0 writes across three 40 m data blocks at a tuning speed; 1 write when the dial stops. Engine Rig+Explore 789 passing, 0 failing.

## 1. What Claude did

**All six tasks are accounted for. Nothing was dropped.** Task 6 was the named
drop candidate and it did not need dropping — it is already built, and that is
one of four mismatches below.

Development computer, prompt claimed `PROJECT: Hamlet`, gate verified against the
tree — `CLAUDE.md`'s header says `Project: Hamlet`, the solution is `Hamlet.sln`,
the namespaces are `Hamlet.*`, and `WORK_INSTRUCTIONS.md` opens with the same
line. Branch `main`, per §9.5.1. Version **1.12.6 to 1.12.7** per HM-DEC-150.
**Nothing in this report is evidence about the radio** (`SHACK_FACTS.md`,
HM-DEC-093). The eight 2026-08-29 captures are still not in the tree — an eighth
consecutive unit.

### Task 1 — the double query does not reproduce, and the gate fired negative

The order's own gate: *if the double query does not reproduce, stop here and
report; do not build tasks 2 and 3 against a fault that is not there.* **It does
not reproduce, so tasks 2 and 3 did not open.**

`Ic7300Rig.HandleFrame` already drops the controller's own echo by sender
address, routes transceive frequency and mode and the scope stream **before** the
pending path, and matches a reply on the command byte **and** the sub-command via
`SubCommandMatches`. `RequestAsync` throws `TimeoutException` rather than
returning whatever arrived.

Five tests in `OneReadReturnsTheAnswerItAskedForTests`, each putting a suspected
interference in front of the true reply:

| what is put in the way | one read answers correctly |
|---|---|
| the radio's own echo, "Link to [REMOTE]" | yes |
| a transceive broadcast mid-read | yes |
| both at once, USB and USB-D | yes, first read, both |
| nothing answers at all | unknown, not the last thing seen |

**Two of them assert the interference genuinely arrived** — `Inbound > Answered`
for the echo, `InboundTransceive > 0` for the broadcast — because a test that
passes with nothing injected proves nothing (§12.5).

### Task 4 — the dwell rule

`ModeDwell` is a pure record struct that **takes its own clock as an argument**
(§5.4), so the tests advance time by hand and the same sequence of looks gives the
same answer every run.

**Movement disqualifies, not position.** The condition is the same neighborhood
and an unchanged frequency across consecutive looks spanning one second. A new
250 ms timer does the looking, because **a dial standing still raises no change
notification at all** — a settle that listened only to changes could never observe
stillness. Suppressed entirely while the scanner runs; leaving re-arms from zero;
leaving early is silent, and a matured dwell fires exactly once rather than four
times a second at a dial nobody is touching.

Measured on the real map, 40 m, which carries three digital blocks nose to tail
(PSK31 7.070, FT8 7.074, JS8 7.078):

| pass | writes |
|---|---|
| slow tune, 100 Hz every quarter second, three blocks crossed | **0** |
| scanner running, 500 Hz steps | **0** |
| scanner parked dead inside FT8 city for ten seconds | **0** |
| dial stops inside FT8 city | **1** |

**The last row is the control.** Without it the first three would pass on a rule
that never writes at all, which is §12.5's failure in miniature.

### Task 5 — data territory sets its mode on dwell, and one real defect found

The frame is asserted rather than the outcome, because a radio already in the
wanted mode would hide a frame that never carried the data flag — and the flag is
the entire reason command `26` is used instead of `06`.

**The defect: an unconfirmed write emptied the mode and left the data variant
reading whatever it last held.** `ReportModeUnknown` marked `RigField.Mode`
unknown and said nothing about `RigField.DataMode`. So a refused or unanswered
`26` left the badge blank beside a data flag still claiming USB-D, from before the
write, looking measured. **A half-emptied answer is the worse shape of the two.**
Both go unknown now. The filter is deliberately not touched, on unit 042's own
reasoning: nothing said anything about it either way.

Seven tests, including the evening of the 28th as a fixture — 14.074 MHz, CW,
FIL2, 500 Hz — proving it is **put right by stopping there and not by passing
through**.

### The four mismatches — reported, not repaired

Per the order: *report the mismatch; do not repair the instruction.*

1. **"That the app reads `26` rather than `04` for the mode" — it reads both.**
   `CivReads.cs:109` reads Mode via `04`; `:126` reads DataMode via `26`. The
   order lists this as an unverified assumption and the tree does both on purpose.
2. **Task 5 says "no filter byte" and the tree sends one.** `MainWindowViewModel`
   sends `CivWrites.WidestFilterSlot` where the block states a passband. That
   landed in `46313cf` on 2026-08-28 as tasks 2 and 5 of work instruction 040,
   **after** HM-DEC-149's "only the mode is written… not the filter", on the
   evidence of an hour lost to a 500 Hz window over a 3 kHz block. **It was not
   removed**: undoing a fix for a measured failure on the strength of an order
   written without knowing it existed is the wrong direction, and it is a ruling
   ask below.
3. **Task 6 is already built and was not dropped.** `Neighborhood.
   WhereTheSignalsAre()` and `MainWindowViewModel.ShowNeighborhood` compose
   exactly what the order describes, from the row's own jump frequency and span.
   Same commit, same unit 040.
4. **The CW/FIL2/500 Hz fixture at 14.074 already existed** as `ScriptedRadio`'s
   starting state, asserted by `CwToDataAndBackTests`. The new test uses it to
   prove the dwell half rather than duplicating it.

No decision was recorded under §12.1. Nothing here was one-way: the filter-byte
question weighs two costs, and every other change is inside the order.

## 2. What Tim should expect

**Tuning across a digital block on the way somewhere else no longer changes your
mode.** Stopping in one for a second still does, and still says so. The dial has
to be genuinely still — the same frequency across a full second — not merely
inside the block.

**And a mode write the radio would not take now empties the data flag as well as
the mode badge.** If you see the variant go blank after a refused write, that is
this, and it is the honest reading rather than a bug.

Build clean, no new warnings. Pushed to `main`: `222e3a4`, `9e35866`, and
`ad1b738` from task 1. Version 1.12.7.

**Engine, Rig and Explore batches: 789 passing, 0 failing.**

**What will look wrong but is not:**

- **Tasks 2 and 3 have no work behind them.** The order's own gate closed them.
- **The filter byte is still sent.** Mismatch 2 above; it is a ruling ask, not an
  omission.
- **Task 6 has no commit.** It was already in the tree.
- **Neither full suite has a single-run result.** Both are batched, and the app
  Views batch does not come back at all. Amended below.
- **HM-OPEN-061 remains open.** The engine test host crashes on full runs, wider
  than the class the issue names. Not touched this unit.

### Amendment — the app suite, and a red test I caused

**One test went red and it was mine: `ModeFollowsTheMapAgainTests.
NothingButTheModeIsEverWritten`** — HM-DEC-149's own sweep of the follow path.
Not a write it objected to: the sweep bounds itself at 6,000 characters between
two method anchors, so that it cannot pass by sweeping the rest of the file and
finding nothing there either, and **the comment I added to `FollowTheMapAsync`
pushed the path past that bound.** The comment is trimmed to five lines pointing
at `ModeDwell`, which carries the reasoning, and the sweep is green.

**It found the right thing.** A guard that only fires on a forbidden call would
not have noticed the method growing until it had grown enough to hide one.

App suite, run in batches because a single run exceeds ten minutes:

| batch | result |
|---|---|
| ViewModels | **240 passing, 0 failing** |
| everything but ViewModels and Views | **217 passing, 0 failing** |
| Views | **did not finish inside ten minutes** |

**The Views batch has no number**, and that is the honest record rather than an
omission. It is the same shape as HM-OPEN-061 on the engine side: a full run that
does not come back. Nothing in this unit touched a view.

## 3. What we should do next

1. Rule on the filter byte, which currently sits against HM-DEC-149's text.
2. Get the eight 2026-08-29 captures into the tree. Eight units have now asked.
3. HM-OPEN-061 — the host crash makes every full-suite number in this project a
   partial one.
4. Verify the manual's p. 4-6 filter table for SSB-D, which is what the order's
   parked proposal turns on and cannot be checked from the tree.

## 4. What's blocking us

One ruling, and it is about what Hamlet writes to the radio, so §12.1 puts it
squarely with Tim.

> **The tune-in write carries a filter byte where the block states the passband it
> needs, and HM-DEC-149's "only the mode is written" is amended to say so.**
>
> HM-DEC-149 reads: *"ONLY THE MODE IS WRITTEN. Not the frequency, not the filter,
> not the power, not the gain, not the preamp or the attenuator, not as a side
> effect and not as a convenience. A sweep of the follow path asserts it."* The
> tree has sent the filter byte since `46313cf` on 2026-08-28, and the sweep that
> ruling names does not catch it.
>
> **The case for the byte is measured and the case against is a text.** On
> 2026-08-28 the operator sat correctly tuned to 14.074 with a 500 Hz window over
> a 3 kHz block and heard nothing for an hour. And **skipping the byte was never
> neutral**: the manual is explicit (p. 19-11, §4) that omitting the trailing
> bytes of a `26` selects DATA OFF and the mode's own default filter, so a byte
> went out either way and the only question was whether anybody chose it.
>
> **Rejected: removing it to match this order's task 5.** That order was written
> without knowledge of `46313cf` — it also names task 6 as a drop candidate and
> task 6 is built — and undoing a fix for a measured hour-long failure on the
> strength of that is the wrong direction.
> **Rejected: leaving it unruled.** It is the second time a session has changed
> what Hamlet writes to the radio without a ruling behind it, and HM-DEC-113 is
> the record of what that costs.
> **What this session could not settle** is the parked proposal underneath it —
> whether SSB-D's FIL2 is 1.2 kHz where SSB's is 2.4. That is a manual page, the
> manual is cited and never committed, and it cannot be checked from the tree.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140.

1. **The filter byte against HM-DEC-149** — raised above, 2026-08-29, and
   recorded as **HM-OPEN-062** so it survives the next report. Waiting on a
   ruling. The change is already in the tree at
   `src/Hamlet.App/ViewModels/MainWindowViewModel.cs`, from `46313cf`.
2. **The eight 2026-08-29 captures are not in the tree**, an eighth consecutive
   unit. Waiting on the files.
3. **The evidence term's unbounded scale** (2026-08-29, unit 049). Waiting on a
   ruling; it subsumes the confidence question units 044 to 048 each carried.
4. **The answer key's licensing**, which bounds how much truth the CW score can
   have.
5. **The mode and filter's place in the owned-settings contract** — unit 047.
6. **What the digital rows state for the five settings they are silent on** —
   unit 047.
7. **The pedestal ranking is measured at 34 of 44 and unbuilt.**
8. **A dial move's threshold is provisional at 500 Hz.**
9. **The transcript break's wording.**
10. **Whether `CwPitch` should follow an admitted station.**
11. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
12. **The `reading` line's span wording needs approval.**
13. **Two stations closer than 125 Hz are not named.**
14. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
15. **Nothing checks that deleting a surface is not deleting a capability.**
16. **The engine test host crashes** on full runs, wider than the class
    HM-OPEN-061 names. Owned by Claude, not waiting on a ruling.
