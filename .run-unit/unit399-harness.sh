cd /c/Source/HamLet
echo "harness history:"; git log --oneline -6 -- tests/Hamlet.RadioEngine.Tests/Cw/CwDecodeHarness.cs | cut -c1-120
echo "harness at 8e3ee277 parent, text source:"; git show 8e3ee277~1:tests/Hamlet.RadioEngine.Tests/Cw/CwDecodeHarness.cs | grep -nE "CharacterDecoded|CharacterSettled|LeadingEdge"
echo "harness at 8e3ee277:"; git show 8e3ee277:tests/Hamlet.RadioEngine.Tests/Cw/CwDecodeHarness.cs | grep -nE "CharacterDecoded|CharacterSettled|LeadingEdge"
echo "tests using CwDecodeHarness.Decode, compiled engine Cw:"; grep -rl "CwDecodeHarness.Decode" tests/Hamlet.RadioEngine.Tests | wc -l
