#!/bin/sh
# unit 457 - task 0 commit: ENTRY line into both outcome copies, the runner's writes, then push.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit457-entry-line.md >> PHASE_OUTCOME.md
cat .run-unit/unit457-entry-line.md >> docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "1 of 5" code none "Task 0 done, numbers as 456 left them; task 1 reading the first element's path, port against cw.cxx at 61b97f41, line by line"
git status --short | head -50
git add -A -- .run-unit/reports STOP .run-unit/parked.txt .run-unit/watched.cpu .run-unit/watched.rc
sh .run-unit/unit457-commit.sh .run-unit/unit457-msg0.txt Directory.Build.props PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md PROJECT_STATUS.md docs/phase-requirements/PHASE_OUTCOME.md docs/phase-requirements/PHASE_STATUS.md .run-unit/approach.txt .run-unit/arbiter-prompt.txt .run-unit/arbiter.json .run-unit/denials.txt .run-unit/last-run.json .run-unit/launch.stamp .run-unit/reload.txt .run-unit/s4-prompt.txt .run-unit/s4-verdict.json .run-unit/section4.txt .run-unit/state-prompt.txt .run-unit/state-verdict.json .run-unit/step-plan.txt .run-unit/step-states.txt .run-unit/watched-runner.bat .run-unit/watched.born .run-unit/watched.pid .run-unit/why.txt .run-unit/unit457-*
git status --short
