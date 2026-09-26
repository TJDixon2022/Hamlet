#!/bin/sh
# unit 455 - task 1 commit: the trace test and its output.
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "2 of 4" code none "Task 1 done: 5 of 9 prosigns one symbol, 4 not in the table; task 2 writing the HM-REQ-071 and 072 tests to watch them fail"
sh .run-unit/unit455-commit.sh .run-unit/unit455-msg1.txt PROJECT_STATUS.md tests/Hamlet.RadioEngine.Tests/Cw/TheProsignArrivesAsOneSymbolTests.cs .run-unit/unit455-*
