cd /c/Source/HamLet
echo "HEAD:"; git log --oneline -3
echo "props 1254:"; sed -n 1254p Directory.Build.props
echo "PROJECT_STATUS at HEAD:"; git show HEAD:PROJECT_STATUS.md | head -10
echo "PHASE_STATUS at HEAD:"; git show HEAD:PHASE_STATUS.md | grep -E "CURRENT_STEP|WORK_INSTRUCTION|^STEP"
echo "PHASE_OUTCOME steps:"; grep -nE "^STEP|^## UNIT|^STATE" PHASE_OUTCOME.md | tail -30
echo "status:"; git status --short -- . ":(exclude).run-unit" | head -20
echo "CLAUDE.md 360:"; sed -n 360p CLAUDE.md | cut -c1-120
echo "PARKED lines:"; wc -l < docs/phase-cw/PARKED.md; grep -c "^## " docs/phase-cw/PARKED.md; grep -cE "^- " docs/phase-cw/PARKED.md; grep -E "396 item" docs/phase-cw/PARKED.md | cut -c1-200
echo "retired exists?"; ls docs/cw-retired-tests.txt
echo "unit395-rework lines:"; wc -l < docs/phase-cw/unit395-rework.md; grep -n "^#" docs/phase-cw/unit395-rework.md
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
echo "Cw vs 7e209cb4:"; git diff --stat 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw; echo end
sh .run-unit/unit397-transmit.sh
echo "decision 20 diff:"; git diff --stat e2b31a40 HEAD -- src tests docs/carry-forward-tests.txt; echo "end decision 20 diff"
echo "printer:"; ls tests/Hamlet.RadioEngine.Tests/Cw/TheReworkNumbersPrinterTests.cs
sed -n 107,108p tests/Hamlet.RadioEngine.Tests/Cw/TheCapturesThatDecodeKeepDecodingTests.cs
echo "carry lines:"; wc -l < docs/carry-forward-tests.txt; sed -n 9p docs/carry-forward-tests.txt | grep -o "timeout [0-9]*"; sed -n 27p docs/carry-forward-tests.txt | cut -c1-80; sed -n 158,159p docs/carry-forward-tests.txt | cut -c1-100
echo "compile remove:"; grep -c "Compile Remove" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
echo "worktrees:"; git worktree list
