cd /c/Source/HamLet
cat .run-unit/unit400-t1line.txt >> PHASE_OUTCOME.md
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; git diff --stat -- src; echo end
sh .run-unit/unit400-transmit.sh
sh .run-unit/unit400-commit.sh "unit400 task 1: CwDecodeHarness reads CharacterSettled, the transcript the CW tab shows; 5 caller types measured, no case green at entry red, CwFixtureTests 14 to 16 green with confident-mistakes on noisy and interference red to green, clean synthetics still 0 of 2 off disk; R12" "TASK 1 of 5" "task 1 committed; task 2 next: adding NoiseAmplitude 0.02 to the two clean requests in CwFixtures.cs" PHASE_OUTCOME.md tests/Hamlet.RadioEngine.Tests/Cw/CwDecodeHarness.cs docs/phase-cw/unit400-harness.md
echo "TASK 2 of 5" > .run-unit/unit400-task.txt
date +%H:%M:%S
