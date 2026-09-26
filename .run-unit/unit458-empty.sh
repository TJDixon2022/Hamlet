#!/bin/sh
# unit 458 task 3 - which recordings our decoder reads as nothing, and whether 135641 is in the tree.
cd /c/Source/HamLet || exit 1
echo "== text lines in the before save:"
grep -a -c "^ *text | " .run-unit/unit458-text-before.sorted.txt
echo "== text lines whose text is empty or spaces only:"
grep -a -E "^ *text \| [^|]+ \| *$" .run-unit/unit458-text-before.sorted.txt
echo "== 135641 anywhere in the save:"
grep -a "135641" .run-unit/unit458-text-before.sorted.txt
echo "== 135641 as a file:"
git ls-files | grep "135641"
find tests -name "*135641*"
