#!/bin/sh
# unit 433 - read-only looks at the record: unit 432's task 0 diff and the launcher's pending root diffs.
cd /c/Source/HamLet || exit 1
echo "== unit 432 task 0"
git diff 134b715e~1 134b715e -- PHASE_OUTCOME.md PHASE_STATUS.md docs/phase-correctness/PARKED.md Directory.Build.props
echo "== pending root diffs, PHASE_STATUS and PHASE_OUTCOME"
git diff -- PHASE_STATUS.md PHASE_OUTCOME.md | head -80
