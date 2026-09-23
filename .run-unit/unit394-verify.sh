cd /c/Source/HamLet
echo "--- HEAD"; git log --oneline -2
echo "--- props 1254"; sed -n 1254p Directory.Build.props
echo "--- status"; git status --short | grep -v "^?? .run-unit" | grep -v "^ M .run-unit" | grep -v "^ D .run-unit"
echo "--- arbiter untracked"; git status --short tools/arbiter
echo "--- CLAUDE.md top row"; grep -n "HM-DEC-16[0-9]" CLAUDE.md | head -5
echo "--- failing set"; wc -l docs/unit239-failing-set.txt
cat docs/unit239-failing-set.txt | sed -E "s/^Hamlet.RadioEngine.Tests.Cw.//" | sed -E "s/\(.*//" | awk -F. '{NF--; print}' OFS=. | sort | uniq -c
echo "--- carry-forward"; wc -l docs/carry-forward-tests.txt
sed -n 153,160p docs/carry-forward-tests.txt | cut -c1-160
grep -n "WHAT UNIT 393 ADDED" docs/carry-forward-tests.txt
echo "--- compile remove"; grep -n "Compile Remove" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj | wc -l
grep -n "Compile Remove" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj | grep -c "Cw"
grep -n "Compile Remove" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj | head -1
grep -n "Compile Remove" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj | tail -1
grep -n "Compile Remove" tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj
grep -n "ABlipDoes" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
echo "--- fixtures"; ls tests/fixtures/cw/*.wav | wc -l; ls tests/fixtures/cw/receiver | head -30; ls tests/fixtures/cw/captured/unadjudicated/*.wav | wc -l
echo "--- retired"; ls docs/cw-retired-tests.txt
echo "--- src diff"; git diff --stat 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw | tail -1
echo "--- worktrees"; git worktree list
