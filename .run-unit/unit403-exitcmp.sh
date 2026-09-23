#!/bin/sh
# Work instruction 403, task 4: exit captures rows against entry rows.
cd /c/Source/HamLet/.run-unit
grep -E " characters against a floor of " unit403-exit-captures.txt | sed 's/^ *//' | sort > unit403-rows-exit.txt
cmp unit403-rows-head.txt unit403-rows-exit.txt && echo "exit rows identical to entry, $(wc -l < unit403-rows-exit.txt) rows"
