cd /c/Source/HamLet
git checkout HEAD -- src/Hamlet.RadioEngine/Cw
F=src/Hamlet.RadioEngine/Cw/CwDecoder.cs
echo "names at HEAD:"
grep -c "HasMeasuredPitch" $F
grep -c "IsWordGap" src/Hamlet.RadioEngine/Cw/CwCharacter.cs
grep -rc "Unreadable" src/Hamlet.RadioEngine/Cw/MorseAlphabet.cs src/Hamlet.RadioEngine/Cw/CwConfidence.cs 2>&1
grep -n "c = " $F | head
for c in 2 1 0; do
  git apply --check -C$c .run-unit/unit396-piece-30-95a5e063.patch 2>&1 | head -2
  echo "check -C$c rc done"
done
grep -n "^@@" .run-unit/unit396-piece-30-95a5e063.patch
