#!/bin/sh
# Work instruction 403, task 2 re-run: the first a902cdf8 build took 2 s and printed a
# transcript identical to HEAD, so this run proves which decoder the worktree dll holds
# before anything is reported. The worktree is removed whatever happens.
cd /c/Source/HamLet
WT=C:/Source/HamLet-wt403
OUT=/c/Source/HamLet/.run-unit
LOG=$OUT/unit403-before-steps.txt
cp "$OUT/unit403-before-steps.txt" "$OUT/unit403-before-steps-run1.txt"
cp "$OUT/unit403-before.txt" "$OUT/unit403-before-run1.txt"
cp "$OUT/unit403-before-captures.txt" "$OUT/unit403-before-captures-run1.txt"
cleanup() {
  cd /c/Source/HamLet
  git worktree remove --force "$WT" >> "$LOG" 2>&1
  echo "worktree removed rc=$?" >> "$LOG"
  git worktree prune >> "$LOG" 2>&1
  git worktree list >> "$LOG" 2>&1
}
trap cleanup EXIT
: > "$LOG"
ls -d "$WT" >> "$LOG" 2>&1
sh tools/status.sh EXECUTING "TASK 2 of 4" code none "Re-running a902cdf8 with proof - the first worktree build took 2 s and printed HEAD's numbers exactly" > /dev/null
git worktree add --detach "$WT" a902cdf8 >> "$LOG" 2>&1 || { echo "worktree add failed" >> "$LOG"; exit 1; }
git -C "$WT" log -1 --format="worktree at %H" >> "$LOG"
ls -d "$WT/tests/Hamlet.RadioEngine.Tests/bin" "$WT/src/Hamlet.RadioEngine/bin" >> "$LOG" 2>&1
echo "bin folders before build listed above, none expected" >> "$LOG"
T=tests/Hamlet.RadioEngine.Tests/Cw
cp "$T/TheCaptureOfTheTwentyThirdTests.cs" "$WT/$T/"
cp "$T/CwDecodeHarness.cs" "$WT/$T/CwDecodeHarness.cs"
cp tests/fixtures/cw/captured/unadjudicated/cw-2026-09-23-125515.wav "$WT/tests/fixtures/cw/captured/unadjudicated/"
echo "copied printer, HEAD harness, capture wav" >> "$LOG"
cmp -s src/Hamlet.RadioEngine/Cw/CwDecoder.cs "$WT/src/Hamlet.RadioEngine/Cw/CwDecoder.cs" && echo "CwDecoder.cs SAME as HEAD" >> "$LOG" || echo "CwDecoder.cs differs from HEAD" >> "$LOG"
grep -c "half the passband" src/Hamlet.RadioEngine/Cw/CwDecoder.cs "$WT/src/Hamlet.RadioEngine/Cw/CwDecoder.cs" >> "$LOG" 2>&1
S=$(date +%s)
timeout 600 dotnet build "$WT/tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj" -v n -nologo > "$OUT/unit403-before-build.txt" 2>&1
echo "build rc=$? in $(( $(date +%s) - S )) s" >> "$LOG"
grep -c "CoreCompile" "$OUT/unit403-before-build.txt" >> "$LOG"
echo "CoreCompile mentions counted above" >> "$LOG"
sha256sum src/Hamlet.RadioEngine/bin/Debug/net8.0/Hamlet.RadioEngine.dll tests/Hamlet.RadioEngine.Tests/bin/Debug/net8.0/Hamlet.RadioEngine.dll "$WT/tests/Hamlet.RadioEngine.Tests/bin/Debug/net8.0/Hamlet.RadioEngine.dll" >> "$LOG" 2>&1
sh tools/status.sh EXECUTING "TASK 2 of 4" code none "a902cdf8 rebuilt; CwEmissionGateTests running there as the proof - #24 should be red on the older decoder" > /dev/null
S=$(date +%s)
timeout 300 dotnet test "$WT/tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj" --no-build --filter "FullyQualifiedName~CwEmissionGateTests" --logger "console;verbosity=normal" > "$OUT/unit403-before-gate.txt" 2>&1
echo "gate rc=$? in $(( $(date +%s) - S )) s" >> "$LOG"
S=$(date +%s)
timeout 300 dotnet test "$WT/tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj" --no-build --filter "FullyQualifiedName~TheCaptureOfTheTwentyThirdTests" --logger "console;verbosity=detailed" > "$OUT/unit403-before.txt" 2>&1
echo "printer rc=$? in $(( $(date +%s) - S )) s" >> "$LOG"
sh tools/status.sh EXECUTING "TASK 3 of 4" code none "Printer re-run at a902cdf8; captures floor running there again in the proven build, about 95 s" > /dev/null
S=$(date +%s)
timeout 600 dotnet test "$WT/tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj" --no-build --filter "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" --logger "console;verbosity=detailed" > "$OUT/unit403-before-captures.txt" 2>&1
echo "captures rc=$? in $(( $(date +%s) - S )) s" >> "$LOG"
