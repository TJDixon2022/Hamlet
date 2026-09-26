#!/bin/sh
# unit 455 - run the four app failures at HEAD with task 3's change stashed, then restore it.
cd /c/Source/HamLet || exit 1
git stash push -u -q -m unit455-task3 -- src tests/Hamlet.App.Tests/Cw/TheTwoNamedPatternIsNamedAsTheTerminalIsSetTests.cs || exit 1
git status --short -- src tests
sh .run-unit/unit455-build.sh athead "3 of 4" "Task 3: building HEAD with the change stashed, to see whether four app failures predate it"
sh .run-unit/unit455-run.sh app-athead app 300 "3 of 4" "Task 3: the four app failures at HEAD, change stashed" "FullyQualifiedName~.SettingsCarriesTheTransmitDriveTests.TheNoteSaysItIsAStartingPointToBeSetAgainstTheRadiosAlc|FullyQualifiedName~.VoiceTests.NoOperatorFacingStringUsesABritishSpelling|FullyQualifiedName~.HowMuchTheApplicationSaysTests" --no-build
git stash pop -q
git status --short -- src tests
