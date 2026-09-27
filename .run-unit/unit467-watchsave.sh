#!/bin/sh
# unit 467 task 1 - the re-watch printouts: red against an empty table, green with CwSwitchTable.Rows; harness then live.
cd /c/Source/HamLet || exit 1
for k in watch-red green
do
  {
    echo "== HM-REQ-128 part (b), $k - harness (EveryHarnessRowIsNoWorseThanTheBetterDecoderAlone)"
    sh .run-unit/unit467-brief.sh b-harness-$k
    grep -a -E "^\s*(real|synthetic).*: (the table says|emitted worse|not classified|listed as)" .run-unit/unit467-b-harness-$k.txt | sed -E "s/^\s+//" | sort -u
    echo "== HM-REQ-128 part (b), $k - live (TheLiveRowIsNoWorseThanTheBetterDecoderAlone, ThePortAloneOnTheLivePathEmitsThePortsOwnReading)"
    sh .run-unit/unit467-brief.sh b-live-$k
  } > .run-unit/unit467-$k.txt
  wc -l .run-unit/unit467-$k.txt
done
