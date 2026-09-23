cd /c/Source/HamLet
C=src/Hamlet.RadioEngine/Cw
for n in CwAccuracy CwSpectralPeak CwSwingSurvey CwStreamSplit CwElementPitch CwPitchRanking CwReferenceDecoder LogSum ReReads WindowSizings ProcessDelayForTests RankThePitch FittedLogLikelihoods AssertAt AssertStation Posterior PitchWasMeasured StrongestBin LikelihoodRatio CwElement IsMark StartHop PitchHz Pattern Ranked Rankings Rank HasTone ElementsResolved CharactersEmitted CharactersUnsure ElementsSeen Characters Text Elements; do
  c=$(grep -rnw "$n" $C | wc -l)
  echo "== $n: $c hits under Cw"
  grep -rnw "$n" $C | grep -vE ":\s*///" | head -2 | cut -c1-170 | sed "s/^/   /"
done
echo "== CwDecoder Flush and Process:"; grep -nE "public void (Flush|Process)\(" $C/CwDecoder.cs
echo "== CwProbabilisticDecoder Decode signatures:"; grep -nE "public static CwProbabilisticResult Decode\(" -A3 $C/CwProbabilisticDecoder.cs | cut -c1-150
echo "== CwProbabilisticResult decl:"; grep -rn "record.*CwProbabilisticResult\|class CwProbabilisticResult" $C | cut -c1-200
echo "== CwDecodeReport decl:"; sed -n 47,70p $C/CwDecodeReport.cs
echo "== CwUnitEstimator.Elements:"; grep -n "Elements(" $C/CwUnitEstimator.cs | head -3
echo "== CwKeyingMeter Update:"; grep -n "public void Update" $C/CwKeyingMeter.cs
echo "== Audio names, src/Hamlet.RadioEngine only, cs files:"
for n in ReusableWindow Sizings SecondsKept ArrivalRatio PumpAll BufferedAudioSource KeyingEnvelope; do
  echo "   $n: $(grep -rlw --include=*.cs "$n" src/Hamlet.RadioEngine | head -3 | tr "\n" " ")"
done
grep -rn "public int Sizings\|public .* Sizings" --include=*.cs src/Hamlet.RadioEngine | head -2
grep -rn "ArrivalRatio(" --include=*.cs src/Hamlet.RadioEngine/Audio | head -1
