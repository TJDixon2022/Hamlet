READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. No step moves, and nothing in the
   tree was changed by this session.
B. No criterion changes state. There was no work order to execute: the last
   commit deleted `WORK_INSTRUCTIONS.md` and wrote no replacement, so this
   session stopped without working (§9.6, HM-DEC-135).
C. The report last, and section 4 raises 3 items on top of unit 383's carried queue,
   the first of which is the missing order itself.

UNIT:       385 - stopped at task 0 of 0, no work order in the tree - 2026-09-21 21:20
PHASE GOAL: Hamlet keeps what it already has working - the hardening phase.
UNIT GOAL:  None was given. The prompt said to execute `WORK_INSTRUCTIONS.md` and there is no such file at the repository root.
ADVANCED:   no
NUMBER:     work orders found at the repository root 1 -> 0
DRIFT:      carried

## 1. What Claude did

**Surface and gate, checked first and against the tree rather than the prompt** (§0.3.1). Claude
Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`;
`PROJECT_CARD.md` says `PROJECT: Hamlet`, `Hamlet.sln`, `SHACK_FACTS.md` and
`src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` exist, `CoreHMI.sln` and `MURC.sln` do not,
and the root is `C:\Source\HamLet`. **The gate passed; the order did not exist.**

**Then it stopped.** `WORK_INSTRUCTIONS.md` is absent from the repository root. A work order is a
file and never pasted text (HM-DEC-135, §9.6), so there was nothing to execute and nothing this
session was authorized to do (§0.4). **No file under `src` or `tests` was touched, no test was
run, nothing was built, and nothing was committed but this report and the status file.**

**What the tree says, measured:**

- `HEAD` is `6c6232cc`, *phase: hardening plan rev5 - arbiter format, step 9, 2.6, status step 7
  field*, committed 2026-09-21 21:10:39 -0400. **That commit deleted `WORK_INSTRUCTIONS.md`**, and
  deleted `output.md` with it, and added no replacement for either.
- The last order ever committed to the root was **work instruction 379**, *the keyboard modes earn
  what FT8 earns*, 675 lines, present at `6c6232cc^` and at `059e3abe`. Before it, `c0e2b8f0`
  carried work instruction 378.
- **No order for 380 through 384 was ever committed.** `git log -S"Work instruction 384" --
  WORK_INSTRUCTIONS.md` returns nothing, and the root file at the last three commits that touched
  it reads 379, 379 and 378.
- `PHASE_STATUS.md` says `WORK_INSTRUCTION: 384 - the carry-forward list, five rounds in a row, on
  a tree that does not move`, and `CURRENT_STEP: 2`.
- **Unit 384 opened and then lost its order.** `9832f972`, *docs(unit384): task 0 - the record, the
  verdict, and the entry round that the dispatcher loop took twice*, 2026-09-21 20:48:53, committed
  a version bump and the outcome, status and plan entries - the shape every unit's task 0 has - and
  22 minutes later `6c6232cc` removed the order it was working from.
- Version in `Directory.Build.props` is **1.13.71**, as unit 384's task 0 left it. **This session
  did not bump it**, because a patch bump is task 0 of a work order and there is no work order.
- Working tree at the end: `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` modified by
  the launcher and **left exactly as they were found** (they are not this session's files), plus
  this `output.md` and `PROJECT_STATUS.md`. `git status` over `src` and `tests` is empty.

**Recorded under §12.1: nothing.** No ruling was made and none was needed: the rule that stopped
this session is already written (§9.6, HM-DEC-135).

## 2. What the owner should expect

**Nothing about Hamlet changed tonight, and nothing is broken by that.** The build, the tests and
every step of the hardening phase stand exactly where unit 384's task 0 left them at 20:48. The
session that should have run after it had no instructions to run: the arbiter's own plan commit at
21:10 removed the work order from the repository root and did not write a new one, so the next
session opened, checked the project gate, found no order, and stopped rather than guess at what
the unit was supposed to be. **That is the rule working**, not a failure of it - a session that
invents its own work order is a session editing your repository for reasons nobody wrote down.

What this costs: one round of the loop. What it needs from the arbiter: one file.

## 3. What you should see

| Question | Answer, measured |
| --- | --- |
| Does the project gate pass? | Yes - card, solution, both must-exist files, neither must-not-exist file |
| Is there a work order at the root? | **No.** `WORK_INSTRUCTIONS.md` does not exist |
| Which commit removed it? | `6c6232cc`, 2026-09-21 21:10:39 -0400, the arbiter's plan rev5 |
| Which order was last committed? | 379, at `6c6232cc^`; 380-384 were never committed |
| What does the launcher believe is running? | `PHASE_STATUS.md`: work instruction 384, step 2 |
| Did unit 384 start? | Yes - `9832f972` at 20:48 is its task 0 |
| Version | 1.13.71, unchanged by this session |
| Files this session changed | `output.md` and `PROJECT_STATUS.md`, nothing else |
| Tests run | **None.** With no order there is no unit's name to run, and HM-DEC-155 forbids running a suite instead |

**Every claim above is read out of git or the working tree. Nothing here is evidence about the
radio** (FACT-004).

## 4. What's blocking us

**The missing work order blocks everything.** Three items; the first is the blocker and is the
arbiter's, not Tim's. The carried queue follows them.

### Raised by this session

**1. There is no work order at the repository root, so no unit can run.**

*Blocking; the arbiter's to fix with one file.* `6c6232cc` deleted `WORK_INSTRUCTIONS.md` while
writing the plan revision. **What the next order has to account for**: `PHASE_STATUS.md` still
names work instruction 384 and unit 384's task 0 is already committed (`9832f972`) with the
version at **1.13.71** and its `PHASE_OUTCOME.md` entry written - so an order re-issued as 384
must not ask for a second task 0, and an order issued as 385 should say what becomes of 384's
open entry. *Rejected here*: reconstructing 384's order from `PHASE_STATUS.md`'s one-line summary
and the `.run-unit` files, which would be a session writing its own instructions (§0.4), and
carrying on with the previous order at `6c6232cc^`, which is 379 and whose step is closed.

**2. Five consecutive work orders were never committed.**

*A finding for the arbiter.* 380 through 384 ran - their task 0 commits are in the log - but no
order for any of them is in git, so **the instruction that produced each of those commits cannot
be read back**. HM-DEC-135's reason for making the order a committed file was exactly this: an
order that is gone when the window closes cannot be diffed, cited or checked against what the
session did. The rule holds; the last five rounds did not follow it.

**3. `output.md` was deleted by the same commit.**

*A finding, and this file replaces it.* HM-DEC-106 has the report live at the root. The copy that
was deleted is unit 383's, and it and every report before it are readable only in git history now.
Nothing is lost - they are committed - but a reader who opens the root sees no report until a
session writes one, and **unit 384 never wrote one at all**: it committed its task 0 and its order
was removed before it could report.

### Asks still outstanding - carried from unit 383's section 4, per HM-DEC-139, verbatim

The words below are unit 383's, from its line under `## 4. What's blocking us` to its end, as
committed in `6c6232cc^` - **the report `6c6232cc` deleted from the working tree**, which is why
they are quoted out of git rather than out of the root. Only that top-level heading is dropped, so
this report keeps four sections. Its nested queues are carried as unit 383 carried them. **No
order reached this session, so nothing in this queue was answered, marked or dropped.**


**1. 4.3 is met on the measurement, and partial under the strictest reading. A finding
with a number, not a ruling request.** 183 lines scanned with nothing excluded; **0**
tier-1 phrases in the sheet's own voice; **2** inside quotes, both declared, both proved
character for character to be sentences Hamlet itself produces; **9** tier-2 mentions
counted. Under the strictest reading of *no sentence tells the operator to touch the
radio*, a declared exception is still a sentence on the page that does - the page quotes
it and a man reading the page reads it. **The remedy would be a change to a sentence
Hamlet says.** One of the two is what Hamlet says when its own unkey did not get out, and
the only correct advice at that moment is the advice it gives; changing either is a change
to what the product tells the operator about a send and about the radio's safety, and that
is the owner's. **This is the honest limit of what a unit can reach**, and nothing is asked
for here.

**2. No quoted tier-1 hit failed its proof, so the item that would have been first does
not exist.** Both declared sentences are kind (a) - produced by the shipped view model,
character for character. Nothing on the sheet says something the screen does not.

**3. `_psk31Canned` has exactly one consumer and it is the token expression** - four
mentions, one read, at `MainWindowViewModel.cs:16727`. Ruling 2 item 4's stop was not
reached, so task 3 ran. What it did: it marks the press and re-routes nothing, and the
three macro rows still send through `AnswerPsk31Command` and the card's own
`CardActionCommand`. A finding, not a ruling request.

**4. A third name has joined the environmental family, and this unit's own load may be why
it showed.** `TheOliviaSendTests.StopMidPlayAbortsAnOliviaSend` went red on **both**
attempts of the exit named-type run with **two different failures** - unit 380's file-lock
`IOException` on the temp `jsonl`, then a wall-clock loss where the record had not been
written when the wait gave up - and is **green on its own in 273 ms**. It is not on the
carry-forward list and the carry-forward invocation was green. **It waits on wall time
under parallel load, which is unit 381's item 1's shape**, and the invocation it lost in
grew from 104 names to 112 tonight because this unit added them. **Recorded, not chased,
and not quarantined** - a finding for whoever picks up 2.3 or 2.4.

**5. The two inherited reds, reported and repaired neither**, verbatim and unmoved from
task 0 to task 4: `ViewTestsActThroughControlsTests.NoViewTestWritesAPropertyAControlOwns`,
`Assert.Empty` failure - *TheStopIsAlwaysOnScreenTests.cs:102 writes OperatingMode, which
the CW / Digital / Voice tab strip owns - press it instead*; and
`TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessedYourTurn`, `Assert.Equal` failure. Neither is
on the carry-forward list and neither is this unit's.

**6. One measurement refined the instruction and the measurement won.** Section 5 names
three sites for *stop it at the radio* and treats them alike. Measured: **`:19154` is the
SLOTTED path** - `WentLine`, FT8 and FT4 - and so is not reachable in this unit's two
modes at all; `:18913` is the unslotted cancelled-run line; `:19587` is the stop press's
own. **The sheet quotes `:19587`, which is the one a fixture produced whole.** `:18913`
says the same thing in different words - *Nothing Hamlet sent to stop the radio got out -
if it is still transmitting, stop it at the radio* - and **is not on the sheet**, because
no fixture here reached a cancelled run whose abort got nowhere. It is not a gap in any
criterion (4.2's backward direction is about refusals, and it is 12 of 12), but it is a
second operator-facing sentence about the same moment, and a later unit could quote it.
**A finding.**

**7. The sheet moved further over its target and the number is reported.** 183 lines and
11,585 bytes against 120 and 10,240, from unit 382's 176 and 10,918. **What bought it is
the stop sentence and the four lines that explain it**, which ruling 1 item 5 put there
because a page read at the radio that leaves out what Hamlet says when its own unkey did
not get out would be hiding the one sentence safety turns on. Nothing was dropped to pay
for it.

**8. Two decisions about shape that the owner should see.** Two new carry-forward names
were added that the instruction did not ask for (section 1, decision 1), and two commits
were merged that section 11 would have split (section 1, decisions 2 and 3), in both cases
to avoid leaving a commit tip with a carry-forward name red. **Findings, and both are
reversible in one line.**

**9. The `RULES_AT` id-scheme split is reported and not repaired**, as it has been by five
units now. `PROJECT_STATUS.md` reads `HM-DEC-165 (2026-09-19)` because `tools/status.sh`
writes that field as a literal, while `CLAUDE.md` §1 holds `CPS-DEC-0165`. **`tools\` is
not this unit's to edit.**

**10. `validate-output.bat` refused again - a sixth unit running - and the six rules were
hand-checked instead.** The exact command, in the shape section 2 prescribes:

```
./tools/arbiter/validate-output.bat output.md
```

The exact refusal: `This command requires approval`. **It is the permission mode and not
the syntax**, and a non-interactive session cannot answer the prompt. **What follows is a
HAND-CHECK against the script's own source, not a run of it**, and nothing below was
produced by the validator:

| Rule, from the script's own header | Hand-check |
|---|---|
| 1 - a `UNIT:` line above section 1, parseable | **ok** - line 29, within the 60 lines it reads |
| 2 - the four top-level sections, in order, exact names | **ok** - `## 1. What Claude did` (48), `## 2. What the owner should expect` (115), `## 3. What you should see` (141), `## 4. What's blocking us` (315) |
| 3 - no fifth top-level section | **ok** - there are exactly four `## ` lines; every deeper heading is `### `, which its pattern `^## ` does not match |
| 4 - section 4 present even when empty | **ok** - present and not empty, with a straight apostrophe as its own matcher expects |
| 5 - section 3 non-empty | **ok** - lines 141 to 314 |
| 6 - the ordering block above the `UNIT:` line, A, B, C, and C naming a count | **ok** - `READ IN THIS ORDER.` line 1, `A.` line 3, `B.` line 10, `C.` line 20, and C says *raises 10 items*, which its `raises \d+ item` pattern matches |

### The carried queue, verbatim per HM-DEC-139 - twenty-nine, and this unit answers none

Unit 382's item 5; unit 381's item 1; unit 380's items 1 and 4; unit 379's items 1, 3 and
7; unit 378's items 1, 3 and 4; unit 377's item 4; unit 376's items 3, 4 and 5; unit 375's
items 3 and 4; unit 374's item 3; unit 373's item 2; unit 372's items 4 and 7; unit 371's
five; unit 369's four.

**Which of unit 382's five came off.** Items 1, 2 and 3 came off, answered in instruction
383 section 3: the `mode` refusal token at `:16400` cannot fire under PSK31 or Olivia and
the token stays exactly as it is; the four `DigitalCaptureRefusal` values belong to the
thirty-second FT8 press; only `DigitalSendCqButton` at `MainWindow.axaml:3558` sends.
**Item 4 - that a sentence Hamlet says would hit R11's word list and the scan excluded
quotes - was this unit's whole subject and is answered tonight**: the scan now covers every
line, the hit is found, and it is declared and proved rather than excluded. **Item 5 stays
on** - five of the twenty-six quotes got kind (b) rather than kind (a), which is still five
of twenty-seven, and nothing tonight was licensed to reach those states.
