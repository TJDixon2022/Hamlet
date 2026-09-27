#!/bin/sh
# unit 470 - compare two saved texts letter by letter with the class set aside, then count class changes.
# Usage: sh .run-unit/unit470-letters.sh <before-suffix> <after-suffix>
cd /c/Source/HamLet || exit 1
export LC_ALL=C
cut -d"|" -f3-5 .run-unit/unit470-text-$1.txt > .run-unit/unit470-letters-a.tmp
cut -d"|" -f3-5 .run-unit/unit470-text-$2.txt > .run-unit/unit470-letters-b.tmp
cmp .run-unit/unit470-letters-a.tmp .run-unit/unit470-letters-b.tmp && echo "letters IDENTICAL, classes aside"
echo "== class changes, before -> after"
cut -d"|" -f6 .run-unit/unit470-text-$1.txt > .run-unit/unit470-class-a.tmp
cut -d"|" -f6 .run-unit/unit470-text-$2.txt > .run-unit/unit470-class-b.tmp
paste -d">" .run-unit/unit470-class-a.tmp .run-unit/unit470-class-b.tmp | sort | uniq -c
echo "== recordings with a class change"
paste -d"|" .run-unit/unit470-text-$1.txt .run-unit/unit470-class-b.tmp | awk -F"|" '$6 != $8 {print $3}' | sort | uniq -c
