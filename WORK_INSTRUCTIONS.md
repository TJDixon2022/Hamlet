# Work instruction 457 - why fldigi's port loses the first dit, and a case chosen before it is read

**Loop unit.** Step 9, criterion 9.1 (HM-REQ-122). Unit 456 built `FldigiCwDecoder` under
`src\Hamlet.RadioEngine\Cw\Second\`:
- six ported files plus the class itself;
- headers present, 7 of 7;
- 34 functions ported and 54 left out.

On its one synthetic case it emitted `GARIS CQ ` where `PARIS CQ` was keyed. It dropped the first
dit of `P`, the first element after 10 s of noise.

**9.1 is open on one clause only:** *"it decodes one synthetic case whose key is exact."* This
unit settles that clause in the only order that is not tuning toward the key:
1. prove whether the lost dit is the port's defect or fldigi's own behaviour;
2. repair the port toward upstream if it is the port's;
3. choose the case from that evidence and from the real recordings, and write the choice down
   before the port reads it.

Five tasks. Drop from the back.

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
`.run-unit\unit457-<name>.sh` and are run with `sh`.

**No C++ compiler is on this machine's path** (the arbiter checked: no `g++`, `gcc`, `clang`,
`cl` or `cmake`). Upstream cannot be built and run beside the port, and installing a toolchain
is a package, which is `MOVE: stop` in the plan's section 6. **Fidelity is shown by reading,
line against line.**

**The four report headings, exactly:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`. Write the `UNIT:` line without
brackets. Write `ADVANCES: step 9 criterion 1`. WHY cites the plan.

**A CW question is answered from the documents and from fldigi's source, never raised to the
owner** (R85). Section 4 records the reading in one line, and the work goes on.

---

## 2. Why this unit exists

**The count today.**

| step | met |
|---|---|
| 0 | done |
| 1 | done |
| 2 | 3 of 5 |
| 3 | 3 of 6 |
| 4 | 5 of 7 |
| 5 | 1 of 6 |
| 6 | 3 of 6 |
| 7 | 0 of 5 |
| 8 | 0 of 6 |
| 9 | 0 of 8 |

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  The port's lost first dit is explained from fldigi's own source
            line by line - a port defect repaired toward upstream, or upstream
            behaviour named - and one synthetic case with an exact key, chosen
            from that evidence and the real recordings before the port reads
            it, is read by the port as keyed.
ADVANCES:   step 9 criterion 1
```

**Read `CW_REQUIREMENTS.md` section M and V-04 first. They win over this instruction.**

- **HM-REQ-122 (must):** *"The second decoder shall be a faithful port of its upstream source,
  receive path only, with its license, authors and upstream commit kept in the file, and no word,
  dictionary or callsign logic (HM-REQ-004)."*
- **HM-REQ-129 (must):** the second decoder is left as ported. It is not edited to match the key.
- **V-04:** *"A fixture the reference decoder cannot read is a generator defect; the control for
  the generator is the real recording. Lowering the gate to admit a fixture is forbidden."*

**Why step 9 and not the launcher's step 2.**
- `PHASE_PLAN.md` step 9 is *"preferred over steps 2 to 7 until 9.2 is met"*, and section 5 says
  the same.
- 9.1 is the first line of step 9, and every later line needs a port that is shown to read.
- Step 2 cannot flip a line this pass. 2.4 counts units with no kept change, and 449's kept
  change reset that count below three. 2.5 is held red by 443's DECIDED (3).

**Why this is not 456 again.** 456 built the port. That approach is recorded `no` against 9.1,
and it is not repeated: **the port is not rebuilt.** This unit does four things 456 did not:
- a line-by-line fidelity audit of the path the first element takes;
- a sample-level trace of that element through the port's state;
- a verdict on whether upstream would lose it too;
- a case construction fixed in writing before the port reads it.

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1, and carry on as far as the tree allows.

- **HEAD** is `f20adf82` or a runner commit on top of it.
- **`src\Hamlet.RadioEngine\Cw\Second\`** holds `FldigiCwDecoder` and its supporting files, each
  headed with GPL-3, the authors and `61b97f41`.
- **`TheSecondDecoderIsAFaithfulPortTests`** exists and fails on the exact case. Its emitted text
  is `GARIS CQ `.
- **`.run-unit\fldigi\`** is untracked. Per 456 it is a partial working tree: `src\cw_rtty`,
  `src\filters`, `src\include` and a few more are present, and `src\misc\status.cxx` is absent.
  - Name what is present.
  - Do not fetch, and do not read the object store if that was refused before. A refused call is
    a denial, recorded and not worked around.
- **The runner's uncommitted writes:** `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md`,
  the deleted `STOP`, and three new files under `.run-unit\reports\`. They are the runner's.
  Commit them as they are, on 453's to 456's precedent.
- **The reload's `RULES_AT` disagreement** (HM-DEC-165 against CPS-DEC-0183) is logged. It is
  not this unit's.

**Expected failures, not regressions:**
- the three red named floors, at 17:37 38 of 46, 032113 43 of 45 and 032129 42 of 64;
- `TheSecondDecoderIsAFaithfulPortTests` red, until task 3;
- the app carry-forward line losing a few tests to Avalonia's headless "dispatcher loop" on a
  first run and passing on one rerun, as units 451 to 456 saw.

---

## 4. Rulings in force - transcribed, do not re-argue

- **R84, and section M of `CW_REQUIREMENTS.md` v1.1.** Two decoders read the same audio.
  fldigi's CW receive modem (`src/cw_rtty/cw.cxx`, GPL-3, W1HKJ and AG1LE) is ported faithfully
  as the second decoder.
  - Rejected: the port as a bench instrument only.
  - Rejected: replacing ours with the port.
  - **The order is fixed:** scored beside ours (123) before it is calibrated (124), and only then
    does it vote (125 to 128).
- **HM-REQ-122 and 129.** Faithful, not improved. A port that differs from upstream is repaired
  toward upstream, never toward the key. The second decoder stays as ported.
- **V-04.** A fixture the reference decoder cannot read is a generator defect. The real
  recording is the generator's control. No gate is lowered to admit a fixture.
- **V-06.** A synthetic case carries a shaped noise band, never digital silence.
- **V-14.** No bound is loosened to pass a fixture.
- **R72.** No word, dictionary or callsign prior, in any form (HM-REQ-004, HM-DEC-175).
- **R77.** A CW test names the requirement it proves.
- **R78.** A change to our decoder is kept on the requirements' metrics: MET-INVENTED at zero,
  MET-CER-SURE below 1%, MET-COVERAGE at or above 90%, MET-WBE at or below 5%. V-11 is the
  overfitting guard. **This unit changes nothing in our decoder**, so every metric and every
  recording's text must be byte-identical at exit.
- **R80.** No unit is authored for bookkeeping.
- **R85.** A CW question is answered from the documents and the second decoder's source, and
  never parked for the owner.
- **443 DECIDED (3), as 448 DECIDED (6) read it.** No floor is re-banked while 17:37's
  boundaries are worse. 2.5, 6.6 and 9.8 are not ticked this unit.
- **456 DECIDED (2) to (5).**
  - The port uses fldigi's shipped `progdefaults` at `61b97f41` and its default detection path.
  - The caller supplies the carrier frequency.
  - Resampling sits outside the ported files.
  - The output stays unclassed until 9.2.
  - `.run-unit\fldigi\` stays untracked and unmodified.
  - The port is wired to nothing the operator sees.
- **`CLAUDE.md` §0.0.** Never present a guess as a decode.
- **`CLAUDE.md` §0.2.** Nothing that keys or transmits. No transmit, keying or QSK code of fldigi
  is ported.
- **`CLAUDE.md` §12.5.** The case's key comes from `CW_SPEC.md`'s patterns, not from fldigi's
  `morse.cxx` and not from `MorseAlphabet`.
- **HM-DEC-155, HM-DEC-165, FACT-004.**

---

## 5. Status cadence

Post one line at the start of each task, naming the task and what it is about to measure or
build. Post one line at each commit, with its hash. Post one line if a type runs past its
timeout. Nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 457 - STEP 9` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 457 with `CURRENT_STEP: 9`, in both copies.
3. Bump the patch: 1.13.143 to 1.13.144.
4. Commit the runner's uncommitted writes as they are. Do not stage `.run-unit\fldigi\`. Check
   `git status` before each commit.
5. Run the entry round:
   - build;
   - both carry-forward lines;
   - the three floor tests;
   - the four metrics at HEAD, real and synthetic;
   - `TheSecondDecoderIsAFaithfulPortTests`, printing what it emits.
6. Save every recording's text from our decoder to `.run-unit\unit457-text-before.txt`.

### Task 1 - the audit: the first element's path, port against upstream, line by line

This task only reads. It changes nothing in `src`.

Take every line the first element touches, from the sample entering `rx_process` to the
character leaving `decode_stream`:
- `rx_init`, `init`, `reset_rx_filter` and the constructor's receive half, which set the state
  the element meets;
- `rx_FFTprocess`, the filter and the moving average;
- in `decode_stream`, the envelope, the AGC peak and its attack and decay, and the thresholds
  written at cw.cxx:640-641;
- `handle_event`, `usec_diff`, `update_tracking` and `sync_parameters`.

For each line, set the upstream text (file:line at `61b97f41`) beside the port's line
(file:line), and mark it:
- **same**;
- **departure listed**, citing 456's departure number; or
- **differs**, stating how.

Give particular care to four things:
- every initial value the state starts from, including `agc_peak`, `modem::metric` (456's
  departure 10), `cw_receive_state`, `space_sent` and `last_element`;
- the attack and decay mapping (`cwrx_attack 1 = 200`, `cwrx_decay 1 = 1000`). Show the table
  or switch upstream maps them through;
- integer versus floating division, and the order of updates inside a sample;
- the decimation factor and the rate the thresholds are timed in.

Write it to `.run-unit\unit457-fidelity.txt`.

**If a line differs, repair the port toward upstream in its own commit**, with the upstream line
cited in the message. This is HM-REQ-122's own repair, not tuning. Then re-run the existing case
once and print what it emits.

- **No repair is made to get the key's text.**
- A repair must be justified by the upstream line alone, and the report shows that line.

### Task 2 - the trace: the first dit, sample by sample

Using the port as it stands after task 1, print the port's state through the case's first two
seconds of keying, from 0.5 s before `P`'s first key-down to the end of `A`, at the decimated
rate. The columns are:
- time;
- the filtered magnitude;
- `agc_peak`;
- `CWupper` and `CWlower`;
- `cw_receive_state`;
- every event raised, with `usec_diff`;
- `two_dots` and the element classed.

Print the same for the last second of the noise lead-in. The question is whether the detector
is key-down on noise when the dit arrives, or whether the AGC has not risen far enough to cross
`CWupper` during the dit.

Then **state one verdict, from the upstream source lines**:
- **(a) port defect.** Task 1 missed a line. Name it, repair it toward upstream in its own
  commit, and re-run the existing case once.
- **(b) upstream behaviour.** Name the upstream lines that make fldigi lose that element. State
  what fldigi needs before a first element to read it: a settled AGC, a quiet interval, or a
  level relation, in its own time constants.

Write it to `.run-unit\unit457-first-dit.txt`, and put the verdict in section 3.

### Task 3 - the case, chosen before it is read, and 9.1 (ticked here)

**First, write the construction down and commit it, before the port is run on the new case.**
Put it in `.run-unit\unit457-case.txt`, committed in its own commit, which comes before the
commit that runs the case. It states:

- **The control.** Look at the real keyed recordings that 456's first look ran, and at least
  three more. For each, state what the audio holds in the 5 s before the first keyed element of
  the key: noise only, another station, a carrier, or the same operator's earlier keying, with
  its level against the first element. V-04 makes the real recording the generator's control.
- **The rule.** One construction, derived from task 2's verdict and the control.
  - If the verdict is (b), the construction gives fldigi what task 2 says it needs, in fldigi's
    own time constants, **only as far as the real recordings show that condition occurs on the
    air**.
  - If the verdict is (a) and the repaired port reads 456's case, 456's case stands unchanged and
    this file says so.
- **The recipe:**
  - the send, from `CW_SPEC.md`'s patterns, with plain letters and one word space;
  - speed, pitch and edges;
  - lead-in, tail, noise shape, seed and SNR at the `CW_SPEC.md` 8.1 reference.

  Enough that another unit can rebuild it.

**Then run it once.** Update `TheSecondDecoderIsAFaithfulPortTests`, naming HM-REQ-122, to the
case in the file.
- Its expected text is the key, spaced by fldigi's own rules.
- **Watch it fail first.** Use a deliberately wrong expectation, then correct it. Record both
  runs.
- The header assertions stay.

**If the port does not read it, 9.1 stays open.** Print what it emitted, with the task 2 columns
at each character.

**Do not:**
- try a second seed, SNR, lead-in or level;
- key an extra element or a preamble in front of the scored text;
- move the scored span;
- map a garbled character into the expectation.

Each of those is tuning toward the key (HM-REQ-122, 129, V-04, V-14, §0.0).

**9.1 is ticked when all of these hold:**
- the port exists under `Cw\Second\` with its headers;
- `.run-unit\unit457-case.txt` was committed before the run;
- the test is green on that case;
- the report names every function ported and every one left out, from 456's list and task 1's
  repairs.

Tick it in both copies of `PHASE_PLAN.md`.

### Task 4 - the first look again (drop candidate)

Only if task 1 or 2 repaired the port:
- re-run 456's first look, on the same synthetic cases and the same three captures, with the
  pitch instrument's pitch;
- print the port's text before and after the repair, beside the key, one block per recording.

No scoring and no table. That is 9.2's. Commit the printout under `.run-unit\`.

**This is the drop candidate.** If no repair was made, it is skipped and the report says so.

### Task 5 - the exit round

Run each of the following, and report every figure beside its entry figure:
- both carry-forward lines;
- the three floor tests;
- the four metrics, real and synthetic;
- `TheSecondDecoderIsAFaithfulPortTests`.

Also print:
- `git diff 7e209cb4` over the eleven transmit files, which prints nothing;
- the diff of our decoder's text against task 0's save, which prints nothing;
- `git status`, showing `.run-unit\fldigi\` still untracked.

---

## 7. Parked - do not touch, do not raise

- **2.2's transmit-file list** (keying).
- **4.5's definition of acquiring** (promise).
- **4.6's proved pitch** and **5.5's proved speed** (promise).
- **6.5's tick and HM-REQ-084's `ABOVE`** on 013637.
- **Unit 455's two 6.1 findings:** KN's pattern is also `(`, and a missing prosign is dropped at
  the end of a send.
- **fldigi's squelch default in `status.cxx`.** It is not in the tree. 456's reading from
  benchmark.cxx:54 stands, and the squelch stays off. It is not re-opened unless task 2's
  verdict turns on it, and then only from lines that are in the tree.
- **456's finding for 9.2:** fldigi turns a 5-unit character gap into a word space. That is
  9.2's mapping, and is logged.

## 8. Do not

- Do not change `CwProbabilisticDecoder`, `CwProbabilisticStream`, `MorseAlphabet`, the tracker
  or the scorer. This unit touches our decoder nowhere.
- Do not rebuild the port. Repair it only where a cited upstream line differs.
- Do not tune, improve or re-threshold the port, and do not give it a confidence (HM-REQ-122,
  124).
- Do not choose the case by running the port on candidates. Write it first, run it once
  (task 3).
- Do not wire the port to the CW tab, the sheet or any live path (HM-REQ-121, 9.6).
- Do not port anything that keys or transmits (§0.2).
- Do not install a compiler or any package. That is `MOVE: stop` in the plan's section 6.
- Do not stage, commit or modify `.run-unit\fldigi\`. Do not fetch from the network.
- Do not re-bank any floor. Do not tick 2.5, 6.6 or 9.8 (443 DECIDED (3)).
- Do not write `parity.md`. That file is 9.2's.
- Do not re-point or retire an existing test (R80).
- Do not raise a CW question to the owner (R85).
- **Do not run an unfiltered `dotnet test`. Never run in the background and poll. Never compose
  a timestamp.**

## 9. Committing and pushing

- **One commit per task.** Task 1 and task 2 each take one extra commit per repair. Task 3 takes
  two commits: the case file first, then the test.
- **Message form:** `unit457 task N: <what> (9.1)`.
- **Exit state:** the build and the named types are green at the exit of every commit, except the
  three floors held red as stated, and the port's test until task 3.
- **Push each commit** to `origin/main`.

## 10. Report

Write `output.md` at the root with the four headings exactly. **The ordering block comes
first.** `validate-output.bat` refuses a report without it.

```
READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 3 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 0 of 5, 8 0 of 6, 9 <n> of 8.
B. Step 9, criterion 9.1: HM-REQ-122 - the lost first dit is <a port
   defect at <upstream line>, repaired | fldigi's own behaviour at
   <upstream lines>>; the case <456's unchanged | per unit457-case.txt>,
   committed before its run at <hash>, key <text>, emitted <text>; 9.1
   <ticked | open>; 9.2 to 9.8 as they stand.
C. The findings weighed against A and B: how many items section 4 raises,
   and whether any is in the way of 9.2.
```

```
UNIT:       457 - <complete|stopped> at task N of 5, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <the criterion and why>
NUMBER:     HM-REQ-122 <met|not met>; lines audited <n>, differing <n>, repaired <n>; first-dit verdict <a|b>; case key <text> emitted <text>; our decoder's text byte-identical <yes|no>
DRIFT:      step 2 <n>; step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 9 <n>
```

**Section 3 leads with task 2's verdict and its upstream lines.** Then give:
1. the first dit's trace rows around the key-down;
2. task 1's count of lines by mark, and every line marked **differs** in full;
3. the control table from task 3, with the construction and its commit hash;
4. what the port emitted.

**Section 2, one paragraph:** nothing the operator sees changes. The owner learns whether
Hamlet's copy of fldigi misreads the first letter because the copy is wrong, or because fldigi
itself does.

---

```
ARBITER-DECISION
STEP: 9
APPROACH: audit FldigiCwDecoder against upstream cw.cxx line by line on the first element's path and trace the lost first dit sample by sample to a port-defect or upstream-behaviour verdict, repair toward upstream only, then fix the synthetic case construction in a committed file from that verdict and the real recordings as control before running it once
MOVE: work around
WHY: PHASE_PLAN.md step 9 is preferred over steps 2 to 7 until 9.2 is met, and 9.1 (HM-REQ-122) is open on its one clause "it decodes one synthetic case whose key is exact" after 456's port emitted GARIS CQ for PARIS CQ; 456's approach, building the port, is recorded no and is not repeated, and this unit instead settles whether the loss is the port's or fldigi's before any case is chosen. Step 2 cannot flip a line this pass, since 2.4's count is below three after 449's kept change and 2.5 is held by 443's DECIDED (3).
STATE: in progress
DECIDED: author's, overrulable - (1) step 9 is worked instead of the launcher's step 2, on the plan's step 9 preference line and section 5; (2) with no C++ compiler on the path and a toolchain being a package stop, fidelity is shown by line-by-line reading and not by building upstream; (3) a port line that differs from upstream is repaired toward upstream in its own commit on the cited upstream line alone, which is HM-REQ-122's own requirement and not tuning; (4) under V-04 the case is chosen from the first-dit verdict and the real recordings as the generator's control, written and committed before the port reads it, and run once - no second seed, SNR, lead-in or level, no preamble, no moved span; (5) 456's DECIDED (2) to (5) and 443's DECIDED (3) hold, and 2.5, 6.6 and 9.8 are not ticked; (6) 456's section 4 is applied as its item 1 reads and otherwise logged: the squelch default stays off per benchmark.cxx:54, and the gap-to-space finding is 9.2's.
LICENCE: PHASE_PLAN.md step 9 line 9.1, its preference line and section 5 and section 6; CW_REQUIREMENTS.md section M, HM-REQ-122, 129, HM-REQ-004, V-04, V-06, V-14; R72, R77, R78, R80, R84, R85; arbiter rulings 443 DECIDED (3), 448 DECIDED (6), 456 DECIDED (2) to (5); V-11; HM-DEC-155; HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5; GPL-3
ACCOMPLISHED: Hamlet's copy of fldigi is shown to be faithful where the first letter is read, the owner learns whether a lost first letter is Hamlet's copying or fldigi's own habit, and the copy reads one known send chosen from what real recordings look like, so it can be scored beside Hamlet's own decoder next
ADVANCES: step 9 criterion 1
END-ARBITER-DECISION
```
