#!/bin/sh
# Unit 391 task 3: newest first over commits touching src/Hamlet.RadioEngine/Cw since 2026-08-24,
# type 3 (the clean synthetics) only, until the first commit where type 3 is green or absent.
cd /c/Source/HamLet || exit 1
git log --format=%h --since=${3:-2026-08-24} -- src/Hamlet.RadioEngine/Cw > .run-unit/unit391-candidates.txt
skip=${1:-0}
i=0
total=$(wc -l < .run-unit/unit391-candidates.txt)
for h in $(cat .run-unit/unit391-candidates.txt); do
  i=$((i+1))
  [ $i -le $skip ] && continue
  [ $i -gt $(( skip + ${2:-6} )) ] && { echo "CHUNK DONE at $((i-1))"; break; }
  line=$(sh .run-unit/unit391-walk.sh $h "3" yes "walk $i of $total, clean synthetics first" | head -1)
  echo "$i $h $line"
  case "$line" in
    *"failed=0"*|*"NOT PRESENT"*) echo "STOP: type 3 green or absent at $h"; break ;;
  esac
  case "$line" in
    *"passed=0 failed=0"*) echo "STOP: lost run at $h"; break ;;
  esac
done
