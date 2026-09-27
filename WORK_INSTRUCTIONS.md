# Work instruction 461 - the channel profiles are generated: CH-AWGN and the Watterson CH-* conditions, proved on their own audio

**Loop unit.** Step 7, criterion 7.1 (`CW_SPEC.md` section 9, HM-REQ-040, 041, 042; V-06).
Step 7 is at 0 of 5 and no attempt has been recorded against any of its lines.

**Why step 7 and not the launcher's step 2.**
- Step 2's only open line is **2.4**. It is a count: three consecutive step-2 units with no kept
  change. Unit 460 made it 1 of 3. No unit can flip it this pass.
- Unit 460 did not advance. A second step-2 unit that cannot flip a line would be the second
  non-advance in a row.
- The plan's section 5 makes steps 2 to 7 independent. Step 7's text says why it matters: **without
  the conditions, no requirement can be judged at its own condition**. HM-REQ-040, 041, 042, 043 and
  044 all name a CH-* profile. HM-REQ-013, 020, 050 and 080 name CH-AWGN. Nothing in the tree
  produces any of them.

**This unit:**
1. Builds a channel layer that produces every CH-* profile `CW_SPEC.md` section 9.1 names, over the
   project's shaped noise band.
2. Proves that layer on its own audio.
3. Writes each recipe down so that another unit can rebuild every case.
4. If time allows, reads one point of HM-REQ-040 and 041 with our decoder.

**Nothing under `src` changes.** Decoding stays byte-identical.

Three working tasks, plus the exit round. Drop from the back.

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
- Scripts go in `.run-unit\unit461-<name>.sh` and are run with `sh`.

**The report:**
- Use the four headings exactly: `## 1. What Claude did`, `## 2. What the owner should expect`,
  `## 3. What you should see`, `## 4. What's blocking us`.
- Write the `UNIT:` line without brackets.
- Write `ADVANCES: step 7 criterion 1`. The launcher reads the digits after "criterion", so this
  means plan line 7.1.
- WHY cites the plan.

**R85. A CW question is answered from the documents, never raised to the owner.** That covers what
"spread" means, where noise goes in the chain, and how SNR is referred. Answer it from
`CW_SPEC.md`, `CW_REQUIREMENTS.md`, the radio's manual and the published Watterson and CCIR
descriptions. Record your reading in the channel document and in one line of section 4, then
carry on.

---

## 2. Why this unit exists

**The count today, by the plan's checkboxes:**

| step | met |
|---|---|
| 0 | done |
| 1 | done |
| 2 | 4 of 5 (2.4's count 1 of 3) |
| 3 | 3 of 6 |
| 4 | 5 of 7 |
| 5 | 1 of 6 |
| 6 | 3 of 6 |
| 7 | 0 of 5 |
| 8 | 0 of 6 |
| 9 | 3 of 8 |

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  The test fixtures can produce every CH-* channel profile
            CW_SPEC.md 9.1 names - CH-AWGN and the two-path Gaussian-Doppler
            profiles - over a shaped noise band and never digital silence,
            at an SNR stated in the 2500 Hz reference, each proved on its own
            audio against what it was built to be, with a recipe another unit
            can rebuild from; nothing under src changes.
ADVANCES:   step 7 criterion 1
```

**Read these first. They win over this instruction:**
- `CW_SPEC.md` sections 5, 8 (SNR and its 2500 Hz reference), 9 (the profiles) and 10 (TX-ITU);
- `CW_REQUIREMENTS.md` sections C and E, and the V-rules.

**The requirements this unit serves.** It meets none of them. It makes them measurable.
- **HM-REQ-040 (must):** on CH-LM and CH-MM, with TX-ITU at 20 WPM, the decoder meets 010, 011 and
  012 down to -4 dB reference.
- **HM-REQ-041 (must):** on CH-HM, the same down to -1 dB reference.
- **HM-REQ-042 (should):** on CH-LD, CH-MD and CH-HD, a floor set when first measured.
- **HM-REQ-020 (must)** and **HM-REQ-013 (must)**, which name CH-AWGN.
- **Verification rows 040 and 041** say "needs vendored profile values". Section 9.1 marks every
  profile except CH-LM `[verify]`. This unit builds with the values as the spec states them and
  carries each profile's status beside it (DECIDED (3)).

**The loop test finds no entry for this approach, and none has been recorded against 7.x.**

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1, and carry on as far as the tree allows.

**Check each of these:**
- **HEAD** is `e6140891` or a runner commit on top of it.
- **`PHASE_PLAN.md`** shows 2.1, 2.2, 2.3 and 2.5 ticked, 2.4 open, and 7.1 to 7.5 open, in both
  copies.
- **The test fixtures** hold `tests\Hamlet.RadioEngine.Tests\Cw\Fixtures\CwFixtureGenerator.cs`.
  Check the following:
  - Its `ShapedNoise` shapes white noise to a 350 to 870 Hz passband, at 8000 samples a second.
  - Its `SignalToNoiseDb` is measured inside that passband, not in the 2500 Hz reference. Section
    8.1 says every such figure is re-labelled. That is a finding, and it is not repaired here.
  - Nothing in the tree produces a CH-* profile. If something does, that is a mismatch. Report it,
    and build on what exists rather than beside it.
- **The runner's uncommitted writes:**
  - the modified `.run-unit` state files, `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and
    `RUN_LEDGER.md`;
  - the untracked `.run-unit\reports\unit-7-output-3.md` and `.run-unit\watched.rc`.

  Commit them as they are. **`.run-unit\fldigi\` is untracked. Do not stage it, and do not fetch.**
- **The reload's `RULES_AT` disagreement** (HM-DEC-165 against CPS-DEC-0183) is logged. It is not
  this unit's.

**Entry figures, 460's exit, to compare against:**
- **Build:** 0 errors.
- **Engine line:** 178 of 178.
- **App line:** 278 of 278.
- **Floor tests:** named 13 of 13, captures 51 of 51, adjudicated 13 of 13.
- **Real set, inferred key:**
  - MET-CER-SURE 33 of 436;
  - MET-INVENTED 33 over 473;
  - coverage 403 over 473;
  - MET-WBE 37 (29 inserted, 8 deleted) over 113.
- **Synthetic set, exact key:**
  - MET-CER-SURE 14 of 173;
  - MET-INVENTED 14 over 252;
  - coverage 159 over 252;
  - MET-WBE 44 (13 inserted, 31 deleted) over 84.

**Expected failures, not regressions:**
- `TheSpeedFollowsTheSendersMarkPairsTests`, red at 28 of 31, as unit 459 left it. It is on neither
  carry-forward line.
- The app line losing a test to Avalonia's headless "dispatcher loop". Handle it under DECIDED (7).

---

## 4. Rulings in force - transcribed, do not re-argue

**Fixtures (the rules this unit lives under):**
- **V-06.** No synthetic fixture contains digital silence. Every one carries a shaped noise band.
- **V-04.** A fixture the reference decoder cannot read is a generator defect, and the control is
  the real recording. Lowering a gate to admit a fixture is forbidden. Here that binds the
  CH-AWGN case at 15 dB reference. A faded or low-SNR case our decoder misreads is the condition
  under test. It is not a generator defect.
- **V-12.** Nothing is diagnosed against audio that has not itself been proved. So task 2 proves
  the audio before task 3 decodes any of it.
- **V-07.** Synthetic segments join across a gap, never mid-character.
- **V-14.** No separation limit, confirmation rule or plausibility bound is loosened to pass a
  fixture.
- **CLAUDE.md §12.5.** A fixture built from the same misunderstanding as the code proves nothing.
  So each channel is proved against what the published model says it is, measured on the audio
  and on the fading processes. It is never proved by asking our decoder whether it can read the
  result.
- **§12.5 and V-04 on the existing corpus.** No existing fixture, recording, key or synthetic case
  is altered, re-seeded, trimmed, padded or dropped. `CwFixtureGenerator`'s existing outputs stay
  byte-identical.

**The documents:**
- **CW_SPEC §8.1.** SNR is stated in the 2500 Hz reference: `SNR_2500 = SNR_BW - 10 log10(2500/BW)`.
- **CW_SPEC §9.1.** Two independently fading paths of equal power, a Gaussian Doppler spectrum, and
  delay and spread per profile, at the values and statuses in its table.

**R78, the keep rule,** does not bind this unit, because nothing under `src` changes. The floors
are held as V-11's guard: the three floor tests and both carry-forward lines stay green at every
commit.

**Scope and questions:**
- **R80.** No traceability, test inventory or decision-log work.
- **R77.** A new CW test names the requirement or V-rule it serves.
- **R85.** As section 1.
- **R72.** No word, dictionary or callsign prior (HM-REQ-004, HM-DEC-175).
- **HM-REQ-122.** Nothing under `src\Hamlet.RadioEngine\Cw\Second\` changes.

**Standing rules:**
- `CLAUDE.md` §0.0: never present a guess as a decode.
- `CLAUDE.md` §0.2: nothing that keys or transmits. **`KeyerCwSender.cs` and the eleven transmit
  files `PARKED.md` names are not touched, not called and not reused.** The generator keys a
  number array, not a radio.
- HM-DEC-155, HM-DEC-165 and FACT-004.

---

## 5. Status cadence

Post one line:
- at the start of each task, naming what it will measure or build;
- at each commit, with its hash;
- if a type runs past its timeout.

Post nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 461 - STEP 7` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 461 with `CURRENT_STEP: 7`, in both copies.
3. Bump the patch from 1.13.147 to 1.13.148.
4. Commit the runner's writes as they are. Check `git status` before each commit.
5. Run the entry round:
   - build;
   - both carry-forward lines;
   - the three floor tests;
   - `TheRequirementsAreMeasuredTests`, real and synthetic;
   - `TheSyntheticCqRebuildsTests` and `WhatTheGeneratorMakesTests`, the generator's own guards.
6. Save every keyed recording's text, with each character's class, to
   `.run-unit\unit461-text-before.txt`.

### Task 1 - the trace: what the tree already makes, and the channel it lacks

This task reads and prints. **Nothing is built.** Print to `.run-unit\unit461-channel-trace.txt`:

1. **What `CwFixtureGenerator` does, stage by stage, with file and line:**
   - the keying envelope and its edge shaping;
   - the tone;
   - `ShapedNoise` and its three bandpass sections, the 30 dB skirt and the RMS it returns;
   - where `SignalToNoiseDb` is applied, and over what bandwidth;
   - the existing `QsbHz` and `QsbDepthDb` fade;
   - the sample rate.
2. **The passband's equivalent noise bandwidth,** measured from `ShapedNoise`'s own output over a
   long seeded run. Then the offset that turns its in-passband SNR into the 2500 Hz reference, by
   §8.1's formula. This is the number every later SNR in this unit rests on.
3. **What each CH-* profile needs that the tree lacks:**
   - two paths;
   - a delay;
   - a complex Gaussian tap per path with a Gaussian Doppler spectrum of the stated spread;
   - equal mean power.
4. **Your reading of the open definitions, under R85, each with its source:**
   - **what "spread" means.** The author's reading, DECIDED (4): the two-sigma width of each
     path's Gaussian Doppler power spectrum, as in Watterson and CCIR Rec. 520.
   - **where the noise enters.** The author's reading: after the fading, so the SNR is the
     unfaded mean signal power against the noise.
   - **what TX-ITU is in the tree.** Say whether the existing keyer is exactly 1:3:1:3:7. Name it
     TX-ITU only if it is.

   A reading that differs from the author's is allowed. State why, from a source.

**Commit the trace.**

### Task 2 - the channel layer, built and proved on its own audio (7.1's own work)

Build a channel layer in the test fixtures, beside `CwFixtureGenerator`, for example
`tests\Hamlet.RadioEngine.Tests\Cw\Fixtures\CwChannel.cs`. **Nothing under `src` changes.**

**It takes:**
- a CH-* profile id;
- an SNR in dB, in the 2500 Hz reference;
- a keyed message;
- a speed;
- a pitch;
- a seed.

**It returns** audio, the exact key by construction, and a sidecar recipe.

**The construction the author recommends.** Take the keyed real envelope `e(t)`, already
edge-shaped as the existing generator shapes it. Form:

`Re{ A [g1(t) e(t) + g2(t) e(t - tau)] exp(j 2 pi f0 t) }`

- `g1` and `g2` are independent complex Gaussian processes. Each has unit mean power and a
  Gaussian Doppler spectrum of the profile's spread.
- `tau` is the profile's delay.

Then add the project's shaped noise band at the level the SNR asks for, after the fading.

**CH-AWGN** is the same path with `g1` = 1 and no second path. You may build another construction
that is equivalent, if the trace says why.

**The profiles:**
- **Every profile in §9.1's table** is produced at the delay and spread the table gives. Each
  profile's status (`confirmed`, `[verify]`, `project baseline`) is carried into its recipe and
  its sidecar.
- **CH-MDV** has no values in the spec. It is **refused by name**: the call throws, stating that
  §9.1 gives no delay or spread, and that §12 item 2 is the source to vendor. **No value is
  invented for it** (DECIDED (3)).

**Prove it.** Write `TheChannelProfilesAreWhatTheySayTests`, naming V-06 and CW_SPEC §9.1 and
serving HM-REQ-040, 041 and 042. Watch each assertion fail first on a deliberately wrong build,
such as the wrong spread, the noise before the fading, or one path. Say how each was watched.

Every figure below is measured from the output, never read back from the recipe:
1. **Spread.** For each fading profile, each path's measured Doppler spectrum has a two-sigma
   width within a tolerance you state and justify. Measure it from the tap process over a long
   seeded run.
2. **Power.** The two paths' mean powers are equal, and the combined long-run mean power matches
   the unfaded power, each within a stated tolerance.
3. **Delay.** The delay between the two paths is the profile's, read from the generated keyed
   edges or the tap-applied signals.
4. **SNR.** CH-AWGN's measured SNR, converted to the 2500 Hz reference, lands within 0.5 dB of the
   request at three levels: -7, -4 and +15 dB reference.
5. **V-06.** No 10 ms block of any output falls below the noise floor, anywhere, including before
   the first mark and after the last.
6. **The key.** The key returned equals the message keyed, by construction.
7. **Determinism.** The same seed gives byte-identical audio.
8. **The refusal.** CH-MDV is refused with its stated reason.

**Keep the existing corpus fixed.** `TheSyntheticCqRebuildsTests` and `WhatTheGeneratorMakesTests`
are green, and every existing generator output is unchanged.

**Write the recipe.** Write `docs/phase-requirements/channels.md`. For every CH-* profile it gives:
- its id, delay, spread and status;
- the construction;
- the Doppler filter;
- the noise band and its equivalent bandwidth;
- the SNR reference and its offset;
- the sample rate;
- the seed rule;
- how to call the layer.

Another unit must be able to rebuild every case from that page alone. Add one line stating **what
these fixtures do not prove** (§12.5, V-04): they are the published model, not the band, and the
values marked `[verify]` are unvendored.

**Run and commit:**
1. Run the three floor tests, both carry-forward lines, the new test type, and the generator's two
   guards.
2. Commit. **The new test is committed green.** No watched-red state is committed.

**Tick 7.1 in both copies of `PHASE_PLAN.md`** only if all of these hold:
- every profile §9.1 gives values for is produced and proved;
- CH-MDV is refused by name;
- no digital silence appears;
- the recipe is written.

Otherwise leave 7.1 open, and name what held it.

### Task 3 - one reading of HM-REQ-040 and 041 (drop candidate)

**Only after task 2 is committed.** This measures our decoder. It changes nothing.

1. **Generate.** Use TX-ITU at 20 WPM, the synthetic CQ text the synthetic set already uses, one
   stated pitch and three seeds, at each of these:
   - **CH-AWGN** at +15, -4, -7 and -10 dB reference;
   - **CH-LM and CH-MM** at +15 and -4 dB reference;
   - **CH-HM** at +15 and -1 dB reference.
2. **Decode** through the same `CwMetrics` calls `TheRequirementsAreMeasuredTests` uses.
3. **Print** a table to `.run-unit\unit461-conditions.txt`. For each profile and SNR, give:
   - the key's kind, which is exact;
   - MET-CER-SURE;
   - MET-INVENTED;
   - coverage;
   - MET-WBE;
   - the texts.

**State for each requirement:**
- **HM-REQ-013**, on the CH-AWGN +15 dB cases: met or not met.
- **HM-REQ-040 and 041**, at their named SNR: met at this point, or not met. This is one point,
  not a sweep. **V-05 says a floor is found by sweeping, so no floor is claimed.**

**If CH-AWGN at +15 dB reference is misread,** V-04 makes that a generator defect to trace against
the existing clean synthetics. Do not lower anything to pass it. Report it.

**Commit** the printer as a fact that asserts nothing. Name it
`TheChannelConditionsAreReadFact`, serving HM-REQ-013, 040 and 041. Keep it off both
carry-forward lines. Commit its printout with it.

**This is the drop candidate.** If time is short, shed it first and say so. Tasks 0 to 2 and the
exit round are not shed.

### Task 4 - the exit round

**Run the following, and report every figure beside its entry figure:**
- build;
- both carry-forward lines;
- the three floor tests;
- the adjudicated readings;
- `TheRequirementsAreMeasuredTests`, real and synthetic;
- `TheSyntheticCqRebuildsTests` and `WhatTheGeneratorMakesTests`;
- `TheChannelProfilesAreWhatTheySayTests`.

**Also print:**
- `git diff e6140891 -- src`, which prints nothing;
- `git diff 7e209cb4` over the eleven transmit files `PARKED.md` names, which prints nothing;
- our texts against task 0's save, which are identical;
- `git status`, showing `.run-unit\fldigi\` still untracked;
- a table of every commit this unit made, with the five results at its exit.

**Ticks:**
- **7.1**, as task 2 says.
- **7.5**, in both copies, only if that commit table shows every commit of this unit green on the
  five, and the exit round agrees (DECIDED (5)).
- Do not tick 7.2, 7.3 or 7.4.
- Do not tick 2.4, and do not count this unit toward it. It is not a step-2 unit.

---

## 7. Parked - do not touch, do not raise

- **The four questions `PARKED.md` records, and their `RESOLVED:` answers.** Read them, do not
  re-work them.
- **Step 2's decoder work, and 2.4's count.** The count stays 1 of 3.
- **`TheSpeedFollowsTheSendersMarkPairsTests`**, which stays red where 459 left it.
- **6.5's tick, HM-REQ-084's `ABOVE`, and unit 455's two 6.1 findings.**
- **fldigi's squelch default, 457's section 4 items 1 and 2, and 458's items 2 to 5.**
- **460's section 4,** logged and not chased (DECIDED (6)).
- **Vendoring the ITU tables (§12).** This unit builds from §9.1 as written.

## 8. Do not

- **Do not change any line under `src`.** That includes `CwMetrics`, the scorer, `MorseAlphabet`,
  any decoder and anything under `Cw\Second\`.
- **Do not touch, call or reuse `KeyerCwSender.cs`** or any transmit file (§0.2).
- Do not change any existing fixture's bytes, recipe, seed or key.
- **Do not invent CH-MDV's values.** Do not replace any §9.1 value with one from memory. A source
  you believe differs is a finding in section 4.
- Do not tune a tolerance in task 2 to what the build produced. State each tolerance, with its
  reason, before you run it.
- Do not claim a sensitivity floor from task 3 (V-05).
- Do not add a new test to either carry-forward line.
- Do not re-bank any floor. Do not touch the captures or the adjudicated tables.
- Do not take anything that knows words, callsigns or letter frequencies (R72).
- Do not edit `PARKED.md`, `CW_SPEC.md`, `CW_REQUIREMENTS.md`, or any ruling in `PHASE_PLAN.md` or
  `CLAUDE.md`.
- **Do not install any package. That is `MOVE: stop`.**
- Do not stage, commit or modify `.run-unit\fldigi\`, and do not fetch.
- Do not re-point or retire an existing test (R80).
- **Do not run an unfiltered `dotnet test`. Never run in the background and poll. Never compose a
  timestamp.**

## 9. Committing and pushing

**One commit per task:**
- task 0;
- task 1's trace;
- task 2's layer, test and recipe;
- task 3's fact and printout;
- task 4.

**Messages** take the form `unit461 task N: <what> (7.1)`.

**Exit state:** every commit exits with these green:
- the build;
- both carry-forward lines;
- the three floor tests;
- the generator's two guards.

The one exception is the dispatcher loss under DECIDED (7).

**Push each commit** to `origin/main`.

## 10. Report

Write `output.md` at the root with the four headings exactly. **The ordering block comes first.**
`validate-output.bat` refuses a report without it.

```
READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 4 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 <n> of 5, 8 0 of 6, 9 3 of 8.
B. Step 7, criterion 7.1: CH-* profiles produced <n> of 10 with values,
   CH-MDV <refused by name | other>; proved on their own audio - spread
   <n of n>, power <n of n>, delay <n of n>, SNR in the 2500 Hz reference
   <n of 3 within 0.5 dB>, V-06 <held | broken>; recipe in channels.md
   <written | not>; 7.1 <ticked | open>; 7.5 <ticked | open>.
   Task 3 <ran | dropped>: HM-REQ-013 on CH-AWGN +15 dB <met | not met>;
   HM-REQ-040 at -4 dB on CH-LM <met | not met>, CH-MM <met | not met>;
   HM-REQ-041 at -1 dB on CH-HM <met | not met>; one point each, no floor
   claimed.
C. The findings weighed against A and B: how many items section 4 raises,
   and whether any is in the way of 7.1 to 7.5 or of HM-REQ-040 and 041
   being measured as sweeps.
```

```
UNIT:       461 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <the criterion and why>
NUMBER:     CH-* profiles produced and proved <n> of 10; in-passband to 2500 Hz reference offset <x> dB; HM-REQ-040/041 at their SNR <met/not met each, or not measured>; real MET-CER-SURE 33/436 to <a>/<sure> (inferred, must be unchanged)
DRIFT:      step 2 <n>; step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 7 <n>; step 9 <n>
```

**Section 3 leads with the profile table.** For each profile, one row:
- id, delay, spread and status;
- measured spread per path, measured power ratio and measured delay;
- measured SNR at each requested level, in the 2500 Hz reference;
- V-06.

Then give:
1. task 1's offset and the three R85 readings, each with its source;
2. how each assertion was watched failing first;
3. if task 3 ran, its table by profile and SNR, with the key's kind, the four metrics and the texts;
4. the commit table, with the five results at each commit.

**Section 2, one paragraph.** Say that the screen reads exactly as before, because nothing under
`src` changed. Then say, in plain words, what the owner can now ask of the decoder that could not
be asked before: how it reads a signal fading the way the published channel models fade.

---

```
ARBITER-DECISION
STEP: 7
APPROACH: build a Watterson two-path Gaussian Doppler channel layer in the test fixtures for every CH-* profile CW_SPEC 9.1 gives values for, over the shaped noise band at an SNR in the 2500 Hz reference, prove spread power delay SNR and no digital silence on its own audio, write the recipe to channels.md, then one decode point per must profile for HM-REQ-040 and 041
MOVE: work around
WHY: PHASE_PLAN.md step 7 line 7.1 asks that the generator produce the CH-* channel profiles CW_SPEC.md names, each with a shaped noise band and never digital silence (V-06), with every recipe stated, and step 7's own text says no requirement can be judged at its condition without them; step 2's only open line 2.4 is a count at 1 of 3 that no unit can flip this pass and unit 460 did not advance, so the plan's section 5 independence routes the work here, and the loop test finds no entry for this approach.
STATE: not started
DECIDED: author's, overrulable - (1) step 7 is worked instead of the launcher's step 2, on PHASE_PLAN.md section 5's independence of steps 2 to 7, because 2.4 cannot flip this pass and a second non-advancing step-2 unit would follow 460's; this unit is not a step-2 unit and leaves 2.4's count at 1 of 3; (2) the layer lives in the test fixtures beside CwFixtureGenerator and nothing under src changes, so decoding is byte-identical and R78 does not bind, with the floors held as V-11's guard; (3) the profiles are built at CW_SPEC 9.1's values with each status carried into the recipe, and CH-MDV, for which the spec gives no values, is refused by name rather than invented - where the plan's all-profiles wording and the spec differ, the spec wins and the difference is a finding; (4) under R85, spread is read as the two-sigma width of each path's Gaussian Doppler power spectrum (Watterson, CCIR Rec. 520), noise enters after fading, and SNR is the unfaded mean signal power against noise referred to 2500 Hz by CW_SPEC 8.1, the unit free to differ from a cited source; (5) 7.5 is ticked at this unit's exit only if every commit it made is green on the five, as 460's section 4 item 4 proposed for the floor lines of steps 3 to 9, and 7.2 to 7.4 are not ticked; (6) 460's section 4 is logged, not chased - items 1 and 2 bear on PARKED.md and 17:37 and are the owner's file and settled, item 3 is the count above, item 4 is applied only as (5), item 5 is kept for step 2's next author, item 6 is logged; (7) the app line's headless dispatcher-loop loss: one rerun, and any type lost again is run alone and must pass, named in the report.
LICENCE: PHASE_PLAN.md step 7 lines 7.1 and 7.5, sections 5 and 6, R77, R78, R80, R85; CW_SPEC.md sections 5, 8.1, 9.1, 9.2, 10 and 12; CW_REQUIREMENTS.md HM-REQ-013, 020, 040, 041, 042, verification rows 040 and 041; V-04, V-05, V-06, V-07, V-11, V-12, V-14; R72; HM-REQ-122; HM-DEC-155; HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: the decoder can for the first time be put through the fading the published HF channel models describe - quiet, moderate and disturbed, low, mid and high latitude - at a signal level stated the way the requirements state it, so the fading requirements can be measured instead of assumed, and one first reading of how it copes is on the page
ADVANCES: step 7 criterion 1
END-ARBITER-DECISION
```
