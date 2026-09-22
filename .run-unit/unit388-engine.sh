#!/bin/sh
# Unit 388 - runs docs/carry-forward-tests.txt line 9 EXACTLY AS WRITTEN.
# The command line is never retyped here: it is read from the file and evaluated,
# so nothing this script does can edit it (work instruction 388 section 5).
cd /c/Source/HamLet
eval "$(sed -n '9p' docs/carry-forward-tests.txt)" 2>&1 | grep -E "^\s*Failed |Passed!|Failed!|error CS|dispatcher loop|Assert\.|Total tests"
