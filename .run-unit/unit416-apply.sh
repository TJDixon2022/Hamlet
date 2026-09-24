#!/bin/sh
# unit 416 - re-apply b68be0dd from git diff 3ddca565 b68be0dd, unchanged, and prove it byte-identical.
cd /c/Source/HamLet || exit 1
git diff 3ddca565 b68be0dd --stat
git diff 3ddca565 b68be0dd > .run-unit/unit416-relabel.patch
git apply --check .run-unit/unit416-relabel.patch || { echo "APPLY CHECK FAILED"; exit 1; }
git apply .run-unit/unit416-relabel.patch || exit 1
echo "== src now against b68be0dd, must print nothing"
git diff --stat b68be0dd -- src
echo "== end"
git status --short src
