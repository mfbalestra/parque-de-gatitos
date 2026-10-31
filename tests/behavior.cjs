const fs=require('node:fs');
const assert=require('node:assert/strict');
const {makeSimulation}=require('./simulation.cjs');
const html=fs.readFileSync(require('node:path').join(__dirname,'..','index.html'),'utf8');
const sim=makeSimulation(html,{orangePreview:true}),w=sim.world,B=sim.context.CatBehavior,P=sim.context.PARK_BEHAVIOR;
for(const action of ['knead','settle','rub','spar'])for(let frame=0;frame<4;frame++){
 w.cats[0].fk=action+frame;assert.doesNotThrow(()=>w.drawCat(w.cats[0]),'fallback '+action+frame);
}
for(const [name,[min,max]] of Object.entries(P.durations))for(let seed=1;seed<=100;seed++){
 const c={};B.init(c,seed);const duration=B.duration(c,name);assert(duration>=min&&duration<=max,name);
}
const sequences=new Set();
for(let seed=1;seed<=100;seed++){
 const c={};B.init(c,seed);const choices=[];
 for(let i=0;i<20;i++){const a=B.select(c,'park',['sleep','sit','groom','wander','knead']);choices.push(a);c.recent=a;}
 sequences.add(choices.join(','));
}
assert(sequences.size>90);
let low=0,high=0;
for(let seed=1;seed<=1000;seed++){
 const a={},b={};B.init(a,seed*1000003);B.init(b,seed*1000003);a.energy=0;b.energy=1;
 low+=B.select(a,'park',['sleep','wander'])==='sleep';high+=B.select(b,'park',['sleep','wander'])==='sleep';
}
assert(low>high);
for(const action of ['knead','settle','rub','cuddle','spar']){
 assert(sim.context.orangeDemo.choose(action));
 let saw=false;
 for(let i=0;i<1800;i++){w.step(1/60);const snap=sim.context.orangeDemo.snapshot();
  if(action==='knead'||action==='settle')saw||=snap.cats.some(c=>c.pose?.startsWith(action));
  else for(const pair of snap.pairs)if(pair.action===action&&pair.phase==='contact'){
   const [a,b]=pair.actors.map(i=>snap.cats[i]);assert(Math.abs(Math.hypot(a.x-b.x,a.y-b.y)-P.pairSeparation[action])<.1);saw=true;
  }
  for(const c of snap.cats)assert(Number.isFinite(c.x)&&Number.isFinite(c.y));
  if(saw)break;
 }
 assert(saw,action+' reaches contact or pose');
}
sim.context.orangeDemo.reset();
for(const pair of [...w.pairs])w.cancelPair(pair);
for(const c of w.cats){c.st='sit';c.tm=0;c.pair=null;c.cooldown=0;c.z=0;c.motion=null;}
assert(w.reserve(w.cats[0],w.cats[1],'spar'));
assert(!w.reserve(w.cats[2],w.cats[3],'spar'));
w.cancelPair(w.pairs[0]);assert(w.cats[0].cooldown>=P.playCooldown);
w.cats[0].tm=w.cats[1].tm=0;
assert(!w.reserve(w.cats[0],w.cats[1],'spar'));
w.cats[0].st='sleep';assert(!w.reserve(w.cats[0],w.cats[2],'rub'));
w.cats[0].st='sit';w.cats[0].cooldown=0;assert(w.reserve(w.cats[0],w.cats[2],'rub'));
w.cycle.back();assert.equal(w.pairs.length,0);assert(w.cats.every(c=>!c.pair));
assert.equal(sim.storage.size,0);
{
 const s=makeSimulation(html,{orangePreview:true}),world=s.world;
 world.cats.forEach((c,i)=>Object.assign(c,{x:[100,102,200,230][i],y:[120,130,106,106][i],st:'sit',tm:i<2?0:1000,motion:null,z:0,cooldown:0}));
 assert(world.reserve(world.cats[0],world.cats[1],'cuddle'));
 let contact=false;for(let i=0;i<400&&world.pairs.length;i++){
  world.step(.05);assert(world.cats[0].x<=world.cats[1].x,'partners retain left/right order');
  if(world.pairs[0]?.phase==='contact'){contact=true;break;}
 }
 assert(contact,'close diagonal partners reach contact before timeout');
}
{
 const s=makeSimulation(html,{orangePreview:true}),world=s.world,demo=s.context.orangeDemo;
 world.cats.forEach((c,i)=>Object.assign(c,{x:[45,96,190,230][i],y:[120,137,110,110][i],st:'sit',tm:1000,motion:null,z:0}));
 assert(demo.choose('knead'));const cat=world.cats[0];assert.equal(cat.st,'kneadApproach');assert.equal(cat.mat,0);assert.equal(cat.tm,0);
 while(cat.st==='kneadApproach')world.step(1/60);
 assert(Math.hypot(cat.x-27,cat.y-120)<1);assert(cat.tm>=8&&cat.tm<=14);
 const partner=world.cats[1];partner.st='groom';partner.tm=10;cat.st='wander';cat.mat=null;
 assert(!world.reserve(cat,partner,'rub'));partner.tm=0;assert(world.reserve(cat,partner,'rub'));
}
{
 const s=makeSimulation(html,{orangePreview:true}),world=s.world,demo=s.context.orangeDemo;
 assert(demo.setFps(30));assert.equal(world.cycle.settings.fps,30);assert.equal(demo.snapshot().fps,30);assert(!demo.setFps(45));assert.equal(s.storage.size,0);
 assert(demo.choose('settle'));const cat=world.cats[0],poses=[];
 while(cat.st==='settle'){world.step(.05);if(cat.fk?.startsWith('settle')&&poses.at(-1)!==cat.fk)poses.push(cat.fk);}
 assert.deepEqual(poses,['settle0','settle1','settle2','settle3']);assert.equal(cat.st,'sleep');assert(cat.tm>=60&&cat.tm<=180);
}
{
 const s=makeSimulation(html,{orangePreview:true}),world=s.world;
 world.cats.forEach((c,i)=>Object.assign(c,{x:30+i*55,y:141,st:'sit',tm:1000,motion:null,z:0,cooldown:0}));
 world.cycle.setPhase('companion');world.cats.forEach(c=>Object.assign(c,{st:'sit',tm:1000}));world.cats[0].tm=world.cats[1].tm=0;
 assert(world.reserve(world.cats[0],world.cats[1],'spar'));
 const roles=new Set();for(let i=0;i<600&&world.pairs.length;i++){
  world.step(.05);assert(world.cats.every(c=>c.y===141));
  if(world.pairs[0]?.phase==='contact'){const [a,b]=world.pairs[0].cats;roles.add(a.fk+','+b.fk);assert.equal((Number(b.fk.at(-1))-Number(a.fk.at(-1))+4)%4,2);}
 }
 for(let frame=0;frame<4;frame++)assert(roles.has('spar'+frame+',spar'+((frame+2)%4)));assert.equal(world.pairs.length,0);
 const rested=world.cats[0];assert(rested.cooldown>0);assert(rested.tm>=30);for(let i=0;i<590;i++){world.step(.05);assert.equal(rested.st,'sit');}world.cycle.setPhase('visiting');rested.st='sit';s.click(.3,.8);assert.equal(rested.st,'sit');
}
{
 const s=makeSimulation(html,{orangePreview:true}),world=s.world;
 world.cats.forEach((c,i)=>Object.assign(c,{x:[40,100,65,220][i],y:120,st:'sit',tm:1000,motion:null,z:0,cooldown:0}));
 world.cats[0].tm=world.cats[1].tm=0;
 assert(world.reserve(world.cats[0],world.cats[1],'rub'));
 s.click(.2,.8);assert(world.cats[0].pair&&world.cats[1].pair);world.cats[2].st='sit';world.cats[2].tm=1000;
 for(let i=0;i<410;i++)world.step(.05);
 assert.equal(world.pairs.length,0);assert(world.cats.every(c=>!c.pair));
}
for(const count of [1,4,8]){
 const settings={cats:count,fps:60,companionMinutes:30,stayMinutes:5};
 const s=makeSimulation(html,{storage:{'parque-gatitos.preferences.v1':JSON.stringify({version:1,settings})}}),world=s.world;
 assert.equal(world.cats.length,count);
 for(const c of world.cats){c.artIndex=0;c.st='sit';c.tm=0;c.cooldown=0;c.motion=null;c.z=0;}
 if(count>1){assert(world.reserve(world.cats[0],world.cats[1],'spar'));
  if(count===4)assert(!world.reserve(world.cats[2],world.cats[3],'spar'));
  if(count===8){assert(world.reserve(world.cats[2],world.cats[3],'spar'));assert(!world.reserve(world.cats[4],world.cats[5],'spar'));}
 }
 for(let i=0;i<3600;i++){world.step(1/60);for(const c of world.cats)assert(Number.isFinite(c.x)&&Number.isFinite(c.y));}
}
console.log('PASS durations, seeded choices, energy, orange actions, reservation, cooldown and cancellation');
