#!/bin/sh
# unit 449 - trace rows, chosen fields: recording, at, span, sent, emitted, class, pattern, rival, margin, wpm x3, mix, tracker, instrument, diff, retune, step, worst mark, gap, before verdict, per hop, routes.
# Usage: sh .run-unit/unit449-rows.sh <suffix> <set>
cd /c/Source/HamLet || exit 1
grep -a "^ *trace | $2 | " .run-unit/unit449-trace449-$1.txt | awk -F' [|] ' '{ printf "%s|%s|%s|%s>%s|%s|%s|rv %s|m %s|wpm %s/%s/%s|mix %s tr %s ins %s d %s|rt %s %s|wm %s wg %s|bv %s|ph %s|%s\n", $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15, $16, $17, $18, $19, $20, $23, $24, $26, $27, $30 }'
