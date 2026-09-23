#!/bin/sh
# unit 404 task 1 - extract the 45 pieces of unit395-rework.md section 1.2 as Cw-only patches,
# transmit files excluded, and set up a scratch repo holding HEAD's Cw folder as "base".
cd /c/Source/HamLet || exit 1
P=.run-unit/unit404-patches
mkdir -p $P
TX=$(cat .run-unit/unit404-txfiles.txt)
EXCL=""
for t in $TX; do EXCL="$EXCL :(exclude)$t"; done
grep -E "^\| [0-9]+ \| [0-9a-f]{8} \| 08-|^\| [0-9]+ \| [0-9a-f]{8} \| 09-" docs/phase-cw/unit395-rework.md \
  | awk -F'|' '{gsub(/ /,"",$2); gsub(/ /,"",$3); print $2, $3}' > .run-unit/unit404-pieces.txt
wc -l < .run-unit/unit404-pieces.txt
while read n h; do
  f=$(printf "%s/%02d-%s.patch" $P "$n" "$h")
  git diff --binary "$h^" "$h" -- src/Hamlet.RadioEngine/Cw $EXCL > "$f"
  printf "%2s %s files=%s hunks=%s\n" "$n" "$h" "$(grep -c '^diff --git' "$f")" "$(grep -c '^@@' "$f")"
done < .run-unit/unit404-pieces.txt
# scratch repo
S=/tmp/u404trace
if [ ! -d $S/.git ]; then
  mkdir -p $S
  git -C $S init -q
fi
git -C $S rm -rq --ignore-unmatch . > /dev/null 2>&1
git -C $S clean -fdq
git archive HEAD src/Hamlet.RadioEngine/Cw | tar -x -C $S
git -C $S add -A
git -C $S -c user.name=u404 -c user.email=u404@local commit -qm base --allow-empty
git -C $S tag -f base > /dev/null
git -C $S log --oneline -1
ls $S/src/Hamlet.RadioEngine/Cw | wc -l
