cd /c/Source/HamLet
# Dump 7e209cb4's Cw folder to .run-unit/at7e for reading; nothing in the tree moves.
mkdir -p .run-unit/at7e
for p in $(git ls-tree --name-only 7e209cb4 src/Hamlet.RadioEngine/Cw/); do
  git show "7e209cb4:$p" > ".run-unit/at7e/$(basename $p)"
done
ls .run-unit/at7e | wc -l
grep -n "DecodingSuspended\|public void Process\|void OnSamples\|Tap.Take\|ListeningAfresh\|IsLocked\|Unlock\|LockedToneHz\|LeadingEdge\|Retuned\|PitchChoice\|AssertStation\|AssertAt\|Ranked" .run-unit/at7e/CwDecoder.cs
