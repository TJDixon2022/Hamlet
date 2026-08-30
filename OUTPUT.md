UNIT:       051 — the guard that silenced the digital map — 2026-08-29
PHASE GOAL: phase 12 — no goal text recorded. `PROJECT_STATUS.md` carries `PHASE: 12`
            and `PROJECT_CARD.md` has no phase field, so there is nothing to quote.
UNIT GOAL:  Arriving in a digital block leaves the radio able to hear it, and where
            Hamlet cannot set something it says so instead of implying it did.
ADVANCED:   not assessable against a phase goal, because no phase goal text exists.
            Against the unit's own number: **0 of 28 digital blocks could receive a
            mode write, and now 28 of 28 can.**
NUMBER:     **digital blocks able to receive a mode write: 0 of 28 before, 28 of 28 after.**
DRIFT:      carried — **no phase goal text exists to measure drift against.**

## 1. What Claude did

**Hamlet confirmed.** All four gate checks verified against the tree before the
order was read: `CLAUDE.md` and `data/bands/us-neighborhoods.json` present,
`ANNUNCIATOR_PANEL.md` and `src/CoreHMI` absent, with `Hamlet.sln` and
`CLAUDE.md`'s own header corroborating. Branch `main`, every task committed and
pushed, every push succeeded. **Nothing in this report is evidence about the
radio.**

**Five of six tasks done. Task 6 was the named drop candidate and is dropped
whole** — the reason is below and it is not only time.

### Task 1 — the blast radius, and the diagnosis holds exactly

Walked the real map against the real `HfBands`, in a test rather than by reading:

| | count |
|---|---|
| digital rows on the map | 28 |
| **digital rows inside their band's CW segment** | **28 of 28** |
| digital rows *outside* one | **none** |
| digital blocks straddling a segment edge | **none** |
| Morse rows inside a CW segment | 20 of 20 |
| digital blocks producing a write, terminal idle | **0 of 28** |

**The order's expected answer was all of them and it is all of them.** The
straddle check is the one the order did not ask for and it matters: had any block
crossed a segment edge there would be a stretch of dial where the write did go
out, which is an intermittent fault rather than a total one, and it would have
shown up as "it worked that one time". It does not happen. **"The dial is in a
digital block" and "the guard fires" are the same statement, with no frequency
between them.**

The second half, answered from the code: **no.** `Neighborhoods` and
`IsInsideCwSegment` both read `SelectedBand.Band`, so they cannot disagree about
which band; a non-null digital `here` therefore implies `IsInsideCwSegment` is
true. There is one call site and no path around it.

### Task 2 — the map decides, and one consequence stated rather than left to be found

`workingCw` stops consulting `IsInsideCwSegment` and asks the map instead.
`IsInCwSegment` and `HfBands` are untouched (HM-DEC-110) and `ModeLineText` keeps
its one remaining use of the property.

**What I want flagged rather than buried: the map operand cannot fire the guard
on its own.** The guard is `workingCw && target.Mode != CivMode.Cw`, so where the
block is Morse the second half is already false, and where it is not the first
half is. **The protection that actually bites is `IsCopyingMorse`** — characters
arriving — which is the operand HM-DEC-149 corrected and the one 2026-08-18 turned
on. The block operand is kept because it states what the evidence *is* rather than
what survives an inlining, and it starts mattering the moment a Morse block wants
anything other than plain CW. It is written into the code as well as here.

### Task 3 — the seam is closed, not tested around

The evidence expression moved out of the view model into
`ModeFollowPlan.WorkingCw`, which the view model now calls. **The bug existed
because that expression lived on a line no test could reach**, so testing around it
would have left the next one just as invisible.

| | before | after |
|---|---|---|
| digital blocks producing a USB-D write | **0 of 28** | **28 of 28** |
| Morse blocks asking for the wrong thing | 0 of 20 | 0 of 20 |
| digital blocks writing while Morse is being copied | 0 of 28 | **0 of 28** |

**That last row is the control and it is the whole reason to trust the first.**
Without it this repair is indistinguishable from deleting the 2026-08-18
protection.

`ArrivingInADigitalBlockDoingNothingElseStillFollows` is **fixed, not deleted**,
and its remark now records how it lied: it claimed 14.074 MHz was "outside any CW
segment" and handed `workingCw: false` to match, while the running app computed
`true`. It keeps the named frequency — a sweep is not what somebody reads when
they come back asking what went wrong — but it derives the value now instead of
writing it down.

### Task 4 — what could be established is less than the order hoped

**The manual is not in this repository** (§2.1 forbids committing it), so p. 19-4
could not be re-read column-aware. What is citable *here*:

| control | status |
|---|---|
| outer Twin PBT | **`14 08`**, cited in §4's own correction note to p. 19-3 |
| inner Twin PBT | **no command recorded anywhere in this repository** |
| RIT state and offset | **no command recorded anywhere in this repository** |

The obvious guess for the inner control is the sub-command beside the outer one.
**That guess is not taken**, because it is precisely how the CW pitch landed on
the wrong row of this two-column page and cost weeks. Both are marked
`Undocumented`, which this tree already distinguishes from unknown and from
unsupported, with the reason recorded.

`PassbandReport` names the remedy — hold `TWIN PBT CLR` for a second — and
**suppresses the audible claim on uncertainty, not only on a bad reading.** Away
from centre and never read are different facts about the radio and the same fact
about what Hamlet knows. **Today that means the claim can never be made**, because
two of the three controls have no read; a test asserts that, so the day somebody
closes them is the day it gets noticed rather than the day it silently starts
claiming.

**A standing guard was narrowed and I want that seen.** `TheCwPitchReadIsSubCommandNine`
forbade *any* `14 08` read. Its stated hazard is a payload, which a read does not
carry, and its purpose is stopping the pitch coming back on the wrong row. It now
says something **stricter**: the pitch is 09, and any `14 08` read must be the Twin
PBT and nothing else — the old form would have gone green on a `14 08` read of some
third field. A separate test carries the half that was always about the real
hazard: **nothing writes `14 08`.** Changing a test to admit one's own change is
the move §12.5 warns about, so it is reported here rather than mentioned in a
commit.

### Task 5 — the reason stops being unrecoverable

Silence on the status line stays. What changes is that every refusal branch now
names itself with a **stable machine token** (§8.1, never a display string), and
rig diagnostics says three things: what the map called for, what the radio is in,
and which test declined. Unread is said as unread.

**This is the fault §8.1 was written against.** With nothing recorded anywhere,
"Hamlet refused", "Hamlet is broken" and "nobody tuned anywhere" were one picture,
and that is how a guard silencing every digital block survived from one ruling to
the next.

### Task 6 — **dropped whole**

It is the named drop candidate, and two of its three parts already exist:
`Neighborhood.WhereTheSignalsAre()` names the dial and the block it opens onto,
composed into the card at `MainWindowViewModel.cs:2495`. Missing is "and the mode
now set", and the card fires on startup and on a click rather than on arrival.

**Beyond being the drop candidate, there is a reason not to build it this
unit.** Task 4 established that Hamlet cannot verify the block is audible — two of
three passband controls have no read. Adding "the mode is now set" to an arrival
card would introduce, on the one sentence the operator acts on, exactly the claim
task 4 exists to suppress. It wants doing *after* those two reads exist, or with
`PassbandReport.CanClaimAudible` wired through it.

**No decision was recorded under §12.1.** Nothing here was one-way: the guard
narrowing weighs two costs, and §12.1 puts anything touching what the display
asserts with Tim without exception.

## 2. What Tim should expect

**Tune to 20 m FT8 and the radio goes to USB-D and says so.** That is the change.
It works in all 28 digital blocks on the map, not just that one, and it needs the
dial to come to rest for a second first (unit 050's dwell, unchanged).

**Copy Morse and it will still refuse**, everywhere, exactly as before — that is
the 2026-08-18 protection and it is asserted across all 28 blocks.

**Open rig diagnostics and there is a new line** saying why mode-follow last wrote
nothing, when it wrote nothing. It is deliberately not on the status line.

**What will look wrong but is not:**

- **Hamlet will not tell you a digital block is audible.** It cannot read the
  inner Twin PBT or RIT, so it declines to claim rather than guessing. That is
  task 4 working, not a gap.
- **The Views batch of the app suite still crashes the host.** Inherited,
  HM-OPEN-063, untouched here.
- **Task 6 has no commit.** Dropped, above.

Build clean, no new warnings. Version unchanged at 1.12.7 — flagged as a mismatch
below, since HM-DEC-150 makes a work unit a patch bump and I did not take one
without knowing whether this unit and the previous 050 count as one phase's work.

**Suites, batched because no full run of either comes back (HM-OPEN-063):**

| suite | result |
|---|---|
| engine `Explore` (incl. every new test) | **541 passing, 0 failing** |
| engine `Rig` | **276 passing, 0 failing** |
| engine `Civ` | 61 passing, 0 failing |
| engine `Bands` | 39 passing, 0 failing |
| engine `Licensing` | 61 passing, 0 failing |
| app ViewModels | **240 passing, 0 failing** |
| app, everything but ViewModels and Views | 217 passing, 0 failing |
| app Views | not run — crashes the host, inherited |
| engine `Cw` | not run — untouched by this unit, and it is where the crash lives |

## 3. What we should do next

**The number this unit was commissioned on: digital rows able to receive a mode
write, 0 of 28 before and 28 of 28 after.**

1. **Read p. 19-4 column-aware** for the inner Twin PBT and RIT. Two explicit
   unknowns are sitting in the ledger and they are what stops Hamlet ever saying a
   block is audible.
2. **Then task 6**, with `CanClaimAudible` wired through the arrival card.
3. Rule on the guard narrowing and the version bump (section 4).
4. HM-OPEN-062, the filter byte, still unruled and still parked.

## 4. What's blocking us

Two, and both are small.

> **A session may narrow a standing guard when its blanket form blocks work the
> order commissioned, provided the replacement is strictly stronger and the report
> says so.**
>
> `TheCwPitchReadIsSubCommandNine` forbade any `14 08` read at all. Task 4
> commissions exactly that read, and the guard's own stated hazard — issuing 08
> *with a payload* — is one a read cannot commit. Its purpose is stopping the CW
> pitch returning on the wrong row of a two-column page, and that purpose is
> untouched.
>
> **Rejected: leaving the guard and refusing the task.** The read is cited in this
> repository's own §4 correction note and the order asks for it.
> **Rejected: deleting the assertion.** What replaced it is stricter — any `14 08`
> read must be the Twin PBT, where the old form would have passed a `14 08` read of
> some third field — and a second test carries the real hazard, that nothing
> *writes* `14 08`.
> **What this session could not settle** is whether `14 08` is genuinely read-capable
> on this radio. §4's note says sub-command 08 is the outer Twin PBT position and
> cites p. 19-3; it does not say in so many words that the row is send/read, and
> the manual is not in the tree.

> **A work unit is a patch bump, and two orders that both called themselves 050
> are two work units or one.**
>
> HM-DEC-150 makes the minor the phase and the patch the work unit. The previous
> order took 1.12.6 to 1.12.7. This one is a separate order, separately issued,
> with its own six tasks — so by that ruling it is 1.12.8. **I did not take the
> bump**, because both orders called themselves 050 and a version that counts work
> units should not be advanced on a session's guess about what counts as one.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140.

1. **The guard narrowing** — raised above, 2026-08-29. In the tree at
   `tests/Hamlet.RadioEngine.Tests/Rig/RigStateModelTests.cs`.
2. **The version bump for this unit** — raised above, 2026-08-29. Nothing in the
   tree; `Directory.Build.props` still says 1.12.7.
3. **`N4L` against the measured pitch** — 2026-08-29, unit 050. Waiting on a
   ruling. In the tree from `efcd524`.
4. **The filter byte against HM-DEC-149** — 2026-08-29, **HM-OPEN-062**. Waiting on
   a ruling. In the tree from `46313cf`.
5. **The eight 2026-08-29 captures are not in the tree**, a ninth consecutive unit.
6. **The evidence term's unbounded scale** (unit 049). Should be re-measured
   against the new pitch before it is ruled on.
7. **The answer key's licensing**, which bounds how much truth the CW score can have.
8. **The mode and filter's place in the owned-settings contract** — unit 047.
9. **What the digital rows state for the five settings they are silent on** — unit 047.
10. **The pedestal ranking is measured at 34 of 44 and unbuilt.**
11. **A dial move's threshold is provisional at 500 Hz.**
12. **The transcript break's wording.**
13. **Whether `CwPitch` should follow an admitted station.**
14. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
15. **The `reading` line's span wording needs approval.**
16. **Two stations closer than 125 Hz are not named.**
17. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
18. **Nothing checks that deleting a surface is not deleting a capability.**
19. **The test host crashes**, in the app suite as well as the engine —
    **HM-OPEN-063**. Owned by Claude, not waiting on a ruling.
20. **`PROJECT_CARD.md` has no phase field**, so no phase goal text exists to
    measure `ADVANCED` or `DRIFT` against. Raised by this order's own header.
