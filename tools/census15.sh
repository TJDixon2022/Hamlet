#!/bin/sh
# Unit 290 task 1 - the census of fifteen. Reading only; changes nothing.
PAT='fifteen|15 s|15-second|15 second|quarter.minute|quarter minute|12\.64|15\.0f|= 15;|FromSeconds(15)'

echo "=== PROSE (comment lines) in Hamlet.RadioEngine + Hamlet.App ==="
grep -rnE "^[[:space:]]*(///|//|\*)" src/Hamlet.RadioEngine src/Hamlet.App --include=*.cs \
  | grep -inE "$PAT" > /tmp/prose.txt
wc -l < /tmp/prose.txt
echo "--- files:"
cut -d: -f2 /tmp/prose.txt | sort -u

echo
echo "=== PROSE in Ft8Sharp + Ft8Sharp.Deep ==="
grep -rnE "^[[:space:]]*(///|//|\*)" src/Ft8Sharp src/Ft8Sharp.Deep --include=*.cs \
  | grep -inE "$PAT" > /tmp/prose-port.txt
wc -l < /tmp/prose-port.txt
echo "--- files:"
cut -d: -f2 /tmp/prose-port.txt | sort -u
