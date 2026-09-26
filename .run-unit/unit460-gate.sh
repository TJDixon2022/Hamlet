cd /c/Source/HamLet
for f in SHACK_FACTS.md src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs CW_REQUIREMENTS.md CW_SPEC.md CoreHMI.sln MURC.sln
do
  if test -e "$f"; then echo "EXISTS $f"; else echo "MISSING $f"; fi
done
pwd -W
git log --oneline -8
git status --short
echo "--- SESSION.lock"
cat SESSION.lock
date -u
