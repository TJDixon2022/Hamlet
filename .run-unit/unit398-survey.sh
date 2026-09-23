cd /c/Source/HamLet
echo "props diff lines:"; git diff --stat HEAD~1 HEAD -- Directory.Build.props
T=tests/Hamlet.RadioEngine.Tests
for f in Audio/TheReadPathDoesNotAllocateTests.cs Audio/TheTapIsNotBehindTheDecoderTests.cs Cw/ABlipDoesNotShiftEverythingAfterItTests.cs Cw/AMoveStartsTheDecoderFreshTests.cs Cw/AStationIsABinThatSwingsTests.cs Cw/EveryElementCarriesItsOwnPitchTests.cs Cw/FittingKeyUpAgainstAssumingItTests.cs Cw/IsTheHertzABiasOrAFloorTests.cs Cw/NoSenderIsSplitInTwoTests.cs Cw/NothingActsOnTheAdmissionVerdictTests.cs Cw/TheCleanReadsStayCleanTests.cs Cw/TheFirstSecondsAreReadAgainTests.cs Cw/ThePeakAgainstASecondSignalTests.cs Cw/ThePeakFindsThePitchTheTrackerMissedTests.cs Cw/ThePosteriorSurvivesItsOwnArithmeticTests.cs Cw/TheProbabilisticDecoderTests.cs Cw/TheQuietestBinNoLongerWinsTests.cs Cw/TheReferenceDecoderIsPortedFaithfullyTests.cs Cw/TheScoreSaysWhatItIsMeasuringTests.cs Cw/WhatDecodeScoringCostsTests.cs Cw/WhereHamletAndTheReferenceDivergeTests.cs; do
  echo "$f lines=$(wc -l < $T/$f) facts=$(grep -cE "\[(Fact|Theory|SkippableFact)" $T/$f)"
done
f=tests/Hamlet.App.Tests/Views/ThePitchControlsAreOffThePanelTests.cs
echo "app $f lines=$(wc -l < $f) facts=$(grep -cE "\[(Fact|Theory)" $f)"
