#!/bin/sh
# unit 426 task 0 - PHASE_STATUS.md names unit 426 and step 7, the version bump, then status.
cd /c/Source/HamLet || exit 1
sed -i "s/^CURRENT_STEP: 3/CURRENT_STEP: 7/; s/^WORK_INSTRUCTION: 425 - the stray letters the key says were never sent/WORK_INSTRUCTION: 426 - the preamp follows the overload after the tune-in/" PHASE_STATUS.md
sed -i "s#<Version>1.13.112</Version>#<Version>1.13.113</Version>#" Directory.Build.props
sed -n 4,5p PHASE_STATUS.md
grep -n "<Version>" Directory.Build.props
sh tools/status.sh EXECUTING "TASK 0 of 4" code none "Unit 426 task 0: version 1.13.113 and PHASE_STATUS set; verifying the instruction against the tree before writing HM-DEC-179"
head -9 PROJECT_STATUS.md
