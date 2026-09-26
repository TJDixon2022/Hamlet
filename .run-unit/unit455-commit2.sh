#!/bin/sh
# unit 455 - task 2 commit: the two tests, the tick, the runs.
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "3 of 4" code none "Task 2 done, 6.2 ticked: 071 red on VE CT BK CL, 072 red for want of a setting; task 3 building the naming setting, default prosign"
sh .run-unit/unit455-commit.sh .run-unit/unit455-msg2.txt PROJECT_STATUS.md PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md tests/Hamlet.RadioEngine.Tests/Cw/TheProsignArrivesAsOneSymbolTests.cs tests/Hamlet.App.Tests/Cw/TheTwoNamedPatternIsNamedAsTheTerminalIsSetTests.cs .run-unit/unit455-*
