#!/bin/sh
# unit 452 - each named floor row's scored region, as the operator reads it, from one named printout.
# Usage: sh .run-unit/unit452-regions.sh <named.txt>
cd /c/Source/HamLet || exit 1
grep -a "^ *named | " "$1" | sed -E "s/.*named [|] ([^|]*) [|].*region (.*)/\\1 \\2/" | sort -u | cut -c1-220
