import json

d = json.load(open('data/callsigns/dxcc-prefixes.json'))
print("top keys:", list(d.keys())[:10])

for k, v in d.items():
    if isinstance(v, dict):
        print(k, "-> dict of", len(v))
    elif isinstance(v, list):
        print(k, "-> list of", len(v))
    else:
        print(k, "->", type(v))
