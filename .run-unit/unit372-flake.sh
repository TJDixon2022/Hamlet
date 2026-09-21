#!/bin/sh
# Unit 372 task 1 item 3: how often the named flake is red, on the single name.
# Step 2's criterion 2.3 evidence. This unit records it and does not repair it (R14).
i=1
while [ "$i" -le 6 ]; do
    timeout 480 dotnet test tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj \
        --filter "FullyQualifiedName~TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns" \
        2>&1 | grep -E "Passed!|Failed!"
    i=$((i + 1))
done
