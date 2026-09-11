import numpy as np, csv, wave, struct
FS=8000; BAUD=31.25; SPB=int(FS/BAUD)  # 256 samples per bit
T=csv.DictReader(open('varicode.csv',encoding='utf-8',newline=''))
ENC={int(r['code']):r['bits'] for r in T}
DEC={v:k for k,v in ENC.items()}

def bits_for(text, idle_before=40, idle_after=40):
    b='0'*idle_before
    for ch in text.encode('latin-1'):
        b+=ENC[ch]+'00'
    b+='0'*idle_after
    return b

def modulate(bits, f0, drift_hz=0.0, amp=0.5, rng=None):
    n=len(bits); phase=1.0; sym=[]
    for bit in bits:
        if bit=='0': phase=-phase
        sym.append(phase)
    sym=np.array(sym)
    # raised-cosine (alpha=1) pulse shaping, pulse spans 2 bit periods
    t=np.arange(-SPB,SPB)/SPB
    h=0.5*(1+np.cos(np.pi*t))
    up=np.zeros(n*SPB); up[SPB//2::SPB]=sym   # symbol centres
    base=np.convolve(up,h,mode='same')
    tt=np.arange(len(base))/FS
    f=f0+drift_hz*tt/tt[-1]
    ph=2*np.pi*np.cumsum(f)/FS
    return amp*base*np.cos(ph)

def add_noise(sig, snr_db_2500, rng):
    # SNR referenced to a 2500 Hz bandwidth, like an FT8 report
    ps=np.mean(sig**2)
    noise_power=ps/(10**(snr_db_2500/10))          # in 2500 Hz
    sigma=np.sqrt(noise_power*(FS/2)/2500)           # scale to full Nyquist band
    return sig+rng.normal(0,sigma,len(sig))

def write_wav(path,x):
    x=np.clip(x,-1,1); pcm=(x*32767).astype('<i2')
    with wave.open(path,'wb') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(FS); w.writeframes(pcm.tobytes())

def demodulate(x, f0, afc=False):
    t=np.arange(len(x))/FS
    # AFC by squaring: BPSK squared has a line at 2*f0
    if afc:
        win=int(FS*2); est=[]
        for s in range(0,len(x)-win,win//2):
            seg=x[s:s+win]**2*np.hanning(win)
            spec=np.abs(np.fft.rfft(seg,n=1<<16)); fr=np.fft.rfftfreq(1<<16,1/FS)
            m=(fr>2*f0-120)&(fr<2*f0+120)
            est.append((s+win/2, fr[m][np.argmax(spec[m])]/2))
        est=np.array(est); ftrack=np.interp(np.arange(len(x)),est[:,0],est[:,1])
    else:
        ftrack=np.full(len(x),f0)
    lo=np.exp(-1j*2*np.pi*np.cumsum(ftrack)/FS)
    bb=x*lo
    # low-pass: moving average over one bit is a crude matched filter
    k=np.ones(SPB)/SPB
    bbf=np.convolve(bb,k,mode='same')
    # bit clock: choose the sample offset maximising |bb| energy at symbol centres
    best=max(range(0,SPB,4), key=lambda o: np.sum(np.abs(bbf[o::SPB])**2))
    s=bbf[best::SPB]
    # remove residual carrier phase per symbol via differential detection
    d=s[1:]*np.conj(s[:-1])
    bits=''.join('1' if v.real>0 else '0' for v in d)
    return bits

def decode_bits(bits):
    out=[]
    for code in bits.split('00'):
        code=code.strip('0')
        if not code: continue
        out.append(chr(DEC[code]) if code in DEC else '\ufffd')
    return ''.join(out)

def cer(ref, got):
    # character error rate via edit distance
    import numpy as np
    a,b=ref,got; d=np.zeros((len(a)+1,len(b)+1),int); d[:,0]=range(len(a)+1); d[0,:]=range(len(b)+1)
    for i in range(1,len(a)+1):
        for j in range(1,len(b)+1):
            d[i,j]=min(d[i-1,j]+1,d[i,j-1]+1,d[i-1,j-1]+(a[i-1]!=b[j-1]))
    return d[len(a),len(b)]/len(a)
