import io, sys

p = r'C:\Source\HamLet\PROJECT_STATUS.md'
state, task, note = sys.argv[1], sys.argv[2], sys.argv[3]
stamp = sys.argv[4]
rename = len(sys.argv) > 5 and sys.argv[5] == 'rename'

s = io.open(p, encoding='utf-8').read().split('\n')
for i, l in enumerate(s):
    if l.startswith('STATE:'):
        s[i] = 'STATE: ' + state
    elif l.startswith('TASK:'):
        s[i] = 'TASK: ' + task
    elif l.startswith('WORK_INSTRUCTION:'):
        s[i] = 'WORK_INSTRUCTION: 217'
    elif l.startswith('BALL:'):
        s[i] = 'BALL: claude'
    elif l.startswith('UPDATED:'):
        s[i] = 'UPDATED: ' + stamp
    elif l.startswith('NOTE:'):
        if rename:
            s[i] = 'NOTE: ' + note + '\nPRIOR_UNIT_216_NOTE:' + l[5:]
        else:
            s[i] = 'NOTE: ' + note
        break
io.open(p, 'w', encoding='utf-8', newline='\n').write('\n'.join(s))
print('status written ' + stamp)
