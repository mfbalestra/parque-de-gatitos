/* The secret owns temporary actors and its own clock/random stream. Normal actors
   are only read at entry and are never mutated, including during the return. */
(()=>{
'use strict';
const FLOWERS=Object.freeze([{x:28,y:108,c:'#ffffff'},{x:120,y:126,c:'#f2c230'},{x:237,y:136,c:'#e8577c'}].map(Object.freeze));
const FLOWER_SCALE=3;
const flowerTarget=f=>({x:f.x,y:f.y-.9*FLOWER_SCALE,radius:4.5});
const FRAME_NAMES=['crawl0','crawl1','crawl2','crawl3','retract0','retract1','retract2','hidden'];
class Secret {
  constructor({reducedMotion=false,onChange=()=>{},onExit=()=>{},seed=5701}={}) {
    this.phase='dormant';this.progress=0;this.selected=new Set();this.actors=[];
    this.ready=false;this.error='';this.time=0;this.feedback=[];this.margins=[];this.reducedMotion=reducedMotion;
    this.onChange=onChange;this.onExit=onExit;this.seed=seed;this.returnRequested=false;
  }
  get active(){return this.phase!=='dormant';}
  drawX(actor,width=256){const margin=this.margins[actor.coat]||0;return Math.max(margin,Math.min(width-margin,actor.x));}
  hitActor(x,y,radius){return this.actors.slice().sort((a,b)=>b.y-a.y).find(a=>Math.abs(this.drawX(a)-x)<=Math.max(radius,this.margins[a.coat]||12)&&Math.abs(a.y-7-y)<=Math.max(radius,12))||null;}
  random(){this.seed=this.seed*16807%2147483647;return (this.seed-1)/2147483646;}
  clear(){this.selected.clear();this.feedback=[];}
  hitFlower(x,y,radius){return FLOWERS.findIndex(f=>{const target=flowerTarget(f);return Math.hypot(target.x-x,target.y-y)<=Math.max(radius,target.radius);});}
  select(index,cats,allowed){
    if(!allowed||this.active||this.error||index<0||index>=FLOWERS.length)return false;
    this.selected.add(index);this.feedback.push({...FLOWERS[index],age:0});
    this.activate(cats,allowed);
    this.onChange();return true;
  }
  activate(cats,allowed){
    if(!allowed||this.active||!this.ready||this.error||this.selected.size!==3)return false;
    if(cats.some(c=>!Number.isInteger(c.artIndex)||!Number.isFinite(this.margins[c.artIndex]))){
      this.error='Falta una variedad de caracol. Recargá la página para volver a intentarlo.';this.onChange();return false;
    }
      this.actors=cats.map((c,i)=>({id:i,coat:c.artIndex,x:c.x,y:c.y-c.z,originX:c.x,originY:c.y-c.z,
        groundY:Math.max(112,c.y),dir:c.dir,distance:0,pause:0,moveFor:2+this.random()*4,speed:2+this.random()*1.8,retract:-1}));
      this.phase='flooding';this.progress=0;this.time=0;this.returnRequested=false;
    this.onChange();return true;
  }
  exit(returnToCompanion=false){
    if(!this.active)return false;
    this.returnRequested=this.returnRequested||returnToCompanion;
    if(this.phase!=='draining'){
      this.actors.forEach(a=>{a.drainX=a.x;a.drainY=a.y;});
      this.drainStart=Math.max(this.progress,.00001);this.phase='draining';this.onChange();
    }
    return true;
  }
  touch(x,y,radius,allowed){
    if(!this.active)return false;
    if(allowed&&this.phase==='underwater'){
      const actor=this.hitActor(x,y,radius);
      if(actor&&actor.retract<0)actor.retract=0;
    }
    return true;
  }
  frame(actor){
    if(actor.retract<0)return 'crawl'+Math.floor(actor.distance/1.8)%4;
    const t=actor.retract;
    if(t<.45)return 'retract'+Math.min(2,Math.floor(t/.15));
    if(t<1.65)return 'hidden';
    if(t<2.1)return 'retract'+Math.max(0,2-Math.floor((t-1.65)/.15));
    return 'crawl0';
  }
  advance(dt,allowed=true){
    if(!allowed||!Number.isFinite(dt)||dt<=0)return;
    this.feedback.forEach(f=>f.age+=dt);this.feedback=this.feedback.filter(f=>f.age<1.1);
    if(!this.active)return;
    this.time+=dt;
    const duration=this.reducedMotion ? .25 : 3.2;
    if(this.phase==='flooding'){
      this.progress=Math.min(1,this.progress+dt/duration);
      this.actors.forEach(a=>{a.y=a.originY+(a.groundY-a.originY)*this.progress;});
      if(this.progress>=1){this.phase='underwater';this.onChange();}
    }else if(this.phase==='draining'){
      this.progress=Math.max(0,this.progress-dt/duration);
      const p=1-this.progress/this.drainStart;
      this.actors.forEach(a=>{a.x=a.drainX+(a.originX-a.drainX)*p;a.y=a.drainY+(a.originY-a.drainY)*p;});
      if(this.progress<=0){const back=this.returnRequested;this.phase='dormant';this.actors=[];this.clear();this.onChange();this.onExit(back);}
    }else for(const a of this.actors){
      if(a.retract>=0){a.retract+=dt;if(a.retract>=2.2)a.retract=-1;continue;}
      if(a.pause>0){a.pause-=dt;continue;}
      a.moveFor-=dt;
      if(a.moveFor<=0){a.pause=1+this.random()*3;a.moveFor=3+this.random()*6;if(this.random()<.35)a.dir*=-1;continue;}
      const dx=a.speed*dt;a.x+=dx*a.dir;a.distance+=dx;
      if(a.x<15||a.x>241){a.x=Math.max(15,Math.min(241,a.x));a.dir*=-1;}
    }
  }
  snapshot(){return {phase:this.phase,progress:this.progress,ready:this.ready,error:this.error,selected:[...this.selected],time:this.time,actors:this.actors.map(a=>({...a,drawX:this.drawX(a),frame:this.frame(a)}))};}
}
function loadArt(data,ImageType=Image){
  if(!data?.background||!Array.isArray(data.coats)||!data.coats.length)return Promise.reject(Error('Falta la ilustración submarina.'));
  const image=url=>new Promise((resolve,reject)=>{const img=new ImageType();img.onload=()=>img.naturalWidth&&img.naturalHeight?resolve(img):reject(Error('La ilustración está vacía.'));img.onerror=()=>reject(Error('No se pudo cargar la ilustración submarina.'));img.src=url;});
  return Promise.all([image(data.background),...data.coats.map(c=>image(c.url))]).then(([background,...images])=>{
    data.coats.forEach((c,i)=>{
      if(!Number.isFinite(c.scale)||c.scale<=0)throw Error('No se pudo preparar el caracol.');
      for(const name of FRAME_NAMES){const r=c.frames?.[name];if(!r||![r.x,r.y,r.w,r.h,r.anchorX,r.anchorY].every(Number.isFinite)||r.x<0||r.y<0||r.w<=0||r.h<=0||r.x+r.w>images[i].naturalWidth||r.y+r.h>images[i].naturalHeight||r.anchorX<0||r.anchorX>1||r.anchorY<0||r.anchorY>1)throw Error('No se pudo preparar el caracol.');}
    });
    return {background,coats:data.coats.map((c,i)=>({...c,image:images[i],margin:1+Math.max(...FRAME_NAMES.map(name=>{const r=c.frames[name];return r.w*c.scale*Math.max(r.anchorX,1-r.anchorX);} ))}))};
  });
}
function flowers(g,secret,drawFlower,time){
  FLOWERS.forEach((f,i)=>{
    const selected=secret.selected.has(i),headY=f.y-1.3*FLOWER_SCALE;
    g.save();
    if(selected){
      const glow=g.createRadialGradient(f.x,headY,.5,f.x,headY,4);
      glow.addColorStop(0,'rgba(255,250,193,.95)');glow.addColorStop(.55,'rgba(255,223,109,.6)');glow.addColorStop(1,'rgba(255,223,109,0)');
      g.fillStyle=glow;g.beginPath();g.arc(f.x,headY,4,0,Math.PI*2);g.fill();
    }
    g.save();g.translate(f.x,f.y);g.scale(FLOWER_SCALE,FLOWER_SCALE);
    drawFlower(g,{x:0,y:0,c:f.c,p:i,prominent:true},time);g.restore();
    if(selected){
      g.translate(f.x+2.5,headY-2);g.scale(.65,.65);
      g.fillStyle='#fff6dc';g.strokeStyle='#526538';g.lineWidth=.35;
      g.beginPath();g.arc(0,0,1.8,0,Math.PI*2);g.fill();g.stroke();
      g.lineWidth=.55;g.lineCap='round';g.lineJoin='round';g.beginPath();g.moveTo(-.85,0);g.lineTo(-.25,.65);g.lineTo(.85,-.65);g.stroke();
    }
    g.restore();
  });
  secret.feedback.forEach(f=>{g.save();g.strokeStyle='#f8fff7';g.lineWidth=.6;g.globalAlpha=1-f.age/1.1;g.beginPath();g.arc(f.x,f.y-4-f.age*10,1.2+f.age,0,Math.PI*2);g.stroke();g.restore();});
}
function render(g,secret,art,width,height){
  if(!secret.active||!art)return;
  const p=secret.progress,line=height*(1-p);
  g.save();g.imageSmoothingEnabled=true;
  if(secret.reducedMotion)g.globalAlpha=p;
  else {g.beginPath();g.moveTo(0,height);g.lineTo(0,line);for(let x=0;x<=width;x+=2)g.lineTo(x,line+Math.sin(x/15+secret.time*2)*Math.sin(p*Math.PI)*1.2);g.lineTo(width,height);g.closePath();g.clip();}
  g.drawImage(art.background,0,0,width,height);
  secret.actors.slice().sort((a,b)=>a.y-b.y).forEach(a=>{
    const coat=art.coats[a.coat],r=coat.frames[secret.frame(a)],w=r.w*coat.scale,h=r.h*coat.scale;
    // One envelope for the entire sequence prevents clipping or shell shifts at
    // the canvas edge as the body changes width or retracts. Simulation is intact.
    const x=secret.drawX(a,width);
    g.save();g.translate(x,a.y);g.scale(a.dir,1);
    g.drawImage(coat.image,r.x,r.y,r.w,r.h,-w*r.anchorX,-h*r.anchorY,w,h);g.restore();
  });
  g.strokeStyle='#effffe';g.lineWidth=.45;g.globalAlpha*=.44;
  for(let i=0;i<8;i++){const x=20+i*31+Math.sin(secret.time*.4+i)*2,y=height-((secret.time*5+i*19)%height);g.beginPath();g.arc(x,y,.7+i%3*.35,0,Math.PI*2);g.stroke();}
  g.restore();
  if(!secret.reducedMotion&&p>0&&p<1){g.save();g.strokeStyle='#e3faff';g.lineWidth=.8;g.beginPath();for(let x=0;x<=width;x+=2)g[x?'lineTo':'moveTo'](x,line+Math.sin(x/15+secret.time*2)*Math.sin(p*Math.PI)*1.2);g.stroke();g.restore();}
}
window.ParkUnderwater={Secret,FLOWERS,flowerTarget,loadArt,flowers,render};
})();
