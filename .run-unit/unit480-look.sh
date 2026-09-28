#!/bin/sh
# unit 480 - read-only look at the entry state: launcher edits, version, the head of DECISIONS.md.
cd /c/Source/HamLet || exit 1
git rev-parse --short HEAD
git diff PHASE_STATUS.md PHASE_OUTCOME.md | cat
grep -n "<Version>" Directory.Build.props
head -12 DECISIONS.md
