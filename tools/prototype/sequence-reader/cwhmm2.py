import sys, numpy as np
sys.path.insert(0,'hmm')
from cwhmm import load, envelope_db, to_text, MORSE

NEG=-1e18
def decode(db, rng, on, s_on, s_off, top, floor):
    n=len(db)
    e_on=-0.5*((db-top)/s_on)**2; e_off=-0.5*((db-floor)/s_off)**2
    c_on=np.concatenate([[0],np.cumsum(e_on)]); c_off=np.concatenate([[0],np.cumsum(e_off)])
    pred={0:[2,3,4,-1],1:[2,3,4,-1],2:[0,1],3:[0,1],4:[0,1,-1]}
    best=np.full((5,n+1),NEG); back=np.zeros((5,n+1,2),dtype=np.int64)
    for j in range(1,n+1):
        for s in range(5):
            lo,hi=rng[s]
            if j<lo: continue
            durs=np.arange(lo,min(hi,j)+1); i=j-durs
            seg=(c_on[j]-c_on[i]) if on[s] else (c_off[j]-c_off[i])
            prev=np.full(len(i),NEG); ps=np.full(len(i),-1)
            for p in pred[s]:
                cand=np.where(i==0,0.0,NEG) if p==-1 else best[p,i]
                b=cand>prev; prev[b]=cand[b]; ps[b]=p
            tot=prev+seg-np.log(hi-lo+1)
            k=int(np.argmax(tot))
            if tot[k]>NEG/2: best[s,j]=tot[k]; back[s,j]=(ps[k],i[k])
    s=int(np.argmax(best[:,n])); j=n; segs=[]
    while j>0:
        p,i=back[s,j]; segs.append((s,int(i),int(j))); s,j=int(p),int(i)
        if s<0: break
    segs.reverse(); return segs

def ranges_from_dit(d):
    r={0:(0.5*d,1.6*d),1:(2.0*d,4.5*d),2:(0.4*d,1.7*d),3:(1.8*d,5.0*d),4:(5.0*d,14*d)}
    return {k:(max(1,int(round(a))),max(2,int(round(b)))) for k,(a,b) in r.items()}

def learn(segs, db):
    L={s:[j-i for (ss,i,j) in segs if ss==s] for s in range(5)}
    med=lambda s,default: float(np.median(L[s])) if len(L[s])>=3 else default
    d=med(0,None)
    if d is None: return None, None
    dah=med(1,3*d); ig=med(2,d); lg=med(3,3*d); wg=med(4,7*d)
    # ranges: around each learned median, bounded so classes don't cross
    r={0:(0.55*d, (d+dah)/2), 1:((d+dah)/2, 1.8*dah), 2:(0.4*ig,(ig+lg)/2), 3:((ig+lg)/2,(lg+wg)/2), 4:((lg+wg)/2, 2.5*wg)}
    r={k:(max(1,int(round(a))),max(2,int(round(b)))) for k,(a,b) in r.items()}
    on_lv=np.concatenate([db[i:j] for (s,i,j) in segs if s<=1]); off_lv=np.concatenate([db[i:j] for (s,i,j) in segs if s>=2])
    stats=(np.median(on_lv), max(2.0,1.4826*np.median(np.abs(on_lv-np.median(on_lv)))), np.median(off_lv), max(2.0,1.4826*np.median(np.abs(off_lv-np.median(off_lv)))))
    return r, (d,dah,ig,lg,wg,stats)

def run(path,pitch,t0,t1,bw,dit_ms,iters=3,hop=5):
    x,sr=load(path); x=x[int(t0*sr):int(t1*sr)]
    db=envelope_db(x,sr,pitch,bw,hop)
    floor,top=np.percentile(db,[20,90]); s_on=s_off=4.0
    rng=ranges_from_dit(int(round(dit_ms/hop)))
    on={0:1,1:1,2:0,3:0,4:0}
    for it in range(iters):
        segs=decode(db,rng,on,s_on,s_off,top,floor)
        r,info=learn(segs,db)
        if r is None: break
        d,dah,ig,lg,wg,(top,s_on,floor,s_off)=info
        print(f"  pass {it+1}: dit {d*hop:.0f} dah {dah*hop:.0f} ig {ig*hop:.0f} lg {lg*hop:.0f} wg {wg*hop:.0f} ms  top {top:.1f}±{s_on:.1f} floor {floor:.1f}±{s_off:.1f}")
        rng=r
    return to_text(segs)

if __name__=='__main__':
    path,pitch,t0,t1=sys.argv[1],float(sys.argv[2]),float(sys.argv[3]),float(sys.argv[4])
    bw=float(sys.argv[5]); dit=float(sys.argv[6])
    print(run(path,pitch,t0,t1,bw,dit))
