cd /c/Source/HamLet
for f in SHACK_FACTS.md src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs CW_REQUIREMENTS.md CW_SPEC.md CoreHMI.sln MURC.sln
do
  if [ -e "$f" ]
  then echo "EXISTS $f"
  else echo "MISSING $f"
  fi
done
pwd
git log --oneline -1
wc -l CLAUDE.md CLAUDE_CODE.md CW_REQUIREMENTS.md CW_SPEC.md PHASE_PLAN.md
