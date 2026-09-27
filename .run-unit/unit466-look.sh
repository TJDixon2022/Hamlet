#!/bin/sh
# unit 466 - read-only lookups in the tree, rewritten as needed.
cd /c/Source/HamLet || exit 1
grep -n "KeyedRecordings\b.*=\|record Keyed\|class Keyed\|Score(" tests/Hamlet.RadioEngine.Tests/Cw/WhatTheStrayLettersRestOnTests.cs | head -10
grep -rn "static .* Whole(\|StartingPitchHz\b.*=\|static .*Folder" tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/SyntheticCq.cs | head
grep -rn "internal static .*RunPort" tests/Hamlet.RadioEngine.Tests/Cw/*.cs
grep -rn "public static .* Measure(" tests/Hamlet.RadioEngine.Tests/Cw/Instruments/*.cs | head -3
grep -rn "namespace" tests/Hamlet.RadioEngine.Tests/Cw/CwMetrics.cs tests/Hamlet.RadioEngine.Tests/Cw/Instruments/CwPitchInstrument.cs
grep -rn "Folder" tests/Hamlet.RadioEngine.Tests/CapturedSignalTests.cs tests/Hamlet.RadioEngine.Tests/Cw/CapturedSignalTests.cs 2>/dev/null | head -3
grep -rn "WordGap\b.*=\|Unreadable\b.*=" src/Hamlet.RadioEngine/Cw/MorseAlphabet.cs
grep -n "public sealed record CwCharacter" -A 12 src/Hamlet.RadioEngine/Cw/CwCharacter.cs
grep -rn "SecondReading\b" tests --include=*.cs -l
