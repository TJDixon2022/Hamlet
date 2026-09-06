import json
d = json.load(open('data/bands/us-neighborhoods.json'))
print(list(d.keys()))
for b in d['bands']:
    print('BAND', b.get('band'), list(b.keys()))
    for h in b['neighborhoods']:
        print('   ', h.get('family'), '|', h.get('name'), '|', h.get('shortName'), '|',
              h.get('lowHz'), h.get('highHz'), h.get('jumpHz'), '|', h.get('source'))
