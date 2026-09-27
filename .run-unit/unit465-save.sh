#!/bin/sh
# unit 465 - save both decoders' characters with class and p, split into ours and port, and compare with a saved pair.
# Usage: sh .run-unit/unit465-save.sh <suffix> "<n of 4>" [compare-suffix]
cd /c/Source/HamLet || exit 1
export LC_ALL=C
sh .run-unit/unit465-run.sh "save-$1" engine 600 "$2" "Saving each decoders characters with class and p over all 35" "FullyQualifiedName~.WhereTheTwoReadingsMeetFact.EachDecodersCharactersWithClassAndP" --no-build
grep -a "^ *save | ours | " .run-unit/unit465-save-$1.txt | sed 's/^ *//' > .run-unit/unit465-text-$1.txt
grep -a "^ *save | port | " .run-unit/unit465-save-$1.txt | sed 's/^ *//' > .run-unit/unit465-port-$1.txt
wc -l .run-unit/unit465-text-$1.txt .run-unit/unit465-port-$1.txt
if [ -n "$3" ]; then
  cmp .run-unit/unit465-text-$3.txt .run-unit/unit465-text-$1.txt && echo "ours BYTE-IDENTICAL to $3"
  cmp .run-unit/unit465-port-$3.txt .run-unit/unit465-port-$1.txt && echo "port BYTE-IDENTICAL to $3"
fi
