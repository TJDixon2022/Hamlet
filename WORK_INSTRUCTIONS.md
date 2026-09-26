# Work instruction 453 - the words the owner named

**The arbiter wrote this against step 6's open criterion 6.5.** Steps 0 and 1 are done. By the
plan's checkboxes, step 2 is at 3 of 5, step 3 at 3 of 6, step 4 at 5 of 7, step 5 at 1 of 6 and
step 6 at 1 of 6. Steps 7 and 8 are at 0.

**Why not step 2, which the launcher named.** Neither of its two open lines can flip in one unit:
- **2.4** closes the step after three step-2 units in a row keep no change. Unit 449 kept a change,
  which set the count to **0 of 3**. No step-2 unit has run since. One unit takes the count to 1 at
  most, and the line flips only on the third.
- **2.5** needs all three named floors green. 443's DECIDED (3), as 448's DECIDED (6) read it, holds
  them red: no floor is re-banked. Only the owner can lift that ruling, and 444's question on 17:37
  is parked with him. Unit 444 measured that 17:37's letter spaces and word spaces overlap, so no
  duration rule gives its boundaries back, and that route is recorded no. Unit 452's boundary change
  was kept and left 17:37 at 7 wrong.

**Why 6.5.** HM-REQ-084 is the one requirement written as text the operator reads, on recordings the
tree holds. It is a must-tier requirement. No unit of this phase has measured it:

| recording, in the tree | span | what `DEV_ANALYSIS_2026-08-27.md` §4 recorded | at HEAD, from unit 452's exit text |
|---|---|---|---|
| `unadjudicated/cw-2026-08-25-013637` | `ABOVE` | `AB OVE` | `AB OVE` |
| same | `BREEZE` | `BREE Z E` | `BR EEZE` |
| `unadjudicated/cw-2026-08-25-021410` | `WEEKEND` | `ATEEKEND` | none of the three words is visible in `A T O MTTT YMTT O AOIHI DT RIGHR IS FLENT 66OAM` |
| same | `THINKING` | `TTHINKING` | same |
| same | `FLEX` | `FLENX` | same |
| `011447` / `011514` | `USED TO USE A FIRM` | `USEDTOUSEAFIRM` | **not in the tree, and never committed** (`git log --all` finds neither) |

- **021410 appears to read less today than it did on 2026-08-25.** Nobody has measured why.
- **This unit changes what the operator reads (R83)** if task 2's change is kept. Either way, the
  owner reads, as text, where the decoder stands on the words he named.

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

## 1. Why this unit exists

These are the counts at HEAD `0659ed8a`, from unit 452's exit. Its kept change moved spaces only;
the letters were identical:

| metric | condition, key | count | value |
|---|---|---|---|
| MET-WBE | real, inferred | 37 over 113 words | 0.3274 |
| MET-WBE | synthetic, exact | 44 over 84 words | 0.5238 |
| MET-WBE | 17:37 alone, inferred | 7 over 6 words | 1.1667 |
| MET-CER-SURE | real, inferred | 33 of 436 sure | 0.0757 |
| MET-CER-SURE | synthetic, exact | 14 of 173 | 0.0809 |
| MET-INVENTED | real, inferred | 33 over 473 | 0.0698 |
| MET-INVENTED | synthetic, exact | 14 over 252 | |
| sure-and-right coverage (R82) | real, inferred | 403 over 473 | 0.8520 |
| sure-and-right coverage (R82) | synthetic, exact | 159 over 252 | 0.6310 |
| MET-PITCH-ERR | files more than 25 Hz off | 9 of 69 | |

The floors at the same HEAD:
- captures 51 of 51;
- adjudicated 13 of 13;
- named 10 of 13. The three red rows are 17:37 (38 against 46), `032113` (43 against 45) and
  `032129` (42 against 64).

**Task 0 re-measures all of these, and where its numbers differ from this table, its numbers win.**

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  HM-REQ-084 is measured on every named span whose recording is in the tree, each
            printed as the operator reads it at HEAD with the marks and gaps under it, and the
            span whose recording is absent is named as not measurable here. Each misread is
            traced to its cause in the audio, and one change against the commonest movable
            cause is built and kept under R78 only if no recording gets worse.
ADVANCES:   step 6 criterion 5 - the plan's line 6.5
```

`step 6 criterion 5` is the launcher's form. It means the plan's line `- [ ] 6.5`.

**Read `CW_REQUIREMENTS.md` first, then `CW_SPEC.md`, then `DEV_ANALYSIS_2026-08-27.md` §4.** Read
these parts:
- in `CW_REQUIREMENTS.md`, section I and the verification table's row for 084, including the truth
  grade it states;
- in `CW_SPEC.md`, MET-WBE's definition and anything it says about acceptance spans;
- in `DEV_ANALYSIS_2026-08-27.md` §4, the four fixtures, the gap clusters it measured on `021410`
  and `013637`, and "Done means".

Where the documents differ from this instruction, the documents win. **In section 1 of your report,
quote each id below from the document, and report any difference as a mismatch.**

- **HM-REQ-084:** "On the acceptance spans named in DEV_ANALYSIS_2026-08-27 §4, the decoder shall
  emit `WEEKEND`, `THINKING`, `FLEX`, `ABOVE`, `BREEZE` and `USED TO USE A FIRM`." Its truth grade is
  **inferred**. **Measured.**
- **HM-REQ-080 and 081:** the boundary guards. They were measured by 6.3; this unit restates them
  only if the change moves them.
- **HM-REQ-004 (R72):** no word, dictionary or callsign prior. **The six words are a judging key,
  never an input to the decoder.**
- **HM-REQ-010 and HM-REQ-011:** guards on the change.

## 2. Verify this instruction against the tree

Check each of these. **Where one is wrong, report it as a mismatch in section 1 and carry on. Do not
repair it.**

- **HEAD and the plan.** HEAD is `0659ed8a`. In both copies of `PHASE_PLAN.md`:
  - 6.3 is ticked and 6.5 is not;
  - 2.1, 2.2 and 2.3 are ticked, and 2.4 and 2.5 are not.
- **The recordings.** `tests/fixtures/cw/captured/unadjudicated/cw-2026-08-25-013637.wav` and
  `-021410.wav` are in the tree. No recording named `011447` or `011514` is, and none ever was.
  - If you find one anywhere on this machine, **do not add it to the tree**. Report its path.
- **Where each span sits.** Find each span's time in its recording from the decoded text and the
  envelope, and state how you found it. `DEV_ANALYSIS` §4 names the words, not their times.
- **`021410`'s gap clusters.** `DEV_ANALYSIS` §4 measured 53, 221 and 913 ms at 18.2 WPM. Measure
  them again at HEAD, and report any difference.
- **`013637`'s gap clusters.** `DEV_ANALYSIS` §4 measured 24, 28 and 171 ms at 30.6 WPM: the element
  gaps and character gaps are 4 ms apart. Measure them again at HEAD, and report any difference.
- **Expected failures at entry. They are not yours to fix:**
  - the three named floors in §1;
  - `TheFiveToEightDecibelPlateauHolds`;
  - `AHeldPitchDoesNotOutliveItsEvidenceTests`, 3 of 4. It is in neither carry-forward line;
  - the app line losing up to 5 to the dispatcher loop, with each type green alone. **If the host
    hangs, rerun once and report it.**
- **Known, and not yours.** Report each once, and edit none of them:
  - `PHASE_OUTCOME.md`'s header still carries the old titles for steps 2, 3 and 8.
  - `CW_SPEC.md` §11 still defines MET-COVERAGE as sure over sent.
  - `PROJECT_STATUS.md` RULES_AT says HM-DEC-165, while `CLAUDE.md` §1 holds CPS-DEC-0183.
  - `PARKED.md`'s header says a session never writes it.
  - `TheQuietestBinNoLongerWinsTests` and `ThePitchControlsAreOffThePanelTests` are excluded by
    `Compile Remove`.

## 3. Rulings in force - do not re-argue

- **`PHASE_PLAN.md` R77 to R81 and §6, and R82 and R83** as work instruction 441 recorded them.
  **R83:** a unit of this phase changes what the operator reads, or it is not authored. Here, what
  may change is how the named words read. **R78 is the keep rule.** It judges on the requirements'
  metrics, never on character counts.
- **R72:** no word, dictionary or callsign prior, in any form. **No rule may place a letter, a cut
  or a boundary because the result spells one of the six words, or any word.** A cut or a boundary
  comes from the audio: mark and gap durations, the unit in force, and energy. Only the judging uses
  the words.
- **R75 and R76**, carried. The pitch instrument never enters `src`.
- **Arbiter rulings carried:**
  - **443's DECIDED (3), as 448's DECIDED (6) read it:** no floor is re-banked. So 6.6, like 2.5,
    3.6, 4.7 and 5.6, stays red.
  - **442's DECIDED (2):** a floors line is read as green at the exit of every commit from the
    unit's working task on.
  - **448's DECIDED (3):** a MET-PITCH-ERR move inside the 0.5 Hz bin is not a rise.
  - **452's §3 (d):** 444's half-unit dropout rule is not rebuilt as it stood, and a refused rule
    never gets a second value.
- **Parked with the owner, and not raised again here** (`PARKED.md`):
  - 444's question on 17:37: whether it is met at 5 or fewer boundaries wrong, or re-banked at 38;
  - 448's question on what "acquiring" means;
  - 450's questions on when a heard pitch may be called proved.
- **This unit's own readings. They are the author's, and the owner may overrule them:**
  - **(a) The tick rule for 6.5.** 6.5 is a measurement. It is ticked whether or not task 2's
    change is kept, when all of these hold:
    - each of the five spans whose recording is in the tree is printed as the operator reads it at
      HEAD, with its recording, its time, and `met` or `not met`;
    - `USED TO USE A FIRM` is stated as `not measurable here - recording not in the tree`, with the
      reading `DEV_ANALYSIS` §4 recorded for it. This follows 6.3's tick, which stated 20 conditions
      as not measurable here;
    - the result is written into `docs/phase-requirements/metrics.md`.

    A span is `met` only when the decoder's text over the span is the word, with a boundary on each
    side and none inside, and its letters are sure. Dim or unknown letters are printed as they are,
    and the span is then `not met`.
  - **(b) The keep rule for task 2's change, under R78. Every one of these must hold:**
    - MET-CER-SURE and MET-INVENTED rise on neither set;
    - sure-and-right coverage falls on neither set;
    - MET-WBE rises on neither set;
    - the adjudicated readings hold, or move onto their own adjudicated text (R66);
    - **V-11, per recording:** no keyed recording, real or synthetic, gains a boundary wrong or a
      sure-wrong letter. The count per recording is the test, as 452 read it. **Every recording where
      a wrong boundary moves, even if the count holds, is printed boundary by boundary**;
    - **of the five measurable spans, none that reads nearer its word at HEAD reads further from it
      after**, and at least one reads nearer. "Nearer" is the edit distance between the span's text
      and its word, spaces counted, printed for each span.

    A capture row or named row whose character count falls is reported, not refused, when every item
    above holds.
  - **(c) Unkeyed recordings, 452's section 4 item 3.** The two span recordings are unkeyed. For
    this unit, the six words are their only key, at the inferred grade the requirement states. **No
    other unkeyed text weighs in the keep rule.** Every unkeyed recording whose text changes is
    printed before and after, and its changes are counted as `mended`, `broken` or `neither` only
    where a keyed recording or a named span shows the same pattern. Otherwise it is printed, not
    judged (R72, V-13).
  - **(d) 452's section 4 item 1.** The relabel's share stays at 1.53 this unit. A change to that
    share is a second value of a kept rule, and is not built here.
  - **(e) What the change may not be.** Each of these has been tried or is excluded:
    - the rival-margin dim class (442);
    - the marks' speed (441);
    - a gap-duration threshold where the letter and word spaces overlap (444);
    - the mark-shape edge (445);
    - the from-cold pitch move (448);
    - the speed-search bounds (446);
    - the relabel's fallback share (452);
    - **a per-gap threshold on `013637`'s element-against-character split.** `DEV_ANALYSIS` §4
      measured those gaps 4 ms apart, and no per-gap threshold can separate them;
    - **replacing the cutter with the joint decoder of `DEV_ANALYSIS` §4.** That section asks for a
      ruling before a session builds it, and one unit is not that session. Report whether the trace
      says that is the only route left.
  - **(f) Step counts.** This unit is not a 2.4, 3.5 or 4.4 attempt. **DRIFT stays as it is: step 2
    at 0, step 3 at 0, step 4 at 1, step 5 at 1.** Step 6 is at 0 if the change is kept, and at 1
    if it is refused or not built.
- **V-04 and V-14:** no fixture is admitted by lowering a gate. No separation limit, confirmation
  rule or plausibility bound is loosened. **V-06:** no digital silence in a synthetic case. **V-13:**
  an inferred key is not proof by itself.
- **`CLAUDE.md` §12.5:** a fixture built from the same misunderstanding as the code proves nothing.
- **`CLAUDE.md` §0.0:** never present a guess as a decode. **§0.2:** nothing that keys or transmits.
- **HM-DEC-155:** no suite. Run named types only, one per invocation, each with its own `timeout`.
  Captures get 600 s. **Never background and poll.**
- **HM-DEC-165, FACT-004.** **If a package is needed: `MOVE: stop` (§6).**

## 4. Status cadence

At the start of each task, run `sh tools/status.sh EXECUTING "<n> of 3" code none "<one line>"`. At
the end, run it with `COMPLETED`. `SESSION.lock` belongs to the runner, so do not take or release
it. Write nothing to `RUN_LEDGER.md`, and touch nothing under `tools\arbiter\`.

What breaks in this shell:
- Apostrophes in quoted heredocs break, and doubled backslashes collapse.
- `;` is refused, `rm` is refused, and Python cannot run here.
- A multi-line commit uses `-m` more than once.

Scripts go in `.run-unit\unit453-<name>.sh` and run with `sh`. **Never compose a timestamp. Read the
clock.**

---

## 5. The tasks

### Task 0 - the entry

- Add `## UNIT 453 - STEP 6` to `PHASE_OUTCOME.md`, from the decision block at the foot.
- Name 453 in `PHASE_STATUS.md` with `CURRENT_STEP: 6`, and patch-bump the version.
- Commit the entry together with the root's uncommitted `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and
  `RUN_LEDGER.md`, and `PARKED.md` if it is modified. **These are the runner's writes. Commit them as
  they are, without editing them.**
- Run the entry round, and print each result as a number:
  - the build;
  - both carry-forward lines, **each with its wall time**;
  - the three floor tests, with the captures type's wall time;
  - the four metrics per condition, with the key's kind beside each number;
  - the count of files more than 25 Hz off, from 447's printer.
- **Save every recording's decoded text at HEAD** to `.run-unit/unit453-text-before.txt`.

### Task 1 - the measurement, then the trace of every misread span

**First, the measurement, which is 6.5's substance.** For each of the five spans in the tree,
print:
- the recording, and the span's start and end time;
- the word, and the decoder's text over the span as the operator reads it, with the confidence class
  of every letter;
- `met` or `not met` under §3 (a), and the edit distance to the word, spaces counted.

Then state `USED TO USE A FIRM` as `not measurable here`, per §3 (a).

**Then the trace.** For every span that is not met, print every mark and every gap inside it and
one gap either side:
- its length in ms, and in units of the unit in force;
- what the decoder called it: dit or dah, element gap, letter gap or word gap;
- what the word's own Morse says it should be, printed beside it. This is judging, not decoding;
- the speed in force and its proof state (451), and the pitch in force and its proof state (450);
- the thresholds in force, and where the read that set them came from.

**Say, for each span, where the first departure from the word's own Morse happens, and what kind it
is:**
- a mark misread;
- a gap misread, and which way;
- nothing emitted at all (squelch, emission gate, acquisition, or the pitch elsewhere).

**`021410` gets one more line.** On 2026-08-25 it read `ATEEKEND`, `TTHINKING` and `FLENX`, and at
HEAD it reads none of the three. State which of the kinds above explains that, and name the gate or
stage that withholds the letters.

**Group the departures by kind**, with counts, and name the commonest kind whose cause is in the
audio and outside §3 (e).

Commit the printer as a fact that asserts nothing, and commit its output as
`.run-unit/unit453-span-trace.txt`.

**Write the measurement into `docs/phase-requirements/metrics.md`**, and commit it on its own:
`unit453 task 1: HM-REQ-084 measured on its named spans (6.5)`.

**Tick 6.5 in both copies of `PHASE_PLAN.md` in that commit, under §3 (a).**

**Drop candidate:** the history of `021410`. That is when, between 2026-08-25 and HEAD, it stopped
reading the three words, found by decoding it at earlier commits. If time is short, name the
withholding stage from HEAD alone, and say the history was dropped. **The five spans' measurement,
the trace and the grouping are never dropped.**

### Task 2 - one change against the commonest movable cause

- **Choose the cause** from the trace: the commonest kind of departure whose cause is in the audio
  and outside §3 (e). Say why you chose it, and name the kinds you did not attack.
  - **If nothing is left outside §3 (e), build nothing.** Say so plainly and name what is left.
    That is a finding, not a failure of the unit.
- **Set the rule from the trace before any numbers are run**, and state it in one sentence.
  - It follows R72: nothing in it knows any word.
  - Try one rule. **If it is refused, do not try a second value of it.**
- **Build it in its own commit.** Judge it under §3 (b) with this table: every metric per
  condition, before and after, with the key's kind, and the per-recording V-11 column for
  boundaries wrong and for sure-wrong letters. Also print:
  - **each of the five spans, before and after, as text with its edit distance**;
  - 17:37's text, before and after;
  - every recording whose text changed, with its before and after text, under §3 (c).
- **If it is kept:**
  - Commit it: `unit453 task 2: <the rule> - <spans met before> to <after> of 5 (6.5)`.
  - Add the after-figures to `metrics.md` beside task 1's.
- **If it is refused:**
  - Leave `src` as it was.
  - Commit the diff as `.run-unit/unit453-notkept.diff`.
  - Name the line of §3 (b) that refused it.
  - Record the refused figures in `metrics.md`.

### Task 3 - the exit round

- Run the build, both carry-forward lines, the three floor tests, the four metrics, MET-PITCH-ERR
  and every type touched. Print each result as a number, with its wall time.
- **Report what changed in `src`, file by file.** Say that none of it keys or transmits.
- **Ticks:** confirm 6.5, per §3 (a). **Do not tick any other line.**
- **Make the exit commit and push it.**
- Report DRIFT per §3 (f).

---

## 6. Parked - do not touch, do not raise

- **Everything in `PARKED.md`:**
  - step 2's question on which files the transmit check covers. **This unit neither defines nor
    runs a transmit-file list.** It reports its `src` diff file by file;
  - 448's question on what "acquiring" means;
  - 450's questions on when a pitch may be called proved.
- 17:37's floor and 444's section 4 question, which stay with the owner.
- **452's section 4, answered or logged here and not chased:**
  - Item 1, the relabel's share against a Farnsworth sender: §3 (d). HM-REQ-054's TX-FARNS spacing
    is measured when step 7 builds the sender profiles.
  - Item 2, V-11 read net or boundary by boundary: §3 (b) keeps the per-recording count and prints
    every moved boundary.
  - Item 3, unkeyed recordings in R78: §3 (c) for this unit. Keying more recordings is not this
    unit's work.
- Earlier units' section 4 items.
- **Work that belongs to other lines:**
  - live against settled boundaries (6.4, HM-REQ-083);
  - the character table and the prosigns (6.1, 6.2);
  - the channel and sender profiles (7.1, 7.2);
  - the speed search (5.1);
  - the traffic-net print (3.4).
- The joint decoder of `DEV_ANALYSIS_2026-08-27.md` §4 (§3 (e)).
- The correctness phase's 5.1, which is Tim's, and all of `PHASE_PLAN.md` §7 (Carried).
- The decision log, the traceability table, and the 43 tests that measure something else. These are
  step 8 (R80).

## 7. What not to do

- Do not let any of the six words reach the decoder, as a list, a prior, a test hook or a constant
  in `src` (R72). They live only in the judging printer and in tests.
- Do not change MET-WBE's definition or its scorer (HM-REQ-082). If the scorer looks wrong, report
  it as a mismatch.
- Do not build anything §3 (e) excludes. Do not try a second value of a refused rule.
- Do not change the pitch path, the tracker, the speed search, or any proof state.
- Do not add a recording to the tree, and do not change a fixture's sidecar.
- Do not re-bank any floor. Do not tick 2.4, 2.5, 3.6, 4.7, 5.6, 6.6 or any line but 6.5.
- Do not edit `traceability.md`, the decision log, or any line of `PARKED.md`.
- Do not correct any ruling. Report the disagreement.
- Do not add a package. If one is needed, stop and report it.
- Do not halt on a question. Write one line in section 4 and go on.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 8. Committing and pushing

Make one commit per task, and **one commit for the change on its own** whether or not it is kept.
Each message names the criterion it serves: `unit453 task N: <what> (6.5)`. Push to `origin/main`
after each commit. End every commit message with:

```
Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
```

## 9. Reporting

Write `output.md` at the root. **`validate-output.bat` refuses it unless every one of these holds:**
- The ordering block comes first, within the first 60 lines. It has a `READ IN THIS ORDER.` line,
  then lines that begin `A.`, `B.` and `C.`.
- **Line C contains the words `Section 4 raises N items`.**
- The `UNIT:` line follows, without brackets.
- Then come exactly these four headings, and no fifth: `## 1. What Claude did`,
  `## 2. What the owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.
- Section 3 is not empty.

**Run `./tools/arbiter/validate-output.bat output.md` before you finish. Do not finish on INVALID.**

```
READ IN THIS ORDER.

A. Hamlet meets the CW requirements: step 6 at <n> of 6, step 5 at 1 of 6, step 4 at 5 of 7,
   step 2 at 3 of 5, step 3 at 3 of 6 by the plan's checkboxes, steps 0 and 1 done, 7 and 8 not
   started.
B. Step 6, criterion 6.5 (HM-REQ-084): <n> of 5 measurable spans met at HEAD, USED TO USE A FIRM
   not measurable here; ABOVE <text>, BREEZE <text>, WEEKEND <text>, THINKING <text>, FLEX <text>;
   021410 withheld by <stage>; the change <kept | refused by <line> | not built, because <why>>,
   spans met <n> to <n>; 6.5 <ticked | not ticked>; 6.1, 6.2, 6.4 and 6.6 open.
C. What this report adds, and whether it stands in the way of a criterion in B.
   Section 4 raises <N> items; <which, if any, is in the way of a criterion in B>.
```

```
UNIT:       453 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <which criteria flipped in PHASE_PLAN.md>
NUMBER:     HM-REQ-084 spans met <n> -> <n> of 5 measurable; MET-WBE real <n> -> <n> over 113; MET-CER-SURE real <n> -> <n>
DRIFT:      step 2 0; step 3 0; step 4 1; step 5 1; step 6 <0|1>
```

**Section 3 opens with the span table:** each of the six words, its recording, its time, the text
the operator reads at HEAD with the confidence classes marked, `met`, `not met` or `not measurable
here`, the edit distance, and the reading `DEV_ANALYSIS` §4 recorded. Then give:
- for each span not met, the marks and gaps under it against the word's own Morse, printed as a
  short aligned block, and where it first departs;
- the groups of departures, with counts;
- `021410` at HEAD against 2026-08-25, and the stage that withholds its letters;
- if a change was built, each span before and after, the §3 (b) table with the V-11 column, and
  every recording whose text changed, as text.

**Section 2, in one paragraph:** When the owner listens to these two recordings, which of the words
he named does Hamlet print right today? For those it gets wrong, is it the letters, the cuts between
them, or nothing printed at all? Did the change mend any of them? Give the evidence (V-13), and say
that the words are an inferred key.

---

```
ARBITER-DECISION
STEP: 6
APPROACH: measure HM-REQ-084 on its named acceptance spans in 021410 and 013637, print each span with every mark and gap in units, trace why each misreads, one change from the audio against the commonest cause kept under R78
MOVE: work around
WHY: PHASE_PLAN.md step 6 line 6.5 asks that HM-REQ-084 be measured on its named spans and the report print what each reads at HEAD; two of its three recordings are in the tree, no unit has measured it, and it is the one must-tier requirement written as text the operator reads. Step 2 cannot flip a line this pass: 2.4's count is 0 of 3 after 449's kept change, and 2.5 is held by 443's DECIDED (3) with 17:37's question parked with the owner.
STATE: partial
DECIDED: author's, overrulable - (1) step 6 is worked instead of the launcher's step 2, because 2.4 cannot reach 3 of 3 in one unit and 2.5 is held by 443's DECIDED (3) as 448's DECIDED (6) read it; 6.5 is chosen over 6.1, 6.2 and 6.4 because it is written as the text the operator reads (R83); (2) 6.5 is a measurement, ticked at task 1 on the five spans in the tree printed at HEAD with met or not met, and USED TO USE A FIRM stated not measurable here because 011447 was never in the tree, on 6.3's precedent for conditions not held; (3) the keep rule is R78 with V-11 by the count per recording, every moved boundary printed, and no measurable span reading further from its word, with the words a judging key only and never a decoder input (R72); (4) 452's section 4 is answered for this unit: the relabel share stays at 1.53, V-11 is judged by the count per recording with every moved boundary printed, and unkeyed text weighs only through the named spans; (5) the change excludes every route recorded no or kept at steps 2, 3 and 6, a per-gap threshold on 013637's 4 ms split, and the joint decoder DEV_ANALYSIS section 4 reserves for a ruling; if nothing is left, nothing is built and that is reported; (6) DRIFT for steps 2 to 5 stays as it is.
LICENCE: PHASE_PLAN.md step 6 line 6.5 and section 5's independence line; R72, R78, R80, R81, section 6; R82 and R83 as recorded in work instruction 441; arbiter rulings 442 DECIDED (2), 443 DECIDED (3), 448 DECIDED (3) and (6), 452 section 3 (a) and (d); V-04; V-06; V-11; V-13; V-14; HM-REQ-004, 010, 011, 080, 081, 084; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: the owner reads, word by word, which of the words he named Hamlet prints right today and why the others go wrong, and the commonest fault the audio can fix is fixed if the fix makes no recording worse
ADVANCES: step 6 criterion 5
END-ARBITER-DECISION
```
