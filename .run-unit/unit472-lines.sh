#!/bin/sh
# unit 472 - print a printout's lines whose first field matches, leading spaces trimmed, cut to a width.
# Usage: sh .run-unit/unit472-lines.sh <file under .run-unit> "<first-field regex>" [width]
cd /c/Source/HamLet/.run-unit || exit 1
export LC_ALL=C
grep -a -E "^ *($2) \|" "$1" | sed 's/^ *//' | cut -c1-"${3:-400}"
