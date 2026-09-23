cd /c/Source/HamLet
sh .run-unit/unit402-build.sh printer "task 1: building the eight-reds printer, asserting nothing"
sh .run-unit/unit402-floors.sh eight 600 "FullyQualifiedName~TheEightRedsTests" "TASK 1 of 5" "task 1: eight-reds printer running, groups A speed polls, B easy tier three events, C qsk preamble, D shares" --no-build
grep -E "^ (A|B|C|D) \|" .run-unit/unit402-eight.txt > .run-unit/unit402-eight-rows.txt
wc -l .run-unit/unit402-eight-rows.txt
grep -E "error CS" .run-unit/unit402-build-printer.txt | sort -u | head
date +%H:%M:%S
