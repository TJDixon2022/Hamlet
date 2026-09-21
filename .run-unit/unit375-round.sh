#!/bin/sh
# Unit 375 task 4: one round of the carry-forward list - both invocations, one build each,
# status written immediately before each one (work instruction 375 section 7).
#
# The two command lines are READ OUT OF docs/carry-forward-tests.txt ITSELF, lines 7 and 9,
# rather than transcribed here: section 9 parks tools/run-carry-forward.sh because it does not
# match the list, and a second copy of the filter in the scratchpad would be the same fault.
# Usage: sh .run-unit/unit375-round.sh <round label>
round=$1

app=$(sed -n '7p' docs/carry-forward-tests.txt)
engine=$(sed -n '9p' docs/carry-forward-tests.txt)

echo "===== ROUND $round - APP ====="
sh tools/status.sh "running" "TASK 4 of 4" "claude" "none" "Task 4 round $round: app invocation of the carry-forward list."
eval "$app" 2>&1 | grep -E "Passed!|Failed!|Failed Hamlet|Error Message|Assert\.|Expected|Actual|InvalidProgramException|dispatcher loop|\[FAIL\]"

echo "===== ROUND $round - ENGINE ====="
sh tools/status.sh "running" "TASK 4 of 4" "claude" "none" "Task 4 round $round: engine invocation of the carry-forward list."
eval "$engine" 2>&1 | grep -E "Passed!|Failed!|Failed Hamlet|Error Message|Assert\.|Expected|Actual|InvalidProgramException|dispatcher loop|\[FAIL\]"
