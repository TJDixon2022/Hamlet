# Work instruction 403 - the same recording, two builds

**Seed under `--seed`.** It builds no feature and repairs nothing. It decodes one
recording - the one Tim made at 12:55 UTC this morning - through the decoder as it stands
and through the decoder as it stood before `7e65aac4`, and prints both transcripts beside
each other. **Four tasks, drop from the back.**

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
its own `timeout`. A run that dies before any assertion is lost, re-run once, counted
neither way; a red on an assertion is red.

**The `UNIT:` line of the report carries no parentheses**, and no `&`, `|`, `<`, `>` or
`^` (`PHASE_PLAN.md` Â§6). Write *tasks 0 to 3, none dropped* with commas.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm`
is refused; Python cannot run here; `-m` more than once for a multi-line commit. A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt. Multi-step
commands go into `.run-unit\unit403-<name>.sh` and are run with `sh`.

## 3. Asks still outstanding

Carried per HM-DEC-139, verbatim in section 4. None is this unit's to answer, and under
R54 anything that blocks no criterion here is parked in `docs\phase-cw\PARKED.md`.

---

## 4. Why this unit exists

```
PHASE GOAL: CW decodes again.
UNIT GOAL:  Decode Tim's 12:55 UTC capture through the decoder at HEAD and
            through the decoder at a902cdf8, and print both transcripts.
ADVANCES:   none - clears a blocker: criterion 3.6
DRIFT:      0
```

**What Tim said, 2026-09-23:** *"The capture was much better before the 5 hour run last
night."* **What the tree says:** the overnight run left `src` byte-identical - 27 pieces
in and 27 back out - and the only commit that changed `src` since the restore is
**`7e65aac4`** at 08:33 local today, unit 402's repair for reds #24 and #41: 21 lines in
`CwDecoder.cs` that stop a pitch follow smaller than half the passband from counting as a
new sender. **Tim's capture is timestamped 12:55:15 UTC, twenty-two minutes after it.**

**What the sidecar shows**, `cw-2026-09-23-125515.txt`: tone 625 Hz, SNR 41.5 dB, duty
37%, 40 characters emitted and 27 unsure, 64 of 64 elements resolved, `decoderWpm 30`
against the radio's `KeyerSpeed 20 WPM`, and a transcript that is placeholders either side
of one clean run, `4 T EST IVDW <BT>`, whose span figures are in the hundreds where the
placeholders are single digits.

**The hypothesis this unit tests and does not assume:** holding one speed across a small
follow let a wrong speed persist, so elements resolve and characters will not name. **It
may be wrong.** The capture was made on a different signal from any fixture and nothing
here controls for that. Print both and let the numbers speak; Â§0.0 forbids calling either
transcript correct, because nobody knows what was sent.

---

## 5. Verify this instruction against the tree

Check, and report any mismatch rather than repairing it:

- `tests\fixtures\cw\captured\unadjudicated\cw-2026-09-23-125515.wav` is on disk (Tim's
  batch put it there). If it is absent, **stop at task 1 and say so** - there is nothing to
  decode and the rest of the unit is pointless.
- `7e65aac4` is the newest commit touching `src`, and `a902cdf8` is its parent.
- `git diff a902cdf8 7e65aac4 -- src` touches `CwDecoder.cs` only.
- The floors at HEAD: captures 37, adjudicated 13, synthetics 2.

## 6. Rulings in force

`PHASE_PLAN.md` R47 to R55 and Â§6. The ones that bind here: **R50** the restore is the
engine folder only; **R51** a piece is kept only on a named number; **Â§6** a floor is
never lowered; **CLAUDE.md Â§0.0** never present a guess as a decode - neither transcript
in this unit is "what was sent" and no sentence may say so; **Â§0.2** nothing that keys is
touched; **HM-DEC-091** a change that reads one recording and quietly costs another is not
a fix; **FACT-004** every number here is an indication; **FACT-006** no radio on this
machine.

**This unit reverts nothing and keeps nothing.** It measures. The ruling on `7e65aac4`
is Tim's and comes after he reads the two transcripts.

## 7. Status cadence

As the header says. `NOTE` says what is moving inside the task.

---

## 8. The tasks

### Task 0 - the record

Append `## UNIT 403 - STEP 3` to `PHASE_OUTCOME.md` in the shape of the existing entries,
copying the decision block at the foot of this file. `PHASE_STATUS.md` names unit 403.
Patch-bump `Directory.Build.props`. Commit the capture files Tim's batch placed under
`tests\fixtures\cw\captured\unadjudicated` if git sees them untracked. **Entry round:**
both carry-forward lines, and the three floor tests by name, all recorded.

**Drop candidate:** none.

### Task 1 - the capture at HEAD

Write a printer fact, `TheCaptureOfTheTwentyThirdTests`, beside the other printers under
`tests\Hamlet.RadioEngine.Tests\Cw`, that reads
`cw-2026-09-23-125515.wav` through `CwDecodeHarness` and writes with `_output.WriteLine`:
the settled transcript whole, characters emitted, characters unsure, elements seen,
elements resolved, the winning speed, and the tone the survey admitted. It asserts only
that the file was read and something was emitted; **it asserts no text**, because nobody
knows what was sent. Run it, keep the output in `.run-unit\unit403-head.txt`.

**Drop candidate:** none.

### Task 2 - the same capture at `a902cdf8`

In a script: `git worktree add --detach C:/Source/HamLet-wt403 a902cdf8`, copy the printer
from task 1 into the worktree's test folder, build there, run the printer there with its
own `timeout`, keep the output in `.run-unit\unit403-before.txt`, then
`git worktree remove --force C:/Source/HamLet-wt403`. **The worktree is removed whatever
happens**, and `git worktree list` in the report proves it.

If the printer will not compile at `a902cdf8` because the harness differs there, copy
HEAD's `CwDecodeHarness.cs` in beside it and say so in the report - the harness is a test
of the tests, not the decoder, and HM-DEC-091 is satisfied by both sides using the same one.

**Section 3 of the report leads with the two transcripts, one above the other**, and a
table: characters emitted, unsure, elements seen, resolved, winning speed, tone, for HEAD
and for `a902cdf8`.

**Drop candidate:** none. Without this there is no comparison.

### Task 3 - what it costs the fixtures

At HEAD and at `a902cdf8`, in the same two builds, run
`TheCapturesThatDecodeKeepDecodingTests` and record for each of the 37 cases the characters
and elements measured. **Report only the cases whose numbers differ between the two
builds**, with both numbers. This says whether `7e65aac4` moved anything the floors can
see, which the floors alone cannot, since they only fail when a number falls below its
floor.

**Drop candidate:** this whole task. If the clock is short, say it was dropped.

### Task 4 - the exit round

Both carry-forward lines and the three floor tests. Nothing under `src` changed by this
unit; `git diff` over `src` between entry and exit prints nothing, and the report says so.

---

## 9. Parked - do not touch, do not raise

- **Reverting or keeping `7e65aac4`.** Tim's, after he reads the report.
- **Reds #6, #15 and #42 to #45**, and 4.7's fourteen chains. The loop's, after this.
- **The screen findings** - the RF gain sentence, the window reflow, hover text. A later
  phase's, recorded in `OPEN_ISSUES.md`.
- **`tonePeak` and `elementHz` reading oddly in the sidecar.** Park them in
  `docs\phase-cw\PARKED.md` as 403 items if seen; chase neither.

## 10. What not to do

- **Do not change a file under `src`.** Not one line, not to test a theory.
- **Do not revert `7e65aac4`.** Measuring is not ruling.
- **Do not assert any text against either transcript**, and do not write in any report
  that either is correct or nearer correct. Nobody knows what was sent (Â§0.0).
- **Do not adjudicate the capture** or add it to any floor table.
- **No unfiltered `dotnet test`. Never background and poll. Do not leave the worktree.**
- **Report mismatches; repair nothing. American spelling. UTF-8.**

## 11. Committing and pushing

Commit per task. Push at the end and say whether it succeeded.

---

## 12. Reporting

`output.md` at the root, canonical headings.

```
READ IN THIS ORDER.

A. The two transcripts, one above the other, with the table.
B. Whether any of the 37 capture cases moved between the two builds.
C. The rest. Section 4 raises <n> items and none blocks a criterion.
```

```
UNIT:       403 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   no - a blocker-clear by design
NUMBER:     characters unsure on the capture: HEAD <n>, a902cdf8 <n>
DRIFT:      0
```

**Section 2 tells Tim in one paragraph** what the two transcripts look like beside each
other, in plain words, without claiming either is right.

---

```
ARBITER-DECISION
STEP: 3
APPROACH: decode Tim's 12:55 UTC capture through the decoder at HEAD and at a902cdf8, the commit before unit 402's speed repair, and print both transcripts with their counts, changing no file under src
MOVE: work around
WHY: PHASE_PLAN.md step 3 orders the remaining repairs one at a time under R49, and the operator reports the decoder reading worse on the air than before 7e65aac4, so whether that commit cost the decoder decides whether the remaining repairs are built on a decoder that reads
STATE: partial
DECIDED: the worktree path C:/Source/HamLet-wt403, the printer's name and the per-type timeouts are the author's, overrulable
LICENCE: PHASE_PLAN.md R49, R50, R51, R55, section 6; CLAUDE.md 0.0 and 0.2; HM-DEC-091; HM-DEC-155; HM-DEC-139; FACT-004
ACCOMPLISHED: Tim can see what the same recording reads as under both decoders, and rule on 7e65aac4 from two transcripts rather than from one impression
ADVANCES: none - clears a blocker: criterion 3.6
END-ARBITER-DECISION
```
