cd /c/Source/HamLet
echo "== HEAD"; git log --oneline -2
echo "== props 1254"; sed -n 1254p Directory.Build.props
echo "== PHASE_OUTCOME states"; grep -nE "^## |^STATE" PHASE_OUTCOME.md | tail -30
echo "== status short root"; git status --short | grep -v "\.run-unit"
echo "== CLAUDE.md 360"; sed -n 360p CLAUDE.md | cut -c1-160
echo "== reload"; grep -o "CPS-DEC-[0-9]*" .run-unit/reload.txt | head -2
echo "== PARKED / retired"; ls docs/phase-cw/PARKED.md docs/cw-retired-tests.txt 2>&1; wc -l docs/phase-cw/PARKED.md; grep -c "^## " docs/phase-cw/PARKED.md; grep -cE "^- |^[0-9]+\. " docs/phase-cw/PARKED.md
echo "== rework doc headings"; grep -nE "^#" docs/phase-cw/unit395-rework.md
echo "== src vs 5688a8a5"; git diff --stat 5688a8a5 HEAD -- src; echo "end"
echo "== Cw vs 7e209cb4"; git diff --stat 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw; echo "end"
echo "== decision 12"; git diff --stat 259eba0a HEAD -- src tests docs/carry-forward-tests.txt; echo "end decision 12"
sh .run-unit/unit396-transmit.sh
echo "== printer"; ls tests/Hamlet.RadioEngine.Tests/Cw/TheReworkNumbersPrinterTests.cs; grep -c "\[Fact" tests/Hamlet.RadioEngine.Tests/Cw/TheReworkNumbersPrinterTests.cs
echo "== floors lines"; sed -n "107,108p" tests/Hamlet.RadioEngine.Tests/Cw/TheCapturesThatDecodeKeepDecodingTests.cs
echo "== carry lines"; wc -l docs/carry-forward-tests.txt; sed -n "9p;27p;158p;159p" docs/carry-forward-tests.txt | cut -c1-200; sed -n 9p docs/carry-forward-tests.txt | grep -o "timeout [0-9]*"
echo "== compile remove"; grep -c "Compile Remove" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
echo "== worktrees"; git worktree list
echo "== second plan"; ls docs/phase-cw/PHASE_PLAN.md
