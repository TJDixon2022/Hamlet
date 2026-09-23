cd /c/Source/HamLet
grep -E "moves," .run-unit/unit401-disp-changeb.txt | sed -E "s/^\s+//" | sort -u | cut -c1-200
sh .run-unit/unit401-floors.sh floors-t2-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 2 of 5" "task 2: change b at 6 of 6 uncommitted; captures type running against entry" --no-build
sh .run-unit/unit401-floors.sh floors-t2-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "TASK 2 of 5" "task 2: change b uncommitted; adjudicated type running" --no-build
sh .run-unit/unit401-types.sh t2 "TASK 2 of 5" "task 2: change b uncommitted;" fixtures gate
echo "captures against entry:"; sh .run-unit/unit401-cmp.sh .run-unit/unit401-floors-1.txt .run-unit/unit401-floors-t2-1.txt
echo "adjudicated against entry:"; sh .run-unit/unit401-adjcmp.sh .run-unit/unit401-floors-2.txt .run-unit/unit401-floors-t2-2.txt
echo "fixtures against entry:"; diff .run-unit/unit401-fixtures-entry-list.txt .run-unit/unit401-fixtures-t2-list.txt; echo "diff rc $?"
echo "gate against entry:"; diff .run-unit/unit401-gate-entry-list.txt .run-unit/unit401-gate-t2-list.txt; echo "diff rc $?"
date +%H:%M:%S
