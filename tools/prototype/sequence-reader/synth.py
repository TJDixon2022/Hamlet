import numpy as np, wave, sys
from scipy.signal import butter, lfilter
MORSE_T={'A':'.-','B':'-...','C':'-.-.','D':'-..','E':'.','F':'..-.','G':'--.','H':'....','I':'..','J':'.---','K':'-.-','L':'.-..','M':'--','N':'-.','O':'---','P':'.--.','Q':'--.-','R':'.-.','S':'...','T':'-','U':'..-','V':'...-','W':'.--','X':'-..-','Y':'-.--','Z':'--..','0':'-----','1':'.----','2':'..---','3':'...--','4':'....-','5':'.....','6':'-....','7':'--...','8':'---..','9':'----.'}
def key(text,wpm,sr,edge_ms=5,ratio=3.0,jitter=0.0,seed=1):
    rs=np.random.RandomState(seed)
    dit=1.2/wpm; out=[]
    def j(x): return x*(1+jitter*rs.uniform(-1,1))
    for w in text.split(' '):
        for li,l in enumerate(w):
            for ei,e in enumerate(MORSE_T[l]):
                out.append((1, j(dit if e=='.' else ratio*dit)))
                out.append((0, j(dit)))
            out[-1]=(0,j(3*dit))
        out[-1]=(0,j(7*dit))
    n=int(sum(d for o,d in out)*sr)+sr; g=np.zeros(n); t=0
    for o,d in out:
        a=int(t*sr); b=int((t+d)*sr); 
        if o: g[a:b]=1
        t+=d
    # raised cosine edges
    e=int(edge_ms/1000*sr); k=np.hanning(2*e+1); k/=k.sum()
    return np.convolve(g,k,'same')
def make(text,wpm,snr_db,sr=12000,pitch=615,fade_db=0,fade_hz=0.7,seed=1,ratio=3.0,jitter=0.0):
    g=key(text,wpm,sr,ratio=ratio,jitter=jitter,seed=seed)
    n=len(g); t=np.arange(n)/sr
    tone=np.sin(2*np.pi*pitch*t)*g
    if fade_db: tone*=10**((fade_db/2)*np.sin(2*np.pi*fade_hz*t)/20)
    rs=np.random.RandomState(seed+100)
    noise=rs.randn(n); b,a=butter(4,[350/(sr/2),870/(sr/2)],'band'); noise=lfilter(b,a,noise)
    noise/=np.sqrt(np.mean(noise**2)); amp=np.sqrt(2)*10**(snr_db/20)
    x=amp*tone+noise; x/=np.max(np.abs(x))*1.1
    return x,sr
if __name__=='__main__':
    text=sys.argv[1]; wpm=float(sys.argv[2]); snr=float(sys.argv[3]); out=sys.argv[4]; fade=float(sys.argv[5]) if len(sys.argv)>5 else 0
    ratio=float(sys.argv[6]) if len(sys.argv)>6 else 3.0; jit=float(sys.argv[7]) if len(sys.argv)>7 else 0.0
    x,sr=make(text,wpm,snr,fade_db=fade,ratio=ratio,jitter=jit)
    w=wave.open(out,'w'); w.setnchannels(1); w.setsampwidth(2); w.setframerate(sr); w.writeframes((x*32767).astype(np.int16).tobytes()); w.close()
