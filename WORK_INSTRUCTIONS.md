# Work instruction 433 - the mixdown follows a move only where the sender keys harder than at the mix

**Seed under `--seed`.** The 7.052 session opens with `UIEH EE E E T I NIEEE E` on the
spliced stream, and then it reads a whole QSO. Unit 429 traced this to `CwDecoder.cs` 617 to 621.
There, the mixdown followed the tracker from 600 to 525 Hz while the sender stood at 625, and
the speed collapsed after that. Three release rules have been tried against that line:
- **`5b6b704c`** (unit 430) held a move of more than 25 Hz until a later survey read confirmed it.
- **`ec76051e`** (unit 430) followed at once if the read just before the move confirmed it.
- **P39** (unit 432) followed once any read since the mix was set had confirmed it.

The first two cured the opening and went out on `cw-2026-08-22-032113`. P39 was never built.
Its replay gate failed on `031905`. Unit 432 measured why no rule of that kind can work: **a
move confirmed by the survey looks the same whether it is right or wrong.** 031905's move at
13.04 s and 032113's 650 Hz move have the same shape. Every one of the three rules asks one
question: did the survey see keying at the new pitch? The opening's 525 Hz move passes that
question too, because it was a `Switch` on its own confirming read.

**This unit asks a different question.** It compares the two pitches directly. The mix follows
a move of more than 25 Hz only when the decoder's own envelope keys harder at the new pitch
than at the pitch it is mixing at now. The comparison uses the same filter and the same three
seconds of audio for both pitches. The survey still says where keying is, and the tracker still
chooses. This rule only asks whether the new pitch reads better than the current one. It is
replayed before it is built, and it is kept only under 3.2's four tests. Four tasks; drop from
the back.

**Status.** `sh tools/status.sh`, real clock, after every commit and every task, and
immediately before every `dotnet test`. **Write files as UTF-8.**

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## 1. The rules that killed sessions

**HM-DEC-155.** Never run the whole suite. Run only this unit's named types and
`docs\carry-forward-tests.txt`, following its top comment. **Never background and poll.** Run one
type per invocation, each with its own `timeout`:
- the engine carry-forward line ran 372 s at unit 432's entry; give it 600 s;
- give the captures type (51 rows) 600 s;
- `WhatTheOpeningHeardTests` has 12 members now; give it 600 s.
  If it runs past 450 s, run its members one at a time;
- give each of unit 430's pitch types 600 s.

A run lost before any assertion counts neither way and is re-run once, alone. That covers the
test host crash inside `Cw` (HM-OPEN-063) and the headless dispatcher loop (P45).

**The report's four top-level headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`.

**Write `output.md` at the root before the session ends, whatever else happened.**

**Nothing in section 4 halts this phase** (R65). Park it and go on.

## 2. The tool facts

These are known limits of the shell here:
- Apostrophes in quoted heredocs break, and doubled backslashes collapse.
- `;` and `rm` are refused, and Python cannot run.
- A multi-line commit needs `-m` more than once.
- A bare `git worktree`, `git checkout` or `git show` is refused at the prompt.

Put multi-step commands in `.run-unit\unit433-<name>.sh` and run them with `sh`. Units 430 and 432
have scripts you can copy. **To take a change back out, commit a revert**
(`git revert --no-edit <sha>` in a script). Never reset or rewrite history.

## 3. Asks still outstanding

These are carried under HM-DEC-139. **None of them belongs to this unit.** P27 stays the
owner's. P29 to P45 stay parked. This unit takes up none of them.

Task 0 parks unit 432's two section 4 items verbatim, with their proposed rulings:
- **P46:** whether the replay gate's third clause is judged against the entry or against
  `5b6b704c`. This instruction's gate compares against the entry, as an author's decision
  (section 6). It does not rule on P39's route, which stays closed.
- **P47:** `WhyTheMixMoved` prints `CwToneSurvey.Analyze`'s verdict and not `Tracker.Verdict`.
  This is for the record.

This unit answers nothing beyond its own criterion.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  Stop the mixdown walking off the sender in a session's opening
            by comparing the two pitches on the decoder's own envelope,
            without costing 032113 or any other row, kept only under 3.2's
            four tests.
ADVANCES:   step 7 criterion 4
DRIFT:      0
```

**The count today:**
- Steps 0, 1 and 2 are done.
- Step 3 is partial and was closed under 3.4 by unit 431 (P43), with 3.6 open.
- Step 4 is not started.
- Step 5 is the owner's verdict.
- Step 6 is partial, with 6.5 open.
- Step 7 is partial, with 7.1, 7.2, 7.4, 7.6 and 7.8 open.

**Units 431 and 432 did not advance.** The loop has sent this instruction back to 3.6 or 7.4,
and it must use an approach the record does not show failing.

**Why 7.4 and not 3.6.** Four routes at 3.6 have failed: a span bar (421), features other than
span (425), span over neighbors (428), and G1's join (431). The one way back that is on record,
P44, is the owner's. At 7.4, three rules have been tried, and all three asked the survey the
same question. Unit 432 measured why that question cannot tell a right move from a wrong one.
A different question has not been asked.

**Why this is not a loop.** Units 430 and 432 varied *when* a survey confirmation releases a
held move. This rule does not read survey confirmations at all. It holds or follows on a
head-to-head measurement: the keying contrast of `CwProbabilisticDecoder.Envelope` at the target
pitch against the same figure at the current mix pitch. At the opening the sender is at 625,
which is 25 Hz from the mix at 600 and 100 Hz from the target at 525. That is the case this
measurement can see and a confirmation cannot. The loop test found no entry for this approach.

**If this unit keeps nothing, it is 7.4's third unit in a row with no kept change** (430, 432,
433). In that case task 3 writes the trace to `PARKED.md`, and 7.4 closes partial as the
criterion itself says.

---

## 5. Verify this instruction against the tree

Check each of these. Report any mismatch and repair nothing:

- `CwDecoder.cs` sets `_lastMeasuredToneHz` from `_tracker.ToneHz` at lines 600 to 603, and
  writes `_probabilistic.ToneHz` at 617 to 621. The file is identical to `a7e6e2f2`.
- `CwProbabilisticDecoder.Envelope(samples, sampleRate, toneHz)` is public and static, and it
  uses `IntegratorBandwidthHz` (45 Hz). The overload that takes a bandwidth exists beside it.
- `CwToneSurvey` keeps 3.0 s of history by default (`seconds = 3.0`).
- `WhatTheOpeningHeardTests.WhyTheMixMoved` and `WhenEachRuleFollows` are in
  `tests\Hamlet.RadioEngine.Tests\Cw`. The second of them came in at `f0845a92`.
- `cw-2026-08-22-032113` has a row in the captures type with 47 named at or above the bar and
  102 elements, and it has a keyed floor of 47.
- The numbers at HEAD are:
  - all keyed: **165 edits over 565**, against inferred keys;
  - 17:37: 19 over 25;
  - 17 added letters, 8 of them single-element;
  - captures 51 of 51, adjudicated 13 of 13, keyed floors 13 of 13.
- The opening at HEAD, on the spliced stream from 30 to 46.2 s, reads
  `UIEH EE E E T I NIEEE E E ET N ■IK`, with 22 named. `003919` cold reads
  `EITEETNXNIK EANQNID EANQNIK`.
- `Directory.Build.props` is at 1.13.119.
- The launcher writes `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and
  `WORK_INSTRUCTIONS.md` at the root, and the reload shows them as modified. It also writes
  `.run-unit` files. Task 0 commits the root files with its record as they stand, and says so.

**Expected failures.** Report each of these and repair none of them:
- dispatcher-loop losses on the app carry-forward line, each re-run alone (P45);
- `AHeldPitchDoesNotOutliveItsEvidenceTests` at 1 of 4 (P40);
- `EveryElementCarriesItsOwnPitchTests` and `ThePeakFindsThePitchTheTrackerMissedTests` run 0
  tests, because they are `Compile Remove`d (P41).

## 6. Rulings in force - do not re-argue

`PHASE_PLAN.md` R59 to R74, with §3 and §6.

**3.2's four tests are the keep rule, as units 430 and 432 applied them to 7.4:**
1. The total edit count over all keyed recordings **does not rise** above 165 over 565, against
   inferred keys. No key scores the opening. This is unit 430's arbiter decision, and it
   stands.
2. No named floor from 2.2 is broken.
3. The three adjudicated readings are unchanged character for character, or changed to exactly
   their own adjudicated text (R66). Print any reading that moves, before and after.
4. No capture row's above-bar named count falls (R71, raw span 13.0), except as R73 allows. R73
   does not reach a fall outside a scored stretch, and it does not reach any fall on an
   unkeyed row.

**Other rulings:**
- **R68:** the acquisition failure belongs to step 7.
- **R73:** only a key-aligned added character inside a scored stretch may leave a floor.
- **R66, R71, R72:** no word, dictionary or callsign prior.
- **R61:** no key is invented, and no scored region is changed.
- **HM-DEC-095 and HM-DEC-127:** the tracker's choice of candidate belongs to the tracker. This
  unit changes only whether the mixdown follows it.
- **HM-DEC-091:** a change that reads one recording and costs another is not a fix.
- **§0.0:** no decode is presented as what was sent.
- **§0.2:** nothing that keys or transmits is touched.
- **§12.5:** no synthetic case is the sole evidence for keeping a change.
- Also in force: **HM-DEC-155**, **HM-DEC-165**, **FACT-004** and **FACT-006**.

**The author's decisions, overrulable, recorded as decisions and not as rulings:**

- **Routing to 7.4 over 3.6.** Section 4 gives the reason.
- **The rule, fixed before the replay and not changed after it:**
  - A move of up to 25 Hz from the current mix pitch is followed at once, as at entry. The
    25 Hz is unit 430's same-station distance, unchanged.
  - For a move of more than 25 Hz, take the audio of the last 3.0 s: the survey's own history
    length, so this adds no new constant.
  - Compute `CwProbabilisticDecoder.Envelope` at the target pitch and at the current mix pitch,
    at `IntegratorBandwidthHz`.
  - Each pitch's **keying contrast** is the 90th percentile of its per-hop magnitudes over the
    10th percentile, in dB.
  - Follow the move when the target's contrast is greater than the mix's. Otherwise hold the
    mix where it is. Re-compare on every survey read while the move is pending. Follow the
    moment the target wins, or when the tracker moves again, which starts a new comparison.
  - Plain greater-than, with no margin.
  - Where either figure cannot be computed because less than 3.0 s has been heard, follow as
    at entry.
- **The replay gate compares against the entry, not against `5b6b704c`.** The rule is built
  only if task 1 shows both of these:
  - it follows `032113`'s 650 Hz move within one survey read of the entry's 26.54 s, so by
    27.04 s;
  - it holds the opening's 600 to 525 Hz move until the tracker's next move, at 36.04 s.

  `031905` is printed and not gated: the four tests decide it. `032113`'s 500 Hz move is not on
  the entry record, so it is not gated either. If either gate answer is no, build nothing and
  report which one failed.
- **One attempt, and no variant.** No other percentiles, windows or margins.
- **7.4 ticks only on a kept change.** If nothing is kept, this is 7.4's third unit in a row
  with no kept change. Task 3 then writes the trace to `PARKED.md` as P48, and step 7's line in
  `PHASE_STATUS.md` records 7.4 as closed partial. **7.4 is not ticked.**
- **P42's figure.** Report the opening as its text beside the cold group `EANQNID`, together
  with its named count.
- **No self-ruling authorizes work outside these tasks.**

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

- Add `## UNIT 433 - STEP 7` to `PHASE_OUTCOME.md`, from the decision block at the foot of this
  file.
- `PHASE_STATUS.md` names unit 433 and `CURRENT_STEP: 7`.
- Patch-bump `Directory.Build.props` from 1.13.119 to 1.13.120.
- Park unit 432's section 4 items 1 and 2 verbatim in `docs\phase-correctness\PARKED.md` as
  P46 and P47, each with the note from section 3.

**Entry round.** Run each of these, one type per invocation:
- both carry-forward lines;
- the three floor tests, with captures at 51 and the captures type's wall time;
- the keyed totals, per recording and all together;
- the added letters, single-element and not;
- `WhatTheOpeningHeardTests`, for the opening's text and named count, cold and on the spliced
  stream;
- unit 430's pitch types.

These are the numbers to hold.

**Drop candidate:** none.

### Task 1 - the replay (7.4)

Build nothing in the decoder in this task. Add one member to `WhatTheOpeningHeardTests` that
**asserts nothing and writes nothing**, and drive the decoder a hop at a time, as
`WhenEachRuleFollows` does. Run it on these stretches:
- the spliced stream from 28 to 38 s;
- `032113`, whole;
- `031905`, whole.

For every tracker move of more than 25 Hz from the mix, print:
- the time of the move, and its pitch from and to;
- at the move and at every survey read while it is pending: the time, the contrast at the
  target, the contrast at the mix, and which one is greater;
- the time at which the mix follows under the entry and under this rule.

Also print, once per stretch, the sender's pitch as `WhyTheMixMoved` reports it, so a reader
can see which pitch the envelope comparison favored.

**Give the gate's two answers** from section 6 as yes or no, each with the print line that
shows it. Say what the rule does on `031905`'s moves at 13.04 and 26.54 s. If either answer is
no, build nothing in task 2, and go to task 3.

**Drop candidate:** `031905`'s print. It is not dropped if task 2 is built.

### Task 2 - the rule, judged (7.4)

Build section 6's rule in `CwDecoder.Step` **in its own commit**. Do not touch any of these:
- the tracker and its survey scoring;
- the speed search;
- the gap estimator;
- the gates;
- `CharacterMargin`, `StrayElementSpan` or the span bar;
- `Envelope` itself.

Keep the 3.0 s of audio the rule needs in `CwDecoder`. It is used only while a move is pending.

Then run each of these, one type per invocation:
- the keyed totals, per recording, with 17:37's edits over its scored region;
- the added letters, single-element and not;
- the keyed floors;
- adjudicated;
- captures, all 51 rows, each with its old, above-bar and below-bar counts, its elements, and
  the type's wall time. **Read `032113` and `031905` first**;
- `WhatTheOpeningHeardTests`;
- unit 430's pitch types.

Judge the change under section 6's four tests, and print each test as a number. **For every
row whose above-bar count falls, list every character that left**, with its recording and its
time, and say whether it sits inside a scored stretch.

**If the change fails any test, take it back out in the next commit**, and say which test
failed.

**If it is kept:**
- report the named characters in the opening 60 seconds of `003901` and `003919`, before and
  after, cold per file and on the spliced stream, beside `EANQNID`;
- report the captures type's wall time before and after;
- tick 7.4 in `PHASE_PLAN.md`.

**Drop candidate:** the after-round of the pitch types. The captures, floors and adjudicated
tests are not dropped.

### Task 3 - the exit round

Run each of these:
- both carry-forward lines;
- the three floor tests, with captures at 51;
- the keyed totals and the added letters;
- `WhatTheOpeningHeardTests`.

Check two things against the tree:
- none of the eleven transmit files differs from `7e209cb4`;
- `data` is unchanged from entry.

If nothing was kept, `src` is also unchanged from entry.

**If nothing was kept**, write P48 in `PARKED.md`: 7.4 closes partial after three units with no
kept change (430, 432, 433). It gives:
- each unit's rule, and the test or gate answer that stopped it;
- the opening's text at entry beside `EANQNID`;
- `032113` at 47 above the bar;
- the numbers left standing.

Then record 7.4 as closed partial on step 7's line in `PHASE_STATUS.md`. Do not tick it.

**Drop candidate:** none.

---

## 9. Parked - do not touch, do not raise

- **3.6, G1 and P44.** P43 and P44 hold them. If the rule moves any single-element added
  letter, report it as a finding and do not claim it for 3.6.
- **P39's release rule.** It is not rebuilt, and P46 does not reopen it.
- **`AHeldPitchDoesNotOutliveItsEvidenceTests` and `Retuned()`** (P40). Report the count and
  nothing more.
- **The held gaps on `004535`** (P37), and **the sidecar's counters** (P38).
- **7.1, 7.2 and 7.8. Step 4. 6.5.**
- **The 2026-09-25 traffic net** (P35).
- **P27, P29 to P47.**
- **Any key, scored region, floor or span bar.** 7.4 moves no floor.

## 10. What not to do

- **Do not move the tracker's choice.** HM-DEC-095 governs it.
- **Do not rebuild `5b6b704c`, `ec76051e` or P39, and do not combine any of them with this
  rule.** All three are recorded as failing.
- **Do not change the percentiles, the window or the comparison after seeing the replay or the
  four tests.** No variant.
- **Do not excuse `032113`'s fall, or any fall outside a scored stretch.**
- **Do not keep a change on the opening's figures alone.** The four tests decide.
- **Do not add a word, dictionary or callsign prior** (R72).
- **Do not touch anything that keys or transmits.**
- **Do not halt for a question.** Park it.
- **Write `output.md` before the session ends.**
- **Never run an unfiltered `dotnet test`. Never background and poll. Never compose a
  timestamp.**
- **Report mismatches and repair nothing. Use American spelling and UTF-8. Use the four
  headings exactly.**

## 11. Committing and pushing

Commit after each task. The change and any revert each go in their own commit. Push at the end,
and say whether the push succeeded.

---

## 12. Reporting

Write `output.md` at the root, with the four headings exactly as section 1 gives them.

```
READ IN THIS ORDER.

A. Hamlet reads a CQ call correctly. Steps 0 to 2 done; 3 partial and
   closed under 3.4 with 3.6 open; 4 not started; 5 the owner's; 6 partial
   with 6.5 open; 7 partial with 7.1, 7.2, 7.4, 7.6 and 7.8 open.
B. Step 7, criterion 7.4: the mix follows a move of more than 25 Hz only
   when the decoder's envelope keys harder at the target than at the mix;
   the replay's two answers; built or not; kept or taken back out, and
   which test decided it; 032113 and 031905 before and after; the
   opening's text before and after beside EANQNID; 7.4 ticked, or closed
   partial with P48.
C. The rest, weighed against A and B. Section 4 raises <n> items; say
   whether any stands in the way of 7.4.
```

```
UNIT:       433 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     keyed 165 -> <n> over 565, inferred keys; 032113 above-bar 47 -> <n>; opening stream 30 to 46.2 s <n> -> <n> named, <text before> -> <text after>
DRIFT:      <0 if a criterion moved, else 1>
```

**Section 3 leads with the replay:** the two gate answers, each with its print line, and the
contrast at both pitches at the opening's 525 Hz move and at `032113`'s 650 Hz move. **Then the
four tests as a table**: each test, the number before, the number after, pass or fail, and
whether the change was kept. **Then the opening's text before and after, beside `EANQNID`.**
**Then `032113` and `031905`**: each one's above-bar count and elements, and the time the mix
followed each move.

---

```
ARBITER-DECISION
STEP: 7
APPROACH: hold a pending mixdown move unless the keying contrast measured at the new pitch over the decoder's last hops beats the contrast at the current mix pitch, head to head - Envelope at 45 Hz over the survey's 3 s, 90th over 10th percentile, replay-gated against the entry on the opening's 525 Hz move and 032113's 650 Hz move, then built once and judged under 3.2's four tests
MOVE: work around
WHY: At 7.4 all three rules tried (5b6b704c, ec76051e, P39) asked only whether the survey confirmed keying at the new pitch, and unit 432 measured that this cannot tell 031905's or 032113's right moves from the opening's wrong one; comparing the two pitches on the decoder's own envelope is the one question not yet asked. 3.6's four routes are all recorded as failing and its way back (P44) is the owner's, so 7.4 is the less exhausted of the two.
STATE: partial
DECIDED: author's, overrulable - routing to 7.4 over 3.6; the rule (25 Hz same-station distance kept, 3.0 s window from the survey's history, contrast as 90th over 10th percentile of Envelope at IntegratorBandwidthHz, plain greater-than, follow as at entry until 3.0 s is heard), fixed before the replay; the gate compares against the entry, not 5b6b704c, on the opening's 525 Hz hold and 032113's 650 Hz follow by 27.04 s, with 031905 left to the four tests (unit 432's item 1 parked as P46, not ruled); one attempt and no variant; test 1 reads does not rise as unit 430's arbiter decided; 7.4 ticks only on a kept change, and if nothing is kept this is its third unit, so P48 is written and 7.4 recorded closed partial, not ticked; unit 432's items parked as P46 and P47. No self-ruling authorizes work outside the tasks.
LICENCE: PHASE_PLAN.md R68, R64, R65, R66, R71, R72, R73, section 6 and criteria 7.3 and 7.4; PARKED.md P39, P42 and P43; unit 432's replay (f0845a92); unit 430's arbiter decision on test 1; HM-DEC-095; HM-DEC-127; HM-DEC-091; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: the first minute of a CW session reads the sender's letters instead of a run of E and T, because the decoder follows its pitch tracker only to a pitch where the sender actually keys harder, and no recording loses a letter it read before - or 7.4 closes partial with three rules measured and the trace on record
ADVANCES: step 7 criterion 4
END-ARBITER-DECISION
```
