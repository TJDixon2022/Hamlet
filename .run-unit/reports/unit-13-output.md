READ IN THIS ORDER.

A. Hamlet meets the CW requirements: step 5 at 1 of 6, step 4 at 5 of 7, step 2 at 3 of 5,
   step 3 at 3 of 6 by the plan's checkboxes, steps 0 and 1 done, 6, 7 and 8 not started.
B. Step 5, criterion 5.5 (HM-REQ-034, 035, 036): speed proof state built; 034 test
   TheSpeedSaysWhetherItWasProvedTests green, watched failing on first moments, the stale hold and
   re-acquiring; 035 met; 036 met; recordings whose text changed 0 of 63; real-set hops proved
   61328, hypothesis 54494, none 22178; synthetic proved hops more than 10% off 0; 5.5 ticked;
   5.1 to 5.4 and 5.6 open.
C. This report adds a three-valued speed proof state in the decode report, and every speed display
   (the sheet's two speed lines, the header, the collapsed summary, the reacquiring pill, the
   transmit offer and the roster) says which state it is in. It also adds the trace of every path
   that reports, holds or withholds a speed, and tests naming HM-REQ-035 and 036, both green.
   Section 4 raises 2 items; neither is in the way of a criterion in B. 5.6 stays red on 443's
   DECIDED (3) and the three named floors, as at entry.

UNIT:       451 - complete at task 3 of 3, none dropped - 2026-09-26 10:03
PHASE GOAL: Every must-tier CW requirement in CW_REQUIREMENTS.md is met and shown met by a test naming it, step by step, ending with Tim reading real CW on the air.
UNIT GOAL:  The decoder says whether the speed it reports was proved on keying still arriving, is a hypothesis it is holding or supposing, or is none. Every place the operator sees a speed says which, the clear and the pitch refinement are measured by tests naming 035 and 036, and not one decoded character changes.
ADVANCED:   yes - PHASE_PLAN.md 5.5 flipped from [ ] to [x] in both copies, on a green HM-REQ-034 test watched red first, 035 and 036 tests run and green, and 63 of 63 recordings' text identical
NUMBER:     text changed 0 of 63; proved 61328, hypothesis 54494, none 22178 hops real; synthetic proved over 10% off 0; 035 met; 036 met
DRIFT:      step 2 0; step 3 0; step 4 1; step 5 1

## 1. What Claude did

**Complete, at task 3 of 3. Nothing was dropped**, including task 1's named drop candidate, the per-recording rows of the real-set table, which are kept.

**Provenance.** Claude Code on the Windows machine, project Hamlet confirmed by the gate (SHACK_FACTS.md, CwProbabilisticDecoder.cs, CW_REQUIREMENTS.md and CW_SPEC.md present; no CoreHMI.sln or MURC.sln), branch `main`.

**Commits**, all pushed to `origin/main`:
- `ba05e6e1`, task 0;
- `05c53b1a`, task 1;
- `c3c83c94`, the state, the display changes and the tests, on their own;
- `0dfd889e`, task 2, the tick;
- the exit commit, which carries this report.

**Quoted from the documents, as section 1 of the instruction asks:**
- **HM-REQ-034:** "The decoder shall report the speed estimate with a proof state of proved, hypothesis, or none." Rationale: "A speed the decoder has not earned is not a number." Verification row 034: "I, T | any | proof state field | present, three values | — | synthetic". Matches.
- **HM-REQ-035:** "When the transcript is cleared, the decoder shall retain its speed, pitch and noise-floor state." Row 035: "T | clear mid-transmission | state retained | yes | — | synthetic". Matches.
- **HM-REQ-036:** "When the pitch is refined for the same station, the decoder shall retain its timing state." Row 036: "T | pitch refine ±10 Hz same station | timing state retained | yes | — | synthetic". **This is a mismatch with the instruction**, which asked for "a step task 1 names". The row's ±10 Hz wins. The test runs +10 and −10 Hz, and also task 1's 25 Hz step.
- **HM-REQ-031:** "After acquisition on TX-ITU, the decoder shall report speed within 10 % of true (MET-WPM-ERR ≤ 10 %)." Printed here as evidence only.
- **`CW_SPEC.md` §2:** "the running speed estimate with its proof state (proved / hypothesis / none)". Matches. Section D of `CW_REQUIREMENTS.md` is Speed, as the instruction says.

**Mismatches between the instruction and the tree** (reported, not repaired):
1. **The "four-part guard" is three parts.** `CwDecoder.WordsPerMinute` (445-454) checks only three things: the window's text is non-empty, the clock is not re-acquiring, and the rounded speed is within 6 to 48. It checks for no located tone and no settled pass that proved a dit. The sheet's remarks at `MainWindowViewModel.cs` 12813-12816 make the same four-part claim; they are now corrected to three.
2. **HEAD called a non-reading a "hypothesis".** At HEAD, `SpeedForTheRecord` printed "the decoder's own best hypothesis was N WPM" even when the gate had refused the whole window (`CwProbabilisticDecoder.cs` 811-818). In that case `Reading.WordsPerMinute` is the grid's winner over noise.
3. **The instruction's facts that hold.** HEAD `f96cd04e`. Both copies of the plan had 4.1, 4.2, 4.3, 4.5 and 4.6 ticked, 4.4 and 4.7 not, and no step 5 line ticked. `CwDecodeReport` carried `PitchProof` and no speed state. `WordsPerMinute` is `int?` (439). `SpeedIsReacquiring` is a boolean (421). `Retuned()` is only `Unlock()` (385).
4. **Traceability** rows 94-96 are as the instruction says: no proving test for 034, 035 or 036, and the tests it names measure something else. Row 94 lists five of them. Not edited (R80).
5. **`AHeldPitchDoesNotOutliveItsEvidenceTests.TheReleaseStartsTheReadingFresh` against HM-REQ-036.** The test asserts that `Reading.WordsPerMinute` goes to 0 after `Retuned()`. It is red at entry and at exit (3 of 4, as logged). Its own remarks call the call a QSY. The application calls `Retuned()` only for a dial move of 500 Hz or more (`MainWindowViewModel.cs` 13606-13610), so a nudge never reaches it. A QSY is not a refinement for the same station, so the assertion does not contradict HM-REQ-036 as the requirement is worded. It would if `Retuned()` were ever called for a nudge, and `Retuned()` itself takes no size to tell the two apart. Nothing in the test was edited.
6. **Known items, each confirmed and none edited.**
   - `PHASE_OUTCOME.md`'s header still has the old titles for steps 2, 3 and 8.
   - `CW_SPEC.md` 327 still defines MET-COVERAGE as sure over sent.
   - `PARKED.md`'s header says a session never writes it.
   - `TheQuietestBinNoLongerWinsTests` and `ThePitchControlsAreOffThePanelTests` are excluded by `Compile Remove`.
   - The RULES_AT item does not hold as stated: `CLAUDE.md` 376 carries HM-DEC-165, which matches `PROJECT_STATUS.md`, and CPS-DEC-0183 does not appear in it. Unit 450 reported the same.
7. **Expected failures, as listed.**
   - The named floors were red at entry and exit with the same readings: 17:37 38 of 46, `032113` 43 of 45, `032129` 42 of 64.
   - `AHeldPitchDoesNotOutliveItsEvidenceTests` is 3 of 4 red.
   - The app line lost 3 at entry: `ThePsk31OfferTests` 1 and `TheFavoritesAreUnderTheGreenZoneTests` 2, each green alone. It lost 1 at exit: `TheCarrierHoldsTheButtonsTests` 1, green alone, 8 of 8. There was no hang.
   - `TheFiveToEightDecibelPlateauHolds` is in neither line and was not run.

**Entry (task 0).**
- Build 0 errors in 15 s.
- Engine line 178 of 178 in 376 s. App line 275 of 278 in 160 s.
- Captures 51 of 51 in 129 s, adjudicated 13 of 13 in 32 s, named 10 of 13 in 67 s.
- All four metrics and MET-PITCH-ERR (9 files) matched the instruction's table.
- The runner's writes were committed unedited, and 63 recordings' text was saved to `.run-unit/unit451-text-before.txt`.

**Task 1.**
- `WhatTheSpeedCanSayItProvedTests` is the sibling of 450's pitch printer. It asserts nothing and changes nothing in `src`. It reads the proposed state off the decoder hop by hop. The unit's provenance was not published at HEAD, so it recomputes that from the stream's own envelope with the stream's own calls.
- The output is `.run-unit/unit451-trace-speed-state.txt`, and the notes, path table and display inventory are `.run-unit/unit451-trace-notes.md`.
- `TheSpeedLineSaysWhatWasProvedTests` printed every speed surface's words on the 23 keyed recordings at HEAD, before `src` changed (`.run-unit/unit451-app-speedlines-before.txt`).

**Task 2.**
- **The 034 test came first**, with `StateOf` read from HEAD's field: a named number is proved, anything else none. It went **red** on three cases (`.run-unit/unit451-req034-red2.txt`, and once more on the final cases in `-red3.txt`):
  - first moments: 2107 hops with a reading were called none;
  - the stale hold: 780 of 6121 hops past the lapse were proved;
  - re-acquiring: 972 of 2400 proved and 1428 none.
- **The 035 and 036 tests came next, run at HEAD**, and both were green (`-req035-head.txt`, `-req036-head3.txt`). The final 036 test, with the row's ±10 Hz steps, is also green at the entry code `f96cd04e` in a separate worktree (`.run-unit/unit451-athead-ARefinementKeepsTheTimingTests.txt`, 23 green checks).
- **Then the state went in**, and 034 is green on it.
- **(c) holds:**
  - 63 of 63 recordings' text is byte-identical (`cmp`).
  - The four metrics are identical on both sets. V-11 compared 35 recordings and found 0 worse.
  - The floors match entry.
  - MET-PITCH-ERR: 69 captures, 0 changed, 248 → 248 windows, 9 files over 25 Hz.
  - The task 1 printer and the built `CwDecoder.SpeedProof` disagree on 0 hops, so the trace totals are the shipped state's.
- 5.5 was ticked in both copies, and one line was added to `metrics.md`.

**Exit (task 3).**
- **Build and carry-forward lines.** Build 0 errors in 8 s. Engine line 178 of 178 in 374 s. App line 277 of 278 in 165 s, with the loss green alone.
- **Floors.** Captures 51 of 51 in 136 s, adjudicated 13 of 13 in 33 s, named 10 of 13 (the same three).
- **Metrics.**
  - Real, inferred: MET-CER-SURE 33 of 436, MET-INVENTED 33 over 473, coverage 403 over 473, MET-WBE 46 over 113.
  - Synthetic, exact: 14 of 173, 14 over 252, 159 over 252, 48 over 84.
  - MET-PITCH-ERR in 253 s: 9 files, 0 changed.
- **Touched types, each run alone.**
  - Engine, green: `TheSpeedSaysWhetherItWasProvedTests` 1/1 (29 s), `ARefinementKeepsTheTimingTests` 1/1, `WhatTheSpeedCanSayItProvedTests` 1/1 (63 s), `CwSpeedSilenceTests` 4/4, `CwCaseCountsSayWhatTheyCountTests` 7/7, `TheSwingIsTheFigureThatHoldsTests` 4/4, `ThePitchSaysWhetherItWasProvedTests` 1/1.
  - Engine, red as logged: `AHeldPitchDoesNotOutliveItsEvidenceTests` 1 of 4.
  - App, green: `TheSpeedLineSaysWhatWasProvedTests` 1/1 (113 s), `AClearKeepsWhatTheDecoderWorkedOutTests` 1/1, `ThePitchLineSaysWhatWasProvedTests` 1/1, `TheRestOfTheSheetIsTrueTests` 10/10, `EverySentenceOnTheSheetTests` 1/1, `TheSheetSaysWhatEachElementWasSentAtTests` 4/4, `CaseRosterSurvivesAnEveningTests` 6/6, `ASheetSaysWhichInstrumentSpokeTests` 3/3, `TheCarrierHoldsTheButtonsTests` 8/8.

**What changed in `src`, file by file.** None of it keys or transmits. **Nothing in the decode path, the tracker or the pitch reads the new state.** The two new reads (`UnitWasMeasured`, `LastKeyedHz`) are read only by `CwDecoder.SpeedProof`.
- `Cw/CwSpeedProof.cs` (new): the enum None, Hypothesis, Proved.
- `Cw/CwDecoder.cs`: `SpeedProof`, computed from existing state, and passed into `Report`.
- `Cw/CwDecodeReport.cs`: `SpeedProof` and `WordsPerMinute` fields, and `SpeedWasProved`, which is read from the state.
- `Cw/CwProbabilisticStream.cs`: `UnitWasMeasured` is set where the read already decides between the estimator's speed, the grid and the marks' overrule, and it is reset in `Restart()`. It is an assignment beside `Last`; the decode is untouched, as the identical text shows.
- `Cw/CwToneTracker.cs`: `LastKeyedHz`, a read-only view of `_lastKeyedHz`.
- `Cw/CwCaseRoster.cs`: an optional `SpeedProof` on `CwCase`. The speed cell adds " (hypothesis, not proved)" to a hypothesis.
- `ViewModels/MainWindowViewModel.cs`: `DetectedSpeedProof`, the header, the collapsed summary, the pill, `SpeedForTheRecord`, `FitLine`, and the roster row's state.
- `ViewModels/CwTransmitViewModel.cs`: `HeardSpeedProof` and `SpeedOffer`'s wording.

**Decisions Claude made for itself, in full:**
- **(1) The definition, in one sentence.** Proved means three things hold:
  - `CwDecoder.WordsPerMinute` names a number (445-454);
  - the window's unit behind it was measured from the keying, not won on the grid (the estimator's dit or the marks' overrule, `CwProbabilisticStream.cs` 431-435 and 505-514);
  - the tracker found keying within half the mixdown filter, 30 Hz, of the pitch being read, inside its own recent span of six surveys (`KeyingRecently` 712 and `KeyingFoundAt` 1217-1224; the half-width is the one `CwDecoder.cs` 733 uses for the same sender).

  Hypothesis means the window holds a reading and any one of those fails. None means nothing has been read, or the gate refused the whole window. It is never "acquired", and nothing gates on it.
- **(2) Six surveys, not the latest survey.** The first reading used the latest survey's keyed verdict alone, as 450 did for the pitch (`.run-unit/unit451-trace-speed-state-run1.txt`). It proved a clean 12 wpm send at 15 dB on only 900 of 5681 hops, because the survey does not confirm keying on every half second of a slow sender. The tree's own remarks at `CwToneTracker.cs` 702-712 say that asking the settled reading whether somebody is keying right now "asks the wrong question". Section 4 item 1 asks the owner to confirm this.
- **(3) `SpeedIsReacquiring` stays** as the decoder's fact about the clock. It is one of the state's inputs, not a restatement of the state: re-acquiring withholds the number, and so the state is never proved while it holds. The new boolean `SpeedWasProved` is read from the state.
- **(4) A proved number now carries a suffix.** The sheet's proved line reads "17  (proved: ...)" where HEAD printed "17". The header's proved text is unchanged ("17 WPM"). The not-named branches of the sheet open with the state word. HEAD's "the settled pass has no clock" (there is no settled pass) now reads "the window read nothing".
- **(5) The 034 re-acquiring case was rebuilt once.** Run first from the join itself, it went red on 72 hops in the first 0.35 s after the join, proved at 16 wpm. That is the first station's own speed, in the generator's 1 s of band before the second station's first mark, where nothing has changed yet (`-req034-green2.txt`). The case now starts at that first mark, which is constructed and not read from the decoder, and it adds a check that anything proved before the mark is the first station's 16. The final cases were watched red again on HEAD's field (`-red3.txt`).
- **(6) The 036 precondition was restated once.** Run first as "the tracker moved toward the new note by half the step", the +10 Hz case was red because the tracker already read 650 on the 640 Hz send, so the new note was where it stood (`-req036-exit.txt`). The check is now "the tracker ends within half the step of the new note, with no follow". Every timing assertion was green in both runs. **So only −10 and +25 moved the tracker. At +10 there was nothing to refine**, which is 4.4's pitch bias, not 036's.
- **(7) The 035 test is in the app project**, because the clear is the app's command (`ClearTerminalCommand`). It runs on the tree's own `cq-18wpm-15db` recording, exact by construction, since the generator is not in the app test project. It compares hop for hop with a twin decoder that is never cleared.
- **(8) The roster's speed cell marks a hypothesis in its own text** rather than in a new column, so the column count is unchanged.

## 2. What the owner should expect

The operator now reads whether the speed in front of them was earned now. A number the decoder measured on keying still arriving at the station's pitch reads as it did ("22 WPM", "They are sending at about 22 words a minute"). A number it is holding from a window whose sender has stopped, or one it won on the search without measuring a dit, now reads "22 WPM, not proved", and the sheet says "HYPOTHESIS, NOT PROVED NOW" and why. A blank speed says whether there is a reading behind it (hypothesis, while the clock re-acquires) or nothing at all (none).

**In 21 of the 23 real recordings, the unit now says hypothesis on hops where HEAD showed a bare number**: 30042 hops of 91370. **At the end of the file**, where the sheet is written, that is 7 recordings: 013347, 003758, 031905, 031948, 032012, 032129 and 004550. Across all 23 at the end of the file, 12 are proved, 10 hypothesis and 1 none. No number appears anywhere HEAD withheld one, and no decoded letter changed.

**A clear does not cost the decoder its speed today.** On `cq-18wpm-15db`, the speed (18), the pitch (625.0 Hz) and the held noise figure (35.494 dB) are identical across the clear. The rest of the file is identical to a twin decoder that was never cleared, over 1953 hops. **A refinement does not cost it either.** On a +10, −10 and +25 Hz step of the note, and on the operator's lock, there was no re-acquisition, no moved discontinuity, and the speed stayed within 1 wpm of an unrefined control on every hop.

**The evidence is synthetic and exact.** The real set's states are inferred-key facts about the decoder, not about the senders (V-13).

**What the synthetic cases do not prove** (§12.5):
- They are one textbook sender at a time, at 15 dB in generated noise, with a clean step.
- A real drifting note, a clear during a follow, and a speed change on one pitch are not tested.
- The held gap structure was never established in the 036 sends, so its "not dropped" check held trivially.

**What will look wrong but is not:**
- A steady station's header can flip between "22 WPM" and "22 WPM, not proved" across the gaps between overs. That is the keying lapsing for six surveys, and it is what the state says.
- The sheet's proved line now carries a parenthesis after the number.
- The roster's wpm cell can read "18 (hypothesis, not proved)". Anything that parses that column as a bare number will need to allow for it.

## 3. What you should see

**The answer: the speed carries a proof state, and every speed display says which.** HM-REQ-034 is met (green, watched red first), HM-REQ-035 is met, HM-REQ-036 is met, and 0 of 63 recordings' text changed.

**Every way the decoder reports, holds or withholds a speed** (task 1; the hop counts are over every keyed and synthetic recording):

| # | where | evidence | current or remembered | state | hops |
|---|---|---|---|---|---|
| 1 | `CwProbabilisticStream.cs` 185, 401-404: nothing read until 3 s of window | nothing | - | none | 20965 |
| 2 | `CwProbabilisticDecoder.cs` 811-818: the gate refused the window, the grid's winner over noise | nothing | current, no reading | none | 24638 |
| 3 | `CwDecoder.cs` 421-425, 445: re-acquiring after a follow of 30 Hz or more (728-737) | the rolling reading, straddling two pitches | current, two senders | hypothesis | 24452 |
| 4 | `CwDecoder.cs` 452-454: rounded outside 6 to 48 | the rolling reading | current | hypothesis | 0 (the search is 8 to 40) |
| 5 | `CwProbabilisticStream.cs` 431-435: no dit measured, the grid's winner | the rolling reading | current | hypothesis | 1901 |
| 6 | `CwToneTracker.cs` 1008-1010, 1217-1224: no keying found for six surveys | a measured dit in a window whose keying stopped | remembered, up to 12 s | hypothesis | 35997 |
| 7 | last keying more than 30 Hz from the pitch being read | a measured dit, keying elsewhere | remembered at this pitch | hypothesis | 200 |
| 8 | all pass: on the latest survey / within six surveys | a measured dit, keying at the pitch | current | proved | 55742 / 30004 |
| - | `MainWindowViewModel.cs` 12820-12823: no decoder | nothing | - | sheet: "not tracking" | - |

**What a clear and a refinement do, beside the tests.**
- **The clear.** It is `ClearTerminal` (10827-10832): it stamps `_clearedUtc` and clears `Transcript`, and does not reach the decoder. The stream's window (`CwProbabilisticStream.cs` 185), the tracker, and the held noise figure (`CwDecoder.cs` 795-814) run on. `Restart()` is reachable only behind `ClearOnAStationChange`, which is `const false` (159, 698). **HM-REQ-035: green at HEAD and after**, identical across the clear and against the twin.
- **The refinement.** It is the tracker's `Switch` with `refining` (`CwToneTracker.cs` 1236-1260): a move of at most 25 Hz while a pitch is held, which counts `Retunes` and not `Follows`. It can also be the fine bank's in-reach reading (1181-1190). `CwDecoder` watches only `Follows`, and marks a discontinuity only at 30 Hz or more (728-737), so a refinement leaves the clock and the window alone. The operator's `Lock()` re-points the mixdown only. **HM-REQ-036: green at HEAD (the entry code in a worktree) and after**:
  - +10 Hz: the tracker stayed at 650, already on the note;
  - −10 Hz: 650 to 625, 7 to 10 retunes, 1 follow throughout;
  - +25 Hz: 650 to 675, 7 to 14 retunes;
  - the lock at 16.3 s: speed 18 on every hop after.

**The sheet's speed line, before and after:**

| recording | state | before (HEAD) | after |
|---|---|---|---|
| `cw-2026-09-23-173723` | proved | `decoderWpm 17` | `decoderWpm 17  (proved: the dit was measured on keying still arriving at the pitch being read)` |
| `cw-2026-08-18-003758` | hypothesis, where HEAD showed a number | `decoderWpm 24` | `decoderWpm 24  HYPOTHESIS, NOT PROVED NOW (no keying has been found at the pitch being read for six surveys, so it is held from a window whose sender has stopped)` |
| `cw-2026-08-22-031838` | none | `decoderWpm not proved (the settled pass has no clock; the decoder's own best hypothesis was 20 WPM)` | `decoderWpm none, not proved (the window read nothing; the search's winner over a window that read nothing was 20 WPM, which describes nobody)` |

**Every other speed display, before and after** (on 003758 unless named):
- **Sheet `reading` line:** "24 WPM won out of 8 to 40, 5.94 better than silence per hop against a gate of 1.40  (this is the last 12 second window…)" becomes "…against a gate of 1.40; the speed is a HYPOTHESIS, NOT PROVED NOW  (this is the last 12 second window…)". A proved one adds "; the speed is proved", and 031838 adds "; the speed is none: the window read nothing".
- **Terminal header:** "24 WPM" becomes "24 WPM, not proved". The proved 173723 still reads "17 WPM".
- **Collapsed summary:** "24 WPM · tail" becomes "24 WPM, not proved · tail".
- **Transmit panel:** "They are sending at about 24 words a minute. You can set the radio's keyer to match, or to whatever you would rather send at." becomes "Hamlet's reading of their speed is about 24 words a minute, and it is not proved: the keying it came from has stopped or was not measured. Set the radio's keyer to whatever you would rather send at." A proved speed keeps HEAD's sentence.
- **Reacquiring pill** (134712, 032050, 032113): "working out the speed" becomes "working out the speed, nothing proved yet".
- **Roster wpm cell:** "24" becomes "24 (hypothesis, not proved)". Proved and none are unchanged ("22", "not tracking").

**Hops by state, with the synthetic speed error beside proved:**

| set | key | proved | hypothesis | none | proved hops more than 10% off the constructed speed | HEAD's shown hops now hypothesis |
|---|---|---|---|---|---|---|
| real, 23 | inferred | 61328 | 54494 | 22178 | - | 30042 of 91370, in 21 recordings |
| synthetic, 12 | exact | 24418 | 8056 | 23425 | 0 of 24418 | 8056 of 32474 |

**The (c) table:**

| | entry | after the change | exit |
|---|---|---|---|
| recordings compared / changed | 63 saved | 63 / 0 | - |
| MET-CER-SURE real, inferred | 33 of 436 | 33 of 436 | 33 of 436 |
| MET-CER-SURE synthetic, exact | 14 of 173 | 14 of 173 | 14 of 173 |
| MET-INVENTED real / synthetic | 33 / 473, 14 / 252 | same | same |
| coverage real / synthetic | 403 / 473, 159 / 252 | same | same |
| MET-WBE real / synthetic | 46 / 113, 48 / 84 | same | same |
| captures / adjudicated / named | 51/51, 13/13, 10/13 | 51/51, 13/13, 10/13 | 51/51, 13/13, 10/13 |
| MET-PITCH-ERR files over 25 Hz | 9 | 9, 69 captures 0 changed | 9, 0 changed |

**Ticks:** 5.5 ticked in both copies of `PHASE_PLAN.md`. 5.1 to 5.4 and 5.6 are not ticked, and nothing in steps 2, 3 or 4 was ticked. **DRIFT:** step 2 0, step 3 0, step 4 1, step 5 1 (§3 (f)).

## 4. What's blocking us

1. **What "current" means for a proved speed. Ruling wanted, and it does not block 5.5.** The unit reads it as keying found at the pitch within the tracker's own six-survey span, three seconds (`KeyingRecently`).
   - **Reasoning.** The tree says the settled reading trails the survey, and that gating it on keying right now asks the wrong question (`CwToneTracker.cs` 702-712).
   - **Rejected: the latest survey alone**, which unit 450 used for the pitch. It proved a clean 12 wpm send at 15 dB on 900 of 5681 hops, and the real set on 40559 hops against 61328.
   - **The cost of the reading taken:** a speed stays proved for up to about six and a half seconds after a sender stops.
   - The owner may prefer the stricter or a looser span. It is one comparison in `CwDecoder.SpeedProof`, and it changes no decoded character either way.
2. **A speed change at one pitch is proved at speeds nobody sent. A finding for HM-REQ-032's line, not repaired here.**
   - **The case.** Printed, not judged, by the 034 test: 16 wpm then 24 wpm at 640 Hz, in the 12 s after the change. The window straddles the two speeds, and the proved hops name 16 (x93), 17 (x400), 19 (x300), 23 (x400) and 24 (x535). 17, 19 and 23 are more than 10% off whichever speed was being sent. The path is row 8: a measured dit, keying current at the pitch.
   - **Why the state cannot see it.** The tree has no re-acquisition for a speed change without a pitch move, so a straddle is not visible to it.
   - **Reasoning.** Proved says the dit was measured on current keying, not that the window holds one sender's speed.
   - **Rejected: calling a straddle a hypothesis this unit.** It would need a new detector for a speed change, and section 7 forbids changing how the speed is found or re-acquired.
   - **Wanted:** a ruling on whether 5.x should give the speed its own re-acquisition on a change of 25% or more, as HM-REQ-032 implies.
