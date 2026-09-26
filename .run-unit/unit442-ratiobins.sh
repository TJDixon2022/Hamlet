#!/bin/sh
# unit 442 - the window marks' unit over the path's unit, from the rival trace, wrong or added against right.
# Bins set before reading: under 0.33, 0.33-0.50, 0.50-0.67, 0.67-0.80, 0.80-1.25, over 1.25, marks cannot say.
cd /c/Source/HamLet || exit 1
grep "^ *rival | " "$1" | grep -v "rival | recording" | awk -F' [|] ' '
  { r = $11; k = ($6 == "right") ? "right" : "bad";
    if (r !~ /^[0-9.]+$/) b = "7 marks cannot say";
    else if (r < 0.33) b = "1 under 0.33"; else if (r < 0.5) b = "2 0.33 to 0.50"; else if (r < 0.67) b = "3 0.50 to 0.67";
    else if (r < 0.8) b = "4 0.67 to 0.80"; else if (r <= 1.25) b = "5 0.80 to 1.25"; else b = "6 over 1.25";
    n[b "|" k]++; bins[b] = 1 }
  END { print "ratio bin | wrong or added | right"; for (b in bins) printf "%s | %d | %d\n", b, n[b "|bad"], n[b "|right"] }' | sort
