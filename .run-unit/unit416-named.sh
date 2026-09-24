#!/bin/sh
# unit 416 - every named floor's count, entry against a later round, and the named regions.
# Usage: sh .run-unit/unit416-named.sh <later-suffix>
cd /c/Source/HamLet || exit 1
pick() {
  grep -a -E "^ ?named \|" .run-unit/unit416-named-$1.txt | sed -E "s/^ +//" | cut -d'|' -f2,3 | sort
}
pick entry > .run-unit/unit416-named-entry.cmp
pick "$1" > .run-unit/unit416-named-$1.cmp
echo "floors printed: entry $(wc -l < .run-unit/unit416-named-entry.cmp), $1 $(wc -l < .run-unit/unit416-named-$1.cmp)"
diff .run-unit/unit416-named-entry.cmp .run-unit/unit416-named-$1.cmp
echo "named counts diff rc $?"
