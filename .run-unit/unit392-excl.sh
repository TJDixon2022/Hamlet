cd /c/Source/HamLet
# For each excluded file: its declared test class, and whether that name is in docs/unit239-failing-set.txt.
for p in $(grep -ohE "Compile Remove=\"[^\"]+\"" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj | sed "s/Compile Remove=//; s/\"//g"); do
  f="tests/Hamlet.RadioEngine.Tests/$(echo $p | tr "\\\\" "/")"
  cls=$(basename "$f" .cs)
  hit=$(grep -c "$cls" docs/unit239-failing-set.txt)
  echo "ENG $cls | in unit239: $hit"
done
for p in $(grep -ohE "Compile Remove=\"[^\"]+\"" tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj | sed "s/Compile Remove=//; s/\"//g"); do
  cls=$(basename "$(echo $p | tr "\\\\" "/")" .cs)
  hit=$(grep -c "$cls" docs/unit239-failing-set.txt)
  echo "APP $cls | in unit239: $hit"
done
echo "== carry-forward mentions"
for c in TheReadPathDoesNotAllocateTests TheTapIsNotBehindTheDecoderTests ThePitchControlsAreOffThePanelTests; do grep -c "$c" docs/carry-forward-tests.txt; done
C=src/Hamlet.RadioEngine/Cw
TX="$C/CwTransmitter.cs $C/KeyerCwSender.cs $C/TransmitChain.cs $C/AutoCall.cs $C/AutoCallAnswers.cs $C/CwTransmitGuard.cs $C/TransmissionWatch.cs $C/TransmitReadiness.cs $C/TransmitPrivileges.cs $C/TransmitNotes.cs $C/ICwSender.cs"
echo "== transmit vs HEAD and vs 7e209cb4"
git diff --stat HEAD -- $TX
git diff --stat 7e209cb4 -- $TX
echo "(end)"
