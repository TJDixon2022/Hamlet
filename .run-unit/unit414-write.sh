#!/bin/sh
# unit 414 - write the synthetic CQ set: WAV, sidecar and key file per case, then list sizes.
# Usage: sh .run-unit/unit414-write.sh "<TASK n of 5>" "<note>"
cd /c/Source/HamLet || exit 1
export HAMLET_WRITE_SYNTHETIC_CQ=1
sh .run-unit/unit414-run.sh write engine 300 "$1" "$2" "FullyQualifiedName~.TheSyntheticCqRebuildsTests.WriteTheSetWhenAsked" --no-build
grep -E "written \|" .run-unit/unit414-write.txt
ls -l tests/fixtures/cw/synthetic-cq
du -cb tests/fixtures/cw/synthetic-cq/*.wav | tail -1
