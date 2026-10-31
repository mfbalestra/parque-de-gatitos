const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const {makeSimulation}=require('./simulation.cjs');
const html=fs.readFileSync(path.join(__dirname,'..','index.html'),'utf8');
function cluster(count,seed,companion=false){
 const settings={cats:count,fps:60,companionMinutes:30,stayMinutes:5};
 const sim=makeSimulation(html,{seed,storage:{'parque-gatitos.preferences.v1':JSON.stringify({version:1,settings})}}),w=sim.world;
 if(companion)w.cycle.setPhase('companion');
 w.cats.forEach(c=>Object.assign(c,{x:237,y:companion?141:118,z:0,motion:null,st:'sleep',tm:90,onS:false,pair:null,restSpot:null,restTravel:false}));
 return sim;
}
for(const count of [1,4,8])for(const seed of [1,7,71,7123])for(const companion of [false,true]){
 const sim=cluster(count,seed,companion),w=sim.world,P=sim.context.PARK_BEHAVIOR;
 let previous=w.cats.map(c=>({x:c.x,y:c.y}));
 for(let step=0;step<3600;step++){
  const timers=w.cats.map(c=>c.tm);w.step(1/60);
  w.cats.forEach((c,i)=>{
   assert(Math.hypot(c.x-previous[i].x,c.y-previous[i].y)<1,'relocation must be continuous');
   if(c.restTravel){assert.equal(c.tm,timers[i],'sleep timer waits for arrival');if(c.mv)assert(/^walk[0-7AB]$/.test(c.fk),'walk to rest using walking artwork');}
   previous[i]={x:c.x,y:c.y};
  });
 }
 for(const c of w.cats){
  assert.equal(c.st,'sleep');assert(!c.restTravel,'find a reachable resting place '+JSON.stringify({count,seed,companion,x:c.x,y:c.y,target:c.restSpot,others:w.cats.map(o=>({x:o.x,y:o.y,target:o.restSpot,travel:o.restTravel}))}));
  assert(c.tm>=30-1e-7,'relocating never shortens a full sleep');
  if(!companion)assert(!P.restAvoid.some(p=>Math.hypot((c.x-p.x)/p.rx,(c.y-p.y)/p.ry)<1),'traffic points stay clear');
 }
 for(let i=0;i<count;i++)for(let j=i+1;j<count;j++)assert(Math.hypot((w.cats[i].x-w.cats[j].x)/26,(w.cats[i].y-w.cats[j].y)/14)>=.99,'durable solo rests must not overlap '+JSON.stringify({count,seed,companion,i,j,positions:w.cats.map(c=>({x:c.x,y:c.y,target:c.restSpot,travel:c.restTravel}))}));
}
{
 const sim=makeSimulation(html,{orangePreview:true}),w=sim.world,c=w.cats[0];
 Object.assign(c,{x:200,y:118.5,z:40,motion:null,onS:true,st:'gym',gi:7,gp:0,pt:0});
 sim.context.orangeDemo.reset();assert.equal(c.st,'gym');assert.equal(c.z,40);
 let slopeSamples=0;
 for(let step=0;step<240&&c.onS;step++){
  w.updCat(c,1/60);
  assert.equal(c.motion,null,'slide never starts a ballistic jump');
  if(c.x>200&&c.x<237){assert(Math.abs(c.z-(237-c.x)*40/37)<1e-6,'paws follow the ramp');assert.equal(c.fk,'sit');slopeSamples++;}
 }
 assert(slopeSamples>10);assert(!c.onS&&c.z===0,'finish slide on ground');
}
{
 const sim=makeSimulation(html,{orangePreview:true}),w=sim.world,c=w.cats[0];
 Object.assign(c,{x:200,y:118.5,z:40,motion:null,onS:true,st:'gym',gi:7,gp:0,pt:0});
 w.cycle.back();let samples=0;
 for(let i=0;i<180&&c.onS;i++){w.step(1/60);if(c.x>200&&c.x<237){assert(!c.motion,'return also completes the slide');samples++;}}
 assert(samples>10);assert(!c.onS);
}
console.log('PASS 24 clustered-rest scenarios, reserved space, complete rest timers, supported slide and return');
