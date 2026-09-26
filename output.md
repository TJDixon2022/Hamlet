READ IN THIS ORDER.

A. Hamlet meets the CW requirements: step 4 at 5 of 7, step 2 at 3 of 5, step 3 at 3 of 6,
   step 5 at 0 of 6 by the plan's checkboxes, steps 0 and 1 done, 6, 7 and 8 not started.
B. Step 4, criterion 4.6 (HM-REQ-093): proof state built; test ThePitchSaysWhetherItWasProvedTests
   green, watched failing on the level case past the lapse and both stale-hold cases; recordings
   whose text changed 0 of 63; real-set hops proved 40664, hypothesis 50810, none 46526; proved
   windows more than 25 Hz off the instrument 0 (by majority state; 5 windows hold a proved
   minority that far off, a finding); 4.6 ticked; 4.4 and 4.7 open.
C. This report adds the three-valued pitch proof state in the decode report and on the record
   sheet, the trace of every path that holds a pitch, and the per-state table against the pitch
   instrument. Section 4 raises 3 items; none is in the way of 4.6. 4.7 stays red by 443's
   DECIDED (3) and the three named floors, as at entry.

UNIT:       450 - complete at task 3 of 3, task 1's named drop candidate dropped - 2026-09-26 08:13
PHASE GOAL: Every must-tier CW requirement in CW_REQUIREMENTS.md is met and shown met by a test naming it, step by step, ending with Tim reading real CW on the air.
UNIT GOAL:  The decoder says whether the pitch it reports is proved by keying now, a hypothesis held from earlier keying, or none, and the sheet says which, with not one decoded character changed.
ADVANCED:   yes - PHASE_PLAN.md 4.6 flipped from [ ] to [x] in both copies, on a green HM-REQ-093 test watched red first and 63 of 63 recordings' text identical
NUMBER:     text changed 0 of 63; proved 40664, hypothesis 50810, none 46526 hops real; proved over 25 Hz off 0 (majority windows); MET-CER-SURE real 33 -> 33
DRIFT:      step 2 0; step 3 0; step 4 1; step 5 1

## 1. What Claude did

**Complete, at task 3 of 3.** Claude Code on the Windows machine, project Hamlet confirmed by the
gate (SHACK_FACTS.md, CwProbabilisticDecoder.cs, CW_REQUIREMENTS.md, CW_SPEC.md present; no
CoreHMI.sln or MURC.sln), branch `main`. Commits: `f7986865` (task 0), `75235a5f` (task 1),
`734f72fd` (the state, the sheet change and the tests, on their own), `4d2e0b03` (task 2, the tick),
and the exit commit with this report. All pushed to `origin/main`.

**Quoted from the documents, as section 1 asks:**
- HM-REQ-093: "The decoder shall report pitch with a proof state of proved, hypothesis, or none." Rationale: "Not a survey candidate, not a stale hold." Verification row 093: "I, T | any | proof state field | present | — | synthetic". Matches the instruction.
- HM-REQ-091: "The decoder shall choose the tracked pitch by keying quality and never by level alone or by the operator's configured pitch." Matches.
- HM-REQ-092: "After acquisition, the decoder shall report the pitch it is demodulating at, within N Hz of true (MET-PITCH-ERR ≤ N)." N is "TBD, needs ruling". Matches.
- HM-REQ-034: "The decoder shall report the speed estimate with a proof state of proved, hypothesis, or none." Not built (5.5's).
- V-12: "Nothing is diagnosed against audio that has not itself been proved." Matches.

**Mismatches between the instruction and the tree** (reported, not repaired):
1. Section 1 says to read "section J" of `CW_SPEC.md`. Section J is in `CW_REQUIREMENTS.md`, and `CW_SPEC.md` has no section J. `CW_SPEC.md` §2 does say "the running pitch with its proof state", as quoted.
2. The instruction says a survey candidate and a from-cold point were reported as measured. **They were not.** The from-cold move sets `_reportedHz` to NaN (`CwToneTracker.cs` 1026), so HEAD already reported it as unmeasured. An unconfirmed survey candidate never sets the reported pitch (1048-1054). What HEAD over-claimed was holds: after the station stopped, while the survey refused a candidate, or while a move was pending.
3. `PitchWasAsserted` is not a field. It is a constant, `=> false` (`CwDecodeReport.cs` 72), so no asserted path exists in this build.
4. `traceability.md` names a third test that measures something else for HM-REQ-093, at line 273: `NothingActsOnTheAdmissionVerdictTests.AnUnmeasuredPitchIsStillReportedAndSaysSo`. The instruction names two.
5. The known item "`CLAUDE.md` §1 holds CPS-DEC-0183" does not hold. `CLAUDE.md` 376 carries HM-DEC-165, which matches RULES_AT, and CPS-DEC-0183 does not appear in `CLAUDE.md`.
6. Known and confirmed, not edited: `PHASE_OUTCOME.md`'s header still has the old titles for steps 2, 3 and 8; `CW_SPEC.md` §11 (line 327) still defines MET-COVERAGE as sure over sent; `PARKED.md`'s header says a session never writes it.
7. Expected failures. The named floors 17:37, `032113` and `032129` were red at entry and at exit, with the same readings (38/46, 43/45, 42/64). The app line lost 2 at entry (`TheCarrierHoldsTheButtonsTests`, `Unit376TheTopBandTests`, each green alone) and 0 at exit, with no hang. `TheFiveToEightDecibelPlateauHolds` is in neither line and was not run. **Not in the instruction's list:** `AHeldPitchDoesNotOutliveItsEvidenceTests` is 3 of 4 red. It is red the same way at entry HEAD `706e3874`, run in a separate worktree (`.run-unit/unit450-athead-*.txt`), and it was red the same way at unit 433's entry.

**Entry (task 0).** Build 0 errors in 16 s. Engine carry-forward 178 of 178 in 375 s. App 276 of 278 in 164 s. Captures 51 of 51 in 130 s, adjudicated 13 of 13 in 32 s, named 10 of 13 in 67 s. All four metrics and MET-PITCH-ERR (9 files, 248 of 1688 windows) matched the instruction's table. The runner's writes were committed unedited, and every recording's text (63) was saved to `.run-unit/unit450-text-before.txt`.

**Task 1.** The printer is `WhatThePitchCanSayItProvedTests`, a sibling of the trace types, and asserts nothing. It reads the proposed state off the tracker's public face each hop, with `src` unchanged. It crosses each hop against 447's instrument over the 23 keyed recordings, the 8 over-25 files that are not keyed, and the 12 synthetic cases. Its output is `.run-unit/unit450-trace-pitch-state.txt`. **Dropped: the cross over all 69 captures**, which was the named drop candidate. Nothing else was dropped.

**Task 2.**
- **The test came first.** It was run with the state derived from HEAD's flags: asserted gives hypothesis, measured gives proved, anything else none. It went **red** on "level, after the keying" (841 of 841 hops proved beside the carrier alone), "stale hold, the band" and "stale hold, key held down" (841 of 841 proved each). The output is in `.run-unit/unit450-req093-red.txt`.
- **Then the state went in.** `CwPitchProof` was added and `CwToneTracker.PitchProof` built. `CwDecodeReport` carries it, and the sheet line was changed. The test is green (`-green2.txt`).
- **(c) holds:**
  - 63 of 63 recordings' text is byte-identical (`cmp`).
  - The four metrics are identical on both sets. V-11 compared 35 recordings and found 0 worse.
  - The floors match entry.
  - MET-PITCH-ERR: 69 captures, 0 changed, 248 → 248 windows.
- 4.6 was ticked in both copies, and one line was added to `metrics.md`.

**Decisions Claude made for itself, in full:**
- **(1) What proved means.** Proved means the latest survey's confirmed keyed verdict names the very pitch the tracker reports. That verdict follows HM-DEC-095: keying seen on two surveys running, and for a move, at least 10 dB of lift and not more than 10 dB under the station being read. It is set in `CwToneTracker.cs` 1136-1174 and 1230. A pitch still held from an earlier verdict is a hypothesis, and no pitch held is none.
  - It is computed as `Verdict.Keyed.ToneHz == _reportedHz`. Every path that sets `_reportedHz` stores that same number in `Verdict`, and every survey replaces `Verdict`.
  - It is never "acquired" and gates nothing.
- **(2) The from-cold point is none, not hypothesis**, per 3 (a). It follows the loudest bin even on an empty band, because 1020 needs only `coarse.Strongest`. So it is level alone, and HM-REQ-005 wants an empty band reported as not measured. This goes against 3 (b)'s list, which names the from-cold point among the hypotheses. (a) is the one that keeps the product from stating more.
- **(3) `PitchWasMeasured` stays, read from the state** as `PitchProof != None`. That is true for proved and hypothesis, which is exactly HEAD's meaning, so the two cannot disagree. The positional parameter was replaced, and one hand-built report in `TheRestOfTheSheetIsTrueTests` was renamed to the new field (`PitchProof = CwPitchProof.None`). The duty line and the `unkeyed` line keep reading the boolean and print what they did.
- **(4) No survey-candidate case.** The tree has no window where a candidate's pitch is reported, and the test's remarks say so. The level case takes its place, per the instruction.
- **(5) The lapse is set at 4 s after the keyed audio ends.** That is the survey's 3 s of history, plus one 0.5 s survey interval, plus 0.5 s more.
- **(6) The proved case's pitch check is printed, not asserted.** I read "the instrument puts the reported pitch within one of its bins" as the instrument confirming the construction (every window 640.0), and that is asserted. On a clean 640 Hz tone at 15 dB, the survey's own confirmed verdicts name 625 (200 hops), 635 (800), 640 (4000), 645 (100) and 650 (500). Holding proved hops to within 0.5 Hz, or even to 5 Hz, would judge HM-REQ-092. That N is TBD, and the tracker's choice belongs to 4.4, so asserting it would fail no matter what the state did.
- **(7) In task 1, a window's state is the one held on most of its hops**, the weaker on a tie. Windows that hold a proved minority far off are printed separately.
- **(8) A printer test was added.** `ThePitchLineSaysWhatWasProvedTests` prints the sheet line before and after on the 23 keyed recordings. It asserts that a hypothesis is never worded as measured from keying. The "before" is HEAD's wording applied to the same report. Decoding is identical and HEAD's line was a function of the flag and the pitch alone, so it was not re-run at HEAD.

**Exit (task 3).**
- **Carry-forward lines.** Build 0 errors in 8 s. Engine carry-forward 178 of 178 in 378 s. App carry-forward 278 of 278 in 166 s.
- **Floors.** Captures 51 of 51 in 129 s, adjudicated 13 of 13 in 31 s, named 10 of 13 in 66 s (the same three).
- **Metrics.**
  - Real: MET-CER-SURE 33 of 436, MET-INVENTED 33 over 473, coverage 403 over 473, MET-WBE 46 over 113.
  - Synthetic: 14 of 173, 14 over 252, 159 over 252, 48 over 84.
  - MET-PITCH-ERR ran in 253 s: 9 files, 248 windows, 0 changed.
- **Touched types, each run alone:**
  - Green: `ThePitchSaysWhetherItWasProvedTests` 1/1 (24 s), `WhatThePitchCanSayItProvedTests` 1/1 (127 s), `NothingActsOnTheAdmissionVerdictTests` 1/1, `WhereAcquisitionPointsTests` 2/2, `TheCaptureOfTheTwentyThirdTests` 1/1, `CwTrackerSwitchTests` 2/2, `TheTrackedPitchIsChosenByKeyingTests` 2/2, `CwToneSurveyTests` 5/5. On the app side: `ThePitchLineSaysWhatWasProvedTests` 1/1 (109 s), `TheRestOfTheSheetIsTrueTests` 10/10, `EverySentenceOnTheSheetTests` 1/1, `TheSheetSaysWhatEachElementWasSentAtTests` 4/4, `TheTonePeakIsAboutThisRecordingTests` 3/3, `WhatTheTonePeakIsAboutTests` 1/1.
  - Red as at HEAD: `AHeldPitchDoesNotOutliveItsEvidenceTests` 1/4.
  - Not run: `TheQuietestBinNoLongerWinsTests` and `ThePitchControlsAreOffThePanelTests` are excluded by `Compile Remove` in their csproj, so no test matched.
- **What changed in `src`, file by file:**
  - `Cw/CwPitchProof.cs` (new): the enum None, Hypothesis, Proved.
  - `Cw/CwToneTracker.cs`: one read-only property, `PitchProof`. No field and no decision changed.
  - `Cw/CwDecodeReport.cs`: the positional `bool PitchWasMeasured` became `CwPitchProof PitchProof`, and `PitchWasMeasured` is now read from it.
  - `Cw/CwDecoder.cs`: `Report` passes `PitchProof: _tracker.PitchProof`.
  - `App/ViewModels/MainWindowViewModel.cs`: `ToneForTheRecord` prints proved or hypothesis.
- **None of it keys or transmits.** Nothing in the decode path reads the new state. Its only readers are `CwDecoder.Report`, the sheet line and the tests.
- **Ticks.** 4.6 is ticked. 4.4, 4.7, 2.4, 2.5 and nothing in steps 3 or 5 were touched.

## 2. What the owner should expect

**What the operator now reads.** The record sheet's `toneHz` line used to call any pitch the decoder was holding "measured from the keying the survey admitted", including one held a minute after the station stopped. It now reads one of three ways:
- **"proved"**, only while the survey's latest verdict confirms keying at that pitch;
- **"HYPOTHESIS, NOT PROVED NOW"**, when keying set the number earlier and the latest survey does not confirm it;
- **the NOT MEASURED sentences, unchanged**, when no pitch is held.

**Where it changed.** On the 23 real keyed recordings, the sheet taken at the end of the file now says hypothesis where HEAD said measured on **15**. Six say proved and two say none. The clearest case is `032050`, whose sheet said "325.0 Hz, measured from the keying". The instrument puts that station at 499.9 Hz, and the line now says HYPOTHESIS.

**The evidence.** Over every hop of those recordings, the windows the instrument puts more than 25 Hz off break down by majority state as:
- 0 of 179 proved windows;
- 38 of 253 hypothesis windows;
- 24 of 188 none windows.

The keys are inferred (V-13), but no key enters this measure. The comparison is against the independent instrument.

**What will look wrong but is not.**
- A station being read flips between proved and hypothesis from one half second to the next. The survey does not confirm on every pass: 37,199 hops across the sets were "keying within the last 3 s, not on this survey".
- Most end-of-file sheets say hypothesis, because by then the station has stopped.
- Proved does not mean accurate. On a clean 640 Hz tone, proved hops sat anywhere from 625 to 650.

**What the synthetic cases do not prove (12.5).** They are one tone with textbook spacing over generated noise, and the 4 s lapse is Claude's choice. They show that the state separates a stale hold and a carrier left alone from keying confirmed now. They do not show that it does so on the air. Not one decoded character changed.

## 3. What you should see

**Task 1's path table: every way the tracker comes to hold a pitch** (`CwToneTracker.cs` unless named)

| path | where | evidence | current or remembered | state | why |
|---|---|---|---|---|---|
| start at the operator's pitch | 318, 382, 443-445 | operator setting | - | none | nothing chose it |
| from cold, to the loudest bin | 1018-1029 (NaN at 1026) | level alone | current | none | level is not keying; moves on an empty band too |
| candidate seen once, unconfirmed | 1048-1054 | keying, one survey | current | unchanged (none cold, else hypothesis) | HM-DEC-095 needs two; sets no pitch |
| move to a confirmed keyed candidate | 1136, 1153, Switch 1230 | keying verdict, two surveys | current | proved | the verdict that set the pitch is the latest |
| inside reach, fine survey keyed | 1162-1169 | keying verdict | current | proved | same |
| inside reach, coarse only | 1172-1174 | keying verdict | current | proved | same |
| move held mid-character | 1143-1151 | old pitch held, keying elsewhere | remembered | hypothesis | latest verdict names a different pitch |
| held move made on a later survey | 970-982 (978) | keying of the earlier survey | remembered | hypothesis, unless that survey confirms it (then proved) | |
| candidate refused, lift under 10 dB | 1076-1084 | a hold | remembered | hypothesis | |
| candidate refused, over 10 dB under `_readingDb` | 1123-1134 | a hold | remembered | hypothesis | 12 real survey hops fit this: 024403 x2, 031905 x5, 031948 x4, 032012 x1 (449 item 2; nothing changed) |
| no keying anywhere | 992-1031 | a hold; protection counts 6 surveys (987-990, 1202) | remembered | hypothesis | the stale hold |
| operator assertion | `CwDecodeReport.cs` 72 | - | - | would be hypothesis | constant false; not reachable |
| operator lock, mixdown fallback | `CwDecoder.cs` 304-322, 600-621 | - | - | not a path | these set what is mixed, not the reported pitch |

**The sheet's pitch line, before and after** (`.run-unit/unit450-touched-ThePitchLineSaysWhatWasProvedTests.txt`)

- proved, `cw-2026-09-24-004405`:
  - before: `625.0 Hz  (measured from the keying the survey admitted: the centre of the survey bin it was admitted in, not interpolated between bins)`
  - after: `625.0 Hz  (proved: the survey's latest verdict confirms keying at this pitch. Measured from that keying: the centre of the survey bin it was admitted in, not interpolated between bins)`
- HEAD said measured, now hypothesis, `cw-2026-08-22-032050` (the instrument puts the station at 499.9):
  - before: `325.0 Hz  (measured from the keying the survey admitted: ...)`
  - after: `325.0 Hz  (HYPOTHESIS, NOT PROVED NOW: keying was found at this pitch earlier, the centre of the survey bin it was admitted in, and the survey's latest verdict does not confirm it. The number is held, not measured from keying now)`
- none, `cw-2026-08-17-134712`, unchanged before and after: `500.0 Hz  (NOT MEASURED: the survey has admitted no keying and nothing has chosen a bin, so this is the middle of the bank the decoder is pointed at rather than a station)`

**Hops per state, with the instrument's more-than-25 Hz count beside each** (windows counted by majority state)

| set | recordings | hops proved | hops hypothesis | hops none | proved windows (>25 Hz) | hypothesis windows (>25 Hz) | none windows (>25 Hz) | key |
|---|---|---|---|---|---|---|---|---|
| real, keyed | 23 | 40664 | 50810 | 46526 | 179 (0) | 253 (38) | 188 (24) | inferred |
| over 25 Hz, not keyed | 8 | 0 | 3194 | 38430 | 0 (0) | 15 (15) | 132 (82) | inferred |
| synthetic | 12 | 14183 | 19209 | 22507 | 58 (0) | 82 (0) | 77 (0) | exact |

**The 9 files more than 25 Hz off, and the state while off:**
- Seven were in none throughout: `014935`, `014308`, `005158`, `005218`, `005243`, `002424` and `002443`.
- `031838` was in hypothesis on 20 of its 21 off windows and none on 1.
- `012823` was in hypothesis on 15 and none on 3.
- None was proved.

**Finding: proved hops more than 25 Hz off.** Five real windows hold a proved minority that far off:
- `032050`: 100 hops at 325 Hz against 499.9, twice;
- `004427`: 625 against 599.5, twice;
- `004507`: 475 against 500.7 (-25.7).

One synthetic window, `cq-18wpm-15db-char5`, holds 650 against 617.8. All of these came by the keyed-verdict path, a move or a reading inside reach. For one survey interval, the survey's own confirmed verdict named a bin that far from the station.

**(c): decoding unchanged**

| measure | entry | after the change | exit |
|---|---|---|---|
| recordings' text compared / changed | 63 / - | 63 / 0 | - |
| MET-CER-SURE real, inferred | 33 of 436 | 33 of 436 | 33 of 436 |
| MET-INVENTED real, inferred | 33 over 473 | 33 over 473 | 33 over 473 |
| coverage real, inferred | 403 over 473 | 403 over 473 | 403 over 473 |
| MET-WBE real, inferred | 46 over 113 | 46 over 113 | 46 over 113 |
| synthetic, exact (CER-SURE, INVENTED, coverage, WBE) | 14/173, 14/252, 159/252, 48/84 | same | same |
| captures, adjudicated, named | 51/51, 13/13, 10/13 | 51/51, 13/13, 10/13 | 51/51, 13/13, 10/13 |
| MET-PITCH-ERR: files over 25 Hz, windows | 9, 248 of 1688 | 9, 248; 69 compared, 0 changed | 9, 248; 0 changed |

**Visible change:** only the sheet's pitch line. Nothing on the CW tab's text changes.

## 4. What's blocking us

Nothing blocks 4.6. There are three items for a ruling, most useful first.

1. **Ruling asked: whether proved lapses at the latest survey or holds for the survey's 3 s of protection.**
   - Claude built the strict version: proved only while the latest survey's confirmed verdict names the pitch.
   - Reasoning: HM-REQ-093's rationale is "not a stale hold". The survey's verdict already rests on 3 s of history, so extending proved by the 3 s keyed-protection countdown (`KeyingRecently`) would let proved rest on evidence up to 6 s old.
   - Rejected: the lenient version, because it states more.
   - Cost of the strict version: a live display would flip between proved and hypothesis while a station is being read. 37,199 hops across the sets were "keying within the last 3 s, not on this survey".
   - This is a presentation question for a live surface. The sheet is a snapshot.
2. **Ruling asked: whether a confirmed keyed verdict far from the instrument should stay proved.**
   - For one survey interval, five real windows and one synthetic window held proved hops at 25 to 175 Hz from the station the instrument found (`032050` at 325 against 499.9).
   - Claude left it. Proved means the survey's rule was met, and changing which bin the survey names is 4.4's work (section 7).
   - Rejected: gating proved on the instrument, because R76 keeps the instrument out of `src`.
3. **Logged, not fixed (12.6).**
   - `AHeldPitchDoesNotOutliveItsEvidenceTests` is 3 of 4 red at entry HEAD and at unit 433. It is in neither carry-forward line and is not on the instruction's list of expected failures. Its reds are about `Retuned()`, which this build reduces to an unlock (`CwDecoder.cs` 385).
   - `TheQuietestBinNoLongerWinsTests` and `ThePitchControlsAreOffThePanelTests` are excluded from compilation by `Compile Remove`, so they never run.
   - CLAUDE.md 12.6 says items like these go in `OPEN_ISSUES.md`. This unit did not edit that file, and names them here instead.
