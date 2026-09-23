cd /c/Source/HamLet
sed -n 360p CLAUDE.md | grep -o "HM-DEC-[0-9]* |$"
grep -o "CPS-DEC-[0-9]*" .run-unit/reload.txt | head -1
git log --oneline -1
git status --short -- src tests docs PHASE_PLAN.md
date "+%Y-%m-%d %H:%M"
