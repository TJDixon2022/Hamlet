cd /c/Source/HamLet
sh .run-unit/unit401-build.sh printer "task 1: building with the four-ways printer added"
sh .run-unit/unit401-floors.sh fourways 300 "FullyQualifiedName~TheDisplacementFloorFourWaysTests" "TASK 1 of 5" "task 1: four-ways printer running, 26 decodes and #24" --no-build
grep -E "ROW" .run-unit/unit401-fourways.txt | sed -E "s/^\s+//" | cut -c1-260
date +%H:%M:%S
