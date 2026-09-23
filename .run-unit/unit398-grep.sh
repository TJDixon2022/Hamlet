cd /c/Source/HamLet
C=src/Hamlet.RadioEngine/Cw
for n in CwAccuracy CwSpectralPeak CwSwingSurvey CwStreamSplit CwElementPitch CwPitchRanking CwReferenceDecoder LogSum ReReads WindowSizings ProcessDelayForTests RankThePitch FittedLogLikelihoods AssertAt AssertStation CoarseSpacingHz PitchWasAsserted PitchWasMeasured PitchChoice StrongestBin Reading Posterior Envelope LogLikelihoods HopMilliseconds DecodeUngated Gate CharacterMargin NoiseSpanSeconds WindowSeconds ReadEverySeconds SlowestWpm FastestWpm CwProbabilisticStream CharacterSettled LeadingEdgeChanged Last LikelihoodRatio WordsPerMinute CwElement IsMark StartHop PitchHz Pattern DecodeQueueDroppedChunks DecodeQueueDroppedSamples Tap Listen Flush Retuned Report Tracker HopSamples IsLocked Ranked Rankings Rank HasKeying HasTone SnrDb CwUnitEstimator MinimumToneHz MaximumToneHz HasMeasuredPitch LockedToneHz Unlock Stream CwKeyingMeter CwDecodeReport ElementsResolved CharactersEmitted CharactersUnsure ElementsSeen; do
  hits=$(grep -rnw "$n" $C | grep -vE "^[^:]+:[0-9]+:\s*(///|//)" | grep -E "(public|internal|private|protected)[^=;(]*\b$n\b|(class|record|struct|enum) $n\b|\b$n\(" | head -2 | cut -c1-170)
  echo "== $n"
  if [ -z "$hits" ]; then echo "   (nothing)"; else echo "$hits" | sed "s/^/   /"; fi
done
echo "== outside Cw, the non-Cw names"
for n in ReusableWindow Sizings SecondsKept ArrivalRatio PumpAll BufferedAudioSource KeyingEnvelope; do
  echo "== $n: $(grep -rlw "$n" src | head -3 | tr "\n" " ")"
done
