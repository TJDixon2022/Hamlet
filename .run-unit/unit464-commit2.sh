#!/bin/sh
# unit 464 - task 2 commit: the property, the map, the adapter, the stream line, the test and CwCalibration it needs, this unit's run files; then push.
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "2 of 4" code none "Task 2 gate green, texts byte-identical; committing the property, the adapter, the test and the constants"
git add -- src/Hamlet.RadioEngine/Cw/CwCharacter.cs src/Hamlet.RadioEngine/Cw/CwCharacterProbability.cs src/Hamlet.RadioEngine/Cw/FldigiConfidence.cs src/Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs tests/Hamlet.RadioEngine.Tests/Cw/EveryCharacterCarriesAConfidenceTests.cs tests/Hamlet.RadioEngine.Tests/Cw/CwCalibration.cs PROJECT_STATUS.md || exit 1
for f in .run-unit/unit464-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git status --short | grep -v "^??"
git status --short | grep "^??"
git commit -q -F .run-unit/unit464-msg2.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
