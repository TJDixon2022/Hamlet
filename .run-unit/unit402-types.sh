cd /c/Source/HamLet
# Usage: sh .run-unit/unit402-types.sh <tag> "TASK n of 5" "<note prefix>" [fixtures|disp|gate ...]
TAG=$1
TK=$2
NP=$3
shift 3
for W in "$@"; do
  case $W in
    fixtures) sh .run-unit/unit402-floors.sh fixtures-$TAG 600 "FullyQualifiedName~Hamlet.RadioEngine.Tests.Cw.CwFixtureTests" "$TK" "$NP CwFixtureTests whole running" --no-build
              grep -E "^\s+(Passed|Failed) " .run-unit/unit402-fixtures-$TAG.txt | sed -E "s/^\s+//" | sed -E "s/Hamlet.RadioEngine.Tests.Cw.//" | sed -E "s/ \[.*//" | sort > .run-unit/unit402-fixtures-$TAG-list.txt ;;
    disp) sh .run-unit/unit402-floors.sh disp-$TAG 300 "FullyQualifiedName~CwDisplacementFloorTests" "$TK" "$NP CwDisplacementFloorTests running" --no-build
          grep -E "^\s+(Passed|Failed) |Assert|Expected|Actual|Values differ|not found|Retune" .run-unit/unit402-disp-$TAG.txt | cut -c1-240 ;;
    gate) sh .run-unit/unit402-floors.sh gate-$TAG 300 "FullyQualifiedName~CwEmissionGateTests" "$TK" "$NP CwEmissionGateTests running" --no-build
          grep -E "^\s+(Passed|Failed) " .run-unit/unit402-gate-$TAG.txt | sed -E "s/^\s+//" | sed -E "s/ \[.*//" | sort > .run-unit/unit402-gate-$TAG-list.txt ;;
  esac
done
date +%H:%M:%S
