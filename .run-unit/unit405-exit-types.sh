cd /c/Source/HamLet
# unit 405 task 4 - the exit round's types, one dotnet test invocation each, each diffed against entry.
for W in acq recv fix adjt gate disp capsig silence whygate twostation
do
  case $W in
    acq) F="FullyQualifiedName~CwAcquisitionWindowTests" ;;
    recv) F="FullyQualifiedName~CwReceiverFixtureTests" ;;
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
  sh .run-unit/unit405-run.sh "$W-exit" engine 300 "TASK 4 of 4" "Task 4 exit round: floors green as entry; type $W running" "$F" --no-build
  sh .run-unit/unit405-list.sh "$W-exit" "$W-entry" | tail -6
done
date +%H:%M:%S
