READ IN THIS ORDER.

A. The fourteen rows with their floors, in section 1 under task 1, and the guard proved working: leaving out every `E` and `T` took the baseline from 33 edits to 21 over 46 characters against inferred keys, and broke 13 of 13 named floors.
B. Step 2's criteria 2.1 to 2.5 are met and ticked, so step 2 is done. Step 1's 1.6 is met and ticked: 10 of 11 captures of the locked-on run keyed by differencing, 41 edits over 156 characters live.
C. The rest. Section 4 raises 1 item, carried from unit 411. Three new asks that block nothing are parked as P3 to P5 in `docs/phase-correctness/PARKED.md`. P3 bears on step 3: which keyed recordings a spacing change is judged on.

UNIT:       412 - complete at task 4 of 4, none dropped - 2026-09-23 22:28
PHASE GOAL: Hamlet reads a CQ call on the air correctly. This is measured as edits against a key over a scored region, with a guard so that going quiet cannot pass for reading well.
UNIT GOAL:  Bank the whole-QSO captures of 2026-09-24 as floors no later change may read below, and give the correctness number its guard: unsure per named, and a named floor per keyed recording, proved by breaking it.
ADVANCED:   yes - 2.5, the criterion this unit was aimed at, is met: fourteen rows banked, captures 51 of 51, the original 37 identical to entry
NUMBER:     capture floor rows: 37 -> 51; keyed recordings: 4 -> 14
DRIFT:      0

## 1. What Claude did

**Complete at task 4 of 4, none dropped.** Machine QUIVERFULL, project Hamlet, gate confirmed at `C:\Source\HamLet`, branch `main`, HEAD at entry `0483369d`. The commits are:
- `cffb8c2f` task 0
- `02ce4602` task 1
- `3105d7c8` task 2
- `3a469493` task 3
- the report commit, which follows them

**Verifying the instruction against the tree (§5). Mismatches are reported here; none was repaired.**
- **The captures were not untracked.** All the `.wav` and `.txt` files and `cases-2026-09-23.txt` were already committed, by the seed commit `0483369d` itself. Task 0 had nothing left to commit, so there is no captures commit.
- **There are fourteen captures, not thirteen.** They are `003901`, `003919`, `004027`, `004108`, `004133`, `004205`, `004234`, `004322`, `004347`, `004405`, `004427`, `004510`, `004535` and `004550`. The last one carries the 625 characters and 11 unsure that R63 quotes. **I banked all fourteen, which is a decision I made myself.** The range the instruction names holds fourteen, and a guard over thirteen of them would leave one unguarded. As a result the locked-on run, from `004108` onward, is eleven captures, not ten. Parked as P4.
- **The capture floor table** holds 37 rows. Named characters and elements are the floors, and placeholders are printed and not asserted (HM-DEC-168). A row is added to `Floors` in `TheCapturesThatDecodeKeepDecodingTests`. As stated.
- **`CwScorer`** has `Whole`, `Within` and `FromFirst`, and reports edits, scored length and the key's kind. As stated.
- **`baseline.md`** holds 33 edits over 46 characters over four recordings. As stated.
- **The sidecar's `text` is cumulative.** In all 13 consecutive pairs, the earlier text is a prefix of the later one. So task 3 stood.
- **Found, not caused by this unit:** `PHASE_STATUS.md` had an uncommitted `HEARTBEAT: 2026-09-23 21:35:11` line when the session started, written by something outside the session. It went into the task 0 commit along with my edits to that file.

**Task 0, the record.**
- HM-DEC-171 is in `DECISIONS.md`, and its row is at the top of `CLAUDE.md` §1.
- `PHASE_STATUS.md` names unit 412 with `CURRENT_STEP: 2`.
- `PHASE_OUTCOME.md` has its `## UNIT 412 - STEP 2` entry.
- The version went from 1.13.98 to 1.13.99.

Entry round, run after one build, with `--no-build` and a status line before each run:

| Run | Result | Time |
|---|---|---|
| Engine carry-forward | 178 of 178 | 370 s |
| App carry-forward | 276 of 278 | 152 s |
| Captures | 37 of 37 | 93 s |
| Adjudicated | 13 of 13 | 29 s |
| Clean synthetics | 2 of 2 | 2 s |

Two app tests were lost to the dispatcher loop before any assertion: `TheTestsStayOffTheNetworkTests.ThePlainFixtureTakesGeneralFromTheFixedAnswer` and `ThePsk31ConversationCardTests.APsk31CardOffersNothingWhileItIsNotHisTurn`. Re-run alone, they passed 2 of 2.

**Task 1, the floors (2.5).** I added fourteen rows, each measured once through the floor harness at `cffb8c2f`. The harness replays each WAV from a cold start at 600 Hz. Beside each row, as a comment, is the sidecar's live `inThis` line: what the application read from the same 30 seconds that night, with its lock carried in from the capture before. The two differ, and the harness figure is the one asserted.

| File | Named | Elements | Placeholders | Live `inThis` |
|---|---|---|---|---|
| `cw-2026-09-24-003901` | 9 | 20 | 0 | 92 emitted, 0 unsure |
| `cw-2026-09-24-003919` | 27 | 54 | 0 | 110 emitted, 0 unsure |
| `cw-2026-09-24-004027` | 40 | 119 | 1 | 51 emitted, 1 unsure |
| `cw-2026-09-24-004108` | 32 | 107 | 0 | 32 emitted, 0 unsure |
| `cw-2026-09-24-004133` | 30 | 87 | 2 | 31 emitted, 2 unsure |
| `cw-2026-09-24-004205` | 34 | 96 | 2 | 36 emitted, 2 unsure |
| `cw-2026-09-24-004234` | 37 | 96 | 1 | 36 emitted, 0 unsure |
| `cw-2026-09-24-004322` | 39 | 112 | 0 | 39 emitted, 0 unsure |
| `cw-2026-09-24-004347` | 40 | 115 | 0 | 39 emitted, 0 unsure |
| `cw-2026-09-24-004405` | 36 | 106 | 1 | 40 emitted, 2 unsure |
| `cw-2026-09-24-004427` | 43 | 112 | 1 | 41 emitted, 1 unsure |
| `cw-2026-09-24-004510` | 38 | 104 | 0 | 34 emitted, 0 unsure |
| `cw-2026-09-24-004535` | 47 | 128 | 1 | 53 emitted, 2 unsure |
| `cw-2026-09-24-004550` | 41 | 123 | 0 | 49 emitted, 1 unsure |

Then I ran the captures type whole: **51 of 51 in 122 s.** A script compares every printed row line with the entry run, and **the original 37 print identically to entry.** No existing row was retired, lowered or reworded.

Four printers also read `Floors`: `TheGateBarSweepTests`, `TheStationStillKeyingTraceTests`, `TheTrackerSwitchTraceTests` and `TheTwoPitchesTableTests`. They now read 51 rows. None of them asserts a row count, and none was run: they are not this unit's names (HM-DEC-155).

**Task 2, the guard (2.1, 2.2, 2.3).**

**2.1, unsure per named.** A new record, `CwReading`, carries each character's unsure flag in the same sense as `CwDecoder`'s own `CharactersUnsure`: a placeholder, or a letter settled below high confidence. `CwScore` gains `Named` and `Unsure`, counted over the same region the edits are. `ToString` now carries both, for example *"29 edits over 25 characters against an inferred key, 0 unsure per 28 named"*.
- The string overloads still work, but on a bare text they can see only placeholders.
- Two hand counts were added to `TheScorerCountsWhatAHandCountsTests`. One went red once, on my own test's wrong expectation: `FromFirst` trims only whitespace. I corrected the test, not the scorer. The type now passes 20 of 20.
- `baseline.md` is re-issued with the column:
  - Baseline: **33 edits over 46 characters against inferred keys, 0 unsure per 47 named.**
  - Outside the baseline: 124 over 363, 4 unsure per 269 named.
- Across all thirteen regions there are 4 unsure characters, and every one is a placeholder. Since unit 408's emission bar, the decoder prints almost nothing it is unsure of.

**2.2, a named floor per keyed recording.** `TheNumberCannotBeGamedTests.EachKeyedRecordingIsReadAtAll` covers 17:37 and all twelve adjudicated recordings, 547 named characters in all, 13 of 13.

**Decisions I made myself (all overrulable):**
- **The floor counts named characters over the whole recording, not over the scored region.** The region is chosen by the alignment and shrinks with the decode, and "read at all" is about everything settled.
- **The count is one per settled character**, as the capture floors count, so a prosign counts once there. The region's `Named` counts text characters, so there a prosign counts by its letters, the same way it counts in the scored length.
- 17:37 is in no capture row, and the three adjudicated rows' count floors retired in favor of their anchors. So this is the first count floor those four have.

**2.3, the guard watched working.** Each change went into `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`, was measured, and was taken out uncommitted.

| Change | Named kept, of 547 | Baseline, over 46 | 17:37, over 25 | Floors broken |
|---|---|---|---|---|
| none, at HEAD | 547 | 33 edits | 29 edits | 0 of 13 |
| **`&& character.Pattern.Length > 1` in `Judged`: no `E`, no `T`** | **350** | **21 edits** | 18 edits | **13 of 13** |
| `CharacterMargin` 1.0 to 100 | 97 | 42 edits | 25 edits, nothing named | 13 of 13 |
| `CharacterMargin` 1.0 to 10 | not summed | 34 edits | 25 edits | 13 of 13 |
| `CharacterMargin` 1.0 to 5 | not summed | 34 edits | 25 edits | 13 of 13 |

**Both numbers the instruction asks for: leaving out every `E` and `T` improves the baseline from 33 edits to 21 over 46 characters against inferred keys, and it breaks 13 of 13 named floors.** For example, 17:37 reads 27 named against a floor of 46, and 013347 reads 27 against 57.

**A decision I made myself, overrulable:** my reading of 2.3.
- The E/T change improves the total edits, but it suppresses about a third of the output (197 of 547 named), not most.
- The bar at 100 suppresses most of the output (450 of 547), but it improves only the whole-scored 17:37, from 29 to 25. It breaks `VA3VRR`, so the total gets worse, 33 to 42.
- Silence pays only on a key scored whole. I ticked 2.3 on the two measurements together.

After taking both changes out, `src` prints nothing against `0483369d`, and the re-run reads 33 edits and 13 of 13.

**Task 3, the keys by differencing (1.6).** I wrote eleven key files, `cw-2026-09-24-004108.key.md` through `-004550.key.md`. Each one:
- states that it is inferred and how it was built;
- quotes what its capture added to the transcript;
- names its scored stretches, each with its key;
- says what it leaves out and why.

That is 14 stretches over 10 captures. `004535` is keyed with nothing to score: it is single-element soup. Left out as ambiguous:
- every callsign and number not read with confidence, including `1 A W / 8`, `W 1A M T 8` and `2 5 9`;
- `N UR ING`, which is probably `DURING` but was left out rather than guessed.

**A decision I made myself:** the stretches and keys live in the key files in a fixed two-line form. `TheBenchmarkIsKeyedTests` reads them from there, so there is no second copy. It asserts two things: each predecessor's text is a prefix of the next capture's, and each stretch is a substring of what its capture added. It passes 1 of 1.

| Capture | Stretch as read live | Key | Live edits | Scored length | Bench edits |
|---|---|---|---|---|---|
| `004108` | `DE KA2 G J V` | `DE KA2GJV` | 3 | 9 | 6 |
| `004133` | `I C H ARD` | `ICHARD` | 3 | 6 | 4 |
| `004205` | `AT Y A H O O D` | `AT YAHOO D` | 4 | 10 | 4 |
| `004234` | `O T C O M <BT> T HANK Y OU` | `OT COM <BT> THANK YOU` | 5 | 21 | 9 |
| `004322` | `T HE A RRL S P O N S ORED A M ER I CA` | `THE ARRL SPONSORED AMERICA` | 11 | 26 | 15 |
| `004322` | `OP ERA T I ON` | `OPERATION` | 4 | 9 | 4 |
| `004322` | `ALL LOGS W` | `ALL LOGS W` | 0 | 10 | 3 |
| `004347` | `ILL BE UPLOADED TO ARRL L OO TW` | `ILL BE UPLOADED TO ARRL LOTW` | 3 | 28 | 4 |
| `004405` | `O OR D IN` | `OORDIN` | 3 | 6 | 3 |
| `004427` | `A T OR` | `ATOR` | 2 | 4 | 2 |
| `004427` | `QSL T NX` | `QSL TNX` | 1 | 7 | 2 |
| `004510` | `KA2GJV` | `KA2GJV` | 0 | 6 | 2 |
| `004510` | `AA3S B` | `AA3SB` | 1 | 5 | 1 |
| `004535` | none scored | | | | |
| `004550` | `DE KA2 GJV` | `DE KA2GJV` | 1 | 9 | 1 |
| **Total** | 14 stretches | **inferred** | **41** | **156** | **60** |

**Live** is the sidecar's stretch, scored whole: 41 edits over 156 characters against inferred keys, 0 unsure per 138 named. By hand, 40 of those 41 edits are spaces. The other is the extra `O` in `L OO TW`.

**Bench** is each WAV replayed cold, with the key aligned by `Within`: 60 edits over 156. Four captures follow the one before by more than 30 s: 004108 (41 s), 004205 (32 s), 004322 (48 s) and 004510 (43 s). Part of what they added was heard before their own WAV begins. That is why 004108's key aligns to unrelated text, ` G K 2 6 `.

All of this is tabled in `baseline.md`, beside the baseline and outside its total (P3).

**Task 4, the exit round (2.4).**
- `Hamlet.sln` builds with warnings as errors.
- Engine carry-forward: **178 of 178** in 371 s.
- App carry-forward: **277 of 278** in 156 s. `TheStopIsAlwaysOnScreenTests.AtEachOf354sNineSizesStopIsInTheStatusBarAndOnTheWindow` was lost to the dispatcher loop before any assertion. Re-run once alone, it passed 1 of 1, so it is counted neither way.
- Floors: captures **51 of 51** in 118 s, with the 37 original rows identical to entry; adjudicated **13 of 13**; synthetics **2 of 2**.
- This unit's types:
  - `TheScorerCountsWhatAHandCountsTests`: 20 of 20
  - `TheBaselineIsScoredTests`: 2 of 2, at 33 edits over 46
  - `TheNumberCannotBeGamedTests`: 13 of 13
  - `TheBenchmarkIsKeyedTests`: 1 of 1, at 41 live and 60 bench over 156
- **The diffs:**
  - The transmit files print nothing against `7e209cb4`.
  - `src/Hamlet.RadioEngine/Cw` prints nothing against `0483369d`, and neither does all of `src`, because neither 2.3 change was ever committed.

Criteria 1.6 and 2.1 to 2.5 are ticked in `PHASE_PLAN.md`. `PHASE_STATUS.md` and `PHASE_OUTCOME.md` mark step 2 done and step 1 partial. **The push succeeded after every commit:** `push rc 0` for the four task commits and for the report commit, `2fd41f76`.

## 2. What the owner should expect

What you watched the decoder read on 7.052 MHz tonight is now the mark. All fourteen recordings from 00:39 to 00:46 UTC are floors in the tree, each holding the named characters and elements it produced. The older recordings stay beside them, so no later change can read less of tonight or of August without turning a test red. The number that scores correctness now also has a guard, so it cannot be gamed by going quiet. I proved this with a change that dropped every `E` and `T`: it made the score twelve edits better and turned all thirteen guard tests red, and it was taken back out. Nothing about the decoder changed in this unit, and neither did anything the radio is sent. **What will look wrong but is not:** the new floors are lower than what the CW tab showed live, for example 9 named for `003901` where the sidecar says 92 emitted. The test replays each 30-second file from a cold start without the lock the live session carried in, and the floor is set at what that replay reads.

## 3. What you should see

**No visible change: this unit only makes the tests catch a regression later.** The question it was commissioned to answer was whether tonight's read is protected. Yes:
- 51 of 51 capture floors are green, with tonight's fourteen among them.
- A deliberately quieter decoder that scored better, 21 edits against 33, was caught by 13 of 13 named floors.

For the spacing work that comes next, the ten locked-on captures now have inferred keys. Live, they read 41 edits over 156 characters, and 40 of those edits are misplaced spaces: `T HE A RRL S P O N S ORED A M ER I CA` for `THE ARRL SPONSORED AMERICA`. The letters are right and the word boundaries are not, on a signal the decoder was locked to.

## 4. What's blocking us

**1. Compare the RF gain on one scale, so an RF gain already at full is neither written nor reported as unconfirmed.** (Carried from unit 411, verbatim, per HM-DEC-139. It is not this unit's work: it changes what is sent to the radio.)
- **Ruling wanted:** in a later unit, make `ReceiverSetup` and `Ic7300Rig.SetSettingAsync` compare the RF gain on one scale, either the condition in percent or the read-back in raw units.
- **Reasoning:** today every CW tune-in writes 255 to `14 02`, even at 100%. Every one is filed `NotConfirmed`, so the memory never records it, and "your hand wins" (HM-DEC-056) can never apply to RF gain. With this unit, the banner's sentence is true but appears on every tune-in. After the fix it would appear only when something is actually off.
- **Rejected, and why:** fixing it here. It changes what is sent to the radio, and step 6 changes only what the operator reads (R62, §6). The fix would also move the outcome to `AlreadyRight` or `Changed` and change what the hover narrates.

Unit 411's item 2, tonePeak, is answered by R63 and leaves the list. Parked under R54 as blocking nothing in this unit, in `docs/phase-correctness/PARKED.md`:
- **P3:** which keyed recordings step 3 is judged on, and whether on the live or the bench reading.
- **P4:** the benchmark is fourteen captures, not thirteen.
- **P5:** the two new guard tests are on no carry-forward line.
