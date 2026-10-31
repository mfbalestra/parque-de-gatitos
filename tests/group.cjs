const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const {makeSimulation}=require('./simulation.cjs');
const html=fs.readFileSync(path.join(__dirname,'..','index.html'),'utf8');
const B=makeSimulation(html).context.CatBehavior;
for(let e=0;e<=10;e++)for(let q=0;q<20;q++){
 const energy=e/10,quiet=q/20;
 for(const action of ['sleep','sit','wander','groom','knead','social','environment']){
  const a=B.groupFactor(action,quiet,energy),b=B.groupFactor(action,quiet+.05,energy);
  assert(a>0&&b>0,'No activity becomes compulsory or forbidden');
  if(action==='sleep'||action==='sit')assert(b<=a+1e-10,'More quiet peers cannot increase a new long rest');
  else assert(b>=a-1e-10,'More quiet peers encourage available activity');
  if(quiet<=.5)assert.equal(a,1,'An active group needs no correction');
 }
}
assert(B.groupFactor('wander',1,1)>B.groupFactor('wander',1,0),'Tired cats retain more freedom to rest');
function run(count,seed,enabled){
 const settings={cats:count,fps:60,companionMinutes:30,stayMinutes:60};
 const sim=makeSimulation(html,{seed,storage:{'parque-gatitos.preferences.v1':JSON.stringify({version:1,settings})}}),w=sim.world;
 if(!enabled)for(const key of Object.keys(sim.context.PARK_BEHAVIOR.groupBalance.multipliers))sim.context.PARK_BEHAVIOR.groupBalance.multipliers[key]=1;
 let quiet=0,activeTotal=0;
 for(let i=0;i<12000;i++){
  w.step(.1);const active=w.cats.filter(c=>c.mv||c.motion||c.st==='knead'||c.st==='groom'||(c.pair&&c.pair.action!=='cuddle')).length;
  quiet+=active===0;activeTotal+=active;
 }
 return {quiet,activeTotal,final:w.cats.map(c=>[c.x,c.y,c.st,c.tm,c.energy])};
}
let before=0,after=0;
for(const count of [4,8])for(const seed of [19,71,7123]){
 const a=run(count,seed,false),b=run(count,seed,true);before+=a.quiet;after+=b.quiet;
 assert(b.activeTotal<12000*count*.65,'The group should retain mostly calm time');
}
assert(after<before*.75,'Group context must materially reduce completely quiet periods across seeds');
assert.equal(JSON.stringify(run(1,19,true)),JSON.stringify(run(1,19,false)),'Solo behavior must be unchanged');
const sim=makeSimulation(html),w=sim.world;
for(const pair of [...w.pairs])w.cancelPair(pair);
w.cats.forEach((c,i)=>Object.assign(c,{st:'sleep',tm:90,x:20+i*28,y:141,onS:false,z:0,motion:null,restSpot:null,restTravel:false}));
for(let i=0;i<100;i++)w.step(.1);
assert(w.cats.every(c=>c.st==='sleep'&&c.tm>=80-1e-6),'Group balancing never wakes existing sleepers');
console.log('PASS group bias, fatigue, positive choices, aggregate balance, unchanged solo and uninterrupted sleep');
