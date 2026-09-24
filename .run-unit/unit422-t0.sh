#!/bin/sh
# unit 422 task 0 - PHASE_STATUS.md names unit 422 and step 6, then status.
cd /c/Source/HamLet || exit 1
sed -i "s/^CURRENT_STEP: 3/CURRENT_STEP: 6/; s/^WORK_INSTRUCTION: 421 - a floor counts what the decoder was sure of/WORK_INSTRUCTION: 422 - every control says what it does/" PHASE_STATUS.md
sed -n 4,5p PHASE_STATUS.md
sh tools/status.sh EXECUTING "TASK 0 of 4" code none "Unit 422 started: writing the record, P19 into PARKED.md and the version bump before the entry round"
head -9 PROJECT_STATUS.md
grep -n "Version>" Directory.Build.props
