cd /c/Source/HamLet
echo "tests since unit 395 entry ee0ea0dc:"
git diff --stat ee0ea0dc HEAD -- tests
echo "end"
echo "floor test file since ee0ea0dc:"
git diff --stat ee0ea0dc HEAD -- tests/Hamlet.RadioEngine.Tests/Cw/TheCapturesThatDecodeKeepDecodingTests.cs tests/Hamlet.RadioEngine.Tests/Cw/TheAdjudicatedReadingsKeepReadingTests.cs
echo "end"
git log --oneline -1 ee0ea0dc
