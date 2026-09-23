cd /c/Source/HamLet
F=tests/Hamlet.RadioEngine.Tests/Cw/ABlipDoesNotShiftEverythingAfterItTests.cs
echo "CwReferenceDecoder lines in the file:"; grep -n "CwReferenceDecoder" $F
echo "compiled engine tests using namespace Training:"; grep -rl "using Hamlet.RadioEngine.Training;" tests/Hamlet.RadioEngine.Tests --include=*.cs | grep -v ABlip | head -3
echo "compiled tests using CwSignal.DefaultToneHz:"; grep -rl "CwSignal.DefaultToneHz" tests/Hamlet.RadioEngine.Tests --include=*.cs | grep -v ABlip | head -3
echo "compiled tests using PumpAll:"; grep -rl "PumpAll()" tests/Hamlet.RadioEngine.Tests --include=*.cs | grep -v ABlip | head -2
echo "compiled tests using Reading.Text:"; grep -rl "Reading.Text" tests/Hamlet.RadioEngine.Tests --include=*.cs | grep -v ABlip | head -2
echo "file history:"; git log --oneline -3 -- $F
echo "csproj line:"; git log --oneline -1 -S "ABlipDoesNotShiftEverythingAfterItTests" -- tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
