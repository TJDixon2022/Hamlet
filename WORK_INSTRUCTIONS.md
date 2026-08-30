```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      CLAUDE.md
  MUST EXIST:      data/bands/us-neighborhoods.json
  MUST NOT EXIST:  ANNUNCIATOR_PANEL.md
  MUST NOT EXIST:  src/CoreHMI

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

# Why this unit exists

**Mode-follow has never once fired in a digital block, and it cannot.** Tim went
to 20 m FT8 on 2026-08-29, on a build where unit 050's dwell work was complete
and passing, and the radio stayed in CW. Nothing was written and nothing was
said.

`MainWindowViewModel.cs:5803`:

```csharp
var workingCw = IsInsideCwSegment || IsCopyingMorse;
```

which feeds the guard at `ModeFollowPlan.Decide`, line 286:

```csharp
if (workingCw && target.Mode != CivMode.Cw)
{
    return ModeFollowDecision.Nothing;
}
```

**`IsInsideCwSegment` is true across every digital watering hole in the map, by
construction.** `HfBands` builds each band's `CwLowHz..CwHighHz` from the
emission ranges carrying `TransmitMode.Data` in 47 CFR 97.305(c), and says so:
*the data ranges are what mark the bottom of a band off from the phone segment
above it.* A "CW segment" in this tree is the CW **and data** segment. It is the
same stretch of band the digital blocks live in — that is what they are.

So for all 28 digital rows on the map, the target is USB-D, the target is not CW,
`workingCw` is true on the segment test alone whether or not anything is
decoding, and the decision is `Nothing`. **Silently**, by design: *"It is silence
rather than a refusal with a sentence."*

**The map already draws this distinction perfectly and is not being asked.** The
operator sees orange and hatched orange for Morse, purple for data. The file
carries 79 rows: 20 Morse (`CW`, `CW DX`, `QRP`) and 28 digital (`FT8`, `FT4`,
`JS8`, `PSK31`, `RTTY`), each cited. `ModeFollowPlan.TargetFor` reads those very
labels. Three lines above 5803 the block the dial is in is already in hand as
`here`. **The guard asks the band plan a question the map answers better**, and
the band plan cannot tell orange from purple because under 97.305(c) they are the
same segment.

```
PHASE GOAL:   phase 12 — no goal text recorded. PROJECT_STATUS.md carries
              PHASE: 12 and PROJECT_CARD.md has no phase field, so there is
              nothing to quote. See section 4 of the delivery message.
UNIT GOAL:    Arriving in a digital block leaves the radio able to hear it —
              and where Hamlet cannot set something, it says so instead of
              implying it did.
ADVANCES:     not assessable — no phase goal text exists to advance.
```

---

# Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report any mismatch.
Unit 050 found four, all real, and reporting them rather than repairing the order
is why this unit exists at all.

Read from the tree by the web session and believed accurate: `MainWindowViewModel.cs:5803`;
`ModeFollowPlan.Decide` line 286; `CwBand.IsInCwSegment` and the 97.305(c)
derivation in `HfBands`; the 79 rows and their labels; and
`NothingTakesHimOutOfCwTests` line 76.

**Not read, and to be established rather than assumed:** anything about the
Twin PBT or RIT command set; whether `RigField` has entries for either.

Report mismatches; do not repair the instruction.

---

# Rulings in force

## HM-DEC-056, as amended by HM-DEC-149 — in force, do not re-argue

The operator's own hand always wins. A mode change Hamlet did not make suspends
the automation until the next band change, and suspended is a visible state
rather than a silent one. Nothing is assumed from having sent a write: the radio
acknowledges or refuses, and anything else leaves the value UNKNOWN. Every write
Hamlet makes on its own initiative is narrated. A flip waits for the dial to
settle.

**The guard at line 286 is correct and stays.** On 2026-08-18 mode-follow wrote
USB-D repeatedly while the operator sat on CW main street with a signal decoding,
and the send controls refused `not_in_morse` for sixty-six seconds — he could not
answer a station because the app had moved his radio out from under him. **What
is wrong is one of the two things feeding it, not the rule.** `IsCopyingMorse`
was already corrected once for exactly this reason, when `IsDecoding` was found
to mean "the decoder is switched on" rather than "somebody is sending". That fix
repaired one operand and left the other.

## HM-DEC-054 — in force, do not re-argue

The neighborhood map lives in `data/bands/us-neighborhoods.json` with a source on
every row. Blocks published as a single dial frequency run to the next one or
three kilohertz, whichever comes first, because these modes are worked in upper
sideband with audio to about three kilohertz.

## HM-DEC-110 — in force, and it is why task 2 is narrow

**The neighborhood file is not the source for the CW segments and must not become
one.** Its Morse rows fall short at the top of every band and leave a hole on
40 m between 7.040 and 7.050. A CW segment is a regulatory boundary; the
privileges file is where regulation lives.

**So `IsInCwSegment` is not wrong and is not being changed.** It is a true
statement about regulation, it is correctly derived, and `ModeLineText` should go
on using it. It is simply not evidence about what the operator is *doing*, which
is the only thing the guard at 286 wants to know.

## HM-OPEN-062 — open, unruled, and out of scope

The filter byte has been sent since `46313cf` and HM-DEC-149's text says only the
mode is written. **Tim has not ruled. Do not add, remove or alter the filter
byte in this unit, and do not re-argue it.** Task 4 reports what the radio says
about the filter; it changes nothing about what is written.

---

# Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` — `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and
`NOTE` saying what is moving inside the task. Same every ten minutes while a task
runs.

---

# Tasks

## Task 1 — measure the blast radius

**Nothing is built until this reports.**

Walk the real map against the real `HfBands` and count: **of the map's digital
rows, how many sit inside their band's CW segment?** The expected answer is all
of them, which would mean mode-follow has been inoperative in digital territory
since the guard landed. Report the count as `n of 28` and name any row that is
*not* inside one, because an exception would be interesting.

Then answer from the code, not from this order: has any digital-block write ever
been possible through `MainWindowViewModel`? Check whether any path reaches
`ModeFollowPlan.Decide` with `workingCw` false while the dial is in a digital
block.

**The before-number this unit is judged on is that count.** If it comes back 0 of
28 blocked, the diagnosis here is wrong — stop, report, and do not build tasks 2
and 3.

## Task 2 — the map decides what he is doing, not the band plan

`workingCw` stops consulting `IsInsideCwSegment`. The evidence that the operator
is working Morse becomes:

- `IsCopyingMorse` — characters actually arriving, which stays exactly as it is;
  and
- **the block the dial is in being a Morse block**, which the map already says
  and which `TargetFor(here)` already computes.

`IsInsideCwSegment` stays where it is used for `ModeLineText` (HM-DEC-055 —
one derivation behind the map, the card and the line). It loses no other caller.

State the reason inline, because a rule without one gets talked out of: a
regulatory segment cannot distinguish Morse from data, since by 97.305(c) they
share it. The map can, it does, and the operator can see it in the colours.

## Task 3 — close the seam the tests were on the wrong side of

`NothingTakesHimOutOfCwTests.ArrivingInADigitalBlockDoingNothingElseStillFollows`
calls `Decide` at `14_074_000` with `workingCw: false` supplied by hand. **In the
running app at that frequency it is `true`.** The test asserts a state the app
cannot reach, passes, and the radio stays in CW. Every other mode-follow test
does the same — including the ones in `Hamlet.App.Tests`, which call `Decide`
directly and never cross line 5803.

Build a test **at the view-model seam**, driving frequency the way the app does
and asserting on what would be written:

- every one of the 28 digital rows: a matured dwell produces a USB-D write;
- every one of the 20 Morse rows: a CW target, and no data write;
- **the control**: the terminal actively copying inside a digital block still
  refuses, so the fix has not simply deleted the 2026-08-18 protection.

Fix the misleading `workingCw: false` at line 76 or delete it in favour of the
seam test, and say which.

## Task 4 — what Hamlet cannot write, it reports

This is the half of "the data settings are set" that has no write behind it, and
it must not be papered over.

Establish first, from the manual and the command table, what can be **read**:
the Twin PBT position, and RIT state and offset. `CLAUDE.md` records `14 08` as
the outer Twin PBT — **the row once confused with the CW pitch, so re-read 19-4
column-aware.** Whether the inner control has a companion sub-command is unknown
and must be established, not assumed centred. If a read does not exist, that is
an explicit unknown in the ledger and the task reports it as one.

Where either is away from neutral, Hamlet says so in the app's voice and names
the remedy — holding `TWIN PBT CLR` for one second until the dot beside the
width disappears (p. 4-5) — and **suppresses any claim that the block is now
audible.** Saying the radio is ready while a hand-set PBT closes the window is
the prime directive broken on the one sentence the operator acts on.

**No writes.** There is no PBT write and RIT is not this unit's to touch.

## Task 5 — a refusal nobody can see is how this lasted

The guard's silence was defensible and it cost weeks. Silence on the status line
stays — a commentary on writes that nearly happened is noise on the one line the
operator reads. **But the reason belongs in rig diagnostics**, where somebody
looking for why nothing happened can find it: what the map called for, what the
radio is in, and which test declined.

## Task 6 — the arrival card says what the radio can hear — DROP CANDIDATE

**Named as the drop candidate. Dropped whole, and say that you dropped it.**

On arriving in a digital block, the card names the dial, the block it opens onto,
and the mode now set — so that **dead at the published frequency and alive one
kilohertz up reads as a correctly tuned radio** rather than an empty band. Check
first whether `Neighborhood.WhereTheSignalsAre()` already covers this; unit 050
reported it built, and if it does, this task is a no-op and says so.

---

# Parked — do not touch, do not raise

- **The filter byte.** HM-OPEN-062, awaiting Tim.
- **The eight 2026-08-29 captures.** Ninth consecutive unit; the ask carries.
- **HM-OPEN-061**, the engine test host crash.
- **The decoder.** Nothing here is evidence about it.

---

# What not to do

- **Do not change `IsInCwSegment` or `HfBands`.** HM-DEC-110 rules the segments
  regulatory and the neighborhood file explicitly not their source. This unit
  changes who *asks*, not what it answers.
- **Do not touch the filter byte.** Unruled.
- **Do not remove the guard at line 286.** Its evidence is wrong, not its rule.
- **No transmit work.** §0.2 untouched.
- **Do not commit the IC-7300 manual.**

---

# Committing, pushing, reporting

Commit and push each task before starting the next. Name the branch and say
whether the push succeeded; a refused push is reported as refused.

Write `output.md` per `CLAUDE_CODE.md` §8. **Section 3 leads with the number this
unit was commissioned on: digital rows able to receive a mode write, before and
after.**

Carry `DRIFT` forward and say on the line that no phase goal text exists to
measure it against.

**Every exit writes the report.** If you stop with tasks remaining, name them and
say whether what you dropped was task 6.

Then stop. Do not start the next unit.
