```
READ IN THIS ORDER.

A. PHASE GOAL - CW decodes again. Steps 1, 2 and 4 done, their criteria
   all met; step 3 partial on 3.6 alone; step 5 is Tim's.
B. THIS STEP - step 3, the inherited reds. 3.1 to 3.5 met; 3.6, 4 of
   eight with a verdict - none of #15 #43 #44 #45 went green, on the
   still-keying property or on the air's trailing silence; all four stay
   open, each not attacked, because neither change passed the trace it
   depended on.
C. THIS REPORT - the red table leads section 3; section 4 raises 6 items
   and items 1 and 2 stand in the way of 3.6. Whether the still-keying
   property separated the moves: no, broken by 032113 at 26.535 s and the
   two-station and #41 handovers. Carried for Tim: #42's line read from
   instruction 406's decision 2 body, and the head's count rule, under
   which 3.6 closes only by greens.
```

```
UNIT:       407 - complete at task 5 of 5, tasks 0 to 5, none dropped - 2026-09-23 15:43
PHASE GOAL: bring the engine's CW decoder back to what it read on the air on 2026-08-25, hold it there with three floor tests, clear the inherited reds, then have Tim judge it at the radio
UNIT GOAL:  find out whether the pitch the tracker is reading is still keying when a held move goes, and use that alone to stop the moves off the one station on #15 #43 #44 without stopping the moves onto it; give #45's test the silence that follows a transmission on the air; keep only a red turned green at no cost
ADVANCED:   no - both changes were conditioned on a trace, and the trace refuted both, so no decoder or test line changed and 3.6 stays at 4 of eight
NUMBER:     of 3.6's eight 4 with a verdict; this unit 0 green, 0 moved, 4 not attacked
DRIFT:      1 consecutive units without advance  (was 0)
```

## 1. What Claude did

**Complete at task 5 of 5. All of tasks 0 to 5 were done and none was dropped.** Tasks 2 and 3
each ended in *no change*, because the condition each depended on failed in task 1's trace.
Machine QUIVERFULL, project Hamlet, branch main, gate confirmed: `SHACK_FACTS.md` and
`CwProbabilisticDecoder.cs` present, no `CoreHMI.sln`, no `MURC.sln`. Nothing in this report is
evidence about the radio. Commits `5d113dc3`, `a6b947e9`, `490fd485`, `8f654f7a` and `e7209910`
were each pushed, and each push succeeded. The report commit follows.

**Task 0, the entry round.** Version 1.13.93 to 1.13.94. `PHASE_STATUS.md` set to 407, step 3.
`git diff --stat 24d14e9c HEAD` over `src`, `tests` and the list prints nothing, so under
decision 8 the lines at entry are unit 406's exit: ENGINE 178 of 178, APP 278 of 278. Every type
was run one per invocation, and every list is identical, name by name, to unit 406's exit:

- captures 37 of 37, every row identical;
- adjudicated 13 of 13; synthetics 2 of 2;
- #6 0.82 and #42 green; #15 0.54; #43 5 + 37; #44 3 + 21; #45 1 + 3;
- `WhatBandwidth` 4 of 6, as before; the gate-window short methods 4 of 4.

The transmit files print nothing against `7e209cb4`. Section 4 item 5 lists three mismatches
against section 5 of the instruction.

**Task 1, the trace.** The printer `TheStationStillKeyingTraceTests` asserts nothing, and no
`src` file changed. It ran alone, 10 of 10 in 108 s. Output is in `.run-unit\unit407-trace.txt`
and findings in `docs\phase-cw\unit407-reds.md` section 2. The printer disturbs nothing: #6
prints 0.82, #15 0.54, and all 37 captures print their floor's count.

- **The property does not separate.** Every move off a single sender on #15, #43 and #44 goes
  while the bank's centre is still keying: 7 moves, 3 to 22 marks each. But known-right moves
  go while still keying too:
  - the two-station handover at 25.035 s;
  - #41's handover, the same move;
  - `032113` at 26.535 s, a hold from 600 to 650 Hz onto the station, with 6 marks at 21.7 dB;
  - `003016` at 25.535 s.

  The *not keying* reads on the other H1 rows are one unbroken mark at 33 to 50 dB. That is a
  limit of the rule, not a finding. Under the survey's own mark rule, every known-right hold on
  the five H1 rows is still keying.
- **#15 seed 104729.** `_readingDb` is 24 to 25 dB below the sender's own keyed level at every
  confirm from 18.0 to 20.5 s. But it is the 700 Hz image's own level. The station was never
  confirmed on this seed, so there is no reading level for line 1091 to keep.
- **#45's tail.** The settle delay is `DecisionDelaySeconds`, 1.0 s, so the padding is 2.0 s of
  digital zero. With it, the settled transcript goes from 1 + 0 to **4 + 0**. The fixture's
  1.5 s of noise after `K` is settled as three placeholders, and the step from noise to zero as
  an eight-element one. coverage-easy goes from 4 + 7 to 8 + 7, and exchange-easy from 0 + 7 to
  4 + 7.

**Task 2.** No change, because decision 4's condition fails. #45 is not attacked.

**Task 3.** No tracker change, under decision 1. Decision 3 is judged only inside that change,
so it is not applied. The narrower shape and G1 each need a change underneath them, so they
fall away without being reached. They are the named drop candidates, but they were not dropped
for time. The session ended at 15:43, 40 minutes after it began at 15:03.

**Task 4, the record.** Four rows marked 407 in `docs\phase-cw\reds-3.6.md`: #15, #43, #44 and
#45, each *not attacked*, with the traced reason, and each sequence broken at 0 of 3. The attack
table, every change not made and why, is in `unit407-reds.md` section 3. No red went green, so
the closing line of `unit239-failing-set.txt` and 3.1's tick are unchanged. 3.6 is not ticked:
4 of eight with a verdict.

**Task 5, the exit round.** Every figure is identical to entry (section 3). 3.5 is re-confirmed
on its own sentence: no commit of this unit touched `src`, and the floors and both lines are
green at exit.

**A choice the session made for itself, reproduced in full.** No `DECISIONS.md` entry was
written. Decision 1 does not say how to detect a mark, so the printer decides it as follows:

> A mark at the bank's centre starts when the centre bin `_fineDb[mid]` stands 10 dB,
> `CwToneSurvey.InterferenceLiftDb`, over the reading's own `NoiseDb`. It ends below 4 dB, which
> is the survey's 3 dB hysteresis either side of that. A 25 ms majority vote removes anything
> shorter than the shortest dit. *Still keying* is at least one key-up between two marks inside
> the span the hold waited through. A direct or cold move has no hold, so its span is the half
> second before it. The survey's own rule, a two-level split of the centre bin's last 3 s with
> 3 dB hysteresis, is printed beside it and decides nothing.

This was fixed before any number was seen. The verdict is *does not separate* under either
rule.

## 2. What the owner should expect

Nothing the decoder does has changed. No line under `src` moved in this unit. The four open
reds read exactly what they read at entry: #15 0.54, #43 5 + 37, #44 3 + 21 and #45 1 + 3.

The unit tested two ideas, and both failed their trace before anything was built:

- **The still-keying check.** The idea was that the tracker should not leave a pitch that is
  still being keyed. It would have stopped the bad moves on the easy tier. It would also have
  stopped a real capture's move onto its station 50 Hz away, which the bank can hear through
  its own reach, and the tracker's first moves toward every sender.
- **The trailing silence.** The idea was to give #45 two seconds of silence, as on the air, so
  the decoder would settle its trailing placeholder away. It made more placeholders, not fewer.

So nothing moved, and nothing cost anything. **This is the third 3.6 unit in a row that did not
flip the criterion.**

**What will look wrong but is not:**

- `TheStationStillKeyingTraceTests` appears in the test list and passes, because it asserts
  nothing.
- `reds-3.6.md` shows every open red at *0 of 3 - not attacked*. An attack is a change made, and
  the instruction's own conditions allowed none.
- An untracked `tests\fixtures\cw\captured\unadjudicated\cw-2026-09-23-173723.wav`, dated 13:37
  today, is in the tree. This session did not make it and did not commit it.
- `PHASE_STATUS.md` still names steps 0 and 2 `not started` (section 4 item 5).

## 3. What you should see

| red | cause as traced | change | before | after | moved | kept | sequence |
|---|---|---|---|---|---|---|---|
| #15 | holds to 700 Hz for a 640 Hz sender at 20.535 and 7.535 s, and to 600 Hz at 28.535 s, each while the 650 Hz bank is still keying; on seed 104729 the reading level is the image's own, the station never confirmed | none: the property does not separate (decision 1), and decision 3 goes only with it | 0.54 | 0.54 | no | - | 0 of 3, broken, not attacked |
| #43 | holds off 615 Hz at 13.035, 22.035 and 39.535 s while the bank is still keying; padding takes the settled transcript from 4 + 7 to 8 + 7 | none (decisions 1 and 4) | 5 + 37 | 5 + 37 | no | - | 0 of 3, broken, not attacked |
| #44 | hold off 615 Hz to 575 Hz at 11.535 s while the bank is still keying; so is the hold back at 20.035 s; padding takes the settled transcript from 0 + 7 to 4 + 7 | none (decisions 1, 4 and 5) | 3 + 21 | 3 + 21 | no | - | 0 of 3, broken, not attacked |
| #45 | 1.5 s of noise after `K` at 9.595 s; with 2.0 s of digital zero the decoder settles 4 placeholders, not 0 | none: decision 4's condition fails | 1 + 3 | 1 + 3 | no | - | 0 of 3, broken, not attacked |

**The answer to the question this unit was commissioned to ask: no.** Whether the pitch being
read is still keying does not tell a stale hold off the station from a move onto it. A station
25 to 50 Hz from the bank's centre keys *through* the bank, both when the tracker should stay
and when it should move onto that station. The cleanest break is `032113` at 26.535 s.

**The separation table**, one row per move, the full table in `unit407-reds.md` 2.1. *Marks /
key-ups* are at the bank's centre over the span the hold waited through.

| case | time | caller | from, to | sender | way | marks / key-ups | still keying |
|---|---|---|---|---|---|---|---|
| #15 7919 | 28.535 s | hold | 650 to 600 | 640 | away | 3 / 2 | yes |
| #15 104729 | 20.535 s | hold | 650 to 700 | 640 | away | 9 / 8 | yes |
| #15 15485863 | 7.535 s | hold | 650 to 700 | 640 | away | 5 / 4 | yes |
| #15, each seed | 1.5 to 2.0 s | cold | 600 to 650 | 640 | toward | 1 / 0, 3 / 2, 1 / 0 | no, yes, no |
| #43 | 13.035 s | hold | 625 to 600 | 615 | away | 6 / 5 | yes |
| #43 | 22.035 s | hold | 600 to 575 | 615 | away | 22 / 21 | yes |
| #43 | 28.535 s | hold | 575 to 625 | 615 | toward | 11 / 10 | yes |
| #43 | 39.535 s | hold | 625 to 600 | 615 | away | 6 / 5 | yes |
| #44 | 11.535 s | hold | 625 to 575 | 615 | away | 13 / 12 | yes |
| #44 | 20.035 s | hold | 575 to 625 | 615 | toward | 5 / 4 | yes |
| #6 7919, gate | 4.535, 7.035 s | direct, hold | 650 to 625, 625 to 650 | 640 | away, toward | 2 / 1, 3 / 2 | yes |
| #6 7919, gate | 10.535 s | H2 drops the hold | 650, held 550 | 640 | away | 9 / 8 | yes |
| #6 15485863, gate | 10.535 s | hold | 650 to 725 | 640 | away | 15 / 14 | yes |
| two-station and #41 | 25.035 s | cold, the handover | 625 to 725 | 730 | onto | 6 / 5 | **yes, breaks** |
| two-station and #41 | 34.535 s | hold | 725 to 725 | 730 | level | 17 / 16 | yes |
| `031838` | 8.535 s | hold | 500 to 525 | 525 | onto | 1 / 0; survey rule 15 / 14 | no |
| `031905` | 13.035, 26.535 s | hold | 500 to 300 | 300 | onto | 1 / 0; survey rule 5 / 4, 3 / 2 | no |
| `031905` | 18.035 s | hold | 300 to 500 | 300 | away | 3 / 2 | yes |
| `032113` | 21.035 s | hold | 500 to 600 | 650 | toward | 1 / 0; survey rule 3 / 2 | no |
| `032113` | 26.535 s | hold | 600 to 650 | 650 | onto | 6 / 5 | **yes, breaks** |
| `032129` | 13.535 s | hold | 500 to 650 | 650 | onto | 1 / 0; survey rule 8 / 7 | no |
| `003016` | 22.035 s | hold | 675 to 650 | 670 | away | 11 / 10 | yes |
| `003016` | 25.535 s | hold | 650 to 675 | 670 | onto | 8 / 7 | **yes, breaks** |
| `003126` | 21.5 to 24.0 s | 2 holds, 2 direct | 675 and 650 alternately | 665 | two away, two toward | 3 to 6 marks | yes |
| other captures | | 12 holds, 13 direct | 25 to 175 Hz | | | 1 / 0 each, 31 to 50 dB | no |

**Gate numbers for each kept change:** none. No change was made.

**Exit round, at `e7209910`, with `src` identical to entry:**

| line or type | exit |
|---|---|
| engine line | 178 of 178 in 375 s |
| app line | 278 of 278 in 171 s, no dispatcher loss |
| captures, adjudicated, synthetics | 37 of 37 with every row identical to entry, 13 of 13, 2 of 2 |
| red-holding types | acquisition 11 of 12, #15 0.54; receiver fixtures 24 of 27, #43 5 + 37, #44 3 + 21, #45 1 + 3; fixture 22 of 23; adjudication 11 of 11 |
| gate 8, displacement 6, own sending 6, the four speed readers 13, 4, 2, 5 | each identical to entry |
| tracker readers | each identical to entry; `WhatBandwidth` 4 of 6 as entry; gate-window short methods 4 of 4, `EveryWidthLeavesTheEmptyRecordingsSilent` unmeasured |
| transmit files against `7e209cb4`; `src` and `src/Hamlet.App` from `9c8198b1` | nothing printed |

Nothing green at entry is red at exit (HM-DEC-165). **Visible change in the app:** none. Nothing
that keys or transmits was touched.

## 4. What's blocking us

1. **#15, #43 and #44 have no route left inside what has been licensed.** Stands in the way of
   3.6.
   - **Ruling asked:** what is the next property to look for, or do these three wait for a
     ruling on the floors or the count rule?
   - **Reasoning:** three properties have now been tried at the hold:
     - H1 asked whether the survey still admits the held pitch. It stopped good moves.
     - H2 asked whether the survey admits the keying back at the bank. It is kept, and it does
       not reach these three.
     - This unit asked whether the bank is still keying. It does not separate.

     The station is audible through the bank's own reach in both kinds of move, so a property
     read at the old pitch cannot tell them apart. One further finding needs Tim's eye before
     anyone builds on it. Under the noise-referenced rule, the H1 rows read *not keying* only
     because a real receiver's passband keeps the key-ups well above the band beside it, which
     the flat noise of the synthetic reds does not do. That is §12.5's warning about fixtures
     built from the same misunderstanding. It is not a property to fit.
   - **Rejected:** narrowing the property to spans, seconds or pitches until the H1 rows pass.
     Section 10 forbids fitting it to rows. Also rejected: trying the narrower shape without a
     first shape. Decision 2 allows it only after a first shape moved a red.
2. **#45's trailing placeholder is the decoder reading the noise after a transmission, not an
   artifact of the file's end.** Stands in the way of 3.6.
   - **Ruling asked:** may a later unit attack how the settled pass reads the band noise after
     the last character, knowing that six capture floors count trailing placeholders (unit 405's
     B2)? Or is #45 to be judged against the floors' own count?
   - **Reasoning:** the air never ends, but the noise after the last `K` continues on the air
     too. Given 2 s more, the stream settles that noise into three more placeholders, so the
     same thing would show on the tab after a real station stops. That puts #45's fault in the
     settled pass, which is where B2 was, and B2 cost six capture rows because those floors
     record placeholders as characters. Re-expressing a floor is Tim's.
   - **Rejected:** the padding as a test event. It raises #45's count on its own settled
     transcript from 1 to 4. Also rejected: B1 alone, which section 10 forbids.
3. **#42's line, carried for Tim** (instruction section 3 item 1). Does not block this unit's
   result.
   - **Ruling asked:** does #42's green stand, with `ObserveOwnTransmission` in the suspended
     arm of `CwDecoder.cs`?
   - **Reasoning:** instruction 406's decision 2 body names an audio clock as one example of a
     `CwDecoder.cs` change it allows. Its decision-block summary says *only if the trace needs
     an audio clock*. The arbiter read the body, so the record stands at `099b3a2a`, green.
   - **Rejected:** re-ruling it in this unit.
4. **The count rule, carried for Tim** (instruction section 3 item 4). Does not block this unit's
   result.
   - **Ruling asked:** none new.
   - **Reasoning:** a unit that attacks nothing breaks every sequence, and a unit that moves a
     red restarts it. So #15, #43, #44 and #45 are at 0 of 3 after four 3.6 units, and 3.6
     closes only by greens.
   - **Rejected:** rewriting either count.
5. **Mismatches against section 5 of the instruction**, reported and not repaired. Do not
   block.
   - H2's hold is `CwToneTracker.cs` 958 to 978, with `Switch(_heldSwitchHz)` at 974, not 957
     to 966. Lines 212, 1077 to 1080 and 1091 are as stated.
   - `PHASE_STATUS.md` read `CURRENT_STEP: 0` from the launcher. At HEAD its step lines still
     name steps 0 and 2 `not started`, while the instruction says 1, 2 and 4 are done. This
     unit set `CURRENT_STEP: 3` and left the step lines alone.
   - The reload disagreement is not as stated. `CLAUDE.md` section 1 holds HM-DEC-165 of
     2026-09-19 at line 361, which matches `RULES_AT`. `CPS-DEC-0167` does not appear in
     `CLAUDE.md`.
   - An untracked capture, `cw-2026-09-23-173723.wav`, is in the tree. It is not committed and
     not this session's.
6. **Two tracker readers still do not fully measure.** Does not block. Nothing is new here.
   - `EveryWidthLeavesTheEmptyRecordingsSilent` is unmeasured under HM-DEC-155.
   - `WhatBandwidth` is 4 of 6, identical at entry and exit, and outside the 51-name set.

### Asks still outstanding

Every ask from before this phase is parked in `docs\phase-cw\PARKED.md` under R54. The arbiter
took unit 406's five items in instruction 407 section 3. The two still open are items 3 and 4
above:

- **#42's line.** First asked by unit 406 on 2026-09-23. It is waiting on Tim's reading of
  instruction 406's decision 2, and the change sits at `CwDecoder.cs` `ObserveOwnTransmission`,
  `099b3a2a`.
- **The count rule.** First asked by unit 405 on 2026-09-23. It is waiting on Tim, and it sits
  in the head of `docs\phase-cw\reds-3.6.md`.
