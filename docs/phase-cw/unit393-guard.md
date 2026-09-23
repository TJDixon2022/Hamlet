# Unit 393 - the CW read guard, watched red and then green (2.2)

Work instruction 393, task 3, 2026-09-22. Machine: the dev machine, no radio (FACT-004,
FACT-006). Every number here is an indication.

## The guard

The clause appended to line 9 of `docs/carry-forward-tests.txt` at task 2, run here alone as the
whole `--filter` against `tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj`,
`timeout 300`, one type per clause, detailed console logger:

```
FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests|(FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests&DisplayName~cw-2026-08-25)
```

26 cases: the adjudicated type's 12 readings and `TheShortfallIsPrintedRatherThanPapered`, and
the thirteen `cw-2026-08-25` captures.

## The break - one hunk, never committed

`src/Hamlet.RadioEngine/Cw/CwDecoder.cs`, inside `Process`, after the tap has taken the chunk and
before anything reaches a decoder. Written as a condition rather than a bare `return` so the
build, which treats warnings as errors, does not refuse the unreachable code after it. Not a
transmit file; `git diff --stat HEAD -- src` showed `CwDecoder.cs` alone, 1 insertion.

```diff
@@ -497,6 +497,7 @@ public sealed class CwDecoder
         // of that: a recording that quietly omitted his own sending would be
         // worth less, not more (§0.0.1). What it does not do is reach a decoder.
         Tap.Take(chunk.Samples, chunk.SampleRate);
+        if (chunk.Samples.Length >= 0) return; // UNIT 393 TASK 3 BREAK - NEVER COMMITTED
 
         if (DecodingSuspended || DigitalMode)
         {
```

Put back with `git checkout HEAD -- src/Hamlet.RadioEngine/Cw/CwDecoder.cs`; `git status --short
src` then printed nothing, and printed nothing again after the green run.

## The two runs

| Run | Cases | Red | Green | Wall time, script | Test time, runner |
|---|---|---|---|---|---|
| Against the broken decoder | 26 | 20 | 6 | 6 s | 2.6 s |
| After the file was put back | 26 | 0 | 26 | 32 s | 29.5 s |

Output kept, uncommitted: `.run-unit/unit393-guard-red.txt`, `.run-unit/unit393-guard-green.txt`.

### The thirteen 08-25 captures - `EachStillProducesWhatItDid`

Floor is characters and elements from the case's own data; the red column is the test's own
message.

| Capture | Floor characters | Floor elements | Broken decoder | Put back |
|---|---|---|---|---|
| cw-2026-08-25-013520 | 60 | 153 | red, fell from 60 characters to 0 | green |
| cw-2026-08-25-013637 | 63 | 164 | red, fell from 63 characters to 0 | green |
| cw-2026-08-25-012922 | 50 | 112 | red, fell from 50 characters to 0 | green |
| cw-2026-08-25-013402 | 61 | 161 | red, fell from 61 characters to 0 | green |
| cw-2026-08-25-013150 | 58 | 139 | red, fell from 58 characters to 0 | green |
| cw-2026-08-25-013010 | 54 | 131 | red, fell from 54 characters to 0 | green |
| cw-2026-08-25-021825 | 41 | 74 | red, fell from 41 characters to 0 | green |
| cw-2026-08-25-012748 | 2 | 4 | red, fell from 2 characters to 0 | green |
| cw-2026-08-25-012823 | 41 | 62 | red, fell from 41 characters to 0 | green |
| cw-2026-08-25-013303 | 54 | 146 | red, fell from 54 characters to 0 | green |
| cw-2026-08-25-011552 | 30 | 89 | red, fell from 30 characters to 0 | green |
| cw-2026-08-25-021410 | 47 | 99 | red, fell from 47 characters to 0 | green |
| cw-2026-08-25-021629 | 47 | 96 | red, fell from 47 characters to 0 | green |

Every 08-25 case has a floor above nought, so all thirteen went red. The instruction expected
`125941` at 0/0 to stay green among them; that capture is `cw-2026-08-26-125941`, an 08-26
recording, and the display-name match does not select it. It is not on the guard.

### The adjudicated type - `TheAdjudicatedReadingsKeepReadingTests`

| Reading | Anchor | Broken decoder | Put back |
|---|---|---|---|
| cw-2026-08-17-013347 | VA3VRR | red, anchor not found in "" | green |
| cw-2026-08-18-003758 | MP/4 QNIK | red, anchor not found in "" | green |
| cw-2026-08-24-012403 | DE KD0UN KD0UN K | red, anchor not found in "" | green |
| cw-2026-08-18-004507 | N HANDLING THIS MESSAG | red, anchor not found in "" | green |
| cw-2026-08-22-031838 | , AND | red, anchor not found in "" | green |
| cw-2026-08-22-031948 | 110, AND 110 W... | red, anchor not found in "" | green |
| cw-2026-08-22-032012 | R OTHER WEBSITES MENTI | red, anchor not found in "" | green |
| cw-2026-08-17-134712 | N4 | green - retired by Tim 2026-08-30, printed, not asserted | green |
| cw-2026-08-22-031905 | DICTED 10.7 | green - retired, squelch, Tim 2026-08-30 | green |
| cw-2026-08-22-032050 | ULLETIN CAN BE FO | green - retired, squelch, Tim 2026-08-30 | green |
| cw-2026-08-22-032113 | INT | green - retired, squelch, Tim 2026-08-30 | green |
| cw-2026-08-22-032129 | OPAGATION | green - retired, squelch, Tim 2026-08-30 | green |
| TheShortfallIsPrintedRatherThanPapered | - | green, asserts no decode | green |

The five readings that stayed green carry a `Retired` reason in the test's own table
(`TheAdjudicatedReadingsKeepReadingTests.cs` lines 124 to 177) and are printed as `RETIRED`
rather than asserted (line 235), so they cannot go red on any decoder. Every case in the guard
that asserts a decode - 20 of 26 - went red against a decoder that emits nothing, and all 26
were green after it was put back.

**What this proves and what it does not.** The guard refuses a decoder that produces nothing.
A subtler break is caught only where it drops a capture below its own floor or loses an
anchor, and a green guard is a count, not a claim that the
decoder reads (`CLAUDE.md` §0.0).
