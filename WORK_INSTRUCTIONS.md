# Work instruction 344 - capture the air, and find the garble

**Single session, Hamlet, before the screen phase resumes.** PSK31 is on the screen for
the first time and the text is garbled. Two things: a button that captures the receive
audio so the demodulator can be proved against real air, and the first measured
hypothesis for the garble. **Three tasks.**

**Status.** `tools/status.sh`, real clock, before every `dotnet` command, after every
commit, after every task. The watchdog polls the process now; the status is for Tim.

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

**Also:** `.run-unit\allowed.txt` must permit `dotnet`. If `dotnet test` is refused, stop
and say so in section 4 - the arbiter install is wrong and nothing here can be proved.

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite; only this unit's names and `docs\carry-forward-tests.txt` as its
top comment says. Never background and poll.

## 2. The tool fact

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm`
is refused; Python cannot run here; `-m` more than once for a multi-line commit.

## 3. Asks still outstanding

Carried per HM-DEC-139 from the last Hamlet report's queue, **verbatim in section 4**.
This unit answers the oldest one - **real off-air PSK31 audio** - by giving Hamlet the
means to make it.

---

## 4. Why this unit exists

```
PHASE GOAL: (the screen phase is paused; this is PSK31 carried repair)
UNIT GOAL:  A button captures the receive audio to a WAV; the demodulator's
            garble on real air gets its first measured cause.
ADVANCES:   The PSK31 phase's step 6, which is open because of this.
DRIFT:      carried.
```

**2026-09-13, 7.070, 23:13 to 23:49, version 1.13.30.** The record: 18 carriers, squelch
quality to 0.999, **106 characters from a station at 329 Hz**, 68 and 68 from one at
1023 Hz, zero lines parsed. **And on the screen, for the first time, a row:**

```
234906   -2   dt  oe epe Ae peey@teI ç
```

That is what a PSK31 decoder produces when most bits are right and a few are wrong:
the short varicodes - e, t, o, a, space - survive and the long ones break. The display
works. The demodulator loses bits on real air that it never lost on the synthetic
fixtures, and nothing in the tree can say why, because **no real audio has ever been
through it.** Every fixture in `assets/fixtures/` was made by `reference-modem.py` on a
machine with no radio.

**Two hypotheses, from the record:**

1. **The filter skirt.** 329 Hz is the bottom edge of a 3 kHz USB-D filter that rolls off
   around 300. A carrier on the skirt arrives with its phase smeared, and PSK31 is
   nothing but phase. Unit 337 proved a 1 Hz carrier error costs nothing and a 2% clock
   error costs a few letters - the row above looks like the latter, which a phase-smeared
   edge could produce.
2. **Fading.** The fixtures are steady; 40 m at night is not. A slow fade through the
   squelch threshold means the bit clock re-acquires mid-word.

Neither can be settled without the audio. So the audio first.

---

## 5. Verify this instruction against the tree

- The PSK31 audio path: where the 48 kHz device stream enters, where it is resampled to
  8 kHz (unit 324), the buffer the search and demodulators read. **The capture taps the
  device stream before the resampler** - the real thing, not Hamlet's version of it.
- The PSK31 panel and its buttons; the telemetry writer and the `psk31` category.
- `Psk31Demodulator` - its bit-clock recovery, the squelch, the AFC; `Psk31CarrierSearch`
  and its passband floor (203 Hz).
- `assets/reference-modem.py`'s convention; `assets/fixtures/manifest.json`.
- Tests: `ThePsk31DemodulatorTests`, `ThePsk31HearsEveryoneTests`, `TheRowShowsWhatWasHeardTests`.

**Report every mismatch; repair nothing but this unit's.**

## 6. Rulings in force

**PSK31 plan R9** a character not sure of is not shown; **R13** telemetry on every stage;
**R12** own tests; **R14** nothing beyond the criterion. **§0.2** nothing here touches
transmit; the capture is receive only. **HM-DEC-018, §2.1** - the WAV is audio and
carries no callsign as data; its filename is a timestamp. **§0.0** - a hypothesis is
reported as a hypothesis until a fixture proves it. **HM-DEC-155**, **HM-DEC-139**,
**FACT-004**, **FACT-006**.

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 1 - the capture button

On the PSK31 panel, one button: **Capture 2 minutes**. Pressing it writes the device
audio - 48 kHz, 16-bit mono, before Hamlet's resampler - for 120 seconds to
`%AppData%\Hamlet\captures\psk31-<UTC timestamp>.wav`, and while it runs the button
reads *capturing · 1:37 left* and then *captured · 2:00*. A second press while running
stops it early and keeps what it has. The panel says where the file went, in words a
person can follow to it.

**Telemetry:** `psk31_capture_started` (dial, sample rate), `psk31_capture_finished`
(seconds, bytes, SHA-256, the carriers held at the moment it started with their offsets
and qualities - so the file and the record can be matched later). No callsign.

**Also:** a one-line note in the report telling Tim to copy any capture he makes into
`assets\fixtures\captured\` and commit it, so the next unit can prove against it.

**Test watched failing first:** `TheCaptureButtonTests`, app: pressing writes a WAV of
the stated format and length from a fed stream; a second press stops early; the events
fire with the stated fields; the panel line names the path; `BindingHealthTests`.

**Drop candidate:** the early-stop. Keep the two-minute capture.

### Task 2 - the two hypotheses, measured on synthetic air

No real audio exists yet. Make it as real as the reference convention allows, and see
whether either hypothesis reproduces the garble:

- **Skirt:** the clean fixture at **330 Hz** offset, passed through a high-pass whose
  −3 dB point is 300 Hz with a realistic slope (a 4th-order Butterworth is the honest
  default; state what you used). Decode; report the CER and the first 36 characters.
  Then the same at 1000 Hz for the control.
- **Fade:** the clean fixture at 1000 Hz with a slow amplitude fade - a 0.2 Hz sinusoid
  taking the level from 0 to −20 dB and back - so the squelch closes and opens mid-text.
  Decode; report CER and what happens at each reopening.

**If either reproduces the shape of the row above** - short letters surviving, long
codes breaking - say which, and **name the fix as the next unit's**: for the skirt, the
search's floor moves up and a row near the edge says *at the filter edge* in words; for
fading, the bit clock holds its phase across a squelch close instead of re-acquiring.
**Do not build the fix here.** If neither reproduces it, say so; the real capture will.

**Test watched failing first:** `Unit344Measure`, engine, asserts nothing; writes the
table.

**Drop candidate:** the fade case. Keep the skirt.

### Task 3 - the record reads the capture back

When a capture exists under `assets\fixtures\captured\`, `ThePsk31DemodulatorTests` gains
one test that runs the full receive path over it and reports - no ceiling - the carriers
found, the characters emitted per carrier, and the first 60 characters of each, into the
test output. **It passes if the file decodes at all and is skipped, not failed, when the
folder is empty.** This is the test every later demodulator change is proved against.

**Test watched failing first:** the new test, skipping on an empty folder.

**Drop candidate:** the whole task.

---

## 9. Parked

- **The screen phase.** Resumes after this unit.
- **The fix for the garble.** Named by task 2, built by the next unit against the real
  capture.
- **Anything touching transmit. Any package.**

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not capture after the resampler.** The device stream.
- **Do not build the garble fix here.** Measure, name, stop.
- **Do not touch transmit. No package. Report mismatches; repair nothing but this unit's.
  Write American.**

## 11. Committing and pushing

Commit per task; push at the end.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The screen phase is paused; this is PSK31 carried repair. Its steps are unchanged.
B. No step criterion moves.
C. The report last, and section 4 raises N items on top of the carried queue.
```

```
UNIT:       344 - <complete|stopped> at task N of 3 - <date time>
PHASE GOAL: <the paused phase, restated>
UNIT GOAL:  <restated in your own words>
ADVANCED:   no
NUMBER:     captures Hamlet can make 0 -> 1 per press; skirt CER at 330 Hz <n>, at 1000 Hz <n>
DRIFT:      carried
```

**Section 2 tells Tim, in three sentences, what the button does, where the file goes,
and to copy it into `assets\fixtures\captured\` and commit it. Section 3 prints the task
2 table.** **Every appearance claim is computed, not seen.**
