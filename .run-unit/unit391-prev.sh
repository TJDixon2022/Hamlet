#!/bin/sh
# Unit 391: print unit 390's committed output.md section 4.
cd /c/Source/HamLet || exit 1
git show HEAD:output.md | sed -n '/^## 4\./,$p'
