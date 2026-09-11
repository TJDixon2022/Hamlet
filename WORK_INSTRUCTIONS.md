# Work instruction 312 - PSK31 exists as a mode

**This is the seed unit of the PSK31 phase.** It is step 0 of `PHASE_PLAN.md`. Its
tasks may be replaced by the arbiter's own; its gate is the project's and is handed
forward.

---

## 0. The project gate

**This instruction is for Hamlet and nothing else.** Confirm the tree:

| check | expected |
| --- | --- |
| `SHACK_FACTS.md` | exists at the root |
| `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` | exists |
| `CoreHMI.sln` | **must not exist** |
| `MURC.sln` | **must not exist** |
| root | `C:\Source\HamLet` |

The extraction gate beside the zip already ran these four. **If any is wrong now, stop
and say so in `output.md` section 4. Write nothing else.**

---

## 1. The two rules that killed sessions

Both HM-DEC-155, Tim, 2026-09-05.

**1. A unit runs no test suite.** Not `dotnet test`, not a whole project, not a whole
type unless the unit wrote the whole type. **Only this unit's own test names, filtered by
exact name, foregrounded, with a stated timeout:**

```
timeout 480 dotnet test <project> --filter "FullyQualifiedName~TypeName.MethodName"
```

Known reds you did not cause are in section 10. **An unfiltered run finds them, spends
twenty minutes, and tells you nothing about your work.**

**2. Never background a command and poll it.** No `&`, no `start`, no `nohup`, no loop
that sleeps and checks. **The watchdog fires at twelve minutes with no status write.** If
something genuinely needs longer than 480 seconds, that is a finding for section 4.

---

## 2. The tool fact

**The shell here breaks on an apostrophe inside a quoted heredoc, and it collapses a
doubled backslash.** Write *do not* rather than `don't` inside a quoted heredoc. Write
single backslashes in paths, or forward slashes. **Check what landed on disk rather than
what you typed.**

---

## 3. Asks still outstanding

Carried per HM-DEC-139 into the new phase. **All of these come back in `output.md`
section 4, verbatim where unresolved.** Do not answer them yourself; do not delete one
because it looks stale.

1. **Does the transmission record ask the radio whether it keyed?** Unit 303's proposal,
   still Tim's: `Played` stays a statement about what the audio path did; a second,
   separate fact says what the radio did, read from `1C 00` and `15 11`, which Hamlet
   already polls four times a second; **unknown** where the radio does not answer.
   **PSK31 sharpens it**: a continuous carrier that did not key is a long silence, not a
   missed slot.
2. **Nothing in this repository can look at a picture.** Real pixels want
   `Avalonia.Headless.Skia`, and **a package is Tim's, not a session's** (§0.4).
3. **Three inherited reds, never chased.** Two in `TheAchievementsScreenTests` -
   `WsprIsNotAFirstAnybodyCanEarnAndTheCardSaysSo` and `TheWindowDrawsEverySixRows` -
   and one in `TheFitGuardAsksAboutTheGridTheSendIsOnTests`, in the engine test project.
4. **Where the explanatory hover wording lives, if anywhere.** `Ft8ContactCard.Closing`
   is uncalled and left standing.
5. **`Ft8GlobePlot`'s unused framing constants.** Report; leave standing.
6. **The licence of `assets/world-flat-relief.png` is unknown.** See
   `assets/PROVENANCE.md`. Raise; do not resolve.
7. **The door sentence is a placeholder.** Carry it.
8. **Acknowledgement indicators.** Named by Tim, not yet defined. **`PHASE_PLAN.md` §3.1
   builds a turn indicator in step 4, which may be what he meant. Do not assume it is.**
9. **Card ordering under scroll.** Raised twice, unruled.
10. **The popup's size** - 720 by 400, a number unit 310 chose.
11. **Unit 311 may or may not have run.** Its zip was delivered on 2026-09-11 - the zoom
    cap and the For You scroll. Task 1 checks; if it did not run, **report it and do not
    fold it in**.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet works PSK31 the way it works FT8.
UNIT GOAL:  Step 0 - PSK31 exists as a mode. Pressing it tunes USB-D to 14.070
            and shows an empty panel that names itself. Nothing decodes.
ADVANCES:   Step 0, wholly. This is the first unit of the phase.
DRIFT:      0. A new phase starts the count.
```

**Read `PHASE_PLAN.md` at the root in full before anything else.** Its step list is the
phase; its §1 is the one fact everything is built on; its §R is the set of rulings the
author made on Tim's behalf, which a session obeys and does not revisit.

**Why the seam comes first.** The FT4 phase found that every shared surface - the panel,
the list, the ledger, the cards, the achievements - had to be told a mode existed before
any of them could be asked to do anything for it. Step 0 does only that telling. It is
deliberately the smallest step in the plan so that steps 1 to 5 land on a mode that is
already wired everywhere rather than one that is wired as each step reaches it.

---

## 5. Verify this instruction against the tree

**The author has not seen the tree since unit 310's report.** Check everything named and
**report every mismatch; do not repair this instruction; do not stop over a mismatch**
unless a task is impossible, in which case say which and why.

- **`PHASE_PLAN.md`, `PHASE_OUTCOME.md` and `PHASE_STATUS.md` at the root are the PSK31
  phase's**, delivered 2026-09-11 with seven steps all `not started`. If the FT4 phase's
  files are still there instead, **stop and report** - the phase delivery did not land.
- The mode palette - `ModePalette`, HM-DEC-032 - and `CLAUDE.md`'s family table, which
  already lists PSK31 under Digital.
- `data/bands/` - the cited band map, which already carries PSK31 at 14.070 (HM-DEC-054).
  **Read the row; report what it says the activity centre and the bandwidth are.**
- How FT4 was added as a mode alongside FT8: the tab, the mode enumeration, the log's
  mode field, the telemetry mode field, the readiness hover, the panel. **This unit
  copies that shape exactly** and reports where FT4 left a seam that PSK31 has to widen.
- `SlotClock` (unit 305) - **do not remove it; note where it is bound**, because step 4
  replaces it for this mode.
- `PROJECT_STATUS.md` for the current version. `PHASE_PLAN.md` §R7: the FT4 phase closed
  at Tim's word on 2026-09-11 and takes the minor bump; **this unit lands on the new
  minor** and reports the number.
- Tests: `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`,
  `TheReadinessHoverTests`, and whatever named the FT4 mode's existence when it was added.

---

## 6. Rulings in force

**`PHASE_PLAN.md` §2** - everything ruled for FT8 and FT4 applies unchanged: the two card
types (R1-R6), the map (R7-R9), the list with the worked-fade and the `CQ` guard, the
four nudge rulings, one click one transmission, the proved transmit chain, the withdrawn
dummy load, the achievements philosophy, the Digital family colour as text only, the
cited band map.

**`PHASE_PLAN.md` §3** - what is different: no slot clock, RST not dB, continuous carrier,
character-at-a-time arrival, no fixed message length. **None of §3 is built in this
unit. Nothing in this unit decodes, transmits, parses or logs.**

**`PHASE_PLAN.md` §R1-§R7** - the author's rulings, obeyed as written.

**HM-DEC-155** - section 1. **HM-DEC-032** - family colour. **HM-DEC-054** - the band map
is cited data and this unit adds no row to it. **§0.5 / HM-DEC-012** - family colour is
text colour only, never a fill. **§0.1** - the engine is never told that tabs exist.
**§0.2** - one click, one transmission; **this unit adds no click that transmits.**
**§2.1** - nothing personal in telemetry.

**FACT-004** - a dev-machine result is an indication, never a finding. **FACT-006** -
this machine has no radio.

**Tim, 2026-09-06 - the dummy load is withdrawn in full.** No compensating control.

---

## 7. Status cadence

`PROJECT_STATUS.md` per `CLAUDE.md` §13: **after every task, and at least every ten
minutes.** The watchdog fires at twelve minutes of silence. Write the status **before**
starting a long test run, saying what you are about to run.

---

## 8. The tasks

Four. Each names the test to watch failing first and one drop candidate. **Drop from the
back.**

### Task 1 - trace, and the phase opens. No production file changes except the version.

**1a.** Confirm the three phase files are the PSK31 phase's. Append `UNIT 312` to
`PHASE_OUTCOME.md` under step 0. **Did unit 311 run?** One line.

**1b.** Read `PHASE_PLAN.md` in full. **Restate §1 and §R1 in your own words in
`output.md`.** A session that cannot restate why PSK31 is a conversation and not a
protocol has found something worth knowing.

**1c.** Everything in section 5, checked and reported with file and line. In particular:
**how was FT4 added**, step by step, because task 2 copies it.

**1d.** The version. Per §R7 the FT4 phase's close takes the minor bump. **Bump the minor,
reset the patch, and report the before and after.** If `PROJECT_STATUS.md` shows a number
that makes this ambiguous, say so and do not guess.

**1e.** Run, filtered and foregrounded, before changing anything:
`BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`, `TheReadinessHoverTests`, and
`docs\carry-forward-tests.txt` if it exists. Report each green or red. **A red here is
inherited, not yours.**

**Test watched failing first:** none.
**Drop candidate:** none. **Not droppable.**

---

### Task 2 - PSK31 is a mode everywhere FT4 is a mode

**Copy the shape task 1c found.** Wherever FT4 is enumerated, named, coloured, logged,
reported in telemetry, offered in a tab or asked about in a hover, **PSK31 is too**, in
the Digital family with text colour only.

**What it tunes to.** Pressing the PSK31 tab tunes the radio to **14.070 MHz in USB-D**,
read from the cited row under `data/bands/`, **not from a constant in code**. If the row
carries an activity centre other than 14.070, **use the row and report the number.**

**What the panel shows.** The same decoded-text panel FT8 and FT4 use, empty, with a line
that names the mode and says plainly that nothing decodes yet - in the voice the readiness
hover already uses, **not a placeholder string**. `VoiceTests` runs if it exists, because
this adds copy the operator reads.

**What the log offers.** PSK31 as a mode. **Do not add the RST field**; that is step 5.
**Do not add an ADIF submode**; that is step 5.

**What telemetry carries.** The mode field says PSK31 when the tab is selected. Nothing
personal (§2.1).

**What is not built.** No decoder, no modulator, no parser, no card, no macro, no turn
indicator, no achievement. **Nothing in this task can cause a transmission.** `SlotClock`
stays where it is; if the panel shows it for PSK31, **report that it does** and leave it -
step 4 owns it.

**Test watched failing first:** `ThePsk31SeamTests`, app project. Watch it fail, then
green:

1. the mode is enumerated alongside FT8 and FT4 and is in the Digital family
2. selecting it asks the radio for 14.070 USB-D, and the frequency came from the cited
   band row - assert the source, not just the number
3. the panel names the mode and says nothing decodes
4. the log's mode list offers it
5. the telemetry mode field reads PSK31 with the tab selected, and no callsign, grid or
   location is in the record
6. **no path from the PSK31 tab reaches anything that keys the transmitter**

Re-run `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` filtered.

**Drop candidate:** assertion 5's telemetry check. Keep the field; drop the assertion.

---

### Task 3 - the reference clone, and nothing read from it yet

**`PHASE_PLAN.md` §R5.** Clone `fldigi` to `C:\Source\fldigi`, **outside the tree**. Pin
it: record the commit hash you cloned at in `docs\psk31-reference.md`, with the date, the
licence as read from the clone's own `COPYING`, and the rule that it is read and never
ported wholesale. **Nothing from it enters `src/` in this unit.**

**If the clone cannot be made** - network, tooling, anything - **say so in section 4 and
carry on**; step 1 needs it, step 0 does not.

**Also record in the same file** the varicode table's public source, so step 1 cites it
rather than transcribing from memory.

**Test watched failing first:** `ThePsk31ReferenceIsPinnedTests`, engine project. Watch
it fail, then green: `docs\psk31-reference.md` exists, names a commit, names the
licence, and **no file under `src/` contains a path into `C:\Source\fldigi`**.

**Drop candidate:** the whole task. Step 1 can do it first if step 0 runs short.

---

### Task 4 - the neighbourhood ribbon

**Drop candidate: this whole task.** It is step 0's nice-to-pass.

When the PSK31 tab is selected, the neighbourhood map's PSK31 ribbon at 14.070 lights the
way the FT8 ribbon lights at 14.074. Same mechanism, same family colour as text.

**Test watched failing first:** extend `ThePsk31SeamTests` by one: the ribbon for the
selected mode is the one lit.

**Drop candidate:** the whole task.

---

## 9. Parked

Not in this unit. Do not start any of it.

- **Anything that decodes** - step 1 and 2.
- **Anything that parses text into a state** - step 3.
- **Anything that transmits, or any macro** - step 4. **The transmit chain is not
  touched.**
- **The turn indicator and the `SlotClock` replacement** - step 4.
- **RST in the log, ADIF submode, PSK31 achievements** - step 5.
- **Acknowledgement indicators** (ask 8). Build nothing.
- **Reading anything out of the `fldigi` clone into `src/`.**
- **The FT8 and FT4 surfaces.** This unit adds a mode; it changes nothing about the two
  that exist.
- **`Avalonia.Headless.Skia` or any other package** (§0.4).

---

## 10. What not to do

- **No unfiltered `dotnet test`.** No whole project, no whole solution.
- **Never background a command and poll it.**
- **Do not touch anything that keys the transmitter.** The proved chain
  `cq_pressed → … → ft8_transmission Played` stays as it is.
- **The dummy load is withdrawn in full** (Tim, 2026-09-06). **No compensating control.**
- **Do not hard-code 14.070.** It comes from the cited band row.
- **Do not add a row to `data/bands/`.** It is cited data and the row exists.
- **Do not fill anything with the family colour.** Text only (§0.5).
- **Do not tell the engine a tab exists** (§0.1).
- **Do not port, copy or paste from `fldigi` into `src/`** in this unit.
- **Do not add or vendor a package.**
- **Do not claim an appearance from computation and call it seen.** Say *computed*.
- **Do not invent a ruling id.** `PHASE_PLAN.md` §R1-§R7 carry none; cite them by section.
- **Do not chase these known reds:** `CwAdjudicationTests.ASpeedChangeInRealisticAudio`;
  the 51 CW cases in `docs\unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests`
  whole-type-list tripwire; `HM-OPEN-088`'s ten; the two in
  `TheAchievementsScreenTests`; the one in
  `TheFitGuardAsksAboutTheGridTheSendIsOnTests`.
- **Do not repair this instruction.** Report mismatches; keep working.
- **Do not write to `DECISIONS.md`.** §12.1.

---

## 11. Reporting

`output.md` at the repository root. **The canonical headings, which
`tools\arbiter\validate-output.bat` requires: `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.**

Open with the A/B/C ordering block, then the header:

```
A. The phase goal - ...
B. Step 0 and its exit criteria - ...
C. The report last, and section 4 raises N items on top of a carried queue of eleven.
```

```
UNIT:       312 - <complete|stopped> at task N of 4, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no, and which step>
NUMBER:     <what moved, before -> after>
DRIFT:      0 consecutive units without advance  (new phase)
```

**Section 1 must answer three things in one line each, near the top:** whether unit 311
ran; the version before and after the minor bump; and what the cited band row says the
PSK31 activity centre is.

**Section 3 must quote, word for word, what the empty PSK31 panel says.**

**Every appearance claim in this report is computed, not seen. Say so once, plainly.**
