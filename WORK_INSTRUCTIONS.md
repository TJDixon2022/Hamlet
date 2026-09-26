# Work instruction 458 - both decoders on the same audio, the same scorer and one parity table

**Loop unit.** Step 9, criterion 9.2 (HM-REQ-123). 9.1 is ticked in the plan. `FldigiCwDecoder`
is under `src\Hamlet.RadioEngine\Cw\Second\`, and unit 457 found 177 lines on its first element's
path the same as upstream `61b97f41`, with none differing. The port reads.

**Nobody yet knows how it reads next to ours.** Section M fixes the order: before the second decoder
votes, both are scored on every keyed recording and on the synthetic set, through the same scorer
and metrics, and the result is tabled (HM-REQ-123). This unit builds that table. It changes
neither decoder.

Four working tasks, plus the exit round. Drop from the back.

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
Captures get 600 s. Never run in the background and poll. If one type needs more than 600 s, split
it by recording into several named types or filters. Never raise the timeout past 600 s.

Apostrophes in quoted heredocs break. Doubled backslashes collapse. `;` is refused. `rm` is
refused. Python cannot run here. A multi-line commit uses `-m` more than once. Scripts go in
`.run-unit\unit458-<name>.sh` and are run with `sh`.

**The four report headings, exactly:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`. Write the `UNIT:` line without
brackets. Write `ADVANCES: step 9 criterion 2`. WHY cites the plan.

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
| 9 | 1 of 8 |

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  Hamlet's decoder and the fldigi port read the same audio of every
            keyed recording and every synthetic case, their text goes through
            the same scorer, and docs/phase-requirements/parity.md tables
            MET-CER-SURE, MET-INVENTED, sure-and-right coverage and MET-WBE for
            each, per recording and per condition, with the key's kind and the
            mapping of fldigi's unclassed output stated.
ADVANCES:   step 9 criterion 2
```

**Read `CW_REQUIREMENTS.md` section M, section B and `CW_SPEC.md`'s metric definitions first.
They win over this instruction.**

- **HM-REQ-123 (must):** *"Before the second decoder votes, both decoders shall be scored on every
  keyed recording and the synthetic set through the same scorer and metrics, and the result tabled
  per recording and per condition."*
- **HM-REQ-122 and 129 (must):** the second decoder is a faithful port and is left as ported.
- **HM-REQ-124:** fldigi carries no confidence. Giving it one is 9.5's work, not this unit's.

**Why step 9, and why 9.2.**
- `PHASE_PLAN.md` step 9 is *"preferred over steps 2 to 7 until 9.2 is met"*, and section 5 says
  the same. 9.2 is now the first open line of step 9.
- 9.3 to 9.7 all read from this table, and the plan's reason for the preference is that every
  later change to ours is better aimed with the comparison in hand.
- Step 2 cannot flip a line this pass. 2.4 counts units with no kept change, and 449's kept
  change reset that count below three. 2.5 is held red by 443's DECIDED (3).

**Why this is not unit 454 again.** 454 tried to port the decoder and score it in one unit. Its
report was refused and its fate was not recorded. The port now exists and 457 audited it. **This
unit does not port, repair or audit.** It only scores.

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1, and carry on as far as the tree allows.

- **HEAD** is `845fd70f` or a runner commit on top of it.
- **`src\Hamlet.RadioEngine\Cw\Second\`** holds `FldigiCwDecoder` and seven supporting files:
  `FldigiFft`, `FldigiFftFilter`, `FldigiMisc`, `FldigiMorse`, `FldigiMovingAverage`,
  `FldigiProgdefaults` and `FldigiRateAdapter`.
- **`TheSecondDecoderIsAFaithfulPortTests`** is green, 8 of 8.
- **`TheRequirementsAreMeasuredTests`** and `tests\...\Cw\CwMetrics.cs` compute the four metrics
  for our decoder. The entry figures are 457's, as listed below.
- **`docs\phase-requirements\parity.md`** does not exist.
- **`.run-unit\fldigi\`** is untracked. Do not stage it, and do not fetch.
- **The runner's uncommitted writes** are the modified `.run-unit` state files, `PHASE_OUTCOME.md`,
  `PHASE_STATUS.md` and `RUN_LEDGER.md`, plus the untracked `.run-unit\reports\unit-4-output-6.md`
  and `.run-unit\watched.rc`. They are the runner's. Commit them as they are, following the
  precedent of units 453 to 457.
- **The reload's `RULES_AT` disagreement** (HM-DEC-165 against CPS-DEC-0183) is logged. It is
  not this unit's.

**Entry figures, 457's exit, to compare against:**
- **Real, inferred:** MET-CER-SURE 33 of 436, MET-INVENTED 33 over 473, coverage 403 over 473,
  MET-WBE 37 (29 inserted, 8 deleted) over 113.
- **Synthetic, exact:** MET-CER-SURE 14 of 173, MET-INVENTED 14 over 252, coverage 159 over 252,
  MET-WBE 44 (13 inserted, 31 deleted) over 84.

**Expected failures, not regressions:**
- the three red named floors: 17:37 at 38 of 46, 032113 at 43 of 45 and 032129 at 42 of 64;
- the app carry-forward line losing a test to Avalonia's headless "dispatcher loop" (see
  DECIDED (6)).

---

## 4. Rulings in force - transcribed, do not re-argue

- **R84, and section M of `CW_REQUIREMENTS.md` v1.1.** Two decoders read the same audio.
  fldigi's CW receive modem is ported faithfully as the second decoder.
  - Rejected: the port as a bench instrument only.
  - Rejected: replacing ours with the port.
  - **The order is fixed:** first scored beside ours (123), then calibrated (124), and only then
    does it vote (125 to 128). **This unit is 123 and nothing after it.**
- **HM-REQ-122 and 129.** Faithful, not improved. The second decoder stays as ported. No line of
  `Cw\Second\` changes behaviour in this unit.
- **R78.** A change to our decoder is kept on the requirements' metrics. **This unit changes
  nothing in our decoder**, so every metric and every recording's text is byte-identical at exit.
- **V-13.** A real recording's key is inferred unless it was transcribed. Every number carries its
  key's kind.
- **V-04 and V-14.** No fixture is altered, re-seeded or dropped because either decoder misreads
  it. No gate is lowered.
- **Step 7.4.** A real capture is never counted toward a CH-* condition.
- **R72.** No word, dictionary or callsign prior (HM-REQ-004, HM-DEC-175).
- **R77.** A CW test names the requirement it proves.
- **R80.** No unit is authored for bookkeeping. No traceability or test-inventory work.
- **R85.** A CW question is answered from the documents and the second decoder's source.
- **443 DECIDED (3), as 448 DECIDED (6) reads it.** No floor is re-banked while 17:37's
  boundaries are worse. 2.5, 6.6 and 9.8 are not ticked this unit.
- **456 DECIDED (2) to (5).**
  - The port uses fldigi's shipped `progdefaults` at `61b97f41` and its default detection path.
  - The caller supplies the carrier frequency.
  - Resampling sits outside the ported files.
  - `.run-unit\fldigi\` stays untracked and unmodified.
  - The port is wired to nothing the operator sees.
- **456 DECIDED (3) handed the mapping of the port's unclassed output to 9.2.** This instruction
  makes that mapping in DECIDED (2), and the unit states it in `parity.md`.
- **`CLAUDE.md` §0.0.** Never present a guess as a decode.
- **`CLAUDE.md` §0.2.** Nothing that keys or transmits.
- **`CLAUDE.md` §12.5.** Keys come from where the metrics already take them, never from either
  decoder's output.
- **HM-DEC-155, HM-DEC-165, FACT-004.**

---

## 5. Status cadence

Post one line at the start of each task, naming the task and what it is about to measure or
build. Post one line at each commit, with its hash. Post one line if a type runs past its
timeout. Nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 458 - STEP 9` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 458 with `CURRENT_STEP: 9`, in both copies.
3. Bump the patch: 1.13.144 to 1.13.145.
4. Commit the runner's writes as they are. Do not stage `.run-unit\fldigi\`. Check `git status`
   before each commit.
5. Run the entry round:
   - build;
   - both carry-forward lines;
   - the three floor tests;
   - `TheRequirementsAreMeasuredTests`, for the four metrics at HEAD, real and synthetic;
   - `TheSecondDecoderIsAFaithfulPortTests`.
6. Save every recording's text from our decoder to `.run-unit\unit458-text-before.txt`.

### Task 1 - the trace: how ours is scored, and where fldigi's text enters the same path

This task reads and prints. It changes nothing in `src`.

Write `.run-unit\unit458-scoring-path.txt`. It states:
- **The corpus.** List the keyed recordings `TheRequirementsAreMeasuredTests` scores (23 per the
  record) and the synthetic set (12). For each, give the audio file, the key's source and kind
  (inferred or exact), the scored span on the recording's clock, and the condition row it is
  counted under.
- **The scorer's input.** State what our decoder hands the scorer: the text, each character's
  class, and each character's time if the span rule uses it. Give file:line for each.
- **The port's output.** From `FldigiCwDecoder` and upstream `cw.cxx` at `61b97f41`, state:
  - what the port emits: characters, spaces, and what `rx_lookup` or its caller puts out for a
    code with no match, citing the upstream line;
  - whether each character carries a time.
- **The join.** State the one place where the port's text enters the same scorer call our text
  enters, and what, if anything, it lacks. If the span rule needs character times and the port
  has none, the unit may add a read-only stamp on 457's recorder precedent, marked "not fldigi's".
  That stamp records the input-sample time at which each character is put out, and it changes
  nothing the receiver reads. **Report the port's latency against ours on one synthetic case**,
  so a span edge is not mistaken for a decoding difference.
- **The pitch each decoder is given.** Ours tracks its own. The port is given the pitch
  instrument's pitch per recording (DECIDED (3)). Table both for every recording, and, on the
  synthetic set, beside the constructed pitch.

### Task 2 - the parity table, and 9.2 (ticked here)

Build one test type, **`BothDecodersAreScoredAlikeTests`**, naming HM-REQ-123. It drives the port
and our decoder over the same samples of every recording from task 1. Each decoder starts cold at
sample 0 of the file, as ours does today, with no audio added or removed. Both texts go through
the **same `CwMetrics` calls**.

**Its assertions are about the harness, never about either decoder's score:**
- **Sameness.** Our decoder's row through this harness equals `TheRequirementsAreMeasuredTests`'
  figures exactly (the entry figures in section 3). **Watch it fail first**: pass a deliberately
  wrong expected figure, see it red, then correct it. Record both runs.
- **Coverage.** Every recording in task 1's list has a row for both decoders, or a row saying why
  a decoder could not be run on it, with the error.

**It writes `docs\phase-requirements\parity.md`,** which holds:
1. **The mapping of the port's unclassed output**, in the words of DECIDED (2), with the upstream
   line for the no-match output.
2. **Per recording:** a table giving the recording, key kind, and for each decoder MET-CER-SURE
   (count over sure), MET-INVENTED (count over sent), sure-and-right coverage (count over sent) and
   MET-WBE (inserted, deleted, over reference boundaries). Put the two decoders side by side on one
   row per recording.
3. **Per condition:** the same columns summed over the rows of each condition the tree has, which
   are the real recordings with inferred keys and the synthetic set with exact keys. State that
   neither is a CH-* condition, and that no real capture is counted toward one (7.4). If the spec's
   condition names give finer rows the metrics can carry, use them.
4. **The decode time** of each decoder, per condition.
5. **What the table does not prove.** The keys on the real rows are inferred. The synthetic set is
   never sole evidence. Neither decoder has a calibrated confidence yet (9.5).

**Report the port's figures as they come.** Its first-element loss after noise (457's verdict (b)),
its 5-unit gap read as a word space (456's finding), and any character it misses are all scored,
not excused. No recording gets a preamble, and no span is moved for either decoder.

**9.2 is ticked when all of these hold:** the test is green, including sameness watched failing
first; `parity.md` is committed with all five parts; every keyed recording and every synthetic
case has a row for both decoders or a stated reason. Tick it in both copies of `PHASE_PLAN.md`.

### Task 3 - where the port reads better (drop candidate, criterion 9.3)

Take the recordings from task 2 where the port scores better than ours on any of the four metrics.
For each one:
- print both texts beside the key for the stretch where they differ, with each character's class;
- name from the port's source, file:line in `cw.cxx`, what it does differently on that stretch:
  AGC, thresholds, speed tracking, element classing or gap handling.

Also run the port on `cw-2026-09-24-135641`, which ours reads as nothing, if it is in the tree.
Print what the port reads beside `nothing read`, and name the recording. Print only, no score:
the recording has no key. Do the same for any other recording ours reads as nothing.

Write it to `.run-unit\unit458-where-it-wins.txt` and summarise it in section 3. **Tick 9.3 only
if every recording where the port scores better is covered.** Otherwise say how many are covered,
of how many.

**This is the drop candidate.** If time is short, it is shed first. Tasks 1 and 2 and the exit
round are not shed.

### Task 4 - the exit round

Run each of the following, and report every figure beside its entry figure:
- both carry-forward lines;
- the three floor tests;
- `TheRequirementsAreMeasuredTests`, real and synthetic;
- `TheSecondDecoderIsAFaithfulPortTests`;
- `BothDecodersAreScoredAlikeTests`.

Also print:
- `git diff 7e209cb4` over the eleven transmit files, which prints nothing;
- the diff of our decoder's text against task 0's save, which prints nothing;
- `git diff 845fd70f -- src/Hamlet.RadioEngine/Cw/Second/`, with every changed line marked as an
  observation that changes nothing the receiver reads, or empty;
- `git status`, showing `.run-unit\fldigi\` still untracked.

---

## 7. Parked - do not touch, do not raise

- **2.2's transmit-file list** (keying).
- **4.5's definition of acquiring** (promise).
- **4.6's proved pitch** and **5.5's proved speed** (promise).
- **6.5's tick and HM-REQ-084's `ABOVE`** on 013637.
- **Unit 455's two 6.1 findings:** KN's pattern is also `(`, and a missing prosign is dropped at
  the end of a send.
- **fldigi's squelch default in `status.cxx`**, which is not in the tree. The squelch stays off,
  per benchmark.cxx:54.
- **457's section 4 item 1, the reading on which 9.1 was ticked.** The state reader judged it not
  honestly supported. The tick stands in the plan until the owner unticks it. It is logged here
  and is not re-worked. This unit's table scores every recording from sample 0 with nothing added,
  so the port's first-element behaviour shows in the numbers.
- **457's section 4 item 2**, the six keys the control printer did not locate.

## 8. Do not

- Do not change `CwProbabilisticDecoder`, `CwProbabilisticStream`, `MorseAlphabet`, the tracker,
  `CwMetrics` or the scorer. This unit touches our decoder nowhere. If the scorer cannot take the
  port's text as it stands, report that in section 4, and table what can be tabled.
- Do not change the port's behaviour: no repair, no tuning, no re-threshold, no squelch, no
  confidence (HM-REQ-122, 124, 129). A read-only observation is the only permitted edit in
  `Cw\Second\`.
- Do not alter, re-seed, trim, pad or drop any recording or synthetic case for either decoder
  (V-04, V-14).
- Do not map a garbled port character to anything but itself, and do not map any port character
  to dim.
- Do not build calibration, voting or arbitration. Those are 9.5 to 9.7.
- Do not take a technique into our decoder. That is 9.4.
- Do not wire the port to the CW tab, the sheet or any live path (HM-REQ-121, 9.6).
- Do not install any package. That is `MOVE: stop` in the plan's section 6.
- Do not stage, commit or modify `.run-unit\fldigi\`. Do not fetch from the network.
- Do not re-bank any floor. Do not tick 2.5, 6.6 or 9.8 (443 DECIDED (3)).
- Do not re-point or retire an existing test (R80).
- Do not raise a CW question to the owner (R85).
- **Do not run an unfiltered `dotnet test`. Never run in the background and poll. Never compose
  a timestamp.**

## 9. Committing and pushing

- **One commit per task.** Task 1 may take one extra commit if it adds the read-only stamp.
  Task 2 takes two commits: the harness with the wrong sameness figure watched red, then the green
  harness and `parity.md`.
- **Message form:** `unit458 task N: <what> (9.2)`, or `(9.3)` for task 3.
- **Exit state:** the build and the named types are green at the exit of every commit, except the
  three floors held red as stated and the one watched-red commit in task 2.
- **Push each commit** to `origin/main`.

## 10. Report

Write `output.md` at the root with the four headings exactly. **The ordering block comes
first.** `validate-output.bat` refuses a report without it.

```
READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 3 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 0 of 5, 8 0 of 6, 9 <n> of 8.
B. Step 9, criterion 9.2: HM-REQ-123 - both decoders on <n> keyed
   recordings and <n> synthetic cases through the same CwMetrics calls,
   sameness watched failing first; per condition, ours against the port:
   MET-CER-SURE <a> / <b>, MET-INVENTED <a> / <b>, coverage <a> / <b>,
   MET-WBE <a> / <b>; the port's unclassed output mapped as <mapping>;
   9.2 <ticked | open>; 9.3 <ticked | n of m covered | dropped>; 9.4 to
   9.8 as they stand.
C. The findings weighed against A and B: how many items section 4 raises,
   and whether any is in the way of 9.3, 9.4 or 9.5.
```

```
UNIT:       458 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <the criterion and why>
NUMBER:     HM-REQ-123 <met|not met>; recordings scored by both <n> of <n> keyed, <n> of <n> synthetic; real ours/port MET-CER-SURE <a>/<b> coverage <a>/<b>; synthetic ours/port MET-CER-SURE <a>/<b> coverage <a>/<b>; our decoder's text byte-identical <yes|no>
DRIFT:      step 2 <n>; step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 9 <n>
```

**Section 3 leads with the per-condition table**, ours and the port side by side on the four
metrics, with key kind and decode time. Then give:
1. the per-recording table, or its first rows with the rest in `parity.md`;
2. the mapping, and how many port characters fell in each mapped class;
3. the sameness check's red and green runs;
4. task 1's latency and pitch findings;
5. task 3's recordings where the port wins, with both texts and the cited upstream lines, and
   what the port reads on 135641.

**Section 2, one paragraph:** nothing the operator sees changes. The owner learns, recording by
recording, whether fldigi's receiver reads the same air better or worse than Hamlet's, and where.

---

```
ARBITER-DECISION
STEP: 9
APPROACH: score FldigiCwDecoder and our decoder on the same audio of every keyed recording and the synthetic set through the same CwMetrics calls, sameness of our row watched failing first, map the port's unclassed characters to sure and its no-match output to placeholder, and table MET-CER-SURE MET-INVENTED coverage and MET-WBE per recording and per condition in parity.md
MOVE: continue
WHY: PHASE_PLAN.md step 9 is preferred over steps 2 to 7 until 9.2 is met, and with 9.1 ticked, 9.2 (HM-REQ-123) is step 9's first open line, which every later line of the step reads from. The port exists and 457 audited it, so this unit only scores, and it is not 454's port-and-score. Step 2 cannot flip a line this pass, since 2.4's count is below three after 449's kept change and 2.5 is held by 443's DECIDED (3).
STATE: partial
DECIDED: author's, overrulable - (1) step 9 is worked instead of the launcher's step 2, on the plan's step 9 preference line and section 5; (2) the mapping 456 DECIDED (3) left to 9.2: every non-space character the port prints is scored as sure, because fldigi shows every character alike and an operator reads it as asserted (CLAUDE.md 0.0), and mapping it to dim would hide its errors from MET-CER-SURE; its no-match output at rx_lookup's caller is scored as a placeholder, as ours is; its spaces are word boundaries as emitted, 456's 5-unit finding included; (3) the port is given the pitch instrument's pitch per recording, synthetic cases included, and is never given the construction pitch or our tracker's pitch; ours tracks its own as today; (4) the conditions are the two the tree has, real with inferred keys and synthetic with exact keys, with no CH-* row until step 7; (5) 456's DECIDED (2) to (5), 457's recorder precedent for read-only observation, and 443's DECIDED (3) hold, and 2.5, 6.6 and 9.8 are not ticked; (6) the app line's headless dispatcher-loop loss, 457's section 4 item 3: if the one rerun also loses a test, each lost type is run alone and must pass, and the report names them; (7) 457's section 4 items 1 and 2 are logged, not chased, and 9.1's tick stands until the owner unticks it.
LICENCE: PHASE_PLAN.md step 9 line 9.2, its preference line and sections 5 and 6, and step 7.4; CW_REQUIREMENTS.md section M, HM-REQ-122, 123, 124, 129, section B, HM-REQ-004; CW_SPEC.md metric definitions; R72, R77, R78, R80, R84, R85; arbiter rulings 443 DECIDED (3), 448 DECIDED (6), 456 DECIDED (2) to (5); V-04, V-11, V-13, V-14; HM-DEC-155; HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5; GPL-3
ACCOMPLISHED: Hamlet knows, recording by recording and in the requirements' own numbers, whether fldigi's receiver reads the same air better or worse than Hamlet's decoder, which is the table every later step-9 line and the choice of what to borrow first are read from
ADVANCES: step 9 criterion 2
END-ARBITER-DECISION
```
