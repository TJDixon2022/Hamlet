cd /c/Source/HamLet
F=src/Hamlet.RadioEngine/Cw/CwDecoder.cs
echo "lines $(wc -l < $F), with CR $(grep -c "$(printf '\r')" $F)"
echo "HEAD lines with CR $(git show HEAD:$F | grep -c "$(printf '\r')")"
