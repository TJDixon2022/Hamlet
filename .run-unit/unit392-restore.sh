cd /c/Source/HamLet
C=src/Hamlet.RadioEngine/Cw
TX="$C/CwTransmitter.cs $C/KeyerCwSender.cs $C/TransmitChain.cs $C/AutoCall.cs $C/AutoCallAnswers.cs $C/CwTransmitGuard.cs $C/TransmissionWatch.cs $C/TransmitReadiness.cs $C/TransmitPrivileges.cs $C/TransmitNotes.cs $C/ICwSender.cs"
echo "== 1. transmit diff, must print nothing"
T=$(git diff --stat 7e209cb4 HEAD -- $TX)
if [ -n "$T" ]; then echo "TRANSMIT FILES DIFFER - STOP"; echo "$T"; exit 3; fi
echo "(nothing)"
git status --short -- src tests
echo "== 2. checkout 7e209cb4 -- $C"
git checkout 7e209cb4 -- $C
git status --short -- src tests
echo "== 3. git rm the HEAD-only files not kept"
for f in CwAccuracy CwPitchRanking CwProbabilisticDecoder.Posterior CwReferenceDecoder CwSpectralPeak CwSwingSurvey; do
  git rm -q "$C/$f.cs"
done
git status --short -- src tests
echo "== 4. diff --stat 7e209cb4 -- Cw, only the kept files"
git diff --stat 7e209cb4 -- $C
echo "== transmit files against HEAD"
git diff --stat HEAD -- $TX
echo "(end)"
