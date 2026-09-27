# Work instruction 463 - the last fldigi technique nobody has tried: its detection front end, a half-dit integrator at the speed in force and its AGC-normalized level, screened on every recording and kept under R78

**Loop unit.** Step 9, criterion 9.4 (`CW_REQUIREMENTS.md` section M, HM-REQ-129; kept under R78
against HM-REQ-010, 011, 012 and 080).

**Why step 9 and not the launcher's step 2.**
- **R86, `PHASE_PLAN.md` section 6:** *"Steps 2 to 8 are not authorable until 9.4 has a kept
  change. Every unit is step 9's until a technique from the second decoder is kept in ours."* The
  owner committed this at `6de324b1`. No later ruling has lifted it.
- Step 2's only open line, 2.4, counts three step-2 units with no kept change. R86 bars step-2
  units, so 2.4 cannot move. This unit is not a step-2 unit and does not count toward 2.4.
- 9.4 is the line everything else waits behind: 9.5 to 9.7 build the vote, and R86 puts 9.4 first.

**What was tried at 9.4, and why this is not that.** Unit 459's grouping named five fldigi
mechanisms, (A) to (E), behind the letters ours loses and the port keeps.
- **(D)**, pair speed tracking (`cw.cxx` 524-535, 831-843), was refused by unit 459.
- **(B)** the half-dit spike rule, **(C)** the two-dot class edge and **(W)** the 2- and 4-dit gap
  edges (`cw.cxx` 515 and 818-822, 846-855, 883-914, which cover (E)'s 880-888) were each refused by
  unit 462.
- **(A), detection, has never been screened.** That is the only mechanism 9.3 named that is left.

**Why (A) now, when 462 left it aside.** Unit 459 put (A) aside because 17:37's lost dahs stand at
noise in *our* envelope (median 1.0 sigma, peak 2.3, against a keyed level of 3.9), so "no
detection rule on our envelope keeps them." That argument is about a threshold laid on the
envelope ours already forms. fldigi's detection is **also how the envelope is formed**:
- a low-pass of 5 x WPM / 1.2 Hz on the mixed baseband (`cw.cxx` 352-356, 396, 696);
- decimation by 16 (`DEC_RATIO`, 704);
- a moving average half a dit long, `bitfilter = symbollen / (2 * DEC_RATIO)` (358-361, 428-431,
  708);
- then an AGC: signal, noise floor and peak each tracked with attack and decay, and the level
  divided by the peak (610-632), before the hysteresis detector (640-656).

Ours integrates at a fixed 45 Hz Hann (`CwProbabilisticDecoder.IntegratorBandwidthHz`, about a
33 ms window), whatever the speed. A half-dit boxcar at 17:37's speed integrates longer and
narrower, and that could lift a mark from noise, which no threshold can. **Whether it does is what
task 1 measures and the screen decides.** HM-DEC-112 already recorded *"bandwidth following speed
is real and comes second, measured separately"*, and `CW_SPEC.md` section 7 ties the analysis
bandwidth to the speed. Nobody has measured it since.

This unit screens **two forms of (A)**, one at a time, each fixed before any screen:
- **(A1) the half-dit integrator.** The envelope the lattice scores is formed through fldigi's
  speed-matched low-pass and half-dit boxcar, at the speed the read is decoded at.
- **(A2) the AGC-normalized level.** The envelope is divided by fldigi's attack/decay peak tracker
  before the key-down and key-up likelihoods.

Both are screened. The one R78 keeps goes into its own commit and 9.4 is ticked. If both are kept,
only the one with the larger fall in real MET-CER-SURE is committed (DECIDED (4)). A refused form
is reverted, with its patch and numbers saved, and never narrowed.

**The port under `Cw\Second\` is not touched** (HM-REQ-122, 129).

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

**HM-DEC-155. No suite:**
- Run named types only, one per invocation, each with its own `timeout`.
- Captures get 600 s.
- Never run in the background and poll.
- If one type needs more than 600 s, split it by recording or by case into several named types or
  filters. Never raise the timeout past 600 s.

**Shell limits:**
- Apostrophes in quoted heredocs break, and doubled backslashes collapse.
- `;` is refused, and so is `rm`.
- Python cannot run here.
- A multi-line commit uses `-m` more than once.
- Scripts go in `.run-unit\unit463-<name>.sh` and are run with `sh`.
- To revert a refused screen, use `git checkout -- <paths>` or `git restore`. Never use `rm`.

**The report:**
- Use the four headings exactly: `## 1. What Claude did`, `## 2. What the owner should expect`,
  `## 3. What you should see`, `## 4. What's blocking us`.
- Write the `UNIT:` line without brackets.
- Write `ADVANCES: step 9 criterion 4`. The launcher reads the digits after "criterion", so this
  means plan line 9.4.
- WHY cites the plan.

**R85. A CW question is answered from the documents and fldigi's source, never raised to the
owner.** That covers which speed is "in force" for the integrator, how fldigi's decimated-sample
time constants map to our hops, and where in our chain the AGC division goes. Record your reading
in one line of section 4, and carry on.

---

## 2. Why this unit exists

**The count today, by the plan's checkboxes:**

| step | met |
|---|---|
| 0 | done |
| 1 | done |
| 2 | 4 of 5 (2.4's count 1 of 3; barred by R86) |
| 3 | 3 of 6 (barred by R86) |
| 4 | 5 of 7 (barred by R86) |
| 5 | 1 of 6 (barred by R86) |
| 6 | 3 of 6 (barred by R86) |
| 7 | 2 of 5 (barred by R86) |
| 8 | 0 of 6 |
| 9 | 3 of 8 |

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  fldigi's detection front end - the envelope integrated over half
            a dit at the speed in force, or its level normalized by fldigi's
            AGC - is taken into our decoder in its own commit and kept
            because the requirements' metrics say ours reads better with it
            and no recording reads worse, with the port left as ported.
ADVANCES:   step 9 criterion 4
```

**Read these first. They win over this instruction:**
- `CW_REQUIREMENTS.md` section M (HM-REQ-122, 123, 129), section B (HM-REQ-010 to 013) and
  section I (HM-REQ-080 and 081), and the V-rules, V-11 and V-14 above all;
- `CW_SPEC.md`'s definitions of MET-CER-SURE, MET-INVENTED, MET-COVERAGE and MET-WBE, and its
  section 7 on analysis bandwidth;
- `.run-unit\fldigi\src\cw_rtty\cw.cxx` at `61b97f41`, lines 290-440 and 593-720, and `DEC_RATIO`
  in `src\include\cw.h`;
- unit 459's report, `.run-unit\reports\unit-6-output-4.md`, sections 1 and 3, for group (a) and
  the level of 17:37's missing dahs;
- the remarks on `IntegratorBandwidthHz` in `CwProbabilisticDecoder.cs`, and
  `ANALYSIS-cw-integrator-bandwidth-2026-08-23.md`, for what the fixed 45 Hz was chosen against;
- HM-DEC-112 in `CLAUDE.md`.

**The requirements this unit serves:**
- **HM-REQ-129:** a technique from the second decoder is taken into ours as a change of its own,
  judged under the keep rule, and the second decoder stays as ported.
- **HM-REQ-010 (MET-CER-SURE < 1%), 011 (MET-INVENTED zero), 012 (coverage at least 90%), 080 and
  081 (MET-WBE at most 5%):** the keep rule's metrics.

**The loop test finds no entry for this approach.** The recorded refusals at 9.4 are unit 459's
(D) and unit 462's (B), (C) and (W). None is repeated here.

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1, and carry on as far as the tree allows.

**Check each of these:**
- **HEAD** is `f40a97f9` or a runner commit on top of it.
- **`PHASE_PLAN.md`**, in both copies:
  - shows 9.1, 9.2 and 9.3 ticked, and 9.4 to 9.8 open;
  - carries R86's line in section 6.

  If `docs/phase-requirements/PHASE_PLAN.md` differs from the root copy, report it.
- **The port** is at `src\Hamlet.RadioEngine\Cw\Second\`, and `git diff 19109b51 --
  src/Hamlet.RadioEngine/Cw/Second/` prints nothing.
- **`IntegratorBandwidthHz` is 45.0**, and both `CwProbabilisticDecoder.Envelope` and
  `CwProbabilisticStream`'s constructor take their window from `IntegratorWindow` with it.
- **Unit 462's saves** are in `.run-unit\`: the three `unit462-<B|C|W>-refused.patch` files, their
  `.txt` numbers, and `unit462-text-before-classes.txt`.
- **The runner's uncommitted writes:**
  - the modified `.run-unit` state files, `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and
    `RUN_LEDGER.md`;
  - the untracked `.run-unit\reports\unit-1-output-23.md` and `.run-unit\watched.rc`.

  Commit them as they are. **`.run-unit\fldigi\` is untracked. Do not stage it, and do not fetch.**
- **The reload's `RULES_AT` disagreement** (HM-DEC-165 against CPS-DEC-0183) is logged. It is not
  this unit's.
- **`outcome-read`'s step titles** for steps 2, 3 and 8 differ from `PHASE_PLAN.md`'s. That is a
  harness finding. Log it, and do not repair it.

**Entry figures, 462's exit, to compare against:**
- **Build:** 0 errors.
- **Engine line:** 178 of 178.
- **App line:** 278 of 278.
- **Floor tests:** named 13 of 13, captures 51 of 51, adjudicated 13 of 13.
- **Real set, 23 keyed recordings, inferred key:**
  - MET-CER-SURE 33 of 436;
  - MET-INVENTED 33 over 473;
  - coverage 403 over 473;
  - MET-WBE 37 (29 inserted, 8 deleted) over 113.
- **Synthetic set, 12 cases, exact key:**
  - MET-CER-SURE 14 of 173;
  - MET-INVENTED 14 over 252;
  - coverage 159 over 252;
  - MET-WBE 44 (13 inserted, 31 deleted) over 84.
- **Decode time**, ours over 690 s of real audio: about 51.7 s.

**Expected failures, not regressions:**
- `TheSpeedFollowsTheSendersMarkPairsTests`, red at 28 of 31, as unit 459 left it. It is on neither
  carry-forward line.
- The app line losing a test to Avalonia's headless "dispatcher loop". Handle it under DECIDED (8).
- The parity run rewriting the decode-time rows of `docs/phase-requirements/parity.md`. Keep a copy
  under `.run-unit\` and restore the committed file, as unit 462 did.

---

## 4. Rulings in force - transcribed, do not re-argue

**The ordering:**
- **R86** (`PHASE_PLAN.md` section 6): *"Steps 2 to 8 are not authorable until 9.4 has a kept
  change. Every unit is step 9's until a technique from the second decoder is kept in ours."*
- **HM-REQ-122 and 129, and section 6:** *"The second decoder is faithful, not improved. A
  technique goes into ours, judged under R78; the second decoder stays as ported."*

**R78, the keep rule.** *"A change is kept when it moves a requirement's metric the right way and
breaks no other requirement. MET-INVENTED at zero, MET-CER-SURE below 1%, MET-COVERAGE at or above
90%, MET-WBE at or below 5%, per condition, at the tier the requirement names. The capture floors
stay as V-11's overfitting guard - no change may redden an earlier capture to green a newer one -
and stop being the keep rule. A capture row's character count falling is a finding to report, not
a rejection, when no requirement's metric got worse."* Applied exactly as units 459 and 462 were
judged (DECIDED (3)). A screened form is **kept** only if all of these hold:
1. **Per condition.** On the real set and on the synthetic set, none of the four metrics moves the
   wrong way, and at least one moves the right way on at least one of them.
2. **The adjudicated readings.** All 13 are unchanged, or move onto their own adjudicated text
   (R66).
3. **V-11.** On every one of the 35 keyed recordings and cases, none of the four metrics is worse.
   One recording worse on any metric refuses the change.
4. **Silence.** The empty-band and no-signal tests the floor and capture types carry still emit no
   sure character (HM-REQ-011). A form that narrows the integrator changes the room under the gate
   (the analysis note's 6.6 to 10.0 against 15 on `cw-2026-08-20-014854`). Print that margin before
   and with each form.

**Standing rulings this unit leans on:**
- **HM-DEC-112.** Bandwidth following speed is real work and is measured separately, so the gain
  can be attributed. This unit is that measurement.
- **R66.** An adjudicated reading may move onto its own adjudicated text.
- **V-14.** No separation limit, confirmation rule or plausibility bound is loosened to pass a
  fixture.
- **R72.** No word, dictionary or callsign prior (HM-REQ-004, HM-DEC-175).
- **R80.** No traceability, test inventory or decision-log work.
- **R77.** A new CW test names the requirement it serves.
- **R85.** As section 1.

**Standing rules:**
- `CLAUDE.md` §0.0: never present a guess as a decode.
- `CLAUDE.md` §0.2: nothing that keys or transmits. **`KeyerCwSender.cs` and the eleven transmit
  files `PARKED.md` names are not touched, not called and not reused.**
- `CLAUDE.md` §12.5.
- HM-DEC-155, HM-DEC-165 and FACT-004.

---

## 5. Status cadence

Post one line:
- at the start of each task, naming what it will measure or build;
- at each screen's verdict, kept or refused, with its four real and four synthetic numbers;
- at each commit, with its hash;
- if a type runs past its timeout.

Post nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 463 - STEP 9` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 463 with `CURRENT_STEP: 9`, in both copies.
3. Bump the patch from 1.13.149 to 1.13.150.
4. Commit the runner's writes as they are. Check `git status` before each commit.
5. Run the entry round:
   - build;
   - both carry-forward lines;
   - the three floor tests;
   - the adjudicated readings;
   - `TheRequirementsAreMeasuredTests`, real and synthetic;
   - `BothDecodersAreScoredAlikeTests`;
   - the port's own tests.
6. Save both decoders' texts, with each of ours' characters' class and the scorer's verdict, the
   way unit 462 did:
   - ours to `.run-unit\unit463-text-before.txt`;
   - the port's to `.run-unit\unit463-port-before.txt`.
7. Save the per-recording four-metric table, all 35, to `.run-unit\unit463-v11-before.txt`. Every
   V-11 comparison in this unit is made against this table.

### Task 1 - the trace: fldigi's front end beside ours, and each form fixed

This task reads and prints. **Nothing under `src` changes.** Write a fact that asserts nothing:
`WhatFldigisFrontEndWouldLiftFact`, serving HM-REQ-129. It prints to
`.run-unit\unit463-frontend.txt`.

1. **The two chains side by side, with file and line.** For ours and for `cw.cxx`, name:
   - the mixer, and what low-pass (if any) follows it;
   - the integrator: its shape, its length in ms, its equivalent noise bandwidth, and what sets it;
   - how the level is normalized before a decision is made from it: noise sigmas, a peak, or a
     floor, and each tracker's time constant in ms. fldigi's `decayavg` weights are counted in
     decimated samples at 8000 / 16 = 500 per second, so attack 200 is about 0.4 s and decay 1000
     is about 2 s at the defaults (`cwrx_attack` and `cwrx_decay` case 1). Confirm this from the
     source, and correct it if it is wrong;
   - where the speed is chosen relative to the envelope, in ours.
2. **The lift, first order, on the marks that matter.** For every mark in unit 459's group (a) (17:37
   R and D, 032012 O, and the synthetic ones), and for every mark of the letters on each side of
   them, print its level in noise sigmas under three envelopes:
   - ours as it stands;
   - (A1), as step 3 below fixes it;
   - (A2), as step 3 below fixes it.

   Do the same for the empty band on `cw-2026-08-20-014854`: its peak against the gate. This is a
   prediction, printed for the record. **It does not rank the forms and it does not stop a
   screen.** Unit 462 found that a first-order count misjudged W (its section 4 item 2), so both
   forms are screened whatever this shows.
3. **Before any screen, state each form once. It is fixed from here on** (DECIDED (2)).
   - **(A1).** The envelope the **lattice** scores is formed at the speed the read is decoded at
     (1200 / WPM ms, 462's R85 reading): fldigi's low-pass at 5 x WPM / 1.2 Hz, then a boxcar half
     a dit long, in place of the 45 Hz Hann. **The speed is chosen exactly as today, from today's
     45 Hz envelope.** Only the envelope the lattice scores after that choice changes, so the
     bandwidth never feeds back into the speed that set it (the loop `IntegratorBandwidthHz`'s
     remarks warn of). Both the offline path and `CwProbabilisticStream` take the form from one
     place, as they take the window today. State where the second envelope is formed, what speed
     it uses when the read has more than one, and what the offline and stream paths each do.
   - **(A2).** fldigi's signal, noise-floor and peak trackers (`cw.cxx` 610-623), at fldigi's
     default attack and decay converted to our hops, run over ours' existing 45 Hz envelope. The
     envelope is divided by the peak (629-632) before the per-hop key-down and key-up
     log-likelihoods, and nothing else in the likelihood model changes. State where the division
     goes and what the likelihoods then take as their noise.

   Where our code follows `cw.cxx`, the form names the line at `61b97f41`.

**Commit the fact and its printout.**

### Task 2 - the screens (9.4's own work)

**Build (A1) in the form task 1 fixed.** It goes into our decoder under
`src\Hamlet.RadioEngine\Cw\`, never under `Cw\Second\`. A comment names the `cw.cxx` lines.

**Judge it in the working tree, before any commit:**
1. Build.
2. Run the adjudicated readings.
3. Run `TheRequirementsAreMeasuredTests`, real and synthetic.
4. Build the per-recording four-metric table and diff it against task 0's.
5. Run the empty-band and silence checks section 4 names, and print the gate margin.
6. Time the decode over the real set.

Apply R78 exactly as section 4 states. Save the diff to `.run-unit\unit463-A1.patch` with
`git diff > file`, and its full numbers and V-11 table to `.run-unit\unit463-A1.txt`. **Then revert
the working tree with `git checkout`, whatever the verdict,** and confirm `git diff -- src` prints
nothing.

**Then screen (A2) exactly the same way**, from a clean tree, saving
`.run-unit\unit463-A2.patch` and `.run-unit\unit463-A2.txt`, and revert.

**Choosing:**
- **Neither kept:** 9.4 stays open. Go to task 4. Nothing under `src` is committed.
- **One kept:** that one is committed.
- **Both kept:** commit only the one with the larger fall in real MET-CER-SURE. On a tie, the one
  with the larger fall in synthetic MET-CER-SURE. On a second tie, (A1). The other is not
  combined with it and not screened on top of it (DECIDED (4)).

**Do not narrow, re-edge or re-scope a refused form** (DECIDED (2)). Do not change a ratio or a
time constant away from fldigi's after seeing R78's numbers.

### Task 3 - the kept change's commit and its test (drop candidate: the test's second case)

**Only if a form was kept.**
1. Re-apply its saved patch with `git apply`.
2. Run the three floor tests and both carry-forward lines.
   - A named floor row that falls on its character count, where that recording's four metrics are
     no worse, is re-banked in the same commit under R78's own sentence. Name the row, its count
     before and after, and its four metrics before and after (DECIDED (5)).
   - A floor row that falls where a metric got worse is not re-banked. That is a V-11 refusal: the
     change is refused, reverted, and 9.4 stays open.
3. Commit the change in **its own commit**. It carries only the change, any re-banked floor row,
   and a test naming HM-REQ-129 and the metric it moved.
   - The test is watched failing first at the parent commit, on the recording the change moved
     most.
   - A second case, on a synthetic case of known key, if the change moved one. **This is the drop
     candidate.** Shed it first; the first case is never shed.
   - Say how the test was watched.
4. Tick **9.4** in both copies of `PHASE_PLAN.md`, in a follow-on commit.

**If nothing was kept:** commit the two saved patches and their numbers under `.run-unit\` as
evidence. Nothing under `src` is committed. The report names, for each form, the recording and
metric that refused it.

### Task 4 - the exit round

**Run the following, and report every figure beside its entry figure:**
- build;
- both carry-forward lines;
- the three floor tests;
- the adjudicated readings;
- `TheRequirementsAreMeasuredTests`, real and synthetic;
- `BothDecodersAreScoredAlikeTests`;
- the port's own tests;
- `TheSpeedFollowsTheSendersMarkPairsTests`. Report its figure. It is not required green.

**Also print:**
- `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/`, which prints nothing;
- the port's texts against task 0's save, which are byte-identical;
- `git diff 7e209cb4` over the eleven transmit files `PARKED.md` names, which prints nothing;
- our texts against task 0's save:
  - if a form was kept, every line that moved, before and after, beside the key;
  - if none was kept, identical;
- `git status`, showing `.run-unit\fldigi\` still untracked;
- a table of every commit this unit made, with its results on the five at its exit: build, both
  carry-forward lines, and the three floor tests.

**Ticks:**
- **9.4**, as task 3 says, and only then.
- Do not tick 9.5 to 9.8, or any line of steps 2 to 8.
- If 9.4 is ticked, **R86's bar is lifted by the plan's own wording.** Say so in section 4 as a
  reading, not a ruling.
- **If neither form is kept, every mechanism 9.3 named, (A) to (E), has now been refused.** Say so
  in section 4 as a plain fact, with each one's refusing recording and metric. Do not propose a
  new technique, and do not propose lifting R86. What happens next is the next arbiter's to
  decide.

---

## 7. Parked - do not touch, do not raise

- **Techniques (B), (C), (D) and (W)** in any form, and 462's saved patches except as evidence.
  Each has been refused.
- **Unit 462's section 4 item 1**, whether V-11 is per recording or the floors alone. DECIDED (3)
  keeps it per recording, as 459 and 462 were judged. Running W's patch against the floors is not
  this unit's.
- **fldigi's hysteresis detector itself** (`cw.cxx` 640-656) and its squelch (646). Ours decides
  marks by lattice, not by threshold. Only the envelope's formation and level are taken.
- **fldigi's SOM decoding, its seed speed and its squelch default.**
- **The four questions `PARKED.md` records, and their `RESOLVED:` answers.**
- **Step 2's decoder work, and 2.4's count.** The count stays 1 of 3.
- **Steps 3 to 8,** under R86.
- **461's section 4:** the 1.0 s lead-in first character, the fading profiles read sure and wrong,
  and the rest. These are step 7's.
- **459's section 4 items 3 to 5,** and 458's items 2 to 5.
- **6.5's tick, HM-REQ-084's `ABOVE`, and unit 455's two 6.1 findings.**
- **The two `PHASE_STATUS.md` copies differing beyond the unit's fields** (462 section 4 item 5).

## 8. Do not

- **Do not change any line under `src\Hamlet.RadioEngine\Cw\Second\`** (HM-REQ-122, 129).
- **Do not change `CwMetrics`, the scorer, `MorseAlphabet` or any test's key.**
- **Do not touch, call or reuse `KeyerCwSender.cs`** or any transmit file (§0.2).
- **Do not commit a refused change under `src`,** even to revert it. Refusals live in the working
  tree and in `.run-unit\` patches.
- **Do not change fldigi's ratio (5 / 1.2, one half), or its attack and decay, after seeing R78's
  numbers** (V-14, DECIDED (2)).
- **Do not let (A1)'s integrator feed the speed choice.** The speed is chosen from today's
  envelope.
- **Do not change `IntegratorBandwidthHz` as a constant**, or sweep it. (A1) is fldigi's
  speed-matched form, not a new fixed width.
- Do not screen both forms at once, and do not combine them into one change.
- Do not widen the 8 to 40 WPM speed bounds. That is 5.1's.
- Do not re-bank any floor except under DECIDED (5). Do not touch the captures or the adjudicated
  tables.
- Do not take anything that knows words, callsigns or letter frequencies (R72).
- Do not edit `PARKED.md`, `CW_SPEC.md`, `CW_REQUIREMENTS.md`, or any ruling in `PHASE_PLAN.md` or
  `CLAUDE.md`.
- Do not add a new test to either carry-forward line.
- Do not re-point or retire an existing test (R80).
- **Do not install any package. That is `MOVE: stop`.**
- Do not stage, commit or modify `.run-unit\fldigi\`, and do not fetch.
- **Do not run an unfiltered `dotnet test`. Never run in the background and poll. Never compose a
  timestamp.**

## 9. Committing and pushing

**One commit per task, and the kept change alone in its own:**
- task 0;
- task 1's fact and printout;
- task 2's saved patches and numbers (nothing under `src`);
- the kept change, if any, with its test and any re-banked row;
- the 9.4 tick;
- task 4.

**Messages** take the form `unit463 task N: <what> (9.4)`. The kept change's message names the
form, the `cw.cxx` lines, and the four real and four synthetic numbers before and after.

**Exit state:** every commit exits with these green:
- the build;
- both carry-forward lines;
- the three floor tests.

The one exception is the dispatcher loss under DECIDED (8).

**Push each commit** to `origin/main`.

## 10. Report

Write `output.md` at the root with the four headings exactly. **The ordering block comes first.**
`validate-output.bat` refuses a report without it.

```
READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 4 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 2 of 5, 8 0 of 6, 9 <n> of 8; steps 2 to 8 <still barred | released>
   by R86.
B. Step 9, criterion 9.4: HM-REQ-129 - fldigi's detection front end,
   (A1) half-dit integrator at the speed in force <kept | refused on
   <recording, metric>>, (A2) AGC-normalized level <kept | refused on
   <recording, metric>>; the committed one <form, cw.cxx lines>: real
   (inferred) MET-CER-SURE 33 of 436 to <a> of <s>, MET-INVENTED 33 to
   <b>, coverage 403 to <c>, MET-WBE 37 to <d>; synthetic (exact) 14, 14,
   159, 44 to <...>; adjudicated <n> of 13; V-11 <0 | n> of 35 worse; gate
   margin on 014854 <before> to <after>; the port byte-identical
   <yes | no>; 9.4 <ticked | open>.
C. The findings weighed against A and B: how many items section 4 raises,
   whether any is in the way of 9.4 or of 9.5, which comes next, and -
   if 9.4 is still open - that every mechanism 9.3 named has been refused.
```

```
UNIT:       463 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <the criterion and why>
NUMBER:     HM-REQ-129 <met | not met>; real MET-CER-SURE 33/436 to <a>/<s> (inferred); synthetic 14/173 to <x>/<y> (exact); MET-INVENTED real 33 to <b>; coverage real 403 to <c>; MET-WBE real 37 to <d>; the port byte-identical <yes|no>
DRIFT:      step 2 <n>; step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 7 <n>; step 9 <n>
```

**Section 3 leads with the screen table.** One row per form, with:
- the four real and four synthetic numbers, before and with it in;
- adjudicated;
- V-11's count of recordings worse, naming each one;
- the gate margin on 014854;
- decode time;
- the verdict.

Then give:
1. task 1's two chains side by side, and the lift table for group (a)'s marks under the three
   envelopes, with a line on whether the prediction matched the screen;
2. every recording the kept change moved: key, ours before, ours after, and the port, as units 459
   and 462 printed them;
3. how the kept change's test was watched failing first;
4. the commit table, with the five results at each commit.

**Section 2, one paragraph.** If a form was kept, say which lines on which recordings now read
differently, in the owner's words, as letters he would see. If none was kept, say the screen reads
exactly as before, which form came closest, and what stood in its way.

---

```
ARBITER-DECISION
STEP: 9
APPROACH: take fldigi's detection front end into our envelope - a half-dit boxcar behind the 5 x WPM / 1.2 Hz low-pass at the speed the read is decoded at, and fldigi's attack/decay AGC peak normalization - screen each form alone on all 35 recordings and commit the one R78 keeps, speed choice unchanged, the port untouched
MOVE: work around
WHY: PHASE_PLAN.md section 6's R86 bars steps 2 to 8 until 9.4 has a kept change, so 9.4 is the only line that can move and step 2's 2.4 cannot; of the five mechanisms 9.3 named, (B), (C), (D) and (W) are recorded as refused and (A), detection, has never been screened, so this unit works around the refusals with it, and the loop test finds no entry for the approach.
STATE: partial
DECIDED: author's, overrulable - (1) step 9 criterion 9.4 is worked instead of the launcher's step 2, on R86 in PHASE_PLAN.md section 6, the owner's commit 6de324b1, as unit 462's arbiter did; this is not a step-2 unit and 2.4's count stays at 1 of 3; (2) technique (A) is taken in two forms, each fixed at task 1 before any screen and never narrowed after - (A1) the envelope the lattice scores is formed through fldigi's 5 x WPM / 1.2 Hz low-pass and half-dit boxcar (cw.cxx 352-361, 396, 428-431, 696-708) at the speed the read is decoded at, with the speed still chosen from today's 45 Hz envelope so the bandwidth never feeds the speed that set it; (A2) fldigi's signal, noise and peak trackers at its default attack and decay (610-632) divide ours' existing envelope before the likelihoods; unit 459's reason for leaving (A) - no threshold on our envelope lifts a mark at noise - does not reach a change to how the envelope is formed, and HM-DEC-112 names bandwidth following speed as work to be measured separately; (3) R78 is applied as 459 and 462 were judged - per condition none of the four metrics wrong and one right, the 13 adjudicated readings held or moved onto their own text, no one of the 35 recordings worse on any metric (V-11 per recording, keeping 462's DECIDED (4) and declining its section 4 item 1's alternative), plus the silence and empty-band checks emitting no sure character with the 014854 gate margin printed; (4) both forms are screened whatever task 1 predicts, on unit 462's finding that a first-order count misjudged W; if both are kept only the larger real MET-CER-SURE fall is committed, then synthetic, then (A1), never the two combined; (5) a named floor row that falls on count alone with its recording's four metrics no worse is re-banked in the kept commit under R78's own sentence, and one that falls with a metric worse refuses the change; (6) screens are judged in the working tree and reverted with patches saved under .run-unit, the kept one re-applied and committed alone as HM-REQ-129 asks; (7) if neither is kept the report states that every mechanism 9.3 named has been refused and proposes nothing, leaving the next move to the next arbiter; (8) the app line's headless dispatcher-loop loss: one rerun, and any type lost again is run alone and must pass, named in the report.
LICENCE: PHASE_PLAN.md step 9 lines 9.3 and 9.4, section 6 (R86, R78, the second decoder is faithful, V-14), R66, R72, R77, R80, R85; CW_REQUIREMENTS.md HM-REQ-010, 011, 012, 080, 081, 122, 123, 129, V-11, V-14; CW_SPEC.md section 7; HM-DEC-112; fldigi cw.cxx at 61b97f41 lines 290-440 and 593-720, cw.h DEC_RATIO; unit 459's report sections 1 and 3; unit 462's report sections 3 and 4; HM-DEC-155; HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: one more thing fldigi's receiver does that ours does not - listening to each mark over half a dit at the sender's own speed instead of a fixed window, or levelling the signal the way fldigi's AGC does - is in Hamlet's decoder because it made the text read better with no recording reading worse, which is what the owner's R86 asks before any other decoder work resumes
ADVANCES: step 9 criterion 4
END-ARBITER-DECISION
```
