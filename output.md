```
READ IN THIS ORDER.

A. The phase goal - CW decodes again. Steps 0, 1, 2 and 4 are ticked on
   every criterion; the outcome file holds 0 and 2 at not started and 4 at
   partial, a layer mismatch reported here, and the launcher named step 0
   for this unit on that reading; step 3 is this unit's, partial at 3.1,
   3.2 and 3.3 at entry with 20 reds open in the set, every one a decode
   result, and the third floor test red at every commit of the phase;
   step 5 is Tim's. After this unit the third floor test is still red,
   0 of 2, because no band under the tone meets its exact-text assertion:
   the harness text carries every leading-edge revision. The set stands
   at 31 green and 20 red-open, and 3.4 and 3.5 still wait on the 20
   repairs left.
B. The criteria, one line each, met or not: 3.1 met by unit 394, its
   sentence gaining no clause because nothing went green; 3.2 met by unit
   398, nothing retired here; 3.3 met, nothing retired here; 3.4 not met,
   the known-reds block and the set's closing line untouched, 20 reds
   left; 3.5 not ticked, the step not at its exit, floors 37 of 37, 13 of
   13 and 0 of 2 at task 4 with no file under src changed. The unit named
   no criterion in ADVANCES, and it cleared nothing: the band was measured,
   did not meet the assertion, and was not applied.
C. The report last. Section 4 raises 1 item, and it is in the way of the
   third floor test and so of 3.4 and 3.5: which text the exact-text
   assertion should read. Everything carried from before this phase and
   every finding that blocks nothing is in docs/phase-cw/PARKED.md under
   R54, not here.
```

```
UNIT:       399 - complete at task 4 of 5, none dropped, task 3 not reached on its condition - 2026-09-23 03:42
PHASE GOAL: get the CW decoder that worked on the air in August working again, proved by three named floor tests and finally by Tim at the radio
UNIT GOAL:  measure the two clean synthetics four ways before touching them, then make the third floor test green by giving the two fixtures a noise band, leaving the assertion and the decoder alone
ADVANCED:   no - a blocker cleared, not a criterion: not cleared, because no band of 0.01, 0.02 or 0.04 meets the exact-text assertion; the decoder's settled text is exact at all three, but the harness text the test compares carries the leading-edge revisions
NUMBER:     clean synthetics 0 of 2 -> 0 of 2, no band applied; way 4 cost 0 captures and 0 anchors, moved 001520 from 5 to 41 characters; set names green 31 -> 31, red-open 20 -> 20; prosigns not reached; floors 37 of 37 and 13 of 13 at both ends; engine line 374 s -> 374 s of 480
DRIFT:      0
```

## 1. What Claude did

**Complete at task 4 of 5.** Tasks 0, 1, 2 and 4 ran. Task 3 was not reached because it runs only
after task 2 regenerates a fixture, and task 2 did not. It was not dropped on the clock: task 2
finished at 03:25, minute 11 of the unit. Windows 11, Hamlet confirmed by the gate, branch `main`.

**Commits, all pushed to `origin main`:**

- `d3d373d3` task 0: the entry round, version 1.13.85 to 1.13.86, `PHASE_STATUS.md`, and the
  `PHASE_OUTCOME.md` entry. `PHASE_OUTCOME.md` and `PHASE_STATUS.md` went in whole, with the layer's
  earlier uncommitted edits, as in units 393 to 398.
- `9280c2cb` task 1: the printer `tests\Hamlet.RadioEngine.Tests\Cw\TheCleanSyntheticsFourWaysTests.cs`,
  doc sections 1 and 2, and `394 item 1` struck through in `PARKED.md`.
- `92a75c6d` task 2: doc sections 3 to 5 only. Nothing regenerated.
- `80b1aa3e` task 4: the exit round, doc section 6, `PARKED.md` 399 items 1 and 2, and the
  `PHASE_OUTCOME.md` task lines, which a failed edit had left out of `92a75c6d`.
- The report commit carrying this file and `PROJECT_STATUS.md`.

**What was measured** (full tables in `docs\phase-cw\unit399-synthetics.md`):

- **Way 1**, the files off disk as the floor test reads them. `clean-12wpm`: 10 letters, all
  unreadable, 8 wpm. `clean-18wpm`: 6 letters, all unreadable, 18 wpm. The texts are the `■` marks
  R53 expects.
- **Ways 2 and 3**, the same requests in memory with `NoiseAmplitude` at 0.02, 0.01 and 0.04. The
  settled text (`CharacterSettled`) was `CQ DE W1AW K` exactly on both fixtures at every band, at
  12 and 18 wpm, with every letter high (29 of 29 and 20 of 20). The harness text
  (`CharacterDecoded`), which is what `TheCleanRecordingsDecodeExactly` compares, was
  `QQQ T DDEDE  A WWEWRJ11E1AAAWW W T KK` and `Q N DEDE E WWAJ11AARW W N K` at every band.
  `CwDecoder.cs` line 266 documents that event as *the same leading edge, one character at a time*,
  and the harness appends every revision. So the size of the band does not matter; the text source
  does.
- **Way 4**, the uncommitted `Estimate` hunk, quoted in the doc: sigma at one hundredth of the 97th
  percentile where the quarter point is nought. The synthetics stayed 0 of 2 with the same
  revision-laden text. Captures 37 of 37, with one row moved: `cw-2026-08-23-001520`, the capture
  the decoder's own comment warns about, went from 5 characters, 45 elements and 4 unsure to 41, 78
  and 0. Adjudicated 13 of 13, every line identical. **Put back** by `git checkout --
  src/Hamlet.RadioEngine/Cw` in the same script. `git diff --stat 5688a8a5 HEAD -- src` and
  `git status --short -- src` both printed nothing, and the restored tree was rebuilt before
  anything else ran.
- **History check** after task 4: `CwDecodeHarness` already took `CharacterDecoded` at
  `8e3ee277~1`, when this test was last green. What changed is what the decoder emits on that
  event, not the harness.

**Decisions applied.** 1: step 3 on the clean synthetics. 2: four ways through one printer that
asserts nothing, kept in the tree. 3: no band met the assertion on both fixtures, so nothing was
regenerated and the decoder route was not started. 4: no commit touches `src`. 5: the diff was
empty, so unit 398's exit runs are the entry lines. 6: not reached, since there was no
regeneration to judge. 7: not reached. 8: no clause added to 3.1 or 1.3, because nothing went
green, and `PHASE_PLAN.md` was not edited. 9: the doc, with six sections. 10: no writer fact was
written. 11: the clock never bound. 12: timeouts as listed.

**Decisions made for myself, uncapped, about how to carry out assigned tasks:**

- **Exact** was judged as the floor test judges it, on the harness text. The settled text is
  recorded beside it and is not used to choose a band.
- The printer takes the settled text from a second pass over the same audio, because the harness
  does not expose `CharacterSettled`.
- Decision 2 lists a medium count, but `CwConfidence` has no `Medium`, so the tables carry high,
  low and unreadable.
- **No self-rulings** that authorize work outside the tasks.

**Regressions: none.** The first exit app run lost 3 names to the headless dispatcher loop, each
`InvalidProgramException` at 1 ms, and the re-run was 278 of 278.

## 2. What the owner should expect

Nothing changed on the CW tab and nothing changed under `src`. The two clean synthetics are
**unchanged**. The plan was to give them a quiet band under the tone, the way every off-air fixture
already has one, following your HM-DEC-127 ruling on `ASignalAtTheWrongPitchIsStillFound`. The band
was measured and not applied, because it would not have turned the third floor test green. With a
band at any of three levels, the decoder's final characters come out as exactly `CQ DE W1AW K`, at
the right speed and all sure. But the test compares the decoder's running leading edge, and that
repeats a letter each time it is revised (`QQQ` for `C`), so the text can never equal what was sent.
The third floor test is still red, 0 of 2, with the same `■` texts as before. Making the decoder
accept exact digital silence would have cost no floor case, but it did not turn the test green
either, and it moved one capture, `001520`, from 5 characters to 41, which is the all-silence
capture the decoder's comment warns about. The prosigns fixture was not touched. The decoder that
worked on the air is unchanged, and its floors are green: 37 of 37 and 13 of 13 at both ends.

**What will look wrong but is not:** a new test type, `TheCleanSyntheticsFourWaysTests`, that
asserts nothing. It is the record of this measurement and is on no carry-forward line.

## 3. What you should see

**No visible change.** This unit measured, and the answer to its question is **no**: a noise band
alone does not turn `TheCleanRecordingsDecodeExactly` green.

**The four ways**, both fixtures:

| Way | Fixture | Band | Harness text | Settled text | wpm | Letters | High | Low | Unreadable | SnrDb | Tone |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | clean-12wpm | none | `■ ■ ■ ■ ■  ■ ■ ■ ■■` | `■■ ■ ■` | 8 | 10 | 0 | 0 | 10 | 85.9 | 600 |
| 1 | clean-18wpm | none | `■ ■ ■  ■■■` | `■ ■ ■ ■ ■ ■■■■■ A  ■ ■ ■■` | 18 | 6 | 0 | 0 | 6 | 85.9 | 600 |
| 2 | clean-12wpm | 0.02 | `QQQ T DDEDE  A WWEWRJ11E1AAAWW W T KK` | `CQ DE W1AW K` | 12 | 29 | 29 | 0 | 0 | 49.6 | 600 |
| 2 | clean-18wpm | 0.02 | `Q N DEDE E WWAJ11AARW W N K` | `CQ DE W1AW K` | 18 | 20 | 20 | 0 | 0 | 49.7 | 600 |
| 3 | clean-12wpm | 0.01 | as way 2 | `CQ DE W1AW K` | 12 | 29 | 29 | 0 | 0 | 55.5 | 600 |
| 3 | clean-18wpm | 0.01 | as way 2 | `CQ DE W1AW K` | 18 | 20 | 20 | 0 | 0 | 55.7 | 600 |
| 3 | clean-12wpm | 0.04 | as way 2 | `CQ DE W1AW K` | 12 | 29 | 29 | 0 | 0 | 43.7 | 600 |
| 3 | clean-18wpm | 0.04 | as way 2 | `CQ DE W1AW K` | 18 | 20 | 20 | 0 | 0 | 43.6 | 600 |

**Way 4**, the hunk quoted in doc section 2, applied uncommitted and put back:

- Synthetics: 0 of 2, with the texts as way 2.
- Captures: 37 of 37, with `001520` moved from 5, 45 and 4 unsure to 41, 78 and 0. The other 36
  rows are identical.
- Adjudicated: 13 of 13, identical.
- **Cost: 0 captures and 0 anchors by pass or fail, and 1 capture moved.**

**The band and the request lines:** no band was chosen. The lines stand as
`new CwSignalRequest(Call, WordsPerMinute: 12)` and `new CwSignalRequest(Call, WordsPerMinute: 18)`,
and the `.wav` files were not written.

**`CwFixtureTests`, 23 cases**, identical by name at task 0 and task 4:

- **14 green:**
  - `EveryFixtureIsOnDisk`
  - the drift theory, 6 of 6
  - `NothingTheDecoderWasSureOfIsWrong` on clean-12wpm, clean-18wpm and prosigns-18wpm
  - `EveryRecordingGivesBackTheShareItShould` on noisy, fading and interference
  - `TheWholeSetStaysSmallEnoughToCommit`
- **9 red:**
  - `TheCleanRecordingsDecodeExactly` clean-12wpm #31 and clean-18wpm #32
  - `TheProsignRecordingDecodesItsProsigns` #33
  - the share theory on clean-12wpm #25, clean-18wpm #26 and prosigns-18wpm #30
  - `NothingTheDecoderWasSureOfIsWrong` on noisy, fading and interference, which are outside the
    set (394 item 5)

**The other two fixture-reading types**, both identical at entry and exit:

- `TheCleanReadsStayCleanTests`: 6 green, 1 red-open on `003758`.
- `TheSurveyAlreadyUsesAShortWindowTests`: 2 of 2.

**Prosigns:** not reached. #30 and #33 stay red-open.

**Carry-forward and floors:**

| Run | Entry | Exit |
|---|---|---|
| App line | 278 of 278 in 160 s, unit 398's exit | 275 of 278 in 162 s with 3 dispatcher-loop losses; re-run 278 of 278 in 166 s |
| Engine line | 176 of 176 in 374 s, unit 398's exit | 176 of 176 in 374 s of 480 |
| Captures | 37 of 37 in 94 s, identical to unit 398's exit | 37 of 37 in 92 s, identical to entry |
| Adjudicated | 13 of 13 in 29 s | 13 of 13 in 29 s |
| Clean synthetics | 0 of 2 in 4 s | 0 of 2 in 3 s |

Decision 5's diff, `git diff --stat 7a297abf HEAD -- src tests docs/carry-forward-tests.txt`,
printed nothing.

**Tree checks at exit:**

- The eleven transmit files: nothing against `7e209cb4`.
- `src`: nothing since `5688a8a5`.
- `git diff --stat 7345a4f9 HEAD -- tests`: only `Cw/TheCleanSyntheticsFourWaysTests.cs`.
- `git status --short tests`: nothing.
- `git worktree list`: the root and the three preflight trees.

**Section 5 of the instruction against the tree.** Everything held except the following, all
parked as 399 item 1:

- `PHASE_STATUS.md` read `CURRENT_STEP: 0` and `WORK_INSTRUCTION: 398`, and was set to 3 and 399.
- `CwConfidence` has no medium value.
- No `Fixtures\CwFixtureGenerator.cs` reads `CwFixtures.`, and the grep lists the four excluded
  files as well.
- `RayleighQuarterPoint` is a constant, not a method.

Also noted, not edited:

- `PHASE_OUTCOME.md` holds step 0 and step 2 at `not started`, step 1 `done`, and steps 3 and 4
  `partial`, with the paired entries (`UNIT 398` beside `UNIT 4`). The layer's files, left as
  found.
- `CLAUDE.md` §1's top row is HM-DEC-167 at line 360, while `PROJECT_STATUS.md` says HM-DEC-165
  because `tools/status.sh` writes it as a literal.

## 4. What's blocking us

**Which text should `TheCleanRecordingsDecodeExactly` compare with what was sent: the decoder's
leading edge, as today, or its settled characters?** This is in the way of the third floor test,
and so of 3.4 and 3.5.

- **Ruling wanted:** one of the following.
  - **(a)** `CwDecodeHarness` builds `Text` from `CwDecoder.CharacterSettled` rather than
    `CharacterDecoded`, and the two clean fixtures are then regenerated at 0.02 under HM-DEC-127
    as instruction 399 planned.
  - **(b)** The decoder's `CharacterDecoded` goes back to emitting each character once, which is a
    change under `src`.
  - **(c)** Leave it, and the two stay red-open.
- **Reasoning:**
  - On every banded run the settled text was `CQ DE W1AW K` exactly, at the sent speed, with every
    letter high. On that measurement (a) would turn #31 and #32 green without moving the assertion's
    text, its speed window or its confidence rule. Only the event it reads would change.
  - The harness read `CharacterDecoded` when the test was last green, before `8e3ee277`. So the
    event's meaning is what changed, and it changed with the decoder that worked on the air.
  - (a) is a test-side change under R12. But it changes the input of an assertion, and this
    instruction forbade that. It also touches the other compiled test files that call
    `CwDecodeHarness.Decode` (10 files grep to it), whose results have not been measured on the
    settled text.
  - (b) is a decoder change, which this unit's decision 4 excluded, and it risks the floors.
- **Rejected:**
  - A band alone: measured, and it changes nothing the assertion reads.
  - The way-4 decoder bypass: measured. It does not turn the test green, and it lets `001520` emit
    41 sure characters.

---

The findings that block nothing are parked as `399 item 1`, the section 5 mismatches, and
`399 item 2`, the three dispatcher-loop losses, in `docs\phase-cw\PARKED.md`.
