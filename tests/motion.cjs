const fs=require('node:fs');
const path=require('node:path');
const assert=require('node:assert/strict');
const {makeSimulation}=require('./simulation.cjs');
const html=fs.readFileSync(path.join(__dirname,'../index.html'),'utf8');
const EPS=1e-8;

// Criteria come from the requested movement: grounded anticipation, a genuine
// accelerating arc with forward travel, planted landing, and a bounded recovery.
for(const dt of [1/120,1/60,1/30,.05])for(const kind of ['gym','yarn','butterfly','play','box']){
  const {world:w}=makeSimulation(html),c=w.cats[0];
  Object.assign(c,{x:130,y:118,z:0,vz:0,motion:null,onS:kind==='gym',gi:0,gp:0});
  const targetZ=kind==='gym'?8:0;
  w.beginJump(c,146,121,targetZ,kind==='butterfly'?17:6,kind);
  const phases=new Set(),poses=new Set();let previous={x:c.x,y:c.y,z:c.z},lastV=null,airFrames=0;
  for(let i=0;c.motion&&i<300;i++){
    const prior=c.motion.phase;w.advanceJump(c,dt);
    const m=c.motion;
    assert(Math.hypot(c.x-previous.x,c.y-previous.y)<100*dt+EPS,'forward position must stay continuous');
    assert(Math.abs(c.z-previous.z)<120*dt+EPS,'altitude must stay continuous');
    if(m){
      phases.add(m.phase);poses.add(c.fk);
      if(m.phase==='prepare'){assert.equal(c.x,130);assert.equal(c.z,0);}
      if(m.phase==='air'){
        airFrames++;
        if(prior==='air'&&lastV!==null)assert(Math.abs(c.vz-lastV+220*dt)<EPS,'gravity must change velocity each flight frame');
        lastV=c.vz;
      }
      if(m.phase==='landing'||m.phase==='recover'){assert.equal(c.x,146);assert.equal(c.y,121);assert.equal(c.z,targetZ);assert.equal(c.vz,0);}
    }
    previous={x:c.x,y:c.y,z:c.z};
  }
  assert.equal(c.motion,null,'jump must always complete');
  assert.deepEqual([...phases].sort(),['air','landing','prepare','recover']);
  for(const pose of ['crouch','jumpRise','jumpTuck','jumpFall','landing'])assert(poses.has(pose),'reachable anatomical pose: '+pose);
  assert(airFrames>4);assert.equal(c.z,targetZ);assert.equal(c.deform,null);
}
console.log('PASS five jump kinds at four frame rates: impulse, gravity, travel, five poses, grounded landing/recovery');

{
  const {world:w}=makeSimulation(html),c=w.cats[0];
  Object.assign(c,{x:182,y:118.6,z:0,vz:0,motion:null,st:'scratch',at:1,tm:4,onS:false,an:0});
  const poses=new Set();
  for(let i=0;i<120;i++){w.updCat(c,1/60);poses.add(c.fk);assert.deepEqual([c.x,c.y,c.z,c.vz],[182,118.6,0,0]);assert(!c.deform);}
  assert.deepEqual([...poses].sort(),['scratchA','scratchB']);
}
console.log('PASS scratch alternates forelegs while body and support remain fixed');

// Longer rests and weighted selection invalidate the old expectation that a
// 600-second random trace must trigger every jump. Exercise each trigger from
// a deliberate setup while keeping the trajectory checks above and below.
for(const kind of ['gym','yarn','butterfly','play','box']){
  const {world:w}=makeSimulation(html),c=w.cats[0];
  Object.assign(c,{x:134,y:118,z:0,vz:0,motion:null,onS:false,at:0,tm:10,an:0});
  if(kind==='gym')Object.assign(c,{st:'gym',onS:true,gi:0,gp:0,pt:0});
  if(kind==='yarn'||kind==='butterfly'){
    const target=kind==='yarn'?w.balls[0]:w.flies[0];
    target.x=c.x;target.y=kind==='yarn'?c.y:c.y-26;Object.assign(c,{st:'chase',tgt:target});
  }
  if(kind==='box'){w.box.cat=c;Object.assign(c,{st:'box',at:1,tm:-1});}
  if(kind==='play')w.beginJump(c,c.x+7,c.y,0,6,'play');
  else w.updCat(c,1/60);
  assert.equal(c.motion?.kind,kind,'explicit trigger: '+kind);
  const phases=new Set();let previous=[c.x,c.y,c.z];
  while(c.motion){phases.add(c.motion.phase);w.advanceJump(c,1/60);assert(Math.hypot(c.x-previous[0],c.y-previous[1],c.z-previous[2])<3,'continuous '+kind);previous=[c.x,c.y,c.z];}
  assert.deepEqual([...phases].sort(),['air','landing','prepare','recover']);
}
console.log('PASS explicit triggers and continuous jump phases');

for(const throws of [false,true]){
  const sim=makeSimulation(html),w=sim.world;
  assert.deepEqual(Array.from(w.cats,c=>c.artIndex),[0,1,2,3,4,5,6,7]);
  const kinds=new Set(),phases=new Set();let previous=w.cats.map(c=>[c.x,c.y,c.z]);
  for(let frame=0;frame<36000;frame++){
    if(throws&&frame%240===0)sim.click((frame%983)/983,.85);
    w.step(1/60);
    w.cats.forEach((c,i)=>{
      for(const value of [c.x,c.y,c.z,c.vz,c.tm,c.an])assert(Number.isFinite(value),'finite state');
      assert(c.x>=9&&c.x<=247&&c.y>=106&&c.y<=141&&c.z>=-EPS&&c.z<=60,'bounded scene');
      assert(Math.hypot(c.x-previous[i][0],c.y-previous[i][1],c.z-previous[i][2])<3,'no actor teleports');
      if(c.motion){kinds.add(c.motion.kind);phases.add(c.motion.phase);assert(c.motion.time<2,'no runaway motion');}
      previous[i]=[c.x,c.y,c.z];
    });
    assert(w.balls.length<=7);
  }
}
console.log('PASS 20 simulated minutes: eight identities, finite/continuous state and bounded balls');
