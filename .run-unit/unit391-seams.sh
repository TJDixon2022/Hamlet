#!/bin/sh
# Unit 391 task 2: every app and app-test file that names a type from src/Hamlet.RadioEngine/Cw.
cd /c/Source/HamLet || exit 1
out=.run-unit/unit391-seams.txt
# Public and internal type names declared in the Cw folder.
git ls-files src/Hamlet.RadioEngine/Cw | grep '\.cs$' > .run-unit/unit391-cwfiles.txt
cat $(cat .run-unit/unit391-cwfiles.txt) | grep -oE '(public|internal)( static| sealed| abstract| readonly| partial| record)* (class|struct|record|enum|interface|delegate [A-Za-z<>]+) [A-Z][A-Za-z0-9_]*' | awk '{print $NF}' | sort -u > .run-unit/unit391-cwtypes.txt
echo "== Cw types declared: $(wc -l < .run-unit/unit391-cwtypes.txt)" > $out
pat=$(paste -sd'|' .run-unit/unit391-cwtypes.txt)
echo "== files in src/Hamlet.App and tests/Hamlet.App.Tests naming one" >> $out
for f in $(git ls-files src/Hamlet.App tests/Hamlet.App.Tests | grep -E '\.(cs|xaml)$'); do
  hits=$(grep -oEw "$pat" "$f" | sort -u | paste -sd' ' )
  using=$(grep -c 'Hamlet.RadioEngine.Cw' "$f")
  if [ -n "$hits" ] || [ "$using" != "0" ]; then
    echo "$f | using=$using | $hits" >> $out
  fi
done
