cd /c/Source/HamLet
echo "HEAD:"; git log --oneline -3 | cut -c1-100
echo "props 1254:"; sed -n 1254p Directory.Build.props
echo "PROJECT_STATUS at HEAD:"; git show HEAD:PROJECT_STATUS.md | head -6
echo "PHASE_STATUS working:"; grep -E "CURRENT_STEP|WORK_INSTRUCTION|^STEP" PHASE_STATUS.md | cut -c1-40
echo "PHASE_OUTCOME units:"; grep -nE "^## UNIT" PHASE_OUTCOME.md | tail -14
echo "status:"; git status --short -- . ":(exclude).run-unit" | head -20
echo "CLAUDE.md 360:"; sed -n 360p CLAUDE.md | cut -c1-120
echo "PARKED:"; grep -c "^## " docs/phase-cw/PARKED.md; grep -cE "^- " docs/phase-cw/PARKED.md; grep -E "397 item" docs/phase-cw/PARKED.md | cut -c1-120
echo "retired exists?"; ls docs/cw-retired-tests.txt
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
echo "Cw vs 7e209cb4:"; git diff --stat 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw; echo end
sh .run-unit/unit398-transmit.sh
echo "decision 4 diff: git diff --stat 3af36501 HEAD -- src tests docs/carry-forward-tests.txt"; git diff --stat 3af36501 HEAD -- src tests docs/carry-forward-tests.txt; echo "end decision 4 diff"
echo "printer:"; ls tests/Hamlet.RadioEngine.Tests/Cw/TheReworkNumbersPrinterTests.cs
sed -n 107,108p tests/Hamlet.RadioEngine.Tests/Cw/TheCapturesThatDecodeKeepDecodingTests.cs
echo "carry lines:"; sed -n 9p docs/carry-forward-tests.txt | grep -o "timeout [0-9]*"; sed -n 27p docs/carry-forward-tests.txt | cut -c1-80; sed -n 158,159p docs/carry-forward-tests.txt | cut -c1-100
echo "compile remove eng:"; grep -n "Compile Remove" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
echo "compile remove app:"; grep -n "Compile Remove" tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj
echo "Cw file count:"; ls src/Hamlet.RadioEngine/Cw | wc -l
echo "worktrees:"; git worktree list
