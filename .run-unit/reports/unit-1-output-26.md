READ IN THIS ORDER.

A. What the owner will see: under the pitch strip on the CW tab there is now a four-second
   oscilloscope, with the newest audio at the right. A thin trace is the energy across the
   radio's filter. A dashed line under it is the floor, the noise's average level between marks.
   A heavier solid line above that is the threshold, labelled "threshold: floor + 9 dB". Wherever
   the trace stands over the threshold, a bar lights along the bottom, labelled with its length
   ("mark 180 ms"). While a bar is up, the tone line reads the pitch from the spectrum
   ("tone 750 Hz, 39 dB over the band"); when nothing is up it says "no tone". A shaded band at
   the top shows where the energy is summed ("filter 500 Hz at pitch 742 Hz: 492 to 992 Hz").
   If the radio hasn't reported its pitch and filter, it says "rig filter unknown: whole band
   100 to 3000 Hz".
B. Step 12. 12.1, the detector: built, watched red then green, ticked. 12.2, the scope: built,
   watched red then green, and NOT ticked, because "the owner sees dits" is his eye's call
   (section 4). 12.3, the row: eight scope fields on every verdict press, the key set closed,
   ticked. 12.4 is not authored (R90).
C. The rest. Section 4 raises 6 items, none blocking. Two bear on B: 12.2 is left for the
   owner's eye, and the floor is tracked differently from the instruction's sketch, because the
   sketch marked 127 of 800 hops of pure noise. No recording was read.

UNIT:       476 - complete at task 4 of 4, none dropped - 2026-09-28 09:09
PHASE GOAL: Hamlet's CW receive meets CW_REQUIREMENTS.md. With the corpus banned, the owner's ear at the radio is the judge of whether it hears CW.
UNIT GOAL:  Give Hamlet a detector that finds a keyed signal the way an oscilloscope would: the envelope over a threshold above the noise, at any pitch. Put it on screen and on the verdict row so his ear can judge it before it drives anything.
ADVANCED:   yes - step 12 criterion 1: CwEnvelopeDetector exists, drives nothing, and a synthetic test was watched red 6/6 on a stub and is green 6/6.
ADVANCES:   step 12 criterion 1
NUMBER:     threshold margin: 9 dB from seven owner rows; pitch search: whole passband; recordings read: 0
DRIFT:      0 consecutive units without advance - was 0

## 1. What Claude did

**Complete: tasks 0 to 4 all done, none dropped.** Claude Code on QUIVERFULL, project Hamlet
(gate confirmed: SHACK_FACTS.md, CwProbabilisticDecoder.cs and CW_REQUIREMENTS.md present, no
CoreHMI.sln or MURC.sln), branch `main`. Each task was committed and pushed before the next one
started: 1f2737fe (task 0), ff956b24 (task 1), 1d398861 (task 2), a0e8b464 (task 3), and the
task 4 commit that carries this report.

**Task 0, the record and the entry round.**
- `## UNIT 476 - STEP 12` was appended to both copies of `PHASE_OUTCOME.md`, from the
  instruction's block.
- Both copies of `PHASE_STATUS.md` now name 476 and `CURRENT_STEP: 12`.
- Version 1.13.161 to 1.13.162.
- HM-DEC-185 is the newest entry in `DECISIONS.md`, and its row is at the top of `CLAUDE.md`
  section 1, dated 2026-09-28, with the instruction's headline.
- Build: 0 errors.
- App carry-forward line: 275 of 278, then 278 of 278 on the one allowed re-run. The first pass
  lost three names to the dispatcher loop, at 1 ms each.

**Task 1, the detector (12.1).** The new file is
`src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs`. It takes audio in any chunk size and walks
it at the decoder's own 5 ms hop (`SampleRate / 200`). For each hop it computes:
- **Envelope:** the energy summed over the passband, in dB below full scale (a full-scale sine
  reads -3 dB). It comes from a Hann FFT of about 10 ms (512 samples at 48 kHz). The passband
  is the radio's `CwPitch` plus and minus half its `FilterBandwidth`. Where either is unknown,
  it is 100 to 3000 Hz.
- **Floor:** in one sentence, the floor is the power average of every hop that is not a mark,
  with a 0.25 s time constant, so no mark ever enters it. While a mark is up it may climb at
  most 3 dB/s, and the first 0.25 s only builds it and judges nothing. Neither constant was
  chosen from a recording, and each remark says what it rests on:
  - 0.25 s settles well inside a word space at 5 wpm (1.68 s).
  - 3 dB/s lifts the floor only 2.2 dB over a 5 wpm dah.
- **Threshold:** the floor plus `ThresholdMarginDb = 9`. Its remark says 9 is half the smallest
  swing on the owner's seven rows, and that nothing else chose it.
- **Mark or gap, and the run:** how long the current mark or gap has lasted, in ms.
- **Pitch and contrast, while a mark is up:** the pitch is the strongest bin of a longer FFT of
  about 43 ms (23.4 Hz bins at 48 kHz), searched across the whole passband, not 300 to 900 Hz.
  The contrast is that bin over the passband's median bin, with the peak's own main lobe left
  out.
- **History:** the last four seconds, and a count of the marks in them.

It drives nothing, and no decoder, tracker, survey or meter file was touched.

`AMarkIsTheEnvelopeOverAThresholdTests` feeds a CQ keyed at 20 wpm over seeded Gaussian noise,
in the app's 960-sample chunks. It was red 6/6 on a stub, then green 6/6:

| case | key-down hops missed | key-up hops marked | pitch read |
|---|---|---|---|
| 742 Hz, 500 Hz filter, 28 dB over noise | 0 of 200 | 0 of 292 | 750 |
| 1250 Hz, rig filter unknown (whole band) | 0 of 200 | 0 of 292 | 1242 |
| 600 Hz, 15 dB over noise | 0 of 200 | 0 of 292 | 609 |
| noise alone, 4 s | - | 0 of 800 | "no tone" |

- The longest run measured 185 ms for a 180 ms dah.
- The mark count was 8 for 8 keyed marks.
- The threshold is the floor plus 9 dB on every hop, and a mark is exactly envelope over
  threshold.

**A decision I made myself, reproduced in full:** the floor rule. The instruction sketches it
as "falling quickly when the envelope drops, rising slowly" and leaves the rule to the author.
- I built that literally first: the floor dropped at once to any quieter hop and rose 3 dB/s.
  It then sat on the deepest dips of a 10 ms envelope in a 500 Hz filter, and ordinary noise
  stood 9 dB over it. **127 of 800 hops of noise alone were marks.**
- I replaced it before keeping anything with the mean-of-the-gaps rule above. That rule marks
  0 of 800 on the same noise.
- It still falls quickly, within about half a second, and it still cannot be dragged up by a
  mark.
- The first-quarter-second build exists because a floor seeded from one hop sat in whatever
  trough that hop fell into. The next noise peak then made a false mark, which the test caught.

Both points are in the constants' remarks and in section 4.

**Task 2, the oscilloscope (12.2).** New file: `src/Hamlet.App/Controls/CwScopeControl.cs`,
under 474's strip on the CW tab (`CwScopeRow` in `MainWindow.axaml`). What it draws is in A
above.
- **Words on every mark (§0.6):**
  - the three lines differ in shape (thin, dashed, heavy), and each is named at its right-hand
    end;
  - every bar carries its length where it fits;
  - the tone line is on the plot and repeated on the text line beneath it;
  - the passband rail is labelled with its numbers.
- **Hover:** the tip says what each line is, that the threshold is "the one number that decides
  a mark", and that the picture is redrawn 20 times a second.
- **View model:** `CwHearingViewModel` gains `CwScopeFrame`, `Scope`, `ObserveScope` and
  `ScopeTip`. Nothing of 474's was removed or changed there.
- **Feed:** `MainWindowViewModel` makes a `CwEnvelopeDetector` on the same audio source the
  decoder listens to, stops it with the decoder, and runs a 50 ms timer. On each tick the timer:
  - hands the detector the rig's CW pitch and filter width, but only in CW or CW-R; in other
    modes it hands nothing, so the whole band is used;
  - puts the detector's history on screen.
- **Fed at 20 per second, as the instruction allowed:** every 5 ms hop is in every frame and
  drawn at its true width. Only the redraw runs at 20 per second, not 200.

`TheScopeShowsTheMarksTests` drives a real detector with synthetic keying. It was red 6/6 on
stubs, then green 6/6:
- 8 bars for 8 keyed marks, the dah about 3 times the dit;
- 4.0 s across the width, newest at the right;
- the tone line reads "tone 7xx Hz, n dB over the band" mid-dah and "no tone" in the gap;
- the threshold and passband labels are right, with and without the rig;
- the hover is as described;
- headless, the scope is on the CW tab, below the strip, and paints without throwing.

Run alone beside it, all green: BindingHealthTests 1/1, EveryControlSaysWhatItDoesTests 3/3,
WhatEveryControlSaysOnHoverTests 1/1, TheStripShowsWhereTheDetectorLooksTests 7/7, and
TheLightSaysWhatHamletThinksItHearsTests 5/5.

**Task 3, the verdict row (12.3).** Each press of *I agree with you* or *You're an idiot* now
also writes these fields from the scope's reading at the press, which is at most one 50 ms
redraw old: `scopeEnvelopeDb`, `scopeFloorDb`, `scopeThresholdDb`, `scopeMark`, `scopeRunMs`,
`scopePitchHz`, `scopeContrastDb` and `scopeMarksLast4s`. A figure that wasn't measured is
written as null, never NaN.

`TheVerdictCarriesTheScopeTests` was red 3/3 (18 keys where 26 were wanted), then green 3/3:
- **Pressed 90 ms into Q's first dah:** mark true, run 70 to 110 ms, pitch within a bin of
  742, envelope over the threshold, 5 marks.
- **Nothing listening:** null, false and 0.
- **In the gap after C:** no pitch, 4 marks.
- The row still serializes as one line of 26 fields.

The closed key list in 474's `TheOwnersVerdictIsARowTests` gains the same eight names, and it
passes 6/6. That is the only change to anything 474 built (section 4).

**Task 4, the exit round.**
- **Build:** a full `--no-incremental` build of `Hamlet.sln` with warnings as errors gave
  0 warnings and 0 errors.
- **App carry-forward line:** 278 of 278 on the first run.
- **Touched types, each run alone, all green (37 tests):**
  - AMarkIsTheEnvelopeOverAThresholdTests 6/6
  - TheScopeShowsTheMarksTests 6/6
  - TheVerdictCarriesTheScopeTests 3/3
  - TheOwnersVerdictIsARowTests 6/6
  - TheStripShowsWhereTheDetectorLooksTests 7/7
  - TheLightSaysWhatHamletThinksItHearsTests 5/5
  - BindingHealthTests 1/1
  - EveryControlSaysWhatItDoesTests 3/3
  - WhatEveryControlSaysOnHoverTests 1/1
- **None of the touched types reads a recording.**
- **`src/Hamlet.RadioEngine/Cw` diff against entry ba7ef00a:** one new file,
  `CwEnvelopeDetector.cs`, and nothing changed. Besides it, `src` has the new
  `CwScopeControl.cs` and additions to `CwHearingViewModel.cs`, `MainWindowViewModel.cs` and
  `MainWindow.axaml`.
- **The eleven transmit files print nothing against 7e209cb4.**
- **Nothing under `tests/fixtures` was touched, and no recording was read.**
- **Plan:** 12.1 and 12.3 are ticked in both copies of `PHASE_PLAN.md`. 12.2 and 12.4 are not.

**Left as found.**
- The session started with `SESSION.lock` and an empty `STOP` already at the root. The
  instruction says "seed it and drop STOP", so both are the launcher's and the owner's; neither
  was taken, released or removed.
- `RUN_LEDGER.md` and `tools/arbiter/` were not touched.
- The launcher's own edits under `.run-unit/` (reload, step-states, watched.*) were left
  uncommitted, as prior units left them.

## 2. What the owner should expect

**Rebuild, tune a station, and watch the scope under the pitch strip.** As you hear dits and
dahs, bars should light along the bottom under the trace, and the tone line should read a
pitch. Press the two buttons as before. Every press now also records what the scope saw at
that moment.

**What to look for:**
- **Bars where you hear keying.** Each bar should be about as long as the element you hear, and
  dahs should be about three times the width of dits.
- **No bars on silence or plain band noise.** The trace should wander just above the dashed
  floor, well under the solid threshold.
- **The tone line where your ear puts the pitch,** to within about 23 Hz. That is the width of
  one spectrum bin at 48 kHz, so a 742 Hz tone may read 750.
- **At any pitch the filter passes,** including well above 900 Hz, where the old tracker never
  looked.

**What will look wrong but is not:**
- **A steady carrier lights a bar that then goes out.** The floor climbs 3 dB a second while a
  mark is up, so a carrier stops being a mark after about three seconds. That is on purpose: a
  carrier is not keying.
- **Nothing lights for the first quarter second after listening starts.** The floor is being
  built.
- **The scope lights while you transmit.** The decoder is suspended during your own sending, but
  the scope shows whatever audio arrives, including your sidetone (section 4).
- **The transcript, the light and the strip behave exactly as before.** The scope drives
  nothing yet. Wiring it into the decoder is 12.4, after your ear has judged it.
- **In USB or any mode other than CW, the rail says "rig filter unknown: whole band 100 to
  3000 Hz".** The scope only takes the radio's CW pitch and filter width while the radio is in
  CW or CW-R.

## 3. What you should see

**The answer to what this unit was commissioned to ask: yes.** Hamlet now has a detector that
finds a keyed signal as the envelope over a threshold, at any pitch. On synthetic keying at
742, 1250 and 600 Hz (the last only 15 dB over the noise), it lit a mark on every key-down hop
and on no key-up hop, and it read the pitch to within one bin. Four seconds of noise alone lit
nothing.

On the CW tab, under the pitch strip, there is now a small oscilloscope. It has:
- the trace;
- a dashed floor;
- a solid threshold labelled "floor + 9 dB";
- green bars along the bottom for every mark, each labelled with its length;
- a line under it saying "tone 742 Hz, 24 dB over the band" or "no tone".

Nothing about what gets decoded has changed. Whether the bars land where you hear keying on
the air is the question this unit hands to you.

## 4. What's blocking us

None of these halts the unit. Most-blocking first.

1. **12.2 is built and tested, and left unticked for your eye.** Ruling asked: tick 12.2 once
   you have watched dits light up on the air, or tick it now on the headless test.
   Reasoning: every clause of 12.2 has a test, but "fed fast enough that the owner sees dits" is
   a claim about your eye, and R90 puts the scope on screen so your ear and eye can judge it.
   Every 5 ms hop is drawn, the picture is redrawn 20 times a second, and a 60 ms dit is
   12 hops. Rejected: ticking it on the session's own word, because that is the thing R90
   reserves for you.

2. **The floor is the mean of the gaps, not "down at once, up slowly".** Ruling asked: keep the
   mean-of-the-gaps rule. Reasoning: the literal rule was built first. It sat on the noise's
   deepest troughs, and 127 of 800 hops of synthetic noise alone were marks. The mean-of-the-gaps
   rule gives 0 of 800, still falls within about half a second, and still cannot be dragged up
   by a mark. It uses two constants, 0.25 s and 3 dB/s, and neither was fitted to a recording.
   Rejected: keeping the literal rule and raising the margin to hide the false marks, because
   that would move the one number the instruction takes from your rows.

3. **One change to what unit 474 built: eight names added to the closed key list in
   `TheOwnersVerdictIsARowTests`.** Ruling asked: accept it. Reasoning: task 3 extends the row,
   and that test asserts the row's key set is closed, so it would have gone red. Nothing was
   removed, and its 18 original fields are asserted exactly as before. Rejected: leaving that
   test red, or loosening it to "contains", which would stop it catching an extra field.

4. **The scope keeps listening while you transmit.** Ruling asked: blank the scope while the
   radio is transmitting, or leave it showing. Reasoning: the decoder suspends itself during
   your own sending (HM-DEC-147). The scope shows whatever audio arrives, so your sidetone will
   light bars, and `scopeMarksLast4s` on a press right after you send will count them. It is
   left showing, because an oscilloscope showing the wire is true, and the instruction said to
   observe the same audio. Rejected: suspending it in this unit without a ruling, because that
   would make the scope decide something.

5. **The pitch is the centre of the strongest bin, 23.4 Hz wide at 48 kHz, with no
   interpolation.** Ruling asked: is one bin fine enough for 12.4? Reasoning: 742 reads 750,
   1250 reads 1242, and 600 reads 609. The decoder's own bandwidth is wider than that, but if
   12.4 is to mix at the scope's pitch, interpolating between bins would roughly halve the
   error. Rejected for now: adding interpolation that no criterion asked for.

6. **The fallback band, 100 to 3000 Hz, and "CW or CW-R only" are the author's, not radio
   facts.** Ruling asked: confirm, or name the band you want used when the radio hasn't said.
   Reasoning: the instruction leaves the drawn passband to the author when the rig state is
   unknown. Outside CW, the radio's CW pitch does not centre the filter, so using it there would
   be a guess. Rejected: taking the radio's pitch and filter in every mode.
