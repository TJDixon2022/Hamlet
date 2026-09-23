cd /c/Source/HamLet
sh .run-unit/unit400-build.sh prosigns "task 3: building the prosigns printer, in memory at 0.02 first"
grep -q " 0 Error(s)" .run-unit/unit400-build-prosigns.txt || { echo "STOP: build failed"; grep -E " error " .run-unit/unit400-build-prosigns.txt | sort -u | head -5 | cut -c1-300; exit 1; }
sh .run-unit/unit400-floors.sh prosigns-printer 300 "FullyQualifiedName~TheProsignsFixtureAtABandTests" "TASK 3 of 5" "task 3: prosigns printer, off disk and in memory at 0.02" --no-build
grep -hE "ROW \|" .run-unit/unit400-prosigns-printer.txt | sed -E "s/^.*ROW/ROW/" | sort -u | cut -c1-400
date +%H:%M:%S
