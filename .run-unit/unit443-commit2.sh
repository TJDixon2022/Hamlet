#!/bin/sh
# unit 443 - task 2 commit: the not-kept diff, metrics.md, the text printer, the printouts.
cd /c/Source/HamLet || exit 1
git status --short -- src
sh tools/status.sh EXECUTING "2 of 3" code none "Task 2: re-read rule not kept (MET-INVENTED 47 to 48); committing diff and metrics.md"
sh .run-unit/unit443-commit.sh .run-unit/unit443-msg2.txt PROJECT_STATUS.md docs/phase-requirements/metrics.md tests/Hamlet.RadioEngine.Tests/Cw/WhereTheSureAddedLettersComeFromTests.cs .run-unit/unit443-added-notkept.diff .run-unit/unit443-*.txt .run-unit/unit443-*.sh
git status -sb | head -1
