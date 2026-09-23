cd /c/Source/HamLet
echo "TASK 3, THE PROSIGNS FIXTURE (decision 10): started at minute 16. In memory at 0.02 through the sibling printer TheProsignsFixtureAtABandTests: W1AW DE K2ABC <BT> R TU <SK>, no confident mistake, 16 of 16, every letter high. Regenerated with NoiseAmplitude 0.02 by a temporary writer deleted before the commit. CwFixtureTests 20 green 3 red to 22 green 1 red, the one red NothingTheDecoderWasSureOfIsWrong on fading-18wpm as at task 0; captures 37 of 37 every row identical to entry, adjudicated 13 of 13, the two other fixture-reading types identical to task 2. Set names #30 and #33 green, the set at 37 green and 14 red-open." >> PHASE_OUTCOME.md
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; git diff --stat -- src; echo end
sh .run-unit/unit400-transmit.sh
sh .run-unit/unit400-commit.sh "step 3 repair: prosigns-18wpm regenerated with a band of 0.02, R49; TheProsignRecordingDecodesItsProsigns and its share case green, CwFixtureTests 22 of 23, floors 37 of 37 and 13 of 13 unmoved; sibling printer TheProsignsFixtureAtABandTests kept; set names #30 and #33 green" "TASK 3 of 5" "task 3 committed; task 4 next: exit round, app carry-forward line first" PHASE_OUTCOME.md tests/Hamlet.RadioEngine.Tests/Cw/CwFixtures.cs tests/Hamlet.RadioEngine.Tests/Cw/TheProsignsFixtureAtABandTests.cs tests/fixtures/cw/prosigns-18wpm.wav docs/phase-cw/unit400-harness.md 2>&1 | grep -v "^	\|^  (use\|^$\|^Untracked\|^Changes not\|^On branch\|^Your branch"
git status --short tests
echo "TASK 4 of 5" > .run-unit/unit400-task.txt
date +%H:%M:%S
