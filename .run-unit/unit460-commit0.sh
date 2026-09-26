#!/bin/sh
# unit 460 - task 0 commit: the record, the runner's writes as they are, the entry round; then push.
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "1 of 4" code none "Task 0 committed at 10 of 13 named as recorded; task 1 reading each red floor's last banking and its condition's four metrics then and now"
git add -A -- .run-unit/reports .run-unit/watched.cpu
sh .run-unit/unit460-commit.sh .run-unit/unit460-msg0.txt Directory.Build.props PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md FUTURE_GOALS.md PROJECT_STATUS.md docs/phase-requirements/PHASE_OUTCOME.md docs/phase-requirements/PHASE_STATUS.md .run-unit/approach.txt .run-unit/arbiter-prompt.txt .run-unit/arbiter.json .run-unit/denials.txt .run-unit/last-run.json .run-unit/launch.stamp .run-unit/reload.txt .run-unit/s4-prompt.txt .run-unit/s4-verdict.json .run-unit/section4.txt .run-unit/state-prompt.txt .run-unit/state-verdict.json .run-unit/watched-runner.bat .run-unit/watched.born .run-unit/watched.pid .run-unit/why.txt .run-unit/unit460-*
git status --short
