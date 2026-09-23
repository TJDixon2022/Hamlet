#!/bin/sh
# unit 404 task 1 - one trial on the scratch repo.
# Usage: sh .run-unit/unit404-trial.sh <target n> [chain n ...]
# Resets /tmp/u404trace to base (HEAD's Cw), applies each chain piece in order, file by file:
#   a file that applies goes in; a file already in the tree (reverse applies, or a new file that
#   already exists) is dropped under unit 395 decision 3; otherwise it goes in with --reject and
#   its refused hunks are counted against the link.
# Then checks the target the same way without applying it.
# Prints: TARGET n CHAIN a,b,c LINKREJ n (link:file:rej@lines) TARGETREJ n (file:rej@lines) DROPPED(...)
P=/c/Source/HamLet/.run-unit/unit404-patches
S=/tmp/u404trace
cd $S || exit 1
git reset -q --hard base
git clean -fdq
T=$1; shift
pat() { ls $P/$(printf "%02d" "$1")-*.patch; }
files() { grep "^diff --git" "$1" | sed 's#^diff --git a/\([^ ]*\) b/.*#\1#'; }
# one_file <patch> <file> <apply|check> -> sets R (refused count), L (lines), D (1 if dropped)
one_file() {
  R=0; L=""; D=0
  if git apply --check --whitespace=nowarn --include="$2" "$1" > /dev/null 2>&1; then
    [ "$3" = apply ] && git apply --whitespace=nowarn --include="$2" "$1" > /dev/null 2>&1
    return
  fi
  if git apply --check -R --whitespace=nowarn --include="$2" "$1" > /dev/null 2>&1; then D=1; return; fi
  git apply --check --whitespace=nowarn --include="$2" "$1" > /tmp/u404-apply.log 2>&1
  if grep -q "already exists in working directory" /tmp/u404-apply.log; then D=1; return; fi
  git apply --reject --whitespace=nowarn --include="$2" "$1" > /tmp/u404-apply.log 2>&1
  R=$(grep -c "^Rejected hunk" /tmp/u404-apply.log)
  [ "$R" -eq 0 ] && R=$(grep -c "^error:" /tmp/u404-apply.log)
  L=$(grep "patch failed:\|No such file" /tmp/u404-apply.log | sed 's/.*patch failed: [^:]*:\([0-9]*\).*/\1/; s/.*No such file.*/missing/' | tr '\n' ' ' | sed 's/ $//')
  find . -name "*.rej" -not -path "./.git/*" -delete
  if [ "$3" = check ]; then git checkout -q -- . 2>/dev/null; git clean -fdq; fi
}
LINKREJ=0; LINKDETAIL=""; DROPPED=""
for c in "$@"; do
  PP=$(pat $c)
  for f in $(files "$PP"); do
    one_file "$PP" "$f" apply
    [ "$D" -eq 1 ] && DROPPED="$DROPPED $c:$(basename $f)"
    if [ "$R" -gt 0 ]; then LINKDETAIL="$LINKDETAIL $c:$(basename $f):$R@$L"; LINKREJ=$((LINKREJ + R)); fi
  done
done
git add -A > /dev/null 2>&1
git -c user.name=u404 -c user.email=u404@local commit -qm chain > /dev/null 2>&1
TP=$(pat $T); TREJ=0; DETAIL=""
for f in $(files "$TP"); do
  one_file "$TP" "$f" check
  [ "$D" -eq 1 ] && DROPPED="$DROPPED $T:$(basename $f)"
  if [ "$R" -gt 0 ]; then DETAIL="$DETAIL $(basename $f):$R@$L"; TREJ=$((TREJ + R)); fi
done
CH=$(echo "$@" | tr ' ' ',')
echo "TARGET $T CHAIN ${CH:-none} LINKREJ $LINKREJ${LINKDETAIL:+ ($LINKDETAIL )} TARGETREJ $TREJ${DETAIL:+ ($DETAIL )}${DROPPED:+ DROPPED($DROPPED )}"
