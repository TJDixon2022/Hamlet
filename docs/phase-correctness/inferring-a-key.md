# Inferring a key from a CQ call on the air

Work instruction 414, task 3; PHASE_PLAN.md 1.3. The rule every inferred key in this phase is
written to, and the one a later unit keys a new capture by.

**Nobody in this project reads Morse** (R61). A key for off-air audio is therefore never a
transcript. It is inferred: reasoned from the decode and from the fixed form of what an
operator sends. **An inferred key is evidence, not proof.** Every number reported against one
says *against an inferred key* (CLAUDE.md 0.0, FACT-004), and a disagreement between an
inferred key and a decode does not by itself show the decoder is wrong. A key that is known
for certain comes only from the generator, which knows what it sent; those keys are *exact*
and live in `tests/fixtures/cw/synthetic-cq/` ([`synthetic-cq.md`](synthetic-cq.md)).

## What may be inferred

1. **The fixed form of a CQ call.** `CQ` repeated, `DE`, a callsign repeated, and `K`. Where
   the decode shows the form, the words of the form may be keyed even where a letter of them
   is misread, because the form is what every operator sends and not what this decoder
   printed. A form word is keyed only inside a stretch the decode shows was a CQ call.
2. **A callsign read the same way in two or more repetitions.** The same letters, in the
   same order, read twice or more, in one recording or across consecutive captures of one
   session. Spaces inside it do not count against the match: `DE KA2 G J V` and `KA2GJV` are
   one reading of one call with the boundaries wrong. Once a call is established this way it
   may be keyed where the form puts it, in the repetition the form requires.
3. **Text a later sidecar adds, found by differencing consecutive transcripts** - 1.6's
   method, cited here and not redone. The application's transcript is cumulative, so taking
   each capture's predecessor away from it leaves exactly what was decoded between them
   (work instruction 412, task 3; `baseline.md`, *The locked-on run of 2026-09-24*; each
   `cw-2026-09-24-*.key.md` says how it was built). Only the stretches of what was added that
   read with confidence are keyed; the rest of what was added is unscored.

## What may not be inferred

- **Anything nobody could read.** Single-element soup, placeholders, a run of `E` and `T`:
  no key is written for it and it is never scored (R61). *The unscored stretch of a
  recording stays unscored.*
- **A callsign seen once.** One reading of a call is the decoder's claim and nothing else.
  Keying it would score the decoder against itself.
- **A number, a signal report or a name that is not repeated.** `HR NR 2 0 R H X` is not a
  key for `NR 20`; a report read once is not a key for a report. Repetition is what lets a
  reading stand in for a transcript, and these are rarely repeated.
- **Anything filled in because it "must have been".** A letter missing from a call is not
  supplied because the call is familiar, a word is not completed from its first half, and a
  stretch that nearly reads is not tidied into what it would be if it did. If the key needs
  the word *must*, the stretch is not keyed.

## How the scored region is chosen

**17:37's rule:** from the first `C` of the first `CQ` to the last character emitted, with the
gaps at its two ends trimmed, scored whole against the key (`CwScorer.FromFirst`, then
`CwScorer.Whole`). Everything before the first `CQ` is unscored. Everything after the key's
last word that the decoder emitted is inside the region and is counted as its error, because
the region ends at the last character emitted and not at the key's last word.

A key that covers a fragment of a recording and names no region of its own - the adjudicated
texts, and the stretches keyed by differencing on the bench - is aligned to the stretch of
the decode that fits it best, and nothing on either side is scored (`CwScorer.Within`).
Scoring the whole decode against a fragment's key would count everything else in the
recording as error, and would be a key for audio nobody read.

**Score less rather than guess more.** When extending a region would need a single word the
rules above do not license, the region stops where the licence stops. A shorter region with
an honest key is a smaller number that means something; a longer one with a guessed key is a
larger number that does not. **A region is never widened to make a number tidier**, and
never narrowed to make it better: the region is chosen from the form and the repetitions
before anything is scored against it.

## The worked example - `cw-2026-09-23-173723`

From its own key file, `tests/fixtures/cw/captured/unadjudicated/cw-2026-09-23-173723.key.md`.

**Step 1 - what the decoder emitted**, from the sidecar the application wrote:

    (first third: single-element soup, no key given)
    ... ETWB6RED CQ CQ CQ DEW B 6 RE D W B 7

**Step 2 - find the form.** `CQ CQ CQ DE` stands in the middle of the decode, confident at
spans of 205 to 850 where the placeholders around it stand at 2 to 4. That is the fixed form
of a CQ call, and it licenses keying `CQ CQ CQ DE` and a callsign after it.

**Step 3 - find the call, twice.** `WB6RED` is read with its letters in order twice: whole,
run into `ET` just before the first `CQ` (`ETWB6RED`), and again after `DE` with the spaces
wrong (`W B 6 RE D`). Two readings of the same letters establish the call under rule 2. The
third reading, `W B 7`, stops short and reads `7` for `6`; it is not what establishes the
call and nothing is inferred from it.

**Step 4 - write the key the form puts there:**

    CQ CQ CQ DE WB6RED WB6RED

The call twice after `DE` is the form's repetition, keyed because the call was established in
step 3; the key's second `WB6RED` stands where the decoder read `W B 7`. **The key stops
there.** No `K` is keyed: the decode never shows one, and supplying it because a CQ call
"must" end in one is what the rules forbid.

**Step 5 - what was left unscored, and why.** Everything before the first `C` of the first
`CQ`, the first third of the recording included. It is single-element soup that nobody could
read, and that stretch is where the call's first reading, `ETWB6RED`, sits too: it
establishes the call but is not scored, because the `ET` run into it and whatever came
before it cannot be keyed. Nobody knows what was sent there, and R61 forbids a key for it.

**Step 6 - score it.** The region runs from that first `C` to the last character the
decoder emitted, trimmed at both ends, and is scored whole against the key: the sidecar's
reading scores with its spaces wrong, and the bench replay of the WAV, which every number in
this phase is measured on, reads `CQ CQ CQ DE W T E E T E  E ERE D E T T TB 7E E I` and scores
**29 edits over 25 characters against an inferred key**, 0 unsure per 28 named
(`baseline.md`). The trailing `7E E I` is inside the region, because the region ends at the
last character emitted; the alignment sets the key's second `WB6RED` against `E T T TB 7E E I`
and every character of it that does not match counts against the decoder. The region is not
cut back to spare them, and it is not widened back into the soup to make the example tidier.

**What the example is for.** It is the one keyed recording that is a CQ call from start to
finish, and the rule above is the one its key file was written by, set down afterwards and
checked against that file here step by step rather than invented around it.

## Where this rule is used

- Every inferred `.key.md` under `tests/fixtures/cw/captured/unadjudicated/`.
- `CwScorer.FromFirst`, `CwScorer.Whole` and `CwScorer.Within`
  (`tests/Hamlet.RadioEngine.Tests/Cw/CwScorer.cs`), and `baseline.md`, *How it is scored*.
- Any later unit that keys a new capture: it cites this file, says which of the three
  licences each keyed word rests on, and names what it left unscored.
