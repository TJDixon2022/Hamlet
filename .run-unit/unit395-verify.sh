cd /c/Source/HamLet
echo "== HEAD"; git log --oneline -2
echo "== props 1254"; sed -n 1254p Directory.Build.props
echo "== PHASE_STATUS"; grep -nE "^(CURRENT_STEP|WORK_INSTRUCTION|STEP)" PHASE_STATUS.md | head -20
echo "== PHASE_OUTCOME states"; grep -nE "^## |^STATE" PHASE_OUTCOME.md | head -60
echo "== status short root"; git status --short | grep -v "\.run-unit"
echo "== CLAUDE.md 360"; sed -n 358,361p CLAUDE.md | cut -c1-200
echo "== PARKED / retired"; ls docs/phase-cw/PARKED.md docs/cw-retired-tests.txt 2>&1
echo "== 45"; git log --reverse --date=short --format="%h %ad %s" 7e209cb4..HEAD --until=2026-09-04 -- src/Hamlet.RadioEngine/Cw | wc -l
git log --reverse --date=short --format="%ad" 7e209cb4..HEAD --until=2026-09-04 -- src/Hamlet.RadioEngine/Cw | sort | uniq -c
git log --reverse --date=short --format="%h %ad" 7e209cb4..HEAD --until=2026-09-04 -- src/Hamlet.RadioEngine/Cw | sed -n "1p;\$p"
echo "== between 07f0397a and 7e209cb4 inclusive"; git log --format="%h" 07f0397a..7e209cb4 -- src/Hamlet.RadioEngine/Cw | wc -l
echo "== diff stat Cw"; git diff --stat 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw
echo "== floors lines"; grep -n "021410\|013637" tests/Hamlet.RadioEngine.Tests/Cw/TheCapturesThatDecodeKeepDecodingTests.cs
echo "== sidecars"; grep -n "^text" tests/fixtures/cw/captured/unadjudicated/cw-2026-08-25-021410.txt tests/fixtures/cw/captured/unadjudicated/cw-2026-08-25-013637.txt | cut -c1-300
echo "== carry lines"; wc -l docs/carry-forward-tests.txt; sed -n "7p;9p;158p;159p" docs/carry-forward-tests.txt | cut -c1-400
echo "== compile remove"; grep -c "Compile Remove" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj; grep "Compile Remove" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj | grep -c "Cw"
echo "== worktrees"; git worktree list
