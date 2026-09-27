#!/bin/sh
# prints the named files with a header each
cd /c/Source/HamLet/.run-unit || exit 1
for f in "$@"; do
  echo "##### $f"
  cat "$f"
done
