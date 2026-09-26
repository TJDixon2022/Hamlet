#!/bin/sh
# unit 453 - task 1 commits: the printer and its trace, then metrics.md with 6.5 ticked.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit453-commit.sh .run-unit/unit453-msg1a.txt tests/Hamlet.RadioEngine.Tests/Cw/WhatTheNamedWordsReadTests.cs .run-unit/unit453-*
sh .run-unit/unit453-tick.sh 6.5
sh tools/status.sh COMPLETED "1 of 3" code none "Task 1 done: 0 of 3 measurable spans met, WEEKEND and THINKING not in 021410's audio, 6.5 ticked; next choosing whether any cause is movable outside the excluded routes"
sh .run-unit/unit453-commit.sh .run-unit/unit453-msg1.txt docs/phase-requirements/metrics.md PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md PROJECT_STATUS.md
