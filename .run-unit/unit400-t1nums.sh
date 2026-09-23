cd /c/Source/HamLet
grep -nE "NothingTheDecoderWasSureOfIsWrong|EveryRecordingGivesBack|TheCleanRecordingsDecodeExactly|TheProsignRecording|^\| *#?[0-9]+ *\|.*Sensitiv" docs/phase-cw/unit394-reds.md | cut -c1-260 | head -20
echo "== printer rows"
grep -E "^\s+(clean|NAME|WAY|way|TEXT|SETTLED|[A-Za-z-]+ +\|)" .run-unit/unit400-printer-after.txt | cut -c1-260 | head -40
grep -nE "Output:" .run-unit/unit400-printer-after.txt | head
echo "== sensitivity sweep entry vs after"
grep -E "^\[xUnit.net [0-9:.]+\] +[0-9.]+ dB" .run-unit/unit400-callers-entry-CwSensitivityTests.txt | sed -E "s/^\[xUnit.net [0-9:.]+\] +//" | sort > .run-unit/unit400-sens-a.txt
grep -E "^\[xUnit.net [0-9:.]+\] +[0-9.]+ dB" .run-unit/unit400-callers-after-CwSensitivityTests.txt | sed -E "s/^\[xUnit.net [0-9:.]+\] +//" | sort > .run-unit/unit400-sens-b.txt
wc -l .run-unit/unit400-sens-a.txt .run-unit/unit400-sens-b.txt
diff .run-unit/unit400-sens-a.txt .run-unit/unit400-sens-b.txt; echo "diff rc $?"
head -30 .run-unit/unit400-sens-b.txt
