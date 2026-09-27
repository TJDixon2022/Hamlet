#!/bin/sh
# unit 467 - copy unit 466's helpers, renamed, pointing at unit467- outputs.
cd /c/Source/HamLet || exit 1
for f in .run-unit/unit466-*.sh; do
  n=$(echo "$f" | sed s/unit466/unit467/)
  sed "s/unit466-/unit467-/g" "$f" > "$n"
done
ls .run-unit/unit467-*.sh
