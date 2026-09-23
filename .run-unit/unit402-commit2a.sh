cd /c/Source/HamLet
sh .run-unit/unit402-transmit.sh
echo "app vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src/Hamlet.App; git diff --stat -- src/Hamlet.App; echo end
git diff --stat -- src tests
sh .run-unit/unit402-commit.sh "step 3 repair 3.6: #24 #41 a follow under half the passband is not a speed discontinuity, CwDecoder 691, CwEmissionGateTests 8 of 8 and CwAdjudicationTests 11 of 11, kept on both greens with captures 37 of 37 every row identical, adjudicated 13, synthetics 2, CwFixtureTests 22 of 23, displacement 6, CwSpeedSilenceTests 4, CwTwoStationTests 5, CapturedSignalTests 13, WhyTheGateDidNotFireTests 2, all as at entry; full passband measured first and put back for naming 21 on exchange-easy" "TASK 2 of 5" "task 2 group A kept; group B: line 203 of CwReceiverFixtureTests reads CharacterSettled, measuring #43 to #45" src/Hamlet.RadioEngine/Cw/CwDecoder.cs tests/Hamlet.RadioEngine.Tests/Cw/TheEightRedsTests.cs
date +%H:%M:%S
