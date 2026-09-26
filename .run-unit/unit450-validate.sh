#!/bin/sh
# unit 450 - run the arbiter's output validator on output.md.
cd /c/Source/HamLet || exit 1
cmd //c "tools\\arbiter\\validate-output.bat output.md" > .run-unit/unit450-validate.txt 2>&1
echo "RC=$?"
cat .run-unit/unit450-validate.txt
