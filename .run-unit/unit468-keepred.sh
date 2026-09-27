#!/bin/sh
# unit 468 - keep each earlier proof run before a rerun overwrites it:
# red-first caught the proof's one-sample edge bias on TX-ITU at 25 WPM; red-second is red after that fix;
# green-first failed two 25 WPM word gaps on a fixed 0.02-unit tolerance tighter than one sample per edge.
cd /c/Source/HamLet || exit 1
cp .run-unit/unit468-senders-red.txt .run-unit/unit468-senders-red-second.txt
cp .run-unit/unit468-senders-green.txt .run-unit/unit468-senders-green-first.txt
ls -l .run-unit/unit468-senders-*
