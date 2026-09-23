cd /c/Source/HamLet
echo "HEAD:"; git log --oneline -3
echo "status root:"; git status --short | grep -v "^?? .run-unit" | head -40
echo "PHASE_STATUS:"; grep -nE "^(CURRENT_STEP|WORK_INSTRUCTION|STEP):?" PHASE_STATUS.md | head -20
echo "props 1254:"; sed -n 1254p Directory.Build.props
echo "CLAUDE.md HM-DEC top:"; grep -n "HM-DEC-167" CLAUDE.md | head -3
echo "PHASE_OUTCOME state lines:"; grep -nE "^## |STATE" PHASE_OUTCOME.md | tail -30
echo "PARKED items:"; grep -cE "^- " docs/phase-cw/PARKED.md; grep -nE "^## |399 item|394 item 1" docs/phase-cw/PARKED.md | cut -c1-200
echo "retired:"; wc -l docs/cw-retired-tests.txt
echo "harness lines:"; wc -l tests/Hamlet.RadioEngine.Tests/Cw/CwDecodeHarness.cs
echo "decoder events:"; grep -nE "CharacterSettled|CharacterDecoded|event " src/Hamlet.RadioEngine/Cw/CwDecoder.cs
echo "app lines:"; sed -n "11117,11124p" src/Hamlet.App/ViewModels/MainWindowViewModel.cs
echo "app at 7e209cb4:"; git show 7e209cb4:src/Hamlet.App/ViewModels/MainWindowViewModel.cs | grep -n "CharacterSettled\|CharacterDecoded"
echo "callers:"; grep -rl "CwDecodeHarness\.Decode" tests --include=*.cs
echo "compile removes:"; grep -n "Compile Remove" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
echo "writers:"; grep -n "File.WriteAllText\|Assert\." tests/Hamlet.RadioEngine.Tests/Cw/TheCwBaselineTable.cs tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/TheTwoStationTable.cs tests/Hamlet.RadioEngine.Tests/Cw/TheIntegratorBandwidthTable.cs 2>&1 | head
echo "fixtures dir:"; ls tests/fixtures/cw
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
echo "Cw vs 7e209cb4:"; git diff --stat 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw; echo end
echo "decision 6 diff 80b1aa3e:"; git diff --stat 80b1aa3e HEAD -- src tests docs/carry-forward-tests.txt; echo end
sh .run-unit/unit400-transmit.sh
echo "carry lines:"; sed -n "7p;9p;158p;159p" docs/carry-forward-tests.txt | cut -c1-400
wc -l docs/unit239-failing-set.txt
echo "worktrees:"; git worktree list
