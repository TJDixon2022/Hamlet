#!/bin/sh
# unit 416 - on the kept build, how many of the 52 recordings really settled what the narrowed emulation predicts,
# counting only the removals (the narrowed relabel never adds a space).
# Usage: sh .run-unit/unit416-real.sh <suffix>
cd /c/Source/HamLet || exit 1
F=.run-unit/unit416-relabel-$1.txt
grep -a -E "^ text \| .* \| entry  " $F | sed -E "s/^ text \| ([^|]+) \| entry  /\1|/" | sort > .run-unit/unit416-text-entry-$1.txt
grep -a -E "^ text \| .* \| real   " $F | sed -E "s/^ text \| ([^|]+) \| real   /\1|/" | sort > .run-unit/unit416-text-real-$1.txt
grep -a -E "^ text \| .* \| after  " $F | sed -E "s/^ text \| ([^|]+) \| after  /\1|/" | sort > .run-unit/unit416-text-after-$1.txt
echo "recordings: $(wc -l < .run-unit/unit416-text-real-$1.txt)"
echo "real equal to the old loop (entry): $(comm -12 .run-unit/unit416-text-entry-$1.txt .run-unit/unit416-text-real-$1.txt | wc -l)"
echo "real equal to the emulation with adds (after): $(comm -12 .run-unit/unit416-text-after-$1.txt .run-unit/unit416-text-real-$1.txt | wc -l)"
echo "== letters only, real against entry, spaces stripped, differing recordings:"
sed "s/ //g" .run-unit/unit416-text-entry-$1.txt > .run-unit/unit416-text-entry-$1.nosp
sed "s/ //g" .run-unit/unit416-text-real-$1.txt > .run-unit/unit416-text-real-$1.nosp
diff .run-unit/unit416-text-entry-$1.nosp .run-unit/unit416-text-real-$1.nosp | grep -c "^<"
