# -*- coding: utf-8 -*-
"""Expand the ARRL DXCC current-entity rows into a flat prefix table.

WHY THIS EXISTS RATHER THAN A HAND-WRITTEN JSON

    Work instruction 252 task 3 forbids typing a prefix table from memory and
    requires the table to come from a published allocation, cited. The rows in
    `arrl-dxcc-current.txt` are transcribed verbatim from that publication; this
    turns their compressed prefix column into the flat list the app matches on,
    so the derivation is reproducible and reviewable rather than asserted.

WHAT IT REFUSES TO EXPAND, AND THAT IS THE POINT

    A prefix claimed by more than one entity is dropped, not guessed at. So is
    any token whose form this expander does not confidently understand. Tim's
    ruling of 2026-09-05 is "say nothing if we don't know", and a token this
    script is unsure about is exactly a thing we do not know.

    Every drop is reported by reason, so the coverage figure in the report is a
    measurement rather than a claim.
"""
import json
import re
import sys
from collections import defaultdict

SRC = sys.argv[1]
OUT = sys.argv[2]

rows = []
for line in open(SRC, encoding="utf-8"):
    line = line.strip()
    if not line or "=" not in line:
        continue
    prefixes, entity = line.split("=", 1)
    rows.append((prefixes.strip(), entity.strip()))

claims = defaultdict(set)
skipped = []


def expand_token(token, stem):
    """One comma-separated token to a list of literal prefixes."""
    token = token.strip()
    if not token:
        return [], stem

    # A bare digit continues the previous token's stem: "KH6,7" is KH6 and KH7,
    # and "3B6,7" is 3B6 and 3B7.
    if token.isdigit() and stem:
        return [stem + token], stem

    # A range: "AP-AS", "9O-9T", "EA6-EH6". Both ends must be the same length
    # and differ in exactly one position, or this does not understand it.
    if "-" in token:
        lo, hi = token.split("-", 1)
        if len(lo) != len(hi):
            return None, stem
        diff = [i for i in range(len(lo)) if lo[i] != hi[i]]
        if len(diff) != 1:
            return None, stem
        at = diff[0]
        a, b = lo[at], hi[at]
        if not (a.isalnum() and b.isalnum() and ord(a) <= ord(b)):
            return None, stem
        out = []
        for c in range(ord(a), ord(b) + 1):
            out.append(lo[:at] + chr(c) + lo[at + 1:])
        return out, lo

    if re.fullmatch(r"[A-Z0-9]+", token):
        # **THE STEM DROPS THE TRAILING DIGITS.** "3B6,7" means 3B6 and 3B7, so
        # what a following bare digit attaches to is "3B" and not "3B6". Getting
        # this wrong produced "3B67", which is not a prefix anybody holds and
        # would have sat in the table looking like one.
        return [token], re.sub(r"[0-9]+$", "", token)

    return None, stem


for prefixes, entity in rows:
    # Footnote markers and the third-party-traffic hash are not prefixes.
    cleaned = re.sub(r"\(\d+\)", "", prefixes).replace("#", "").replace("^", "")
    stem = ""
    for token in cleaned.split(","):
        got, stem = expand_token(token, stem)
        if got is None:
            skipped.append((token.strip(), entity, "form not understood"))
            continue
        for p in got:
            claims[p].add(entity)

certain = {p: sorted(e)[0] for p, e in claims.items() if len(e) == 1}
shared = {p: sorted(e) for p, e in claims.items() if len(e) > 1}

doc = {
    "source": {
        "name": "ARRL DXCC List, current entities",
        "publisher": "American Radio Relay League",
        "url": "https://www.arrl.org/files/file/DXCC/Current_Deleted.txt",
        "documentDate": "January 2026",
        "retrieved": "2026-09-06",
        "entityCountStated": 340,
        "note": (
            "Transcribed rows are in tools/dxcc/arrl-dxcc-current.txt and this "
            "file is generated from them by tools/dxcc/expand-prefixes.py. The "
            "entity names are the ARRL's own. Rows whose prefix column this "
            "expander does not confidently understand, and prefixes claimed by "
            "more than one entity, are excluded rather than guessed at."
        ),
    },
    "prefixes": dict(sorted(certain.items())),
    "sharedAndThereforeSilent": dict(sorted(shared.items())),
}

with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    json.dump(doc, f, indent=2, ensure_ascii=False)
    f.write("\n")

print("rows read        :", len(rows))
print("prefixes certain :", len(certain))
print("prefixes shared  :", len(shared), sorted(shared)[:12])
print("tokens skipped   :", len(skipped))
for t in skipped[:15]:
    print("   skip:", t)
