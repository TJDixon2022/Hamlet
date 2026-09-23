cd /c/Source/HamLet
# Usage: sh .run-unit/unit397-out.sh <n> <piece hash> <piece commit> "<extra message>"
# Reverts the piece commit in the next commit with the doc row, message per decision 9.
N=$1
H=$2
PC=$3
git revert --no-commit $PC
echo "revert rc $?"
git add -- docs/phase-cw/unit395-rework.md docs/phase-cw/unit397-rework.md
git commit -q -m "step 4 piece $N out: $H moved nothing" -m "$4" -m "Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
echo "commit rc $?"
git push -q origin main 2>&1
echo "push rc $?"
git log --oneline -1
git diff --stat 5688a8a5 HEAD -- src
echo "src against 5688a8a5 end"
git status --short -- src tests docs
