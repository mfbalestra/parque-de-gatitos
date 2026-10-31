const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm'),crypto=require('node:crypto');
const {chromium}=require(path.join(process.argv[2],'playwright'));
const ids=['naranja','gris','calico','negro','blanco','tuxedo','siames','marron'];
(async()=>{
 const browser=await chromium.launch({headless:true});
 try{
  const page=await browser.newPage(),coats=[],checks=[];
  const context={window:{}};vm.runInNewContext(fs.readFileSync(path.join(__dirname,'orange-art-data.js'),'utf8'),context);
  for(const id of ids){
   const url='assets/acciones-'+id+'.png',bytes=fs.readFileSync(path.join(__dirname,url));
   const data=await page.evaluate(async source=>{
    const image=new Image();image.src=source;await image.decode();
    const canvas=document.createElement('canvas');canvas.width=image.width;canvas.height=image.height;
    const g=canvas.getContext('2d');g.drawImage(image,0,0);const rgba=g.getImageData(0,0,image.width,image.height).data;
    const alpha=(x,y)=>rgba[(y*image.width+x)*4+3];
    let transparent=0;for(let i=3;i<rgba.length;i+=4)if(rgba[i]===0)transparent++;
    if(alpha(0,0)!==0||transparent/(image.width*image.height)<.3)throw Error('Opaque background');
    const rows=[0],cw=image.width/4,ch=image.height/4;
    function gutter(center,range,score){
     const candidates=[];for(let p=Math.round(center-range);p<=Math.round(center+range);p++)candidates.push({p,n:score(p)});
     candidates.sort((a,b)=>a.n-b.n||Math.abs(a.p-center)-Math.abs(b.p-center));
     if(candidates[0].n>2)throw Error('No clear gutter at '+center+': '+candidates[0].n);
     return candidates[0].p;
    }
    for(let i=1;i<4;i++)rows.push(gutter(ch*i,12,y=>{let n=0;for(let x=0;x<image.width;x++)if(alpha(x,y)>=24)n++;return n;}));
    rows.push(image.height);
    const frames={};let largest=0;
    for(let row=0;row<4;row++){
     const y0=rows[row],y1=rows[row+1],columns=[0];
     for(let i=1;i<4;i++)columns.push(gutter(cw*i,40,x=>{let n=0;for(let y=y0;y<y1;y++)if(alpha(x,y)>=24)n++;return n;}));
     columns.push(image.width);
     for(let col=0;col<4;col++){
      const x0=columns[col],x1=columns[col+1];let l=x1,t=y1,r=x0,b=y0;
      for(let y=y0;y<y1;y++)for(let x=x0;x<x1;x++)if(alpha(x,y)>=24){l=Math.min(l,x);r=Math.max(r,x);t=Math.min(t,y);b=Math.max(b,y);}
      if(l>=r||t>=b)throw Error('Empty cell');
      l=Math.max(x0,l-2);t=Math.max(y0,t-2);r=Math.min(x1-1,r+2);b=Math.min(y1-1,b+2);
      const w=r-l+1,h=b-t+1;largest=Math.max(largest,w);
      frames[['knead','settle','rub','spar'][row]+col]={x:l,y:t,w,h,anchorX:(cw*(col+.54)-l)/w,anchorY:1};
     }
    }
    return {width:image.width,height:image.height,scale:23/largest,frames,transparent:transparent/(image.width*image.height)};
   },'data:image/png;base64,'+bytes.toString('base64'));
   const {transparent,...sprite}=data;
   coats.push(id==='naranja'?{id,...context.window.PARK_ORANGE}:{id,url,...sprite});
   checks.push({id,sha256:crypto.createHash('sha256').update(bytes).digest('hex'),transparent,frames:Object.keys(data.frames).length});
  }
  fs.writeFileSync(path.join(__dirname,'interaction-art-data.js'),'window.PARK_INTERACTIONS = '+JSON.stringify({coats},null,2)+';\n');
  fs.writeFileSync(path.join(__dirname,'assets/interaction-art-checks.json'),JSON.stringify(checks,null,2)+'\n');
  console.log(checks);
 }finally{await browser.close();}
})().catch(error=>{console.error(error);process.exitCode=1});
