#!/bin/sh
# Unit 387 - runs docs/carry-forward-tests.txt line 9 EXACTLY AS WRITTEN.
# The command line is never retyped here: it is read from the file and evaluated,
# so nothing this script does can edit it (work instruction 387 section 5).
cd /c/Source/HamLet
eval "$(sed -n '9p' docs/carry-forward-tests.txt)"
