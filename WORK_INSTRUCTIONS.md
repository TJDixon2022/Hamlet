# Work instruction 465 - one transcript from two readers: both decoders read the same samples live, and an arbiter emits each character by agreement, calibrated confidence and tie, with both readings on the sheet

**Loop unit.** Step 9, criterion 9.6 (`CW_REQUIREMENTS.md` section M: HM-REQ-120, 121, 125, 126,
127, with HM-REQ-124's vote rule, which is already measured, beside them).

**Why step 9, and why 9.6, and not the launcher's step 2.**
- **R86, `PHASE_PLAN.md` section 6:** *"Steps 2 to 8 are not authorable until 9.4 has a kept
  change. Every unit is step 9's until a technique from the second decoder is kept in ours."* No
  later ruling lifts it. Step 2's only open line, 2.4, counts step-2 units, and R86 bars step-2
  units. This is not a step-2 unit, and 2.4's count stays at 1 of 3.
- **9.4 has no route the plan's words allow.** All five mechanisms 9.3 named are recorded as
  refused: (D) by unit 459; (B), (C) and (W) by unit 462; (A) in both forms by unit 463. A sixth
  attempt would restate one of them.
- **9.5 is ticked** (unit 464, `b30bef48`). Each decoder now carries a p, and
  `docs/phase-requirements/calibration.md` names where each is calibrated. **9.6 is the next line
  section M orders**, and nothing it needs is missing. No unit has attempted it. The loop test
  finds no entry for this approach.
- **What 9.6 will do to the text today.** Unit 464 measured the port calibrated on no condition.
  Under HM-REQ-124 it therefore votes nowhere, and on today's table the arbitrated transcript
  should equal ours, character for character. That is expected and is the check in task 2. This
  unit builds the mechanism the requirements order, proves each rule on injected cases where both
  decoders vote, and puts both readings on the sheet. **9.7 measures whether it earns its place.**

Four working tasks, plus the exit round. Drop from the back.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST EXIST:      CW_REQUIREMENTS.md
  MUST EXIST:      CW_SPEC.md
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all six are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all six hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules, short

**HM-DEC-155. No suite:**
- Run named types only, one per invocation, each with its own `timeout`.
- Captures get 600 s.
- Never run in the background and poll.
- If one type needs more than 600 s, split it by recording or by case into several named types or
  filters. Never raise the timeout past 600 s.

**Shell limits:**
- Apostrophes in quoted heredocs break, and doubled backslashes collapse.
- `;` is refused, and so is `rm`.
- Python cannot run here. Everything is C#.
- A multi-line commit uses `-m` more than once.
- Scripts go in `.run-unit\unit465-<name>.sh` and are run with `sh`.

**The report:**
- Use the four headings exactly: `## 1. What Claude did`, `## 2. What the owner should expect`,
  `## 3. What you should see`, `## 4. What's blocking us`.
- Write the `UNIT:` line without brackets.
- Write `ADVANCES: step 9 criterion 6`. The launcher reads the digits after "criterion", so this
  means plan line 9.6.
- WHY cites the plan.

**R85. A CW question is answered from the documents and fldigi's source, never raised to the
owner.** That covers:
- what "the same span" means;
- what a decoder that does not vote contributes;
- what is emitted where neither decoder votes;
- how the live condition is chosen.

Section 6 below answers each of these. Where the tree forces a reading this instruction did not
foresee, record it in one line of section 4 and carry on.

---

## 2. Why this unit exists

**The count today, by the plan's checkboxes:**

| step | met |
|---|---|
| 0 | done |
| 1 | done |
| 2 | 4 of 5 (2.4's count 1 of 3; barred by R86) |
| 3 | 3 of 6 (barred by R86) |
| 4 | 5 of 7 (barred by R86) |
| 5 | 1 of 6 (barred by R86) |
| 6 | 3 of 6 (barred by R86) |
| 7 | 2 of 5 (barred by R86) |
| 8 | 0 of 6 |
| 9 | 4 of 8 (9.4 open with every mechanism 9.3 named refused; 9.6, 9.7, 9.8 open) |

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  both decoders read the same samples at the same time, live;
            one arbiter turns their two readings into the one transcript
            the CW tab shows - agreement with the more confident class,
            disagreement to the higher calibrated p, a tie within the
            margin dim and never sure, a decoder that is not calibrated
            on the condition advisory only - and the sheet records both
            readings per character; each rule watched failing first on
            an injected synthetic case.
ADVANCES:   step 9 criterion 6
```

**Read these first. They win over this instruction:**
- `CW_REQUIREMENTS.md` section M, HM-REQ-120 to 128, all of it, and the verification table's rows
  for 120, 121 and 125 to 127;
- section A, HM-REQ-001, for what dim is for;
- `docs/phase-requirements/calibration.md`: the constants, and the per-condition verdicts,
  held-out;
- `docs/phase-requirements/parity.md` sections 1 and 3, for how the port's output maps to classes
  and for the condition rows;
- `CwCharacter.Probability` and `FldigiConfidence`, which unit 464 built;
- `CwDecoder`'s events (`LeadingEdge`, `CharacterDecoded`, `CharacterSettled`), and where
  `MainWindowViewModel` subscribes to them (around line 11244);
- `FldigiCwDecoder`'s constructor, `Frequency`, `rx_process`, `Emissions`, `KeyEvents` and
  `CW_SAMPLERATE` (8000), without changing any of them.

**The requirements this unit serves, verbatim:**

> **HM-REQ-120:** "Two decoders shall read the same audio: every character the operator sees has
> been read by both decoders from the same samples at the same time."
>
> **HM-REQ-121:** "The operator shall see one transcript. Which decoder produced a character is
> never shown on the CW tab; the capture sheet records it per character."
>
> **HM-REQ-125:** "Where both decoders emit the same character for the same span, it is emitted
> with the class of the more confident decoder."
>
> **HM-REQ-126:** "Where the decoders disagree on a span, the character of the decoder with the
> higher calibrated confidence is emitted, and the disagreement is recorded on the sheet with both
> characters and both confidences."
>
> **HM-REQ-127:** "Where the two confidences are within a margin of each other, the winning
> character is emitted in the dim class and never sure, and the sheet records the tie. The margin
> is TBD, needs ruling (recommended 0.05)."

**9.6 is met when all of these are true:**
1. Both decoders read the same samples at the same time, in the live pipeline and in the harness.
2. Agreement emits with the more confident decoder's class.
3. Disagreement emits the character of the higher calibrated confidence, with both characters and
   both p's recorded on the sheet.
4. A tie within the margin emits dim, never sure, and the sheet records the tie.
5. The CW tab shows one transcript and no decoder name.
6. Each of 1 to 5 has a test naming its requirement, watched failing first on an injected
   synthetic case.

**Not 9.6's, and not this unit's:** measuring the arbitrated output against each decoder alone on
every metric, and switching arbitration off per condition. That is 9.7 (HM-REQ-128).

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1, and carry on as far as the tree allows.

**Check each of these:**
- **HEAD** is `71ac86be` or a runner commit on top of it.
- **`PHASE_PLAN.md`**, in both copies:
  - shows 9.1, 9.2, 9.3 and 9.5 ticked, and 9.4, 9.6, 9.7 and 9.8 open;
  - carries R86's line in section 6.

  If `docs/phase-requirements/PHASE_PLAN.md` differs from the root copy, report it.
- **The port** is at `src\Hamlet.RadioEngine\Cw\Second\`, and `git diff 19109b51 --
  src/Hamlet.RadioEngine/Cw/Second/` prints nothing.
- **Unit 464's work is present:**
  - `CwCharacter.Probability`;
  - `src\Hamlet.RadioEngine\Cw\FldigiConfidence.cs`;
  - `CwCalibration` in the test project;
  - `docs/phase-requirements/calibration.md`, whose held-out verdicts are:
    - ours calibrated on real HF all, real sender not stated, synthetic all, and synthetic TX-ITU
      15 dB;
    - the port calibrated on none.
- **The runner's uncommitted writes:**
  - the modified `.run-unit` state files, `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and
    `RUN_LEDGER.md`;
  - the untracked `.run-unit\reports\unit-3-output-11.md` and `.run-unit\watched.rc`.

  Commit them as they are. **`.run-unit\fldigi\` is untracked. Do not stage it, and do not fetch.**
- **Logged and not this unit's:**
  - the reload's `RULES_AT` disagreement (HM-DEC-165 against CPS-DEC-0183);
  - `outcome-read`'s step titles for steps 2, 3 and 8, which differ from `PHASE_PLAN.md`'s.

**Entry figures, 464's exit, to compare against:**
- **Build:** 0 errors.
- **Engine line:** 178 of 178.
- **App line:** 278 of 278.
- **Floor tests:** named 13 of 13, captures 51 of 51, adjudicated 13 of 13.
- **Real set, 23 keyed recordings, inferred key:** MET-CER-SURE 33 of 436, MET-INVENTED 33 over
  473, coverage 403 over 473, MET-WBE 37 over 113.
- **Synthetic set, 12 cases, exact key:** MET-CER-SURE 14 of 173, MET-INVENTED 14 over 252,
  coverage 159 over 252, MET-WBE 44 over 84.
- **The port, per parity.md:** real 62 wrong of 239 sure, coverage 177; synthetic 34 of 168,
  coverage 134.
- **Decode time**, ours over 690 s of real audio: about 52 s.

If any entry figure differs, report it and use the measured one.

**Expected failures, not regressions:**
- `TheSpeedFollowsTheSendersMarkPairsTests`, red at 28 of 31. It is on neither carry-forward line.
- The app line losing a test to Avalonia's headless "dispatcher loop". Handle it under DECIDED (9).
- The parity run rewriting the decode-time rows of `parity.md`. Keep a copy under `.run-unit\` and
  restore the committed file, as units 462 to 464 did.

---

## 4. Rulings in force - transcribed, do not re-argue

**The ordering:**
- **R86** (`PHASE_PLAN.md` section 6): *"Steps 2 to 8 are not authorable until 9.4 has a kept
  change. Every unit is step 9's until a technique from the second decoder is kept in ours."*
- **R84** (`PHASE_PLAN.md` section R): *"Every character the operator sees has been read by both
  (120); the operator sees one transcript (121). The order is fixed by the requirements: ... it
  votes only where its confidence is calibrated against the keys (124); agreement emits with the
  more confident's class (125); disagreement goes to the higher calibrated confidence (126); a tie
  is dim, never sure (127); and the arbitration is switched off on any condition where it loses to
  the better decoder alone (128). Rejected: the port as a bench instrument only; replacing ours
  with the port."*
- **HM-REQ-122 and 129, and section 6:** *"The second decoder is faithful, not improved."*
  - Not one line under `Cw\Second\` changes, not even to expose a field or add a setter.
  - The arbiter, the resampling that feeds the port, and the vote table all live outside
    `Second\`.
- **Section 6:** *"A requirement with a TBD threshold (HM-REQ-032, 065, 092) is measured and the
  measurement reported; the threshold is the owner's and its absence never halts a unit."*
  HM-REQ-127's margin is TBD in the same way. DECIDED (5) sets how it is held.

**R78, the keep rule, applies to any change that moves a letter or a class.** Under today's vote
table, the port votes nowhere, so the arbitrated transcript must equal ours character for
character and class for class on all 35 recordings. **R78 is engaged only if it does not.** A
difference under today's table is a defect in the arbiter, not a result. Find it before
committing.

**Standing rulings this unit leans on:**
- **R77.** A new CW test names the requirement it serves.
- **R80.** No traceability, test inventory or decision-log work.
- **R72.** No word, dictionary or callsign prior (HM-REQ-004, HM-DEC-175). The arbiter never
  prefers a character because it makes a word.
- **R61.** A key is inferred unless it was transcribed.
- **V-11 and V-13.** One scorer, one set of keys.
- **V-14.** The span rule, the vote table and the margin are fixed at task 1, before any
  arbitrated text is seen, and never changed after.
- **R85.** As section 1.

**Standing rules:**
- `CLAUDE.md` §0.0: never present a guess as a decode. A tie is dim for exactly this reason.
- `CLAUDE.md` §0.2: nothing that keys or transmits. **`KeyerCwSender.cs` and the eleven transmit
  files `PARKED.md` names are not touched, not called and not reused.** The arbiter is receive
  only.
- `CLAUDE.md` §12.5: a fixture built from the same misunderstanding as the code proves nothing.
  The injected cases are built by hand from the requirement's words, not from the arbiter's
  output.
- HM-DEC-155, HM-DEC-165 and FACT-004.

---

## 5. Status cadence

Post one line:
- at the start of each task, naming what it will measure or build;
- when task 1 fixes the span rule, the vote table and the margin;
- when the corpus-through-the-arbiter check comes back identical or not;
- at each commit, with its hash;
- if a type runs past its timeout.

Post nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 465 - STEP 9` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 465 with `CURRENT_STEP: 9`, in both copies.
3. Bump the patch from 1.13.151 to 1.13.152.
4. Commit the runner's writes as they are. Check `git status` before each commit.
5. Run the entry round:
   - build;
   - both carry-forward lines;
   - the three floor tests;
   - the adjudicated readings;
   - `TheRequirementsAreMeasuredTests`, real and synthetic;
   - `BothDecodersAreScoredAlikeTests`;
   - `EveryCharacterCarriesAConfidenceTests`;
   - the port's own tests.
6. Save both decoders' texts, each character with its class and p:
   - ours to `.run-unit\unit465-text-before.txt`;
   - the port's to `.run-unit\unit465-port-before.txt`.

   Every byte-identical check in this unit is made against these two.
7. Time ours over the real set, as the entry decode time.

### Task 1 - the trace: where the two readings meet, and the rules fixed

This task reads and prints. **Nothing under `src` changes.**

Write a fact that asserts nothing: `WhereTheTwoReadingsMeetFact`, serving HM-REQ-120 and 125 to 127.
It prints to `.run-unit\unit465-meet.txt`.

1. **The common clock.** For each of the 35 recordings and cases, put both decoders' characters on
   one clock, in seconds from the recording's first sample:
   - ours from `CwCharacter.At`, and its span from what the character carries (name the members);
   - the port's from `FldigiCwEmission.InputSample` at 8000 Hz, and its span from its key events.

   State how the harness feeds the port today: the resampling, and the frequency it is
   constructed at. Name the file.
2. **The pairing, printed per recording** (drop candidate, see below). For every character of
   either decoder, print:
   - its span, text, class and p;
   - the other decoder's character on the same span, or `none`;
   - the case it falls in: agree, disagree, one-sided, or tie within 0.05.
3. **Counts per condition**, using `parity.md` section 3's rows: agree, disagree, one-sided ours,
   one-sided port, and tie within 0.05. Also give the port's p against ours' p on the disagreements.
4. **The seams the live product reads.** Name, by file and member:
   - every place the CW tab takes characters from `CwDecoder` (the three events, and anything
     else);
   - where the capture sheet for a CW read is written, and what it records per character today;
   - where a port could be fed the same chunks ours is fed, and at what sample rate those chunks
     arrive;
   - whether the port can follow a pitch change without a line under `Second\` changing. For
     example, is there an upstream-faithful frequency setter, or must it be re-constructed?
5. **Fix, once, before any arbitrated text is seen** (V-14). Write these into the fact's header and
   the commit message:
   - **the span rule** (DECIDED (2));
   - **the vote table** (DECIDED (3));
   - **the margin** (DECIDED (5)).

**Drop candidate for this task:** item 2's per-character print. Shed it to the per-condition counts
of item 3 if time runs short. Items 1, 3, 4 and 5 are never shed.

**Commit the fact and its printout.**

### Task 2 - the arbiter, and the three rules watched failing first (HM-REQ-124 to 127)

1. **`CwArbiter` under `src\Hamlet.RadioEngine\Cw\`** (never under `Second\`). It is a pure type:
   - it takes ours' characters and the port's characters with their p's, plus the vote table's
     answer for the condition in force;
   - it returns one stream of `CwCharacter`, each carrying an arbitration record: which decoder's
     character was emitted, the other decoder's character and p or `none`, and whether the span
     was an agreement, a disagreement, one-sided or a tie.

   Its remarks name HM-REQ-124 to 127 and the rules in DECIDED (2) to (6).
2. **`CwVoteTable`**, beside it. This holds the held-out verdicts of `calibration.md`, per decoder
   and per condition, transcribed with the file and commit named, and the condition the live
   product runs under (DECIDED (3)).
3. **The margin**, a named constant, `0.05`. Its remarks say it is HM-REQ-127's recommended value,
   that the threshold is the owner's, and that it is held until he rules (DECIDED (5)).
4. **A test type naming HM-REQ-125, 126 and 127, `TheHigherCalibratedReadingWinsTests`,** each
   case watched failing first against a stub arbiter that emits ours unchanged. Say how each was
   watched. The cases are hand-built readings on one span, with p's and a vote table injected so
   both decoders vote:
   - **125:** both read `K`, ours dim at 0.62, the port at 0.91. `K` is emitted sure. Also the
     mirror case: `K` is emitted with ours' class when ours is the more confident.
   - **126:** ours reads `R` at 0.70 and the port reads `K` at 0.90. `K` is emitted, and the
     record carries both characters and both p's. Also the mirror case.
   - **127:** ours reads `R` at 0.88 and the port reads `K` at 0.90. The winner, `K`, is emitted
     dim, never sure, and the record says tie.
   - **124 advisory:** the same disagreement with the port's vote withdrawn. Ours' `R` is emitted
     at ours' class, and the port's `K` is on the record.
   - **Neither votes:** ours is emitted as it prints today (DECIDED (4)).
5. **The corpus through the arbiter, under today's vote table.** On all 35 recordings, the
   arbitrated transcript is compared with task 0's save of ours, text and class. It must be
   identical; see section 4 on R78. Print the count of spans per case per condition from the
   arbiter's own records, and check that they match task 1's counts.
6. **Byte-identical, before committing:**
   - ours alone and the port alone against task 0's saves;
   - the four real and four synthetic metrics at their entry figures;
   - `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` printing nothing.
7. Run the three floor tests and both carry-forward lines. Commit the arbiter, the vote table, the
   margin and the test in one commit.

### Task 3 - live: the same samples at the same time, one transcript, both readings on the sheet (HM-REQ-120, 121)

1. **The port reads live.** In the CW receive pipeline, outside `Second\`, the port is fed the same
   chunks ours is fed, resampled to 8000 Hz, at the pitch our tracker has in force. The pitch
   rule is task 1's item 4 finding, applied in a way that changes no line under `Second\`. Report
   how often the port is re-constructed or retuned on the real set.
2. **The CW tab reads the arbiter only.** Every seam task 1 named is routed through `CwArbiter`,
   so no character reaches the tab until both decoders have read its span (DECIDED (6)).
   - Nothing on the tab names a decoder.
   - The transcript is one transcript.
   - Where a live rendering (the leading edge, say) cannot wait for the port without breaking
     HM-REQ-083's live-to-settled word boundaries, report it in section 4 with the seam named, and
     route what can be routed.
3. **The sheet records both readings.** For every CW character, it records the decoder emitted, the
   other decoder's character and p or `none`, and the case: agreement, disagreement, one-sided or
   tie. Every disagreement carries both characters and both p's (126). Every tie is marked (127).
4. **Tests naming HM-REQ-120 and 121**, each watched failing first at the parent commit. Say how
   each was watched:
   - `BothDecodersReadTheSameSamplesTests` (120): a synthetic keyed send of exact key is played
     through the live `CwDecoder` path. Every character that reaches the transcript carries an
     arbitration record showing a port reading on its span, from the same samples. A character
     only ours read is marked one-sided, never silently passed.
   - `TheOperatorSeesOneTranscriptTests` (121), in the app test project: with an injected
     disagreement, the CW tab's transcript shows one character per span and no decoder name
     anywhere on the tab. The sheet written for the same read carries both.
5. **Decode time**, before and after: ours alone against ours plus the port plus the arbiter, over
   the real set. Report it. It is not a keep rule, but a doubling is a finding for section 4.
6. **Byte-identical again:**
   - the corpus through the live path equals task 0's save of ours, text and class;
   - the port is untouched.
7. Run the three floor tests and both carry-forward lines. Commit.
8. **Tick 9.6** in both copies of `PHASE_PLAN.md`, in a follow-on commit, only when all six of
   section 2's conditions hold, each with its test green and watched failing first. If any part is
   not met, leave 9.6 open and say which part is missing. Do not tick anything else.

### Task 4 - the exit round

**Run the following, and report every figure beside its entry figure:**
- build;
- both carry-forward lines;
- the three floor tests;
- the adjudicated readings;
- `TheRequirementsAreMeasuredTests`, real and synthetic;
- `BothDecodersAreScoredAlikeTests`;
- `EveryCharacterCarriesAConfidenceTests`;
- the port's own tests;
- `TheHigherCalibratedReadingWinsTests`, `BothDecodersReadTheSameSamplesTests` and
  `TheOperatorSeesOneTranscriptTests`;
- `TheSpeedFollowsTheSendersMarkPairsTests`. Report its figure. It is not required green.

**Also print:**
- `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/`, which prints nothing;
- the arbitrated transcript and both decoders alone against task 0's saves: identical, or each
  difference named;
- `git diff 7e209cb4` over the eleven transmit files `PARKED.md` names, which prints nothing;
- `git status`, showing `.run-unit\fldigi\` still untracked;
- a table of every commit this unit made, with its results on the five at its exit: build, both
  carry-forward lines, and the three floor tests.

**Ticks:** 9.6 only, as task 3 says. Do not tick 9.4, 9.7 or 9.8, or any line of steps 2 to 8.

---

## 7. Parked - do not touch, do not raise

- **9.4 and every mechanism 9.3 named, (A) to (E) and (W), in any form.** Each is refused.
- **9.7: measuring the arbitrated output against each decoder alone on every metric, and the
  per-condition switch.** This unit's corpus check is only the identity check of task 2, item 5.
- **Refitting either confidence, a new feature for the port, or re-measuring calibration.**
  Unit 464's section 4 item 3 found the port's timing margin non-monotonic. That is a later
  unit's choice, and the vote table uses 464's verdicts as they stand.
- **Unit 464's section 4 item 5:** port characters outside the scored stretches.
- **Unit 463's section 4 items 3 and 4.**
- **The four questions `PARKED.md` records, and their `RESOLVED:` answers.**
- **Step 2's decoder work, and 2.4's count.** The count stays 1 of 3.
- **Steps 3 to 8,** under R86.
- **461's section 4, 459's section 4 items 3 to 5, 458's items 2 to 5, 6.5's tick, HM-REQ-084's
  `ABOVE`, and unit 455's two 6.1 findings.**
- **The two `PHASE_STATUS.md` copies differing beyond the unit's fields.**

## 8. Do not

- **Do not change any line under `src\Hamlet.RadioEngine\Cw\Second\`** (HM-REQ-122, 129). This
  includes making a field public or adding a frequency setter.
- **Do not change a letter, a class, a threshold, the lattice or the confidence map** of our
  decoder. The arbiter sits after both decoders.
- **Do not show a decoder's name, or which decoder won, anywhere on the CW tab** (HM-REQ-121).
- **Do not let a decoder that is not calibrated on the condition displace a character or raise a
  class** (HM-REQ-124).
- **Do not emit a tie sure** (HM-REQ-127).
- **Do not change the span rule, the vote table or the margin after seeing an arbitrated text**
  (V-14).
- **Do not change `CwMetrics`, the scorer, `MorseAlphabet`, `CwCalibration` or any test's key.**
- Do not use any rule that knows words, callsigns or letter frequencies (R72).
- **Do not touch, call or reuse `KeyerCwSender.cs`** or any transmit file (§0.2).
- Do not add a new test to either carry-forward line.
- Do not re-point or retire an existing test (R80).
- Do not edit `PARKED.md`, `CW_SPEC.md`, `CW_REQUIREMENTS.md`, or any ruling in `PHASE_PLAN.md` or
  `CLAUDE.md`.
- **Do not install any package. That is `MOVE: stop`.** The resampler is written in C#, or reuses
  one already in the tree; name which.
- Do not stage, commit or modify `.run-unit\fldigi\`, and do not fetch.
- **Do not run an unfiltered `dotnet test`. Never run in the background and poll. Never compose a
  timestamp.**

## 9. Committing and pushing

**One commit per task:**
- task 0;
- task 1's fact and printout;
- task 2's arbiter, vote table, margin and test;
- task 3's live wiring, sheet record and the two tests;
- the 9.6 tick, if earned;
- task 4.

**Messages** take the form `unit465 task N: <what> (9.6)`. Task 2's message states whether the
corpus through the arbiter was identical to ours.

**Exit state:** every commit exits with these green:
- the build;
- both carry-forward lines;
- the three floor tests.

The one exception is the dispatcher loss under DECIDED (9).

**Push each commit** to `origin/main`.

## 10. Report

Write `output.md` at the root with the four headings exactly. **The ordering block comes first.**
`validate-output.bat` refuses a report without it.

```
READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 4 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 2 of 5, 8 0 of 6, 9 <n> of 8; steps 2 to 8 still barred by R86,
   and 9.4 open with every mechanism 9.3 named refused.
B. Step 9, criterion 9.6: HM-REQ-120 <met|not> - the port reads the
   same chunks live at <rate>, pitch followed by <rule>; 121 <met|not>
   - one transcript, no decoder name, the sheet records <fields>;
   125 / 126 / 127 <met|not> each, watched failing first <how>;
   span rule <rule>; margin 0.05, provisional; live condition <row>,
   so ours votes <yes|no> and the port votes <yes|no>; corpus through
   the arbiter identical to ours <yes|no>; decode time <before> to
   <after>; the port byte-identical <yes|no>; 9.6 <ticked|open>.
C. The findings weighed against A and B: how many items section 4
   raises, whether any is in the way of 9.7 - the arbitrated output
   measured against each decoder alone - and whether R86 with 9.4's
   refusals is in the way of steps 2 to 8.
```

```
UNIT:       465 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <the criterion and why>
NUMBER:     HM-REQ-120 121 125 126 127 <met count> of 5; spans per condition agree <n> disagree <n> one-sided <n> tie <n>; arbitrated text identical to ours on <k> of 35; decode time <s> to <s> over 690 s
DRIFT:      step 2 <n>; step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 7 <n>; step 9 <n>
```

**Section 3 leads with the meeting table.** One row per condition, with the key's kind, giving:
- the spans the two decoders agreed on, disagreed on, and read one-sided, and the ties within 0.05;
- who votes on that condition under the vote table;
- whether the arbitrated text equals ours there.

Then give:
1. the span rule, the vote table and the margin, as task 1 fixed them;
2. the seams task 1 named, and what each now reads;
3. how each of the five tests was watched failing first;
4. three disagreements from the real set, with both characters, both p's and the key, as the
   sheet records them;
5. decode time before and after;
6. the commit table, with the five results at each commit.

**Section 2, one paragraph, in the owner's words.** Say what changes on the screen (nothing, if
the check holds) and what changes on the sheet. Say plainly that the second reader is listening
live now, but that under today's calibration it is only advising. It gets a vote on a condition
only once its confidence proves out there, and 9.7 decides whether the vote helps.

**Section 4 must say, as a plain reading and not a ruling request:**
- **HM-REQ-127's margin is 0.05, the requirement's recommended value, held provisionally.** The
  threshold is the owner's under section 6 and never halts a unit.
- **9.4's words and 9.3's five refused mechanisms leave 9.4 no authorable route, and R86 holds
  steps 2 to 8 behind 9.4.** This is logged for him, not a stop: it touches neither transmit nor
  what the product promises the operator.

---

```
ARBITER-DECISION
STEP: 9
APPROACH: build the arbiter that runs both decoders on the same samples, aligns their characters by span, emits agreement with the more confident class, disagreement by the higher calibrated p with the uncalibrated decoder advisory, a tie within 0.05 dim, one transcript on the CW tab, both recorded on the sheet, each watched failing first on an injected synthetic case
MOVE: work around
WHY: PHASE_PLAN.md section 6's R86 bars steps 2 to 8 until 9.4 has a kept change, so the launcher's step 2 line 2.4 cannot move, and 9.4's "one technique 9.3 named" has no route now that all five mechanisms 9.3 named are recorded refused; 9.5 is ticked, so step 9's next line in section M's fixed order is 9.6 (HM-REQ-120, 121, 125 to 127), which R86 permits, no unit has attempted, and the loop test does not find.
STATE: partial
DECIDED: author's, overrulable - (1) step 9 criterion 9.6 is worked instead of the launcher's step 2 on R86 (PHASE_PLAN.md section 6, the owner's commit 6de324b1), as units 462 to 464's arbiters did; 9.4 is not re-attempted; this is not a step-2 unit and 2.4's count stays at 1 of 3; (2) the same span, under R85: two characters on the common sample clock whose spans overlap by at least half the shorter span, fixed at task 1 before any arbitrated text; (3) the vote table is calibration.md's held-out verdicts as unit 464 measured them, a decoder voting only on a condition where it is calibrated (HM-REQ-124), and the live product runs under the real HF, all row because no sender or channel profile is known live; so today ours votes live and the port is advisory everywhere; (4) an advisory decoder's character and p go on the sheet and never displace a character or raise a class, and where neither decoder votes ours is emitted as it prints today, because HM-REQ-124 withdraws the vote and not the reading, and per-condition switching is 9.7's (HM-REQ-128); a span only one decoder read is emitted from it at its own class only if it votes, else from ours as today, and is marked one-sided on the sheet; (5) HM-REQ-127's margin is its recommended 0.05 on the absolute difference of the two p's, a named constant held until the owner rules, since section 6 says a TBD threshold is the owner's and never halts a unit; (6) no character reaches the CW tab until both decoders have read its span, and a live rendering that cannot wait without breaking HM-REQ-083 is reported with its seam and not forced; (7) under today's table the arbitrated transcript must equal ours character and class on all 35 recordings, a difference being a defect and not a result, so R78 is not engaged; (8) the port runs outside Second at 8000 Hz on the same chunks at our tracked pitch, following a pitch change only by means that change no line under Second; (9) the app line's headless dispatcher-loop loss: one rerun, and any type lost again is run alone and must pass, named in the report.
LICENCE: PHASE_PLAN.md step 9 lines 9.4, 9.5 and 9.6, section R (R84, R85), section 6 (R86, the second decoder is faithful, the TBD-threshold rule, V-14, R78), R61, R72, R77, R80; CW_REQUIREMENTS.md HM-REQ-001, 083, 120, 121, 122, 124, 125, 126, 127, 128, 129, V-11, V-13, V-14; docs/phase-requirements/calibration.md and parity.md sections 1 and 3; unit 464's report sections 3 and 4; HM-DEC-155; HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: fldigi's decoder now listens to the same audio as Hamlet's own, live, and every letter on the CW tab has been heard by both; where they agree the surer one sets how bright it prints, where they disagree the one whose confidence has proved honest wins, a near tie prints dim instead of sure, and the sheet keeps both readings, while the tab still shows one clean transcript with no decoder named
ADVANCES: step 9 criterion 6
END-ARBITER-DECISION
```
