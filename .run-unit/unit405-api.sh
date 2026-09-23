cd /c/Source/HamLet
grep -n "DefaultSampleRate =\|DefaultToneHz =" src/Hamlet.RadioEngine/Training/CwSignal.cs
grep -rn "public static .* Read(" src/Hamlet.RadioEngine/Audio/WavAudio.cs
grep -n "public .*Folder" tests/Hamlet.RadioEngine.Tests/Cw/CapturedSignalTests.cs
grep -n "record CwProbabilisticCharacter\|record CwProbabilisticResult" -A8 src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
grep -n "public int HopSamples\|public bool HasMeasuredPitch\|public double ToneHz\|public int Follows" src/Hamlet.RadioEngine/Cw/CwToneTracker.cs
grep -rn "class BufferedAudioSource" src tests
grep -n "Expand" -A12 tests/Hamlet.RadioEngine.Tests/Cw/CwAlignment.cs
grep -n "ExchangeText =\|CoverageText =\|TightFistText =" -A2 tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/CwFixtureCatalogue.cs
