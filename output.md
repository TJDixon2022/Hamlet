```
READ IN THIS ORDER.

A. PHASE GOAL - CW decodes again. Steps 1, 2 and 4 done, their criteria
   all met; step 3 partial on 3.6 alone; step 5 is Tim's.
B. THIS STEP - step 3, the inherited reds. 3.1 to 3.5 met; 3.6, 4 of
   eight with a verdict - #6 went green on the tracker switch and #42 on
   the radio's word, both kept; #15 #43 #44 moved and stay open, #45 stays
   open, not attacked.
C. THIS REPORT - the red table leads section 3; section 4 raises 5 items
   and items 1, 2 and 3 stand in the way of 3.6. #45's fault is a
   placeholder the floors count and no red has three no-movement attacks,
   so whether 3.6 can close under the head's count rule is Tim's to weigh.
```

```
UNIT:       406 - complete at task 5 of 5, tasks 0 to 5, none dropped - 2026-09-23 14:54
PHASE GOAL: bring the engine's CW decoder back to what it read on the air on 2026-08-25, hold it there with three floor tests, clear the inherited reds, then have Tim judge it at the radio
UNIT GOAL:  find and fix the tone tracker's move that takes the mix off a single sender, and give #42's test the radio's own word that the operator is sending, keeping only a change that turns a red green and costs no recording
ADVANCED:   yes - step 3 criterion 6, from 2 to 4 of eight with a verdict; #6 and #42 green at kept commits with every floor row identical
NUMBER:     of 3.6's eight 4 with a verdict; this unit 2 green, 3 moved, 1 not attacked
DRIFT:      0 consecutive units without advance  (was 1)
```

## 1. What Claude did

**Complete at task 5 of 5. All of tasks 0 to 5 were done and none was dropped.** Machine
QUIVERFULL, project Hamlet, branch main, gate confirmed. Commits `cca21dea`, `b66a220e`,
`775907b6`, `099b3a2a`, `0fe55b79` and `13a09580` were each pushed, and each push succeeded.
Every section 5 item matched the tree. HM-DEC-127's text lives in the decision table of
`CLAUDE.md`, line 397, not in `DECISIONS.md`.

**Task 0, the entry round.** Version 1.13.92 to 1.13.93, and `PHASE_STATUS.md` set to 406 and
step 3. Under decision 7 the carry-forward lines at entry are unit 405's exit. The floors match
unit 405's exit row for row: captures 37 of 37, adjudicated 13 of 13, synthetics 2 of 2. The
reds: #6 0.75, #15 0.54, #42 70, #43 5 + 37, #44 3 + 21, #45 1 + 3. The tracker readers are the
grep for `CwToneTracker` plus `ThePitchCanBeHeldTests`, compiled only, ten types. Two of those
ten need a note:

- `WhatBandwidthTheDecoderListensThroughTests` is 4 of 6 at entry. Two reds, outside the 51-name
  set, are left alone.
- `TheGateHasItsOwnWindowNowTests` **did not fit 300 s, nor 590 s alone.** The test host was
  killed with no result both times. Its two short methods run 4 of 4 in 1 s.
  `EveryWidthLeavesTheEmptyRecordingsSilent` went unmeasured at entry and at exit.

**Task 1, the trace.** The printer `TheTrackerSwitchTraceTests` asserts nothing, and no `src`
file changed. Findings are in `docs/phase-cw/unit406-reds.md` section 2. The printer disturbs
nothing: #6, #15 and every capture print their test's own numbers.

- **The cause.** Of the ten moves away from a single sender, nine go through the hold at
  `CwToneTracker.cs` 959. That hold makes a deferred move *before* the survey is analysed, and
  it never checks that the survey still finds keying there. The comment at 1084 promises that
  re-confirmation.
- **7 of the 9 are stale holds.** The survey read at execution admits nothing, or admits only
  the sender's own bin.
- **No known-right switch is stale.** The two-station fixture's real handover is not a `Switch`
  call. It is a cold move at 1004.
- **#6's first 1.54 s** is the tracker's initial 600 Hz (`CwToneTracker.cs` 382), not a switch
  still to be made.
- **#42.** The radio's word on the audio clock takes `during` from 70 to 0. It also takes
  `OwnTransmitSeconds` from 9.08 to 0.00 s, because line 501 skips the guard along with the
  tracker. A wall clock never resumes decoding.

**Task 2, the tracker change.** Two shapes were tried:

- **H1**: a held switch goes only if the survey still admits the held pitch. It took #6 to 0.89
  and moved #15, #43 and #44. But it cost captures 35 of 37, with `031838` down 57 to 37: on
  real captures, stale holds moved onto the station. **Put back.**
- **H2**, the narrower shape at the same property: a held switch is dropped only when the
  survey finds the keying back at the bank already being listened to. #6 went from 0.75 to
  0.82 against 0.79. Every floor row, gate type, speed reader and tracker reader was identical
  to entry. **Kept, `775907b6`.**

**Task 3.** Two attacks:

- **#42, kept at `099b3a2a`.** Its event pumps a chunk at a time and reports
  `RadioIsTransmitting` from the recipe's span, 1.00 to 13.00 s, on audio time. Both
  assertions and the 13 s window are unchanged. `CwDecoder.cs` got one change: while decoding
  is suspended, the transmit guard alone is handed each hop's level. #42 went from 70 to 0,
  with own transmit 9.5 s. Every floor row was identical.
- **G1 on top, put back.** It moved #44 from 3 + 21 to 4 + 16, but turned no red green.

**Task 4, the record.** Six attack-3 rows are in `reds-3.6.md`, and the attack table is in
`unit406-reds.md` section 3. The failing set's closing line now reads 47 green and 4 red-open,
and 3.1's tick sentence has a clause for #6 and #42. **3.6 is not ticked.**

**Task 5, the exit round.** Engine line 178 of 178, app line 278 of 278. The floors and every
type are identical to entry, apart from #6 and #42 now green. The transmit files and
`src/Hamlet.App` print nothing. 3.5 is re-confirmed.

**Decisions I made myself, in full:**

1. **The `CwDecoder.cs` line for #42 is not a clock.** Decision 2's text says *if task 1 finds
   that the change needs a line under `src`, it goes in `CwDecoder.cs` only. One example is a
   clock...* The decision block's shorter wording says *allowed only if the trace needs an
   audio clock*. The trace needed no clock, because `nowUtc` is a parameter the test can fill
   with audio time. What it did need was the guard, because otherwise the unchanged first
   assertion goes red. I read the text over the block, made the change, and kept it under the
   keep rule. It is item 1 in section 4.
2. **H1's gate stopped at the floors.** Once captures fell to 35 of 37, the remaining gate
   types were not run for H1. It was out either way.
3. **G1's captures were run for the record.** Its reds had already sent it back.
4. **`HamletDoesNotDecodeYourOwnSendingTests`, 6 of 6, was run beyond task 3's list.** It
   exercises the suspended arm I changed.
5. **#45 is marked *not attacked*.** Its fixture makes no `Switch` call at all, so decision 5's
   condition applies. Its number did not move under either shape.
6. **`.run-unit` is not committed.** That covers this unit's scripts and logs, as the
   instruction says. An untracked capture, `tests/fixtures/cw/captured/unadjudicated/cw-2026-09-23-173723.wav`,
   appeared at 13:37 local, before the entry round. It is not this session's. I left it alone
   and did not commit it.

## 2. What the owner should expect

Two of the six reds now pass, and nothing else moved. #6 is the fast fist with no run-up. Its
share went from 0.75 to 0.82 because the tracker no longer carries out a held move that the
current survey contradicts. That is how its second call used to lose three or four letters on
two of the three seeds. #42 is the operator's own full break-in preamble. Nothing comes out
during it now that the test tells the decoder the radio is transmitting, the way the radio
tells Hamlet on the air. The cost of that is the first `TE` after the preamble, which falls
inside the 500 ms resume and is not read. Every one of the 37 captures, 13 adjudicated
readings and 2 synthetics produced exactly the numbers it did at entry. #15, #43 and #44 moved
only under a first shape that cost two capture floors, so it was not kept. #45 did not move.

**What will look wrong but is not:**

- `OwnTransmitSeconds` in #42's output reads 9.5 s where it used to read 9.08. The guard now
  measures each 5 ms hop while decoding is suspended, instead of the tracker's 40 ms window.
- `TheTrackerSwitchTraceTests` appears in the test list and passes, because it asserts
  nothing.
- `reds-3.6.md` shows #15, #43 and #44 at *0 of 3* again. They moved, and the head's rule
  restarts the count on movement.

## 3. What you should see

| red | cause as traced | change | before | after | moved | kept | sequence |
|---|---|---|---|---|---|---|---|
| #6 | the tails on two seeds follow a stale held switch at `CwToneTracker.cs` 959 to 550 and 725 Hz, while the survey admits the 640 Hz sender's own bin | H1; **H2 at 957** | 0.75 | H1 0.89; **H2 0.82** | yes | **yes, `775907b6`** | **green, verdict** |
| #15 | seed 15485863: stale hold to 700 Hz; seed 104729: the survey re-admits a 700 Hz image about 23 dB down while the sender sends dahs only | H1, H2, G1 on H2 | 0.54 | H1 0.61; H2 0.54 | yes, under H1 | no: H1 cost captures 35 of 37 | 0 of 3, restarts |
| #42 | the test never gives the decoder the radio's report; given it, the guard goes blind while suspended | the event reports `RadioIsTransmitting` from the recipe's span on audio time; the suspended arm feeds the guard | 70 | 0 | yes | **yes, `099b3a2a`** | **green, verdict** |
| #43 | four stale holds off 615 Hz, each on a survey admitting nothing | H1, H2, G1 on H2 | 5 + 37 | H1 4 + 32; H2 5 + 37 | yes, under H1 | no: H1 cost captures | 0 of 3, restarts |
| #44 | stale hold to 575 Hz at 11.5 s on a survey admitting nothing; held gaps 15/1127/323 ms | H1, H2; G1 on H2 | 3 + 21 | H1 2 + 20; H2 3 + 21; G1 4 + 16 | yes, under H1 and G1 | no: H1 cost captures; G1 turned no red green | 0 of 3, restarts |
| #45 | no `Switch` call on its fixture; the trailing placeholder the flush settles | none of its own (decision 5) | 1 + 3 | 1 + 3 under H1, H2, G1 | no | not attacked | 0 of 3, broken |

**The answer to the question this unit was commissioned to ask:** the tracker's switch was the
cause for #6, and a change at its line turned #6 green at no cost. For #43 and #44 the same
line is the cause. But the one shape that reached them also blocked moves onto the station on
five real captures, so it was not kept. The radio's word turned #42 green. 3.6 went from 2 to
4 of eight with a verdict.

**Gate numbers for each kept change:**

| kept change | gate |
|---|---|
| H2, `775907b6` | captures 37 of 37, every row identical to entry; adjudicated 13 of 13; synthetics 2 of 2; `CwAcquisitionWindowTests` 11 of 12, only #6 changed; `CwReceiverFixtureTests` 23 of 27 identical; `CwFixtureTests` 22 of 23, `CwAdjudicationTests` 11, `CwEmissionGateTests` 8, `CwDisplacementFloorTests` 6, `CapturedSignalTests` 13, `CwSpeedSilenceTests` 4, `WhyTheGateDidNotFireTests` 2, `CwTwoStationTests` 5, each identical; tracker readers identical, `WhatBandwidth` 4 of 6 as entry at 48 against 50 |
| #42, `099b3a2a` | `CwReceiverFixtureTests` 24 of 27, only #42 changed; captures 37 of 37 every row identical; adjudicated 13 of 13; synthetics 2 of 2; `CwEmissionGateTests` 8 of 8; `HamletDoesNotDecodeYourOwnSendingTests` 6 of 6 |

**Exit round, at `13a09580`:**

| line or type | exit |
|---|---|
| engine line | 178 of 178 in 375 s |
| app line | 278 of 278 in 173 s, no dispatcher loss |
| captures, adjudicated, synthetics | 37 of 37 with every row identical to entry, 13 of 13, 2 of 2 |
| red-holding types | acquisition 11 of 12, #6 green; receiver fixtures 24 of 27, #42 green; fixture 22 of 23 as entry; adjudication 11 of 11 |
| gate, displacement, the four speed readers | each identical to entry |
| tracker readers | each identical to entry; gate-window reader's short methods 4 of 4, `EveryWidthLeavesTheEmptyRecordingsSilent` unmeasured |
| transmit files against `7e209cb4`; `src/Hamlet.App` from `527b1659` | nothing printed |

**Visible change:** in the app, a single station no longer loses letters when the tracker
carries out a held move that the current survey contradicts. While the radio reports
transmitting, the report's own-transmit time keeps counting. Nothing that keys or transmits
was touched.

## 4. What's blocking us

1. **The guard line in `CwDecoder.cs` goes beyond the decision block's wording.** Stands in the
   way of 3.6, because #42's verdict rests on it.
   - **Ruling asked:** does #42's green stand with `ObserveOwnTransmission` in the suspended
     arm? Or does the decision block's *only if the trace needs an audio clock* govern, which
     puts #42 back to red-open?
   - **Reasoning:** the radio's word alone empties the preamble but blinds the transmit guard,
     so the test's unchanged first assertion reads 0.00 s against > 3. The line feeds the guard
     each hop's level and nothing else. The survey still hears none of the sidetone, and
     nothing is suspended or resumed on it. It cannot reach a capture, because no capture calls
     `RadioIsTransmitting`, and every row is identical.
   - **Rejected:** changing `Report.OwnTransmitSeconds` to count the radio-reported time. That
     would satisfy an assertion about the guard without the guard noticing anything. Also
     rejected: feeding the whole tracker while suspended, which lets the survey retune to the
     sidetone, as `CwDecoder.cs` 503 to 507 forbids.
2. **#45 cannot move under the instruction as written.** Stands in the way of 3.6.
   - **Ruling asked:** is #45 to get a change of its own in a later unit? If not, how does it
     reach a verdict?
   - **Reasoning:** its fixture makes no `Switch` call, and its only fault is the trailing
     placeholder that the capture floors themselves count. Decision 5 gave it no change, so it
     is *not attacked*, and the head's rule breaks its sequence. It can reach neither green nor
     section 6's parking.
   - **Rejected:** B2 again. It is on section 10's list, and it costs six capture rows.
3. **#15, #43 and #44 have a traced line and no keepable shape yet.** Stands in the way of 3.6.
   - **Ruling asked:** may the next unit attack the HM-DEC-127 floor's reading level? And is a
     property that tells a stale hold onto the station apart from one off it to be sought, or
     parked?
   - **Reasoning:** #43's and #44's moves are stale holds on a survey that admits nothing. H1
     caught them, but on `031838`, `032113` and `032129` the same kind of hold moved onto the
     station, so the property does not separate the two. #15's seed 104729 is not stale. There
     the survey re-admits an image 23 dB below the sender, and HM-DEC-127's 25 dB floor does
     not refuse it. The floor compares against `_readingDb`, and line 1076 overwrites that with
     each confirmed candidate's own level, the image's included. That is a traced line for a
     next attack, inside a ruled decision.
   - **Rejected:** narrowing H1 to the reds' seconds or pitches. That would be fitting the
     shape to the rows it lost.
4. **The count rule, again.** Does not block this unit's result.
   - **Ruling asked:** none new. It stays logged for Tim under section 3 item 4 of the
     instruction.
   - **Reasoning:** #15, #43 and #44 have now moved in 402, 405 and 406. Under the head's rule,
     none of them will ever reach three no-movement attacks. So 3.6 closes only by greens, and
     #45 has no route to one (item 2).
   - **Rejected:** rewriting either count, as the instruction says.
5. **Two tracker readers do not fully measure.** Does not block.
   - **Ruling asked:** may `TheGateHasItsOwnWindowNowTests.EveryWidthLeavesTheEmptyRecordingsSilent`
     be run at a longer timeout outside the Bash tool's 600 s ceiling, or be left out of the
     tracker-reader list? And are `WhatBandwidthTheDecoderListensThroughTests`'s two reds known?
   - **Reasoning:** the gate-window method did not finish in 590 s alone at entry, so it is
     unmeasured on either side of this unit's changes. `WhatBandwidth`'s
     `MostRealRecordingsSitInTheWidestWindow` and `HoldingTheWindowLongInTimeReadsMore(004507)`,
     48 against 50, were red at entry and identical at exit. Neither is in the 51-name set.
   - **Rejected:** repairing either here. Neither is this unit's.
