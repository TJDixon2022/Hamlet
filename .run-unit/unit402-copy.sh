cd /c/Source/HamLet
printf 'TASK 0 of 5\n' > .run-unit/unit402-task.txt
echo "PHASE_STATUS before:"; grep -nE "^(CURRENT_STEP|WORK_INSTRUCTION)" PHASE_STATUS.md
sed -i 's/^CURRENT_STEP: .*$/CURRENT_STEP: 3/' PHASE_STATUS.md
sed -i "s/^WORK_INSTRUCTION: .*$/WORK_INSTRUCTION: 402 - the eight reds, first attempt/" PHASE_STATUS.md
echo "PHASE_STATUS after:"; grep -nE "^(CURRENT_STEP|WORK_INSTRUCTION|STEP)" PHASE_STATUS.md
sh tools/status.sh EXECUTING "TASK 0 of 5" code none "task 0: PHASE_STATUS set to 402 and step 3; copying unit 401 scripts, entry diff next"
date "+%H:%M:%S start"
for f in floors carry build commit transmit cmp adjcmp nums types; do
  sed "s/unit401/unit402/g" .run-unit/unit401-$f.sh > .run-unit/unit402-$f.sh
done
ls .run-unit/unit402-*
echo "HEAD:"; git log --oneline -3
echo "props 1254:"; sed -n 1254p Directory.Build.props
echo "CLAUDE.md top rows:"; grep -nE "HM-DEC-16[5-9]|CPS-DEC" CLAUDE.md | head -4 | cut -c1-160
echo "PHASE_OUTCOME step lines:"; grep -nE "^STEP [0-9]|^## UNIT 40[01]|^## UNIT [0-9] " PHASE_OUTCOME.md | tail -12 | cut -c1-120
echo "decision 9 diff:"; git diff --stat 162f0259 HEAD -- src tests docs/carry-forward-tests.txt docs/unit239-failing-set.txt; echo end
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
sh .run-unit/unit402-transmit.sh
echo "set:"; wc -l docs/unit239-failing-set.txt; sed -n "6p;15p;24p;41p;42p;43p;44p;45p;52p" docs/unit239-failing-set.txt
echo "carry:"; wc -l docs/carry-forward-tests.txt
echo "PARKED:"; wc -l docs/phase-cw/PARKED.md; tail -3 docs/phase-cw/PARKED.md | cut -c1-200
ls docs/phase-cw/reds-3.6.md 2>&1
wc -l tests/Hamlet.RadioEngine.Tests/Cw/CwAcquisitionWindowTests.cs tests/Hamlet.RadioEngine.Tests/Cw/CwEmissionGateTests.cs tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/CwAdjudicationTests.cs tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/CwReceiverFixtureTests.cs src/Hamlet.RadioEngine/Cw/CwDecoder.cs tests/Hamlet.RadioEngine.Tests/Cw/TheDisplacementFloorFourWaysTests.cs
echo "worktrees:"; git worktree list
