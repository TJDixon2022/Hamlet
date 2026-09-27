#!/bin/sh
# unit 468 - tick 7.2 in both copies of PHASE_PLAN.md, commit and push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
for f in PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
do
  sed -i 's/^- \[ \] 7\.2 The generator produces the TX-\* sender profiles/- [x] 7.2 The generator produces the TX-* sender profiles/' "$f"
  grep -n -E "^- \[[ x]\] 7\.[0-9]" "$f" | cut -c1-90
done
cmp PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md && echo "plan copies identical"
git diff --stat
sh tools/status.sh EXECUTING "2 of 3" code none "Task 2 - ticking 7.2: profiles produced, proof watched red then green, recipe written, HM-REQ-050 measured (not met on all four)"
git add -- PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md PROJECT_STATUS.md .run-unit/unit468-tick.sh || exit 1
git commit -q -m "unit468: tick 7.2 - five TX-* profiles produced, two refused by name, proved on the rendered tone and watched red, recipe in senders.md, HM-REQ-050 measured and not met on TX-ITU, TX-KEYER-W, TX-FARNS and TX-TIGHT (7.2)" -m "Not met is the finding 7.2 asks for, not a failure of the criterion (work instruction 468 section 2). 7.3 and 7.4 stay open." -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>" || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
