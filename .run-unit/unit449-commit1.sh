#!/bin/sh
# unit 449 - task 1 commit: the printer, the diagnostic record field, the trace output.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "1 of 3" code none "Trace done: 40 real and 14 synthetic traced; largest open group 9 wrong vs 6 right, the tracker leaving the station for a quieter keyed candidate; next the test"
sh .run-unit/unit449-commit.sh .run-unit/unit449-msg1.txt PROJECT_STATUS.md src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs src/Hamlet.RadioEngine/Cw/CwCharacter.cs src/Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs tests/Hamlet.RadioEngine.Tests/Cw/WhatTheFortySureWrongLettersRestOnTests.cs .run-unit/unit449-*
git status --short | grep -v "^??" | head
