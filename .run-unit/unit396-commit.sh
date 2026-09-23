cd /c/Source/HamLet
# Usage: sh .run-unit/unit396-commit.sh "<message>" "TASK n of 4" "<note>" <paths...>
MSG=$1
TASK=$2
NOTE=$3
shift 3
git add -- "$@"
git commit -q -m "$MSG" -m "Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
echo "commit rc $?"
git push -q origin main 2>&1
echo "push rc $?"
git log --oneline -1
git status --short -- src tests docs PHASE_PLAN.md
sh tools/status.sh EXECUTING "$TASK" code none "$NOTE"
