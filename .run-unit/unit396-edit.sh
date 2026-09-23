cd /c/Source/HamLet
# Edits the copied scripts: the doc rows also land in unit396-rework.md, the task label overridable by TL.
sed -i -e "s#git add -- docs/phase-cw/unit395-rework.md#git add -- docs/phase-cw/unit395-rework.md docs/phase-cw/unit396-rework.md#" .run-unit/unit396-out.sh
sed -i -e "s/\"TASK 2 of 4\"/\"\${TL:-TASK 2 of 4}\"/g" .run-unit/unit396-piece.sh .run-unit/unit396-measure.sh .run-unit/unit396-build.sh .run-unit/unit396-drop.sh
sed -i -e "s#unit394-transmit.sh#unit396-transmit.sh#" .run-unit/unit396-measure.sh
grep -n "TL:-\|git add\|transmit.sh" .run-unit/unit396-*.sh
