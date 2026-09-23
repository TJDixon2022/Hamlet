#!/bin/sh
# Unit 391: remove the task 3 worktree if present, then list worktrees.
cd /c/Source/HamLet || exit 1
if git worktree list | grep -q HamLet-wt391; then
  git worktree remove --force C:/Source/HamLet-wt391 && echo "removed HamLet-wt391"
fi
git worktree prune
git worktree list
