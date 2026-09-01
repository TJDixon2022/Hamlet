READ IN THIS ORDER — A, then B, then C.

A. PHASE — Hamlet hears FT8 off the radio and displays the decoded text on screen. Seven steps.
Step 1 is closed; steps 3, 4, 5, 6 and 7 remain not started and unreachable until step 2 closes,
because every step depends on the one before it by the plan's own named deviation. Step 2 is in
progress and this is its third unit. Task 1 measured that the message layer unit 207 left under
step 2 is still standing: `Ft8Sharp.Tests` at 108 total, 107 passed, 0 failed, 1 skipped in 3.4 s;
`src/Ft8Sharp` building at 0 warnings and 0 errors with no `PackageReference` and no
`ProjectReference`; attribution 58 paths from `2828ab6` with not one under any Hamlet project; and
all three channel classes green at 55 and 13.
B. STEP 2 — messages round-trip through 77 bits. SIX exit criteria as `PHASE_PLAN.md` numbers them.
(1) CRC matches known values, must-pass — closed by unit 206, untouched tonight. (2) standard,
free-text, telemetry and non-standard-callsign messages round-trip across a large generated corpus,
must-pass — THIS UNIT'S TARGET, and the last open must-pass criterion of the step. Measured at
seed 20871: standard 200 000, of which 198 280 round-tripped and 1720 came back as a different call
through upstream's two prefix work-arounds, which are counted apart and never as passes; free text
100 000, all of them; telemetry 100 000, all of them; non-standard callsign 100 000, with its three
legs reported SEPARATELY and never summed — 91 590 full calls round-tripped with no cache needed,
91 590 hashed and resolved through a warm cache, and 91 590 refused by a cold cache with no text
written on identical bits, with 8410 skipped as too long for the 58-bit field. Zero failures on any
category and on any leg. CRITERION 2 IS CLOSED. (3) any random 77-bit pattern either decodes or
fails cleanly and never throws, must-pass — RE-TAKEN tonight rather than inherited, 1 000 000
patterns at seed 20871, cold and warm. Cold: 0 exceptions, 0 decodes for a type not built, 0 decodes
carrying a call the cache never stored. Warm, against a cache filled to its 256-entry capacity and
therefore frozen for the run: the same three zeros, with 5471 calls correctly resolved from a hash
counted separately, and 298 003 decodes against the cold run's 292 532. The type cover is complete.
CRITERION 3 IS CLOSED AGAIN. (4) contest and DXpedition types round-trip, nice-to-pass, with "an
unsupported type must fail as unsupported and never as a wrong decode" must-pass — 15 combinations
enumerated, 5 built and 10 refused as unsupported, against unit 207's 4 and 11. Exactly one row
moved. The nice-to-pass half is still not built. (5) Ft8Sharp tests green, must-pass every unit —
148 total, 147 passed, 0 failed, 1 skipped in 4.0 s, the skip being the table write gate. (6)
attribution clean from `2828ab6` and the channel tests green, must-pass every unit — 72 paths, none
under `src/Hamlet.App/`, `src/Hamlet.RadioEngine/`, `tests/Hamlet.App.Tests/` or
`tests/Hamlet.RadioEngine.Tests/`; `AudioSeamTests` and `PrivilegeTests` green at 55;
`DecisionLogOrderTests`, `VersionTests`, `DecisionEmissionTests` and `VoiceTests` green at 13, all
re-run AFTER the version bump.
C. THIS REPORT — the HASH stands on two of its three legs: leg A machine-corroborated 10 of 10 of
its scalars against the pin at run time with none uncorroborated, though every one of the ten is a
literal inside a function body rather than a macro and is therefore the weaker expression-anchored
form; leg B, an independent implementation that does not call the library, agreed on all three
widths for all 100 000 generated callsigns; leg C does not exist — the pin states no hash value for
a named callsign anywhere, asked once across 95 sources. A round-trip against this library's own
cache is NOT evidence that the hash matches upstream, and step 3's bit-identical symbol comparison
must include a message carrying a hashed callsign or the hash goes unsettled into step 4. On a
collision the cache returns nothing rather than either call, which diverges from upstream, whose
lookup returns whichever its probe chain reaches first. Task 6, the collision census, was NOT
dropped. The `Ft8Sharp` project still returns in about four seconds and no corpus was cut for the
clock. Section 4 raises 3 items and none of them stands in the way of a criterion named in B.

```
UNIT:       208 — complete at task 8 of 8 — 2026-09-01 14:03
PHASE GOAL: Hamlet takes FT8 off the air and puts the decoded words on the screen.
UNIT GOAL:  Build the three callsign hashes and the rolling cache that resolves them, and the
            message that carries them, so a station heard once can be read back when a later
            message names it only by a hash — and one never heard is refused rather than guessed.
ADVANCED:   yes — criterion 2, the last open must-pass criterion of step 2, closed on all four
            categories with all three legs of the fourth measured separately and zero failures.
NUMBER:     step 2 must-pass criteria demonstrated: 5 -> 6 of 6
DRIFT:      0 consecutive units without advance  (was 0 — unit 207 advanced)
```

# 1. What Claude did

**Complete, at task 8 of 8. Nothing was dropped, including the named drop candidate.**

Windows 11, `C:\Source\HamLet`, project gate `PROJECT: Hamlet` verified against the tree —
`SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both present, neither
`CoreHMI.sln` nor `MURC.sln` present, `Hamlet.sln` the only solution. Branch `main`, eight commits,
every one pushed and every push succeeded.

**Task 1 — the trace.** `HEAD` at `666db6e` on `main` as stated. `git status --short` printed **34**
lines against the instruction's 33; unit 207 measured 34 too, and the extra line is the loop's own
`SESSION.lock`. Ft8Sharp 108 / 107 / 0 / 1 in 3.4 s, matching exactly. Library builds 0 warnings
0 errors, `net8.0`, nullable on, warnings as errors, no `PackageReference`, no `ProjectReference`.
Attribution 58 paths, 0 under any Hamlet project. All three channels green at 55 and 13. Known
items 5 and 9 confirmed and neither touched.

**Task 2 — the sanctioned read.** The clone is reachable from a checked-in test and the port had
its route. Three inventory tests added, mirroring unit 207's and reusing its reachability probe and
skip-when-absent attribute rather than writing a second one of either.

**Task 3 — the three hashes**, `src/Ft8Sharp/Message/Ft8CallsignHash.cs`, with both provenance legs
that exist.

**Task 4 — the rolling cache**, `src/Ft8Sharp/Message/Ft8CallsignCache.cs`, constructible per test
with no static instance anywhere.

**Task 5 — the non-standard-callsign message**, `src/Ft8Sharp/Message/Ft8NonstandardMessage.cs`, and
the one row of the type cover that moved.

**Task 6 — the collision census**, not dropped, and it took under a second.

**Task 7 — criterion 2's closing corpus and criterion 3 re-taken.**

**Task 8 — the record, both versions, the channels re-run after the bump, and this report.**

## The decisions this session made for itself, reproduced in full

All six are recorded in `src/Ft8Sharp/porting-notes.md` beside unit 207's seven, numbered on from
them, and all six are taken under `CLAUDE.md` §0.0 / HM-DEC-009 with unit 207's precedent.

**8. A cache miss refuses the whole message.** Upstream writes a literal `<...>` into the callsign
field and returns the message with it in. That is a decode with a station's name missing from it and
no way for the operator to know which station. A miss writes no text — at the cache, at the field, at
the message, and at the dispatcher.

**9. A cache collision refuses rather than answering. This is the divergence that mattered most.**
Two distinct callsigns can share a 22, 12 or 10-bit hash; a 12-bit hash has only 4096 values.
Upstream stores both and its lookup returns whichever its probe chain reaches first — a real,
plausible, entirely wrong callsign, presented with no mark of doubt on it, which is precisely the one
output HM-DEC-009 forbids. This lookup finds *every* stored call matching at the requested width and
returns nothing where there is more than one. Refusing costs a decode upstream would have shown;
answering costs the operator a logged contact with a station that was never on the air, and a wrong
callsign in a log is worse than a gap in one. Note the consequence: **a cache that knows more
sometimes produces less**, and that is the correct direction, because the extra knowledge is what
reveals the answer was never certain. What no cache can know is a station it has never heard whose
call collides with one it has, and nothing here pretends otherwise.

**10. The lookup examines every occupied slot rather than stopping at the first empty one.**
Upstream's early stop is correct only while nothing has ever been removed, and its own ageing pass
punches holes in the table. A hole can hide the second half of a colliding pair behind it, which
turns decision 9's refusal back into a confident wrong answer. Scanning the whole table costs a walk
of at most the capacity and makes the refusal depend on what the cache holds rather than on the
order it was filled in.

**11. The cache stores the callsign it was given rather than clipping it to eleven characters.**
Upstream copies eleven characters into a fixed buffer, so two calls agreeing that far collapse into
one entry spelled as neither of them, and a lookup returns that. Storing what was actually heard
turns the same case into an ambiguity, which refuses. The hash itself still reads only eleven
characters, because that part is on the air and may not be changed.

**12. A full cache answers rather than spinning.** Upstream's insert walks the table looking for an
empty slot with no bound at all and loops forever once every slot is taken.

**13. The hashed field is resolved before this message's own call is remembered, and upstream's
order is the other way round.** Upstream stores the callsign a message spells out in full and *then*
looks up its twelve-bit hash — so where the two calls in one message happen to share that hash, its
lookup finds the call the message is already carrying and reports the addressed station as the
transmitting one. A hashed field names a station the receiver is expected to have heard *already*,
so it is resolved here against what the receiver knew before this message arrived, and the call is
remembered afterwards — including when the message is then refused, because the call really was in
those bits and a receiver that threw it away would never warm up from the very messages it cannot
yet read. **Measured at 21 of 100 000 generated messages**, about the one in four thousand the width
predicts. This divergence *recovers* decodes rather than refusing more of them.

**14. A callsign too long for the 58-bit field is refused rather than truncated.** Upstream reads
eleven characters and stops without checking whether there were more, so a twelve-character call
goes on the air as its first eleven — a callsign nobody has, written as though it were certain.

## One thing to report against myself

**I committed the probe file of known item 9 by accident, and could not undo it.** Task 3's commit
used `git add -A` over the test project, which picked up
`tests/Ft8Sharp.Tests/TempEncoderProbe.cs` — the 673-byte file, emptied to a comment, that five
sessions' sandboxes have refused to delete and that this instruction says to leave alone. On
noticing, I tried `git rm --cached` to return it to untracked without touching the file on disk.
**The harness refused the command.** Reported as a refusal and not worked around; I switched to
explicit paths for every later commit. The file is unchanged on disk and no sixth deletion was
attempted, but it is now tracked, and the next unit's known item 9 should say so.

# 2. What the owner should expect

**Ft8Sharp is now 0.4.0 and the root is 1.12.15.** The library can read a message that names a
station by a hash of its call rather than by the call itself, which is the mechanism FT8 actually
uses once stations stop introducing themselves. Until tonight Hamlet would have gone deaf at exactly
that point.

**What will look wrong but is not:**

- **A resolved callsign comes back inside angle brackets — `<W9XYZ>`.** That is upstream's own
  convention and it is deliberately kept, because it marks a call that was *remembered* rather than
  read out of these bits. It is a claim of a different strength and the display should be able to
  tell the two apart.
- **1720 of 200 000 standard messages came back as a different callsign.** These are upstream's two
  prefix work-arounds — a call spelled the way the Swaziland or Guinea work-around spells its own
  compressed form packs to the same integer as the call the work-around is for. Upstream's wire
  format, not a defect here; unit 207 measured the same thing at 4971 of a million.
- **The four-category corpus reports 91 590 non-standard messages out of 100 000, with 8410
  skipped.** The skipped ones are longer than eleven characters, which is more than the 58-bit field
  can hold. That is the protocol's limit and the generator deliberately produces some.
- **A cache that has heard *more* stations sometimes decodes *fewer* messages.** Decision 9. It is
  the right direction and it is asserted directly.
- **`Ft8CallsignCache` is not thread-safe and has no static instance.** Both deliberate; a decoder
  owns its cache, and a shared one would make corpus results depend on test ordering.
- **`Ft8CallsignHash.TryCompute` answers for the empty string.** So does upstream's. Nothing in the
  library ever puts that hash in a cache — the cache refuses anything shorter than three characters.
- **`Ft8Sharp.Tests` grew from 108 to 148 tests and still finishes in about four seconds.** The
  fast inner loop is intact.
- **`tests/Ft8Sharp.Tests/TempEncoderProbe.cs` is tracked now.** See section 1; the file itself is
  untouched.

# 3. What you should see

## CRITERION 2 — the three legs of the non-standard-callsign round trip

**Corpus 100 000. Seed 20871.**

| leg | count | failures |
|---|---|---|
| full call round-tripped, no cache needed | **91 590** | **0** |
| hashed and resolved through a warm cache | **91 590** | **0** |
| refused by a cold cache with no text written, on identical bits | **91 590** | **0** |

8410 were skipped as longer than the 58-bit field holds. 21 of the 91 590 had two calls sharing a
12-bit hash and resolved correctly anyway because of decision 13. **The three legs are reported
separately and are never summed.**

### The four categories of criterion 2 together

| category | corpus | round-tripped | failures |
|---|---|---|---|
| standard | 200 000 | 198 280 | **0** |
| free text | 100 000 | 100 000 | **0** |
| telemetry | 100 000 | 100 000 | **0** |
| non-standard callsign | 100 000 | 91 590 | **0** |

The 1720 standard messages not counted as passes are upstream's prefix work-around collisions,
counted apart rather than as passes.

**Criterion 2 is closed. That is the last open must-pass criterion of step 2.** Whether the step is
done is not this unit's call.

## The hash's provenance

**Leg A — machine-read from the pin at run time, and every scalar matched.**

| scalars | corroborated by macro | corroborated by expression | uncorroborated |
|---|---|---|---|
| the hash itself | 0 | **10** | **0** |
| the rolling cache | **1** | **5** | **0** |
| the non-standard message's field widths | 0 | **5** | **0** |

The hash's ten, by role: the packing base; the callsign length read; the callsign length padded; the
multiplier; the product width; the hash width; the hash mask; the 12-bit truncation shift; the
10-bit truncation shift; and the packing alphabet, corroborated as an *identifier* by its position
in the pin's own declaration order and then tied to the base by its length. **Not one of the ten is
a macro** — every one is a literal inside a function body — so each is located by anchoring on the
expression that uses it inside the definition it belongs to and put through the same literal reader
the table converter uses. That is a mechanical read rather than a transcription, but it is **weaker
than a macro**, because a shape can be rewritten upstream in a way a name cannot, and every line of
the test says so. The cache's provenance is weaker again for a reason unrelated to the reading:
**the pin implements its cache in a demo application rather than in its library.**

**Leg B — the independent checker: 100 000 callsigns across 10 shapes, seed 20826, agreed on all
three widths, 0 disagreements, 0 refused.** Written from the pin in the test project, it does not
call the library and does not borrow a constant from it — its alphabet is spelled out separately, so
a wrong alphabet in the library could not make the two agree. It catches an ordinary porting slip.
**It does not catch a misreading made twice.**

**Leg C — a known value. There is none. The pin states no hash value for a named callsign
anywhere.** Asked once, mechanically, across **95** sources with `ft4_ft8_public/` excluded; five
mention hashing at all and not one pairs a hash mention with a numeric literal and a callsign-shaped
token.

**A round trip against this library's own cache is not evidence that the hash matches upstream.** It
is internal self-consistency and nothing more. **Step 3's bit-identical symbol comparison against
`ft8_lib` is where the hash gets settled, and that comparison must include a non-standard-callsign
message.** A comparison covering only standard messages with basecalls in them will pass whatever the
hash does, because no hash will have been on the wire. **If step 3 does not compare a message
carrying a hashed callsign, the hash goes unsettled into step 4**, where a wrong hash looks exactly
like a quiet band. This is a note for step 3's arbiter.

## CRITERION 3 — the fuzz re-taken, cold and warm

**1 000 000 random 77-bit patterns. Seed 20871.**

| count | cold | warm |
|---|---|---|
| exceptions | **0** | **0** |
| decodes returned for a type not built | **0** | **0** |
| decodes carrying a call the cache never stored | **0** | **0** |
| — | | |
| decoded in all | 292 532 | 298 003 |
| of those, carrying a call resolved from a hash | 0 | **5471** |
| refused | 707 468 | 701 997 |

**Cold** is no cache at all, which is the strictest reading of cold: it has heard nothing and cannot
warm up part-way through the run, so the third count stays unambiguous. **Warm** is a cache filled to
its 256-entry capacity *before* the run and therefore frozen for it, since a full cache stores
nothing further — which is what makes "a call the cache never stored" a checkable claim about a fixed
set rather than a moving target. The set was verified unchanged at the end of the run. The 5471
resolved calls are the correct outcome and are counted separately; they are also what shows the warm
run measured something the cold one did not. **The type cover is complete at 15 combinations.**

## The type cover — one row moved

| i3 | n3 | type | verdict |
|---|---|---|---|
| 0 | 0 | FreeText | built |
| 0 | 1 | DxPedition | refused as UnsupportedType |
| 0 | 2 | EuVhfContest | refused as UnsupportedType |
| 0 | 3 | ArrlFieldDay | refused as UnsupportedType |
| 0 | 4 | ArrlFieldDay | refused as UnsupportedType |
| 0 | 5 | Telemetry | built |
| 0 | 6 | Unknown | refused as UnsupportedType |
| 0 | 7 | Unknown | refused as UnsupportedType |
| 1 | — | Standard | built |
| 2 | — | Standard | built |
| 3 | — | ArrlRttyRoundup | refused as UnsupportedType |
| **4** | **—** | **NonstandardCallsign** | **built — THE ROW THAT MOVED** |
| 5 | — | WwrofContest | refused as UnsupportedType |
| 6 | — | Unknown | refused as UnsupportedType |
| 7 | — | Unknown | refused as UnsupportedType |

**15 combinations, 5 built, 10 refused as unsupported**, against unit 207's 4 and 11. None threw and
none returned a decode for a type not built. Measured with a fresh cache per combination and each
combination handed the best message it could carry. **This is a new test and it does not replace
unit 207's cover**, which still runs unchanged and still asks the harder question — what the
dispatcher does with bits declaring a type they are not.

## The cache's collision behaviour

**It returns nothing.** Decision 9 above, in full in section 1. **It diverges from upstream**, whose
lookup returns whichever call its probe chain reaches first. Colliding pairs were found at all three
widths by search over this project's own generated callsigns: **148 ms, after walking 4733 of
200 000 generated calls, seed 20841**. Each pair resolves correctly when the cache holds only one of
the two, and refuses with no text when it holds both — asserted at the cache, and again through the
whole stack on real bits at both the standard and the non-standard message.

## The miss refusing

Asserted at every level: at the cache for all three widths; at the field across a sweep of the
hashed sub-range, every value refusing as `UnresolvedCallsign` with no text and no field type; at
the message, which refuses whole rather than returning one field with a hole in it; and at
`Ft8MessageDecoder`, which returns an empty `Text` and a default `Fields`. **A null cache behaves
exactly as a cold one**, so every refusal unit 207 measured is still measured by the overloads
without a cache, unchanged.

## The collision census — task 6, NOT dropped

**1 000 000 distinct generated callsigns across 10 shapes, seed 20861, all of them hashable.
Generated in 519 ms, censused in 753 ms.**

| width | distinct calls | distinct hashes | colliding pairs | largest group | values the width holds |
|---|---|---|---|---|---|
| 22-bit | 1 000 000 | 890 166 | 118 559 | 5 | 4 194 304 |
| 12-bit | 1 000 000 | 4096 | 122 075 605 | 297 | 4096 |
| 10-bit | 1 000 000 | 1024 | 488 255 230 | 1099 | 1024 |

**What that means for an operator, measured on a real cache rather than modelled:** a cache of the
pin's own size, filled with 256 callsigns and then asked for each of their own 12-bit hashes,
resolved **242** and refused **14** because a second call it was also holding shared the hash. That
is **5.5 per cent** of the stations a full cache has genuinely heard which it will refuse to name
rather than pick between. What the display does about a refusal is Tim's under §12.1 and this stops
at the number.

## The end-to-end integration, with the new type included

**1905 messages carried whole through text → pack → `Ft8Payload.Create` → `LdpcEncoder` → all 83
parity checks → `Ft8Payload.TryRead` → unpack → the same text. 906 of them non-standard-callsign
messages through a warm cache.** Every one cleared all 83 checks and came back as the text that went
in. Separately, 19 033 packed non-standard messages were checked directly for a bit set past the
seventy-seventh: none had one, asserted on the bits and again through the container.

## The Ft8Sharp totals

| | before | after |
|---|---|---|
| total | 108 | **148** |
| passed | 107 | **147** |
| failed | 0 | **0** |
| skipped | 1 | **1** |
| wall clock | 3.4 s | **4.0 s** |

**40 tests added.** The one skip is `Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`, the
write gate, not a failure. All clone-gated tests passed rather than skipped, so the pin was reachable
throughout.

## Attribution and the three channels

`git diff --name-only 2828ab6..HEAD` lists **72 paths** (58 at the start of the unit), and **not one
is under `src/Hamlet.App/`, `src/Hamlet.RadioEngine/`, `tests/Hamlet.App.Tests/` or
`tests/Hamlet.RadioEngine.Tests/`** — `grep -c` over that list returns 0.

| channel class | verdict |
|---|---|
| `AudioSeamTests` + `PrivilegeTests` | **green, 55** |
| `DecisionLogOrderTests` | **green** |
| `VersionTests` | **green — re-run AFTER the version bump** |
| `DecisionEmissionTests` | **green** |
| `VoiceTests` | **green** |

13 in the App project, 68 in all. `git status --short` printed **34** lines at the start of the unit
and **36** at the end; the two extra are `Directory.Build.props` and `src/Ft8Sharp/Directory.Build.props`
awaiting task 8's commit. The loop's own files were counted and not committed.

## What task 2's inventory found in the clone — names and shapes only

`ft8/message.c` is 37 805 bytes and 1156 lines. `ft8/message.h` is 8497 bytes and 161 lines,
`ft8/text.c` 6421 and 304, `ft8/text.h` 3041 and 83, `ft8/constants.h` 3728 and 91. **`ft8/pack.c`
and `ft8/unpack.c` are both absent, confirmed on this session's own reading.**

**The hash does not live where its name would suggest.** `message.c` declares **no** function whose
name mentions hashing; all three widths are computed in one static function reached through a
two-entry function-pointer interface declared in `message.h`, whose enumeration names the 22, 12 and
10-bit widths as three members. **Two** functions in `message.c` mention non-standard by name, one to
encode and one to decode.

**The rolling cache is not in the pin's library at all.** `message.c` calls the interface and never
implements it; the only implementations are in the clone's demo decoder and its test harness, both
of which declare the table's capacity as a macro. The emitter was extended by one array to reach
them.

The gated emitter is off unless `FT8_HASH_SOURCE_DUMP=1` is set on the run, and it emits **named
definitions** rather than whole files — `message.c` is over a thousand lines and a reader who has to
page through all of them to reach the function being ported will skim, which is the failure mode a
faithful port of a hash cannot afford. **Not one line of upstream source is in this report, in any
commit message, or in any committed file.**

## The two version numbers as they now stand

| | was | is |
|---|---|---|
| `src/Ft8Sharp/Directory.Build.props` | 0.3.0 | **0.4.0** (HM-DEC-152) |
| root `Directory.Build.props` | 1.12.14 | **1.12.15** (HM-DEC-150) |

Both reasoned in the props file and in `porting-notes.md`. `Ft8Sharp.AssemblyInfo.cs` was read after
the build and carries 0.4.0 with no Hamlet commit in it.

# 4. What's blocking us

**Nothing is blocking. The pinned clone was reachable all night and no ruling is needed to
continue.** Three items, none of which stands in the way of any criterion named in B, and none of
which is a ruling request.

**1. Step 3's symbol comparison must include a message carrying a hashed callsign. A note for step
3's arbiter, not a request.** The hash is the one artifact in this port whose correctness cannot be
established by any test this library can run against itself, because it travels on the air and is
only useful if it agrees with what the transmitting station computed. Two of its three provenance
legs exist and both are recorded honestly; the third does not exist in the pin. **A comparison
covering only standard messages with basecalls in them will pass whatever the hash does**, because no
hash will have been on the wire. Already acted on to the extent this unit can: written into
`porting-notes.md` under its own heading so the next author reads it there as well as here.

**2. The probe file of known item 9 is tracked now, and the harness refused the command that would
have untracked it.** Task 3's commit used `git add -A` over the test project and picked up
`tests/Ft8Sharp.Tests/TempEncoderProbe.cs`. `git rm --cached` was refused by the sandbox; reported as
a refusal and not worked around. The file on disk is untouched and no sixth deletion was attempted.
**An observation for whoever writes known item 9 next**, not a ruling request — it changes nothing
about what the library does.

**3. Two mismatches between the instruction and the tree, reported and not repaired.** `git status
--short` printed **34** lines rather than the stated 33, which is the loop's own `SESSION.lock` and
which unit 207 also measured at 34. And the instruction describes upstream's cache as one where "the
later call overwrites the earlier one"; **measured, it does not overwrite** — it stores both in
adjacent slots by linear probing and its lookup returns whichever the probe chain reaches first,
which is the *earlier* one. The consequence is identical, a confident wrong callsign, and the
decision taken was the same either way; the mechanism differs and the next author should know it.
Known items 5 and 9 were confirmed and neither touched; nothing under `tools\` was edited; neither
`PHASE_STATUS.md` nor `PHASE_OUTCOME.md` nor `PHASE_PLAN.md` nor `PROJECT_CARD.md` was hand-edited.
