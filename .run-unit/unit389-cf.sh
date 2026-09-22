#!/bin/sh
# Unit 389: run carry-forward line $1 of docs/carry-forward-tests.txt exactly as printed, one build.
cmd=$(sed -n "${1}p" docs/carry-forward-tests.txt)
eval "$cmd" 2>&1 | grep -E "^\s*Failed |Passed!|Failed!|error CS|dispatcher loop|Assert\.|at Hamlet|Test Run Aborted|crash" | head -80
