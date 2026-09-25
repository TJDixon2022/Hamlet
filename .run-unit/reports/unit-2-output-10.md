```
READ IN THIS ORDER.

A. Hamlet reads a CQ call correctly. Steps 0 to 2 done; 3 partial with 3.4
   and 3.6 open; 4 not started; 5 the owner's; 6 partial with 6.5 open;
   7 partial with 7.1 to 7.4, 7.6 and 7.8 open.
B. Step 7, criterion 7.3: whether the bench reproduces the opening, and the
   line or property that differs between the E ET E E stretch and the
   locked one - met. A cold decode does not reproduce the opening. The whole
   session spliced into one stream does, and in it the mixdown pitch
   (CwDecoder.cs 617 to 621) leaves the sender first. The speed
   (CwProbabilisticStream.cs 428 to 435) follows it off.
C. The rest, weighed against A and B. Section 4 raises 4 items. None stands in
   the way of 7.3. Items 1 and 2 bear on 7.4: which figure to change, and which
   bench to judge it on.
```

```
UNIT:       429 - complete at task 4 of 4, none dropped - 2026-09-25 00:49
PHASE GOAL: When a station calls CQ in Morse, the CW tab shows what was sent, measured as edit distance against a key, and Tim at the radio says it read.
UNIT GOAL:  Using the decoder's own figures, find what it held differently while the 7.052 session read E ET E E and once it had locked, and name the line. Build nothing.
ADVANCED:   yes - 7.3 is ticked: a fact that asserts nothing prints the four figures for the opening beside the locked stretch, and the line that differs is named with its figures
NUMBER:     the named difference - mix pitch set at CwDecoder.cs 617 to 621, median 525 Hz in the opening against 625 Hz locked and a survey tone of 625, then the held speed 38 wpm from the grid against 21.8 from the estimator; keyed 165 -> 165 over 565, unchanged
DRIFT:      0 consecutive units without advance  (was 1)
```

## 1. What Claude did

**Complete, 4 of 4 tasks, none dropped.** The task 1 drop candidate, the continuous stream, was **not** dropped, because the cold decodes did not show the opening's litter. Machine QUIVERFULL, project Hamlet confirmed by the section 0 gate, branch main, HEAD at entry f94886a1. Commits, each pushed with rc 0: f832eb4f, bfcbf34d, df7d7627, 7a0b9d23, and the exit commit carrying this file. Nothing under `src` or `data` changed.

**The instruction against the tree (section 5). Reported; nothing repaired.**

- **The bench does not reproduce the opening from a cold start. This is the first finding.**
  - Cold 003901 reads **nothing for its first 20.6 s**, then `EII E T NHHK` from 20.6 to 24.7 s. Live read `E ET E E ... QNIK EE`.
  - Cold 003919 reads `EITEETNXNIK EANQNID EANQNIK`: the repeated group read fairly cleanly, not litter.
  - Cold 004108, the locked recording, reads as live did.
  - **The session spliced into one stream reproduces the litter.** Over 003919's new 16.2 s, carried warm from 003901, it reads `UIEH EE E E T I NIEEE E ... E ET N ■IK`. Live read `EESIH S E E TEIE E RIEEE` over the same audio. Cold, that audio reads `EANQNID EANQNIK`.
- **The recordings overlap, and by how much is exact.**
  - The sidecars' `audioSeen` is the live decoder's own sample count at the moment each file was kept, and every file is the 30 s before that.
  - 003901 and 003919 are 777,600 samples apart, so they **overlap by 13.80 s**. The `captured` stamps are 18 s apart, which would give 12 s; they disagree by 1.8 s on this pair only. Every other pair agrees within 1 s.
  - Each overlap was confirmed **sample for sample**: 662,400 of 662,400 samples identical for this pair, and all samples identical on the other seven overlapping pairs, each at exactly the offset `audioSeen` gives.
  - Five joins are jumps over audio never kept. The longest is 36.6 s, between 003919 and 004027. The task 1 commit message says six; five is right.
- **The opening is not a cold start, and it is not the first 30 s after the clear.**
  - At 003901 the live decoder had counted 5,479,200 samples, 114 s, so it had been listening since about 00:37:07.
  - 003901's 30 s run from about 00:38:31 to 00:39:01, so only its last 7 s or so follow the clear at 00:38:54.
  - The bench has none of the 84 s before 003901, so no bench run can carry the live decoder's state into 003901. Only the state carried into 003919 can be rebuilt.
- **The sidecar's counters do not match its text.** 003901's `inThis` and `characters` both say 92 characters and 223 elements "since the transcript was cleared". Its `text` holds 27 named characters. The counters look like counts since the decoder started. Parked as section 4 item 4.
- **The live litter would not print today.** 003901's `spanLlr` shows its opening `E`s at raw spans 4.1 to 12.8, all under `StrayElementSpan` 13.0. That bar arrived at a913f927, 2026-09-24 14:39 -0400, after the session. The litter the bench stream reproduces stands at raw 14.3 and above, so it passes today's bar.
- **Where the four figures are held:**
  - **Speed.** The window's unit is measured by `CwUnitEstimator.Measure` at `CwProbabilisticStream.cs:428`. If the implied speed is ready and inside 8 to 40 wpm, it is the speed (lines 431 to 435). Otherwise the grid searches 8 to 40 in steps of 2 (`CwProbabilisticDecoder.cs:756` to 771). The winner is carried as `CwProbabilisticResult.WordsPerMinute`, and the unit it implies is 1200 over that.
  - **Mix pitch.** `CwDecoder.cs:617` to 621 sets `CwProbabilisticStream.ToneHz`: the operator's lock if there is one, else the last pitch the tracker measured (`CwToneTracker.ToneHz`, line 443), else the tracker's bank.
  - **Unit estimate.** `CwUnitEstimator.Measure`, `CwUnitEstimator.cs:77`. The stream does not keep it; the trace measures it again on the very window each read decoded.
  - **Admission.** The window gate `Gate` 1.40 (`CwProbabilisticDecoder.cs:791`), then `Judged` (line 1314): span margin at or above `CharacterMargin` 1.0 per hop, and for one element, raw span at or above `StrayElementSpan` 13.0 (lines 1347 to 1350).
- **Keyed totals at HEAD:** all keyed 165 edits over 565, and 17 added letters, 8 of them single-element. Both match unit 428's exit.

**Task 0.**
- Records: `PHASE_OUTCOME.md` `## UNIT 429 - STEP 7` from the decision block, with the entry round. `PHASE_STATUS.md` names unit 429 and CURRENT_STEP 7. Version 1.13.115 to 1.13.116. Unit 428's section 4 items 1 and 2 are parked verbatim as P35 and P36; item 3 was for the record and is not parked.
- **Entry round, one type per invocation:**
  - build: 0 errors
  - engine carry-forward: 178 of 178
  - app carry-forward: 277 of 278. The one loss is `TheWindowHoldsBelowItsMinimumTests` on *You've caused dispatcher loop* in 1 ms; the type is 3 of 3 alone.
  - captures: 51 of 51 in 119 s
  - adjudicated: 13 of 13
  - keyed floors: 13 of 13
  - the keyed totals as above

**Task 1 - `WhatTheOpeningHeardTests.TheOpeningOnTheBenchAndLive`**, a fact that asserts nothing:
- Prints every sidecar's clock.
- Decodes all 14 recordings cold, beside the text each sidecar added.
- Splices the session into one stream and prints its text per recording and per 30 s.
- The decoder is driven a hop at a time with a 600 Hz start, as the captures type drives it.

**How the overlaps were cut** is my decision, overrulable:
- Each recording is placed on the `audioSeen` clock.
- Where two overlap, the later one's first 4,800 samples are searched for in the earlier one, one second either side of where `audioSeen` puts them, nearest first. The later one's leading samples up to the join are dropped.
- Where two do not overlap, they are butted together with no fill, and the join is named as a jump.

**Task 2 - `WhatTheDecoderHeldAtEachCharacter`**, a theory that asserts nothing:
- For every character settled in a stretch, it prints the character and its time, and the speed and unit the settling read held.
- Whether that speed came from the estimator or the grid, and the estimator's own unit on the same window.
- Whether the gap structure was held, and the held gaps.
- The median mix pitch and tracker pitch across the character's own span, and the survey's live tone.
- The window ratio, span margin and raw span, and the element count.
- A summary per figure, and a timeline of every read twice a second.
- Four private fields of `CwProbabilisticStream` are **read** by reflection and none is written: `_envelope`, `_structureHeld`, `_heldGaps`, `_hopsSeen`.

**My choices, overrulable:**
- **The opening** is the stream's 30.0 to 46.2 s, which is 003919's new 16.2 s. The stream's 0 to 30 s is 003901 decoded cold and cannot carry the live state, so this is the first stretch where carried state exists.
- **The locked stretch** is 004108 at 13.8 to 30.0 s. 004108 is the first keyed recording of the ten. Live, it held 22 wpm at 5.6 over silence with keying at 625 Hz, and it reads `DE KA2 G J V`. The stretch sits at the same place in its recording as the opening does in 003919, and more than 12 s after the jump before it, so the window holds only 004108.
- **Each stretch is also decoded cold on identical audio.** This is beyond what the instruction asked. It is the control that separates state carried in from sound heard.

**Task 3.** The theory ran unchanged on 004027's opening, the next recording as the instruction names. It does not show the litter: warm and cold read the same text. **Beyond the instruction's choice, and my decision,** I added a third row on 004535, the one later stretch where both the stream and live read a run of `E`. That run has a different cause (section 3). The row starts at 316.3 s rather than 316.2 so it falls inside 004535, whose join is at 316.21 s.

**Task 4 - exit round**, one type per invocation:
- `Hamlet.sln` non-incremental with warnings as errors: 0 warnings, 0 errors
- engine carry-forward: 178 of 178
- app carry-forward: 274 of 278. All four losses are *You've caused dispatcher loop* in 1 ms: `TheRecordNamesTheSubModePressedTests`, two in `ThePsk31ConversationCardTests`, and `TheChipSaysTheChosenModeTests`. Alone, the types pass 12 of 12, 8 of 8 and 6 of 6.
- captures: 51 of 51 in 119 s, all 51 rows the same as entry
- adjudicated: 13 of 13, output identical to entry apart from timing lines
- keyed floors: 13 of 13
- keyed and baseline totals: identical to entry, 165 over 565
- added letters: 17, 8 single-element, as at entry
- `WhatTheOpeningHeardTests`: 4 of 4 in 141 s

**`src` and `data` show nothing against entry f94886a1, and the transmit files show nothing against 7e209cb4.** The new type is on no carry-forward line. It runs under 300 s, but it asserts nothing and so guards nothing (my decision).

## 2. What the owner should expect

**Nothing on the CW tab changes.** No decoder code was touched.

**What is now known:**
- The opening's litter is **not** the decoder reading an empty band. On the same audio, a cold decode reads a repeated group (`EANQNID`, `EANQNIK`) at 24 wpm, so a sender is there.
- What the live decoder carried into that audio was a **mixdown pitch 100 Hz off that sender**. The tracker moved the mix from 600 to 525 Hz at the stream's 31.0 s, while the survey put the sender at 625. The mix filter is 60 Hz wide.
- Within a second the estimator's dit fell from about 50 ms to 27.5 ms. That implies 43.6 wpm, above the grid's 40 ceiling, so the estimator was set aside and the grid chose **38 wpm** for a sender sending about 22 to 24. At that speed the sender's marks came apart into `E`, `I` and `T`.
- The live sidecars agree with the stream's figures at both captures:
  - at 003901's end, live says 24 wpm and 3.0 better than silence; the stream says 24 and 3.03
  - at 003919's end, live says the clock is re-acquiring, 34 wpm, 0.8; the stream says the tracker has just moved again, 28 wpm, 0.80

**What will look wrong but is not:**
- On the bench, 003901 decoded alone reads almost nothing where the live screen read litter. That is not a regression and not a better decoder. The bench starts cold, and the live decoder had 84 s of history the tree does not hold.
- The litter `E`s printed live also sit under today's single-element bar and would be refused now.
- The captures floor for 003901 (9 named) is what a cold decode reads and says nothing about the opening.

## 3. What you should see

**No visible change.** This unit only traced. What follows is the answer to 7.3.

**Side by side: every character settled in each stretch, median with range.** Opening is stream 30.0 to 46.2 s, which is 003919 13.8 to 30.0 s. Locked is 004108 13.8 to 30.0 s. Both are 16.2 s. The survey's live tone is 625 Hz on both.

| figure | opening, warm, the litter | same audio, cold | locked, warm |
|---|---|---|---|
| characters, of them single-element | 23, 13 single | 14, 2 single | 17, 3 single |
| **speed held** | **38 wpm, unit 31.6 ms** (24 to 40); **15 of 23 from the grid** | 24 wpm, 50 ms, all from the estimator | **21.8 wpm, 55 ms**, all from the estimator |
| **mix pitch** | **525 Hz** (525 to 625) | 600 Hz | **625 Hz** (600 to 650) |
| **estimator's unit** | **27.5 ms** (22.5 to 50) | 50 ms | **55 ms** |
| gap structure held | never | always | 1 of 17 |
| window ratio per hop, gate 1.40 | 1.93 (1.44 to 3.32) | 3.85 | 5.53 |
| span margin per hop, bar 1.0 | 4.59 (1.52 to 30.1) | 13.6 | 12.2 |
| raw span, single-element bar 13.0 | 67.5 (14.3 to 850) | 626 | 752 |

**The line: `CwDecoder.cs:617` to 621**, where the mixdown pitch is set from the last pitch the tracker measured (`CwToneTracker.ToneHz`). **It moves first.**
- The opening's median is 525 Hz against 625 locked: 100 Hz apart, beyond half the decoder's 60 Hz filter.
- **The second figure: the speed, `CwProbabilisticStream.cs:428` to 435.** The estimator's unit is 27.5 ms against 55 locked, half. Once it implies more than 40 wpm, the speed falls to the grid (`CwProbabilisticDecoder.cs:756` to 771), which chose 38 wpm against 21.8. The speed moved second.

The order, read by read:

| stream s | what the reads show |
|---|---|
| 21.0 to 30.5 | mix 600; estimator 50 to 85 ms, speed from the estimator; window about 3.0 |
| **31.0** | **tracker and mix 600 to 525 Hz** |
| 31.5 / 32.0 | estimator 40 ms, then 30 ms |
| **32.5** | estimator 27.5 ms, 43.6 wpm: grid 38 wpm; the litter settles from 32.0 on |
| 36.5 | tracker back to 625; window falls to 0.6, as the window now holds audio mixed at 525; nothing settles to 41.0 |
| 45.0 | tracker to 575; 003919 is kept at 46.2 |

**How the trace tells litter from an empty stretch:** the cold decode of the identical audio reads `EANQNID` and `EANQNIK` with a window of 3.0 to 4.6. That is a sender, heard through a filter 100 Hz off the sender's pitch.

**The bench beside the sidecar, for the opening:**

| | text |
|---|---|
| live, 003901 as kept | `E ET E E   E  E E  E  E E  E    E A TE E T N QNIK     EE` |
| bench cold, 003901 | `EII E T NHHK`, from 20.6 s; nothing before |
| live, added by 003919 | `EESIH S E E TEIE E RIEEE` |
| **bench stream, 003919's new 16.2 s** | **`UIEH EE E E T I  NIEEE E         E ET N ■IK`** |
| bench cold, 003919 | `EITEETNXNIK       EANQNID        EANQNIK` |
| live, added by 004108 | `QENV U GE ANN DE KA2 G J V HR NR 2 0 R H X G K E 8 W G K 2 6 S T M W O H SEP 1 9 R` |
| bench cold, 004108 | `HR NR 2 0 R H X G K E 8 W G K 2 6 STMW OH SEP19 RIC` |

**Task 3: two more stretches.**
- **004027, 0 to 16.2 s, the next recording.** No litter. Warm reads `QNE K  E E  2 G ■ ■G KA2GJV` and cold reads the same but for one character. Both run at 23 to 24 wpm from the estimator, with the mix at 575 to 600 against a survey of 600. It neither confirms nor contradicts task 2. Live read litter before `KA2GJ V`, but most of that came from the 36.6 s never kept.
- **004535, 5.0 to 21.2 s, my addition.** Litter: `DN■ EEETEDEEEEETEEESK■D`, where live read `E E ET E D E E EE E TE E ES`. **Here a different figure differs.**
  - The speed is right: 24 wpm from a 50 ms estimator unit.
  - The mix is 585 to 605 against a survey of 600.
  - The run of `E` begins at 324.6 s, exactly when the held gap lengths become **12/345/306 ms** (element, character, word): a word gap shorter than the character gap. The same audio decoded cold holds 48/238/443 and reads `DN5EIN5`.
  - Those lengths are replaced by any read whose gaps separate (`CwProbabilisticStream.cs:459` to 464) and handed to the path at lines 483 to 494.
- **So the two litter stretches do not share one property.** The opening's is the mix pitch, then the speed. The later one's is the held gaps. Each is one reading, not evidence of a single cause.

**7.3 is ticked in `PHASE_PLAN.md`.** 7.4 is not claimed.

**The top of the speed search (7.1, 7.2), a finding only:** in the opening, the 40 wpm ceiling is what sends the speed to the grid. But the estimator's 43.6 wpm was wrong for a sender at 22 to 24, so a higher ceiling would have admitted the wrong figure rather than helped. Nothing was built.

## 4. What's blocking us

Nothing blocks the phase (R65). Four items are parked, most useful first.

**1. What 7.4 changes first.**
- **Proposed ruling:** 7.4 attacks the mixdown pitch at `CwDecoder.cs:617` to 621. It moves first, and the unit's collapse follows it within a second. The change is judged under 3.2's four tests.
- **Reasoning:** on the one stretch that reproduces the opening, the estimator held about 50 ms at 600 Hz and fell to 27.5 ms only after the mix went to 525. Speed repaired alone would still be decoding through a filter 100 Hz off the sender.
- **Rejected:**
  - Raising or removing the 40 wpm ceiling, because the estimator's 43.6 was the wrong figure.
  - Holding the gap structure, because the locked stretch reads cleanly without it.

**2. Which bench 7.4 is judged on.**
- **Proposed ruling:** 7.4 reports the named characters in its opening stretch twice, cold and on the spliced stream, the stream's 0 to 46.2 s by this unit's splice (`WhatTheOpeningHeardTests.Splice`). The stream is the instrument that shows the fault.
- **Reasoning:** cold, 003901 reads almost nothing for 20 s and 003919 reads the group fairly cleanly. A change aimed at carried state cannot show on a bench that carries none.
- **Rejected:** judging on the cold floors alone, which would score a fix to a fault they do not contain.

**3. The held-gap litter on 004535.**
- **Proposed ruling:** parked as a second, separate cause. It is a trace for a later step 3 or step 7 unit, not 7.4's.
- **Reasoning:** held gaps of 12/345/306 ms, with a word gap under the character gap, arrive with the run of `E`, and the cold decode of the same audio holds 48/238/443. One reading.
- **Rejected:** folding it into 7.4, which would put two changes against one criterion.

**4. The sidecar's counters.** For the record.
- **Proposed ruling:** a later unit checks what `inThis`, `characters` and `elements` count and words them to match.
- **Reasoning:** 003901 says 92 characters and 223 elements "since the transcript was cleared", beside a text of 27 named characters. The figures fit a count since the decoder started, 114 s earlier.
- **Rejected:** repairing it here; section 10 forbids it and it touches nothing 7.3 or 7.4 depends on.
