cd /c/Source/HamLet
C=src/Hamlet.RadioEngine/Cw
wc -l $C/CwAccuracy.cs $C/CwElementPitch.cs $C/CwJointCutter.cs $C/CwPitchChoice.cs $C/CwPitchRanking.cs $C/CwProbabilisticDecoder.Posterior.cs $C/CwReferenceDecoder.cs $C/CwSpectralPeak.cs $C/CwStreamSplit.cs $C/CwSwingSurvey.cs tools/Hamlet.PitchRank/Program.cs
echo "== adjudicated uses"
grep -n "CwSpectralPeak" tests/Hamlet.RadioEngine.Tests/Cw/TheAdjudicatedReadingsKeepReadingTests.cs
echo "== Envelope decl"
grep -rn "Envelope\b" $C/CwReferenceDecoder.cs $C/CwProbabilisticDecoder.cs | grep -E "record|class|struct|static .*Envelope\(" | head
echo "== app Envelope"
grep -n "Envelope" src/Hamlet.App/ViewModels/MainWindowViewModel.cs | head
echo "== adjudicated diff vs 7e209cb4"
git diff --stat 7e209cb4 HEAD -- tests/Hamlet.RadioEngine.Tests/Cw/TheAdjudicatedReadingsKeepReadingTests.cs tests/Hamlet.RadioEngine.Tests/Cw/TheCapturesThatDecodeKeepDecodingTests.cs tests/Hamlet.RadioEngine.Tests/Cw/Fixtures
echo "== PitchRank csproj"
cat tools/Hamlet.PitchRank/Hamlet.PitchRank.csproj
