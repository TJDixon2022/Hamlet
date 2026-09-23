#!/bin/sh
# Work instruction 403, tasks 2 and 3: the same printer and the captures floor at a902cdf8.
# The worktree is removed whatever happens - the trap runs on every exit.
cd /c/Source/HamLet
WT=C:/Source/HamLet-wt403
OUT=/c/Source/HamLet/.run-unit
LOG=$OUT/unit403-before-steps.txt
cleanup() {
  cd /c/Source/HamLet
  git worktree remove --force "$WT" >> "$LOG" 2>&1
  echo "worktree removed rc=$?" >> "$LOG"
  git worktree prune >> "$LOG" 2>&1
  git worktree list >> "$LOG" 2>&1
}
trap cleanup EXIT
: > "$LOG"
sh tools/status.sh EXECUTING "TASK 2 of 4" code none "Adding the a902cdf8 worktree and copying in the printer, HEAD's harness and the capture" > /dev/null
git worktree add --detach "$WT" a902cdf8 >> "$LOG" 2>&1 || { echo "worktree add failed" >> "$LOG"; exit 1; }
git -C "$WT" log -1 --format="worktree at %H" >> "$LOG"
T=tests/Hamlet.RadioEngine.Tests/Cw
cp "$T/TheCaptureOfTheTwentyThirdTests.cs" "$WT/$T/"
cp "$T/CwDecodeHarness.cs" "$WT/$T/CwDecodeHarness.cs"
cp tests/fixtures/cw/captured/unadjudicated/cw-2026-09-23-125515.wav "$WT/tests/fixtures/cw/captured/unadjudicated/"
echo "copied printer, HEAD harness, capture wav" >> "$LOG"
git -C "$WT" diff --stat a902cdf8 -- src >> "$LOG" 2>&1
echo "src diff in worktree against a902cdf8 printed above, empty means none" >> "$LOG"
sh tools/status.sh EXECUTING "TASK 2 of 4" code none "Building the engine tests in the a902cdf8 worktree" > /dev/null
S=$(date +%s)
timeout 600 dotnet build "$WT/tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj" -v q -nologo > "$OUT/unit403-before-build.txt" 2>&1
echo "build rc=$? in $(( $(date +%s) - S )) s" >> "$LOG"
sh tools/status.sh EXECUTING "TASK 2 of 4" code none "a902cdf8 worktree built; printer running on the 12:55 capture there" > /dev/null
S=$(date +%s)
timeout 300 dotnet test "$WT/tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj" --no-build --filter "FullyQualifiedName~TheCaptureOfTheTwentyThirdTests" --logger "console;verbosity=detailed" > "$OUT/unit403-before.txt" 2>&1
echo "printer rc=$? in $(( $(date +%s) - S )) s" >> "$LOG"
sh tools/status.sh EXECUTING "TASK 3 of 4" code none "Printer done at a902cdf8; captures floor running there, 37 cases, about 95 s" > /dev/null
S=$(date +%s)
timeout 600 dotnet test "$WT/tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj" --no-build --filter "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" --logger "console;verbosity=detailed" > "$OUT/unit403-before-captures.txt" 2>&1
echo "captures rc=$? in $(( $(date +%s) - S )) s" >> "$LOG"
