cd /c/Source/HamLet
sh .run-unit/unit397-clean.sh
echo "names at HEAD in CwDecoder:"
grep -c "Tap.Window\|ReadHeldAudioAgain\|MaybeSwing\|MaybePeak\|MaybeRank\|_reReadAt" src/Hamlet.RadioEngine/Cw/CwDecoder.cs
grep -n "Tail(" src/Hamlet.RadioEngine/Cw/CwKeyingMeter.cs
ls src/Hamlet.RadioEngine/Audio/ReusableWindow.cs
ls .run-unit/ | grep "unit395-piece-1-"
grep -l "ReadHeldAudioAgain" .run-unit/unit395-piece-1-*.patch
