cd /c/Source/HamLet
echo "HEAD:"; git log --oneline -3 | cut -c1-100
echo "props 1254:"; sed -n 1254p Directory.Build.props
echo "status:"; git status --short -- . ":(exclude).run-unit" | head -20
echo "CLAUDE.md 360:"; sed -n 360p CLAUDE.md | cut -c1-120
echo "PARKED:"; grep -c "^## " docs/phase-cw/PARKED.md; grep -cE "^- " docs/phase-cw/PARKED.md; grep -nE "394 item 1|398 item" docs/phase-cw/PARKED.md | cut -c1-160
echo "retired lines:"; wc -l docs/cw-retired-tests.txt
echo "fixtures:"; ls tests/fixtures/cw
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
echo "Cw vs 7e209cb4:"; git diff --stat 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw; echo end
sh .run-unit/unit399-transmit.sh
echo "decision 5 diff: git diff --stat 7a297abf HEAD -- src tests docs/carry-forward-tests.txt"; git diff --stat 7a297abf HEAD -- src tests docs/carry-forward-tests.txt; echo "end decision 5 diff"
echo "printer:"; ls tests/Hamlet.RadioEngine.Tests/Cw/TheReworkNumbersPrinterTests.cs
echo "carry lines:"; sed -n 9p docs/carry-forward-tests.txt | grep -o "timeout [0-9]*"; sed -n 27p docs/carry-forward-tests.txt | cut -c1-80; sed -n 158,159p docs/carry-forward-tests.txt | cut -c1-100
echo "compile remove eng:"; grep -n "Compile Remove" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
echo "compile remove app:"; grep -n "Compile Remove" tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj
echo "CwFixtures readers:"; grep -rl "CwFixtures\." tests --include=*.cs
echo "decoder lines:"; grep -nE "Estimate\(|quarter <= 0|IsNaN\(sigma\)|RayleighQuarterPoint\(|PercentileOf\(|LogLikelihoods\(" src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs | cut -c1-140
echo "CwSignal 28 and 78:"; sed -n 28p src/Hamlet.RadioEngine/Training/CwSignal.cs | cut -c1-200; sed -n 78p src/Hamlet.RadioEngine/Training/CwSignal.cs
echo "worktrees:"; git worktree list
date +%H:%M:%S
