#!/bin/sh
# unit 429 - the clock fields of every 2026-09-24 sidecar: kept at, samples seen, tone, speed, counts since last, text.
cd /c/Source/HamLet/tests/fixtures/cw/captured/unadjudicated || exit 1
for f in cw-2026-09-24-*.txt
do
  echo "== $f"
  grep -E "^(captured|audioSeen|toneHz|decoderWpm|reading|inThis|sinceLast|textCovers|duty|keying) " "$f" | cut -c1-230
  grep -E "^text " "$f" | cut -c1-400
done
