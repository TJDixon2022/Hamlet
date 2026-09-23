#!/bin/sh
# Unit 391: assemble output.md.
cd /c/Source/HamLet || exit 1
clock=$(date "+%Y-%m-%d %H:%M")
{
sed "s/@@CLOCK@@/$clock/" .run-unit/unit391-out-a.md
sh .run-unit/unit391-table.sh
cat .run-unit/unit391-out-b.md
sed -n '/^| `/p' docs/phase-cw/unit391-seams.md
cat .run-unit/unit391-out-c.md
git show HEAD:output.md | sed -n '/^## 4\./,$p' | sed '1d' | sed '1{/^$/d}' | sed 's/^## /#### /; s/^### /#### /'
} > output.md
wc -l output.md
grep -n "^## " output.md
