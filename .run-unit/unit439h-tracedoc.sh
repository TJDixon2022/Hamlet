#!/bin/sh
# unit 439 (by hand) tasks 1 and 2 - writes docs/phase-requirements/traceability.md from the
# reviewed classification (.run-unit/unit439h-trace-raw.txt) and CW_REQUIREMENTS.md.
cd /c/Source/HamLet || exit 1
RAW=.run-unit/unit439h-trace-raw.txt
OUT=docs/phase-requirements/traceability.md
REQS=.run-unit/unit439h-reqmeta.txt
PLACE=.run-unit/unit439h-place.txt

# id|tier|section letter|statement
grep -E "^\| HM-REQ-" CW_REQUIREMENTS.md | awk -F'|' '
{ id=$2; gsub(/ /,"",id); t=$(NF-1); gsub(/^ +| +$/,"",t); s=$3; gsub(/^ +| +$/,"",s)
  n=substr(id,8)+0
  sec = n<10?"A":n<20?"B":n<30?"C":n<40?"D":n<50?"E":n<60?"F":n<70?"G":n<80?"H":n<90?"I":n<100?"J":n<110?"K":"L"
  print id"|"t"|"sec"|"s }' > $REQS

secname() {
  case $1 in
    A) echo "A. Honesty of the record" ;; B) echo "B. Confidence" ;; C) echo "C. Refusal and sensitivity" ;;
    D) echo "D. Speed" ;; E) echo "E. Channel and fading" ;; F) echo "F. Sender profiles" ;;
    G) echo "G. Interference" ;; H) echo "H. Character set" ;; I) echo "I. Word boundaries" ;;
    J) echo "J. Pitch" ;; K) echo "K. Acquisition time and latency" ;; L) echo "L. Signal measurements reported" ;;
  esac
}

TOTAL=$(wc -l < $RAW)
PROVING=$(awk -F'|' '$4 ~ /HM-REQ/' $RAW | wc -l)
NONE=$(awk -F'|' '$4=="none"' $RAW | wc -l)
UNCLEAR=$(awk -F'|' '$4=="unclear"' $RAW | wc -l)
RELATED=$(awk -F'|' '$5!="-"' $RAW | wc -l)
NOTCOMPILED=$(awk -F'|' '$6 ~ /^NOT COMPILED/' $RAW | wc -l)
FILES=$(cut -d'|' -f1 $RAW | sort -u | wc -l)

# requirement side
: > .run-unit/unit439h-reqrows.txt
while IFS='|' read id tier sec stmt; do
  p=$(awk -F'|' -v I="$id" '$4 ~ I {print "`"$2"."$3"`"}' $RAW | tr '\n' ' ' | sed 's/ $//; s/ /, /g')
  r=$(awk -F'|' -v I="$id" '$5 ~ I {print "`"$2"."$3"`"}' $RAW | sort -u | tr '\n' ' ' | sed 's/ $//; s/ /, /g')
  pc=$(awk -F'|' -v I="$id" '$4 ~ I' $RAW | wc -l)
  rc=$(awk -F'|' -v I="$id" '$5 ~ I' $RAW | wc -l)
  if [ "$pc" -gt 0 ]; then st="proved"; elif [ "$rc" -gt 0 ]; then st="none - only tests that measure something else"; else st="none - no test on its subject"; fi
  echo "$id|$tier|$sec|$pc|$rc|${p:-none}|${r:--}|$st" >> .run-unit/unit439h-reqrows.txt
done < $REQS
RWITH=$(awk -F'|' '$4>0' .run-unit/unit439h-reqrows.txt | wc -l)
RNONE=$(awk -F'|' '$4==0' .run-unit/unit439h-reqrows.txt | wc -l)
ROTHER=$(awk -F'|' '$5>0' .run-unit/unit439h-reqrows.txt | wc -l)
ROTHERONLY=$(awk -F'|' '$5>0 && $4==0' .run-unit/unit439h-reqrows.txt | wc -l)
RBARE=$(awk -F'|' '$5==0 && $4==0' .run-unit/unit439h-reqrows.txt | wc -l)
MUSTNONE=$(awk -F'|' '$4==0 && $2 ~ /^must/' .run-unit/unit439h-reqrows.txt | wc -l)

{
echo "# Traceability - every CW test and every CW requirement"
echo
echo "Work instruction 439, tasks 1 and 2 (PHASE_PLAN.md criteria 0.1, 0.2 and 0.3), run by hand"
echo "outside the loop on 2026-09-25. The specification is \`CW_REQUIREMENTS.md\` and \`CW_SPEC.md\` at"
echo "the repository root (HM-DEC-183, R77); nothing here changes either, and nothing under \`src\` or"
echo "any test file was changed to write it."
echo
echo "## How a test was judged"
echo
echo "A test **proves** a requirement when what it asserts is what the requirement states: the same"
echo "behaviour, under the requirement's condition or a narrower instance its verification row"
echo "accepts, against the requirement's threshold or a stricter one. A test that asserts a"
echo "mechanism the requirement does not name, or prints and asserts nothing, proves \`none\`. A test"
echo "on a requirement's subject that measures something else - another threshold, a character-count"
echo "floor, part of the required range, a ratchet where the row says pass or fail, another"
echo "condition - proves \`none\` and names that requirement under **measures something else**, with"
echo "the reason. Where the answer is genuinely unclear the column says \`unclear\` and why. Nothing"
echo "was assigned to fill a column (\`CLAUDE.md\` 0.0). The rubric is \`.run-unit/unit439h-rubric.md\`;"
echo "the classification was read from each test's source, and every proving row below was re-read"
echo "by the session before it was accepted."
echo
echo "Everything the specification puts outside the receive decoder (\`CW_SPEC.md\` section 2) - the"
echo "transmit path, the keying sweep, the capture and sidecar writer, the terminal, the scanner, the"
echo "contact-state model and callsign claiming - proves \`none\` here by construction, and its note"
echo "says so."
echo
echo "## Where the session looked"
echo
echo "- \`tests/Hamlet.RadioEngine.Tests/Cw/*.cs\` and \`tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/*.cs\`, every test method."
echo "- \`tests/Hamlet.App.Tests/Cw/*.cs\`, every test method."
echo "- Every other \`*.cs\` under \`tests/\`, searched for the receive decoder's types (\`CwDecoder\`,"
echo "  \`CwProbabilistic*\`, \`CwCharacter\`, \`CwConfidence\`, \`CwTranscript\`, \`CwDecodeReport\`, \`CwSignal\`,"
echo "  \`CwSignalRequest\`, \`CwKeyingThresholds\`, \`CwCase*\`, \`CwCountsCover\`, \`CwReadingStage\`,"
echo "  \`CwTerminalControl\`, \`CwToneTracker\`, \`CwToneSurvey\`); every test method in a file that names one is"
echo "  listed. Files that name only a CW transmit, band-plan or rig type (\`CwStop\`, \`CwBand\`, \`CwPitch\`,"
echo "  \`CwTransmitViewModel\`, \`CwMessage\`, \`CwReadiness\`, \`CwLowHz\` and the like) are CW-mode features outside"
echo "  the receive decoder's boundary and are not listed."
echo "- \`tests/Ft8Sharp.Tests\` and \`tests/Ft8Sharp.Deep.Tests\`: no CW test. \`tests/Shared\`: no test method."
echo "- A method is any member carrying an attribute ending in \`Fact\` or \`Theory\`, custom ones included;"
echo "  a \`[Theory]\` is one row however many cases it runs."
echo
echo "**Twelve files are excluded from compilation** by \`<Compile Remove>\` in the two test projects (work"
echo "instruction 392). Their methods never run; they are traced from source and each note starts"
echo "\`NOT COMPILED\`."
echo
echo "## The counts"
echo
echo "| | count |"
echo "|---|---|"
echo "| test methods traced | $TOTAL, in $FILES files |"
echo "| proving at least one requirement | $PROVING |"
echo "| \`none\` | $NONE |"
echo "| \`unclear\` | $UNCLEAR |"
echo "| \`none\` or \`unclear\` but on a requirement's subject (measures something else) | $RELATED |"
echo "| in files not compiled | $NOTCOMPILED |"
echo "| requirements | $(wc -l < $REQS) |"
echo "| requirements with at least one proving test | $RWITH |"
echo "| requirements with none | $RNONE, $MUSTNONE of them must-tier |"
echo "| requirements with a test that measures something other than what it states | $ROTHER, $ROTHERONLY of them with no proving test |"
echo "| requirements with no test on their subject at all | $RBARE |"
echo
echo "## 1. Every requirement and what proves it (section T extended, criterion 0.2)"
echo
echo "\`CW_REQUIREMENTS.md\` section T lists fifteen rows from test names in HM-OPEN-016 and HM-DEC-104."
echo "This extends it to every requirement id. **Proved by** names the tests that assert what the"
echo "requirement states; **measures something else** names the tests on its subject that do not, and"
echo "their reasons are in section 3's rows."
echo
echo "| requirement | tier | proved by | measures something else | status |"
echo "|---|---|---|---|---|"
awk -F'|' '{print "| "$1" | "$2" | "$6" | "$7" | "$8" |"}' .run-unit/unit439h-reqrows.txt
echo
echo "### The \`none\` rows by section - where the next steps are emptiest"
echo
echo "| section | requirements | with no proving test | of which no test on the subject at all | ids with no proving test |"
echo "|---|---|---|---|---|"
for s in A B C D E F G H I J K L; do
  all=$(awk -F'|' -v S=$s '$3==S' .run-unit/unit439h-reqrows.txt | wc -l)
  n=$(awk -F'|' -v S=$s '$3==S && $4==0' .run-unit/unit439h-reqrows.txt | wc -l)
  b=$(awk -F'|' -v S=$s '$3==S && $4==0 && $5==0' .run-unit/unit439h-reqrows.txt | wc -l)
  ids=$(awk -F'|' -v S=$s '$3==S && $4==0 {sub("HM-REQ-","",$1); print $1}' .run-unit/unit439h-reqrows.txt | tr '\n' ' ' | sed 's/ $//')
  echo "| $(secname $s) | $all | $n | $b | $ids |"
done
echo
echo "### Where this table and section T of \`CW_REQUIREMENTS.md\` differ"
echo
echo "Section T is the requirements document's own and is not edited here; where the two differ, the"
echo "difference is a finding for the owner rather than a correction."
echo
echo "- **064**: T says \`NothingIsInventedAtTheHandover\` exists. It does, and it allows up to half of what"
echo "  is emitted to be characters never sent, where 064 allows none: it measures something else."
echo "- **044, 090, 032, 083, 036, 020, 022, 013**: T records a test as existing for each. None of those"
echo "  tests asserts the requirement's own threshold or range, so each is \`measures something else\` here"
echo "  (044's threshold is unset, 090 stops at 400 to 875 Hz, 032's N is unruled, 020 and 022 are not in"
echo "  the 2500 Hz reference, 013's easy tier strips the spaces). T itself says most of this in its"
echo "  status column."
echo "- **011**: T says none. Two tests prove it on synthetic fixtures (\`CwFixtureTests.NothingTheDecoderWasSureOfIsWrong\`,"
echo "  \`TheProsignRecordingDecodesItsProsigns\`): no sure character wrong or invented against the exact"
echo "  text. On the keyed recordings it is measured, not proved, by task 3."
echo "- **094, 066, 005, 031**: T does not list them; a test proves each."
echo
echo "## 2. The red, known-red and retired names (criterion 0.3)"
echo
echo "Placed, not repaired, retired or restored (HM-DEC-103). A \`[Theory]\` case is placed by its method."
echo
cat $PLACE
echo
echo "## 3. Every CW test and what it proves (criterion 0.1)"
echo
echo "One row per test method. **Proves** is the requirement ids, \`none\` or \`unclear\`. **Measures"
echo "something else** names a requirement the test is about without asserting what it states."
echo
echo "| file | type | method | proves | measures something else | what it asserts |"
echo "|---|---|---|---|---|---|"
awk -F'|' '{f=$1; sub("tests/","",f); print "| "f" | "$2" | "$3" | "$4" | "$5" | "$6" |"}' $RAW
} > $OUT
wc -l $OUT
