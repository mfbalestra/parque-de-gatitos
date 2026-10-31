const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const {makeSimulation}=require('./simulation.cjs');
const html=fs.readFileSync(path.join(__dirname,'..','index.html'),'utf8');
function fixture(count=4,seed=19){
 const settings={cats:[1,4,8].includes(count)?count:8,fps:60,companionMinutes:60,stayMinutes:60};
 const s=makeSimulation(html,{seed,storage:{'parque-gatitos.preferences.v1':JSON.stringify({version:1,settings})}});
 s.world.cats.splice(count);
 s.world.cats.forEach((c,i)=>Object.assign(c,{artIndex:0,st:'sit',tm:0,x:14+i*28,y:141,z:0,onS:false,motion:null,pair:null,cooldown:0,socialCooldown:0,restSpot:{x:14+i*28,y:141},restTravel:false}));
 return s;
}
{
 const s=fixture(),w=s.world,last=w.cats.at(-1);
 w.cats.slice(0,-1).forEach(c=>Object.assign(c,{st:'sleep',tm:90}));
 Object.assign(last,{st:'settle',tm:.01,settleDuration:4});
 const before=w.cats.slice(0,-1).map(c=>[c.st,c.tm,c.x,c.y]);
 w.updCat(last,.05);
 assert(['sit','groom'].includes(last.st),'The last awake cat must finish settling into a quiet awake activity');
 assert.equal(last.x,98);assert.equal(last.y,141);
 assert.deepEqual(w.cats.slice(0,-1).map(c=>[c.st,c.tm,c.x,c.y]),before,'Existing sleepers are untouched');
}
for(let count=1;count<=8;count++)for(let asleep=0;asleep<count;asleep++){
 const s=fixture(count),w=s.world,B=s.context.CatBehavior;
 w.cats.slice(0,asleep).forEach(c=>{c.st='sleep';c.restTravel=true;});
 for(let joining=1;joining<=Math.min(2,count-asleep);joining++){
  const candidates=w.cats.slice(asleep,asleep+joining);
  assert.equal(B.canSleep(w.cats,candidates),count===1||asleep+joining<count,'Sleep reservations must leave someone awake, including approaching sleepers');
 }
}
{
 const s=fixture(),w=s.world;
 assert(w.reserve(w.cats[0],w.cats[1],'cuddle'));
 assert(!s.context.CatBehavior.canSleep(w.cats,w.cats.slice(2)),'An approaching pair already reserves two sleepers');
 assert(!w.reserve(w.cats[2],w.cats[3],'cuddle'),'Two pairs cannot reserve the whole group for sleep');
 assert(w.reserve(w.cats[2],w.cats[3],'rub'),'An awake encounter remains available');
}
const calm=new Set();
for(let seed=1;seed<=120;seed++){
 const s=fixture(4,seed),w=s.world,c=w.cats[3];
 w.cats.slice(0,3).forEach(o=>Object.assign(o,{st:'sleep',tm:90,restTravel:true}));
 w.choose(c);assert.notEqual(c.st,'sleep','Direct choice must preserve the last awake cat');
 if(['sit','groom','knead','kneadApproach'].includes(c.st))calm.add(c.st);
}
assert(calm.has('sit')&&calm.has('groom'),'Staying awake must allow stationary calm activities');
for(const count of [2,4,8])for(const seed of [2,19,71])for(const phase of ['companion','visiting']){
 const s=fixture(count,seed),w=s.world;
 w.cycle.setPhase(phase);
 for(let step=0;step<12000;step++){
  w.step(.1);
  assert(w.cats.some(c=>c.st!=='sleep'&&c.pair?.action!=='cuddle'),'Autonomy must never commit the whole group to sleep');
 }
}
for(const count of [4,8])for(let seed=1;seed<=40;seed++){
 const settings={cats:count,fps:60,companionMinutes:60,stayMinutes:60};
 const s=makeSimulation(html,{seed,storage:{'parque-gatitos.preferences.v1':JSON.stringify({version:1,settings})}});
 assert(s.world.cats.some(c=>c.st!=='sleep'),'Incremental initialization must also leave an awake cat');
}
console.log('PASS awake reservation, direct choice, settling, pairs, quiet options and 20-minute autonomy');
