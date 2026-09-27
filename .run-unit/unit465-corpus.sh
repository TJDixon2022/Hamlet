#!/bin/sh
# unit 465 - the corpus through the arbiter: the arbitrated transcript against task 0's save of ours, the arbiter's counts beside task 1's.
# Usage: sh .run-unit/unit465-corpus.sh <suffix> "<n of 4>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit465-run.sh "corpus-$1" engine 600 "$2" "Task $2 - all 35 through the arbiter under the vote table; comparing with ours saved at task 0" "FullyQualifiedName~.WhereTheTwoReadingsMeetFact.TheCorpusThroughTheArbiter" --no-build
grep -a "^ *save | ours | " .run-unit/unit465-corpus-$1.txt | sed 's/^ *//' > .run-unit/unit465-arbitrated-$1.txt
wc -l .run-unit/unit465-arbitrated-$1.txt
cmp .run-unit/unit465-text-before.txt .run-unit/unit465-arbitrated-$1.txt && echo "ARBITRATED TEXT, CLASS AND P BYTE-IDENTICAL to ours at task 0"
grep -a -c "harvest check | .* | same |" .run-unit/unit465-corpus-$1.txt
grep -a "harvest check | .* | DIFFERS" .run-unit/unit465-corpus-$1.txt
echo "== arbiter's counts against task 1's"
grep -a "^ *arbiter count |" .run-unit/unit465-corpus-$1.txt | sed 's/^ *//' | cut -d'|' -f2-9 > .run-unit/unit465-counts-arbiter-$1.txt
grep -a "^count |" .run-unit/unit465-meet.txt | cut -d'|' -f2-9 > .run-unit/unit465-counts-task1.txt
diff .run-unit/unit465-counts-task1.txt .run-unit/unit465-counts-arbiter-$1.txt && echo "COUNTS IDENTICAL to task 1"
grep -a "^ *arbiter count | real HF, all\|^ *arbiter count | synthetic, all" .run-unit/unit465-corpus-$1.txt | cut -c1-260
