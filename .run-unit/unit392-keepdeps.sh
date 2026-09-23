cd /c/Source/HamLet
echo "== 7e209cb4 files naming a HEAD-only type"
grep -lwE "CwAccuracy|CwElementPitch|CwJointCutter|CwElement|CwPitchChoice|CwPitchRanking|CwPitchRank|CwReferenceDecoder|CwSpectralPeak|CwStreamSplit|CwSwingSurvey|CwStreamDivision" .run-unit/at7e/*.cs
echo "grep exit $?"
echo "== keep-candidate dependencies"
C=src/Hamlet.RadioEngine/Cw
for f in CwElementPitch CwStreamSplit CwPitchChoice CwJointCutter CwSpectralPeak; do
  echo "-- $f"
  grep -v "^ *///" $C/$f.cs | grep -oE "Cw[A-Z][A-Za-z]*(\.[A-Z][A-Za-z]*)?|KeyingEnvelope(\.[A-Z][A-Za-z]*)?|AudioTap(\.[A-Z][A-Za-z]*)?" | sort -u | tr "\n" " "
  echo
done
