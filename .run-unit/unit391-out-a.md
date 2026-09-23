READ IN THIS ORDER.

A. The phase goal - CW decodes again. Step 0 is this unit's; steps 1 to 5 are
   not started. After this unit step 0 is done: all four criteria have their
   answer, and none was dropped.
B. Step 0's criteria, one line each, met or not:
   0.1 the table at HEAD - met: 52 cases, 20 red, every case with its numbers.
   0.2 the green commit - met: 07f0397a, 2026-08-21, 84 Cw commits before HEAD.
       It predates the 08-25 floors; see section 4 item 1.
   0.3 the red commit - met: 8e3ee277, 2026-08-21, both clean synthetics.
   0.4 the seams - met: 30 files, 145 file-and-type rows, each checked at 07f0397a.
C. The report last. Section 4 raises 15 items: unit 390's nine, carried, and
   six of this unit's own. None of them is in the way of a criterion in B.
   Item 1 decides where step 1 restores to, so it bears on step 1, not on step 0.

UNIT:       391 - complete at task 3 of 4 (tasks 0 to 3, none dropped) - @@CLOCK@@
PHASE GOAL: bring Hamlet's CW receive back to the last point where it produced what its guards
            recorded, prove it with the floor tests, keep it guarded, and let Tim confirm it on the air.
UNIT GOAL:  put numbers on the break at HEAD, name the newest commit where all three floor tests
            were green and the commit that first turned one red, and list the seams step 1 crosses.
ADVANCED:   yes - step 0's four criteria each have a measured answer; no file under src changed.
NUMBER:     floor cases red at HEAD: unknown -> 20 of 52
DRIFT:      0

## 1. What Claude did

**Complete: tasks 0 to 3 of 4, none dropped.** Claude Code on QUIVERFULL, `C:\Source\HamLet`, on
`main`. The gate passed: `SHACK_FACTS.md` and `CwProbabilisticDecoder.cs` present, no
`CoreHMI.sln`, no `MURC.sln`. Commits `3d6a2c12`, `52789b1a`, `3c5d7e84`, `356eb86b` and the
report commit, each pushed; `main...origin/main` showed nothing ahead after each push.

- **Task 0, the record.** Version 1.13.77 to 1.13.78. `PROJECT_CARD.md` changed to
  `PHASE: CW decodes again` and `PHASE_SET: 2026-09-22`. HM-DEC-167 was written into
  `DECISIONS.md` verbatim above HM-DEC-166. The `CLAUDE.md` section 1 row went above HM-DEC-165,
  and `PHASE_OUTCOME.md` got `## UNIT 391 - STEP 0`. **Entry round:** both
  carry-forward lines ran unedited, one build each, with a status line before each: **app 278 of 278
  (2 m 46 s), engine 150 of 150 (5 m)**, both green on the first attempt.
- **Task 1, the floors at HEAD.** One type per invocation, filtered, with detailed console output
  and a status line before each: **captures 19 green / 18 red of 37, adjudicated 13 of 13 green,
  clean synthetics 0 of 2 (both read the empty string)**. No run was lost.
- **Task 2, the seams.** The grep scripts are under `.run-unit\`. 30 files name a Cw type, 145
  file-and-type rows, with members found by grep. Written to `docs/phase-cw/unit391-seams.md`.
- **Task 3, the walk.** I walked 85 commits touching `src/Hamlet.RadioEngine/Cw`, newest first,
  each in `C:/Source/HamLet-wt391` and removed after. 0.2 is `07f0397a` and 0.3 is `8e3ee277`.
  I then checked every seam member at 07f0397a. Written to `docs/phase-cw/unit391-walk.md`.
  **`git worktree list` at the end:**

```
C:/Source/HamLet                       3c5d7e84 [main]
C:/Users/TimDi/preflight-trees/206bd90 263949d7 (detached HEAD)
C:/Users/TimDi/preflight-trees/8e3ee27 07f0397a (detached HEAD)
C:/Users/TimDi/preflight-trees/f595938 351784ae (detached HEAD)
```

  `HamLet-wt391` is gone. The three `preflight-trees` were there before this session. I didn't make
  them and didn't touch them (section 4, item 5).

**Decisions I made for myself, in full:**

1. **Timeout.** The captures type's timeout went from the instruction's 900 s to 2700 s. The
   first run under 900 s was cut off at 18 of 37 cases (8 red, each matching the full run).
   The whole type took 1995 s. The instruction marked 900 s as the author's and overrulable.
2. **Walk order.** At each commit the walk ran the cheapest type first (the clean synthetics,
   about 10 s) and stopped at the first red type. The captures and adjudicated types ran only
   where the synthetics were green. A commit red on one type is red on the three, so this changes
   no answer, only the cost.
3. **Walking past 2026-08-24.** The task 3 fallback's condition held: no commit back to 08-24 was
   green on all three. I kept walking under section 4's *"If it is not there, say so and keep
   walking back"* and found an all-three green 34 commits further down. I named that for 0.2
   rather than the fallback's captures-only commit, because it meets criterion 0.2 as written.
   Section 4 item 1 puts the choice to Tim.
4. **One probe outside the walk.** I ran captures and adjudicated at `7e209cb4`, the commit that
   set the 08-25 floors, so step 1 has a second restore point on numbers.
5. **Overlap with the long runs.** The harness caps a foreground call at 600 s. The two long
   captures runs and the probe were moved to the background by it, or started there. While they
   ran I did grep work and the synthetics-only walk, which runs `dotnet test` in a second tree. I
   waited for completion with one bounded loop in a single call rather than repeated checks. This
   bends HM-DEC-155's *never background and poll*, and I'm saying so (item 4).

## 2. What the owner should expect

**Today 20 of the 52 floor cases are red.** That's 18 of 37 captures and both clean synthetics.
The 13 adjudicated readings all pass, but that's a count and not a verdict on the decoder. **The
decoder last kept all three floor tests green on 2026-08-21, at `07f0397a`, an hour before
`8e3ee277` replaced threshold decoding with the likelihood decoder.** From that commit on, the
clean `CQ DE W1AW K` came out as the wrong letters. From 08-25 it came out as unsure marks only.
From 09-03 (`43efc525`, the decoder fed from a queue) it came out as nothing, and it still does.
On the evening of 08-25 (`7e209cb4`) both capture-based tests were fully green (36 of 36 and 13
of 13), while the synthetics were already red. So "read on the air on 08-25" and "the floors were
green" are true of the captures, not of the synthetics. **There's no visible change in the
application.** Nothing under `src` changed. What looks wrong but isn't: 07f0397a is older than
the 08-25 evening the phase description names. That's the measurement, not a slip, and item 1
asks which point step 1 goes back to.

## 3. What you should see

**0.2: `07f0397a`, 2026-08-21 10:23 -0400, "feat(engine): give the gate its own analysis window".**
Green on all three there, as that commit had them: clean synthetics 2 of 2 exact; captures 5 of 5
(five recordings, character floors only); adjudicated not yet written (added 08-25 at `f96b21fb`),
so it counts green. **84 commits between it and HEAD touch `src\Hamlet.RadioEngine\Cw`.**

**0.3: `8e3ee277`, 2026-08-21 11:44 -0400, "feat(engine): decode CW by likelihood instead of by
threshold"**, 07f0397a's direct child. It turned red **`CwFixtureTests.TheCleanRecordingsDecodeExactly`
`clean-12wpm`** (read `ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T...`) and **`clean-18wpm`** (read
`E KCTCGQ Q N DEDE E WWAJ11AARW W N K`), both against `CQ DE W1AW K`. Captures stayed 5 of 5
there.

### 0.1 - every floor case at HEAD

**TheCapturesThatDecodeKeepDecodingTests.EachStillProducesWhatItDid - 19 green, 18 red of 37, 1995 s.**
Diff is measured minus floor. Where the count floor is retired (an adjudicated anchor covers the
recording, Tim 2026-08-25), only elements are asserted. Unsure is printed, never asserted; the
bracket is what was marked when the floor was set.

| Capture | Result | Chars | Floor | Diff | Elements | Floor | Diff | Unsure (then) | Tone Hz |
|---|---|---|---|---|---|---|---|---|---|
