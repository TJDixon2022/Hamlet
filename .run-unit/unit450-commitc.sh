#!/bin/sh
# unit 450 - the state, the sheet change and the tests, on their own.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit450-commit.sh .run-unit/unit450-msgc.txt src/Hamlet.RadioEngine/Cw/CwPitchProof.cs src/Hamlet.RadioEngine/Cw/CwToneTracker.cs src/Hamlet.RadioEngine/Cw/CwDecodeReport.cs src/Hamlet.RadioEngine/Cw/CwDecoder.cs src/Hamlet.App/ViewModels/MainWindowViewModel.cs tests/Hamlet.RadioEngine.Tests/Cw/ThePitchSaysWhetherItWasProvedTests.cs tests/Hamlet.App.Tests/Cw/ThePitchLineSaysWhatWasProvedTests.cs tests/Hamlet.App.Tests/Cw/TheRestOfTheSheetIsTrueTests.cs
