
## 4. What's blocking us

**Nothing blocks a criterion. This unit's six items are findings and mismatches, and none needs
a ruling to proceed. Unit 393's five, unit 392's items 2 to 5, unit 391's items 2 to 6 and unit
390's nine are carried per HM-DEC-139, verbatim, below. None of them is this unit's to answer.
Unit 393's item 1, the Olivia demodulator type in `CpuMeasuredAlone`, is a self-ruling you may
overrule, carried as such.**

**1. Task 3 was dropped on its clock rule, so the two clean synthetics are unmeasured four ways.**
*A finding.* Task 2 ended at 22:44 with 19 minutes of the hour left. The four-way printer,
`Cw\CwCleanSyntheticsDiagnosisTests.cs`, was not written. The next unit makes this measurement
before any repair of #25, 26, 31 and 32, as the instruction says. Tonight's runs add one
indication. `CwDisplacementFloorTests` generates its audio in memory through `CwSignal.Generate`
and reads the same kind of placeholders, `■ ■■ ■`. So the committed `.wav` path alone is unlikely
to be the whole of it. That is a guess from one pattern, not a measurement.

**2. `ABlipDoesNotShiftEverythingAfterItTests` names `CwReferenceDecoder` only in doc prose.**
*A mismatch with section 5, decision 4 and unit 392's table.* The name occurs once, at line 30,
inside a `<para>` of the class remarks. The code never uses it, and prose in backticks does not
bind. So the name unit 392 quoted is not what keeps the file out of the build, or not alone.
The file's code names `CwSignal.Generate`, `CwSignalRequest`, `CwSignal.DefaultToneHz`,
`BufferedAudioSource.PumpAll`, `CwDecoder.Listen`, `Flush` and `Reading.Text`. Every one but
`Reading.Text` is used by a compiled engine test. `Reading.Text` appears elsewhere only in
`AMoveStartsTheDecoderFreshTests.cs`, which is itself excluded. I opened nothing under `src` and did
not build the file, so which name fails is not measured. The classification does not depend on
it: the file is red-open either way. **Option A:** the next step 3 unit re-includes the file in
one build to read the real error, and rewires it under R12 if the error is a renamed member.
**Option B:** leave it for step 4's verdict. **My recommendation:** A. It is one build, and it
may turn an uncompilable test into a runnable one.

**3. `OneDecoderNotTwoTests` does not fit in one 600 s call, and the harness caps at 600 s.**
*A finding against HM-DEC-155.* The whole type ran over 600 s. Its slower method alone ran over
580 s, with 47 of 53 green, 0 red and 6 unmeasured. A `timeout 600` inside a call that also
writes a status line is over the harness's cap by about a second. The first call was moved to
the background by the harness, not by me, and was not polled. A type this slow needs splitting
below the method, by case, if its last six cases are ever wanted. They are not in the set.

**4. Section 5 mismatches:**
- **The excluded files outside the set number twenty-one, not twenty.** Unit 392's table has 22
  rows, 21 engine and 1 app, and removing `ABlipDoesNotShiftEverythingAfterItTests` leaves 21.
  All 21 are listed in the doc's section 4.
- **`PHASE_STATUS.md` read `CURRENT_STEP: 0` and `WORK_INSTRUCTION: 393`,** as stated, and is now
  3 and 394. Its `STEP: 0` and `STEP: 2` lines still read `not started`. They belong to the layer,
  and I did not edit them.
- **`PHASE_OUTCOME.md` holds step 0 and step 2 at `not started`** with every criterion `[x]`, and
  carries the paired `## UNIT 392` / `## UNIT 1` and `## UNIT 393` / `## UNIT 2` entries. As
  stated, reported, not edited. I committed it and `PHASE_STATUS.md` whole, as unit 393 did, so the
  layer's uncommitted lines rode in `02c64ae2`.
- **`docs\phase-cw\PHASE_PLAN.md` is a second copy that differs from the root `PHASE_PLAN.md`.**
  Units tick the root copy, and I did too. The instruction does not mention the second copy.
- **`SESSION.lock` is untracked at the root,** and the instruction does not list it. I left it as
  found.
- **`CLAUDE.md` §1's top row reads HM-DEC-167** at line 360. `PROJECT_STATUS.md` says HM-DEC-165
  because `tools/status.sh` writes it as a literal.
- **Held as stated:** HEAD `9fe8bab4`; 1.13.80 at line 1254; `PROJECT_STATUS.md` at unit 393,
  `COMPLETED`, `TASK 4 of 5`; the five root files and the three `tools\arbiter\` entries; the
  failing set's 51 lines in the fifteen rows as counted; `docs\carry-forward-tests.txt` at 918
  lines, line 7 with 65 terms, line 9 with 27 and `timeout 480`, the known-reds block at 153 with
  its CW lines at 158 and 159, `WHAT UNIT 393 ADDED` at 900; `ASpeedChangeInRealisticAudio` at
  line 41 of the set; 21 and 1 `<Compile Remove>`, 19 under `Cw\`; 6 fixture `.wav`s, the
  receiver tier, 49 unadjudicated captures; no `docs\cw-retired-tests.txt`; the Cw diff of 4
  files, 165 and 1; the transmit files silent; three preflight worktrees.

**5. The failing set is not all of its types' reds.** *A finding for step 3's scope.*
`CwFixtureTests.NothingTheDecoderWasSureOfIsWrong` is red on `fading-18wpm`, `noisy-18wpm` and
`interference-18wpm`, and none of the three is in `docs\unit239-failing-set.txt`. Criterion 3.1's
set does not reach them, so step 3 can close with them red unless the plan says otherwise.

**6. The headless dispatcher loop lost 4 names across the two exit app runs,** and none at entry.
*A finding, recorded and not chased (§6).* No name was lost twice.

**`validate-output.bat`:** not run. It asked for approval in earlier units. I checked this file
against its rules by hand: the ordering block and `UNIT:` above section 1; a `UNIT:` line with no
parentheses and none of `& | < > ^`; four sections in order with the canonical names; section 4
present.

**`git worktree list`:** the root and the three preflight trees, nothing else.
**`git diff --stat 02c64ae2 HEAD -- src` at the end:** prints nothing. **Push:** all four task
commits pushed without refusal. This report goes in a fifth commit.

### Asks still outstanding

**Carried per HM-DEC-139, verbatim: unit 393's five, unit 392's items 2 to 5, unit 391's items 2
to 6, and unit 390's nine. None is this unit's to answer.**

**Unit 393's five, verbatim:**

