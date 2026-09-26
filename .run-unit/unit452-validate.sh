#!/bin/sh
# unit 452 - run the arbiter's output validator on output.md.
cd /c/Source/HamLet || exit 1
cmd //c "tools\\arbiter\\validate-output.bat output.md" > .run-unit/unit452-validate.txt 2>&1
echo "RC=$?"
cat .run-unit/unit452-validate.txt
