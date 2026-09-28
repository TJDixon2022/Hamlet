cd /c/Source/HamLet
pwd
for f in SHACK_FACTS.md src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs CW_REQUIREMENTS.md CoreHMI.sln MURC.sln
do
  if [ -e "$f" ]
  then
    echo "EXISTS $f"
  else
    echo "ABSENT $f"
  fi
done
git log -1 --format=%H
git status --short
