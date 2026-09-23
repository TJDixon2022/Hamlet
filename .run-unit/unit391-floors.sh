#!/bin/sh
# Unit 391: one floor-test type per invocation, foregrounded, timeout 900, detailed console.
# Usage: sh .run-unit/unit391-floors.sh <root> <label> <n> "<note>"
#   n = 1 captures, 2 adjudicated, 3 clean synthetics
root=$1
label=$2
n=$3
note=$4
TASKLINE=${5:-TASK 1 of 4}
limit=${6:-900}
here=/c/Source/HamLet
case $n in
  1) filter="FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" ;;
  2) filter="FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" ;;
  3) filter="FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" ;;
esac
out=$here/.run-unit/unit391-floors-$label-$n.txt
[ -f $out ] && mv $out $out.prev
cd $here && sh tools/status.sh EXECUTING "$TASKLINE" code none "$note"
cd $root || exit 1
start=$(date +%s)
echo "== $label $filter started $(date "+%Y-%m-%dT%H:%M:%S%:z")" > $out
timeout $limit dotnet test tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj --filter "$filter" --logger "console;verbosity=detailed" >> $out 2>&1
rc=$?
echo "== rc=$rc seconds=$(( $(date +%s) - start ))" >> $out
grep -E "^(Passed!|Failed!)|Total tests|rc=" $out
grep -cE "^\s+Passed " $out
grep -cE "^\s+Failed " $out
