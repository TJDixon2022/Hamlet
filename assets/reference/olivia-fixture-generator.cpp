// Olivia fixture generator: drives Pawel Jalocha's reference MFSK transmitter
// (fldigi src/include/jalocha/pj_mfsk.h, GPL) exactly as fldigi's olivia.cxx does.
#include <cstdio>
#include <cstring>
#include <vector>
#include "jalocha/pj_mfsk.h"
int main(int argc,char**argv){
  if(argc<6){fprintf(stderr,"gen tones bandwidth centerHz outfile 'text'\n");return 2;}
  int tones=atoi(argv[1]); int bw=atoi(argv[2]); double center=atof(argv[3]); const char*out=argv[4]; const char*text=argv[5];
  MFSK_Transmitter<float> Tx;
  Tx.Tones=tones; Tx.Bandwidth=bw; Tx.SampleRate=8000; Tx.OutputSampleRate=8000;
  double fc_offset = Tx.Bandwidth*(1.0-0.5/Tx.Tones)/2.0;
  Tx.FirstCarrierMultiplier=(center-fc_offset)/500.0; Tx.Reverse=0;
  if(Tx.Preset()<0){fprintf(stderr,"preset failed\n");return 1;}
  std::vector<int16_t> pcm; std::vector<double> buf(Tx.MaxOutputLen);
  Tx.Start();
  size_t i=0; bool stopped=false;
  size_t guard=0;
  while(Tx.Running() && guard++<200000){
    if(!stopped){
      // fldigi feeds a character whenever fewer than one symbol's worth is queued
      while(Tx.GetReadReady()<Tx.BitsPerSymbol && i<strlen(text)){ if(Tx.PutChar((unsigned char)text[i])<=0) break; i++; }
      if(i>=strlen(text)){ Tx.Stop(); stopped=true; }
    }
    int len=Tx.Output(buf.data());
    for(int k=0;k<len;k++){ double v=buf[k]; if(v>1)v=1; if(v<-1)v=-1; pcm.push_back((int16_t)(v*0.5*32767)); }
  }
  // write wav 8000 Hz mono 16-bit
  FILE*f=fopen(out,"wb"); uint32_t dsz=pcm.size()*2; uint32_t rsz=36+dsz; uint16_t one=1,ch=1,bits=16,ba=2; uint32_t sr=8000,br=16000,fmtsz=16;
  fwrite("RIFF",1,4,f); fwrite(&rsz,4,1,f); fwrite("WAVEfmt ",1,8,f); fwrite(&fmtsz,4,1,f); fwrite(&one,2,1,f); fwrite(&ch,2,1,f); fwrite(&sr,4,1,f); fwrite(&br,4,1,f); fwrite(&ba,2,1,f); fwrite(&bits,2,1,f); fwrite("data",1,4,f); fwrite(&dsz,4,1,f); fwrite(pcm.data(),2,pcm.size(),f); fclose(f);
  printf("%s: %zu samples, %.1f s, %zu chars\n",out,pcm.size(),pcm.size()/8000.0,i);
  return 0;
}
