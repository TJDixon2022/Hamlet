cd /c/Source/HamLet
C=src/Hamlet.RadioEngine/Cw
echo "== CwProbabilisticResult:"; sed -n 35,50p $C/CwProbabilisticDecoder.cs
echo "== None:"; grep -n "static .*CwProbabilisticResult None" $C/CwProbabilisticDecoder.cs
echo "== CwDecoder.PitchWasAsserted:"; grep -n "PitchWasAsserted" $C/CwDecoder.cs
echo "== MorseAlphabet:"; grep -rln "class MorseAlphabet" src/Hamlet.RadioEngine --include=*.cs; grep -rn "Unreadable =" src/Hamlet.RadioEngine --include=*.cs | head -1
echo "== CwKeyingMeter Update:"; grep -n " Update(" $C/CwKeyingMeter.cs | head -2
echo "== CwToneTracker Process:"; grep -n "public .* Process(" -A2 $C/CwToneTracker.cs | head -4
echo "== CwToneTracker ToneHz:"; grep -n "public double ToneHz" $C/CwToneTracker.cs
echo "== LogLikelihoods sig:"; sed -n 901,903p $C/CwProbabilisticDecoder.cs
echo "== Decode 5-arg:"; sed -n 666,674p $C/CwProbabilisticDecoder.cs
echo "== CwProbabilisticStream ToneHz/Process:"; grep -nE "public double ToneHz|public void Process\(|public void Flush" $C/CwProbabilisticStream.cs
echo "== CwCharacter At/Text:"; grep -nE "TimeSpan At|string Text" $C/CwCharacter.cs | head -3
echo "== test helpers:"; grep -rn "class OneDecoderNotTwoTests\|Captures\b" tests/Hamlet.RadioEngine.Tests/Cw/OneDecoderNotTwoTests.cs | head -2; grep -n "RadioPitchHz\|static string Settled" tests/Hamlet.RadioEngine.Tests/Cw/TheAdjudicatedReadingsKeepReadingTests.cs | head -3
grep -n "OneDecoderNotTwoTests" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
echo "== reds section 4:"; grep -n "^## " docs/phase-cw/unit394-reds.md
