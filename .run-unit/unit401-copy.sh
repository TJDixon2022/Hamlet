cd /c/Source/HamLet
printf 'TASK 0 of 5\n' > .run-unit/unit401-task.txt
sed -i 's/^CURRENT_STEP: .*$/CURRENT_STEP: 3/' PHASE_STATUS.md
sed -i "s/^WORK_INSTRUCTION: .*$/WORK_INSTRUCTION: 401 - the pile is closed out on R49's letter/" PHASE_STATUS.md
sh tools/status.sh EXECUTING "TASK 0 of 5" code none "task 0: PHASE_STATUS read CURRENT_STEP 0 and unit 400, now 3 and 401; copying unit 400 scripts to unit401, entry diff next"
date "+%H:%M:%S start"
for f in floors carry build commit transmit cmp adjcmp; do
  sed "s/unit400/unit401/g" .run-unit/unit400-$f.sh > .run-unit/unit401-$f.sh
done
ls .run-unit/unit401-*
grep -nE "^(CURRENT_STEP|WORK_INSTRUCTION)" PHASE_STATUS.md
cat .run-unit/unit400-cmp.sh .run-unit/unit400-adjcmp.sh
