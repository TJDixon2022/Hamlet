# Work instruction 404 - the fourteen go back in as chains

**Authored by the arbiter.** This unit applies the fourteen rework pieces that units 395 to 397
could not apply alone. Each goes back inside the chain of earlier pieces it needs, and each
chain is applied as one piece under R55 and PHASE_PLAN.md section 6. A chain is kept only if
the three floor tests stay green and a named number moves. **Five tasks. Drop from the back
under the clock rule in task 3.**

**Status.** Run `sh tools/status.sh` on the real clock after every commit, after every task,
and immediately before every `dotnet test`. **Write every file as UTF-8.**

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

**HM-DEC-155.** Never run a whole suite. Run only this unit's names and
`docs\carry-forward-tests.txt`, and run the list as its top comment says. **Never background
a run and poll it.** Run one type per invocation, each with its own `timeout`. A run that dies
before any assertion is lost: re-run it once and count it neither way. A red on an assertion
is red and is never re-run.

**The report's `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>` or `^`
(PHASE_PLAN.md section 6). Write *tasks 0 to 4, none dropped* with commas.

## 2. The tool facts

- Apostrophes inside quoted heredocs break them.
- Doubled backslashes collapse.
- `;` is refused. `rm` is refused.
- Python cannot run here.
- For a multi-line commit message, use `-m` more than once.
- A bare `git worktree`, `git checkout` or `git show` is refused at the prompt.
- Put any multi-step command into `.run-unit\unit404-<name>.sh` and run it with `sh`.
  `git apply --check` and `git apply --3way` belong inside such scripts.

## 3. Asks still outstanding

Every ask carried from before this phase is parked in `docs\phase-cw\PARKED.md` under R54,
and the arbiter reads that file as *parked to a later phase*. None of those asks is this
unit's. The ruling on `7e65aac4` that unit 403 left for Tim is his. It blocks no criterion
here and is not carried.

---

## 4. Why this unit exists

**The count today.** Step 4 has six of seven criteria ticked. **4.7 is open.** Units 395 to
397 judged all 45 pieces and kept none. Fourteen pieces were never applied, because each
needs a chain longer than the one pair section 6 then allowed. R55, Tim's ruling of
2026-09-23, made **a chain of any length one piece**, applied in one commit and judged once.
No unit has tried this. The loop test on this approach found nothing.

Step 3's 3.6 is also open. Its one attack, unit 402, is recorded as no. Unit 403 then measured
Tim's capture and found `7e65aac4` moved nothing on it or on any of the 37 cases. Step 4 is
independent of step 3, so the loop works 4.7 and comes back to 3.6.

```
PHASE GOAL: CW decodes again.
UNIT GOAL:  Apply each of the fourteen chained rework pieces inside its chain,
            one commit per chain, judge each chain once on the floors and the
            named numbers, keep or take back out, and add a row per chain.
ADVANCES:   step 4 criterion 7
DRIFT:      0
```

---

## 5. Verify this instruction against the tree

Check each item below. **Report a mismatch in section 4 of the report; do not repair it.**

- HEAD is `bc2484d5` or a descendant whose commits touch no file under `src`.
- `7e65aac4` is the newest commit touching `src`.
- The fourteen hashes of 4.7 appear in `docs\phase-cw\unit395-rework.md` sections 1.2 and 2,
  each with its dependencies written in its row.
- `tests\Hamlet.RadioEngine.Tests\Cw\TheReworkNumbersPrinterTests.cs` exists. It prints the
  WEEKEND, THINKING, FLEX, ABOVE and BREEZE distances.
- The floors at entry are captures 37 of 37, adjudicated 13 of 13 and clean synthetics
  2 of 2, as unit 403 left them.

**Expected, and not a mismatch:** chain patches meeting `CwDecoder.cs` in a different shape
from what units 395 to 397 saw. `7e65aac4` added 21 lines there after those units. That is a
seam to adapt under task 2's rules, not a fault.

## 6. Rulings in force - do not re-argue

These come from PHASE_PLAN.md R47 to R55 and section 6, transcribed where they bind:

- **R50.** The restore is the engine folder only. **No file under `src\Hamlet.App` is
  written.** When a piece has an app half, a tools half or a test half, that half is not
  taken. A chain that needs its app half to build is adapted inside `Cw` or it counts as out.
- **R51.** A piece is kept only if the three floor tests stay green and one named number
  moves. The named numbers are:
  - 021410 above its floor of 47 characters, or its text nearer `WEEKEND`, `THINKING` and
    `FLEX`;
  - 013637 nearer `ABOVE` and `BREEZE`;
  - any capture above its character floor, with no capture below its own.
  A piece that moves nothing is not kept, and the report says so.
- **R55.** 4.7 takes the fourteen as chains. Each chain is one commit and gets one verdict.
  **Section 6:** a chain of any length is one piece.
- **4.3.** A chain that moves nothing is taken back out in the next commit.
  **4.4:** a kept chain's numbers become the new floors, and floors only rise.
- **4.7's own clause.** A chain that will not build after its seams are adapted is listed
  with its errors and counts as out.
- **Section 6.** A floor is never lowered. Anything that would change what keys or transmits,
  or a byte of the eleven transmit files, means `MOVE: stop`. A package is never added.
- **PHASE_PLAN.md section 3.** These eleven files are read and never written:
  - `CwTransmitter.cs`, `KeyerCwSender.cs`, `TransmitChain.cs`, `AutoCall.cs`
  - `AutoCallAnswers.cs`, `CwTransmitGuard.cs`, `TransmissionWatch.cs`
  - `TransmitReadiness.cs`, `TransmitPrivileges.cs`, `TransmitNotes.cs`, `ICwSender.cs`

  A chain hunk that touches one of them is dropped, HEAD's copy is kept, and the report
  says so.
- **CLAUDE.md section 0.0.** Never present a guess as a decode. A floor is a count, and no
  sentence says a chain makes CW "read".
- **CLAUDE.md section 0.2.** Transmit safety is absolute.
- **HM-DEC-091.** A change that improves one recording and quietly costs another is not a
  fix.
- **HM-DEC-155.** Never run a suite.
- **HM-DEC-165.** Nothing may be red that was green before.
- **FACT-004.** Every number is an indication.
- **FACT-006.** There is no radio on this machine.

## 7. Status cadence

As the header says. Put what is moving inside the task in `NOTE`: the chain, the pieces
applied so far, and the build state.

---

## 8. The tasks

### Task 0 - the record and the entry round

1. Append `## UNIT 404 - STEP 4` to `PHASE_OUTCOME.md` in the shape of the existing entries,
   copying the decision block at the foot of this file.
2. Set `PHASE_STATUS.md` to unit 404 and CURRENT_STEP 4.
3. Patch-bump `Directory.Build.props`.
4. Run the entry round and record every figure: both carry-forward lines as the list's
   comment says, then these by name, one type per invocation, each with its own timeout:
   - `TheCapturesThatDecodeKeepDecodingTests`, timeout 300 s
   - `TheAdjudicatedReadingsKeepReadingTests`, timeout 180 s
   - `CwFixtureTests.TheCleanRecordingsDecodeExactly`, timeout 120 s
   - `TheReworkNumbersPrinterTests`, timeout 300 s

   The eleven transmit files must print nothing against `7e209cb4`.

**Drop candidate:** none.

### Task 1 - the trace: find each chain before applying one

**Write no code in this task.** In a script, find each of the fourteen pieces' minimal
chain on HEAD's `src`:

- A **piece** is `git diff <hash>^ <hash> -- src/Hamlet.RadioEngine/Cw`, minus the transmit
  files.
- The **chain** is the smallest ordered set of earlier pieces from section 1.2's list, oldest
  first, after which every one of the piece's hunks passes `git apply --check`.
- Start from the dependencies each row already names, then add pieces until the check passes
  or the list runs out.

Then group the fourteen. **Author's, overrulable:**
- When one chain contains another, the two are judged once, as the longer.
- When two chains share any piece, they are merged into one.

Unit 395's rows point to roughly three groups:
- **the survey:** `4786c7e7` and `f2e1db7a` on pieces 7 and 9;
- **the lattice and the estimator:** `386fdb5d`, `fc1ee77f`, `68a18d66`, `a91d8fe7`,
  `a37cfcff` and `ee2cba8d`;
- **the decoder's hooks:** `4c6e4321`, `0f2089f3`, `62262b94`, `efcd5242`, `aeea24f2` and
  `9c2a7f99`.

That grouping is a hypothesis. The trace decides.

Write the chain table to `docs\phase-cw\unit404-chains.md`. It has one row per chain with:
- which of the fourteen it carries;
- every piece in it, oldest first;
- the hunks that would still refuse, if any;
- whether it touches a transmit file, or an app, tools or test file that R50 leaves out.

Commit it. **Every one of the fourteen must sit in exactly one chain.** If a piece fits no
chain, because no set of earlier pieces clears its check, it becomes a chain of its own. It
is then judged in task 2 as out under 4.7's build clause, with the refusing hunks as its
errors.

**Drop candidate:** none. Without the table there is nothing to apply.

### Task 2 - apply and judge each chain, smallest first

Take the chains in order of size, smallest first. For each chain:

1. **Apply it** in one commit: every piece in order with `git apply --3way`, restricted as in
   task 1.
   - Adapt seams only inside `src\Hamlet.RadioEngine\Cw` outside the transmit files, under
     unit 395's decisions 3 and 4: drop a hunk already in the tree, and meet a duplicate
     member by keeping the chain's.
   - List every adaptation with its file and reason.
   - A setting a piece shipped off stays off. The chain is judged as it shipped.
2. **Build** `Hamlet.sln` with warnings as errors. If it still does not build after the
   seams are adapted, record the errors verbatim with their count, revert, and mark the
   chain **out** under 4.7's clause.
3. **Measure**, one type per invocation, with task 0's timeouts: captures, adjudicated,
   clean synthetics and the printer. Record every capture's characters, elements, unsure and
   tone, and the five distances.
4. **Judge** under R51:
   - **Kept** only if all three floor tests are green and a named number moves, with no
     capture below its floor.
   - **Out** otherwise: revert in the next commit under 4.3 and say what was measured.
   - **If kept:** raise the floors of every capture that rose in the floor table under 4.4,
     in its own commit. The next chain is judged against the kept state.
5. **Row.** Add the chain's row to `docs\phase-cw\unit395-rework.md` section 2, under a new
   heading `### Chains under 4.7, judged by unit 404`, in that table's columns: chain, pieces,
   number before, number after, captures wall, kept or out, why.

**Drop candidate:** none inside this task. The criterion needs every chain's verdict.

### Task 3 - what the chains do to the six open reds

Run the types that hold 3.6's six red-open tests: `CwAcquisitionWindowTests`,
`CwReceiverFixtureTests`, `CwAdjudicationTests` and `CwFixtureTests`, one type per
invocation.
- **If any chain was kept:** run them at the kept state after the last chain.
- **If none was kept:** say so, and say this task measured HEAD again.

Report each of #6, #15, #42, #43, #44 and #45 with its number beside unit 402's. **This is a
record for 3.6, not an attempt at it.** It does not count toward 3.6's three attempts, and
nothing is changed to move a red.

**Drop candidate: this whole task.** **Clock rule:** if the unit has run past five hours
when task 2 ends, drop this task, say so, and go straight to task 4. If the clock is short
**inside** task 2, finish the chain in hand, leave the rest not started, and say which. 4.7
then stays open, and the report says so plainly.

### Task 4 - the exit round and the tick

1. Run both carry-forward lines and the three floor tests by name.
   - **HM-DEC-165.** If a kept chain turns red anything on either line that was green at
     entry, the chain goes back out in its own commit. Its row changes to **out**, with the
     name that turned red. The exit round is then run again.
   - The eleven transmit files print nothing against `7e209cb4`.
   - `git diff` over `src\Hamlet.App` between entry and exit prints nothing.
2. **Tick 4.7 in `PHASE_PLAN.md`** only if all three hold:
   - every one of the fourteen is in a chain with a verdict;
   - every chain has its row;
   - the floor tests are green at exit.

   Add the evidence in the bold form the ticked lines use: unit 404, the chain count, kept
   and out, and where the rows are. Any chain without a verdict means no tick.

**Drop candidate:** none.

---

## 9. Parked - do not touch, do not raise

- **3.6's six red-open tests**, beyond task 3's record. The next attempt is the loop's, after
  this unit.
- **`7e65aac4`: reverting it or keeping it.** Tim's.
- **Tim's 12:55 capture, and unit 403's items 1 to 4.** Logged, not chased.
- **The keying meter half of `9c2a7f99`**, parked as 397 item 2. If its chain is applied, the
  meter hunk rides with it only if the chain needs it to build. Otherwise it stays out.
- **Everything in PHASE_PLAN.md section 7.**

## 10. What not to do

- **Do not re-apply any of the 31 pieces already judged out on their own.** A piece enters
  this unit only as a link in one of the fourteen's chains.
- **Do not take any piece's `src\Hamlet.App`, `tools\` or test half.**
- **Do not flip a setting a piece shipped off** to find a number.
- **Do not lower a floor, and do not edit a floor test's assertion.**
- **Do not change a decoder file for any purpose but a chain's seam.** Do not repair a red
  here.
- **Do not describe any transcript as correct** (section 0.0).
- **No unfiltered `dotnet test`. Never background a run and poll it.**
- **Report mismatches; repair nothing. American spelling. UTF-8.**

## 11. Committing and pushing

- Commit per task, and per chain in task 2: the chain commit, its revert or floor-raise
  commit, and its row.
- Every chain commit message names the chain's pieces by hash.
- Push at the end, and say whether the push succeeded.

---

## 12. Reporting

Write `output.md` at the root with the canonical headings. **The ordering block comes
first**, and `validate-output.bat` refuses a report without it:

```
READ IN THIS ORDER.

A. PHASE GOAL - CW decodes again. Steps 1 and 2 done; step 3 partial on 3.6,
   six reds open; step 4 partial on 4.7 alone; step 5 is Tim's.
B. THIS STEP - step 4, the August rework judged on numbers. 4.1 to 4.6 met;
   4.7, the fourteen as chains, <met or not, with the count of chains judged
   and how many of the fourteen have a verdict>.
C. THIS REPORT - the chain table leads section 3; section 4 raises <n> items
   and <none | which> stands in the way of 4.7.
```

Then the six-line header:

```
UNIT:       404 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes - step 4 criterion 7 | no - and why>
NUMBER:     chains judged <n>, kept <n>, out <n>, of the fourteen <n> with a verdict
DRIFT:      0
```

**Section 3 leads with the chain table:**
- one row per chain: pieces, number before, number after, and kept or out;
- then any floor raised;
- then task 3's six reds beside unit 402's.

**Section 2 tells Tim in one paragraph** whether any of the August work earned its place back
when put in whole. It uses no word that claims a decode.

---

```
ARBITER-DECISION
STEP: 4
APPROACH: apply the fourteen chained rework pieces inside their dependency chains found by git apply --check, overlapping chains merged, each chain one commit judged once on the three floor tests and the printer distances, kept with floors raised or reverted, a row per chain
MOVE: work around
WHY: 4.7 is the one open criterion of step 4 and nobody has tried it - units 395 to 397 left the fourteen unapplied because section 6 then allowed only a pair, and R55 made a chain of any length one piece; 3.6's first attack is recorded no and unit 403 cleared its blocker without moving it, and section 5 of the plan routes to the independent step when one stalls, so the loop works 4.7 and returns to 3.6.
STATE: partial
DECIDED: author's, overrulable - a chain is the smallest ordered set of earlier pieces after which the piece passes git apply --check; nested chains are judged once as the longer and overlapping chains are merged; chains are applied smallest first; a setting a piece shipped off stays off; task 3's record of the six reds is not a 3.6 attempt; the per-type timeouts
LICENCE: PHASE_PLAN.md R50, R51, R55, criteria 4.3, 4.4 and 4.7, section 6 on chains, floors and transmit files, section 3; CLAUDE.md 0.0 and 0.2; HM-DEC-091; HM-DEC-155; HM-DEC-165
ACCOMPLISHED: every piece of the August rework has been put back and measured - the fourteen that could not go in alone go in with what they stood on, and each group earns its place on the numbers or comes back out, so none of that work is left unjudged
ADVANCES: step 4 criterion 7
END-ARBITER-DECISION
```
