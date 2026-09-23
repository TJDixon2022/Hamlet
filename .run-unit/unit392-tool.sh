cd /c/Source/HamLet
git log --diff-filter=A --format="%h %ad %s" --date=short -- tools/Hamlet.PitchRank/Program.cs tools/Hamlet.ScopeCheck/Program.cs | cut -c1-160
echo "== tools at 7e209cb4"
git ls-tree -d 7e209cb4 tools/ | cut -f2
echo "== sln"
grep -n "PitchRank" Hamlet.sln
echo "== PitchRank Cw names"
grep -oE "Cw[A-Z][A-Za-z]*(\.[A-Z][A-Za-z]*)?" tools/Hamlet.PitchRank/Program.cs | sort | uniq -c | sort -rn | head -40
