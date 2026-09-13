# Work instruction 337 - the row shows what the ear heard

**Single session, or seed under `--seed`.** One fault, proved from the record, fixed
before anything else in the maintenance phase: **the PSK31 demodulator reads real air
and the screen shows none of it.** Three tasks.

**Status.** `tools/status.sh`, real clock, before every `dotnet` command, after every
commit, after every task.

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

Carried per HM-DEC-139 from unit 336's queue, **verbatim in section 4**.

---

## 4. Why this unit exists

```
PHASE GOAL: The screen says what is true and looks like someone meant it.
UNIT GOAL:  A PSK31 row shows the characters its demodulator emits, as they
            arrive, and every count of them agrees.
ADVANCES:   No step of this phase. Carried repair from the PSK31 phase, whose
            step 6 is open because of this fault.
DRIFT:      carried.
```

**Tim, 2026-09-13:** *"listened for psk31 all night, not one message, it is broken."*

**The record says otherwise, and says where.** `%AppData%\Hamlet\telemetry\2026-09-12.jsonl`,
session `0ec9f69e`, version 1.13.16, 14.070, 18:11 to 19:17:

- `psk31_carrier_retired` for carrier 19 at 1085.4 Hz: **`charactersEmitted: 262`**,
  lifetime 84.7 s. Carrier 20 at 1082.3 Hz: **190**. Carrier 18: **187**. Carrier 23 at
  1476.7 Hz: **184**. Carrier 13: **143**. Twenty-eight of thirty-three carriers emitted
  characters. About three a second - PSK31's typing speed.
- `psk31_squelch` opened 75 times, qualities up to 0.997.
- `psk31_line_parsed` at 18:36:18: **`kind: Cq, certain: true, characters: 52`** from
  845 Hz. A real station, read correctly, parsed as a CQ.
- `psk31_listening_stopped` at 19:17:54: **`charactersEmitted: 0, linesParsed: 2`**.

**Two counters disagree by 262 to 0, and the screen showed nothing.** Last night on 7.070
(`2026-09-13.jsonl`, session `5d5209ec`, 04:09 to 13:15): 28 carriers, 34 `psk31_reading`
events, one line parsed, nothing on the screen.

**So the ear works.** What is broken is between the demodulator's output and the row.
Whether the 262 characters were readable text or bit-slip garbage is a second question -
the certain CQ says the demodulator can read - but since the screen showed neither, the
display is the first fault and this unit's.

---

## 5. Verify this instruction against the tree

- Where a `Psk31Demodulator` emits a character and who consumes it: the per-carrier
  buffer, the row object (unit 314's replace-in-place, as units 327-332 left it), the
  splitter (unit 316), the parser, the panel binding.
- The two counters: the per-carrier `charactersEmitted` in `psk31_carrier_retired` and
  the session total in `psk31_listening_stopped`. Which is read from what.
- The PSK31 row's text binding in `MainWindow.axaml`; the *heard, not readable yet*
  dimmed form (unit 324) and what switches a row from dimmed to text.
- `ThePsk31HearsEveryoneTests`, `ThePsk31PanelHearsTests`, `ThePsk31TelemetryTests`,
  `ThePsk31StationIdlesTests`.

**Report every mismatch; repair nothing but the fault.**

## 6. Rulings in force

**`PHASE_PLAN.md` §6** - three stops only; a later ruling wins. **PSK31 plan R9** a
character the demodulator is not sure of is not shown - **and a character it is sure of
is.** **R13** telemetry. **R12** own tests. **§0.0** a row that shows nothing while 262
characters arrived is a false claim. **HM-DEC-155**, **HM-DEC-139**, **FACT-004**,
**FACT-006**.

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - reproduce from the fixture, then find the break

Append `UNIT 337` to `PHASE_OUTCOME.md` as carried repair; patch-bump; run the
carry-forward list.

Play `assets\fixtures\psk31-four-signals.wav` through the running PSK31 path with the
panel up. **Do the rows show text?** If they do, the fault is in something real air has
that the fixture lacks - report the difference between the fixture's path and the record's
(offsets near 1085 Hz, idle gaps, AFC movement) and reproduce with a fixture that has it.
If they do not, the break is in the tree at HEAD; find it. Either way: **with file and
line, where the emitted character stops.**

**Drop candidate:** none.

### Task 1 - the row shows every emitted character

A held carrier's row shows the characters its demodulator emits, appended as they
arrive, whatever they are. The dimmed *heard, not readable yet* form is for a carrier
whose squelch is closed; **the moment the squelch opens and a character is emitted, the
row lifts and the character is on it.** The splitter and parser act on the same text
the row shows, not on a different copy.

**The counters agree.** `psk31_listening_stopped`'s `charactersEmitted` is the sum of the
per-carrier counts; `linesParsed` likewise. One source.

**Test watched failing first:** `TheRowShowsWhatWasHeardTests`, app: the four-signal
fixture puts 483 characters on four rows and the session total says 483; a carrier that
emits a character while its row is dimmed lifts on that character; the text the parser
splits is the text the row shows; `psk31_listening_stopped` totals equal the sum of the
retires.

**Drop candidate:** none.

### Task 2 - what the 262 characters were

With the display fixed, the second question. Take the record's carrier 19 - 84.7 s at
1085 Hz, 262 characters, 0 lines - and say which is more likely from the tree: readable
text the splitter never cut (no turnover word in 262 characters is unusual for a QSO but
not impossible in a ragchew), or bit-slip garbage. **Measure, do not guess**: feed the
idle fixture and the clean fixture with a deliberate 1 Hz AFC offset and a 2% clock
error and report what the demodulator emits. If it emits garbage at a small clock
error, that is the next unit's fault and is named here.

**Test watched failing first:** none new; a measurement, reported.

**Drop candidate:** the whole task.

---

## 9. Parked

- **The maintenance phase's steps 1 and 2.** The arbiter continues them after this.
- **Anything touching transmit. Any package.**

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not filter characters out of the row for looking wrong.** A character the
  demodulator emitted is shown; the squelch is the only gate.
- **Do not touch the transmit chain. No package. Report mismatches; repair only the
  fault. Write American.**

## 11. Committing and pushing

Commit per task; push at the end.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - the screen says what is true. Steps unchanged by this unit.
B. No step criterion moves; carried repair on the PSK31 row.
C. The report last, and section 4 raises N items on top of the carried queue.
```

```
UNIT:       337 - <complete|stopped> at task N of 3 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   no
NUMBER:     characters shown from the four-signal fixture 0 -> 483; the break at <file:line>
DRIFT:      carried
```

**Section 2 tells Tim, in one paragraph, where the characters were going and what he
will see tonight on 7.070.** **Every appearance claim is computed, not seen.**

---

```
ARBITER-DECISION
STEP: 0
APPROACH: find where an emitted PSK31 character stops between the demodulator and the row, make the row show every emitted character as it arrives, make the two character counters one source, and measure whether the 262-character carrier was text or bit-slip
MOVE: continue
WHY: the owner's record shows the demodulator emitting hundreds of characters and a certain parsed CQ on real air while the screen showed nothing; the ear works and the display does not
STATE: not started
DECIDED: the fault is display-first by the record; whether the characters were readable is measured in task 2 and named for the next unit if not
LICENCE: PHASE_PLAN.md section 6; PSK31 plan R9, R12, R13; CLAUDE.md 0.0
ACCOMPLISHED: a PSK31 station Hamlet can hear is a row with his words on it, as the record already said it should have been
ADVANCES: none - carried repair
END-ARBITER-DECISION
```
