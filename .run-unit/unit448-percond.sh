#!/bin/sh
# unit 448 - per condition, MET-CER-SURE and MET-WBE from one metrics printout.
# Usage: sh .run-unit/unit448-percond.sh <metrics.txt>
cd /c/Source/HamLet || exit 1
grep -h "^ *condition | real | real HF\|^ *condition | synthetic | synthetic" "$1" | awk -F' [|] ' 'NF >= 13 && $5 ~ /^[0-9]+$/ {
  s = $4; sub(/.*sender /, "", s); sub(/ \(.*$/, "", s);
  if ($4 ~ /character gap 5/) s = "char 5"; else if ($4 ~ /1:3:1:3:7/) s = "ITU";
  lvl = $4; sub(/ in the passband.*/, "", lvl); sub(/.* /, "", lvl);
  if ($3 == "synthetic") s = s " " lvl;
  printf "%s %s | %s | CER-SURE %s of %s = %s | WBE %s of %s = %s\n", $3, s, $5, $6, $5, $7, $11, $10, $12 }'
