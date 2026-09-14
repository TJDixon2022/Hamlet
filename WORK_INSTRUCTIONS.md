# Work instruction 358 - the seam: Olivia exists as a mode

**Seed of the Olivia phase, under `--seed`.** Step 0 of `PHASE_PLAN.md`. The arbiter
authors steps 1 through 5 after it; step 6 is Tim's, and he is away five days. **Six
tasks, small.**

**Status.** `tools/status.sh`, real clock, after every commit and every task.

---

## 0. The project gate

```
SHACK_FACTS.md                                          must exist
src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs     must exist
CoreHMI.sln                                             must not exist
MURC.sln                                                must not exist
root                                                    C:\Source\HamLet
```

**If any of the four is wrong, stop and say so in `output.md` section 4. Write nothing
else.** The refusal text: *This is not Hamlet. Nothing was changed.*

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite; only this unit's names and `docs\carry-forward-tests.txt` as its
top comment says. Never background and poll.

## 2. The tool fact

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm`
is refused; Python cannot run here; `-m` more than once for a multi-line commit.

## 3. Asks still outstanding

Carried per HM-DEC-139 from unit 357's queue, **verbatim in section 4**.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet works Olivia the way it works PSK31.
UNIT GOAL:  Olivia exists as a mode - the tab, the cited calling spot, the panel,
            the Capture button, the RSID and calling data in the tree. Nothing
            decodes.
ADVANCES:   step 0 criterion 1, 2, 3, 4, 5
DRIFT:      0. A new phase starts the count.
```

**Read `PHASE_PLAN.md` at the root in full.** §1 says what Olivia is. §R27-§R31 are the
rulings from the interview. §6 is how the loop decides.

**Why the seam first.** The FT4 and PSK31 phases both found that every shared surface
had to be told a mode existed before any of them could be asked to do anything for it.
This unit does only the telling, so steps 1 to 5 land on a mode already wired everywhere.

---

## 5. Verify this instruction against the tree

- `PHASE_STATUS.md` line 1 names *Hamlet works Olivia the way it works PSK31* with seven
  steps. **If it is the screen phase, stop and say so** - `install-phase.bat` did not
  run.
- `docs\phase-screen-run\` holds the screen phase's three files.
- `assets\fixtures\olivia\` - nine WAVs and `manifest.json`; `assets\data\rsid-codes.json`;
  `assets\data\olivia-calling.json`; `assets\reference\SOURCE.md` and the generator source.
  **Hash every fixture against the manifest and report.**
- How PSK31 was added as a mode (units 312-314): the mode strip, `DigitalModeFor`,
  `DigitalCallingFrequencies.Find`, `ContactModes`, the telemetry mode field, the readiness
  line, the family palette. **This unit copies that shape exactly.**
- The Capture button (unit 344) on the PSK31 panel.
- `PROJECT_CARD.md`'s `PHASE` and `PHASE_SET`.
- Tests: `ThePsk31SeamTests`, `ThePsk31TabIsInertTests` or its successor,
  `TheCaptureButtonTests`, `BindingHealthTests`, `VoiceTests`.

**Report every mismatch; repair nothing.**

## 6. Rulings in force

**`PHASE_PLAN.md` §R27-§R31 and §6.** The PSK31 plan's R1-R20 stand. **§0.5** family
color as text; **§0.1** the engine never learns tabs exist; **§0.2** nothing here can
transmit; **§2.1** nothing personal in telemetry; **HM-DEC-054** the calling table is
cited data and labeled a convention. **HM-DEC-155**, **HM-DEC-139**, **FACT-004**,
**FACT-006**, **the dummy load withdrawn.**

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the phase opens

Append `UNIT 358` to `PHASE_OUTCOME.md` under step 0. Patch-bump. Set `PROJECT_CARD.md`'s
`PHASE` and `PHASE_SET`. Record in `DECISIONS.md`, as Tim's ruling of 2026-09-14: the
Olivia phase set after an interview, PSK31 tabled after unit 357, the screen phase
archived with step 3 open. Hash the nine fixtures against the manifest. Run the
carry-forward list.

**Drop candidate:** none.

### Task 1 - the data is in the tree and read

`assets\data\rsid-codes.json` to `data\rsid\rsid-codes.json`; `assets\data\olivia-calling.json`
to `data\bands\olivia-calling.json` beside the cited band rows. Both read at startup; a
missing or malformed file is reported in a sentence on the panel and nothing is guessed.
The calling row for the current band is what the Olivia tab tunes to.

**Test watched failing first:** `TheOliviaDataTests`, engine: both files parse; the
8/250 code is 69 and BPSK31 is 1; 20 m's calling center is 14,073,000; a malformed copy
yields the sentence and no value.

**Drop candidate:** none.

### Task 2 - Olivia is a mode everywhere PSK31 is a mode

Wherever PSK31 is enumerated, named, colored, logged, reported in telemetry, offered in
a tab or asked about in a hover, **Olivia is too**, in the Digital family, text color
only. Pressing the tab tunes to the band's calling center from task 1's row, USB-D. The
panel: the same decoded-text panel, empty, with a line in the readiness voice naming the
mode and saying it cannot be read yet. **The log offers `OLIVIA`**; the submode is step
5's. Telemetry's mode field says Olivia.

**Under Olivia no other decoder runs and no path reaches the send chain.** The seam is
inert for sending until step 4.

**Test watched failing first:** `TheOliviaSeamTests`, app: the mode is enumerated in the
Digital family; selecting it asks the radio for the calling center from the cited row;
the panel names the mode; the log offers it; the telemetry field says Olivia with nothing
personal; no decoder is attached and no path reaches anything that keys.
`BindingHealthTests`, `VoiceTests`.

**Drop candidate:** none.

### Task 3 - the Capture button

The button from unit 344 is on the Olivia panel and does what it does on PSK31: 48 kHz,
before the resampler, two minutes, the events, the path in words.

**Test watched failing first:** extend `TheCaptureButtonTests`: pressing under Olivia
writes the WAV and the events with `mode: olivia`.

**Drop candidate:** none.

### Task 4 - the neighborhood map

The map picks out the Olivia spot when the tab is selected, as it does for PSK31.

**Test watched failing first:** extend `TheOliviaSeamTests` by one.

**Drop candidate:** the whole task - it is the nice-to-pass.

### Task 5 - the timing table's first row

Step 2 needs seconds-per-character per variant. From the manifest alone - seconds and
characters for each fixture, minus the RSID burst's 2.32 s where present - compute a
first estimate for 8/250, 16/500 and 32/1000 and write it to `data\olivia\timing.json`
marked *estimated from the fixtures' lengths; step 2 measures it*. Nothing reads it yet.

**Test watched failing first:** none; a data file with a stated method.

**Drop candidate:** the whole task.

---

## 9. Parked

- **Anything that decodes, detects RSID, modulates, parses, or transmits.** Steps 1-5.
- **Reading `pj_mfsk.h`** beyond confirming the clone is pinned.
- **Any package.**

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not hard-code a calling frequency or an RSID code.** The files.
- **Do not attach a decoder or reach the send chain.**
- **No package. Report mismatches; repair nothing. Write American.**

## 11. Committing and pushing

Commit per task; push at the end.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Step 0
   <state after this unit>, 1-6 not started.
B. Step 0's criteria 0.1 to 0.6 - each met or not, with the number.
C. The report last, and section 4 raises N items on top of the carried queue.
```

```
UNIT:       358 - <complete|stopped> at task N of 6, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no>
NUMBER:     fixtures hashed 0 -> 9; calling rows read 0 -> 8
DRIFT:      0
```

**Every appearance claim is computed, not seen.**

---

```
ARBITER-DECISION
STEP: 0
APPROACH: wire Olivia everywhere PSK31 is wired, read the cited calling table and the RSID codes from data files, put the Capture button on the panel, and keep the tab inert for decoding and sending
MOVE: continue
WHY: step 0 depends on nothing; the shape is the PSK31 seam's, proved twice; every value comes from a cited file
STATE: not started
DECIDED: nothing beyond the plan; the timing table's first row is marked an estimate
LICENCE: PHASE_PLAN.md R27, R29, R30, section 6; PSK31 plan R11, R12, R13, R14, R19; CLAUDE.md 0.1, 0.2, 0.5, 2.1; HM-DEC-054
ACCOMPLISHED: Olivia is a tab that tunes to the right place and says what it cannot do yet, with its data cited and its fixtures hashed
ADVANCES: step 0 criterion 1, 2, 3, 4, 5
END-ARBITER-DECISION
```
