cd /c/Source/HamLet
# Usage: sh .run-unit/unit402-six.sh <tag> "TASK n of 5" "<note prefix>" [acq gate adj recv fixtures disp ...]
TAG=$1
TK=$2
NP=$3
shift 3
for W in "$@"; do
  case $W in
    acq) F="FullyQualifiedName~Hamlet.RadioEngine.Tests.Cw.CwAcquisitionWindowTests"; T=600 ;;
    gate) F="FullyQualifiedName~CwEmissionGateTests"; T=300 ;;
    adj) F="FullyQualifiedName~Hamlet.RadioEngine.Tests.Cw.Fixtures.CwAdjudicationTests"; T=600 ;;
    recv) F="FullyQualifiedName~Hamlet.RadioEngine.Tests.Cw.Fixtures.CwReceiverFixtureTests"; T=600 ;;
    fixtures) F="FullyQualifiedName~Hamlet.RadioEngine.Tests.Cw.CwFixtureTests"; T=600 ;;
    disp) F="FullyQualifiedName~CwDisplacementFloorTests"; T=300 ;;
    cap) F="FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests"; T=600 ;;
    adjud) F="FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests"; T=600 ;;
    capsig) F="FullyQualifiedName~Hamlet.RadioEngine.Tests.Cw.CapturedSignalTests"; T=600 ;;
    silence) F="FullyQualifiedName~Hamlet.RadioEngine.Tests.Cw.CwSpeedSilenceTests"; T=600 ;;
    whygate) F="FullyQualifiedName~Hamlet.RadioEngine.Tests.Cw.WhyTheGateDidNotFireTests"; T=600 ;;
    twostation) F="FullyQualifiedName~Hamlet.RadioEngine.Tests.Cw.Fixtures.CwTwoStationTests"; T=600 ;;
    syn) F="FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly"; T=300 ;;
  esac
  echo "=== $W"
  sh .run-unit/unit402-floors.sh $W-$TAG $T "$F" "$TK" "$NP $W running" --no-build
  grep -E "^\s+(Passed|Failed) " .run-unit/unit402-$W-$TAG.txt | sed -E "s/^\s+//" | sed -E "s/Hamlet.RadioEngine.Tests.Cw.//" | sed -E "s/ \[.*//" | sort > .run-unit/unit402-$W-$TAG-list.txt
done
date +%H:%M:%S
