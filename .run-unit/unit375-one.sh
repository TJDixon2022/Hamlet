#!/bin/sh
# Unit 375 task 4: ONE invocation of the carry-forward list, for re-running a run that the
# headless dispatcher loop killed before any assertion (work instruction 375 section 6, second
# ruling: that is a lost run, it is re-run, and it counts neither toward the five nor against).
# Status is written immediately before it, as for every other invocation.
# The command line is read out of docs/carry-forward-tests.txt itself, line 7 or line 9.
# Usage: sh .run-unit/unit375-one.sh app|engine <label>
which=$1
label=$2

if [ "$which" = "app" ]; then
    line=7
else
    line=9
fi

cmd=$(sed -n "${line}p" docs/carry-forward-tests.txt)

echo "===== $label - $which ====="
sh tools/status.sh "running" "TASK 4 of 4" "claude" "none" "Task 4 $label: $which invocation of the carry-forward list."
eval "$cmd" 2>&1 | grep -A 10 -E "Passed!|Failed!|Failed Hamlet|Error Message|\[FAIL\]"
