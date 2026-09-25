READ IN THIS ORDER.

A. MET-CER-SURE, real keyed recordings with inferred keys, stays at 67 of 426 sure (0.1573).
   G1 took it to 56 of 419 (0.1337) and was not kept, because MET-COVERAGE as the spec writes
   it fell with it.
B. The 54, grouped:
   - split: a part of the sent letter, 27;
   - none of these, 9;
   - one element of the wrong kind, 8;
   - merged with part of a neighbor, 5;
   - several elements differ, 4;
   - one element lost, 1.
   46 of the 54 were read on textbook spacing.
C. The rest. Section 4 raises 3 items, none blocking.

```
UNIT:       440 - complete at task 4 of 4, task 3 dropped - 2026-09-25 19:05
PHASE GOAL: Hamlet meets the CW requirements, judged on the requirements' own metrics.
UNIT GOAL:  Find why the decoder prints the wrong letter as sure, and cut that number
            without anything else the operator reads getting worse.
NUMBER:     sure-but-wrong characters: 54 -> 54 at exit, 51 under G1 not kept;
            MET-CER-SURE 0.1573 -> 0.1573 at exit, 0.1337 under G1 not kept
```

## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt and `WORK_INSTRUCTIONS.md` both carry `PROJECT: Hamlet`. The four
`MUST EXIST` files are present, `CoreHMI.sln` and `MURC.sln` are absent, and `PROJECT_CARD.md`
says `PROJECT: Hamlet`. Hamlet confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop**, as unit 439 was:
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same way.
- Nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched.
- The only ticks are steps 0 and 1, which section 6 task 0 licenses.

**Section 4 checks, and what disagreed.**
- **The 54.** Unit 439's `output.md` and `docs/phase-requirements/metrics-baseline.md` give 54
  sure wrong. A fresh count at entry gives 54 too, so the figure is unit 439's.
- **MET-CER-SURE.** `CW_SPEC.md` 11 reads *"MET-CER over characters emitted as sure; dim and
  placeholders excluded, not counted wrong."* Unit 439 took the denominator as the sure
  characters emitted. That is unit 439's finding 4, and this unit used the same reading.
- **Who calls the four metrics.** `CwMetrics` holds them. `TheMetricsCountWhatAHandCountsTests`
  and `TheRequirementsAreMeasuredTests` call all four, and this unit's new trace calls
  `Invented` too.
- **The keyed recordings.** 23 real ones, all with inferred keys, and 12 synthetic ones with
  exact keys.
- **How a character becomes sure.** `CwProbabilisticDecoder.Judged` keeps a character whose
  per-hop span margin reaches `CharacterMargin` (1.0) and which is not a stray single element.
  `CwProbabilisticStream` then emits it as `High` whenever its pattern is in the alphabet, and
  as a placeholder only when it is not. **So any wrong pattern that happens to spell a letter
  prints as sure.** That is why the 54 exist at all.
- **Mismatch: R72 is cited as HM-DEC-175 again,** in the instruction and in the plan. It is
  item 2 of section 4.

**Task 0 (`ac95ae80`).**
- `PHASE_OUTCOME.md` gained `## UNIT 440 - STEP 2` in both copies.
- `PHASE_STATUS.md` names unit 440 and `CURRENT_STEP: 2`, with its step lines rewritten from
  the revised plan.
- The version went from 1.13.126 to 1.13.127.
- 0.1 to 0.4 and 1.1 to 1.5 are ticked from unit 439's report without re-measuring. The
  outcome entry names 439's figures beside them.
- The owner's revision of `docs/phase-requirements/PHASE_PLAN.md` (R80, R81, steps 2 to 8) was
  uncommitted in the tree. It was committed as it stood and copied to the root copy of the plan.

Entry round, the numbers to beat:
- build 0 errors;
- engine carry-forward 178 of 178;
- app 278 of 278;
- captures 51 of 51, adjudicated 13 of 13, named 13 of 13;
- real keyed recordings, inferred keys: MET-CER-SURE 67 of 426, MET-INVENTED 67 over 473,
  MET-COVERAGE 426 over 473 with 359 right.

**Task 1, the trace.** `WhereTheSureWrongLettersComeFromTests` is a printer. It asserts only
that it traced the metric's own 54. For each sure-wrong character it prints:
- the recording and the time;
- what the key says was sent and what the decoder emitted, with both patterns;
- its span and its neighbors' spans;
- the speed and the pitch in force;
- the envelope's marks and gaps in milliseconds and in units;
- the gap reading the stream held when the character settled, read by reflection.

The marks come from `CwUnitEstimator.Elements` over the character's span, one unit either
side. That is an independent segmentation of the envelope, not the path's own.

**The grouping is by how the emitted pattern relates to the sent one,** because the pattern is
what the decoder decided and the letter only follows from it. **No key was left out as
doubtful**: no scored stretch's key file calls its own reading doubtful.

This task's trace was committed together with task 2's change in `cce7985d`, not in a commit
of its own. That departs from commit-per-task.

**Task 2, G1 (`cce7985d` built, `5aa9c167` taken out).** The largest group is split, 27. Of
those, 20 were read on textbook spacing at a speed the envelope shows is wrong (see section 3),
and 7 under held gaps. Three of the seven were read under a held reading whose character gap
stands past its word gap: 17:37's 828 ms against 250.

**G1 refuses exactly that reading.** It is unit 431's `856d226e` re-applied unchanged, with no
threshold. It was taken out twice before on character counts, which R78 no longer judges by.

Judged under R78:
- MET-CER-SURE falls: real 67 of 426 to 56 of 419, synthetic 24 of 180 to 14 of 173.
- MET-INVENTED falls: 67 to 56.
- The adjudicated readings hold, 13 of 13. `032012` goes from `ARTICLESOR` to `ARTICLES OR`,
  its own adjudicated text.
- Captures go to 50 of 51: `004133` drops from 28 to 25 named, all outside its scored stretches.
- **V-11 holds.** No recording's sure-wrong-or-added count rises, and none loses a right
  character.
- **MET-COVERAGE falls as the spec writes it**: 426 to 419 over 473 on the real set, 0.7143 to
  0.6865 on the synthetic set. Sure-and-right rises on both, 359 to 363 and 156 to 159.

**So it is not kept.** `src` is identical to entry, and the figure is in
`docs/phase-requirements/metrics.md`.

**Task 3, dropped, with what was measured.** Every group other than split was read on textbook
spacing, 26 of their 27 characters. 11 of those 27 were mixed 25 Hz or more off the sender's
pitch. The trace names two causes for them: the speed the path is given (step 5) and the
tracker's pitch (step 4), and both are parked by section 7. A different group can only be
attacked there, and a narrowing of G1 is forbidden.

**Task 4, the exit round,** matched entry on every figure:
- build 0 errors;
- engine 178 of 178;
- app 274 of 278. The four failures are the dispatcher-loop loss, `ThePsk31OfferTests` and
  `TheChipSaysTheChosenModeTests`, and both types pass alone, 2 of 2 and 6 of 6.
- captures 51 of 51, adjudicated 13 of 13, named 13 of 13;
- metrics as at entry;
- the trace, 54 again.

`git diff` over `src` from entry prints nothing, and the transmit files print nothing against
`7e209cb4`.

**Decisions the session made for itself** (author's, overrulable):
1. The trace groups by pattern relation, and crosses each group with the gap reading, the
   inserted space and the pitch.
2. G1 was chosen as the change because it is the only rule on record the trace ties to a named
   reading. It was built without change.

No ruling was recorded.

## 2. What the owner should expect

Yes, the decoder still prints wrong letters with confidence. On the 23 keyed recordings, 54
characters it emits as sure are not what the inferred key says was sent, the same as this
morning, because the one change built was not kept.

The trace says why they exist: a wrong pattern that spells a letter is printed as sure, and
there is no other confidence. Half of them are a letter broken in two. The decoder reads a
character gap in the middle of an `L`, a `W` or an `A` and prints the first piece as an `E` or
a `T`. Mostly that happens because it is timing the sender at the wrong speed.

The one change that fixes part of it, refusing a gap reading where the character gap is longer
than the word gap, makes 17:37 read `CQ CQ CQ DEWB6 RE D W B` where it reads
`CQ CQ CQ DEWTEETEEERE D ETTTB` today. It was measured, and it is sitting one ruling away from
being kept.

What will look wrong but is not:
- Version 1.13.127 carries no code change.
- App carry-forward reads 274 of 278 on the dispatcher-loop loss.

**Push:** succeeded, `1fb0bad6..ad198291 main -> main`, and this line's own commit after it.

## 3. What you should see

**The 54 sure-but-wrong characters, grouped.** Real keyed recordings, 12 recordings, all keys
inferred, none left out as doubtful.

| group | count | read on textbook gaps | beside a space the decoder inserted | mixed 25 Hz or more off | most common |
|---|---|---|---|---|---|
| split: a part of the sent letter | 27 | 20 | 10 | 1 | L->E x3, W->T x2, R->E x2, A->E x2, N->T x2, 2->T x2, O->T x2 |
| none of these | 9 | 9 | 5 | 2 | 2->D, R->T, C->I, F->W |
| one element of the wrong kind | 8 | 7 | 3 | 4 | E->T x2, T->E x2 |
| merged: the sent letter and part of a neighbor | 5 | 5 | 0 | 3 | T->A x2 |
| same length, several elements differ | 4 | 4 | 2 | 1 | R->O, 1->5 |
| one element lost inside the letter | 1 | 1 | 0 | 1 | R->I |

**In the split group, the envelope measures the gap the decoder cut at as 0.7 to 1.1 units of
the read's own speed**: an element gap by the decoder's own clock. The textbook-read splits sit
at speeds the marks contradict:
- `003758` is read at 40 WPM, the search ceiling, with dits of 60 ms, 2 read units.
- `031838` has dahs of 4.7 to 6.2 read units.
- `032129` is read at 8 to 12 WPM with marks of 0.2 to 0.8 units.

**One line, before and after**, 17:37 against its inferred key `CQ CQ CQ DE WB6RED WB6RED`:

    today:     CQ CQ CQ DEWTEETEEERE D ETTTB 7E E I
    under G1:  CQ CQ CQ DEWB6 RE D W B 7E E I

**MET-CER-SURE per condition, under G1, not kept.** Real keyed recordings with inferred keys:
- sender not stated: 17 of its 20 recordings unchanged. 17:37 went from 14 sure-wrong-or-added
  to 3, and `032012` and `004133` changed in right letters and boundaries only;
- TX-FARNS, TX-ITU and TX-TIGHT: unchanged.

Synthetic CQ set with exact keys: `cq-12wpm-5db` went 3 to 0 and `cq-18wpm-15db` 7 to 0. The
full table is in `docs/phase-requirements/metrics.md`.

## 4. What's blocking us

Nothing blocks. Three items, the one holding the most work first.

**1. MET-COVERAGE counts wrong sure characters, and that alone kept G1 out.**
- **Proposed ruling:** MET-COVERAGE counts the sure characters that align to a sent character
  and are right, over characters sent, so it cannot rise by emitting wrong letters or fall by
  withholding them.
- **Reasoning:**
  - Under G1 every part of R78 held except coverage as written. It fell by exactly the seven
    sure characters the key calls wrong or added, while sure-and-right rose 359 to 363.
  - As written, the guard against dimming everything also punishes the fix step 2 exists for.
  - Under the proposed reading G1 is kept as it stands. MET-WBE rises by one under it, 57 to 58
    over 113 words, all on 17:37 (5 to 7). R78 does not list that metric for step 2, but the
    owner may want it counted.
- **Rejected:**
  - keeping G1 on this unit's own reading, which section 7 forbids;
  - leaving the metric as written, which makes step 2's keep rule contradict step 2's goal.

**2. R72 is cited as HM-DEC-175 in this instruction and in `PHASE_PLAN.md` §2.** HM-DEC-175 is
the CW-block receiver ruling. HM-DEC-183 already records the knowledge rule as R72, standing
per HM-DEC-181. This is a record fix for step 8 (R80), not this unit's to make.

**3. Step 2 is waiting on step 5.** 46 of the 54 were read on textbook spacing, and the split
group's textbook cases sit at speeds the envelope contradicts. Once G1 is settled, the largest
remaining cause of sure-wrong letters is the speed the path is given. The routing is the
arbiter's.

### Asks still outstanding

Carried from unit 439's report, 2026-09-25, not yet ruled:
- **Item 4, the two readings of `CW_SPEC.md` 11**, MET-CER-SURE's denominator and MET-COVERAGE
  above one. It is waiting on the owner. No change sits in the tree; coverage's half is item 1
  above.
- **Item 5, two green tests that contradict the spec.** `CwEmissionGateTests.TheBoundsAreTheRadiosOwn`
  pins 6 WPM against HM-REQ-030's 5. `CwSurveyThresholdPinTests.FindingTheToneIsNotClaimingSomebodyIsSending`
  asserts no keying on `134712` against HM-DEC-144. It is waiting on the owner. No change sits
  in the tree.

Unit 439's other four items were answered by the plan revision or scheduled by it:
- item 1 is answered in 7.4's text;
- items 2, 3 and 6 are scheduled as step 8's 8.1, 8.3 and 8.4 under R80.

They are dropped from the queue.
