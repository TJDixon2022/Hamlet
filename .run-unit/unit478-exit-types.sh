#!/bin/sh
# unit 478 task 3 - every type this unit touched, one named type per invocation, each with its own timeout (HM-DEC-155). All app; the engine was not touched.
cd /c/Source/HamLet || exit 1
for t in TheScopeIsTheMiddlePictureTests TheScopeShowsTheMarksTests TheVerdictCarriesTheScopeTests TheOwnersVerdictIsARowTests BindingHealthTests EveryControlSaysWhatItDoesTests WhatEveryControlSaysOnHoverTests
do
  echo "== $t"
  sh .run-unit/unit478-run.sh "exit-$t" 300 "3 of 3" "Task 3 exit round: $t, alone" "$t" app --no-build | grep -E "^RC=|Total tests|Passed:|Failed:|^Failed "
done
