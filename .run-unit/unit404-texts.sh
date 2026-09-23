#!/bin/sh
# unit 404 - compare the printer's TEXT and SETTLED lines of a run against entry.
# Usage: sh .run-unit/unit404-texts.sh <printer outname>
cd /c/Source/HamLet || exit 1
grep -E "SETTLED \[|TEXT +\[" .run-unit/unit404-printer-entry.txt > .run-unit/unit404-printer-entry-texts.txt
grep -E "SETTLED \[|TEXT +\[" ".run-unit/unit404-$1.txt" > ".run-unit/unit404-$1-texts.txt"
diff .run-unit/unit404-printer-entry-texts.txt ".run-unit/unit404-$1-texts.txt" && echo "TEXTS IDENTICAL TO ENTRY"
