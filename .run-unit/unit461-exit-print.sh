#!/bin/sh
# unit 461 - the exit round's prints in one file: metric totals and rows against entry, src, Cw/Second, the transmit files, texts and git status.
cd /c/Source/HamLet || exit 1
OUT=.run-unit/unit461-tx-exit.txt
: > $OUT
echo "== metric totals, entry then exit" >> $OUT
grep -a -E "^ total \| (real|synthetic)" .run-unit/unit461-metrics-entry.txt >> $OUT
grep -a -E "^ total \| (real|synthetic)" .run-unit/unit461-metrics-exit.txt >> $OUT
echo "== metric rows: entry against exit (nothing between the markers means none moved)" >> $OUT
grep -a -E "^ (row|condition) \|" .run-unit/unit461-metrics-entry.txt > .run-unit/unit461-rows-entry.tmp
grep -a -E "^ (row|condition) \|" .run-unit/unit461-metrics-exit.txt > .run-unit/unit461-rows-exit.tmp
diff .run-unit/unit461-rows-entry.tmp .run-unit/unit461-rows-exit.tmp >> $OUT
echo "== end rows" >> $OUT
echo "== git diff e6140891 -- src (empty means no src line changed over the unit):" >> $OUT
git diff e6140891 -- src >> $OUT
echo "== end src" >> $OUT
echo "== git diff e6140891 -- src/Hamlet.RadioEngine/Cw/Second/ (empty):" >> $OUT
git diff e6140891 -- src/Hamlet.RadioEngine/Cw/Second/ >> $OUT
echo "== end Cw/Second" >> $OUT
C=src/Hamlet.RadioEngine/Cw
echo "== git diff 7e209cb4 over the eleven transmit files PARKED.md names (empty):" >> $OUT
git diff 7e209cb4 -- $C/CwTransmitter.cs $C/KeyerCwSender.cs $C/TransmitChain.cs $C/AutoCall.cs $C/AutoCallAnswers.cs $C/CwTransmitGuard.cs $C/TransmissionWatch.cs $C/TransmitReadiness.cs $C/TransmitPrivileges.cs $C/TransmitNotes.cs $C/ICwSender.cs >> $OUT
for f in CwTransmitter KeyerCwSender TransmitChain AutoCall AutoCallAnswers CwTransmitGuard TransmissionWatch TransmitReadiness TransmitPrivileges TransmitNotes ICwSender
do
  [ -f "$C/$f.cs" ] || echo "missing $C/$f.cs" >> $OUT
done
echo "== end transmit" >> $OUT
echo "== files this unit changed, e6140891 to HEAD, outside .run-unit:" >> $OUT
git diff --stat e6140891 HEAD -- . ":(exclude).run-unit" >> $OUT
echo "== our texts: every recording's text at exit against task 0's save" >> $OUT
diff .run-unit/unit461-text-before.sorted.txt .run-unit/unit461-text-exit.sorted.txt >> $OUT && echo "(no difference, 126 lines)" >> $OUT
echo "== git status --short at exit" >> $OUT
git status --short >> $OUT
echo "== commits this unit" >> $OUT
git log --oneline e6140891..HEAD >> $OUT
cat $OUT
