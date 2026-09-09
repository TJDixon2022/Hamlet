# -*- coding: utf-8 -*-
"""Transcribe the continent column of the ARRL DXCC List, current entities.

WHY THIS EXISTS RATHER THAN A HAND-WRITTEN MAP

    Work instruction 252 task 3 forbids typing a prefix table from memory and
    requires it to come from a published allocation, cited; `expand-prefixes.py`
    says so in its own header. **The same rule binds a continent.** A card
    reading `Europe - 4 of 65` is a claim about somebody's callsign, and nobody
    checks a continent, so a wrong one would live forever.

    Unit 298 found the DXCC table carried no continent and that the cited
    publication does: its header row is

        Prefix              Entity                         Continent ITU   CQ    Entity Code

    This reads that column out of the publication and writes the transcription
    `arrl-dxcc-continents.txt`, one `Entity = XX` line per current entity.

WHAT MAKES IT A TRANSCRIPTION RATHER THAN A DOWNLOAD

    **Every row is cross-checked against `arrl-dxcc-current.txt`**, which unit
    252 transcribed from the same publication and committed, and a row this file
    cannot join to one already there is REPORTED and DROPPED rather than taken on
    trust. So a continent is only accepted where two independent transcriptions
    of the same publication already agree about the row it sits on.

    THE JOIN IS TWO RULES, IN ORDER, AND EACH ROW SAYS WHICH ONE MATCHED IT.

      1. **Prefix and entity name both identical.** The strong case, and it is
         what resolves a prefix the publication lists more than once - `3D2` is
         Fiji, Conway Reef and Rotuma I. on three separate rows, and only the
         name tells them apart.

      2. **Prefix identical and unique in the publication.** Unit 252's
         transcriber shortened some of the publication's parentheticals, so
         `Singapore (Republic of)` is `Singapore` and `Germany (Federal Rep of)`
         is `Germany`. Requiring rule 1 alone dropped thirty entities including
         the operator's own country. **Every name difference is printed.**

    Anything neither rule joins is dropped. The counts this prints are the
    evidence, and the report quotes them.

WHAT IT REFUSES

    **The DELETED ENTITIES section**, which begins where the current one ends and
    holds entities nobody can work.

    **An entity the publication gives more than one continent**, written in the
    column as two codes. It is dropped with its reason rather than having one of
    the two chosen, which is the same rule the expander applies to a prefix
    claimed by two entities: say nothing where we do not know.

WHAT IT WRITES

    Two files, the same shape unit 252 used: a **transcription** under `tools/`
    that a person can read beside the publication, and a **generated** JSON under
    `data/` that the application embeds. The JSON carries its own source block and
    the counts from the cross-check, so the file says how it was made.

    usage: transcribe-continents.py <arrl-list.txt> <arrl-dxcc-current.txt>                                     <out.txt> <out.json>
"""
import json
import re
import sys
from collections import defaultdict

ARRL, TRANSCRIBED, OUT, OUT_JSON = sys.argv[1:5]

# --- the current-entities section only -------------------------------------
lines = open(ARRL, encoding="utf-8", errors="replace").read().splitlines()

start = next(i for i, l in enumerate(lines) if l.strip() == "CURRENT ENTITIES")
end = next(i for i, l in enumerate(lines) if l.strip() == "DELETED ENTITIES")

print("current-entities section: lines %d to %d" % (start + 1, end))

# --- the publication's own rows --------------------------------------------
# Prefix, Entity, Continent, ITU, CQ, Entity Code. The continent is two or three
# letters; ITU and CQ are numbers; the code is three digits. Anchoring on the
# trailing numeric columns is what makes the entity name's own spaces harmless.
# **THE ITU AND CQ COLUMNS ARE NOT PLAIN NUMBERS AND THE FIRST CUT OF THIS
# PATTERN LOST THIRTY ROWS TO IT**, the United States among them: the
# publication writes `(A)` where a zone is given by a footnote, and `06-08`
# and `23,24` where an entity spans several. They are matched loosely because
# nothing here reads them - the continent is the column being transcribed and
# the zones are only what anchors it.
ROW = re.compile(
    r"^\s{2,}(?P<prefix>\S+)\s{2,}(?P<entity>.+?)\s{2,}"
    r"(?P<continent>[A-Z]{2}(?:,[A-Z]{2})*)\s+\S+\s+\S+\s+\d+\s*$")

published = []
for line in lines[start:end]:
    m = ROW.match(line)
    if m:
        published.append(
            (m.group("prefix").strip(), m.group("entity").strip(),
             m.group("continent").strip()))

print("rows parsed from the publication: %d" % len(published))

# --- what unit 252 already committed ---------------------------------------
committed = []
for line in open(TRANSCRIBED, encoding="utf-8"):
    line = line.strip()
    if not line or "=" not in line:
        continue
    prefix, entity = line.split("=", 1)
    committed.append((prefix.strip(), entity.strip()))

print("rows in the committed transcription: %d" % len(committed))

# --- the cross-check --------------------------------------------------------
# **THE JOIN IS ON THE PREFIX STRING, AND THE ENTITY NAMES ARE COMPARED RATHER
# THAN REQUIRED.** The prefix column is what the continent sits beside in the
# publication and it is character-identical in both files - `K,W,N,AA-AK#`,
# `DA-DR(14)`. The entity NAME is not always: unit 252's transcriber shortened
# some of the publication's parentheticals, so `Singapore (Republic of)` is
# `Singapore` and `Germany (Federal Rep of)` is `Germany`. Requiring both to
# match dropped thirty entities including the operator's own country.
#
# **SO EVERY DIFFERENCE IS PRINTED**, and the report quotes the count. A silent
# join on a key that sometimes disagrees is exactly the seam this cross-check
# exists to expose.
by_pair = {(p, e): c for p, e, c in published}

by_prefix = {}
for p, e, c in published:
    by_prefix.setdefault(p, []).append((e, c))

agreed, missing, split, renamed = [], [], [], []
by_name, by_unique_prefix = 0, 0

for prefix, entity in committed:
    # RULE 1: prefix and name both identical.
    if (prefix, entity) in by_pair:
        continent = by_pair[(prefix, entity)]
        by_name += 1

    # RULE 2: prefix identical and unique in the publication.
    elif len(by_prefix.get(prefix, [])) == 1:
        published_entity, continent = by_prefix[prefix][0]
        renamed.append((prefix, entity, published_entity))
        by_unique_prefix += 1

    else:
        missing.append((prefix, entity))
        continue

    if "," in continent:
        split.append((entity, continent))
        continue

    agreed.append((entity, continent))

print("joined by prefix and name       : %d" % by_name)
print("joined by unique prefix alone   : %d" % by_unique_prefix)
print("joined, total                   : %d" % len(agreed))
print("not joined at all               : %d" % len(missing))
print("more than one continent         : %d" % len(split))
print("entity name differs             : %d" % len(renamed))

for entity, continent in split:
    print("  split: %s = %s" % (entity, continent))

for prefix, entity in missing:
    print("  no single published row: %s = %s" % (prefix, entity))

for prefix, ours, theirs in renamed:
    print("  name differs: %s -> ours %r, published %r" % (prefix, ours, theirs))

# --- one continent per entity, or nothing ----------------------------------
claims = defaultdict(set)
for entity, continent in agreed:
    claims[entity].add(continent)

kept = {e: sorted(c)[0] for e, c in claims.items() if len(c) == 1}
conflicted = {e: sorted(c) for e, c in claims.items() if len(c) > 1}

for entity, seen in conflicted.items():
    print("  conflicting rows for %s: %s" % (entity, seen))

print("entities with one agreed continent: %d" % len(kept))

with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    for entity in sorted(kept):
        f.write("%s = %s\n" % (entity, kept[entity]))

print("written: %s" % OUT)

# --- the generated file the application embeds -----------------------------
# **THE NAMES ARE `arrl-dxcc-current.txt`'S AND NOT THE PUBLICATION'S**, because
# `DxccPrefixes.EntityOf` returns those, and a continent keyed by a name nothing
# in the application ever produces would resolve for nobody.
document = {
    "source": {
        "name": "ARRL DXCC List, current entities",
        "publisher": "American Radio Relay League",
        "url": "https://www.arrl.org/files/file/DXCC/Current_Deleted.txt",
        "documentDate": "January 2026",
        "retrieved": "2026-09-09",
        "column": "The Continent column. The publication's header row reads: "
                  "Prefix              Entity                         "
                  "Continent ITU   CQ    Entity Code",
        "note": "Transcribed rows are in tools/dxcc/arrl-dxcc-continents.txt and "
                "this file is generated from them by "
                "tools/dxcc/transcribe-continents.py. Entity names are "
                "arrl-dxcc-current.txt's, which is what DxccPrefixes.EntityOf "
                "returns. Every row was cross-checked against that file, which "
                "unit 252 transcribed from the same publication: a row this "
                "script could not join to one already there is excluded rather "
                "than guessed at.",
        "declined": "An entity the publication gives two continents is excluded "
                    "rather than having one of the two chosen, which is the same "
                    "rule the prefix expander applies to a prefix two entities "
                    "claim.",
    },
    "crossCheck": {
        "rowsInThePublication": len(published),
        "rowsInTheCommittedTranscription": len(committed),
        "joinedByPrefixAndName": by_name,
        "joinedByUniquePrefixAlone": by_unique_prefix,
        "notJoined": len(missing),
        "declinedForTwoContinents": len(split),
        "entityNameDiffers": len(renamed),
    },
    "codes": {
        "AF": "Africa",
        "AN": "Antarctica",
        "AS": "Asia",
        "EU": "Europe",
        "NA": "North America",
        "OC": "Oceania",
        "SA": "South America",
    },
    "continents": {e: kept[e] for e in sorted(kept)},
}

with open(OUT_JSON, "w", encoding="utf-8", newline="\n") as f:
    json.dump(document, f, indent=2, ensure_ascii=False, sort_keys=False)
    f.write("\n")

print("written: %s" % OUT_JSON)
