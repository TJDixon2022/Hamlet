```
READ IN THIS ORDER.

A. The phase goal - CW decodes again. Steps 0, 1, 2 and 4 are ticked on
   every criterion; the outcome file holds 0 and 2 at not started and 4 at
   partial, a layer mismatch reported here, and the launcher named step 0
   for this unit on that reading; step 3 is this unit's, partial at 3.1,
   3.2 and 3.3 at entry with 20 reds open in the set, every one a decode
   result, and the third floor test red at every commit of the phase;
   step 5 is Tim's. After this unit the third floor test is green 2 of 2,
   the harness reads the settled transcript, the set stands at 37 green
   and 14 red-open, and 3.4 still waits on the 14 repairs left.
B. The criteria, one line each, met or not: 3.1 met by unit 394, its
   sentence gaining #25, #26, #31, #32 and #30, #33; 3.2 met by unit 398,
   nothing retired here; 3.3 met, nothing retired here; 3.4 not met, the
   known-reds block and the set's closing line untouched, 14 reds left;
   3.5 ticked with its R53 qualification at 7d1ffde6 - floors 37 of 37,
   13 of 13 and 2 of 2 at task 4, lines app 277 of 278 in each of two
   runs with each loss the dispatcher loop and green in the other run,
   engine 176 of 176 in 374 s, no file under src changed. The unit named
   3.5 in ADVANCES; it flipped.
C. The report last. Section 4 raises 0 items and none is in the way of a
   criterion in B; everything carried from before this phase and every
   finding that blocks nothing is in docs/phase-cw/PARKED.md under R54,
   not here.
```

```
UNIT:       400 - complete at task 4 of 5, none dropped, tasks 0 to 4 all run - 2026-09-23 04:43
PHASE GOAL: get the CW decoder that worked on the air in August working again, proved by three named floor tests and in the end by Tim at the radio
UNIT GOAL:  make the test harness read the same settled characters the CW tab shows, prove that costs no test that was green, then give the clean fixtures a quiet band so the third floor test goes green with its assertion untouched and nothing under src changed
ADVANCED:   yes - step 3 criterion 3.5 ticked at 7d1ffde6 with its R53 qualification
NUMBER:     harness cost 0 cases in 5 caller types, 0 pinned; clean synthetics 0 of 2 -> 2 of 2 at band 0.02; set names green 31 -> 37, red-open 20 -> 14; prosigns green, #30 and #33; floors 37 of 37 and 13 of 13 at both ends; engine line 374 s -> 374 s of 480
DRIFT:      0
```

## 1. What Claude did

**Complete at task 4 of 5, tasks 0 to 4, none dropped.** Claude Code on Tim's Windows machine,
Hamlet (gate: `SHACK_FACTS.md` and `CwProbabilisticDecoder.cs` present, no `CoreHMI.sln` or
`MURC.sln`, root `C:\Source\HamLet`), branch `main`. The unit's first status line was at 04:05:37; it
finished at 04:43.

**Commits, all pushed to `origin/main`:**

| Hash | Task | What it changed |
|---|---|---|
| `cc373cfc` | 0 | entry round; `PHASE_OUTCOME.md` entry, `PHASE_STATUS.md` whole, version 1.13.86 to 1.13.87, doc section 1 |
| `bec38668` | 1 | `CwDecodeHarness.cs` line 71 `CharacterDecoded` to `CharacterSettled` plus its remark; doc section 2 |
| `7d1ffde6` | 2 | **the repair**: `CwFixtures.cs` clean requests gain `NoiseAmplitude: 0.02` and the comments decision 7 names; `clean-12wpm.wav` and `clean-18wpm.wav` regenerated; doc section 3 |
| `d11096ca` | 3 | `prosigns-18wpm` request gains `NoiseAmplitude: 0.02`; `prosigns-18wpm.wav` regenerated; sibling printer `TheProsignsFixtureAtABandTests.cs` added; doc section 4 |
| `5e70860d` | 4 | exit round, doc section 5 |
| `52f4a8e0` | 4 | the ticks in `PHASE_PLAN.md`: 3.5, the 3.1 and 1.3 clauses; doc section 6 |
| `08576089` | 4 | `PARKED.md` 400 items 1 to 5 |

**Task 0.** Section 5 verified against the tree; the mismatches are in section 3 below. Decision 6's
diff printed nothing, so unit 399's exit runs are the entry lines. One build and then the floors:
37 of 37 with every row identical to unit 399's exit, 13 of 13, 0 of 2. `CwFixtureTests` 14 green
and 9 red. The caller baseline had one surprise: `CwSensitivityTests.TheDecoderReadsAsFarDownAsItDidBefore`
is red at entry on `Assert.NotNull`. It is a compiled name outside the set that no document names.
It was red before anything changed, so it is baseline and not a regression, and it is parked as
400 item 1.

**Task 1.** The harness change was built and run uncommitted against `CwFixtureTests` whole, the five
caller types, `TheCleanReadsStayCleanTests` and `TheSurveyAlreadyUsesAShortWindowTests`, every case by
name. **No case green at task 0 went red.** Two red cases went green:
`NothingTheDecoderWasSureOfIsWrong` on `noisy-18wpm` and on `interference-18wpm`. They are outside
the set, 394 item 5's cases. The synthetics stayed 0 of 2 off disk, as the instruction expected.
Decision 4's first branch applied, with no pin, and the change was committed.

**Task 2.** Band 0.02 was tried first and held, so 0.04 and 0.01 were not regenerated. `CwFixtureTests`
went from 16 green to 20 green, with `TheCleanRecordingsDecodeExactly` 2 of 2, the share and
confident-mistakes cases green on both clean names, and the drift guard 6 of 6. Captures were 37 of
37 with every row identical to entry, adjudicated 13 of 13. Decision 8 held. **Two slips of mine,
both caught before anything was committed from them.** The first writer fact lacked
`using Xunit;`, its build failed, and the script carried on and ran `CwFixtureTests` against the old
files and the new requests. That run judged no band, and step 2 was re-run whole from a script that
stops on a failed build. Then the first commit attempt named the deleted writer in `git add`. The
writer had never been tracked, so git refused the whole add and nothing was committed. The commit
was made again without that path. Both are 400 item 5.

**Task 3.** Started at minute 16, inside decision 13's clock. In memory at 0.02 the sibling printer
gave `W1AW DE K2ABC <BT> R TU <SK>`, 0 confident mistakes, 16 of 16. The file was regenerated the same
way. `CwFixtureTests` went to 22 of 23, the one red being `fading-18wpm`'s confident-mistakes case,
red since task 0. Captures and adjudicated were unmoved, and decision 8 held. The prosigns writer
was deleted with `rm -f` inside the script, because `git rm` refuses a file that was never tracked.

**Task 4.** Both lines, the three floor tests, every fixture-reading type and every caller type were
run; the numbers are in section 3. **No regression.** The ticks went in their own commit.

**Decisions applied**, all of the instruction's: 1 (step 3, the clean synthetics first); 2 (the trace);
3 (settled characters, option a); 4 (first branch, nothing costed, no pin); 5 (nothing under `src`
at any commit); 6 (entry lines from unit 399's exit); 7 (band 0.02); 8 (held for both repairs);
9 (temporary writer facts, run by filter). For decision 9, `git rm` was usable only on the first
writer, before the commit. The second was removed with `rm -f`, a how-to decision reported here.
10 (prosigns, held); 11 (the ticks); 12 (the doc, six sections); 13 (clock: task 3 at minute 16, task
4 at about minute 26); 14 (timeouts as listed).

**Self-rulings: none.** There were two how-to decisions, both uncapped and reported here. The 3.5
sentence says *app 277 of 278 in each of two runs ...* because no run printed 278, and it gives the
dispatcher-loop reason in place of the template's single `<n> of 278`. The prosigns printer is a
sibling file and does not add a second fact to `TheCleanSyntheticsFourWaysTests`, which section 5
said not to edit.

## 2. What the owner should expect

Nothing changed on the CW tab and nothing changed under `src`. The test harness that nearly every CW
fixture test reads through now collects the same characters the CW tab's transcript keeps: the
settled ones, from `CwDecoder.CharacterSettled`. Before, it collected the running leading edge,
which re-announces a letter every time the decoder revises it, so one letter could appear three
times in the text the tests compared. That follows your R12, that a session rewrites its own tests,
and the arbiter's answer to unit 399's question. The two clean synthetics, and the prosigns one,
now carry a quiet noise band under the tone, the way `fading-18wpm` and every off-air fixture
already do, under your HM-DEC-127 of August. **The third floor test is green, 2 of 2, for the first
time in the phase**, giving back `CQ DE W1AW K` at 12 and 18 wpm, every letter high. The prosigns
fixture went the same way and is green. The harness change cost nothing: every test green before is
green after, and two confident-mistakes cases on the noisy and interference fixtures went from red
to green. The decoder that worked on the air is unchanged, and its captures and adjudicated floors
are green and unmoved, row for row.

**What will look wrong but is not:** the app line printed 277 of 278 twice. Each time one different
name died in 1 ms on the Avalonia headless dispatcher loop before reaching an assertion, and each is
green in the other run, the same shape step 1's 1.5 was ticked on. The three `.wav` files show
the same byte sizes as before: same length, different samples. `CwSensitivityTests.TheDecoderReadsAsFarDownAsItDidBefore`
is red, and was red before this unit started.

## 3. What you should see

**The answer: `CwFixtureTests.TheCleanRecordingsDecodeExactly` 0 of 2 -> 2 of 2**, from repair
commit `7d1ffde6` onward. 3.5 is ticked on that commit.

**The caller table**, task 0 against the corrected harness at task 1, identical again at exit:

| Type | Cases | Task 0 | Corrected | Moved | Run |
|---|---|---|---|---|---|
| `CwFixtureTests` | 23 | 14 green, 9 red | 16 green, 7 red | `NothingTheDecoderWasSureOfIsWrong` noisy and interference, red to green | yes |
| `CwAcquisitionWindowTests` | 12 | 10 green, #6 and #15 red | the same | nothing | yes |
| `CwSensitivityTests` | 2 | 1 green, `TheDecoderReadsAsFarDownAsItDidBefore` red | the same | nothing by case; emitted count 19 or 20 to 9 per level | yes |
| `EveryCharacterCarriesItsOwnEvidenceTests` | 3 | 3 green | 3 green | nothing | yes |
| `WhereAcquisitionPointsTests` | 2 | 2 green | 2 green | nothing | yes |
| `CwRefusalFloorTableTests` | 1 | 1 green | 1 green | nothing | yes |
| `TheCleanSyntheticsFourWaysTests` | 3 | - | 3 of 3, asserts nothing; `TEXT` and `SETTLED` agree on every row | - | once, for the record |
| `TheCwBaselineTable`, `TheTwoStationTable`, `TheIntegratorBandwidthTable` | 1 each | - | - | - | **not run**: they write an `ANALYSIS-cw-*.md` page at the root and assert only that it exists |
| `TheOperatorIsToldAboutASecondStationTests` | - | - | - | - | not run: builds its own decoder, does not call the harness |

**`CwFixtureTests`, all 23 cases:**

| Case | Task 0 | Task 1 | After 0.02 on clean | After prosigns |
|---|---|---|---|---|
| `EveryFixtureIsOnDisk` | green | green | green | green |
| `EveryFixtureIsStillTheAudioItWasGeneratedFrom` x6 | green | green | green | green |
| `EveryRecordingGivesBackTheShareItShould` clean-12wpm #25, clean-18wpm #26 | red | red | green | green |
| ... fading, interference, noisy | green | green | green | green |
| ... prosigns-18wpm #30 | red | red | red, 1 of 16 | green |
| `NothingTheDecoderWasSureOfIsWrong` clean-12, clean-18, prosigns | green | green | green | green |
| ... fading-18wpm | red | red | red | red |
| ... interference-18wpm, noisy-18wpm | red | green | green | green |
| `TheCleanRecordingsDecodeExactly` clean-12wpm #31, clean-18wpm #32 | red | red | green | green |
| `TheProsignRecordingDecodesItsProsigns` #33 | red | red | red | green |
| `TheWholeSetStaysSmallEnoughToCommit` | green | green | green | green |
| **Total** | 14 / 9 | 16 / 7 | 20 / 3 | **22 / 1** |

**The harness hunk**, `tests/Hamlet.RadioEngine.Tests/Cw/CwDecodeHarness.cs`, plus an eight-line
remark on `Decode(MonoAudio, ...)`:

```diff
-        decoder.CharacterDecoded += characters.Add;
+        decoder.CharacterSettled += characters.Add;
```

**The app lines**, read and not written. At HEAD, `MainWindowViewModel.cs` 11117
`_decoder.LeadingEdge += Transcript.OfferEdge;`, 11118 `_decoder.CharacterSettled += Transcript.Settle;`,
and 11123 `_decoder.CharacterDecoded += _ =>`, which sets two timestamps. At `7e209cb4`: 3251
`_decoder.CharacterSettled += Transcript.Settle;` and 3256 `_decoder.CharacterDecoded += _ =>`.

**The band and the request lines:** 0.02, the first tried.

```diff
-            new CwSignalRequest(Call, WordsPerMinute: 12),
+            new CwSignalRequest(Call, WordsPerMinute: 12, NoiseAmplitude: 0.02),
-            new CwSignalRequest(Call, WordsPerMinute: 18),
+            new CwSignalRequest(Call, WordsPerMinute: 18, NoiseAmplitude: 0.02),
-            new CwSignalRequest("W1AW DE K2ABC ^BT R TU ^SK", WordsPerMinute: 18),
+            new CwSignalRequest(
+                "W1AW DE K2ABC ^BT R TU ^SK", WordsPerMinute: 18, NoiseAmplitude: 0.02),
```

`Clean: true`, `Sent`, the speeds, `ReadableShare` and every assertion are unchanged.

**The prosigns fixture:** off disk, it gave 1 of 16 at 8 wpm with no `<BT>` or `<SK>`. In memory at
0.02 it gave `W1AW DE K2ABC <BT> R TU <SK>`, 16 of 16, all high, 18 wpm, no confident mistake. It was
regenerated, and #30 and #33 are green.

**The carry-forward lines and floors:**

| | Entry | Exit |
|---|---|---|
| app line | 278 of 278 in 166 s, unit 399's exit under decision 6 | 277 of 278 in 161 s; re-run 277 of 278 in 166 s; the losses were `TheCqPressWritesTheLabelTheOperatorPressed(label: "Olivia")` then `ThePlainFixtureTakesGeneralFromTheFixedAnswer`, dispatcher loop, 1 ms, each green in the other run |
| engine line | 176 of 176 in 374 s, the same | 176 of 176 in 374 s of 480 |
| captures | 37 of 37 in 94 s | 37 of 37 in 92 s, every row identical |
| adjudicated | 13 of 13 in 29 s | 13 of 13 in 29 s, identical |
| clean synthetics | 0 of 2 in 4 s, `■ ■ ■ ■ ■  ■ ■ ■ ■■` and `■ ■ ■  ■■■` | **2 of 2** |

Decision 6's diff, `git diff --stat 80b1aa3e HEAD -- src tests docs/carry-forward-tests.txt`, printed
nothing at task 0.

**Tree at exit:** the transmit files show nothing against `7e209cb4`, and `src` shows nothing
against `5688a8a5`. `git diff --stat 0aa08d32 HEAD -- tests` lists exactly `CwDecodeHarness.cs`,
`CwFixtures.cs`, `TheProsignsFixtureAtABandTests.cs` and the three `.wav` files. `git status --short tests`
is empty, and there is no `ANALYSIS-cw-*.md` page. `git worktree list` shows the root and the three
preflight trees.

**The ticks as written** are in `PHASE_PLAN.md` and quoted in `docs/phase-cw/unit400-harness.md`
section 6. 3.5 is ticked with the R53 qualification, naming `7d1ffde6` and the condition that it
stands only while every later commit of the step keeps all three tests and both lines green. 3.1
gains the clause for #25, #26, #31, #32 and #30, #33, with the set at 37 green and 14 red-open. 1.3
gains *the two clean synthetics green 2 of 2 from unit 400.* 3.4 is not touched.

**Section 5 mismatches, reported, not repaired:**
- `PHASE_STATUS.md` read `CURRENT_STEP: 0` and `WORK_INSTRUCTION: 399 - the clean synthetics get a band`. It was set to 3 and `400 - the harness reads the transcript`. Its `STEP: 0` and `STEP: 2` lines still read `not started`, and step 4 `partial`; those are the layer's.
- `PHASE_OUTCOME.md` holds step 0 and step 2 at `not started` and steps 3 and 4 at `partial`, with the paired `UNIT 399` and `UNIT 5` entries. Only this unit's entry was appended, and it rode whole in task 0's commit along with `PHASE_STATUS.md`.
- `CwDecodeHarness.cs` was 90 lines, not 91; every quoted line number held.
- `TheIntegratorBandwidthTable.cs` is under `Cw\Fixtures\`.
- `PARKED.md` carried 44 bullets, one struck.
- `CLAUDE.md` §1's top row is HM-DEC-167 at line 360. `PROJECT_STATUS.md` says HM-DEC-165.
- Everything else in section 5 held.

**Git hygiene:** the layer's uncommitted root files, `tools/arbiter/*` and `.run-unit/` are left as
found. The Gmail, Google Calendar and Google Drive connectors in this session need authorizing in
claude.ai's connector settings. This unit did not use them.

## 4. What's blocking us

Nothing. No criterion of step 3 waits on a ruling: 3.4 waits on the 14 red-open repairs R49 already
orders, one at a time. The findings that block nothing, 400 items 1 to 5, are in
`docs/phase-cw/PARKED.md`.
