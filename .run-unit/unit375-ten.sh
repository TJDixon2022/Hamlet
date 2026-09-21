#!/bin/sh
# Unit 375 task 1: run one whole type N times, filtered, foregrounded, one build each.
# Shape copied from .run-unit/unit372-flake.sh. Records the count per run, and for every
# red the name and the failure verbatim. This task builds nothing and repairs nothing.
# Usage: sh .run-unit/unit375-ten.sh <TypeName> <count> <first-run-number>
type=$1
count=$2
n=$3
i=1
while [ "$i" -le "$count" ]; do
    echo "===== RUN $n ====="
    timeout 480 dotnet test tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj \
        --filter "FullyQualifiedName~$type" \
        2>&1 | grep -E "Passed!|Failed!|Failed Hamlet|Error Message|Assert\.|Expected|Actual|InvalidProgramException|dispatcher loop|\[FAIL\]"
    i=$((i + 1))
    n=$((n + 1))
done
