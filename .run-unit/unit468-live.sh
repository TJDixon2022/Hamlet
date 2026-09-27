#!/bin/sh
# unit 465 - the corpus through the live CwDecoder path: the transcript against task 0's save of ours, the port's builds, decode time.
# Usage: sh .run-unit/unit468-live.sh <suffix> "<n of 4>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit468-run.sh "live-$1" engine 600 "$2" "Task 3 - all 35 through the live CwDecoder path, ours alone and with the port and arbiter; decode time before and after" "FullyQualifiedName~.WhereTheTwoReadingsMeetFact.TheLivePathOverTheCorpus" --no-build
grep -a "^ *save | ours | " .run-unit/unit468-live-$1.txt | sed 's/^ *//' > .run-unit/unit468-livetext-$1.txt
wc -l .run-unit/unit468-livetext-$1.txt
cmp .run-unit/unit468-text-before.txt .run-unit/unit468-livetext-$1.txt && echo "LIVE PATH TEXT, CLASS AND P BYTE-IDENTICAL to ours at task 0"
grep -a -E "^ *(live|decode time) \|" .run-unit/unit468-live-$1.txt | sed 's/^ *//' | cut -c1-240
