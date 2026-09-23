#!/bin/sh
# Unit 391 task 0 entry round: docs/carry-forward-tests.txt lines 7 and 9, unedited, one build each.
cd /c/Source/HamLet || exit 1
which=$1
if [ "$which" = "app" ]; then
  sh tools/status.sh EXECUTING "TASK 0 of 4" code none "Entry round: app carry-forward line 7 running now, one build; engine line next"
  start=$(date +%s)
  sed -n 7p docs/carry-forward-tests.txt | sh > .run-unit/unit391-entry-app.txt 2>&1
  echo "rc=$? seconds=$(( $(date +%s) - start ))" >> .run-unit/unit391-entry-app.txt
  tail -5 .run-unit/unit391-entry-app.txt
else
  sh tools/status.sh EXECUTING "TASK 0 of 4" code none "Entry round: engine carry-forward line 9 running now, one build; the app line is done"
  start=$(date +%s)
  sed -n 9p docs/carry-forward-tests.txt | sh > .run-unit/unit391-entry-engine.txt 2>&1
  echo "rc=$? seconds=$(( $(date +%s) - start ))" >> .run-unit/unit391-entry-engine.txt
  tail -5 .run-unit/unit391-entry-engine.txt
fi
