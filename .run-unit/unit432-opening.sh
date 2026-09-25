#!/bin/sh
# unit 432 - the opening's text and named count: cold 003901 and 003919 whole, and the stream 0 to 46.2 s.
# A named character is any settled character but a space or the placeholder.
# Usage: sh .run-unit/unit432-opening.sh <file under .run-unit>
cd /c/Source/HamLet/.run-unit || exit 1
for L in "cold | cw-2026-09-24-003901 | bench | " "cold | cw-2026-09-24-003919 | bench | " "stream | cw-2026-09-24-003901 | 0.00 to 30.00 s | " "stream | cw-2026-09-24-003919 | 30.00 to 46.20 s | " "cold | cw-2026-09-24-003901 | sidecar text, whole | " "cold | cw-2026-09-24-003919 | sidecar added since the capture before | "
do
  grep -F "$L" "$1" | head -1 | awk -v p="$L" '{
    t = substr($0, index($0, p) + length(p));
    s = t; gsub(/■/, "", s); gsub(/ /, "", s);
    printf "%d named | %s%s\n", length(s), p, t }'
done
