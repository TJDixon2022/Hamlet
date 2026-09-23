cd /c/Source/HamLet
echo "HEAD $(git rev-parse --short HEAD)"
grep -n "<Version>" Directory.Build.props
grep -nE "^(STATE|TASK|WORK_INSTRUCTION):" PROJECT_STATUS.md
grep -nE "^(CURRENT_STEP|WORK_INSTRUCTION):" PHASE_STATUS.md
grep -n "^STEP: 0" PHASE_OUTCOME.md
grep -n "^## UNIT" PHASE_OUTCOME.md
git status --short -- tools/arbiter PHASE_OUTCOME.md PHASE_STATUS.md PROJECT_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md
echo "--- CLAUDE.md s1 top row ref"
grep -n -m1 "^| 2026" CLAUDE.md | grep -oE "\| [A-Z]+-DEC-[0-9]+ \|$"
echo "--- OliviaRows 279 306 674-675"
sed -n "279p;306p;674,675p" tests/Hamlet.App.Tests/ViewModels/TheOliviaRowsTests.cs
echo "--- JsonlTelemetry"
grep -n "BlockingCollection\|new Thread\|AppendAllText\|FileShare\|void Dispose\|Join" src/Hamlet.RadioEngine/Telemetry/JsonlTelemetry.cs
echo "--- siblings"
grep -n "Lines(" tests/Hamlet.App.Tests/ViewModels/TheOliviaMoveUpTests.cs | head -20
grep -rn "File.ReadAllLines" tests/Hamlet.App.Tests --include=TheOliviaExportSaysOliviaTests.cs
grep -c "TheOliviaMoveUpTests\|TheOliviaExportSaysOliviaTests" docs/carry-forward-tests.txt
echo "--- carry file"
wc -l docs/carry-forward-tests.txt
sed -n 7p docs/carry-forward-tests.txt | grep -o "FullyQualifiedName~" | wc -l
sed -n 9p docs/carry-forward-tests.txt | grep -o "FullyQualifiedName~" | wc -l
sed -n 9p docs/carry-forward-tests.txt | grep -o "timeout [0-9]*"
grep -n "TheOliviaRowsTests" docs/carry-forward-tests.txt | cut -c1-80
grep -n "ASpeedChangeInRealisticAudio\|51 CW cases" docs/carry-forward-tests.txt
echo "--- floors-1 0825"
grep -E "Passed .*cw-2026-08-25" .run-unit/unit392-floors-1.txt | sed -E "s/^\s+//" | cut -c1-200
grep -cE "Passed .*cw-2026-08-25" .run-unit/unit392-floors-1.txt
echo "--- CpuMeasuredAlone"
grep -rn "CpuMeasuredAlone" tests/Hamlet.RadioEngine.Tests | head
echo "--- diff stat Cw"
git diff --stat 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw
echo "--- transmit"
git diff --stat 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw/CwTransmitter.cs src/Hamlet.RadioEngine/Cw/KeyerCwSender.cs src/Hamlet.RadioEngine/Cw/TransmitChain.cs src/Hamlet.RadioEngine/Cw/AutoCall.cs src/Hamlet.RadioEngine/Cw/AutoCallAnswers.cs src/Hamlet.RadioEngine/Cw/CwTransmitGuard.cs src/Hamlet.RadioEngine/Cw/TransmissionWatch.cs src/Hamlet.RadioEngine/Cw/TransmitReadiness.cs src/Hamlet.RadioEngine/Cw/TransmitPrivileges.cs src/Hamlet.RadioEngine/Cw/TransmitNotes.cs src/Hamlet.RadioEngine/Cw/ICwSender.cs
echo "transmit diff printed above if any"
ls src/Hamlet.RadioEngine/Cw/ | grep -E "Transmit|Keyer|AutoCall|ICwSender"
echo "--- compile remove"
grep -c "Compile Remove" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj
wc -l docs/unit239-failing-set.txt
ls docs/cw-retired-tests.txt
git worktree list
