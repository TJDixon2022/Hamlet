#!/bin/sh
# Unit 389 task 1: step 9's four carry-forward types, together, one build, every name's result.
timeout 480 dotnet test tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj --logger "console;verbosity=normal" --filter "FullyQualifiedName~TheCarrierHoldsTheButtonsTests|FullyQualifiedName~TheCardOffersLogAndAnXTests|FullyQualifiedName~TheTurnMovesOnEveryHandBackTests|FullyQualifiedName~TheFourAreOnOliviaCardsTooTests" 2>&1 | grep -E "^\s*(Passed|Failed) |Passed!|Failed!|error CS|dispatcher loop"
