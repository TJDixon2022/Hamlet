# Work instruction 455 - a prosign arrives as a prosign

**Loop unit.** Step 6, criterion 6.2: HM-REQ-071 and HM-REQ-072 each get a test naming them,
and the report says whether each is met. **The output is two tests, both watched failing first,
and a plain statement for each requirement: met or not met, with the text the decoder emits.**
If one is not met, one change built to meet it and judged under R78. Four tasks, drop from the
back.

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

**HM-DEC-155.** No suite. Run named types only, one per invocation, each with its own `timeout`.
Captures get 600 s. Never run in the background and poll.

Apostrophes in quoted heredocs break. Doubled backslashes collapse. `;` is refused. `rm` is
refused. Python cannot run here. A multi-line commit uses `-m` more than once. Scripts go in
`.run-unit\unit455-<name>.sh` and are run with `sh`.

**The four report headings, exactly:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`. Write the `UNIT:` line without
brackets. Write `ADVANCES: step 6 criterion 2`. WHY cites the plan.

**A CW question is answered from the documents, never raised to the owner** (R85). Section 4
records the reading in one line, and the work goes on.

---

## 2. Why this unit exists

**The count today.** Step 6 has 2 of 6 met (6.3 and 6.5). Step 2 has 3 of 5 met, step 3 has 3
of 6, step 4 has 5 of 7, step 5 has 1 of 6, step 7 has 0 of 5, step 8 has 0 of 6 and step 9 has
0 of 8.

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  A prosign sent as one run reaches the operator as one symbol,
            named as the terminal is set to name it, and two tests that
            name HM-REQ-071 and 072 say so.
ADVANCES:   step 6 criterion 2
```

**Read `CW_REQUIREMENTS.md` section H and `CW_SPEC.md` §6.2 first. They win over this
instruction.**

- **HM-REQ-071 (must):** *"When a run of elements is sent with no character gap in it and matches
  a prosign, the decoder shall emit it as one symbol."* Prosigns arrive as prosigns and are never
  split into letters.
- **HM-REQ-072 (must):** *"Where one pattern has both a punctuation name and a prosign name, the
  decoder shall emit one symbol and name it per the terminal's setting."* This is naming, not a
  claim about the signal.
- **`CW_SPEC.md` §6.2:** `=` and `BT` are one symbol with a naming choice, and so are `+` and
  `AR`. They are not two entries.

**Why step 6, and not the launcher's step 2.** Step 2 cannot flip a line this pass:

- **2.4:** unit 449's kept change reset the count of units with no kept change to 0 of 3, and
  no step-2 unit has run since.
- **2.5:** held red by 443's DECIDED (3). The named floors are 17:37 at 38 of 46, `032113` at
  43 of 45 and `032129` at 42 of 64.

Step 9 is also closed for now. Its first row (9.1) needs fldigi's source, the session's clone
was refused, and `.run-unit\fldigi\` is still absent. Every later row of step 9 waits on 9.1.
Unit 454's section 4 asked the owner for that source. The request is logged here and not
chased.

**Why 6.2 over the other open lines:**
- **It changes what the operator reads (R83).** A sign-off sent as one run and printed as `EN`
  or `RK` is a wrong letter printed with confidence.
- **It can be judged in one unit,** on a synthetic fixture whose key is exact.
- **The other open lines of step 6:**
  - 6.1 needs a vendored M.1677-1 file and a cited ARRL source, and the unit must first check
    that they exist.
  - 6.4 cannot be judged until 6.1's table is settled.
  - 6.6 is held by the floors, like 2.5.

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1 and carry on with the task as the tree allows.

- **`MorseAlphabet.cs`.** Its remarks say prosigns come out as `<AR>` and that where a pattern
  has two names, "the prosign wins". Confirm this:
  - which prosigns its table holds;
  - whether `KN`, `BK` and `CL` are in it;
  - whether the name is fixed in code or read from anything the operator can set.
- **The terminal's setting.** Find whether any setting in `src\Hamlet.App` or the engine names
  `=`/`BT` or `+`/`AR`. **Expected: none**, which would leave HM-REQ-072 not met at HEAD.
- **The fixture.** Find where the prosign reaches the transcript through
  `CwProbabilisticDecoder` and `CwCharacter`. Check the synthetic `prosigns-18wpm` fixture
  (named at `CwProbabilisticDecoder.cs:1137`) and its exact key.
- **Tests already in the tree.** Find any existing test on prosigns and state which requirement
  it names, if any. Do not re-point it. That is step 8's work (R80).

**Expected failures, not regressions:**
- the three red named floors, at the values above;
- the app carry-forward line losing a few tests to Avalonia's headless "dispatcher loop" on a
  first run and passing on one rerun, as units 451 to 454 saw.

---

## 4. Rulings in force - transcribed, do not re-argue

- **R72.** No word, dictionary or callsign prior, in any form (HM-REQ-004, HM-DEC-175). A
  prosign is recognized from its run of elements alone, never from the words around it.
- **R77.** The requirements are the specification, and a CW test names the requirement it
  proves.
- **R78.** A change is kept when it moves a requirement's metric the right way and breaks no
  other requirement:
  - MET-INVENTED at zero;
  - MET-CER-SURE below 1%;
  - MET-COVERAGE at or above 90%;
  - MET-WBE at or below 5%.

  These apply per condition, at the tier the requirement names. The capture floors stay as
  V-11's overfitting guard: no change may redden an earlier capture to green a newer one.
- **R80.** No unit is authored for bookkeeping. A test is written only where a requirement this
  phase is meeting needs one to be judged.
- **R83**, as recorded in work instruction 441: prefer the change the operator reads.
- **R85.** A CW question is answered from the documents, recorded as the reading, and never
  parked for the owner.
- **443 DECIDED (3), as 448 DECIDED (6) read it.** No floor is re-banked while 17:37's boundaries
  are worse. 2.5 and 6.6 are not ticked this unit.
- **`CLAUDE.md` §0.0.** Never present a guess as a decode. **§0.2:** nothing that keys or
  transmits. **§12.5:** a fixture built from the same misunderstanding as the code proves
  nothing. The prosign fixture's key comes from `CW_SPEC.md` §6.2's patterns, not from
  `MorseAlphabet`.
- **HM-DEC-155, HM-DEC-165, FACT-004.**

---

## 5. Status cadence

Post one line at the start of each task, naming the task and what it is about to measure. Post
one line at each commit, with its hash and the four real metrics. Post one line if a type runs
past its timeout. Nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 455 - STEP 6` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 455 with `CURRENT_STEP: 6`, in both copies.
3. Bump the patch: 1.13.141 to 1.13.142.
4. Commit the runner's uncommitted writes as they are, on 453's and 454's precedent.
5. Run the entry round: build, both carry-forward lines, the three floor tests, and the four
   metrics at HEAD, real and synthetic.
6. Save every recording's text to `.run-unit\unit455-text-before.txt`.

### Task 1 - the trace

This task measures before anything is built. It changes nothing in `src`. For every prosign in
`CW_SPEC.md` §6.2, print:
- its pattern;
- what `MorseAlphabet` returns for that pattern;
- what the decoder emits for it on the synthetic `prosigns-18wpm` fixture at HEAD;
- whether the output is one symbol or split into letters.

The prosigns are `VE`, `CT`, `SK`, `AS`, `KN`, `BK`, `CL`, `BT`/`=` and `AR`/`+`. The error
signal is out of scope: it is HM-REQ-073, not this criterion.

Then print every prosign the real keyed recordings' keys carry, with what the decoder emitted at
that span.

**Where a prosign is not in the table at all**, report it as a 6.1 finding and do not add it.
6.1 wants the table generated from cited data, not a hand-added constant.

### Task 2 - the two tests (6.2 is ticked here)

- **`TheProsignArrivesAsOneSymbolTests`, naming HM-REQ-071.**
  - Build a synthetic send, key exact by construction, of each prosign in the table, sent as
    one run with no character gap.
  - Assert that each emits as one symbol and is never split into letters.
  - Add one case with a real character gap inside (`A` then `R`), and assert it emits two
    letters, so the test cannot pass by merging everything.
- **`TheTwoNamedPatternIsNamedAsTheTerminalIsSetTests`, naming HM-REQ-072.**
  - For `-...-` and `.-.-.`, assert one symbol per send.
  - Assert that the name it carries follows the terminal's setting both ways.
- **Watch each fail first.** Where HEAD already meets a requirement, show the failure on a
  deliberately wrong expectation, then correct the expectation. Record both runs.
- **State each requirement met or not met at HEAD**, with the emitted text beside it. A red test
  is committed red on no carry-forward line. Its name and its requirement id go in the report.

**6.2 is ticked when both tests exist and name their requirements, and the report states whether
each requirement is met.** Tick it in both copies of `PHASE_PLAN.md`.

### Task 3 - meet what is not met

Do this only for a requirement task 2 found not met. Make one change per requirement, each in
its own commit.

- **072, if no setting exists.** Add a terminal setting that chooses the name of the
  two-named patterns:
  - it has two values, prosign name and punctuation name;
  - the default is today's prosign name, so nothing the operator reads changes until they
    change it;
  - the decoder emits one symbol, and the name is applied where the transcript is rendered.
- **071, if a run is split.** Make one change in how the decoder reads a run with no character
  gap, from the audio alone (R72).
- **Judge each change under R78.** Report, real and synthetic, with the key's kind:
  - MET-CER-SURE, MET-INVENTED, sure-and-right coverage and MET-WBE, before and after;
  - V-11 on every capture;
  - every recording's text diffed against task 0's save.

  **A 072 setting that leaves the default rendering byte-identical passes on that identity.**

**Drop candidate: this whole task.** If the unit runs long, stop after task 2. 6.2 is already
ticked there, and the not-met requirement is stated as the unit's product.

### Task 4 - the exit round

Run, and report every figure beside its entry figure:
- both carry-forward lines;
- the three floor tests;
- the four metrics;
- every type touched;
- the two new tests.

Also print:
- `git diff` of the transmit files against `7e209cb4`, which prints nothing;
- the diff of every recording's text against task 0's save.

---

## 7. Parked - do not touch, do not raise

- **2.2's transmit-file list** (keying).
- **4.5's definition of acquiring** (promise).
- **4.6's proved pitch** and **5.5's proved speed** (promise).
- **The fldigi source request** (unit 454). It is the owner's to act on, and this unit neither
  repeats it nor routes around the refused clone.
- **6.5's tick and HM-REQ-084's `ABOVE`** on 013637, both waiting on the owner.

## 8. Do not

- Do not re-bank any floor, and do not tick 2.5 or 6.6 (443 DECIDED (3)).
- Do not add a prosign to the table by hand. That is 6.1's work, from cited data.
- Do not touch HM-REQ-073's error signal, or delete anything on it.
- Do not re-point or retire an existing test. That is step 8's work (R80).
- Do not use a word or context to decide that a run is a prosign (R72).
- Do not raise a CW question to the owner (R85).
- Do not touch `src\Hamlet.RadioEngine\Cw\Second\`, fldigi, or step 9.
- **Do not run an unfiltered `dotnet test`. Never run in the background and poll. Never compose
  a timestamp.**

## 9. Committing and pushing

Every commit follows the same rules:
- **One commit per task,** and one per kept change in task 3.
- **Message form:** `unit455 task N: <what> (6.2)`.
- **Exit state:** the build and the named types are green at the exit of every commit, except
  the three floors held red as stated.
- **A refused change is committed** under `.run-unit\` as its diff and numbers. It never goes in
  `src`.
- **Push each commit** to `origin/main`.

## 10. Report

Write `output.md` at the root with the four headings exactly. **The ordering block comes
first.** `validate-output.bat` refuses a report without it.

```
READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 3 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 <n> of 6,
   7 0 of 5, 8 0 of 6, 9 0 of 8 (blocked on fldigi's source).
B. Step 6, criterion 6.2: HM-REQ-071 <met|not met> and HM-REQ-072
   <met|not met>, the test that names each, and whether 6.2 is ticked;
   6.1, 6.4 and 6.6 as they stand.
C. The findings weighed against A and B: how many items section 4 raises,
   and whether any is in the way of a criterion in B.
```

```
UNIT:       455 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <the criterion and why>
NUMBER:     HM-REQ-071 <met|not met> on <n> of <n> prosigns; HM-REQ-072 <met|not met>; MET-CER-SURE real <n> of <n>, synthetic <n> of <n>
DRIFT:      step 2 <n>; step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 9 <n>
```

**Section 3 leads with the prosign table from task 1:** every prosign, its pattern and what
the operator reads at HEAD, and after task 3 if it ran. **Section 2, one paragraph:** what the
operator sees when a station signs off with `AR` or `SK` or sends `BT` between paragraphs, and
whether they can choose to see `=` instead.

---

```
ARBITER-DECISION
STEP: 6
APPROACH: test naming HM-REQ-071 prosign one symbol and HM-REQ-072 prosign naming per terminal setting, watched failing first on synthetic keyed prosigns, state met or not, one change to emit a run with no character gap as one prosign symbol kept under R78
MOVE: work around
WHY: PHASE_PLAN.md step 6 line 6.2 asks that HM-REQ-071 and 072 each have a test naming them with the report stating whether each is met, and a prosign split into letters is a wrong letter the operator reads; step 2 cannot flip a line this pass (2.4 at 0 of 3 after 449's kept change, 2.5 held by 443's DECIDED (3)) and step 9 waits on fldigi's source, which is still absent.
STATE: partial
DECIDED: author's, overrulable - (1) step 6 is worked instead of the launcher's step 2 for the reason in WHY, and step 9 is not authored while .run-unit/fldigi is absent, the source request being 454's section 4 and logged, not chased; (2) 6.2 is ticked at task 2 on two tests naming HM-REQ-071 and 072, each watched failing first, with met or not met stated, whether or not task 3 runs; (3) under R85, HM-REQ-072's terminal setting, if absent, is built with the prosign name as default so the default rendering is byte-identical, and naming is not a claim about the signal, so this is not the promise stop; (4) a prosign missing from the table is a 6.1 finding and is not hand-added; (5) 443's DECIDED (3) holds, and 2.5 and 6.6 are not ticked; (6) 454's section 4 is logged and not chased.
LICENCE: PHASE_PLAN.md step 6 line 6.2 and section 5's independence line; CW_REQUIREMENTS.md HM-REQ-071, 072, HM-REQ-004; CW_SPEC.md 6.2; R72, R77, R78, R80, R85, section 6; R83 as recorded in work instruction 441; arbiter rulings 443 DECIDED (3), 448 DECIDED (6); V-11; HM-DEC-048; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: a prosign sent as one run reaches the operator as one symbol and not as stray letters, named the way the operator chose, with two tests that say so by requirement id
ADVANCES: step 6 criterion 2
END-ARBITER-DECISION
```
