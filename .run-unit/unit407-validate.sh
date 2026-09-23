cd /c/Source/HamLet
cmd //c "tools\\arbiter\\validate-output.bat C:\\Source\\HamLet\\output.md" > .run-unit/unit407-validate.txt 2>&1
echo "rc $?"
tail -25 .run-unit/unit407-validate.txt
