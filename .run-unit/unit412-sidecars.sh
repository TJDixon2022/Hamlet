#!/bin/sh
# unit 412 - the fourteen sidecars' inThis, characters, sinceLast and textCovers lines, and whether each text starts with the previous one.
cd /c/Source/HamLet || exit 1
D=tests/fixtures/cw/captured/unadjudicated
PREV=""
for f in $D/cw-2026-09-24-*.txt
do
  n=$(basename "$f" .txt)
  echo "== $n"
  grep -E "^(captured|inThis|characters|sinceLast|textCovers|toneHz|decoderWpm) " "$f"
  T=$(sed -n "s/^text       //p" "$f")
  echo "textLen ${#T}"
  if [ -n "$PREV" ]
  then
    case "$T" in
      "$PREV"*) echo "prefix: previous text is a prefix of this one" ;;
      *) echo "prefix: NO - previous text is not a prefix of this one" ;;
    esac
  fi
  PREV="$T"
done
