import sys

p = 'src/Hamlet.App/Views/MainWindow.axaml'
lines = open(p, encoding='utf-8-sig', newline='').readlines()

assert 'THE TWO CONTROLS GO IN THE HEADER' in lines[4143], lines[4143]
assert 'CollapsiblePanel.HeaderAction>' in lines[4281], lines[4281]

note = [
    '                                    <!-- **THE CONTROLS LEFT THIS HEADER IN 331 TASK\n',
    '                                         1a** and are on the bar above both panels\n',
    '                                         now - `DigitalListControlsBar`, which says\n',
    '                                         why. 0.5 puts a panel own controls in its\n',
    '                                         own header and that held while the two\n',
    '                                         panels were the same width; this one is the\n',
    '                                         narrow half now and four controls do not fit\n',
    '                                         in its header without wrapping. Nothing about\n',
    '                                         WHEN they appear changed. -->\n',
]

new = lines[:4143] + note + lines[4282:]
open(p, 'w', encoding='utf-8', newline='').writelines(new)
print('ok', len(lines), '->', len(new))
