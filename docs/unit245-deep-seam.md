# The seam between Ft8Sharp and Ft8Sharp.Deep

**Written by unit 245, which created `Ft8Sharp.Deep`.** This is task 1.2's
reachability census, written up so the unit that is authored against step 2 does
not have to re-measure it.

**Everything below is a measurement of the tree as it stood on 2026-09-04 at
`Ft8Sharp` 0.10.7.** Nothing here is a plan, a recommendation about API design,
or a licence for anything. Where it names a cost, the cost is named and not
weighed — that is step 2's unit's call and its arbiter's.

---

## 0. The one-line answer

**Yes.** The loop inside `Ft8SlotDecoder.Decode(Ft8Waterfall)` can be reproduced
from outside the assembly using only public members, without `InternalsVisibleTo`
and without copying a line of it.

**Except for one thing, and it is exactly the thing an OSD stage needs.** Nothing
outside `Ft8Sharp` can construct an `Ft8CodewordResult`. It has a private
constructor and its three factories — `FromMessage`, `Unreadable`, `Refused` — are
`internal`. The only public route to one is to call
`Ft8CodewordDecoder.Decode(ratios, cache, maxIterations)` and be handed what that
returns.

That matters because `Ft8SlotMessage` is
`readonly record struct Ft8SlotMessage(Ft8Candidate Candidate, Ft8CodewordResult Result)`
and `Ft8SlotMessage.Text` reads `Result.Message.Text`. **So a codeword that
ordered statistics decoding recovers, which the belief propagation refused, cannot
be turned into an `Ft8SlotMessage` and therefore cannot be put into an
`Ft8SlotResult`** — which is what the scoreboard reads.

§4 names the three routes past that. **Route A was run, not reasoned about, and
it works with public members only while leaving both of the port's gates in
place** — so nothing step 2 needs is out of reach without changing `Ft8Sharp`.

---

## 1. The port's decode surface

`src/Ft8Sharp/Dsp/Ft8SlotDecoder.cs`. All `public`.

| Member | Line | Signature |
|---|---|---|
| `Ft8SlotDecoder` (type) | `:51` | `public sealed class` |
| `DefaultMessageLimit` | `:63` | `public const int` = 50 |
| constructor | `:81` | `Ft8SlotDecoder(Ft8WaterfallGeometry? geometry = null, Ft8SyncSearch? search = null, int messageLimit = DefaultMessageLimit, int maxIterations = LdpcDecoder.DefaultMaxIterations)` |
| `Geometry` | `:112` | `public Ft8WaterfallGeometry { get; }` |
| `MessageLimit` | `:115` | `public int { get; }` |
| `MaxIterations` | `:118` | `public int { get; }` |
| `CandidateLimit` | `:121` | `public int =>` the search's |
| `MinimumScore` | `:124` | `public int =>` the search's |
| `Decode` | `:133` | `public Ft8SlotResult Decode(ReadOnlySpan<float> samples)` |
| `Decode` | `:139` | `public Ft8SlotResult Decode(Ft8Waterfall waterfall)` |
| `AlreadySeen` | `:219` | `private static bool` |

`Ft8SlotMessage`, `:242`, `public readonly record struct`. Members
`Candidate`, `Result` (both `public`, positional), `Text` `:245`,
`FrequencyHz(Ft8WaterfallGeometry)` `:248`, `TimeSeconds(Ft8WaterfallGeometry)`
`:251`. **All public. The primary constructor is public** — but see §0 on what
can be put in the `Result` position.

`Ft8SlotResult`, `:270`, `public readonly record struct`. Five counts —
`CandidateCount`, `ParitySatisfiedCount`, `ChecksumPassedCount`,
`BecameTextCount`, `DuplicateCount` — plus `Messages` and the derived `Texts`
`:279`. **All public, and the primary constructor is public**, so a caller
outside the assembly can build a whole result.

Nothing in either type is `internal`.

---

## 2. The reachability census, stage by stage

The loop at `Ft8SlotDecoder.cs:162-213`, in order, with each stage's type and the
member the loop calls.

| Stage | Type | Public? | Member the loop uses | Public? |
|---|---|---|---|---|
| monitor | `Ft8Monitor` `Ft8Monitor.cs:37` | yes | ctor `:48`, `Analyse(ReadOnlySpan<float>)` `:211` | yes |
| waterfall | `Ft8Waterfall` `Ft8Waterfall.cs:28` | yes | held and passed; `DecibelsFor`, `StoredFor` static | yes |
| geometry | `Ft8WaterfallGeometry` | yes | ctor, `SampleRate`, `DefaultSampleRate` | yes |
| sync search | `Ft8SyncSearch` `Ft8SyncSearch.cs:50` | yes | ctor `:103`, `Find(Ft8Waterfall)` `:175`, `Find(ReadOnlySpan<float>, Ft8WaterfallGeometry?)` `:159`, `ScoreAt` `:265` | yes |
| candidates | `Ft8Candidate` `Ft8Candidate.cs:48` | yes | positional ctor, `CompareTo`, `FrequencyHz`, `TimeSeconds` | yes |
| soft symbols | `Ft8SoftSymbols` `Ft8SoftSymbols.cs:70` | yes | `RatioCount` `:73`, `Extract` `:117`, `ExtractSymbol` `:213`, `Normalise` `:287`, `Variance` `:323`, `HardDecision` `:351` | yes |
| callsign cache | `Ft8CallsignCache` `Ft8CallsignCache.cs:53` | yes | ctor | yes |
| LDPC | `LdpcDecoder` `LdpcDecoder.cs:81` | yes | `Decode` `:136`, `CodewordBits` `:100`, `DefaultMaxIterations` `:94` | yes |
| the gate | `Ft8CodewordDecoder` `Ft8CodewordDecoder.cs:46` | yes | `Decode` `:70` | yes |
| gate result | `Ft8CodewordResult` `Ft8CodewordDecoder.cs:174` | yes (readable) | **ctor `private`; `FromMessage`, `Unreadable`, `Refused` `internal`** | **NO** |
| gate status | `Ft8CodewordStatus` `Ft8CodewordDecoder.cs:142` | yes | enum | yes |
| correction detail | `LdpcDecodeResult` `LdpcDecodeResult.cs:30` | yes | read only | read only |
| message decode | `Ft8MessageDecoder` `Ft8MessageDecoder.cs:34` | yes | `Decode(ReadOnlySpan<byte>)` `:50`, `Decode(ReadOnlySpan<byte>, Ft8CallsignCache?)` `:76` | yes |
| message result | `Ft8DecodeResult` `Ft8MessageDecoder.cs:152` | yes (readable) | **ctor `private`; `Message`, `Refusal` `internal`** — but obtainable from `Ft8MessageDecoder.Decode` | obtainable |
| payload | `Ft8Payload` `Ft8Payload.cs:54` | yes | `MessageBits` `:57`, `MessageBytes` `:60`, `Create`, `TryRead`, `ExtractCrc` | yes |
| checksum | `Crc14` `Crc14.cs:56` | yes | `Compute` `:95` | yes |
| tables | `Ft8Tables` `Tables/Ft8Tables.g.cs:40` | yes | the parity and generator tables step 2 needs | yes |

**No `InternalsVisibleTo` is needed for any of it.** `Ft8Sharp.csproj` declares
none; the only `InternalsVisibleTo` in this tree is `Ft8Sharp.Tests` granting it
to `Ft8FixtureMaker`.

---

## 3. What is out of reach, stated exactly

**One thing: an `Ft8CodewordResult` carrying a decoded message, made from
anything other than a span of 174 ratios that `LdpcDecoder`'s belief propagation
itself satisfies.**

```csharp
public readonly struct Ft8CodewordResult
{
    private Ft8CodewordResult(...);                                 // Ft8CodewordDecoder.cs:176
    internal static Ft8CodewordResult FromMessage(...);             // :199
    internal static Ft8CodewordResult Unreadable(...);              // :202
    internal static Ft8CodewordResult Refused(...);                 // :205
}
```

`Ft8DecodeResult` is sealed against construction the same way, **but it is
obtainable**: `Ft8MessageDecoder.Decode(messageBits, cache)` is public and
returns a real one. `Ft8CodewordResult` has no such public producer other than
`Ft8CodewordDecoder.Decode`, which takes ratios and applies both gates.

Everything else the loop does — analyse, search, extract, normalise, correct,
check, unpack, de-duplicate on `codeword[..Ft8Payload.MessageBits]`, count, and
assemble an `Ft8SlotResult` — is reachable.

---

## 4. What step 2 would have to get past, and the three routes

**Nothing here recommends one. This is a price list.**

**Route A — round-trip the recovered codeword through the gate. MEASURED, not
proposed.** Once ordered statistics decoding has produced a candidate 174-bit
codeword, synthesise ratios of the correct sign from it, put them on upstream's
scale with `Ft8SoftSymbols.Normalise`, and hand those to
`Ft8CodewordDecoder.Decode`. Belief propagation converges on an already-valid
codeword, the CRC-14 gate is applied by the port exactly as it always is, and a
genuine `Ft8CodewordResult` comes back.

`tests/Ft8Sharp.Deep.Tests/Ft8DeepSeamProbeTests.cs` ran it, on a codeword the
test encoded itself standing in for whatever a future OSD stage would produce:

```
status  Decoded
text    "HAMLET 245"
iterations spent 1
```

and the same file flipped 40 bits of that codeword and watched the port refuse
it — `ParityNeverSatisfied`, empty text.

- Costs one extra belief propagation per OSD success only, and it converged in
  **one** iteration on a valid codeword.
- Uses public members only. Changes nothing in `Ft8Sharp`.
- **Keeps `CLAUDE.md` §0.0 intact**: nothing bypasses the checksum, because the
  port applies it. A codeword OSD gets wrong is refused by the port, in the
  port's own words, and that refusal has been watched.
- The result then goes into `new Ft8SlotMessage(candidate, result)` and
  `new Ft8SlotResult(...)`, whose primary constructors **are** public. That was
  run too.
- The thing to state plainly: this is a **re-derivation**, not a second CRC check
  on a different quantity, and there is still exactly one checksum check in the
  library.

**Because route A works, nothing step 2 needs from the port is unreachable
without changing the port.** Unit 245 therefore opened no `OPEN_ISSUES.md` entry:
an issue that asks for nothing is worse than none.

**Route B — the sibling carries its own result type.** `Ft8Sharp.Deep` defines
its own message and result records and converts at the edge. Nothing is out of
reach, because nothing of the port's is being constructed.

- Costs: `Ft8LadderHarness.Decoder` is `Func<float[], Ft8SlotResult>`
  (`Ft8LadderHarness.cs:73`), so either the seat's signature changes or the
  sibling converts back into an `Ft8SlotResult` — and converting back runs into
  §3 again for any message OSD alone recovered.
- The harness is test-project code and may be changed; the port may not.

**Route C — make the factories public in `Ft8Sharp`.** One line, and it is the
obvious answer, and **this phase forbids it**: `PHASE_PLAN.md` rules that nothing
in this phase changes a line of the port, because the port's whole value is that
it cannot drift. A later phase may take it; no unit of this one may.

---

## 5. Where the second column plugs in

`tests/Ft8Sharp.Tests/Dsp/Ft8LadderHarness.cs`.

- `internal sealed record Decoder(string Name, Func<float[], Ft8SlotResult> Decode)`
  at `:73`.
- `internal static IReadOnlyList<Decoder> Available()` at about `:190` after unit
  245's edit — **two entries now**, `Ft8Sharp` and `Ft8Sharp.Deep`.
- **One added entry costs one line plus a `ProjectReference`.** Nothing else in
  the harness and no call site knows how many decoders there are: `Run` iterates
  `decoders`, `Compare` iterates `decoders`, `Report` and `FixtureReport` iterate
  `results`. Unit 245 added the sibling and changed no caller.
- Two callers took a deliberate single seat afterwards —
  `Ft8LadderHarnessTests.TheSameRungWalkedTwiceGivesTheSameThreeCounts` and
  `ADifferentSeedIsADifferentDraw` — because they measure the harness's
  determinism and paying for a second identical column says the same thing twice.

**A third column costs the same one line.** If step 2's unit wants OSD-on and
OSD-off side by side against the port, that is three entries and no other change.

---

## 6. The boundary guard, and whether it covers the new direction

`tests/Ft8Sharp.Tests/Ft8SharpBoundaryTests.cs`, two halves:

- `DeclaresNoReferences` `:35` — loads `src/Ft8Sharp/Ft8Sharp.csproj` with
  `XDocument` and asserts **zero** `ProjectReference` and **zero**
  `PackageReference`, matching on local name so a namespaced project still counts.
- `NoHamletAssemblyArrives` `:59` — walks `Ft8SharpAssembly.Assembly`'s
  `GetReferencedAssemblies()` for anything starting `Hamlet`.

**Which catches `Ft8Sharp` referencing `Ft8Sharp.Deep`?** `DeclaresNoReferences`,
and it catches it immediately, because its bound is zero rather than a list of
forbidden names. `NoHamletAssemblyArrives` would **not** — it filters on the
prefix `Hamlet`, and the sibling is called `Ft8Sharp.Deep`. That is the shape of
that guard rather than a defect in it: its own remarks record that it is the
second net and cannot catch a declaration on its own.

**The guard as written already covers the new direction and needs nothing.** Unit
245 added a second net anyway, from the other side, in
`tests/Ft8Sharp.Deep.Tests/Ft8DeepBoundaryTests.cs`: the sibling's built assembly
**does** reference `Ft8Sharp`, the port's built assembly does **not** reference
`Ft8Sharp.Deep`, neither references any `Hamlet` assembly, and the sibling's own
csproj declares exactly one `ProjectReference` and no `PackageReference`.

---

## 7. The reference clone, as of this unit

- `C:\Source\ft8_lib` **is on this machine.** `ReferenceClone.Probe` returns
  `Reachable`; the whole `Ft8Sharp.Tests` suite ran with a single skip and that
  skip is `Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`, not a
  `[RequiresReferenceCloneFact]` skip.
- `ReferenceRecordings.All()` returns **69** recordings.
- `[RequiresReferenceCloneFactAttribute]`
  (`ReferenceCloneProbeTests.cs:304`) sets xunit's `Skip` in its constructor when
  the probe returns `Absent`, so on a fresh clone every test over the recordings
  skips with a message naming the path. The location is overridable with
  `FT8_LIB_PATH`, so the skip path can be watched on a machine that has the clone.
- Nothing is copied out of the clone. `ReferenceRecordings` locates and reads;
  its own summary states the ruling.

---

## 8. How a project joins `Hamlet.sln`

Three places, and `dotnet sln add` is **not** on this sandbox's allow-list — unit
245 tried it once and was refused, and edited the file.

1. A `Project(...) = ... EndProject` pair with the C# project type GUID
   `{9A19103F-16F7-4668-BE54-9A1E7A4F7556}`, the project name, the path with
   backslashes, and a fresh project GUID.
2. **Twelve** lines in `GlobalSection(ProjectConfigurationPlatforms)` — six
   configurations (`Debug`/`Release` × `Any CPU`/`x64`/`x86`), each with
   `.ActiveCfg` and `.Build.0`, and every one of them mapping to `Any CPU`.
3. One line in `GlobalSection(NestedProjects)` putting it under the `src` folder
   `{A5F1B2C3-0001-4000-9000-000000000001}` or the `tests` folder
   `{A5F1B2C3-0002-4000-9000-000000000002}`.

Unit 245 used `{B7E2C4D5-0006-4000-9000-000000000016}` for `Ft8Sharp.Deep` and
`{B7E2C4D5-0007-4000-9000-000000000017}` for `Ft8Sharp.Deep.Tests`, continuing
the existing sequence. `dotnet build Hamlet.sln` builds all ten projects.
