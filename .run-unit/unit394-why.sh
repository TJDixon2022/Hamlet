cd /c/Source/HamLet
# Usage: sh .run-unit/unit394-why.sh <Type> - prints each failure name and its error message
grep -E -A4 "^\s+Failed Hamlet" .run-unit/unit394-set-$1.txt | grep -vE "Stack Trace|^\s+at |^--" | sed -E "s/Hamlet.RadioEngine.Tests.Cw.//" | cut -c1-400
