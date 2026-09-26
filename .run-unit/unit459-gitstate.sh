#!/bin/sh
# unit 458 - the HamLet repository's own state, read from its root.
cd /c/Source/HamLet || exit 1
git status -sb | head -1
git status --short
git log --oneline -3
