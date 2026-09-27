cd /c/Source/HamLet
for f in SHACK_FACTS.md src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs CW_REQUIREMENTS.md CW_SPEC.md CoreHMI.sln MURC.sln
do
  if [ -e "$f" ]
  then echo "EXISTS $f"
  else echo "ABSENT $f"
  fi
done
pwd -W
git log --oneline -3
git status --short
