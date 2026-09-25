cd /c/Source/HamLet
tail -60 PHASE_OUTCOME.md
echo ====
git diff PHASE_STATUS.md
echo ====
git show --stat --format="%h %s" 91d87c5e | cut -c1-120 | head -30
echo ====
git show 91d87c5e -- PHASE_STATUS.md Directory.Build.props | head -60
