# -*- coding: utf-8 -*-
"""Turn Hamlet's own sub-region groupings into the table the application reads.

WHAT IS EDITORIAL AND WHAT IS VERIFIED

    `hamlet-subregions.txt` says which regions exist and which entities are in
    each. That is Hamlet's own judgement, `ACHIEVEMENTS_PHILOSOPHY.md` §4 licenses
    it, and the screen says so on its face.

    **Everything else here is checked against cited data and dropped if it does
    not check out.**

      - **Every entity name** must appear in `arrl-dxcc-current.txt` exactly as
        spelled. A region cannot name a country that does not exist.
      - **Every prefix in a nudge** is read out of `dxcc-prefixes.json`, which is
        generated from the same publication. Nothing types a prefix.
      - **Every entity's continent** is read out of `dxcc-continents.json`, and a
        region whose members are on more than one continent is REPORTED, because
        the screen shows a region inside a continent and one that straddles two
        would be in the wrong place under one of them.

    usage: build-subregions.py <hamlet-subregions.txt> <arrl-dxcc-current.txt> \\
                               <dxcc-prefixes.json> <dxcc-continents.json> <out.json>
"""
import json
import sys
from collections import defaultdict

SRC, TRANSCRIBED, PREFIXES, CONTINENTS, OUT = sys.argv[1:6]

# --- the entity names the cited transcription holds -------------------------
known = set()
for line in open(TRANSCRIBED, encoding="utf-8"):
    line = line.strip()
    if line and "=" in line:
        known.add(line.split("=", 1)[1].strip())

print("entity names in the cited transcription: %d" % len(known))

prefixes = json.load(open(PREFIXES, encoding="utf-8-sig"))["prefixes"]
continents = json.load(open(CONTINENTS, encoding="utf-8-sig"))["continents"]

# **THE PREFIXES FOR AN ENTITY, READ OUT OF THE GENERATED TABLE.** The shortest
# form of each is what a nudge shows: `TI` rather than `TI,TE#`, because it is
# what a callsign actually starts with and it is what he will see in a list.
by_entity = defaultdict(set)
for prefix, entity in prefixes.items():
    by_entity[entity].add(prefix)

regions = []
refused = []

for line in open(SRC, encoding="utf-8"):
    line = line.strip()

    if not line or line.startswith("#") or "=" not in line:
        continue

    name, members = line.split("=", 1)
    name = name.strip()

    kept, seen_continents = [], set()

    for member in [m.strip() for m in members.split(",") if m.strip()]:
        if member not in known:
            refused.append((name, member, "not in the cited transcription"))
            continue

        code = continents.get(member)

        if code is None:
            refused.append((name, member, "the continent table declines it"))
            continue

        seen_continents.add(code)

        # The shortest prefix, which is what a callsign starts with.
        shortest = sorted(by_entity.get(member, set()), key=lambda p: (len(p), p))

        kept.append({
            "entity": member,
            "continent": code,
            "prefix": shortest[0] if shortest else None,
        })

    if not kept:
        print("  REGION DROPPED, no member verified: %s" % name)
        continue

    # **A REGION SITS INSIDE ONE CONTINENT OR IT IS NOT SHOWN.** The screen draws a
    # sub-region under the continent it belongs to, and one whose members are on two
    # would be in the wrong place under whichever it was filed. **Refused with its
    # reason rather than distorted**: the Mediterranean is the case, because Cyprus
    # is Asia in the ARRL's own column, and a later unit can split it or drop a
    # member on Tim's ruling rather than on a generator's.
    if len(seen_continents) > 1:
        refused.append((
            name, ", ".join(m["entity"] for m in kept),
            "the region spans " + " and ".join(sorted(seen_continents))
            + ", and a sub-region is drawn inside one continent"))

        print("  REGION REFUSED, spans %s: %s"
              % (sorted(seen_continents), name))
        continue

    regions.append({
        "name": name,
        "continent": sorted(seen_continents)[0],
        "members": kept,
    })

    print("  %-20s %d members, %s" % (name, len(kept), sorted(seen_continents)))

for region, member, why in refused:
    print("  REFUSED  %s / %s: %s" % (region, member, why))

document = {
    "source": {
        "name": "Hamlet's own groupings",
        "note": "THESE ARE NOT DXCC CATEGORIES AND THE SCREEN SAYS SO. DXCC "
                "defines continents; Central America, the Caribbean, Scandinavia "
                "and the Mediterranean are this application's own way of "
                "grouping entities inside one, licensed by "
                "ACHIEVEMENTS_PHILOSOPHY.md section 4 and required by it to be "
                "declared. Which regions exist and who is in each is editorial; "
                "every entity name was checked against "
                "tools/dxcc/arrl-dxcc-current.txt and every prefix was read out "
                "of data/callsigns/dxcc-prefixes.json, so nothing here names a "
                "country or a prefix that does not exist.",
        "editorial": "tools/dxcc/hamlet-subregions.txt",
        "generatedBy": "tools/dxcc/build-subregions.py",
    },
    "refused": [
        {"region": r, "entity": e, "why": w} for r, e, w in refused
    ],
    "regions": regions,
}

with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    json.dump(document, f, indent=2, ensure_ascii=False)
    f.write("\n")

print("regions: %d, refused members: %d" % (len(regions), len(refused)))
print("written: %s" % OUT)
