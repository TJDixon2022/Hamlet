cd /c/Source/HamLet
A=.run-unit/at7e
H=.run-unit/athead
mkdir -p $H
for f in CwDecoder CwProbabilisticDecoder CwCharacter CwDecodeReport CwProbabilisticStream CwKeyingMeter; do
  git show "HEAD:src/Hamlet.RadioEngine/Cw/$f.cs" > $H/$f.cs
done
echo "== HEAD Elements in decoder"
grep -n "Elements" $H/CwProbabilisticDecoder.cs | head -20
echo "== HEAD Decode signatures"
grep -n "public static CwProbabilisticResult Decode\|internal static CwProbabilisticResult Decode\|static CwProbabilisticResult Decode" $H/CwProbabilisticDecoder.cs
echo "== 7e Decode signatures"
grep -n "static CwProbabilisticResult Decode\|private static.*Decode(" $A/CwProbabilisticDecoder.cs
echo "== 7e CwProbabilisticResult record"
grep -n "record CwProbabilisticResult" -A 30 $A/CwProbabilisticDecoder.cs | grep -v "///" | head -30
echo "== HEAD CwProbabilisticResult record"
grep -n "record CwProbabilisticResult" -A 40 $H/CwProbabilisticDecoder.cs | grep -v "///" | head -40
echo "== HEAD CwCharacter WidestRecordedLlr MarginLlr MarginShareForRecord"
grep -n "WidestRecordedLlr\|MarginLlr\|MarginShareForRecord" $H/CwCharacter.cs
echo "== 7e CwCharacter Margin"
grep -n "Margin\|Llr\|LogLikelihood" $A/CwCharacter.cs | head
echo "== HEAD CwPitchChoice"
grep -n "^    [A-Z][A-Za-z]*,\?$" src/Hamlet.RadioEngine/Cw/CwPitchChoice.cs
echo "== HEAD report PitchChoice/Rank/PitchWasAsserted"
grep -n "PitchChoice\|CwPitchRank\|PitchWasAsserted" $H/CwDecodeReport.cs
echo "== 7e report tone"
grep -n "PitchWasMeasured\|ToneHz\|HasTone" $A/CwDecodeReport.cs | head
echo "== HEAD Retuned, Queue, UseJointCutter"
grep -n "public void Retuned\|DecodeQueueDropped\|UseJointCutter\|ProcessDelayForTests" $H/CwDecoder.cs | head
echo "== 7e reset-like"
grep -n "public void [A-Z]" $A/CwDecoder.cs
echo "== app UseJointDecoder"
grep -rn "UseJointDecoder" src/Hamlet.App | head
echo "== AudioArrival dropped"
grep -rn "DroppedChunks\|DroppedSamples\|DecodeQueueDropped" src/Hamlet.App src/Hamlet.RadioEngine --include=*.cs | grep -v "Cw/CwDecoder.cs" | head -20
echo "== SamplesSeen stream"
grep -n "SamplesSeen" $H/CwProbabilisticStream.cs $A/CwProbabilisticStream.cs | head
