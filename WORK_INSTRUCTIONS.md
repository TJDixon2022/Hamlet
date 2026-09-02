# Work instruction 224 - the Digital tab stops being a picture of an FT8 session and becomes one

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

The four are carried forward unchanged per `ARBITER.md` section 7.

---

# THIS IS A SEED INSTRUCTION

**Written by the web session. The launcher was given `--seed`, so iteration 1
executes this file as written and the arbiter authors every unit after it.**

**The arbiter continues after this unit.** Step 7 is unlikely to close in one
unit and is not expected to. What this unit must do is put **real decoded text on
the Digital tab** and leave the next unit a clean entry. Step 6 is also open and
independent; the arbiter may take either.

Read `PHASE_PLAN.md` before this file.

---

# Why this unit exists

**The Digital tab is finished-looking and entirely fake.** Work instruction 037
built it that way on purpose and the markup says so in its own words:

> Nothing here decodes, nothing moves, nothing is wired. It exists so the
> operator can look at a finished-looking FT8 session and say what is wrong with
> it before there is a decoder to argue with.

There is a decoder to argue with now. Steps 1 to 5 are closed: the library packs
a callsign into 77 bits, produces tones bit-identical to upstream's for 51 of 51
messages, finds 56 of 56 signals at rank 1 including in noise, and turns a found
signal into a message without inventing any - **0 wrong messages in 18 000
trials, 0 of 5096 bad-CRC codewords returned.**

**Tonight the tab stops lying.**

```
  PHASE GOAL:   Hamlet hears FT8 off the radio and displays the decoded text
                on screen.
  STEP:         7 of 7 - Hamlet displays decoded FT8.
  UNIT GOAL:    Real decodes reach the Digital tab's decoded table, and every
                hardcoded row is gone.
  ADVANCES:     step 7. This unit is not required to close it.
```

**The owner wants to be at the radio tomorrow morning with this working.** That
is the shape of the night: a working path first, polish second.

---

# What already exists - do not rebuild any of it

Read by the web session from a harvest taken 2026-09-02. **Verify each against
the tree and report any mismatch.**

**Capture is real.** `MainWindowViewModel.CaptureDigital` at about line 6256
takes `_decoder.Tap.Snapshot()`, gets an audio buffer with `Samples`,
`SampleRate` and `Duration`, and writes it with `WavAudio.Write`. It is not a
mock-up and the button works today.

**`ClockOffset` already exists** and is already passed to
`DigitalCaptureSheet.Compose`. **Step 7's clock criterion may already be met** -
check before building anything for it.

**`DigitalSpectrum`** is an `AudioSpectrumSource` constructed at about line 3589
from `_audioInput.SampleRate`, listening to `_audioInput`, started and stopped
with the session. The waterfall panel binds to it.

**The four panels exist** in `Views/MainWindow.axaml` from about line 2712:
`DigitalModeStrip`, `DigitalWaterfallPanel`, `DigitalDecodedPanel`,
`DigitalSayingPanel`. They are the tab's, fixed, not widgets, not draggable
(Tim, 2026-08-28). **Nothing here reintroduces a canvas.**

**`ViewModels/DigitalIdleText.cs`** holds the four idle strings, written in the
CW terminal's voice. **These are real and they stay.** An empty panel is
indistinguishable from a broken one, and one message for the whole tab is lost
when a panel is collapsed, so each carries its own (HM-DEC-021). The unit swaps
between idle and live; it does not delete them.

**The decoded table's columns are already committed**: `utc / snr / dt / hz /
message`. That is what the decoder produces. **The shape is settled and this unit
is wiring, not design.**

---

# Tasks

## Task 1 - the ground, and what step 7 already has

Re-measure rather than inherit: `Ft8Sharp` totals, attribution from `2828ab6`,
the channel tests. Then answer three questions from the code:

- **What sample rate does `_audioInput` deliver, and does anything already
  resample?** `Ft8Sharp` wants 12 kHz.
- **Is `ClockOffset` measured against UTC, and to what accuracy?** If it already
  satisfies step 7's clock criterion, say so and do not rebuild it.
- **Does `AudioTap` expose a continuous stream or only `Snapshot()`?** Slot-
  aligned decoding needs to know.

**Report what you find. Do not assume this instruction is right about any of it.**

## Task 2 - a decode reaches the table, by the shortest honest path

**Press the capture button, decode what it captured, put the result in the
table.** That is the whole task.

The path exists end to end already: `Tap.Snapshot()` gives samples,
`Ft8Sharp` decodes them, the table has columns for what comes out. **Do not
build slot alignment, a background service, or a continuous pipeline in this
task.** One button, one decode, real rows.

**Bind the table.** The four data rows in `MainWindow.axaml` are literal
`Text="..."` values - `CQ K1ABC FN42` at 1240 Hz, `VE7AA N0RR RR73` at -21 dB
and the rest. They become an `ItemsControl` over a collection the view model
owns. **Every hardcoded row goes in the same change that puts real ones in**, so
the table never shows both.

**When the collection is empty, `DigitalIdleText.Decoded` shows.** That is what
those strings were written for.

## Task 3 - slot alignment, if the window allows

FT8 transmits in 15-second slots aligned to the UTC quarter minute. A decoder
handed audio that straddles two slots decodes neither.

**This task is the drop candidate and it is named as such.** If task 2 lands and
the window is thin, **drop this whole and say so** - a capture button that
decodes real audio is worth more tonight than a half-built continuous path, and
the next unit picks it up with the hard part already proven.

If it is built: align on the quarter minute, decode each slot as it completes,
append to the same collection. **`ClockOffset` is what tells the operator the PC
clock is wrong** - FT8 needs it within about a second of UTC or nothing decodes,
and it fails silently, which is the commonest newcomer failure in this mode.
**Hamlet says so plainly rather than showing an empty table.**

## Task 4 - the other three panels

Only if tasks 1 and 2 are done and time remains. Mode strip, waterfall summary,
and the plain-English panel. **The plain-English panel is the one to leave
last** - what Hamlet says a message means is the owner's under `CLAUDE.md` 12.1,
and this unit must not decide it.

## Task 5 - what the next unit inherits

Write into `porting-notes.md` or the report: what was wired, what was left, the
sample rate answer, and whether slot alignment was built or dropped.

---

# What not to do

- **Do not delete `DigitalIdleText`.** It is real and it is the owner's voice.
- **Do not reintroduce the canvas** or make the panels widgets.
- **Do not decide what a message means in plain English.** `CLAUDE.md` 12.1.
- **Do not tune any decoder threshold.** Step 6's number is not this unit's.
- **Do not run the full Hamlet suite.**
- **No transmit work.** `CLAUDE.md` 0.2. Nothing this unit produces reaches an
  antenna, a port, or a keying line.
- **Do not commit `ft8_lib`, its WAVs, its built oracles, or the IC-7300 manual.**

---

# Committing, pushing, reporting

Commit and push each task before starting the next. Take the version bumps.

Write `output.md` per `CLAUDE_CODE.md` section 8. **Exactly four top-level
sections, at `##` level, spelled and ordered exactly:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

Use `###` and deeper for your own headings. **Then run
`tools\arbiter\validate-output.bat output.md` yourself and fix the report until
it exits 0.**

**Section 3 leads with whether real decoded text appeared on the Digital tab, and
what it said.**

Then stop. Do not start the next unit.

---

```
ARBITER-DECISION
STEP: 7
APPROACH: wire the Digital tab's decoded table to real Ft8Sharp output by the shortest honest path - capture button to decoder to bound collection - and remove every hardcoded row in the same change
MOVE: continue
WHY: Steps 1 to 5 are closed and the decoder turns found signals into messages without inventing any, but the Digital tab is still the finished-looking mock-up work instruction 037 built on purpose so the operator could argue with a picture before there was a decoder to argue with. There is one now. Step 7 is independent of step 6 by the plan's 2026-09-02 ruling, so the tab can be made real while step 6's sensitivity question is still open, and step 6's 4.2 per cent at threshold does not bear on it - 20 m FT8 is full of strong signals and this decoder reads 760 of 1298 reference lines. The owner wants to be at the radio tomorrow morning with text on screen, so the night is shaped working path first and polish second.
STATE: partial
DECIDED: That the shortest honest path is the capture button rather than a continuous pipeline, because that path already exists end to end - Tap.Snapshot gives samples, Ft8Sharp decodes them, the table has columns for what comes out - and a decode reaching the screen tonight is worth more than a half-built background service. That slot alignment is the named drop candidate for the same reason, and that dropping it whole is explicitly licensed rather than treated as a failure. That every hardcoded row comes out in the same change that binds real ones, because a table showing both is worse than a table showing neither. That DigitalIdleText is not deleted - it is the owner's voice from 2026-08-28, each panel carries its own line because one message for the whole tab is lost when a panel collapses, and the unit swaps between idle and live rather than removing the idle case. That the plain-English panel is left last and its wording is not decided, because what Hamlet says a message means is the owner's under CLAUDE.md 12.1. That task 1 asks three questions of the code rather than asserting answers, because the web session read this tree from a harvest and has been wrong about it before.
LICENCE: PHASE_PLAN.md's step 7 section; the plan's 2026-09-02 ruling that steps 6 and 7 depend on step 5 and not on each other, which is what licenses working step 7 while step 6 is blocked; the plan's ruling on what a unit runs, which forbids the full Hamlet suite; the plan's 2026-09-01 ruling that an unmet nice-to-pass criterion does not hold a step open; CLAUDE.md 0.2, restated because audio moves in this unit and nothing it produces may reach a transmitter; CLAUDE.md 12.1, which is why the plain-English panel's wording is not this unit's; HM-DEC-021 for each panel carrying its own idle line; HM-DEC-152 and HM-DEC-150 for the version bumps. Reported plainly: this instruction was written from a harvest of src/Hamlet.App/Views and ViewModels taken 2026-09-02, and every claim it makes about CaptureDigital, ClockOffset, DigitalSpectrum, AudioTap and the markup is to be checked against the tree rather than trusted.
ACCOMPLISHED: Twenty-four units have built a decoder nobody has ever seen work. Tonight the operator presses a button on the Digital tab and reads what the radio actually said - not a mock-up of what it might say, and not a number in a test report. The four hardcoded rows that have stood in for a working session since work instruction 037 come out, and what replaces them is whatever the band was doing when the button was pressed. If the band is quiet the table says so in the words Tim wrote for it in August. Either way the tab stops being a picture of an FT8 session and becomes one, and tomorrow morning at 14.074 there is something on screen to be right or wrong about.
ADVANCES: Step 7. Real decodes on the Digital tab and the mock-up rows removed. The step is not expected to close in this unit and the arbiter continues.
END-ARBITER-DECISION
```
