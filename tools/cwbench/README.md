# `cwbench.py` — a reference bench, not part of the application

This is a standalone Morse decoder in about a hundred and fifty lines of Python,
implementing the classification scheme from Guenther (1973). It reads a WAV and
prints text. **It is here so an idea can be tested before a unit is spent on it.**

**Nothing in Hamlet imports it, and nothing in Hamlet may.** It shares no code
with the application, it is not built, it is not on the test path, and it has a
dependency the application does not have. If it ever disappeared, the app would
not notice.

## Why a second decoder exists at all

Every unit from 044 onward was built on an argument about what the decoder was
doing wrong. An argument is cheap to make and expensive to be wrong about: a unit
gets spent building the thing the argument implies, and the measurement arrives at
the end. **A bench inverts that.** An idea from the literature can be put into a
hundred lines here, run over the operator's own captures, and either it reads
better or it does not — before anything touches the decoder that ships.

It has already earned that twice. A plain magnitude peak with parabolic
interpolation, twelve lines of it, found the pitch on two captures where the tone
tracker did not. And its first version had a run-merging bug that corrupted every
duration after a dropped blip, which is a specific fault worth checking Hamlet for
rather than a general worry.

## What it is not

**It is not better than Hamlet and it is not a candidate to replace it.** On the
ARRL bulletin captures it reads noticeably worse. More importantly it has **no
refusal**: on audio holding no station it emits text anyway, cheerfully, because
Guenther has no silence property and neither does any classical decoder in the
literature. Hamlet's refusal is its own, it is the whole of §0.0 in one behavior,
and nothing measured here is worth trading for it.

So the bench is a source of specific, testable ideas. It is never the answer.

## Running it

    python tools/cwbench/cwbench.py tests/fixtures/cw/captured/*.wav

It needs `numpy` and nothing else. It prints one tone estimate and one line of
text per file.
