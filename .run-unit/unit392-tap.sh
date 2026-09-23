cd /c/Source/HamLet
echo "== Tap at HEAD in CwDecoder"
grep -n "Tap\b\|Tap?\|Tap(" src/Hamlet.RadioEngine/Cw/CwDecoder.cs | head -20
echo "== Tap at 7e209cb4 in CwDecoder"
git grep -n "Tap\b" 7e209cb4 -- src/Hamlet.RadioEngine/Cw/CwDecoder.cs | head -20
echo "== DigitalMode at HEAD"
grep -n "DigitalMode" src/Hamlet.RadioEngine/Cw/CwDecoder.cs | head
echo "== DigitalMode at 7e209cb4"
git grep -n "DigitalMode" 7e209cb4 -- src/Hamlet.RadioEngine/Cw/CwDecoder.cs | head
echo "== resample at HEAD / 7e209cb4"
grep -n "Resampl" src/Hamlet.RadioEngine/Cw/CwDecoder.cs | head
git grep -n "Resampl" 7e209cb4 -- src/Hamlet.RadioEngine/Cw/CwDecoder.cs | head
echo "== capture button test sample rate"
grep -rn "SampleRate\|48000\|8000\|12000" tests/Hamlet.App.Tests --include=TheCaptureButtonTests.cs | head
echo "== app Tap uses"
grep -n "\.Tap\b" src/Hamlet.App/ViewModels/MainWindowViewModel.cs | head -12
echo "== 865e66d8"
git show --stat --format="%h %ad %s" --date=short 865e66d8 | head -12
