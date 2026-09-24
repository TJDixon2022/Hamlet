#!/bin/sh
# unit 422 - the not-connected half of an inventory run, one control a line, tip cut to 200.
# Usage: sh .run-unit/unit422-table.sh <inventory-file>
cd /c/Source/HamLet/.run-unit || exit 1
sed -n "/NOT CONNECTED/,/CONNECTED TO THE TRAINING/p" "$1" | grep -E "^ (CW tab|band row) [|]" | sed -E "s/ [|] does: .*//" | cut -c1-260
