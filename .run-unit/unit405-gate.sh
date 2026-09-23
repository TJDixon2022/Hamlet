cd /c/Source/HamLet
# Usage: sh .run-unit/unit405-gate.sh <tag> "<note prefix>" <acq|fix|adjt|gate|disp|capsig|silence|whygate|twostation>...
TAG=$1
NP=$2
shift 2
for W in "$@"
do
  case $W in
    acq) F="FullyQualifiedName~CwAcquisitionWindowTests" ;;
    fix) F="FullyQualifiedName~Hamlet.RadioEngine.Tests.Cw.CwFixtureTests" ;;
    adjt) F="FullyQualifiedName~CwAdjudicationTests" ;;
    gate) F="FullyQualifiedName~CwEmissionGateTests" ;;
    disp) F="FullyQualifiedName~CwDisplacementFloorTests" ;;
    capsig) F="FullyQualifiedName~CapturedSignalTests" ;;
    silence) F="FullyQualifiedName~CwSpeedSilenceTests" ;;
    whygate) F="FullyQualifiedName~WhyTheGateDidNotFireTests" ;;
    twostation) F="FullyQualifiedName~CwTwoStationTests" ;;
  esac
  echo "== $W"
  sh .run-unit/unit405-run.sh "$W-$TAG" engine 300 "TASK 2 of 4" "$NP: $W running" "$F" --no-build
  sh .run-unit/unit405-list.sh "$W-$TAG" "$W-entry" | tail -8
done
date +%H:%M:%S
