#!/bin/sh
# unit 471 task 3 - append the one PARKED.md line (time from the clock) and tick 2.4 in both copies of PHASE_PLAN.md.
cd /c/Source/HamLet || exit 1
NOW=$(date "+%Y-%m-%d %H:%M")
LINE="PARKED: 2.4 | unit 10 launched not recorded | $NOW | drift | Step 2 closed partial after three consecutive units with no kept change (460, 470 and 471), with MET-CER-SURE standing at 33 of 436 sure on the real set (inferred keys) and 14 of 173 on the synthetic set (exact keys) against HM-REQ-010's one in a hundred, and the trace is held in docs/phase-requirements/metrics.md's step-2 sections, .run-unit/unit470-trace.txt, .run-unit/unit471-trace.txt, and the facts WhatTheSureLettersWerePrintedUnderFact and WhatTheAcquiringLettersReadAtTheProvedValuesFact."
tail -c 1 PARKED.md | od -c | head -1
printf '%s\n' "$LINE" >> PARKED.md
echo "== PARKED.md tail"
tail -n 3 PARKED.md | cut -c1-200
for f in PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
do
  sed -i 's/^- \[ \] 2\.4 /- [x] 2.4 /' "$f"
  grep -n -E "^- \[.\] 2\.[0-9]" "$f" | cut -c1-60
done
cmp PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md && echo "copies identical"
git diff --stat -- PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md PARKED.md
