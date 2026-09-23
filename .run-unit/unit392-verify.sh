cd /c/Source/HamLet
TX="src/Hamlet.RadioEngine/Cw/CwTransmitter.cs src/Hamlet.RadioEngine/Cw/KeyerCwSender.cs src/Hamlet.RadioEngine/Cw/TransmitChain.cs src/Hamlet.RadioEngine/Cw/AutoCall.cs src/Hamlet.RadioEngine/Cw/AutoCallAnswers.cs src/Hamlet.RadioEngine/Cw/CwTransmitGuard.cs src/Hamlet.RadioEngine/Cw/TransmissionWatch.cs src/Hamlet.RadioEngine/Cw/TransmitReadiness.cs src/Hamlet.RadioEngine/Cw/TransmitPrivileges.cs src/Hamlet.RadioEngine/Cw/TransmitNotes.cs src/Hamlet.RadioEngine/Cw/ICwSender.cs"
echo "== HEAD"; git rev-parse --short HEAD
echo "== version"; grep -i "version" Directory.Build.props
echo "== diff 3d6a2c12 src tests"; git diff --stat 3d6a2c12 HEAD -- src tests
echo "== log count 7e209cb4..HEAD Cw"; git log --format="%h %ad %s" --date=short 7e209cb4..HEAD -- src/Hamlet.RadioEngine/Cw | wc -l
git log --format="%h %ad" --date=short 7e209cb4..HEAD -- src/Hamlet.RadioEngine/Cw | head -1
git log --format="%h %ad" --date=short 7e209cb4..HEAD -- src/Hamlet.RadioEngine/Cw | tail -1
echo "== diff stat Cw"; git diff --stat 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw | tail -1
echo "== name-status Cw"; git diff --name-status 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw
echo "== transmit files exist at HEAD"; for f in $TX; do if test -e "$f"; then echo "present $f"; else echo "MISSING $f"; fi; done
echo "== transmit diff"; git diff --stat 7e209cb4 HEAD -- $TX
echo "== transmit diff end"
echo "== engine test Cw diff"; git diff --stat 7e209cb4 HEAD -- tests/Hamlet.RadioEngine.Tests/Cw | tail -1
git diff --name-status 7e209cb4 HEAD -- tests/Hamlet.RadioEngine.Tests/Cw
echo "== app tests"; for f in tests/Hamlet.App.Tests/Cw/TheSheetSaysWhatEachElementWasSentAtTests.cs tests/Hamlet.App.Tests/Views/ReturningToCwShowsCwTests.cs tests/Hamlet.App.Tests/Views/BindingHealthTests.cs tests/Hamlet.App.Tests/VoiceTests.cs; do if test -e "$f"; then echo "present $f"; else echo "MISSING $f"; fi; done
echo "== unit239"; wc -l docs/unit239-failing-set.txt
if test -e docs/cw-retired-tests.txt; then echo "retired EXISTS"; else echo "retired absent"; fi
echo "== worktrees"; git worktree list
echo "== sln"; dotnet sln Hamlet.sln list
echo "== carry"; cat docs/carry-forward-tests.txt
