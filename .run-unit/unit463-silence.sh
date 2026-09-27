#!/bin/sh
# unit 463 - section 4 check 4: the silence and empty-band types, and the gate room through the stream.
# Usage: sh .run-unit/unit463-silence.sh <suffix> "<n of 4>"
cd /c/Source/HamLet || exit 1
R=.run-unit/unit463-run.sh
sh $R "silence-$1" engine 300 "$2" "Silence: TheSilencePropertyIsLockedTests" "FullyQualifiedName~.TheSilencePropertyIsLockedTests." --no-build
sh $R "nokeying-$1" engine 300 "$2" "Silence: NothingIsReadFromAudioWithNoKeyingTests" "FullyQualifiedName~.NothingIsReadFromAudioWithNoKeyingTests." --no-build
sh $R "gate-$1" engine 300 "$2" "Gate room: the empty band through the stream" "FullyQualifiedName~.WhatFldigisFrontEndWouldLiftFact.TheEmptyBandThroughTheStream" --no-build
grep -a "gate-stream |" .run-unit/unit463-gate-$1.txt | cut -c1-260
