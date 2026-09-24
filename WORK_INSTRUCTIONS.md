# Work instruction 417 - tonePeak is about the recording it sits beside

**Seed under `--seed`.** Step 3 has every criterion ticked that can be ticked: two spacing
changes kept, all keyed recordings 217 edits to 167 over 565 against inferred keys, no letter
moved. Its last criterion, 3.4, fires only after three units in a row keep nothing, and the
last unit kept two. **So the loop moves to the screen, as R64 and R65 prefer when the spacing
has nothing open to work.** This unit makes the capture sidecar's `tonePeak` a figure measured
over that capture's own audio, labeled as such, as R63 ruled. Four tasks, drop from the back.

**Status.** `sh tools/status.sh`, real clock, after every commit and every task, and
immediately before every `dotnet test`. **Write files as UTF-8.**

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite. Only this unit's names and `docs\carry-forward-tests.txt`, run as
its top comment says. **Never background and poll.** One type per invocation, each with its
own `timeout`. The captures type is 51 rows; give it 600 s.

**The report's four top-level headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`.

**Nothing in section 4 halts this phase** (R65). A question is parked in
`docs\phase-correctness\PARKED.md` and the loop goes on. **Do not carry an ask forward as
blocking** unless 6.7 itself cannot be met without it.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt. Multi-step commands
go into `.run-unit\unit417-<name>.sh` and run with `sh`. Unit 416's scripts can be copied.

## 3. Asks still outstanding

None carried. **Unit 416's P9 is answered by this instruction** - see section 4 - and stays in
`PARKED.md` with the answer appended beneath it, one paragraph, marked the arbiter's,
overrulable.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  The capture sidecar's tonePeak line states a figure measured
            over that capture's own audio and says so, watched failing
            first against the held-and-decaying figure it prints today,
            with what it costs at the moment of capture measured.
ADVANCES:   step 6 criterion 7
DRIFT:      0
```

**The count today.** Steps 0, 1 and 2 done. Step 3 partial: 3.1, 3.2, 3.3, 3.5 met, 3.4 open.
Step 4 not started. Step 5 the owner's. Step 6 partial: 6.1 and 6.6 met; 6.2, 6.3, 6.4, 6.5,
6.7 and 6.8 open.

**Why not step 3.** 3.4 reads *after three consecutive units with no kept change*. Unit 416
kept two, so the count stands at zero and no unit can honestly flip 3.4 now; a step 3 unit
that kept a third change would advance nothing the launcher counts, and one that kept nothing
would be the first of three. **Author's, overrulable (P9's answer): step 3 stays partial and is
not marked done**, because 3.4 is unmet and a closed step is closed for good. What is left on
17:37 is letters, not spaces - `CQ CQ CQ DEWTEETEEERE D ETTTB 7E E I` - which is P6's wall, and
it is logged there, not chased here.

**Why 6.7 of the six screen criteria.** R63 has already ruled what the figure is - *measured
over the recording the sidecar is about, labeled as such* - so the unit needs no ruling; unit
411 dropped it only because that ruling did not yet exist. And 6.2 is open **on `tonePeak`
alone** (unit 411's exit: `elementHz` and `keying` repaired, watched failing first), so the same
work should close 6.2 as well. 6.3, 6.4 and 6.5 stay for the next units.

**What the tree says today**, `src\Hamlet.App\ViewModels\MainWindowViewModel.cs`:

- `TonePeakRecordLine(CwDecodeReport report)`, near line 12373, prints `report.SnrDb` with the
  caption *the highest the tracked tone ever stood above the noise beside it, held and
  decaying; not a figure about this recording*. It is honest and useless: the one number on
  the sheet about the signal's strength is about something else.
- The comment above its call, near line 11790, records why: the held figure rates
  `cw-2026-08-20-014854` at 41.7 and `-014935` at 38.4, neither holding keying at any pitch,
  above `cw-2026-08-17-013347` at 34.7, the one the decoder reads a callsign from. **A work
  order was written from that reading.**
- The line below it, `inThis`, is already derived from the audio in the file
  (`InThisRecording(audio, samplesSeen)`), so the writer has the recording's samples in hand.

---

## 5. Verify this instruction against the tree

Check, report any mismatch, repair nothing:

- `TonePeakRecordLine` exists as quoted and is the only place the sidecar's `tonePeak` line is
  composed; `TheSidecarIsReReadTests` asserts the line starts `tonePeak   `.
- `CwDecodeReport.SnrDb` is the held-and-decaying figure, set from `CwDecoder._lastSnrDb`.
- `CwCaseRoster`'s `tonePeakDb` column reads `SnrDb` from the report, **not** from the sidecar,
  so nothing parses the sidecar line. If anything does, say so before changing the line.
- The sidecar writer has the capture's audio in hand at the point it composes the line.
- The keyed totals at HEAD are 167 over 565, the ten 35 over 156, 17:37 19 over 25.

---

## 6. Rulings in force - do not re-argue

`PHASE_PLAN.md` R59 to R66, §3 and §6. The ones this unit leans on, in full:

**R63, Tim, 2026-09-24, on `tonePeak`:** *ruled (a): a figure measured over the recording the
sidecar is about, labeled as such. HM-DEC-091 protects the held peak other things were built
on; it does not require the per-capture sheet to print that particular figure. Rejected: not
printing it in the sidecar; leaving it with its contradicting caption.*

**§6: Step 6 changes what the operator reads, never what the radio does.** A screen criterion
is met by making a sentence true, never by deleting the sentence and saying nothing, and never
by changing a radio setting to match a claim.

**HM-DEC-091**: `CwDecodeReport.SnrDb`, the held peak, is not deleted and not changed. The
roster's `tonePeakDb` column stays as it is.

**R65** nothing carried halts the loop. **CLAUDE.md §0.0** never state as known what is not
known: where the figure cannot be measured - no tone tracked, too little audio - the line says
so in words and prints no number. **§0.2** nothing that keys or transmits is touched.
**HM-DEC-155**, **HM-DEC-165**, **FACT-004**.

No decision record is needed: R63 is already the owner's.

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

`PHASE_OUTCOME.md` gets its `## UNIT 417 - STEP 6` entry from the decision block at the foot
of this file. `PHASE_STATUS.md` names unit 417 and `CURRENT_STEP: 6`. Patch-bump
`Directory.Build.props`. P9's answer appended in `PARKED.md`. **Entry round:** both
carry-forward lines, the three floor tests, `TheSidecarDoesNotContradictItselfTests` and
`TheSidecarIsReReadTests`, recorded as the numbers to beat.

**Drop candidate:** none.

### Task 1 - the trace

A fact asserting nothing, in `tests\Hamlet.App.Tests\Cw`, that replays saved captures through
the writer's own code and prints, per capture, the `tonePeak` line as the sheet writes it today
beside a tone-over-noise figure measured over that WAV alone. At least these four:
`cw-2026-08-20-014854`, `-014935`, `cw-2026-08-17-013347`, and `cw-2026-09-23-173723`.

**How the figure is measured is yours** and goes in DECIDED in your report - the tone's peak
above the noise beside it, in dB, the same kind of quantity the held figure is, over this file's
samples only, at the pitch the decoder tracked. State the window, the noise estimate and what
happens with no tracked pitch. **Choose the method from what it is meant to measure, never from
which ordering of the four it produces.** Print the ordering it gives; do not predict it here.

Also print, per capture, the wall time the measurement costs on that file's length.

**Drop candidate:** none - the build in task 2 is chosen from this.

### Task 2 - the line (6.7, 6.2)

Write the test first, and watch it fail against the current line:
`TheTonePeakIsAboutThisRecordingTests` - on at least two saved captures, the sidecar's
`tonePeak` number equals the figure measured independently over that capture's audio, and its
caption says it is a figure over this recording. **It goes red today because the line prints
the held figure**; quote the red.

Then change `TonePeakRecordLine` and its caller so the line carries the figure measured over
the recording, labeled as such. Where it cannot be measured the line says so in words. The held
figure may be dropped from the sheet or kept on its own line under its own honest caption -
yours, and say which. `CwDecodeReport.SnrDb` does not change.

**The cost at the moment of capture** (6.7's last clause): measure the time the new figure adds
to writing one capture at the longest capture length in the tree, say which thread it runs on,
and state both in the report. If it runs on the UI thread and costs more than 50 ms, move it off
and say so.

Update `TheSidecarIsReReadTests` only where the line's wording forces it, and say what changed.
Regenerate `.run-unit\unit417-sidecar-*.txt` for 17:37 and 014113 as 411 did, and print the old
and new `tonePeak` lines side by side. **No capture sidecar in the tree is edited.**

Tick 6.7. **Tick 6.2 only if** its three clauses now hold on the regenerated sheets - `tonePeak`
about this recording, `elementHz` and `keying` as 411 left them - and say so line by line.

**Drop candidate:** the move off the UI thread, if the cost is under 50 ms it is not needed; if
it is over and the clock is short, report the number and leave it.

### Task 3 - the exit round (6.6 holds)

Hamlet.sln builds with warnings as errors. Both carry-forward lines, the three floor tests with
captures at 51, `TheSidecarDoesNotContradictItselfTests`, `TheSidecarIsReReadTests`,
`CaseRosterSurvivesAnEveningTests`, the new tests, and every type touched. The keyed totals
unchanged at 167 over 565. `src\Hamlet.RadioEngine\Cw` prints nothing against entry; the
transmit files print nothing against `7e209cb4`.

---

## 9. Parked - do not touch, do not raise

- **Step 3** and P6, P7, P8, P9. The spacing stays as unit 416 left it.
- **Step 4, the pitch judge**, and the roster's `tonePeakDb` column.
- **6.3 the reflow, 6.4 hover text, 6.5 the dead button, 6.8 the RF gain scale.** The next units'.
- **The acquisition failure** - the first two minutes of the 7.052 session.
- **Any key, scored region or floor.** Fixed.

## 10. What not to do

- **Do not change `CwDecodeReport.SnrDb`, `CwDecoder` or anything in `src\Hamlet.RadioEngine\Cw`.**
- **Do not delete the `tonePeak` line** or leave it saying nothing; R63 rejected both.
- **Do not choose the measurement from the ordering it produces** on the four captures.
- **Do not edit a sidecar already in the tree.**
- **Do not touch what keys, transmits, or writes to the radio.**
- **Do not halt for a question.** Park it.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing. American spelling. UTF-8. The four headings exactly.**

## 11. Committing and pushing

Commit per task. The failing test in task 2 is committed red on its own before the change,
with the red quoted in the message. Push at the end and say whether it succeeded.

---

## 12. Reporting

`output.md` at the root, the four headings exactly as section 1 gives them.

```
READ IN THIS ORDER.

A. Hamlet reads a CQ call correctly. Steps 0, 1, 2 done; 3 partial,
   3.4 only open and not flippable this unit; 4 not started; 5 the
   owner's; 6 partial.
B. Step 6: 6.7 met or not, with the red quoted and the cost at capture
   in ms and its thread; 6.2 met or not, clause by clause; 6.6 held.
C. The rest. Section 4 raises <n> items; <none | which> in the way of
   6.7 or 6.2.
```

```
UNIT:       417 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     tonePeak on 17:37 <held figure> -> <recording figure> dB; capture cost <n> ms
DRIFT:      <0 if a criterion moved>
```

**Section 3 leads with** the old and new `tonePeak` lines for 17:37 and 014113, one above the
other, then the four-capture table from task 1: held figure, recording figure, whether the
capture holds keying.

**Section 2 tells the owner in one paragraph** that the strength figure on each capture's sheet
is now about that capture, what it said for 17:37 before and after, and what it costs.

---

```
ARBITER-DECISION
STEP: 6
APPROACH: measure tonePeak over the capture's own recording and label it in the sidecar, watched failing first against the held-and-decaying figure, capture cost measured
MOVE: continue
WHY: PHASE_PLAN.md step 6 criterion 6.7 asks that tonePeak in a per-capture sidecar be a figure measured over that recording and labeled as such, watched failing first against the held figure, with the cost at capture stated; step 3's only open criterion, 3.4, cannot flip after unit 416 kept two changes, so under R64 and R65 the loop moves to the screen.
STATE: partial
DECIDED: author's, overrulable - P9 answered: step 3 stays partial and is not marked done because 3.4 is unmet and cannot fire while the no-kept-change count is zero; 6.7 chosen over 6.3, 6.4 and 6.5 because R63 already rules it and 6.2 is open on tonePeak alone; the measurement method, whether the held figure stays on its own line, the 50 ms UI-thread bound and the per-type timeouts are the unit's to decide and report
LICENCE: PHASE_PLAN.md R63, R64, R65, section 6; step 6 criteria 6.2 and 6.7; step 3 criterion 3.4; HM-DEC-091; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0 and 0.2
ACCOMPLISHED: the strength figure on every capture's sheet is about that capture, so the next work order is not written from a number that rates an empty recording above a callsign
ADVANCES: step 6 criterion 7
END-ARBITER-DECISION
```
