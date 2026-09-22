#!/bin/sh
# Unit 388 - runs docs/carry-forward-tests.txt line 7 EXACTLY AS WRITTEN.
# The command line is never retyped here: it is read from the file and evaluated,
# so nothing this script does can edit it (work instruction 388 section 5).
cd /c/Source/HamLet
eval "$(sed -n '7p' docs/carry-forward-tests.txt)" 2>&1 | grep -E -A 8 "^\s*Failed |Passed!|Failed!|error CS" | grep -v -E "^\s*at |Stack Trace|^--$"
