cd /c/Source/HamLet
for f in SHACK_FACTS.md src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs CoreHMI.sln MURC.sln; do
  if test -e "$f"; then echo "exists $f"; else echo "absent $f"; fi
done
pwd -W
echo ---- status.sh
cat tools/status.sh
echo ----
wc -l CLAUDE.md CLAUDE_CODE.md PHASE_PLAN.md PHASE_OUTCOME.md PHASE_STATUS.md
