# Work instruction 434 - an instrument that knows where a tone is

**Seed under `--seed`.** Six units measured four rules about when the mixdown may follow the
tracker, and kept none, because unit 433 showed the evidence points the same way as the
tracker's mistake. The fault is the tracker's choice of pitch, and it is now open - **but not
before there is an instrument good enough to judge it.** This unit builds that instrument.
Four tasks, drop from the back.

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
own `timeout`. The captures type is 51 rows and runs about 120 s; give it 600 s.

**The report's four top-level headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`.

**`ADVANCES` reads exactly `step 4 criterion 1`.** **WHY cites a line of the plan.**

**Write `output.md` at the root before the session ends, whatever else happened.**

**Nothing in section 4 halts this phase.** Park it and go on.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt. Multi-step commands
go into `.run-unit\unit434-<name>.sh` and run with `sh`. **A denial parks the criterion and is
recorded; no session widens its own `allowed.txt`.**

## 3. Asks still outstanding

Carried per HM-DEC-139, verbatim in section 4. **None is this unit's to answer.** P27 and unit
427's six stay the owner's; P43, P45 to P48 stay parked.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  An instrument that says where a tone actually is, proved
            against tones known by construction.
ADVANCES:   step 4 criterion 1
DRIFT:      0
```

**Why the tracker could not be judged.** The restore phase's 3.8 was ticked on a sweep that
admitted **5 of 38 cases** and, on the three synthetics whose tone is known by construction,
**landed 60 to 85 Hz high** - worse than the decoder it was judging. Every question about the
tracker since has been asked with that instrument or with none.

**What the tracker gets wrong, measured.** Unit 433, on the 7.052 opening at 30.54 s: the
decoder's envelope ranks **525 Hz at 15.02 dB**, above 850 Hz at 14.39, above **the sender's
own 625 Hz at 13.67**, above the mix at 600 Hz at 12.69. On the air, `cw-2026-09-24-135641`
mixed at 675 Hz while the survey named a tone at 600 Hz, +15.7 dB, keyed 87% of the time, and
read nothing in 44 minutes; `cw-2026-09-24-152135` mixed at 675 with the survey at 600 and read
nothing in 26. Eleven minutes later, `cw-2026-09-24-153202` mixed at 505 with the survey at
500 and read 34 characters in 30 seconds. **When the mix lands on the tone, it reads; when it
lands 50 to 75 Hz off, it reads nothing.**

**R76:** nothing touches `CwToneTracker` until this instrument exists and is proved. This unit
builds and proves it, and changes no decoder code.

---

## 5. Verify this instruction against the tree

Check, report any mismatch, repair nothing:

- `CwToneSurvey`, its step of 25 Hz, its range, its window, and where it is called.
- The synthetic fixtures whose tone is known by construction: which they are, what tone each
  was generated at, and where the generator records it. **Name them; they are 4.1's proof.**
- Whether the earlier 38-case pitch table from the restore phase's unit 409 survives anywhere,
  and in what form.
- The captures named in section 4 are on disk, and what each sidecar's `toneHz` says.
- `CwToneTracker`'s public surface, **read only** - this unit changes none of it.

## 6. Rulings in force - do not re-argue

`PHASE_PLAN.md` R59 to R76, §3 and §6.

**R76** the instrument is built and proved before the tracker is touched. **This unit changes
nothing under `src\Hamlet.RadioEngine\Cw`.** It adds an instrument, and where that instrument
lives - test project or engine - is the author's, stated in DECIDED, with the rule that **if it
lives in the engine it is not wired into the decode path in this unit**.
**R75** the tracker is open, later, and not here.
**§0.0** never present a guess as a decode; an instrument that cannot measure a case **says so
and reports no number**.
**§12.5** a fixture built from the same misunderstanding as the code proves nothing - which is
why 4.1 is proved on tones known by construction and **the instrument shares no line of code
with `CwToneTracker`**.
**§0.2** nothing that keys or transmits is touched.
**HM-DEC-155**, **HM-DEC-165**, **FACT-004**, **FACT-006**.

**Record this in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md` §1's table
dated 2026-09-25, headline **The tone tracker is open, once an instrument can judge it**, ref
HM-DEC-182:**

```
---
id: HM-DEC-182
date: 2026-09-25
refs: PHASE_PLAN.md R75 R76 and criteria 4.1, 4.5, 4.6, 4.7; unit 433 output.md; HM-DEC-095; HM-DEC-127; work instruction 434
---

**`CwToneTracker` is open to change in this phase, and an instrument able to judge it is built
and proved first.** Tim, 2026-09-25.

**What was measured.** Units 428 to 433 built four rules about when the mixdown may follow a
pitch move and kept none. Unit 433 measured why: on the 7.052 opening at 30.54 s the decoder's
own envelope ranks 525 Hz at 15.02 dB above 850 Hz at 14.39, above the sender's 625 Hz at
13.67, and above the mix at 600 Hz at 12.69. The evidence a follow rule could weigh points the
same way as the tracker's mistake, so no such rule can undo a choice the evidence supports.

**What is ruled.** The tracker's choice of pitch is open to change, and HM-DEC-095 and
HM-DEC-127 are amended to that extent and no further: their reasoning stands, and a unit that
changes the tracker states which of their clauses it works against and why. **No unit changes
the tracker until criterion 4.1 is met** - an instrument finer than 25 Hz, proved against tones
known by construction, sharing no line of code with the tracker. The restore phase's 3.8 was
ticked on a judge that admitted 5 of 38 cases and missed a known tone by 60 to 85 Hz, and that
is why the question has been unanswerable. When a tracker change is kept, criteria 3.6 and 7.4
are re-measured and reopened or ticked on the numbers.

**Whose words are whose.** The ruling is Tim's; the wording is work instruction 434's record of
it.
```

**Also at task 0, correct the record by appending** (the record is append-only): the
`UNIT 2 - STEP 1` entry of 2026-09-14 in `PHASE_OUTCOME.md` or its archived phase folder is
false. **Do not edit or delete the row.** Append a note beneath it naming it as recorded in
error, with today's date and this unit's number. If the row cannot be found, say so.

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

`PHASE_OUTCOME.md` gets its `## UNIT 434 - STEP 4` entry from the decision block at the foot of
this file. `PHASE_STATUS.md` names unit 434 and `CURRENT_STEP: 4`. Patch-bump
`Directory.Build.props`. `DECISIONS.md` HM-DEC-182 and the `CLAUDE.md` row. The appended note
for the 2026-09-14 row. **Entry round:** both carry-forward lines, the three floor tests, the
keyed totals, recorded.

**Drop candidate:** none.

### Task 1 - the instrument (4.1)

Build a pitch instrument, in its own type, that takes audio and returns **the pitch it finds,
its confidence, and how many bins it looked in** - or says plainly that it cannot measure this
audio and returns no pitch. Resolution finer than 25 Hz; **how much finer is the author's**,
chosen from what the synthetics can prove and not from what a later unit would like.

**It shares no line of code with `CwToneTracker` or `CwToneSurvey`** (§12.5). If that means
writing a transform from scratch, write it.

**Watch it fail first** against a stub, on a synthetic tone whose frequency is known by
construction.

**Drop candidate:** none.

### Task 2 - the proof (4.1's second half)

Over every synthetic fixture whose tone is known by construction, print a table: the case, the
tone it was generated at, the pitch the instrument found, **the error in hertz**, its
confidence, and the wall time per hop.

Then state, plainly and as numbers:

- how many cases the instrument admits at all, and how many it refuses;
- the largest error in hertz over the cases it admits;
- whether every admitted case lands within one of its own bins of the truth.

**If it does not, say so and do not tick 4.1.** A second attempt at the method is task 3's; a
third is the next unit's. **Do not widen a bin to make a case pass** - that is choosing the
instrument from what it admits, and it is how 3.8 got ticked on a bad judge.

**The restore phase's sweep missed these by 60 to 85 Hz. Print its error beside the new one**
for the same cases, so the comparison is on the page.

**Drop candidate:** the sweep comparison column.

### Task 3 - where the tracker went wrong (4.5)

Turn the instrument on the three places the tracker is known to have been wrong, and print
what it says:

- **the 7.052 opening at 30.54 s**, where the envelope ranked 525 Hz over the sender's 625;
- **`cw-2026-09-24-135641`**, mixed at 675 while the survey named 600 and nothing was read;
- **`cw-2026-09-24-152135`**, mixed at 675 while the survey named 600 and nothing was read.

For each: what the instrument reports, what the tracker took, what the survey said, and
**whether the instrument would have named the sender's pitch over the tracker's**. Say it in
one line per case.

**Change nothing.** This is the evidence 4.6 is built on, and 4.6 is not this unit's.

**Drop candidate:** whole task, with what was measured stated. If dropped, 4.5 stays open and
the next unit does it.

### Task 4 - the exit round

Both carry-forward lines, the three floor tests with captures at 51, the keyed totals, and
every type touched. **`src\Hamlet.RadioEngine\Cw` prints nothing against entry** - no decoder
change - and the transmit files print nothing against `7e209cb4`.

---

## 9. Parked - do not touch, do not raise

- **`CwToneTracker` itself.** 4.6's, and only after 4.1 and 4.5 (R76).
- **3.6 and 7.4.** They reopen under 4.7 when a tracker change is kept.
- **7.1 the speed ceiling, 6.5 the dead button, 7.8 the attenuator sentence.** Other criteria;
  the loop routes there on its own.
- **Unit 427's six findings, P27, P43, P45 to P48.**
- **The decode path.** Nothing built here is wired into it.

## 10. What not to do

- **Do not change anything under `src\Hamlet.RadioEngine\Cw`.**
- **Do not reuse `CwToneTracker`'s or `CwToneSurvey`'s code** in the instrument (§12.5).
- **Do not widen a bin, a window or a tolerance to make a case pass.**
- **Do not tick 4.1 on a table that does not meet it.**
- **Do not report a pitch for audio the instrument cannot measure** - say it cannot (§0.0).
- **Do not edit the 2026-09-14 row.** Append beneath it.
- **Do not touch what keys or transmits.**
- **Do not halt for a question.** Park it.
- **Write `output.md` before the session ends.**
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing. American spelling. UTF-8. The four headings exactly.**

## 11. Committing and pushing

Commit per task. Push at the end and say whether it succeeded.

---

## 12. Reporting

`output.md` at the root, the four headings exactly as section 1 gives them.

```
READ IN THIS ORDER.

A. The instrument's largest error in hertz on tones known by construction,
   and how many cases it admits.
B. Step 4: 4.1 met or not with the table, 4.5 if task 3 ran.
C. The rest. Section 4 raises <n> items, none blocking.
```

```
UNIT:       434 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     largest error on a known tone: 60 to 85 Hz -> <n> Hz; cases admitted <n> of <n>
DRIFT:      0
```

**Section 3 leads with the known-tone table**, the new instrument's error beside the old
sweep's.

**Section 2 tells the owner in one paragraph** that nothing in the app changed, and what the
instrument now makes possible that was not possible before.

---

```
ARBITER-DECISION
STEP: 4
APPROACH: build a pitch instrument finer than 25 Hz that shares no code with the tone tracker or the survey, prove it against tones known by construction with its error in hertz per case, and turn it on the three places the tracker is known to have been wrong
MOVE: continue
WHY: PHASE_PLAN.md step 4 criterion 4.1 asks for a pitch instrument whose resolution is finer than 25 Hz, shown on the synthetic cases whose tone is known by construction to land within one of its own bins of the truth, with its error tabled per case and its cost per hop measured
STATE: not started
DECIDED: the instrument's method, its resolution, where it lives, and the per-type timeouts are the author's, overrulable, and are chosen from what the synthetics can prove rather than from what a later unit would like
LICENCE: PHASE_PLAN.md R75, R76, section 6; HM-DEC-182; HM-DEC-095; HM-DEC-127; CLAUDE.md 0.0, 0.2 and 12.5; HM-DEC-155; FACT-006
ACCOMPLISHED: the project can say where a tone actually is, so the tracker's choices can be judged instead of argued about
ADVANCES: step 4 criterion 1
END-ARBITER-DECISION
```
