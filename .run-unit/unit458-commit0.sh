#!/bin/sh
# unit 458 - task 0 commit: the record, the runner's writes as they are, the entry round; then push.
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "1 of 4" code none "Task 0 done, every entry figure as 457 left it; task 1 tracing how our text reaches CwMetrics and where the port's text joins it"
git add -A -- .run-unit/reports .run-unit/watched.cpu .run-unit/watched.rc 2>/dev/null
git add -A -- .run-unit/reports .run-unit/watched.cpu
sh .run-unit/unit458-commit.sh .run-unit/unit458-msg0.txt Directory.Build.props PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md PROJECT_STATUS.md docs/phase-requirements/PHASE_OUTCOME.md docs/phase-requirements/PHASE_STATUS.md .run-unit/approach.txt .run-unit/arbiter-prompt.txt .run-unit/arbiter.json .run-unit/denials.txt .run-unit/last-run.json .run-unit/launch.stamp .run-unit/reload.txt .run-unit/s4-prompt.txt .run-unit/s4-verdict.json .run-unit/section4.txt .run-unit/state-prompt.txt .run-unit/state-verdict.json .run-unit/step-plan.txt .run-unit/step-states.txt .run-unit/watched-runner.bat .run-unit/watched.born .run-unit/watched.pid .run-unit/why.txt .run-unit/unit458-*
git status --short
