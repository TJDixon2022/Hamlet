#!/bin/sh
# unit 422 - the source diff against a base: stat, every removed line, and Send's element before and after.
# Usage: sh .run-unit/unit422-diff.sh <base>
cd /c/Source/HamLet || exit 1
echo "== stat against $1"
git diff --stat "$1" -- src
echo "== every removed line in src against $1"
git diff -U0 "$1" -- src | grep -E "^-" | grep -v "^---"
echo "== end removed"
echo "== Send's element at $1"
git show "$1:src/Hamlet.App/Views/MainWindow.axaml" | grep -n -A6 'x:Name="TransmitButton"'
echo "== Send's element now"
grep -n -A7 'x:Name="TransmitButton"' src/Hamlet.App/Views/MainWindow.axaml
