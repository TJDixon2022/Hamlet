#!/bin/sh
# unit 438 - the stream's own reads 30 to 46.2 s, entry beside the built change: s, mix Hz, unit ms, wpm, from, settles.
# Taken from WhenTheWindowIsRemixed's unchanged shadow, which reads exactly as the decoder does.
cd /c/Source/HamLet/.run-unit || exit 1
for f in entry local
do
  awk '/WhenTheWindowIsRemixed\(run: "stream"\)/ { on = 1 } /WhenTheWindowIsRemixed\(run: "cw-/ { on = 0 } on && /^ remix read \| [0-9]/' unit438-opening-$f.txt \
    | awk -F'|' '{ gsub(/ /, "", $2) } $2 + 0 >= 30.0 && $2 + 0 < 46.2 { printf "%s|%s|%s|%s|%s|%s\n", $2, $3, $6, $7, $8, $12 }' | sort -u -t'|' -k1,1n > unit438-perread-$f.tmp
done
echo "s | entry: mix Hz | unit ms | wpm | from | settles || local cut built: mix Hz | unit ms | wpm | from | settles"
join -t'|' unit438-perread-entry.tmp unit438-perread-local.tmp | sed 's/|/ |/g'
