cd /c/Source/HamLet
sed "s/$/\r/" .run-unit/unit402-outcome2.txt >> PHASE_OUTCOME.md
tail -c 300 PHASE_OUTCOME.md
sh .run-unit/unit402-transmit.sh
sh .run-unit/unit402-commit.sh "unit402 task 4: exit round - app 278 of 278 counting two runs of dispatcher losses neither way, engine 178 of 178 in 371 s; floors 37 of 37 every row identical to entry, 13 of 13, 2 of 2; CwEmissionGateTests 8 of 8, CwAdjudicationTests 11 of 11, the other four types identical; transmit files unchanged, src only CwDecoder.cs, App untouched; no regression, 3.5 stands, 3.6 not ticked at 2 of 8" "TASK 4 of 5" "task 4 committed; writing output.md" docs/phase-cw/unit402-reds.md PHASE_OUTCOME.md
date +%H:%M:%S
