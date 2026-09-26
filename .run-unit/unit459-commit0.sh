#!/bin/sh
# unit 459 - task 0 commit: the record, the runner's writes as they are, the entry round; then push.
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "1 of 4" code none "Task 0 committed, every entry figure as 458 left it; task 1 reading our decoder's marks and gaps under each letter the port reads right"
git add -A -- .run-unit/reports .run-unit/watched.cpu
sh .run-unit/unit459-commit.sh .run-unit/unit459-msg0.txt Directory.Build.props PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md PROJECT_STATUS.md docs/phase-requirements/PHASE_OUTCOME.md docs/phase-requirements/PHASE_STATUS.md .run-unit/approach.txt .run-unit/arbiter-prompt.txt .run-unit/arbiter.json .run-unit/denials.txt .run-unit/last-run.json .run-unit/launch.stamp .run-unit/reload.txt .run-unit/s4-prompt.txt .run-unit/s4-verdict.json .run-unit/section4.txt .run-unit/state-prompt.txt .run-unit/state-verdict.json .run-unit/step-plan.txt .run-unit/step-states.txt .run-unit/watched-runner.bat .run-unit/watched.born .run-unit/watched.pid .run-unit/why.txt .run-unit/unit459-*
git status --short
