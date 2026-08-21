# WORK_INSTRUCTIONS.md

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      Hamlet.sln
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticStream.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  src\CoreHMI

These four files are fixed. Do not substitute a different file for any of
them and do not report a check against a file this list does not name.

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## Why this unit exists

**Removing the old decoder broke HM-DEC-120. The app invents text right now.**

The sweep went from 0.00 invented at every level to 0.11 at eighteen decibels and
0.25 at zero. Three of the four recordings read worse. `003758` went from
`KIS QRL TU ... AA4MP/4 QNIK` to `E URL TS EHEIISEIA■IH/5■IS`.

**The cause is diagnosed and the previous session's diagnosis is trusted.**
`CwToneTracker.MidCharacter` is HM-DEC-096 phase 3's interlock: the tracker may not
jump elsewhere in the band while a character is part-read, because the rest of that
character then gets assembled from a different station. **The removed gate fed it
from the elements it had in flight. Nothing feeds it now.** Setting it to a
constant `true` reproduces the old table exactly — 1.00 and 0.00 at eighteen
decibels — which is the proof that this and nothing else is the difference.

**The previous session was right not to invent a replacement**, and right that
`_tracker.HasKeying` is the wrong instrument — its verdict takes three seconds to
form and the damage happens inside them.

### Ruled by Tim

**The probabilistic decoder supplies it.** It has a Viterbi path with element
state, and whether the current position sits inside a key-down, an inter-element
gap or a word gap **is** the question the interlock asks. That is a truer signal
than the removed gate's elements-in-flight ever was.

**And a constant `true` ships first, tonight, as a stopgap.** It reproduces
yesterday's numbers exactly and costs every retune, which is tolerable for an
evening of hand tuning and not tolerable permanently. **It comes out in task 3 and
does not survive this unit.**

*Rejected: reverting the removal.* It puts back the two-decoder trap that cost a
day.

*Rejected: leaving the interlock unfed while the answer is designed.* The app
invents text in the meantime.

---

## Verify this instruction against the tree

- **Report mismatches; do not repair the instruction silently.**
- **Rulings below are cited by number only. Read each and apply what it says, not
  what this order says it says.** The previous order got two wrong — HM-DEC-146
  named as `ShortestVote` when it is HM-DEC-119's mark-length figures, and §12.2
  named as the no-radio rule when it is the report's four headings. **If a ruling
  does not support what this order needs, report it and stop.**
- **The failing set is 30.** Twenty-two predate the removal; eight are new and the
  previous report attributes six of them to this interlock and two to the
  reacquiring guard. **Record the exact set before and after and name every
  difference.**
- **A revert during the previous session took `CwDecoder.cs` back to the old
  decoder and it was reconstructed from that session's own record.** **Before
  anything else, confirm the file on `main` is the probabilistic host and not a
  reconstruction with something missing.** Say what you checked.

---

## Rulings in force

- **HM-DEC-120.** Nothing is emitted on audio holding no signal. **This unit
  exists because it is currently broken. It is the pass/fail.**
- **HM-DEC-096**, whose phase 3 the interlock belongs to. **Read it before
  building; it says what the interlock is for.**
- **HM-DEC-091.**
- **HM-DEC-150**, the version scheme. Task 5.
- **HM-DEC-093** and `SHACK_FACTS.md` — no radio on the development machine.

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md`
**§13**, which names that file's fields — `STATE`, `PHASE`, `BALL`, `NEXT_PASTE`,
`UPDATED`, `NOTE`. `UPDATED` from the clock; `NOTE` says what is moving inside the
task. Also every ten minutes while a task runs.

---

## Task 1 — The stopgap, first, and on its own

`MidCharacter` returns `true`. One line.

**Report the sweep and all six recordings immediately.** The expectation, from the
previous session's measurement, is the old table exactly: 0.00 invented at every
level, both empty recordings silent, and the four station recordings back to their
previous text.

**If that is not what happens, stop and report.** The diagnosis would then be
incomplete and the rest of this unit is aimed at the wrong thing.

**Commit this on its own** so it can be reached without the rest of the unit.

---

## Task 2 — Ask the decoder where it is

`CwProbabilisticStream` reports whether the most likely path currently sits inside
a character — a key-down or an inter-element gap — rather than between characters.

- **Report what state the path actually carries** and how far behind the newest
  audio it is known. The stream settles a second late; **the interlock needs an
  answer about now, not about a second ago.** If the settled path cannot answer
  for the present moment, say what can — the provisional tip is still a path.
- **If the decoder genuinely cannot answer for the present moment, stop and
  report.** Do not substitute something that nearly answers it.

---

## Task 3 — Feed the interlock from it, and take the constant out

The tracker's interlock reads the decoder's answer. **The constant from task 1 is
removed in this task and does not survive the unit.**

- **Prove the sweep is still clean** — 0.00 invented at every level.
- **Prove a legitimate retune still happens.** The whole reason a constant `true`
  is not the answer is that it blocks every move to another station. **Name the
  test that shows a retune between characters still works.**
- The six tests the previous report attributes to this interlock —
  `CwSensitivityTests` ×2, `CwAcquisitionWindowTests` ×3,
  `CwAdjudicationTests.ClearingTheScreen…`, `MostRealRecordingsSitInTheWidestWindow`
  — **should go green. Report each by name and say whether it did.**

---

## Task 4 — Prove nothing else moved

- All four station recordings at or above their previous text, quoted verbatim
  against the previous report's strings.
- Both empty recordings silent, offline and streamed.
- The sweep, every level.
- **The failing set, exactly, with every survivor named.**

**If any recording reads worse than the strings in this order, stop and report.**

---

## Task 5 — Bump the version

Read the current version from `Directory.Build.props`, bump the patch, report what
it moved from and to. **HM-DEC-150.** One work unit, one patch.

---

## Parked — do not touch, do not raise

- **The reacquiring guard**, the two remaining new failures. Real, on the safe side
  — a speed withheld rather than a wrong one shown — and its own unit.
- **The twenty-two failures that predate the removal.**
- **`HM-OPEN-051` recorded open while HM-DEC-143 says it closes it.** Named by the
  previous session, left per §12.6.
- **The sidecar's `text` excluding the leading edge.** Tim's.
- **The captures from the 20th and 21st are not in the tree.**
- **Mode-follow's thirty-second guard**, `RfGain`, the likelihood gate at 15.0, the
  keying meter's thresholds.
- **HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098, HM-OPEN-033,
  HM-OPEN-007.**

---

## Asks still outstanding

Carried inbound per HM-DEC-139. **Verify against `OPEN_ISSUES.md` and report any
ask here that is closed, or open and missing.**

- Whether the sidecar's `text` should include the leading edge.
- The captures from the evenings of the 20th and 21st are not in the tree.
- Thirty seconds since the last character, for mode-follow's guard.
- Whether `RfGain`'s hundred per cent is a defect or the right answer.
- The likelihood gate at 15.0.
- The keying meter's provisional thresholds.
- HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098, HM-OPEN-033,
  HM-OPEN-007.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch and
it is `main`, **and every session commits and pushes to it**; no interactive or
destructive git; do not invent a ruling id; do not touch coverage thresholds.

Unit-specific:

- **Do not leave the constant in.** *It blocks every retune and that is why it is
  a stopgap and not an answer.*
- **Do not use `HasKeying` or any three-second verdict.** *Measured, does not help,
  the damage happens inside the delay.*
- **Do not weaken a test to make it pass.** *Delete it if its subject is gone and
  say so; otherwise make it pass honestly.*
- **Do not touch the tone tracker's own following, the survey, or the keying
  meter.** *Only what feeds the interlock.*
- **Do not improve the decoder.** *This unit restores a property.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. **§12.2 names the four
headings** — **What Claude did**, **What Tim should expect**, **What we should do
next**, **What's blocking us** — the last carrying **Asks still outstanding** per
HM-DEC-139. No other headings.

**Section 1 opens with the sweep after task 1** — whether the constant reproduced
the old table.

**Section 2 states in one sentence whether the app still invents text.**

**Stop and report.**
