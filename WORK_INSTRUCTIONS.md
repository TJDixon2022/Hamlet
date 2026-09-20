# Work instruction 369 - settings survive an upgrade, and a missing device says so

**Seed of the hardening phase, under `--seed`.** Step 0 of `PHASE_PLAN.md`; the arbiter
authors steps 1 to 4 after it; step 5 is Tim's. Three tasks. Touches the settings model
and the refusal text; nothing about what keys.

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

Carried per HM-DEC-139 from the last report's queue, **verbatim in section 4**.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet holds what it has.
UNIT GOAL:  A saved setting outlives an upgrade; a send with no transmit device
            chosen says so and opens Settings; a refusal names the device, the
            rate and the OS error.
ADVANCES:   step 0 criterion 1, 2, 3, 4, 5
DRIFT:      carried.
```

**What happened.** Between 1.13.30 (2026-09-14, PSK31 transmitting) and 1.13.48
(2026-09-19), the transmit audio device setting was lost. FT8 sends stopped after
read-back with no message; unit 362 gave the stop a voice - `send_refused, stage arm,
reason transmit_device_would_not_open` - and Tim found the cause on his own: *"the
settings lost the listing setting."* He re-chose the USB Audio CODEC and FT8 transmitted.

**Two faults.** A settings change since 1.13.30 - the RSID work, the capture work, or
the ALC reference - dropped a saved value on load, and nothing tested that an old
settings file still loads whole. And the refusal named the wrong fault: the device did
not fail to open; **no device was chosen**, and Hamlet knew which.

---

## 5. Verify this instruction against the tree

- `PHASE_STATUS.md` line 1 names *Hamlet holds what it has*; **if it is the Olivia phase, stop
  and say so** - `install-phase.bat` did not run.

- The settings model, its file, its loader; every field added since 1.13.30 (the ALC
  reference, capture, RSID) and how the loader treats a file that lacks them; where the
  transmit device is read at arm time.
- `git log` for the settings file's shape at 1.13.30, 1.13.40 and HEAD - the unit
  reconstructs old-shape files from those commits as fixtures.
- Unit 362's refusal path and the `send_refused` reasons.
- Settings UI: the transmit device picker and how it is opened.

**Report every mismatch; repair nothing but this unit's.**

## 6. Rulings in force

**§0.0** a refusal says the true reason. **§0.2** a click that does not transmit says why.
**R11** nothing is asked of the operator at the radio - but a device on the computer is
his to choose, once, and Hamlet remembers it. **R13** telemetry. **R12**, **R14**, **R19**.
**HM-DEC-155**, **HM-DEC-139**, **FACT-004**, **FACT-006**.

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - which change dropped it

Append `UNIT 369` to `PHASE_OUTCOME.md` under step 0. Patch-bump. Set `PROJECT_CARD.md`'s
`PHASE` and `PHASE_SET`; record in `DECISIONS.md`, as Tim's ruling of 2026-09-20, the
hardening phase set and the Olivia phase archived at 38 of 40 with 5.4 and 6.1 his. Run the carry-forward list. Then: from the git history of the settings
model between 1.13.30 and 1.13.48, **name the commit and the line that made the loader
drop the transmit device** - a renamed key, a new required field, a rewrite that missed
one. Report it.

**Drop candidate:** none.

### Task 1 - settings survive an upgrade

The loader reads any settings file written since 1.13.30 and keeps every value it
carried; a field the file lacks gets its default and is written back; **a field the
file has is never dropped.** Fixtures: settings files reconstructed at 1.13.30, 1.13.40
and HEAD, each loaded and every value asserted.

**Test watched failing first:** `TheSettingsSurviveAnUpgradeTests`, app: each fixture
loads with its transmit device, receive device, operator grid, callsign, license class,
power offer and ALC reference intact; a missing new field takes its default; nothing
present is lost; the loaded file round-trips.

**Drop candidate:** none.

### Task 2 - a missing device says so

At arm time, no transmit device chosen is its own refusal: `send_refused reason
no_transmit_device`, and on the panel, in words, *No transmit device is chosen. Open
Settings and pick the radio's sound card* - with the word *Settings* opening it. A device
that is chosen and will not open keeps `transmit_device_would_not_open` and **carries the
device name, the rate asked for and the OS error text** in the event and on the panel.

**Test watched failing first:** `TheRefusalNamesTheFaultTests`, app: no device chosen
yields the `no_transmit_device` refusal and the sentence; a chosen device that refuses
yields the other with name, rate and error; the Settings link opens the picker;
`TheSendReachesTheAirTests` green.

**Drop candidate:** the Settings link. Keep the sentence.

---

## 9. Parked

- **Steps 1 to 4.** The arbiter authors them.
- **Anything else.**

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not change what any setting means.** Only how it survives.
- **Do not touch the send path beyond the refusal's reason and text.**
- **No package. Report mismatches; repair nothing but this unit's. Write American.**

## 11. Committing and pushing

Commit per task; push at the end.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 <state after this unit>, 1-5 not started.
B. Step 0's criteria 0.1 to 0.5 - each met or not, with the number.
C. The report last, and section 4 raises N items on top of the carried queue.
```

```
UNIT:       369 - <complete|stopped> at task N of 3 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no>
NUMBER:     settings-file shapes that load whole 0 -> 3; the commit that dropped the device <hash>
DRIFT:      carried
```

**Section 1 names the commit and the line. Section 2 tells Tim, in two sentences, that
his settings will not be lost again and what he will see if a device is ever missing.**

---

```
ARBITER-DECISION
STEP: 0
APPROACH: find the commit that dropped the transmit device on load, make the loader keep every value an old file carries, and split the refusal into no-device-chosen with a Settings link and device-would-not-open with the name, rate and OS error
MOVE: continue
WHY: the transmit device setting was lost on upgrade and FT8 could not send for five days; the refusal named the wrong fault
STATE: not started
DECIDED: nothing beyond the fault
LICENCE: CLAUDE.md 0.0, 0.2; PSK31 plan R11, R12, R13, R14, R19
ACCOMPLISHED: an upgrade never again forgets what Tim chose, and a send that cannot go says the true reason
ADVANCES: step 0 criterion 1, 2, 3, 4, 5
END-ARBITER-DECISION
```
