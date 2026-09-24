#!/bin/sh
# unit 423 task 0 - PHASE_STATUS.md names unit 423 and step 6, the version bump, then status.
cd /c/Source/HamLet || exit 1
sed -i "s/^CURRENT_STEP: 3/CURRENT_STEP: 6/; s/^WORK_INSTRUCTION: 422 - every control says what it does/WORK_INSTRUCTION: 423 - the window holds still outside his privileges/" PHASE_STATUS.md
sed -i "s#<Version>1.13.109</Version>#<Version>1.13.110</Version>#" Directory.Build.props
sed -n 4,5p PHASE_STATUS.md
grep -n "<Version>" Directory.Build.props
sh tools/status.sh EXECUTING "TASK 0 of 4" code none "Unit 423 task 0: record written, version 1.13.110, building Hamlet.sln for the entry round"
head -9 PROJECT_STATUS.md
