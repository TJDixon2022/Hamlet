cd /c/Source/HamLet
grep -n "Sensitivity\|AcquisitionWindow" docs/unit239-failing-set.txt
grep -n "Sensitivity" docs/phase-cw/unit394-reds.md | head -5 | cut -c1-300
for f in CwAcquisitionWindowTests CwSensitivityTests EveryCharacterCarriesItsOwnEvidenceTests WhereAcquisitionPointsTests CwRefusalFloorTableTests TheCleanSyntheticsFourWaysTests; do
  echo "== $f"
  grep -nE "Assert\.|CwDecodeHarness|Sweep|\[Fact|\[Theory" tests/Hamlet.RadioEngine.Tests/Cw/$f.cs | cut -c1-170
done
echo "== CwSensitivity.cs"; grep -nE "CwDecodeHarness|public static" tests/Hamlet.RadioEngine.Tests/Cw/CwSensitivity.cs
grep -rn "CwTwoInOnePassband\.\(Tracked\|Audio\|Alone\)" tests --include=*.cs | cut -c1-160
grep -rln "CwSensitivity\.Sweep" tests --include=*.cs
echo "== clean fixture names in callers"; grep -ln "clean-12wpm\|clean-18wpm" tests/Hamlet.RadioEngine.Tests/Cw/CwAcquisitionWindowTests.cs tests/Hamlet.RadioEngine.Tests/Cw/CwSensitivityTests.cs tests/Hamlet.RadioEngine.Tests/Cw/CwSensitivity.cs tests/Hamlet.RadioEngine.Tests/Cw/EveryCharacterCarriesItsOwnEvidenceTests.cs tests/Hamlet.RadioEngine.Tests/Cw/WhereAcquisitionPointsTests.cs tests/Hamlet.RadioEngine.Tests/Cw/CwRefusalFloorTableTests.cs; echo "end"
grep -n "TheDecoderReadsAsFarDown" .run-unit/unit399-carry-exit-eng.txt | head -3
