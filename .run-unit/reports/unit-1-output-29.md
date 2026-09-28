READ IN THIS ORDER.

A. The tolerance as a formula, and the three synthetic results: the 15 dB
   tone 51 -> 40 unmarked key-down hops of 208, still red; the 10 dB tone
   208 of 208 unmarked, red, because it never makes a first pair; the noise
   count 0 of 1500, unchanged.
B. Step 12: 12.1 to 12.3 re-ticked from units 477 and 478's reports; 12.4
   moved by two changes (the tolerance, and the meter's swing bar 17 -> 15)
   and not ticked - it is judged by the owner's ear.
C. The rest. Section 4 raises 6 items, none blocking. No recording was read.

UNIT:       479 - complete at task 3 of 3, none dropped - 2026-09-28 12:19
PHASE GOAL: Hamlet decodes CW the way the requirements say it must; for now that means finding a keyed station in the audio the way the owner's ear does, with no recording as the judge
UNIT GOAL:  weaker stations, 10 to 15 dB over the noise, make bars on the scope and light the meter, without noise making bars
ADVANCES:   step 12 criterion 4
ADVANCED:   no - 12.4 stays open; the 15 dB tone improved (51 -> 40 unmarked hops) and the meter's bar moved, but a 10 dB tone still makes no bars and nothing has been judged by ear yet
NUMBER:     tolerance: 1.5 fixed -> max(1.5, wobble at measured contrast); ConfidentSwingDb 17 -> 15; 15 dB tone's dah hops unmarked: 51 -> 40; recordings read: 0
DRIFT:      3 consecutive units without advance (was 2 - the instruction carried no count, so it is taken from PHASE_OUTCOME.md: 476 yes, 477 no, 478 no)

## 1. What Claude did

**Complete: 3 of 3 tasks, none dropped.** The two synthetic tone tests the instruction wanted green are still red; both are left red and named below. Provenance: QUIVERFULL, Hamlet (the gate's five checks held), branch `main`, every task committed and pushed (`75ed1da7`, `4058ee4b`, `9e2c0eea`, plus this report's commit). I was launched with the launcher's `SESSION.lock` and an empty `STOP` already present. I did not take, release or remove either.

**Task 0 - the record, the ticks, the entry round.**
- `PHASE_OUTCOME.md` (both copies) has `## UNIT 479 - STEP 12`.
- `PHASE_STATUS.md` (both copies) names 479 and `CURRENT_STEP: 12`. The launcher's edits to the root copy were carried as found.
- Version 1.13.164 -> 1.13.165.
- HM-DEC-187 is at the top of `DECISIONS.md`, and its row is at the top of `CLAUDE.md` §1.
- 12.1, 12.2 and 12.3 are re-ticked in both `PHASE_PLAN.md` copies, each with its report's counts beside it:
  - 12.1 from 477: ABarIsALevelThatHoldsTests 3/3, AMarkIsTheEnvelopeOverAThresholdTests 5/6.
  - 12.2 from 478: TheScopeIsTheMiddlePictureTests 4/4, TheScopeShowsTheMarksTests 5/5.
  - 12.3 from 478: TheVerdictCarriesTheScopeTests 3/3, TheOwnersVerdictIsARowTests 7/7.
- Entry round:
  - Build: 0 warnings, 0 errors, with warnings as errors.
  - App carry-forward line: 275/278, then 278/278 on the one re-run. All three first-run failures were "You've caused dispatcher loop", counted neither way (HM-DEC-155).

**Task 1 - the tolerance follows the contrast.**
- **Watched it fail first.** `AToneFifteenDecibelsOverTheNoiseStillKeys` was red at HEAD: 51 of 208 key-down hops unmarked.
- **The formula is now in.** `CwEnvelopeDetector.ToleranceDb(S) = max(FlatToleranceDb, 20·log10(1 + 10^(-S/20)))`. The floor stays at 1.5 and there is no ceiling. Run membership uses it.
- **Applied literally, it cannot move the 15 dB case.** Over the gaps' power mean that tone measures about 16 dB of contrast. The formula gives 1.28 dB, which is under the floor, and the test stayed at exactly 51. The instruction's own numbers show the same thing: "1.4 at 15" is also under 1.5.
- **So the decisions below are mine**, author's and overrulable, all written into the constant's remark:
  1. **S for a keying bin** is its bars' mean level over the *loudest hop of its paired gaps* in the last second, keeping clear of the key edges. Reason: the formula's noise term is the noise that adds to or takes from the tone at one hop, and a run has to hold through the loudest noise it meets.
  2. **Before a bin has its own contrast**, S is the run's level (with the new hop in it) over the loudest such gap any bin has. Where no bin has one, it is the run's level over the bin's own lowest level in the last second. **Both of these are exactly as the instruction names them.**
  3. **A run at or under its reference (S <= 0) holds the floor.** Without this, runs at noise level took tolerances of 6 to 20 dB and swallowed the key edges.
  4. **When a bin's contrast is first measured, its stored history is re-read at that contrast**, so the first dah is judged the same way as later ones.
- Pairing, the gap ceiling and the choice of pitch still use the 1.5 floor.
- **Tried and taken out**, each measured worse:

  | Tried | What happened |
  |---|---|
  | The loudest hop over every non-bar hop | Split dah pieces counted as gap; loud tones lost 35 to 46 hops |
  | Tolerance from the hop's own level | Low hops joined bars |
  | The bin's power mean as the reference before the first gap | Noise read keying in 106 of 1500 reads |
  | Pairing bars at the wider tolerance | No change; reverted |

- **Results.** All synthetic; no recording read.

  | Case | HEAD | Now |
  |---|---|---|
  | 15 dB tone, unmarked key-down hops | 51 of 208 | 40 of 208, still red |
  | New 10 dB tone | - | 208 of 208, red |
  | Loud noise, 117 bins, 30 s | 0 of 1500 reads keying | 0 of 1500 |

  - **15 dB:** the misses are the first dah (35 hops) plus 5 hops at the edges of the last two elements. Every other element is an unbroken bar.
  - **10 dB:** at the 1.5 floor it never makes a first pair, so no contrast is ever measured for it, and the bin's own lowest level reads its contrast as about 30 dB.
  - **Noise:** the instruction's "stays at its count or says by how much it rose" is met: it stayed.
- The loud-tone and scope tests all hold:
  - AMarkIsTheEnvelopeOverAThresholdTests 5/7 (the two red cases above)
  - ABarIsALevelThatHoldsTests 3/3
  - TheScopeIsTheMiddlePictureTests 4/4
  - TheScopeShowsTheMarksTests 5/5
  - TheVerdictCarriesTheScopeTests 3/3
- The AMark test's output now prints a hop-by-hop mark picture and the bins. That is output only; no assertion changed.

**Task 2 - the meter's swing bar.**
- **To test a profile without audio,** I pulled the meter's window decision out whole and unchanged into `CwKeyingMeter.LooksKeyed(KeyingProfile)`. `Update` now calls it.
- **Watched it fail first.** New `TheSwingBarIsTheLowestTheOwnerHeardTests` uses a profile with score 0.20, element median 45 ms and swing 15.5 dB. It was red at 17.
- **Then `ConfidentSwingDb` went 17 -> 15** and the test went green, 2/2. The same profile at a 14.9 dB swing is still refused.
- The remark now says:
  - 475 set 17 from seven rows; 479 sets 15 from four more, the lowest swing on a station the owner heard being 15.1.
  - This is the gate 477's task 3 was to replace and did not.
  - What 15 gives up, named.
- TheOwnersRefusedStationIsKeyingTests 1/1.
- **Not run, because each reads a recording:** CwKeyingMeterTests, TheMeterRunsOnLiveAudioTests, TheSwingIsTheFigureThatHoldsTests, TheTwoPitchesTableTests, TheKeyingWitnessSaysNothingImpossibleTests.

**Task 3 - the exit round.**
- **Build:** full no-incremental build of `Hamlet.sln` with warnings as errors: 0 warnings, 0 errors.
- **App carry-forward line:** 276/278, then 277/278 on the one re-run. Every failure was "You've caused dispatcher loop", counted neither way:
  - first run: BindingHealthTests and TheWindowHoldsBelowItsMinimumTests
  - re-run: Unit376TheTopBandTests
- **Every touched type that reads no recording, re-run at exit:** results as above.
- **`src/Hamlet.RadioEngine/Cw` against entry `73622034`:** only `CwEnvelopeDetector.cs` and `CwKeyingMeter.cs` changed.
  - `CwEnvelopeDetector.cs`: the tolerance formula and the contrast it reads.
  - `CwKeyingMeter.cs`: the constant 17 -> 15 and the pure `LooksKeyed` extraction.
- **The eleven transmit files** print nothing against `7e209cb4`.
- Nothing under `tests/fixtures` was touched. **No recording was read.**

## 2. What the owner should expect

**Rebuild, then tune the weaker stations you pressed *You're an idiot* on this afternoon, and look for bars.** Here is what will look wrong but is expected:
- **A station around 15 dB over the noise** should now get unbroken bars from its second element on.
- **Its very first dah may show no bar.** Hamlet has not yet measured how loud that station is against its gaps, so it holds the first dah to the old 1.5 dB.
- **A station around 10 dB over the noise will very likely still show nothing.** On synthetic audio it never makes the first pair that the wider tolerance depends on (section 4, item 2). If you press *idiot* on one, that row is useful evidence.
- **The meter now says keying from a 15 dB swing instead of 17.** Stations swinging 15 to 17 that it refused this afternoon should now light it.

**If noise now shows bars where nothing is there, or the meter says keying on an empty stretch, that is the one thing to press *idiot* on.** Fifteen sits inside the swing the old empty test windows showed (14.7 to 17.7), so an empty band calling keying is the risk this change took on.

## 3. What you should see

**Weaker stations partly: yes at about 15 dB, no at about 10 dB.** The evidence is synthetic only:
- **15 dB:** a keyed 15 dB tone now shows as unbroken bars on every element after its first dah. That is 40 of 208 key-down hops without a mark, down from 51.
- **10 dB:** a keyed 10 dB tone still shows no bars at all.
- **Noise:** loud noise alone still made nothing: 0 of 1500 reads.

On the CW tab:
- The scope's bars should hold across the dahs of weaker stations, where before they broke into pieces.
- The meter should light on stations with a 15 to 17 dB swing.

## 4. What's blocking us

Nothing here blocks the loop.

1. **What S is measured against.**
   - **Ruling wanted:** accept S as the bar over the loudest hop of its paired gaps, or rule another definition.
   - **Reasoning:** over the gaps' average, the formula gives 1.28 dB at the 15 dB tone's 16 dB. That is under the 1.5 floor, so R93 as written changes nothing there. The instruction expected that case to go green.
   - **Rejected:** the literal average, because it left 51 unchanged. The loudest hop over every non-bar hop, because split dahs counted as gap and loud tones lost hops.
2. **How a weak station makes its first pair.**
   - **Ruling wanted:** how a bin with no measured contrast should estimate one, or what tolerance it gets before its first pair.
   - **Reasoning:** the instruction's fallback, the bin's own lowest level in the last second, sits about 20 dB under the noise. It reads a 10 dB tone as about 30 dB of contrast, which holds it to the floor, so it never pairs and never earns the wider tolerance. That is the 10 dB test's whole failure.
   - **Rejected:** the bin's own power mean, because noise then read keying in 106 of 1500 reads.
3. **The 15 dB test is still red: 40 hops.**
   - **Ruling wanted:** accept this as the formula's limit, or rule a fix.
   - **Reasoning:** the misses are the first dah and a few edge hops. Re-reading the history at the measured contrast did not recover the first dah in the bin being watched. At about 13 dB over the gaps' loudest hop, the formula allows 1.8 dB, and a first dah that the floor split stays split.
   - **Rejected:** widening pairing to the tolerance, which made no difference.
4. **Fifteen sits inside the empty windows' 14.7 to 17.7.**
   - **Ruling wanted:** none now. The owner's rows are the check, and the bar test that 477's task 3 owed is still the replacement for this gate.
   - **Reasoning:** the meter's own history names eleven empty six-second windows that clear its score and element range. More of them are now called keying.
   - **Rejected:** running the recordings to measure it, because R88 bans that.
5. **12.1's criterion text is false of the tree** (carried from 477).
   - **Ruling wanted:** rewrite 12.1 (and 12.2) to the bars and the trace, or untick them.
   - **Reasoning:** this unit re-ticked them as directed, but 12.1 still says "a tracked noise floor, a threshold of floor plus a margin", which 477 removed. 12.2 still names the floor, threshold and passband shading, which 478 removed. The launcher's judge read 12.2 against its old text for exactly that reason.
   - **Rejected:** editing criterion text myself, because the plan is the owner's.
6. **The two `PHASE_STATUS.md` copies differ.**
   - **Ruling wanted:** which copy the launcher owns.
   - **Reasoning:** the root copy carries the launcher's `HEARTBEAT` line and its own step states (steps 2 to 7 partial; 9, 11 and 12 present). The docs copy has steps 2 to 7 not started, and no 9, 11 or 12. I carried both as found and changed only `CURRENT_STEP` and `WORK_INSTRUCTION`.
   - **Rejected:** reconciling them, because that would be repairing unrelated things on the way past (§12.6).
