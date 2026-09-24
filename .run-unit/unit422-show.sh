#!/bin/sh
# unit 422 - print the lines of a run file that carry the star, the connect button, 40 m and any control with no tip.
# Usage: sh .run-unit/unit422-show.sh <run-file>
cd /c/Source/HamLet/.run-unit || exit 1
grep -E "star  :|digits:|connected: |connected    :|band 40 m|none$" "$1" | cut -c1-400
