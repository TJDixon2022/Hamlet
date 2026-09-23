#!/bin/sh
# Unit 391 task 3: one candidate commit in a detached worktree, the floor types as that commit has them.
# Usage: sh .run-unit/unit391-walk.sh <hash> "<types in order, e.g. 3 2 1>" <stop-at-first-red: yes|no> "<note prefix>"
hash=$1
types=$2
stop=$3
prefix=$4
here=/c/Source/HamLet
wt=C:/Source/HamLet-wt391
cd $here || exit 1
git worktree remove --force $wt 2>/dev/null
git worktree add --detach $wt $hash > .run-unit/unit391-wt-$hash.txt 2>&1 || { echo "worktree add failed"; cat .run-unit/unit391-wt-$hash.txt; exit 1; }
d=tests/Hamlet.RadioEngine.Tests/Cw
for n in $types; do
  case $n in
    1) probe="$d/TheCapturesThatDecodeKeepDecodingTests.cs" ; word="class TheCapturesThatDecodeKeepDecodingTests" ;;
    2) probe="$d/TheAdjudicatedReadingsKeepReadingTests.cs" ; word="class TheAdjudicatedReadingsKeepReadingTests" ;;
    3) probe="$d/CwFixtureTests.cs" ; word="TheCleanRecordingsDecodeExactly" ;;
  esac
  if ! grep -q "$word" "$wt/$probe" 2>/dev/null; then
    echo "type $n: NOT PRESENT at $hash - counts green"
    continue
  fi
  sh .run-unit/unit391-floors.sh $wt $hash $n "$prefix: type $n at $hash running now" "TASK 3 of 4" > .run-unit/unit391-walk-last.txt
  f=.run-unit/unit391-floors-$hash-$n.txt
  passed=$(grep -cE "^\s+Passed " $f)
  failed=$(grep -cE "^\s+Failed " $f)
  summary=$(grep -E "^(Passed!|Failed!)" $f)
  if [ "$passed" = "0" ] && [ "$failed" = "0" ]; then
    echo "type $n: LOST RUN at $hash, re-running once"
    mv $f .run-unit/unit391-floors-$hash-$n-lost.txt
    sh .run-unit/unit391-floors.sh $wt $hash $n "$prefix: type $n at $hash re-run after a lost run" "TASK 3 of 4" > .run-unit/unit391-walk-last.txt
    passed=$(grep -cE "^\s+Passed " $f)
    failed=$(grep -cE "^\s+Failed " $f)
    summary=$(grep -E "^(Passed!|Failed!)" $f)
  fi
  echo "type $n at $hash: passed=$passed failed=$failed $(tail -1 $f) $summary"
  if [ "$failed" != "0" ] && [ "$stop" = "yes" ]; then break; fi
done
git worktree remove --force $wt && echo "worktree removed"
