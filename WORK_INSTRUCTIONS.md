# Work instruction 456 - fldigi's CW receiver, ported as the second decoder

**Loop unit.** Step 9, criterion 9.1 (HM-REQ-122). The owner has now placed fldigi's source at
`.run-unit\fldigi\`, cloned from `https://github.com/w1hkj/fldigi.git` at upstream commit
`61b97f4133c488063f3de1795c894d22d5032e8a`. **The output is `FldigiCwDecoder`:**
- a faithful C# port of fldigi's CW receive path, and nothing else of fldigi;
- under `src\Hamlet.RadioEngine\Cw\Second\`;
- carrying fldigi's GPL-3 notice, its authors and that commit;
- decoding one synthetic case whose key is exact;
- with a report that lists every upstream function ported and every one left out.

It is not scored against ours, and it is not wired to anything the operator sees. Both of those
come later in step 9. Five tasks. Drop from the back.

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
`.run-unit\unit456-<name>.sh` and are run with `sh`.

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
UNIT GOAL:  A second CW decoder exists in the tree - fldigi's receive path,
            ported line for line with its license, authors and upstream
            commit - and it reads one synthetic send whose key is exact.
ADVANCES:   step 9 criterion 1
```

**Read `CW_REQUIREMENTS.md` section M first. It wins over this instruction.**

- **HM-REQ-122 (must):** *"The second decoder shall be a faithful port of its upstream source,
  receive path only, with its license, authors and upstream commit kept in the file, and no word,
  dictionary or callsign logic (HM-REQ-004)."*
- **HM-REQ-129 (must):** the second decoder is left as ported. It is the teacher, and the teacher
  is not edited to match the student.

**Why step 9 and not the launcher's step 2.** `PHASE_PLAN.md` step 9 says: *"Preferred over steps
2 to 7 until 9.2 is met, because every later change to ours is better aimed with the comparison
in hand."* Section 5 says the same.

- **What blocked step 9 is gone.** Units 454 and 455 were stopped or routed away because
  `.run-unit\fldigi\` was absent. It is present now, as a full clone whose `.git` records the
  commit above.
- **9.1 comes first.** Every other line of step 9 needs the port.
- **Step 2 cannot flip a line this pass.**
  - 2.4 counts units with no kept change, and the count is not at three.
  - 2.5 is held red by 443's DECIDED (3), with the floors at 38 of 46, 43 of 45 and 42 of 64.

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1, and carry on as far as the tree allows.

- **The clone.**
  - Confirm that `.run-unit\fldigi\.git` records `61b97f41...` as `master` from
    `https://github.com/w1hkj/fldigi.git`.
  - Confirm that `src\cw_rtty\cw.cxx`, `src\cw_rtty\morse.cxx`, `src\include\cw.h`,
    `src\include\morse.h`, `src\filters\` and `src\include\configuration.h` are present.
  - If the checkout is partial, name what is missing. Do not fetch anything. A refused network
    call is a denial and is recorded, not worked around.
- **`src\Hamlet.RadioEngine\Cw\Second\`.** Expected: absent, or empty.
- **The sample rate.** fldigi's CW modem runs at a fixed rate that its source names. The corpus
  runs at the rates `CwProbabilisticDecoder` reads. State both.
- **The defaults.** fldigi's receive path reads `progdefaults` fields for speed, bandwidth,
  tracking, range, the matched filter, lower and upper thresholds, and `CW_noise`. Find each
  field's shipped default in `configuration.h` at this commit.

**Expected failures, not regressions:**
- the three red named floors, at the values above;
- the app carry-forward line losing a few tests to Avalonia's headless "dispatcher loop" on a
  first run, and passing on one rerun, as units 451 to 455 saw.

---

## 4. Rulings in force - transcribed, do not re-argue

- **R84, and section M of `CW_REQUIREMENTS.md` v1.1.** Two decoders read the same audio.
  fldigi's CW receive modem (`src/cw_rtty/cw.cxx`, GPL-3, W1HKJ and AG1LE) is ported faithfully
  as the second decoder.
  - Rejected: the port as a bench instrument only.
  - Rejected: replacing ours with the port.
  - **The order is fixed:** the port is scored beside ours (123) before it is calibrated (124),
    and only then does it vote (125 to 128).
- **HM-REQ-122 and 129.** Faithful, not improved. A technique goes into ours, judged under R78.
  The second decoder stays as ported.
- **R72.** No word, dictionary or callsign prior, in any form (HM-REQ-004, HM-DEC-175). fldigi's
  CW receiver has none that this instruction knows of. If the trace finds any, it is left out
  and named.
- **R77.** A CW test names the requirement it proves.
- **R78.** A change to our decoder is kept on the requirements' metrics:
  - MET-INVENTED at zero;
  - MET-CER-SURE below 1%;
  - MET-COVERAGE at or above 90%;
  - MET-WBE at or below 5%.

  V-11 is the overfitting guard. **This unit changes nothing in our decoder**, so every metric
  and every recording's text must be byte-identical at exit.
- **R80.** No unit is authored for bookkeeping. A test is written only where a requirement this
  phase is meeting needs one to be judged.
- **R85.** A CW question is answered from the documents and the second decoder's source, and
  never parked for the owner.
- **443 DECIDED (3), as 448 DECIDED (6) read it.** No floor is re-banked while 17:37's
  boundaries are worse. 2.5, 6.6 and 9.8 are not ticked this unit.
- **`CLAUDE.md` §0.0.** Never present a guess as a decode.
- **`CLAUDE.md` §0.2.** Nothing that keys or transmits. fldigi's transmit path, its keying
  interfaces and its QSK code are never ported, not even as dead code.
- **`CLAUDE.md` §12.5.** A fixture built from the same misunderstanding as the code proves
  nothing. The synthetic case's key comes from `CW_SPEC.md`'s patterns, not from fldigi's
  `morse.cxx` and not from `MorseAlphabet`.
- **V-04, V-06, V-14.**
  - No fixture is admitted by lowering a gate.
  - A synthetic case carries a shaped noise band, never digital silence.
  - No bound is loosened to pass a fixture.
- **HM-DEC-155, HM-DEC-165, FACT-004.**

---

## 5. Status cadence

Post one line at the start of each task, naming the task and what it is about to measure or
build. Post one line at each commit, with its hash. Post one line if a type runs past its
timeout. Nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 456 - STEP 9` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 456 with `CURRENT_STEP: 9`, in both copies.
3. Bump the patch: 1.13.142 to 1.13.143.
4. Commit the runner's uncommitted writes as they are, on 453's to 455's precedent.
   - **Do not stage `.run-unit\fldigi\`.** It is a nested repository and the owner's reference
     copy, and it stays untracked.
   - Check `git status` before each commit.
5. Run the entry round:
   - build;
   - both carry-forward lines;
   - the three floor tests;
   - the four metrics at HEAD, real and synthetic.
6. Save every recording's text to `.run-unit\unit456-text-before.txt`.

### Task 1 - the trace: what fldigi's receiver is

This task reads before anything is built. It changes nothing in `src`.

Starting at `cw::rx_process`, follow every call it makes, all the way down. Include:
- `cw.cxx`;
- `morse.cxx`;
- the filters under `src\filters\`, such as `fftfilt` and the moving averages;
- any helper in `src\include\`.

Write `.run-unit\unit456-fldigi-rx.txt`. For each function in the receive call graph, give:
- its file and line at `61b97f41`;
- what it does, in one line;
- **port** or **leave out**;
- the reason for any function left out.

Expected reasons for leaving a function out:
- transmit (`tx_*`, `send_*`, `nco`, `qsknco`, `create_edges`);
- UI or display (`update_syncscope`, `update_Status`, anything `FL_` or waterfall);
- configuration persistence.

Beside that list, give every `progdefaults` field the receive path reads, with its shipped
default and where the default is defined.

**Name the decoder modes.** fldigi's receiver has more than one detection path. Name each one,
and the default one at this commit. **Port the default path.** Port another mode only if it is
the same code under a switch, and name it if you do.

### Task 2 - the port

Build `FldigiCwDecoder` under `src\Hamlet.RadioEngine\Cw\Second\`. Supporting types go beside
it, one per upstream unit they come from, for example a `FldigiFftFilter` and a `FldigiMorse`.
The port's rules:

- **The file header of every ported file carries:**
  - fldigi's GPL-3 notice, verbatim from the upstream file;
  - every author the upstream header names (Dave Freese W1HKJ, Mauri Niininen AG1LE, and the
    gmfsk authors credited there);
  - the upstream path;
  - `https://github.com/w1hkj/fldigi` at commit `61b97f4133c488063f3de1795c894d22d5032e8a`.
- **Faithful:** the same constants, the same state, the same order of operations, the same
  thresholds and the same defaults as the `progdefaults` values in task 1, in the same units.
  - Name each upstream function in a one-line comment at the method that ports it.
  - A departure forced by C++ to C# (a type, a clock, a buffer) is allowed. List each one in the
    report with its upstream line. **No departure is an improvement.**
- **Receive path only.** Samples in, characters out, with the carrier frequency given by the
  caller.
  - fldigi takes its frequency from the waterfall cursor. Here the caller supplies it, and the
    report says so.
  - Nothing in the port keys, transmits, or references `ICwSender`, `CwTransmitter`, `Transmit*`
    or a keyer.
- **No word, dictionary or callsign logic** (R72).
- **Its own output, unclassed.** fldigi has no confidence. The port emits exactly what fldigi
  would print, including its `CW_noise` character where fldigi prints one.
  - Do not map the output to sure, dim or placeholder here. That mapping is 9.2's, and it is
    stated there.
- **Not wired to anything.**
  - No change to `CwProbabilisticDecoder`, `CwProbabilisticStream`, the capture sheet, the CW
    tab or any live path.
  - It is reachable from tests only. HM-REQ-121 and 9.6 come later.
- **The sample rate.** If the corpus rate differs from fldigi's, the port takes fldigi's rate.
  - Put any resampling in a separate adapter outside the ported files.
  - State the method, and state that it is not fldigi's.
  - Use no package. A package is a `MOVE: stop` in the plan's section 6, so if one seems needed,
    stop and report it.

### Task 3 - the case, and 9.1 (ticked here)

Write `TheSecondDecoderIsAFaithfulPortTests`, naming HM-REQ-122.

- **One synthetic send, key exact by construction:**
  - The pattern comes from `CW_SPEC.md`'s table, not from fldigi's table and not from
    `MorseAlphabet` (§12.5).
  - Use plain letters and one word space, for example `PARIS CQ`.
  - Send it at fldigi's default speed from task 1, at a stated pitch.
  - Carry a shaped noise band at a stated SNR, never digital silence (V-06).
  - State the recipe, so another unit can rebuild it.
- **Assert** that `FldigiCwDecoder`, given that pitch, emits the key's text.
  - Spacing follows fldigi's own rules, and the report prints the exact string emitted.
- **Assert the header:** each ported file carries the GPL-3 notice, the named authors and the
  upstream commit.
- **Watch it fail first.** Run it against a deliberately wrong expectation, or before the port's
  decode is wired, then correct it. Record both runs.
- **If the port does not read the case,** do not tune it.
  - Print what it emitted, with fldigi's internal speed and thresholds at each character.
  - Check the harness first: rate, level, and frequency given.
  - A port that differs from upstream is repaired toward upstream, never toward the key.

**9.1 is ticked when all of these hold:**
- the port exists under `Cw\Second\` with its headers;
- the test is green on the exact case;
- the report names every function ported and every one left out, from task 1's list.

Tick it in both copies of `PHASE_PLAN.md`. If the case is not read, 9.1 stays open, and the
report says what was emitted and why.

### Task 4 - a first look (drop candidate)

Run the port over the synthetic set and over three keyed real recordings. The unit chooses the
three and names them.
- Give it the pitch that the pitch instrument measures, not our tracker's pitch, and say so.
- Print its text beside ours and beside the key, one block per recording.
- **No scoring and no table.** Scoring is 9.2's, done through the same scorer and metrics, and
  this is not a substitute for it.
- Commit the printout under `.run-unit\`.

**This is the drop candidate.** If the unit runs long, stop after task 3. 9.1 is ticked there.

### Task 5 - the exit round

Run each of the following, and report every figure beside its entry figure:
- both carry-forward lines;
- the three floor tests;
- the four metrics, real and synthetic;
- the new test.

Also print:
- `git diff 7e209cb4` over the eleven transmit files, which prints nothing;
- the diff of every recording's text against task 0's save, which prints nothing because our
  decoder is untouched;
- `git status`, showing `.run-unit\fldigi\` still untracked.

---

## 7. Parked - do not touch, do not raise

- **2.2's transmit-file list** (keying).
- **4.5's definition of acquiring** (promise).
- **4.6's proved pitch** and **5.5's proved speed** (promise).
- **6.5's tick and HM-REQ-084's `ABOVE`** on 013637, both waiting on the owner.
- **Unit 455's two 6.1 findings:**
  - KN's pattern is also `(`;
  - a missing prosign is dropped at the end of a send.

  Both are 6.1's, and are logged.

## 8. Do not

- Do not change `CwProbabilisticDecoder`, `CwProbabilisticStream`, `MorseAlphabet`, the tracker
  or the scorer. This unit touches our decoder nowhere.
- Do not improve, tune or re-threshold the port, and do not give it a confidence. That is 9.5's
  work (HM-REQ-122, 124).
- Do not wire the port to the CW tab, the sheet, or any live path (HM-REQ-121, 9.6).
- Do not port anything that keys or transmits (§0.2).
- Do not stage, commit or modify `.run-unit\fldigi\`. Do not fetch from the network.
- Do not re-bank any floor. Do not tick 2.5, 6.6 or 9.8 (443 DECIDED (3)).
- Do not write `parity.md`. That file is 9.2's.
- Do not re-point or retire an existing test. That is step 8's work (R80).
- Do not raise a CW question to the owner (R85).
- **Do not run an unfiltered `dotnet test`. Never run in the background and poll. Never compose
  a timestamp.**

## 9. Committing and pushing

- **One commit per task.** Task 2 may take more than one commit if the port is committed file by
  file.
- **Message form:** `unit456 task N: <what> (9.1)`.
- **Exit state:** the build and the named types are green at the exit of every commit, except
  the three floors held red as stated.
- **Push each commit** to `origin/main`.

## 10. Report

Write `output.md` at the root with the four headings exactly. **The ordering block comes
first.** `validate-output.bat` refuses a report without it.

```
READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 3 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 0 of 5, 8 0 of 6, 9 <n> of 8.
B. Step 9, criterion 9.1: HM-REQ-122 - FldigiCwDecoder <exists|does not>
   under Cw\Second\ at upstream 61b97f41, the synthetic case <read|not read>
   as <text emitted>, functions ported <n> and left out <n>, and whether
   9.1 is ticked; 9.2 to 9.8 as they stand.
C. The findings weighed against A and B: how many items section 4 raises,
   and whether any is in the way of 9.2.
```

```
UNIT:       456 - <complete|stopped> at task N of 5, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <the criterion and why>
NUMBER:     HM-REQ-122 <met|not met>; synthetic case key <text> emitted <text>; functions ported <n>, left out <n>; our decoder's text byte-identical <yes|no>
DRIFT:      step 2 <n>; step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 9 <n>
```

**Section 3 leads with the function table from task 1:**
- every upstream function in the receive call graph;
- its file and line;
- ported or left out, and why.

Then give the departures forced by C#, each with its upstream line, and then the synthetic case:
its recipe, its key, and what the port emitted. **Section 2, one paragraph:** nothing the
operator sees changes yet. Hamlet now carries a second, known CW reader that the next units will
score beside its own.

---

```
ARBITER-DECISION
STEP: 9
APPROACH: port fldigi cw.cxx receive path to C# as FldigiCwDecoder under Cw/Second with GPL-3 notice and upstream commit, decode one exact-key synthetic case, list functions ported and left out
MOVE: continue
WHY: PHASE_PLAN.md step 9 is preferred over steps 2 to 7 until 9.2 is met, and its first line 9.1 (HM-REQ-122) was blocked only by fldigi's source, which the owner has now placed at .run-unit/fldigi at upstream 61b97f41; every later line of step 9 needs the port, and step 2 cannot flip a line this pass (2.4 below three units with no kept change, 2.5 held by 443's DECIDED (3)).
STATE: not started
DECIDED: author's, overrulable - (1) step 9 is worked instead of the launcher's step 2, on the plan's step 9 preference line and section 5; (2) the port uses fldigi's shipped progdefaults at 61b97f41 and its default detection path, the caller supplies the carrier frequency in place of the waterfall cursor, and any resampling sits in an adapter outside the ported files; (3) the port's output stays unclassed this unit, and the mapping to sure, dim or placeholder is left to 9.2; (4) .run-unit/fldigi stays untracked and unmodified as the owner's reference copy; (5) the port is reachable from tests only and wired to nothing the operator sees, so neither keying nor the promise is touched, and the transmit path is not ported; (6) 443's DECIDED (3) holds, and 2.5, 6.6 and 9.8 are not ticked.
LICENCE: PHASE_PLAN.md step 9 line 9.1, its preference line and section 5; CW_REQUIREMENTS.md section M, HM-REQ-122, 129, HM-REQ-004; R72, R77, R78, R80, R84, R85, section 6; arbiter rulings 443 DECIDED (3), 448 DECIDED (6); V-04, V-06, V-11, V-14; HM-DEC-155; HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5; GPL-3
ACCOMPLISHED: Hamlet carries a second, known CW reader - fldigi's receiver, ported faithfully with its license and authors - that reads a known send, so the next units can score it beside Hamlet's own decoder and learn where each is better
ADVANCES: step 9 criterion 1
END-ARBITER-DECISION
```
