#!/bin/sh
# unit 472 - every recording the change moved, before (entry) and after (exit).
cd /c/Source/HamLet || exit 1
for R in cq-18wpm-5db-char5 cw-2026-08-17-013347 cw-2026-08-17-134712 cw-2026-08-18-004507 unadjudicated/cw-2026-08-18-003758 unadjudicated/cw-2026-08-22-031838 unadjudicated/cw-2026-08-22-031948 unadjudicated/cw-2026-08-22-032050 unadjudicated/cw-2026-08-22-032129 unadjudicated/cw-2026-09-24-004234
do
  echo "== $R"
  printf 'before: '
  sh .run-unit/unit472-render.sh entry "$R"
  printf 'after:  '
  sh .run-unit/unit472-render.sh exit "$R"
done
