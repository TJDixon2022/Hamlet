cd /c/Source/HamLet
# Usage: sh .run-unit/unit392-errsum.sh <n> - one line per file and distinct missing name.
f=.run-unit/unit392-errors-$1.txt
sed -E "s/^([^(]*)\([0-9]+,[0-9]+\): error ([A-Z0-9]+): (.*)$/\1 | \2 | \3/" $f \
  | sed -E "s/ and no accessible extension method.*$//; s/ \(are you missing.*$//" \
  | sed -E "s#^tests.Hamlet.RadioEngine.Tests.##; s#^tests.Hamlet.App.Tests.#APP: #" \
  | sort -u
