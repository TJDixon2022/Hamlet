```
READ IN THIS ORDER.

A. The phase goal - CW decodes again. Steps 0, 1 and 2 are ticked on every
   criterion; the outcome file holds 0 and 2 at not started and 1 at done,
   a layer mismatch reported here; step 3 is this unit's, at 3.1; steps 4
   and 5 are not started. After this unit step 3 is in progress with 3.1
   met, and 3.3 met with it.
B. The criteria, one line each, met or not:
   3.1 met - all 51 names run by type at HEAD and classified with numbers in
       docs/phase-cw/unit394-reds.md: 30 green, 21 red-open, none repaired,
       retired or unmeasured.
   3.2 not met, vacuous - no retirement was made, so nothing to quote and no
       docs/cw-retired-tests.txt; left open.
   3.3 met - no audio-reading test retired; every one of the 21 reds reads
       audio and is listed red-open with its number and what it asserts.
   3.4 not this unit's - the known-reds block is unchanged, decision 7.
   3.5 not ticked, the step's exit - at this unit's exit the floors are 37 of
       37 and 13 of 13, both lines green but for dispatcher-loop losses with
       no red on an assertion, and the synthetics red at both ends under R53.
C. The report last. Section 4 raises 29 items - unit 390's nine, unit 391's
   five, unit 392's four and unit 393's five carried, and this unit's own
   six - and none is in the way of a criterion in B. Task 3, the clean
   synthetics measured four ways, was dropped on its own clock rule and is
   the next unit's first measurement.
```

```
UNIT:       394 - complete at task 4 of 5, tasks 0, 1, 2 and 4, task 3 dropped on its clock rule - 2026-09-22 23:00
PHASE GOAL: get the CW decoder reading again from the last code that read, keep it from breaking silently, clear the inherited CW reds one at a time under R49, judge the August rework on numbers, and end with Tim hearing it read at the radio
UNIT GOAL:  run every one of the 51 inherited CW reds against the restored decoder, one type per invocation, and put a classification and a number beside every name, so step 3 repairs from a list and not a guess
ADVANCED:   yes - step 3 criterion 3.1 is ticked on measured runs, and 3.3 with it
NUMBER:     of the 51 inherited CW names, classified at HEAD: 0 -> 51; green 30, red-repaired 0, red-retired 0, red-open 21, unmeasured 0; engine line 375 s -> 371 s of 480
DRIFT:      0
```

## 1. What Claude did

**Complete at task 4 of 5. I ran tasks 0, 1, 2 and 4. Task 3 was dropped under its own clock
rule: task 2 ended at 22:44, 41 minutes into the hour that began at 22:03, leaving 19 minutes
against the rule's 25. Task 3 was the named drop candidate.** QUIVERFULL, `C:\Source\HamLet`,
Hamlet confirmed by the gate's four checks, branch `main`.

**Task 0, the record and the entry round (`02c64ae2`).** I appended `## UNIT 394 - STEP 3` to
`PHASE_OUTCOME.md` with the decision block's fields. `PHASE_STATUS.md` read `CURRENT_STEP: 0` and
`WORK_INSTRUCTION: 393 - the list is green, and CW is on it`; I set them to 3 and
`394 - the pile is counted, name by name`. The version went from 1.13.80 to 1.13.81. The entry
round ran both lines of `docs\carry-forward-tests.txt` as its comment says, one build each, with a
status line before each: app 278 of 278 in 170 s; engine 176 of 176 in 375 s, of which 26 are CW
(176 less 150). The floors, one type per invocation: captures 37 of 37 in 97 s, with `001520`
and `013637` of the failing set green in it; adjudicated 13 of 13, 30 s of test time; clean
synthetics 0 of 2 in 5 s, the `■` placeholders R53 expects. The eleven transmit files printed
nothing against `7e209cb4`. **Slip:** my edit adding the `ENTRY` line to `PHASE_OUTCOME.md` failed
on an ambiguous match, so task 0's commit went without it. It rode in task 1's commit, and the
line says so.

**Task 1, the pile run by type (`c7fc1ecf`).** I ran thirteen invocations, one per compiled type,
each `--no-build` after task 0's engine-line build (decision 5), with `timeout 600`, the detailed
logger, and a status line naming the type and its ordinal. No run died before an assertion and
nothing was re-run. **`OneDecoderNotTwoTests` did not finish inside 600 s.** The whole-type run
printed 81 green and no red before `timeout` killed it at 601 s, and every case of
`ListeningAndFeedingReadTheSame` was green, the set's three among them. Under decision 5 I split it
by method and ran `TheBufferSizeChangesNothing` alone. That run printed 47 green and no red before
it too was killed. Six of its 53 cases are unmeasured at the cap, and none is in the set.
**Deviation, reported:** the first call, at `timeout 600` plus the status line, passed the
harness's 600 s foreground cap by about a second, and the harness moved it to the background. I
did not poll it; the notice of its end was the only read. I ran the split at `timeout 580` so it
stayed in the foreground. `ABlipDoesNotShiftEverythingAfterItTests` was not run, because it is
excluded from compilation, and I read it instead (decision 4).

**Task 2, the classification (`73b0bec0`).** `docs\phase-cw\unit394-reds.md` sections 1 to 4:
**30 green, 0 red-repaired, 0 red-retired, 21 red-open, 0 unmeasured.** No red has a wiring
cause. All 20 that ran fail on a decode result the console printed: characters, share or speed.
The one excluded file reads audio and asserts characters, so R49's second sentence forbids retiring
it. **No repair and no retirement was made, and `docs\cw-retired-tests.txt` was not created.**
3.1 and 3.3 are ticked in `PHASE_PLAN.md`; 3.2 is left open as vacuous.
`git diff --stat 02c64ae2 HEAD -- src` prints nothing.

**Task 4, the exit round (`af4ce62b`).** App line 277 of 278 in 172 s, with 1 lost to the
headless dispatcher loop. I re-ran it once: 275 of 278 in 169 s, with 3 lost the same way. Each
lost name was green in the other run, and nothing went red on an assertion. Engine line 176 of
176 in 371 s. Floors 37 of 37, 13 of 13, and synthetics 0 of 2 reading the same placeholders as
at entry. The transmit files printed nothing against `7e209cb4`, and `src` printed nothing since
`02c64ae2`. **There is no regression.** Section 5 of the doc.

**Author's decisions applied, all overrulable:** 1, step 3's entry taken as satisfied on step 2's
four ticks, with the outcome file's *not started* reported, not repaired; 2, the set is 51
distinct names run by type, and the known-reds block adds none; 3, the four classifications as
written, of which only green and red-open occurred; 4, the excluded file classified from its
source, red-open; 5, `--no-build` and `timeout 600` per type with a split by method on a timeout,
applied to `OneDecoderNotTwoTests` at 580 s as above; 6, not applied, because task 3 was dropped;
7, nothing on or off either line, and the known-reds block untouched; 8, the task 0 and task 4
floor runs cover every commit; 9, the timeouts as listed. **I made no self-ruling.** Every
repair: none. Every retirement: none. Every regression: none.

## 2. What the owner should expect

Nothing changes on the CW tab. This unit ran tests and changed nothing under `src`. The pile of
CW tests called *inherited* since August now has a result beside every one of its 51 names.
**30 of them are green on the decoder you heard read on 2026-08-25.** Green means each
assertion held, not that CW works. **21 are red on a decode result and wait their turn.** 14 of
those assert the characters read, 5 the share of a message, and 2 a speed. The largest group is
generated audio of `CQ DE W1AW K` that comes back as `■` placeholders or the wrong letters: the
two clean synthetics, the six `CwDisplacementFloorTests`, and the speed test on an 18 wpm signal.
Four receiver-tier recordings come back with the right tone and the wrong letters. **Nothing was
retired.** The one test that cannot compile decodes audio and compares text, and R49 keeps those.
Task 3 was dropped, so **where the two clean synthetics fail is still not measured.** The next
unit measures it first. What will look wrong but is not: the app line lost a few names to the
headless dispatcher loop at exit. That is the known lost run, and each lost name was green in the
other run.

## 3. What you should see

**The 51 names**, in the order of `docs\unit239-failing-set.txt`, prefix
`Hamlet.RadioEngine.Tests.Cw.` dropped, copied from `docs\phase-cw\unit394-reds.md` section 2:

