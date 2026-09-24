#!/bin/sh
# unit 424 task 0 - PHASE_STATUS.md names unit 424 and step 7, the version bump, then status.
cd /c/Source/HamLet || exit 1
sed -i "s/^CURRENT_STEP: 3/CURRENT_STEP: 7/; s/^WORK_INSTRUCTION: 423 - the window holds still outside his privileges/WORK_INSTRUCTION: 424 - the preamp is what the manual says, and nothing argues with it/" PHASE_STATUS.md
sed -i "s#<Version>1.13.110</Version>#<Version>1.13.111</Version>#" Directory.Build.props
sed -n 4,5p PHASE_STATUS.md
grep -n "<Version>" Directory.Build.props
sh tools/status.sh EXECUTING "TASK 0 of 4" code none "Unit 424 task 0: record written as HM-DEC-177, version 1.13.111, committing before the preamp trace"
head -9 PROJECT_STATUS.md
