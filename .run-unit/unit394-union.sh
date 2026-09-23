cd /c/Source/HamLet
grep -oE "ListeningAndFeedingReadTheSame\(name: \"[^\"]*\"" .run-unit/unit394-set-OneDecoderNotTwoTests.txt | grep -oE "\"[^\"]*\"" | sort -u > .run-unit/unit394-caps.txt
cat .run-unit/unit394-set-OneDecoderNotTwoTests.txt .run-unit/unit394-set-OneDecoderNotTwoTests-BufferSize.txt | grep -E "^\s+Passed Hamlet.*TheBufferSizeChangesNothing" | grep -oE "\"[^\"]*\"" | sort -u > .run-unit/unit394-buf.txt
echo "captures:"; wc -l < .run-unit/unit394-caps.txt
echo "buffer-size green in either run:"; wc -l < .run-unit/unit394-buf.txt
echo "not measured:"; comm -23 .run-unit/unit394-caps.txt .run-unit/unit394-buf.txt
