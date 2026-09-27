#!/bin/sh
# unit 469 - task 3 commit: the exit round's printouts, output.md, the interference.md correction, the final status; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh COMPLETED "3 of 3" tim "output.md -> Claude Web" "Unit 469 done: 7.3 ticked - INT-ADJ, COCHAN, CARRIER, PILEUP generated and proved; 060, 061, 062 not met (tracker walks to the neighbour or starts on a carrier), 066 met only weakly; exit round as at entry; screen unchanged"
git add -- output.md docs/phase-requirements/interference.md PROJECT_STATUS.md || exit 1
for f in .run-unit/unit469-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
echo "== staged"
git status --short | grep -v "^??"
echo "== untracked"
git status --short | grep "^??"
git commit -q -m "unit469 task 3: exit round - every entry figure as at entry; the INT-* proof 21 of 21; app line 275 of 278 on the rerun and the two lost types alone 5 of 5 and 8 of 8 (DECIDED 8); src, port and transmit diffs empty; output.md written (7.3)" -m "interference.md: the port-alone sentence corrected - worse or empty in 26 of 27 cases, not in all; at INT-CARRIER(+50, +10) the port read one sure right E against ours T  E." -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>" || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
git status --short
