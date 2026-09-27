#!/bin/sh
# unit 469 - the INT-* proof alone, one invocation; "red" sets the nominal mixer through the environment.
# Usage: sh .run-unit/unit469-int.sh <red|green> <suffix> "<n of 3>"
cd /c/Source/HamLet || exit 1
export LC_ALL=C
T="FullyQualifiedName~.TheInterferenceProfilesAreWhatTheySayTests."
case "$1" in
  red)
    HAMLET_UNIT469_INTERFERENCE=nominal sh .run-unit/unit469-run.sh "int-red-$2" engine 600 "$3" "Task 1 - the INT-* proof against the nominal mixer, which must be red on every case" "$T" --no-build
    ;;
  green)
    sh .run-unit/unit469-run.sh "int-green-$2" engine 600 "$3" "Task 1 - the INT-* proof against CwInterference" "$T" --no-build
    ;;
esac
cp .run-unit/unit469-int-$1-$2.txt .run-unit/unit469-int-$1.txt
grep -a -E "^\s*(FAIL|verdict) \|" .run-unit/unit469-int-$1-$2.txt | sed -E "s/^\s+//" | cut -c1-260 | sort -u | head -80
