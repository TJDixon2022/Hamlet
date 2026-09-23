cd /c/Source/HamLet
sh .run-unit/unit399-build.sh ways "task 1: printer written, building before ways 1 to 3"
sh .run-unit/unit399-floors.sh ways 600 "FullyQualifiedName~TheCleanSyntheticsFourWaysTests" "TASK 1 of 5" "task 1: ways 1 to 3 running, both fixtures, off disk and at 0.01, 0.02, 0.04" --no-build
grep -hE "ROW \|" .run-unit/unit399-ways.txt | sed -E "s/^\s+//"
date +%H:%M:%S
