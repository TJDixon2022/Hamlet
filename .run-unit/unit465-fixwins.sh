#!/bin/sh
# unit 465 - in TheHigherCalibratedReadingWinsTests, read the record only after the character and class are asserted.
F=/c/Source/HamLet/tests/Hamlet.RadioEngine.Tests/Cw/TheHigherCalibratedReadingWinsTests.cs
sed -i 's/var (c, r) = One(/var (c, record) = One(/' "$F"
awk '
/var \(c, record\) = One\(/ { pending = 1 }
pending && /Assert\.(Equal|Same)\(.*[(, ]r\./ { match($0, /^ */); printf "%svar r = record();\n\n", substr($0, 1, RLENGTH); pending = 0 }
{ print }
' "$F" > "$F.tmp" && mv "$F.tmp" "$F"
grep -c "var r = record();" "$F"
