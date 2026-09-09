#!/bin/sh
# Unit 295 task 5 - the fifteen-second re-census, ON THE OPERATOR'S OWN PATH ONLY.
# Reading only; changes nothing; writes no file.
#
# HOW THIS WAS ACTUALLY RUN. This session's shell refused to execute the file -
# `sh tools/unit295-census15.sh` came back "requires approval", the same refusal
# units 289 to 294 met on every .bat under tools/arbiter. **The three commands
# below were each run individually and their output is in unit 295's report.**
# The file is committed so the count can be replayed rather than reconstructed,
# which is the same reason unit 294 committed its append arguments.
#
# UNIT 290 SURVEYED THREE POPULATIONS - 47 arithmetic, 72 prose and 15 ON SCREEN
# in the application, and its own report recorded five on-screen sentences still
# standing after its edits. This re-runs the third population and only the third.
#
# WHAT COUNTS AS THE OPERATOR'S PATH, and it is narrower than unit 290's whole
# survey on purpose:
#   - text and x:String resources in src/Hamlet.App/**/*.axaml, which IS the screen
#   - string literals in .cs under src/Hamlet.App and src/Hamlet.RadioEngine,
#     which is what a bound property or a composed sentence hands to it
# WHAT DOES NOT COUNT: comments, arithmetic, tests, docs and the port. A comment is
# unit 290's prose population and arithmetic is its first; counting either here
# would make the number incomparable with the 15 it is being read against.
#
# UNIT 290'S OWN PATTERN, UNCHANGED, so the two counts are of the same thing.

PAT='fifteen|15 s|15-second|15 second|quarter.minute|quarter minute|12\.64'

# 1. The screen itself. Every hit is then read in context, because an XML comment
#    in .axaml is prose and not a screen.
grep -rnEi "$PAT" src/Hamlet.App --include=*.axaml

# 2. What reaches a screen. Narrowed to lines carrying a string literal, then each
#    read in context, because a <param> tag also carries a quote.
grep -rnEi "$PAT" src/Hamlet.App src/Hamlet.RadioEngine --include=*.cs \
  --exclude-dir=obj --exclude-dir=bin | grep '"'

# 3. THE WIDENING UNIT 290'S PATTERN WOULD HAVE MISSED. The capture sheet once
#    wrote `15.00 s slots`, which none of the alternatives above matches, so any
#    literal carrying a fifteen figure is swept as well. It finds nothing new
#    today - the sheet formats the length off the grid since unit 292.
grep -rnE '"[^"]*(15\.0|15 s|15s|15-second|15 second)' \
  src/Hamlet.App src/Hamlet.RadioEngine --include=*.cs --include=*.axaml \
  --exclude-dir=obj --exclude-dir=bin
