#!/bin/sh
# unit 442 - the transmit files against 7e209cb4 and against this unit's entry, and src file by file.
cd /c/Source/HamLet || exit 1
TX=$(git ls-files src | grep -i "transmit\|ptt\|keyer\|/tx\|sender\|sending")
echo "== transmit files: $(echo "$TX" | wc -l)"
echo "== diff against 7e209cb4 (stat):"
git diff --stat 7e209cb4 HEAD -- $TX
echo "== diff against 0439a8e7, this unit's entry (stat):"
git diff --stat 0439a8e7 HEAD -- $TX
echo "== src this unit, file by file:"
git diff --stat 0439a8e7 HEAD -- src
