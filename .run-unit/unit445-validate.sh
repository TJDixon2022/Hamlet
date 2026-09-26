#!/bin/sh
# unit 445 - validate output.md with the arbiter's validator.
cd /c/Source/HamLet || exit 1
cmd //c "tools\\arbiter\\validate-output.bat output.md"
echo "RC=$?"
