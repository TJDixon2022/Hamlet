cd /c/Source/HamLet
sed "s/unit400/unit401/g" .run-unit/unit400-nums.sh > .run-unit/unit401-nums.sh
echo "HEAD:"; git log --oneline -2
echo "props 1254:"; sed -n 1254p Directory.Build.props
echo "CLAUDE.md top row:"; grep -n "HM-DEC-167" CLAUDE.md | head -2 | cut -c1-120
echo "PHASE_OUTCOME headings tail:"; grep -nE "^## |^ADVANCED|^ATTEMPT" PHASE_OUTCOME.md | tail -8 | cut -c1-160
echo "PARKED items:"; grep -cE "^- " docs/phase-cw/PARKED.md; grep -nE "^## " docs/phase-cw/PARKED.md
echo "retired:"; wc -l docs/cw-retired-tests.txt
echo "carry lines:"; wc -l docs/carry-forward-tests.txt; sed -n "158p;159p;909p;910p" docs/carry-forward-tests.txt
echo "set:"; wc -l docs/unit239-failing-set.txt; sed -n 41p docs/unit239-failing-set.txt
echo "displacement lines:"; wc -l tests/Hamlet.RadioEngine.Tests/Cw/CwDisplacementFloorTests.cs; sed -n 45p tests/Hamlet.RadioEngine.Tests/Cw/CwDisplacementFloorTests.cs
echo "decision 6 diff:"; git diff --stat 5e70860d HEAD -- src tests docs/carry-forward-tests.txt docs/unit239-failing-set.txt; echo end
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
sh .run-unit/unit401-transmit.sh
echo "grep Cw CW carry:"; grep -n "Cw\|CW" docs/carry-forward-tests.txt | cut -c1-200
echo "grep set readers:"; grep -rl unit239-failing-set tools .claude; echo "rc $?"
echo "CharacterDecoded:"; grep -n CharacterDecoded tests/Hamlet.RadioEngine.Tests/Cw/*.cs
echo "compile removes:"; grep -n "Compile Remove" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
echo "worktrees:"; git worktree list
