READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. What decides keying now: the scope's detector (`CwEnvelopeDetector`) calls a pitch keying when its level holds flat for a dit or longer, drops, and holds again at the same level within the last second. It uses no floor and no margin, and the bars must clear their gaps by more than the gaps' own measured wander. The moment the keying meter says keying at a pitch, the tracker mixes there on its next hop and tells the decoder a station is present, whatever the swing.
B. Step 12 and its exit criteria:
   - 12.1: the detector is rewritten as bars, not waves, and is green on the three synthetic tests.
   - 12.1, 12.2 and 12.3 are ticked from unit 476's work, as task 0 directed. 12.1's text in PHASE_PLAN.md still describes the old floor and margin (section 4, item 1).
   - 12.4: its first half, the tracker obeying the meter, is wired and green. It is not ticked, because the criterion waits on the owner's ear and on the detector driving the decoder.
   - Task 3, the bar test in the meter, was dropped whole.
   - 12.5 is not ticked: the app line lost names to the dispatcher loop on both runs.
C. What this report adds: the three synthetic tests' numbers, one of unit 476's tests left red with its cost named, and five places where this instruction does not match the tree. Section 4 raises 6 items, none blocking. No recording was read.

UNIT:       477 - complete at task 4 of 4, task 3 dropped (the named drop candidate) - 2026-09-28 10:20
PHASE GOAL: make Hamlet meet the CW requirements in CW_REQUIREMENTS.md and CW_SPEC.md, judged by requirement ids and at the end by Tim at the radio; while R88 stands, only steps 11 and 12 (the owner's ear and the oscilloscope) are authorable
UNIT GOAL:  a keyed station at any pitch is found by its bars, not by a floor and a margin, and the tracker mixes at the meter's pitch and flags keying within a hop of the meter finding it
ADVANCED:   yes - step 12 criterion 4's first half: the tracker now takes the meter's pitch and keying on the next hop (proved on synthetic audio), and the scope's detector decides by bars
NUMBER:     gates removed: floor and margin (the detector), plus the swing gate as the decider of the tracker's keying flag; the meter's own 17 dB swing gate stays because task 3 was dropped; tracker follows the meter within 1 hop; recordings read: 0
DRIFT:      0 consecutive units without advance (the instruction carried no count)

## 1. What Claude did

**Complete at task 4 of 4, with task 3 dropped.** Task 3 is the drop candidate the instruction named. Claude Code on QUIVERFULL, project Hamlet confirmed by the five gate checks, branch `main`, entry `4d1d24cf`. Every task was pushed: `83dd6e0e` (task 0), `b2b8fc87` (task 1), `ccea84d0` (task 2), and this report's commit for task 4. `SESSION.lock` and an empty `STOP` were already at the root when the session started. Both are the launcher's; the session did not take, release or remove either.

**Task 0: the record.**
- PHASE_OUTCOME.md (both copies) has `## UNIT 477 - STEP 12`.
- PHASE_STATUS.md (both copies) names 477 and `CURRENT_STEP: 12`.
- The version goes from 1.13.162 to 1.13.163.
- HM-DEC-186 is at the top of DECISIONS.md, verbatim, and its row is at the top of CLAUDE.md §1.
- 12.1, 12.2 and 12.3 are ticked in both PHASE_PLAN.md copies, as the instruction directs.
- Entry round: the build had 0 errors. The app line ran 277/278, then 276/278 on the one re-run. Every failure was "You've caused dispatcher loop", so neither run is counted.

**Task 1: bars, not waves.** `CwEnvelopeDetector` has no floor tracker, no margin and no floor-rise any more. The rule:
- **Bins:** every 25 Hz (`KeyingEnvelope.ToneStepHz`, the meter's step) across CwPitch ± half FilterBandwidth, or 100 to 3000 Hz when the rig state is unknown (117 bins).
- **Level:** each bin's level is read every 5 ms hop over a 10 ms Hann window.
- **Run:** consecutive hops within `FlatToleranceDb` = **1.5** of the run's own mean.
- **Bar:** a run whose windows cover at least `ShortestBarMs` = `CwToneSurvey.ShortestDitMs` (25 ms), standing wholly above the runs either side.
- **Pair:** two consecutive bars within a second form a pair when all of these hold:
  - the second bar holds the first's level within the tolerance;
  - the hops between them last at least a dit, and every one sits under the lower bar's band;
  - the bars clear the gaps' power-mean level by more than the gap hops' own wander. The wander is a standard deviation in dB, measured in that bin over the last second, with key-edge hops left out.
- **Keying:** a bin is keying when a pair ended in the last second.
- **Pitch:** the keying bin with the most paired bars in the last second. Only bins whose bars hold the loudest bar level within the window's ±200 Hz reach are counted, and the louder bin breaks a tie.
- **Scope:** it watches that bin. The trace is its level; the dashed line, "gap level, measured", is its gap level; the solid line, "midway: gap to bar, measured", is the midpoint. The hover says the solid line decides nothing and bars decide.
- `CwEnvelopeReading` gains `Keying`; `CwBarBin` and `Bins()` are new. The scope control no longer draws a line it has no measurement for.

**The tolerance and why (my decision, overrulable).** A tone S dB over its bin's noise wobbles by at most 20·log10(1 + 10^(-S/20)): 1.4 dB at 15 dB, 2.4 dB at 10 dB. The measurements that settled it, all on seeded synthetic audio in the tests and none on a recording or on the owner's rows:
- **2 dB:** with every other rule in place, 30 s of loud noise read as keying in 11 of 1500 reads.
- **1.5 dB:** the same noise read keying in 0 of 1500.
- **The price, named in the constant's remark:** a tone 15 dB over the noise in a 500 Hz filter (measured 16 dB over its bin's gaps) splits its long dahs. Unit 476's `AToneFifteenDecibelsOverTheNoiseStillKeys` misses **51 of 208** key-down hops at 1.5 dB (35 at 2 dB). **That test is left red.**

**The rules I added beyond the instruction's list, each reached only after the plainer rule failed a synthetic test:**
- The gap must last at least a dit, and the second bar must hold the first's level. Without these, noise made 9 chance bars a second and paired them.
- A bar must stand *wholly* above the runs either side.
- **The bars must clear the gaps' measured wander.** Without this, noise's chance flat runs, which sit at the noise's own level, paired into keying. It is the rule nearest to a margin; it is measured on the bin and set by no constant (section 4, item 4).
- The pitch is counted only among full-level bins. Without this, a bin 125 Hz off paired one hop before the tone's own bin and took the pitch.

**Red first, then green:**
- `ABarIsALevelThatHoldsTests` was red 3/3 on a stub, then green 3/3.
- `AMarkIsTheEnvelopeOverAThresholdTests` is 5/6. Its margin test became `TheLinesAreTheGapLevelAndTheMidpointMeasured`, and the 15 dB case is red as above.
- `TheScopeShowsTheMarksTests` is 6/6 after I changed three things in unit 476's test: the label strings, the hover phrase, and the mid-dah frame. The frame moved from C's first dah (0.89 s) to Q's first dah (1.73 s), because the first element of a transmission has no partner bar yet and is marked only when the next element pairs with it.
- `TheVerdictCarriesTheScopeTests` is 3/3 and unchanged.

**Task 2: the tracker obeys the meter.**
- `CwToneTracker.FollowMeter(KeyingReading)` takes the reading from any thread.
- While the verdict is Keying at a pitch, the tracker mixes there from its next hop, as a measured pitch the decoder takes: a `Switch` outside the fine bank's reach, the nearest fine bin inside it.
- Meanwhile the survey moves nothing: no admission, no held switch, no cold-start move. `HasKeying` is true at any swing.
- When the meter stops saying keying, the survey owns every move again.
- `MainWindowViewModel.PublishKeying` hands the same reading the light shows to `_decoder.Tracker`.
- The decoder is not changed.
- `ConfidentSwingDb` stays, and its remark now says the tracker's flag does not consult it.
- `TheTrackerObeysTheMeterTests` was red 3/3 on a stub:
  - the tracker held 500;
  - `HasKeying` was false at 18.6 dB;
  - the survey's cold-start rule dragged the tracker to 750.
- It is now green 3/3:
  - the tracker is at 600 on the next hop, as a measured pitch;
  - `HasKeying` is true at 375 Hz on 18.6 dB, and false again when the meter stops;
  - 1800 hops read 600 against a keyed 750.
- One limit: in that last test the survey admitted no candidate in 8 s. What it proves is that the survey's cold-start move is held off. An admission elsewhere goes through the same early return and is not exercised separately.
- I also decided that the move goes at once, even mid-character (section 4, item 5).

**Task 3: dropped whole.** The meter still gates on its swing. Three reasons:
- The instruction's precondition is "only if task 1 and task 2 are green", and task 1 left unit 476's 15 dB test red.
- `CwKeyingMeterTests` reads a WAV recording, so under R88 the meter's own tests cannot check a change to its verdict. The silence property HM-DEC-120 rests on also cannot be re-measured.
- The meter now drives the tracker, and the bar test has a measured weakness at about 16 dB over the bin's noise that the 17 dB swing gate does not have.

Task 2's wiring lands regardless.

**Task 4: the exit round.**
- **Build:** a full non-incremental build of Hamlet.sln with warnings as errors gives 0 warnings and 0 errors.
- **App line:** 274/278, then 276/278 on the one re-run. All six failures across the two runs are "You've caused dispatcher loop": the three `TheFavoritesAreUnderTheGreenZoneTests` tests and `ThePsk31OfferTests.TheOfferIsOneButtonAndItIsTheOneTheEngineNamed`. Neither run is counted.
- **Touched types, one invocation each:**

  | Type | Result |
  |---|---|
  | `ABarIsALevelThatHoldsTests` | 3/3 |
  | `AMarkIsTheEnvelopeOverAThresholdTests` | **5/6** (the 15 dB case) |
  | `TheTrackerObeysTheMeterTests` | 3/3 |
  | `TheScopeShowsTheMarksTests` | 6/6 |
  | `TheVerdictCarriesTheScopeTests` | 3/3 |
  | `TheOwnersVerdictIsARowTests` | 6/6 |
  | `TheLightSaysWhatHamletThinksItHearsTests` | 5/5 |
  | `TheStripShowsWhereTheDetectorLooksTests` | 7/7 |
  | `BindingHealthTests` | 1/1 |
  | `EveryControlSaysWhatItDoesTests` | 3/3 |
  | `WhatEveryControlSaysOnHoverTests` | 1/1 |

- **Not run because it reads a recording:** `CwKeyingMeterTests`. The meter's source changed only in a remark.
- **Transmit files:** print nothing against `7e209cb4`.
- **Fixtures:** nothing under `tests/fixtures` was touched.
- **`src/Hamlet.RadioEngine/Cw` against `4d1d24cf`: three files.**
  - `CwEnvelopeDetector.cs`: the floor and margin are replaced by per-bin bars, pairs, pitch and measured lines.
  - `CwToneTracker.cs`: `FollowMeter` is added, the survey holds while the meter says keying, and `HasKeying` follows the meter.
  - `CwKeyingMeter.cs`: a remark only, saying `ConfidentSwingDb` is not the tracker flag's gate.

**Where the instruction does not match the tree** (reported, not repaired):
1. `ConfidentSwingDb` is **17**, not 20 (R89, unit 475). The 01:16:22 profile at 18.6 dB already passes the meter's swing test, so task 3's red case would not have been red.
2. `CwToneTracker.HasKeying` never rested on the 20 dB swing gate. It was `Verdict.Keyed is not null`, the survey's admission: eight clean marks over three seconds, confirmed by two surveys.
3. The meter's sweep is 300 to 900 Hz at 25 Hz: `KeyingEnvelope.LowestToneHz` and `HighestToneHz`, taken from the tracker's own range. So the tracker obeys the meter only inside 300 to 900. The scope's detector sweeps the whole passband.
4. PHASE_PLAN.md's log says "12.1 rewritten in place", but 12.1's criterion text still describes "a tracked noise floor, a threshold of floor plus a margin".
5. The meter feeds the app once a second over a 6 s window, so "within a second" is the meter's cadence plus one hop.

## 2. What the owner should expect

Rebuild and tune a station at any pitch the filter passes. On the CW tab, watch three things.

**Bars under the trace as you hear keying.**
- The trace is now the level in the one pitch Hamlet found the most bars in, not the whole filter summed.
- A dit or a dah lights a bar only once there is a second element to pair it with, so the first element of an over appears a moment late. It appears when the second arrives, drawn where it happened.
- The dashed line is that pitch's measured gap level, and the solid line is midway to the bars. Neither decides anything. With no bars, neither line is drawn.

**The mixing line jumping to where the meter's line is within a second.**
- When the light says "somebody is keying" at a pitch, the strip's tracker line should move there on the next update, at any swing.
- The meter reads six seconds once a second, so the jump comes when the meter says keying, not before.
- The meter still only looks from 300 to 900 Hz.

**Characters.** The decoder was not touched. It is now mixed at the meter's pitch and told a station is present, so where the survey kept it on an empty bin it should now read the station. Then press.

**What will look wrong but is not:**
- A steady carrier shows no bars, and neither does plain noise however loud: no bar, no pairing, no tone line.
- A weak station, about 10 to 16 dB over the noise in a 500 Hz filter, may show broken or missing bars on long dahs. That is the tolerance's named cost (section 4, item 2).
- The meter itself still refuses a station swinging under 17 dB, because task 3 was dropped.

## 3. What you should see

**The answer this unit was commissioned for.** Bars decide keying on the scope, and the tracker follows the meter within one hop. The three synthetic tests (`ABarIsALevelThatHoldsTests`, seeded audio, no recording) print this:

```
keyed:   CQ at 650 Hz, 20 wpm, noise wandering 12 dB peak to trough, 500 Hz filter
         key down hops 208, missed 0; key up hops 359, marked 0
         marks in 4 s 8 (8 keyed), pitch 650 to 650 Hz on every mark
         bar -13.5 dB, gap -50.2 dB, contrast 35.9 dB
carrier: 700 Hz steady from 0.8 s: bars 1, gaps 0, keying False, keying seen False, marked hops 0
noise:   30 s loud (0.5 RMS), rig unknown, 117 bins: keying seen False, 0 of 1500 reads, marked hops 0
```

The tracker (`TheTrackerObeysTheMeterTests`):

```
before: 500 Hz, measured False
after one hop: 600 Hz, measured True, keying True
meter keying at 375 on 18.6 dB: HasKeying True, 375 Hz
meter listening: HasKeying False
survey admits: (nothing); tracker read 600 to 600 Hz against a keyed 750
```

**The case left red:** unit 476's 15 dB station, 51 of 208 key-down hops unmarked.

**Nothing visible changes in how text is decoded,** apart from the pitch it is decoded at and the keying flag.

## 4. What's blocking us

Nothing here blocks the unit. Six rulings are wanted, most consequential first.

1. **12.1 is ticked, but its text describes the detector this unit removed.**
   - **Ruling wanted:** rewrite 12.1's criterion text to the bar rule (runs, bars, pairs, measured lines, no floor or margin), or untick it until it is rewritten.
   - **Reasoning:** the instruction told task 0 to tick 12.1 to 12.3 from unit 476's work. Task 1 then replaced that work, so the tick now stands on text that is false of the tree.
   - **Rejected:** editing PHASE_PLAN.md's criterion text myself. The plan is the owner's and the web session's, and the instruction said to tick, not to rewrite.

2. **The flat tolerance, 1.5 dB, and the weak station it costs.**
   - **Ruling wanted:** accept 1.5, and rewrite or retire unit 476's 15 dB test to say what the bar rule does with a station that weak. Or go to 2 dB and accept noise reading as keying about once in 136 reads of 20 ms over the whole band.
   - **Reasoning:** at 2 dB, 30 s of loud noise read as keying in 11 of 1500 reads; at 1.5 dB, in none. A station 15 dB over the noise in a 500 Hz filter loses 51 of 208 key-down hops at 1.5 dB and 35 at 2 dB. §0.0 favours silence over a false station.
   - **Rejected:** fitting the tolerance to the owner's rows or a recording (R91, HM-DEC-186), and joining short same-level runs back into one bar. That joins noise too.

3. **The meter still gates on a 17 dB swing (task 3 dropped).**
   - **Ruling wanted:** whether the next unit replaces the meter's swing test with the bar test, now that the meter drives the tracker.
   - **Reasoning:** the instruction's red case (the 18.6 dB profile) already passes at 17. A station swinging under 17 is refused by the meter, so it now also does not move the tracker. The meter's own tests read a recording and cannot run under R88, so the change could not be checked against them here.
   - **Rejected:** doing it anyway on synthetic evidence alone, in a unit whose precondition for it was not met.

4. **"The bars clear the gaps' measured wander" is the rule nearest to a margin.**
   - **Ruling wanted:** whether it stands under "no floor, no margin, no swing threshold".
   - **Reasoning:** without it, noise's chance flat runs paired into keying in every form I tried. It has no constant; the number is each bin's own gap wander over the last second. For noise that is about 5.6 dB, and for a station it is whatever its gaps measure.
   - **Rejected:** a fixed decibel figure for how far bars must stand over gaps. That is the margin HM-DEC-186 removed.

5. **The tracker moves to the meter's pitch even mid-character.**
   - **Ruling wanted:** whether HM-DEC-186's "at once" overrides HM-DEC-096's never-switch-mid-character for meter moves.
   - **Reasoning:** the ruling says "at once". A tracker waiting on the survey is usually mixing an empty bin, where there is no character to protect. HM-DEC-096 still governs every survey move.
   - **Rejected:** holding the meter's move until the character ends. The decoder's `InsideCharacter` on an empty bin could hold the move for as long as noise looks like a character.

6. **While the meter owns the pitch, `PitchProof` reads Hypothesis.**
   - **Ruling wanted:** whether a pitch set by the meter's verdict should read Proved.
   - **Reasoning:** Proved is defined as the survey's keyed verdict matching the reported pitch. While the meter holds the pitch, the survey's verdict is cleared, so the pitch the decoder uses is labelled a hypothesis. That is the less confident claim, and harmless under §0.0, but it undersells the meter.
   - **Rejected:** redefining Proved in this unit. The instruction said to change only two things.
