#!/bin/sh
# unit 469 - the 7.3 tick in both plan copies, in its own commit; then push.
cd /c/Source/HamLet || exit 1
cmp PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md && echo "plan copies identical"
git diff --stat -- PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
grep -n -E "^- \[.\] 7\.[0-9]" PHASE_PLAN.md | cut -c1-90
sh tools/status.sh EXECUTING "2 of 3" code none "Task 2 pushed 5f6eb688; ticking 7.3 in both plan copies - generated, proved red then green, recipe written, all four measured"
git add -- PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md PROJECT_STATUS.md .run-unit/unit469-tick.sh || exit 1
git commit -q -m "unit469: tick 7.3 - INT-ADJ, INT-COCHAN, INT-CARRIER and INT-PILEUP produced over the wanted TX-ITU station, proved on the rendered audio and watched red, recipe in interference.md, HM-REQ-060, 061 and 062 measured and not met, 066 measured and met on the pre-fixed reading (7.3)" -m "Plan lines only; code as 5f6eb688. 7.4 left open: it asks for a real capture reported by its sender profile." -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>" || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
