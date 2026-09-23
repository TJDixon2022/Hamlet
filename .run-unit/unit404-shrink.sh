#!/bin/sh
# unit 404 task 1 - backward elimination from the whole prefix to an irreducible chain.
# Usage: sh .run-unit/unit404-shrink.sh <target n>
# A link is removed, newest first, when after removing it the target refuses no more hunks than
# the whole prefix left refusing, and the remaining links together refuse no more than they did.
cd /c/Source/HamLet || exit 1
T=$1
LOG=.run-unit/unit404-shrink-$T.txt
: > $LOG
trial() { sh .run-unit/unit404-trial.sh $T $* < /dev/null; }
trej() { echo "$1" | sed 's/.* TARGETREJ \([0-9]*\).*/\1/'; }
lrej() { echo "$1" | sed 's/.* LINKREJ \([0-9]*\).*/\1/'; }
# refusals charged to link $2 in trial output $1
own() { echo "$1" | tr ' ' '\n' | grep "^$2:.*@" | sed 's/.*:\([0-9]*\)@.*/\1/' | awk '{s+=$1} END {print s+0}'; }
SET=""
i=1
while [ $i -lt $T ]; do SET="$SET $i"; i=$((i + 1)); done
# an optional starting superset, when one is already known to clear the target
shift
[ -n "$1" ] && SET="$*"
OUT=$(trial $SET)
echo "START $OUT" >> $LOG
BASE_T=$(trej "$OUT")
CUR="$OUT"
for j in $(echo $SET | tr ' ' '\n' | sort -rn); do
  TRY=$(echo $SET | tr ' ' '\n' | grep -vx "$j" | tr '\n' ' ')
  OUT=$(trial $TRY)
  NT=$(trej "$OUT"); NL=$(lrej "$OUT")
  OL=$(lrej "$CUR"); OJ=$(own "$CUR" $j)
  if [ "$NT" -le "$BASE_T" ] && [ "$NL" -le $((OL - OJ)) ]; then
    SET="$TRY"; CUR="$OUT"
    echo "drop $j" >> $LOG
  else
    echo "keep $j  ($OUT)" >> $LOG
  fi
done
echo "FINAL $CUR" >> $LOG
echo "FINAL $CUR"
