cd /c/Source/HamLet
git log --oneline -3
echo "=== status"
git status --short
echo "=== src and tests diff vs HEAD"
git diff --stat HEAD -- src tests data Directory.Build.props
echo "=== 7.4"
grep -n "^- \[.\] 7\.4" PHASE_PLAN.md
echo "=== claude"
tasklist //V | grep -Ei "claude.exe|cmd.exe" | cut -c1-110
ps -ef | grep -Ei "dotnet|testhost" | grep -v grep
echo "=== version"
grep -n "Version" Directory.Build.props
echo "=== phase status"
head -20 PHASE_STATUS.md
