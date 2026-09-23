cd /c/Source/HamLet
# Usage: sh .run-unit/unit400-callers.sh <entry|after|exit> "TASK n of 5" "<note prefix>"
# The five asserting caller types of decision 4, --no-build, one invocation each, timeout 600.
for T in CwAcquisitionWindowTests CwSensitivityTests EveryCharacterCarriesItsOwnEvidenceTests WhereAcquisitionPointsTests CwRefusalFloorTableTests; do
  echo "=== $T"
  sh .run-unit/unit400-floors.sh callers-$1-$T 600 "FullyQualifiedName~$T" "$2" "$3 $T running" --no-build > /dev/null 2>&1
  grep -E "Total tests|Passed:|Failed:|Total time|Test Run Aborted|active test run was aborted" .run-unit/unit400-callers-$1-$T.txt
  grep -E "^\s+(Passed|Failed) " .run-unit/unit400-callers-$1-$T.txt | sed -E "s/^\s+//" | sed -E "s/Hamlet.RadioEngine.Tests.Cw.//" | sed -E "s/ \[[0-9.]+ m?s\]//" | sort | cut -c1-240
done
date +%H:%M:%S
