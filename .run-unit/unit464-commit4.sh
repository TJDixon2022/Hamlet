#!/bin/sh
# unit 464 - task 4 commit: output.md, the exit printouts, the status; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh COMPLETED "4 of 4" tim none "Unit 464 done: 9.5 ticked - both decoders carry p; held-out, ours calibrated on 4 of 12 conditions, the port on none (advisory everywhere under HM-REQ-124); texts byte-identical; output.md written"
git add -- output.md PROJECT_STATUS.md || exit 1
for f in .run-unit/unit464-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git status --short | grep -v "^??"
git commit -q -F .run-unit/unit464-msg4.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
git status --short
