# Work instruction 412 - tonight's thirteen become the mark

**Seed under `--seed`.** Tim worked 7.052 MHz from 00:39 to 00:46 UTC and the decoder read
a whole QSO: 625 characters, 11 unsure. This unit banks those thirteen captures as floors,
sets the guard from them, and closes step 2. **Five tasks, drop from the back.**

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

**HM-DEC-155.** No suite. Only this unit's names and `docs\carry-forward-tests.txt`, run as
its top comment says. **Never background and poll.** One type per invocation, each with its
own `timeout`.

**The report's four top-level headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

No other wording, no fifth top-level heading. Unit 410 was refused at stop 7 for writing
*What Tim should expect*.

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`.

**Thirteen more 30-second recordings join the capture floors.** The captures type runs 92 s
for 37 rows; expect around 125 s for 50 and give it 600.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt. Multi-step commands
go into `.run-unit\unit412-<name>.sh` and run with `sh`. Unit 411's runner scripts can be
copied under this unit's name.

## 3. Asks still outstanding

Carried per HM-DEC-139, verbatim in section 4. **Unit 411's item 2 is answered by R63 and
leaves the carried list. Its item 1, the RF gain scale, is not this unit's** - it changes
what is sent to the radio and is Tim's, carried. Under R54 anything blocking no criterion
here goes to `docs\phase-correctness\PARKED.md`.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  Tonight's thirteen captures are floors in the tree, and the
            guard that keeps every later change from going backwards.
ADVANCES:   step 2 criterion 5
DRIFT:      0
```

**Tim, 2026-09-24:** *"This should be our minimum benchmark and future iterations should
run against that. I don't want to go backwards."*

**What the thirteen hold.** 7.052 MHz, one session, transcripts cumulative across the run.
`cw-2026-09-24-003901` and `-003919` are the acquisition failure: 92 and 110 characters of
`E ET E E` before the decoder locks. From `-004027` onward it reads a QSO end to end -
callsigns, an email address, an ARRL operation, a sign-off - with the words shattered:
`R I C H ARD`, `OP ERA T I ON`, `S T M W O H`. **That shattering is step 3's target and is
not attacked here.**

**Why the floors matter more than the keys tonight.** Six changes this week read one
recording better and another worse (HM-DEC-091). Without these thirteen banked, a spacing
change has nothing to prove it did not cost Tim the read he watched tonight.

---

## 5. Verify this instruction against the tree

Check, report any mismatch, repair nothing:

- Thirteen `.wav` and thirteen `.txt` named `cw-2026-09-24-003901` through `-004550` are in
  `tests\fixtures\cw\captured\unadjudicated\`, untracked, plus `cases-2026-09-23.txt`.
  **Unit 411 saw them and did not commit them; they are this unit's to commit.**
- The capture floor table holds 37 rows with named characters and elements as floors and
  placeholders printed (HM-DEC-168), and how a row is added to it.
- `CwScorer` is in `tests\Hamlet.RadioEngine.Tests\Cw` with `Whole`, `Within` and
  `FromFirst`, and reports edits, scored length and the key's kind.
- `docs\phase-correctness\baseline.md` holds 33 edits over 46 characters over four
  recordings.
- Whether the sidecar's `text` field is cumulative across a session, so differencing
  consecutive files gives what was decoded between them. **If it is not, say so and task 3
  drops.**

## 6. Rulings in force

`PHASE_PLAN.md` R59 to R64, §3 and §6.

**R63** tonight's thirteen are the benchmark; **the older captures are not retired**; a
fixture retires only by ruling (HM-DEC-103). **R64** a unit that cannot advance its own
criterion reports and hands on; the arbiter authors the next against a different criterion
rather than halting.
**R57 / HM-DEC-168** a floor counts named characters, placeholders are free.
**§3.1** a correctness number is always edits, scored length, and the key's kind.
**§0.0** no decode is called what was sent. **§0.2** nothing that keys is touched.
**HM-DEC-091**, **HM-DEC-155**, **HM-DEC-165**, **FACT-004**, **FACT-006**.

**Record this in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md` §1's
table dated 2026-09-24, headline **Tonight's thirteen captures are the benchmark, and the
loop moves rather than halting**, ref HM-DEC-171:**

```
---
id: HM-DEC-171
date: 2026-09-24
refs: PHASE_PLAN.md R63 R64, criteria 1.6 2.5 6.7, work instruction 412 task 0, HM-DEC-091, HM-DEC-103, HM-DEC-168
---

**The thirteen captures of 2026-09-24 are the benchmark, the older captures stay, and the
loop moves on rather than halting.** Tim, 2026-09-24.

**What happened.** On 7.052 MHz between 00:39 and 00:46 UTC the decoder read a whole QSO -
625 characters, 11 unsure, callsigns clean and repeated - where two days earlier it read
nothing. Tim: *"This should be our minimum benchmark and future iterations should run
against that. I don't want to go backwards."*

**What is ruled.** The thirteen captures are banked with a named-character floor and an
element floor apiece, measured once at what they produced tonight. The older captures are
not retired: they are the guard against a change that reads one signal better and another
worse, and a fixture retires only by ruling. The locked-on run also gets inferred keys,
built by differencing consecutive transcripts, with ambiguous stretches left unscored.
`tonePeak` in a per-capture sidecar becomes a figure measured over that recording.

**And on the loop.** A unit that cannot advance the criterion it was authored for reports
what it measured and hands on; the arbiter authors the next unit against a different
criterion. Preference when nothing is blocked: the spacing first, then the screen.

**Whose words are whose.** The rulings are Tim's; the wording is work instruction 412 task
0's record of them. Rejected: retiring older captures; leaving tonePeak as it was; not
printing it at all.
```

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

`PHASE_OUTCOME.md` gets its `## UNIT 412 - STEP 2` entry from the decision block at the foot
of this file. `PHASE_STATUS.md` names unit 412 and `CURRENT_STEP: 2`. Patch-bump
`Directory.Build.props`. `DECISIONS.md` HM-DEC-171 and the `CLAUDE.md` row. **Commit the
thirteen captures, their sidecars and `cases-2026-09-23.txt`** in their own commit.
**Entry round:** both carry-forward lines and the three floor tests, every number recorded.

**Drop candidate:** none.

### Task 1 - the thirteen become floors (2.5)

Add the thirteen rows to the capture floor table, measured once at HEAD: named characters,
elements, placeholders, for the 30 seconds in each file - the sidecar's `inThis` line, not
its cumulative `characters` line. **The report prints all thirteen rows: file, named,
elements, placeholders.** Run the captures type whole afterward: **50 rows, and the
original 37 identical to entry.** No existing row is retired, lowered, or reworded.

**Drop candidate:** none.

### Task 2 - the guard (2.1, 2.2, 2.3)

- **2.1** `CwScorer` carries unsure characters per named character for the scored region,
  and `baseline.md` is re-issued with that column.
- **2.2** A named floor per keyed recording says how many named characters must be read at
  all, set from what each reads today.
- **2.3** Watch the guard work: make a change that suppresses most output, measure that it
  improves edits while breaking a floor from 2.2, and take it back out in the same unit.
  **Report both numbers.** That is the whole point of the criterion - a decoder must not be
  able to score well by going quiet.

**Drop candidate:** none. Step 3 cannot start until step 2 is done.

### Task 3 - the keys by differencing (1.6)

For each capture from `-004108` onward, subtract the previous file's `text` from this one's
to get what was decoded in those 30 seconds, then write a key file beside it: the scored
region, the key, and a statement that it is inferred and how it was built.

**Be conservative.** Where a word is shattered but unmistakable in context (`R I C H ARD`),
the key carries the word. **Where the differencing is ambiguous, or a callsign or number
cannot be read with confidence, leave that stretch out of the scored region** (R61: score
less rather than guess more). Score each with `CwScorer` and table them beside the baseline.

**Drop candidate:** whole task. If the clock is short, say how many were keyed and stop.

### Task 4 - the exit round

Both carry-forward lines and the floor tests, captures now 50. `git diff` over
`src\Hamlet.RadioEngine\Cw` between entry and exit prints nothing except what 2.3 put back;
the transmit files print nothing against `7e209cb4`.

---

## 9. Parked - do not touch, do not raise

- **The spacing.** Step 3's, and the next unit's under R64's preference. **Unit 405's G1** -
  an out-of-order gap reading is not separated - is its first candidate, described in
  `docs\phase-cw-run\` material; do not build it here.
- **6.3 the reflow, 6.4 hover text, 6.5 the dead button, 6.7 tonePeak.** Step 6's.
- **The RF gain scale, 255 against 100.** It changes what is sent to the radio. Tim's.
- **The acquisition failure** in `-003901` and `-003919`. Recorded as floors, not attacked.
- **Retiring any older capture.** Never here.

## 10. What not to do

- **Do not retire, lower or reword an existing floor row.**
- **Do not attack the spacing.** This unit banks and guards.
- **Do not invent a key.** Ambiguous stretches stay out of the scored region.
- **Do not edit a capture or a sidecar in the tree.**
- **Do not leave 2.3's suppression change in the tree.**
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing. American spelling. UTF-8. The four headings exactly.**

## 11. Committing and pushing

Commit per task. Push at the end and say whether it succeeded.

---

## 12. Reporting

`output.md` at the root, the four headings exactly as section 1 gives them.

```
READ IN THIS ORDER.

A. The thirteen rows with their floors, and the guard proved working.
B. Step 2's criteria 2.1 to 2.5, and step 1's 1.6 if task 3 ran.
C. The rest. Section 4 raises <n> items.
```

```
UNIT:       412 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     capture floor rows: 37 -> <n>; keyed recordings: 4 -> <n>
DRIFT:      0
```

**Section 2 tells the owner in one paragraph** that what he watched tonight is now the mark,
that no later change can read less than it without turning a test red, and that nothing
about the decoder changed in this unit.

---

```
ARBITER-DECISION
STEP: 2
APPROACH: bank the thirteen captures of 2026-09-24 as named-character and element floors measured at what they produced tonight, set the unsure-per-named guard and a read-at-all floor per keyed recording, prove the guard by breaking it, and key the locked-on run by differencing consecutive transcripts
MOVE: continue
WHY: PHASE_PLAN.md step 2 criterion 2.5 asks that the thirteen captures carry a named-character floor and an element floor each, measured once at HEAD, run with the other rows, with no existing row retired or lowered
STATE: not started
DECIDED: the form of the new floor rows, the conservatism of each inferred key, and the per-type timeouts are the author's, overrulable
LICENCE: PHASE_PLAN.md R57, R61, R63, R64, section 3, section 6; HM-DEC-171; HM-DEC-168; HM-DEC-103; HM-DEC-091; HM-DEC-155; CLAUDE.md 0.0
ACCOMPLISHED: what Tim watched the decoder read tonight becomes the mark every later change has to hold, and the spacing work has ten real cases to be judged on instead of one
ADVANCES: step 2 criterion 5
END-ARBITER-DECISION
```
