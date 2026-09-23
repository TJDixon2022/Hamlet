#!/bin/sh
# Unit 391 section 5 checks against the tree.
cd /c/Source/HamLet || exit 1
f=tests/Hamlet.RadioEngine.Tests/Cw/TheCapturesThatDecodeKeepDecodingTests.cs
echo "== floor table rows"
grep -c '^        { "' $f
echo "== captures on disk"
for n in $(grep -o '^        { "[^"]*"' $f | sed 's/.*"\(.*\)"/\1/'); do
  if [ -f "tests/fixtures/cw/captured/$n.wav" ]; then echo "present $n"; else echo "MISSING $n"; fi
done
echo "== adjudicated readings"
grep -c 'new Reading(' tests/Hamlet.RadioEngine.Tests/Cw/TheAdjudicatedReadingsKeepReadingTests.cs
echo "== version and status"
grep -o '<Version>[^<]*' Directory.Build.props
grep '^WORK_INSTRUCTION' PROJECT_STATUS.md
echo "== git log over Cw since 2026-08-24"
git log --format="%h %ad %s" --date=short --since=2026-08-24 -- src/Hamlet.RadioEngine/Cw
echo "== with tests and fixtures"
git log --format="%h %ad %s" --date=short --since=2026-08-24 -- src/Hamlet.RadioEngine/Cw tests/Hamlet.RadioEngine.Tests/Cw tests/fixtures/cw | wc -l
