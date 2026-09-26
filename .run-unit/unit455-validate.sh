#!/bin/sh
# unit 455 - run the report validator on output.md and keep what it says.
cd /c/Source/HamLet || exit 1
cmd //c "tools\arbiter\validate-output.bat C:\Source\HamLet\output.md" > .run-unit/unit455-validate.txt 2>&1
echo "RC=$?"
tail -25 .run-unit/unit455-validate.txt
