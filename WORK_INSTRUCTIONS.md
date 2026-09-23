# Work instruction 410 - the number exists

**Seed under `--seed`.** The first unit of *Hamlet reads a CQ call correctly*, step 0. It
builds the scorer, scores every recording in the tree that has a key, and names what kind
of error dominates. It changes nothing under `src`. **Four tasks, drop from the back.**

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

**HM-DEC-155.** No suite. Only this unit's names and `docs\carry-forward-tests.txt`, run
as its top comment says. **Never background and poll.** One type per invocation, each with
its own `timeout`. A run lost before any assertion is re-run once and counted neither way.

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`.

**`TheIntegratorBandwidthTable.Write` runs 362 s; `OneDecoderNotTwoTests` will not fit a
600 s call.** Neither is in this unit's set.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm`
is refused; Python cannot run here; `-m` more than once for a multi-line commit. A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt. Multi-step
commands go into `.run-unit\unit410-<name>.sh` and run with `sh`.

## 3. Asks still outstanding

Carried per HM-DEC-139, verbatim in section 4. **The restore phase's 5.1 is Tim's and is
not this phase's debt.** Under R54, anything that blocks no criterion here goes to
`docs\phase-correctness\PARKED.md`.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  Build the scorer, score every keyed recording, and name the
            error kind that dominates.
ADVANCES:   step 0 criterion 2
DRIFT:      0 - the first unit of the phase
```

**Nothing in this project has ever measured whether the text is right.** Every number to
date counts characters emitted. The restore phase's last unit produced the first
correctness figure by accident: the bench reads 46 named characters on
`cw-2026-09-23-173723` and stands **29 edits** from its inferred key over the scored
region. That capture reads `CQ CQ CQ DEW B 6 RE D W B` where `CQ CQ CQ DE WB6RED WB6RED`
was sent. **Every letter is right. The spaces are wrong.** This unit turns that accident
into an instrument.

**Tim, 2026-09-23:** *"Build me a unit that can run. Always moving forward."*

---

## 5. Verify this instruction against the tree

Check, report any mismatch, repair nothing:

- `tests\fixtures\cw\captured\unadjudicated\cw-2026-09-23-173723.wav` and
  `cw-2026-09-23-173723.key.md` are on disk and tracked (committed in `65b4f9d0`).
- `TheSeventeenThirtySevenCaptureTests` exists and is 5 of 5, and how it replays the WAV.
- `TheAdjudicatedReadingsKeepReadingTests` holds 13 cases over three recordings -
  `013347`, `134712`, `003758` - and **what each asserts is the key material for them**:
  read the expected strings out of that file rather than inventing any.
- The floor table counts named characters and elements with placeholders separate
  (HM-DEC-168), and `captures` is 37 of 37 at 97 s.
- `CwProbabilisticDecoder.Judged` is where the emission gate applies `CharacterMargin` 1.0.

## 6. Rulings in force

`PHASE_PLAN.md` R59 to R61 and §3 and §6, read before task 1.

**R59** the yardstick is edit distance against keys over a scored region, with
unsure-per-real carried as the guard. **R60** nothing but 5.1 waits on Tim. **R61** a key
is inferred unless it was transcribed; a synthetic key is exact; **no key is ever invented
for audio nobody could read**.

**§3.1 a correctness number is always three parts**: edits, scored length, and whether the
key is exact or inferred. **Never a bare percentage.**

**CLAUDE.md §0.0** the 17:37 key is inferred and is never called what was sent; **§0.2**
nothing that keys is touched; **§12.5** a synthetic proves less than it looks;
**HM-DEC-091**, **HM-DEC-155**, **HM-DEC-165**, **FACT-004**, **FACT-006**.

**Record this in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md` §1's
table dated 2026-09-23, headline **The phase is *Hamlet reads a CQ call correctly*; the
restore phase is archived with 5.1 open**, ref HM-DEC-169:**

```
---
id: HM-DEC-169
date: 2026-09-23
refs: PHASE_PLAN.md R59 R60 R61, docs/phase-cw-run/, docs/phase-correctness/, PROJECT_CARD.md, work instruction 410 task 0, PHASE_GOAL.md
---

**The restore phase is archived with 5.1 open, and *Hamlet reads a CQ call correctly* is
the phase in force.** Tim, 2026-09-23.

**What is archived.** *CW decodes again*, set 2026-09-22, closes with all 29 loop criteria
ticked: the decoder restored to 2026-08-25 and reading in 97 s where HEAD took 1995 s, a CW
read guard on the carry-forward line, the inherited reds cleared or parked, the August
rework judged and discarded on numbers, the floors re-expressed as named characters, and an
emission gate that took placeholders across the 37 captures from 299 to 28 with no named
character lost. **5.1, Tim's verdict at the radio, is open and stays his.** The run folder
is `docs/phase-cw-run/`.

**What is set.** *Hamlet reads a CQ call correctly*: the first phase in this project to
score text. Edit distance against a key over a scored region, keys inferred from the fixed
form of a CQ call or exact by construction from the generator, unsure-per-real carried as
a guard so a decoder cannot score well by going quiet, then the fault the first measurement
names - letters right, word boundaries wrong. `PHASE_GOAL.md`'s 80 percent is what it
measures toward and not what it promises.

**Why a ruling and not an edit.** `PROJECT_CARD.md` holds standing facts and is changed
only by ruling (CLAUDE.md 13.3), and `PHASE` and `PHASE_SET` are two of them.

**Whose words are whose.** The phase name and yardstick are Tim's ruling; the wording is
work instruction 410 task 0's record of it. Rejected at the interview: W1AW bulletins as
the yardstick, kept as a confirming measurement; unsure-per-real alone as the yardstick,
kept as the guard.
```

## 7. Status cadence

As the header says. `NOTE` says what is moving inside the task.

---

## 8. The tasks

### Task 0 - the record

`PHASE_OUTCOME.md` gets its `## UNIT 410 - STEP 0` entry in the existing shape from the
decision block at the foot of this file. `PHASE_STATUS.md` names unit 410 and
`CURRENT_STEP: 0`. Patch-bump `Directory.Build.props`. `PROJECT_CARD.md` `PHASE: Hamlet
reads a CQ call correctly` and `PHASE_SET: 2026-09-23`. `DECISIONS.md` HM-DEC-169 and the
`CLAUDE.md` row. **Entry round:** both carry-forward lines and the three floor tests, every
number recorded.

**Drop candidate:** none.

### Task 1 - the scorer (0.1)

A scorer in `tests\Hamlet.RadioEngine.Tests\Cw`, reachable by the other Cw test types, that
takes a decode and a key and returns **edits, scored length, and the key's kind**. Edits are
Levenshtein over the scored region, spaces included, because spacing is the fault under
investigation. **Watch it fail first** on a case whose answer is known by construction: a
pair of strings whose edit distance can be counted by hand, written into the test, red
before the scorer exists and green after.

**How the scored region is chosen** is the author's, stated in the report, and taken from
the key file where the key file names one. The 17:37 key names its own: from the first `C`
of the first `CQ` to the last character emitted, and **the first third is never scored**.

**Drop candidate:** none.

### Task 2 - the baseline (0.2)

Score at HEAD, and table in `docs\phase-correctness\baseline.md`:

- **`cw-2026-09-23-173723`** against its inferred key.
- **The three adjudicated recordings**, `013347`, `134712` and `003758`, against the
  strings `TheAdjudicatedReadingsKeepReadingTests` already asserts. **Those strings are the
  key material; read them from that file and do not invent any.** Their kind is whatever
  that file's own provenance says - if it does not say, mark them inferred and say so.

Every row carries the three parts of §3.1. **The report states the phase's baseline total
in one line.** No percentage without its three parts anywhere in the report.

**Drop candidate:** none. The rest of the phase is measured against this table.

### Task 3 - what kind of error (0.3)

From the baseline, count per case: characters wrong, characters missing, characters added,
and **word boundaries misplaced** - a space where none was sent, or none where one was.
Count them from the alignment the scorer computes; **do not assert which dominates, report
the counts and let them say it.** The 17:37 case is expected to be dominated by boundaries,
and if it is not, that is the finding and the report says so plainly.

**Drop candidate:** the three anchors' breakdown. The 17:37 case alone is the minimum.

### Task 4 - the exit round

Both carry-forward lines and the three floor tests. `git diff` over `src` between entry and
exit prints nothing, and the report says so. The transmit files print nothing against
`7e209cb4`.

---

## 9. Parked - do not touch, do not raise

- **Any repair to the decoder.** Step 3's, after the baseline exists.
- **The generator and synthetic CQ calls.** Step 1's.
- **The pitch judge and the tracker.** Step 4's.
- **The restore phase's 5.1, and `#15`, `#43`, `#44`, `#42`.** Tim's and parked there.
- **The screen findings** - RF gain, the reflow, hover text - and `tonePeak`, `elementHz`,
  the `keying` line's wording. `OPEN_ISSUES.md` and `PARKED.md`.

## 10. What not to do

- **Do not change a file under `src`.** This unit measures.
- **Do not invent a key**, do not extend a scored region beyond what the key file names,
  and do not score the unscored stretch of the 17:37 recording (R61).
- **Do not call the 17:37 key a transcript** or any decode "what was sent."
- **Do not report a percentage without edits, scored length and the key's kind.**
- **Do not tune the scorer until a number looks better.** It is an instrument.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing. American spelling. UTF-8.**

## 11. Committing and pushing

Commit per task. Push at the end and say whether it succeeded.

---

## 12. Reporting

`output.md` at the root, canonical headings.

```
READ IN THIS ORDER.

A. The baseline table, with every row's edits, scored length and key kind.
B. Step 0's criteria: 0.1 the scorer, 0.2 the baseline, 0.3 the error kinds,
   0.4 the exit round. Steps 1 to 5 not started.
C. The rest. Section 4 raises <n> items.
```

```
UNIT:       410 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     baseline edits, 17:37: 29 -> <n> over <m> characters, inferred key
DRIFT:      0
```

**Section 2 tells Tim in one paragraph** what the number means and what it does not:
that it says how far the text is from a key, that the key for 17:37 is inferred from the
shape of a CQ call and not transcribed, and that nothing about the app has changed.

---

```
ARBITER-DECISION
STEP: 0
APPROACH: build a scorer that reports edits, scored length and the key's kind, score the 17:37 capture and the three adjudicated recordings at HEAD into a baseline table, and count what kind of error dominates
MOVE: continue
WHY: PHASE_PLAN.md step 0 criterion 0.2 asks for every recording in the tree that has a key scored at HEAD and tabled with its three parts as the phase's baseline
STATE: not started
DECIDED: the scorer's placement in the test project, the alignment it uses for the error breakdown and the per-type timeouts are the author's, overrulable
LICENCE: PHASE_PLAN.md R59, R60, R61, section 3, section 6; HM-DEC-169; HM-DEC-155; HM-DEC-139; CLAUDE.md 0.0 and 12.5; FACT-004
ACCOMPLISHED: the project can say for the first time how far its CW text is from what was sent, and every later change can be judged on that number
ADVANCES: step 0 criterion 2
END-ARBITER-DECISION
```
