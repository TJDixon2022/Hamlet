#!/bin/sh
# Unit 391: write docs/phase-cw/unit391-seams.md from the grep outputs.
cd /c/Source/HamLet || exit 1
doc=docs/phase-cw/unit391-seams.md
{
echo "# Unit 391 - the seams into src/Hamlet.RadioEngine/Cw (step 0, criterion 0.4)"
echo
echo "Written by work instruction 391 tasks 2 and 3, 2026-09-22, from the tree at 3d6a2c12, by grep."
echo "Scripts: .run-unit/unit391-seams.sh, unit391-members.sh, unit391-seams-at.sh (not committed)."
echo
echo "**How the rows were found.** Every .cs and .xaml file under src/Hamlet.App and"
echo "tests/Hamlet.App.Tests from git ls-files, kept when it has a using of Hamlet.RadioEngine.Cw."
echo "The type column is every one of the 102 type names declared under src/Hamlet.RadioEngine/Cw"
echo "that the file names as a whole word. 30 files are kept, 9 under src/Hamlet.App and 21 under tests/Hamlet.App.Tests, 145 file-and-type rows. Dropped as name-only matches: 27 files that name only"
echo "Outcome (a nested enum in CwAccuracy.cs, a common word) with no Cw using, and"
echo "AchievementsViewModel.cs, which names CwTransmitter and KeyerCwSender in a comment only."
echo
echo "**How the members column was found, and what it is not.** A member is listed when the file"
echo "has .Member for a Member declared public in the file that declares the type. Types declared"
echo "in the same file (CwCharacter, CwConfidence and CwReadingStage in CwCharacter.cs, for"
echo "instance) therefore show the same members. It is a grep, not a compiler: it can over-list"
echo "a member that another type in the file shares, and cannot see a member reached through var"
echo "or a lambda parameter if the member name is not written with a dot. Step 1's build is the"
echo "real check."
echo
echo "**At 07f0397a** (the commit named for 0.2): the type is looked for by declaration under"
echo "src/Hamlet.RadioEngine/Cw at that commit, and each member by whole word in the file that"
echo "declares the type there. Outcome rows in files that also have a Cw using are kept, but the"
echo "Outcome those files mean may be another type of that name."
echo
echo "| File | Type | Members touched (grep) | At 07f0397a |"
echo "|---|---|---|---|"
paste -d'#' .run-unit/unit391-members.txt .run-unit/unit391-seams-at-07f0397a.txt | while IFS='#' read -r a b; do
  f=$(echo "$a" | cut -d'|' -f1 | sed 's/ *$//')
  t=$(echo "$a" | cut -d'|' -f2 | sed 's/^ *//; s/ *$//')
  m=$(echo "$a" | cut -d'|' -f4 | sed 's/^ *//; s/ *$//')
  s=$(echo "$b" | cut -d'|' -f3 | sed 's/^ *//; s/ *$//')
  x=$(echo "$b" | cut -d'|' -f4 | sed 's/^ *//; s/ *$//; s/^missing: none$//')
  [ -z "$m" ] && m="-"
  if [ "$s" = "TYPE ABSENT" ]; then at="**type absent**"; elif [ -n "$x" ]; then at="type there; **$x**"; else at="all there"; fi
  echo "| \`$f\` | $t | $m | $at |"
done
} > $doc
wc -l $doc
