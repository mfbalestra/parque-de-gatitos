const fs=require('node:fs');
const path=require('node:path');
const assert=require('node:assert/strict');
const {makeSimulation}=require('./simulation.cjs');
const html=fs.readFileSync(path.join(__dirname,'../index.html'),'utf8');
const defaults={cats:8,fps:60,companionMinutes:30,stayMinutes:5};
const KEY='parque-gatitos.preferences.v1';
function runUntil(w,phase,max=6000){for(let i=0;i<max&&w.cycle.phase!==phase;i++)w.step(1/60);assert.equal(w.cycle.phase,phase,'phase must be reachable');}
function normal(value){return JSON.parse(JSON.stringify(value));}

for(const options of [{},{storage:{[KEY]:'broken'}},{storage:{[KEY]:JSON.stringify({version:1,settings:{...defaults,cats:3}})}},{readError:true}]){
  const sim=makeSimulation(html,options);assert.deepEqual(normal(sim.world.cycle.settings),defaults);assert.equal(sim.world.cats.length,8);
  if(Object.keys(options).length)assert(sim.element('scene-notice').textContent.includes('No se pudieron leer'));
}
console.log('PASS safe defaults and visible explanation for corrupt/denied storage');

{
  const s=makeSimulation(html),w=s.world,ids=[...w.cats];for(let i=0;i<240;i++)w.step(1/60);const elapsed=w.cycle.elapsed;
  assert.equal(w.api.save({...defaults,fps:30,stayMinutes:2}),null);assert.equal(w.cycle.elapsed,elapsed);assert(ids.every((c,i)=>c===w.cats[i]));
  assert(w.api.save({...defaults,cats:4}));assert.equal(w.cats.length,8);
  s.options.writeError=true;const previous=normal(w.cycle.settings),saved=s.storage.get(KEY);assert(w.api.save({...defaults,fps:60}));assert.deepEqual(normal(w.cycle.settings),previous);assert.equal(s.storage.get(KEY),saved);
  s.options.writeError=false;
  for(const invalid of [{stayMinutes:0},{companionMinutes:241},{stayMinutes:1.5},{fps:45},{cats:0}])assert(w.api.save({...previous,...invalid}));
  assert.deepEqual(normal(w.cycle.settings),previous);
  const reload=makeSimulation(html,{storage:Object.fromEntries(s.storage)});assert.deepEqual(normal(reload.world.cycle.settings),previous);
}
console.log('PASS atomic settings, validation, reload, elapsed/identity preservation and park count lock');

{
  const s=makeSimulation(html),w=s.world,cat=w.cats[0],ids=[...w.cats];
  Object.assign(cat,{x:182,y:118,z:40,onS:true,st:'gym',gi:5,pt:0,motion:null});
  w.beginJump(cat,194,118,40,4.5,'gym');w.box.cat=w.cats[1];w.cats[1].st='box';w.cats[1].at=1;
  w.api.back();assert.equal(w.box.cat,null);let previous=w.cats.map(c=>[c.x,c.y,c.z]),phases=new Set();
  for(let i=0;i<4000&&w.cycle.phase!=='companion';i++){
    w.step(1/60);phases.add(w.cycle.phase);
    w.cats.forEach((c,j)=>{assert(Math.hypot(c.x-previous[j][0],c.y-previous[j][1],c.z-previous[j][2])<3,'return must be continuous');assert(Number.isFinite(c.walkDistance));previous[j]=[c.x,c.y,c.z];});
  }
  assert.equal(w.cycle.phase,'companion');assert(phases.has('disappearing'));assert(ids.every((c,i)=>c===w.cats[i]));assert(w.cats.every(c=>!c.motion&&c.z===0));
  const ballCount=w.balls.length;s.click(.5,.8);s.key('Enter');s.key('ArrowRight');assert.equal(w.balls.length,ballCount);
  w.api.visit();w.api.visit();assert.equal(w.cycle.phase,'appearing');runUntil(w,'visiting');assert.equal(w.cycle.elapsed,0);assert(ids.every((c,i)=>c===w.cats[i]));
  w.api.back();w.api.visit();w.api.visit();assert(w.cycle.revisit);runUntil(w,'appearing');assert(!w.cycle.revisit);w.api.back();runUntil(w,'companion');
  assert.equal(w.api.save({...defaults,cats:1}),null);assert.equal(w.cats.length,1);assert.equal(w.cats[0].artIndex,0);assert.equal(w.box.cat,null);
  for(const count of [4,8]){assert.equal(w.api.save({...defaults,cats:count}),null);assert.equal(w.cats.length,count);assert.deepEqual(Array.from(w.cats,c=>c.artIndex),Array.from({length:count},(_,i)=>i));assert(w.cats.every(c=>c.tgt===null));}
}
console.log('PASS full transitions, active jump/platform descent, box release, repeated actions, queued revisit and 1/4/8 counts');

{
  const s=makeSimulation(html),w=s.world;
  for(let i=0;i<3660;i++)w.step(1/60);
  assert(w.cycle.elapsed>60);assert.equal(w.api.save({...defaults,stayMinutes:1,companionMinutes:1}),null);
  assert.equal(w.cycle.phase,'visiting','save must not advance a paused/current cycle by itself');
  w.step(1/60);assert.equal(w.cycle.phase,'leaving');runUntil(w,'companion');assert.equal(w.cycle.elapsed,0);
  for(let i=0;i<3599;i++)w.step(1/60);assert.equal(w.cycle.phase,'companion');w.step(1/60);w.step(1/60);assert.equal(w.cycle.phase,'appearing');
  runUntil(w,'visiting');assert.equal(w.cycle.elapsed,0,'stay starts only after arrival');
}
console.log('PASS saved shorter duration uses elapsed time; automatic return/visit clocks start at completed arrival');

{
  const s=makeSimulation(html),w=s.world;
  s.frame(1000/60);w.api.togglePause();const before=normal(w.cats),time=w.getTime(),elapsed=w.cycle.elapsed;assert.equal(s.pending.size,0);s.frame(900000);assert.equal(w.getTime(),time);assert.equal(w.cycle.elapsed,elapsed);assert.deepEqual(normal(w.cats),before);
  w.api.toggleHide();w.api.toggleHide();assert(w.api.state().paused);assert.equal(s.pending.size,0);
  w.api.togglePause();s.frame(900000+1000/60);assert(Math.abs(w.getTime()-time-1/60)<1e-8);
  s.visibility(true);const background=w.getTime();assert.equal(s.pending.size,0);s.frame(1800000);assert.equal(w.getTime(),background);s.visibility(false);s.frame(1800000+1000/60);assert(Math.abs(w.getTime()-background-1/60)<1e-8);
  w.api.toggleHide();const hidden=w.getTime();s.frame(2000000);assert.equal(w.getTime(),hidden);assert.equal(s.pending.size,0);
  s.context.parkArtChanged();assert.equal(s.pending.size,0);
  const reduced=makeSimulation(html,{reducedMotion:true});assert(reduced.world.api.state().paused);assert.equal(reduced.pending.size,0);reduced.context.parkArtChanged();assert.equal(reduced.pending.size,0);
}
console.log('PASS pause/hide/background cancel RAF, freeze actors/timers, no catchup, reduced motion and paused image redraw');

{
  const a=makeSimulation(html),b=makeSimulation(html);b.world.api.save({...defaults,fps:30});
  for(let i=1;i<=600;i++){a.frame(i*1000/60);b.frame(i*1000/60);}
  assert.equal(a.world.getTime(),b.world.getTime());
  for(let i=0;i<8;i++)for(const key of ['x','y','z','walkDistance','st','tm'])assert.equal(a.world.cats[i][key],b.world.cats[i][key]);
  const c=a.world.cats[0];Object.assign(c,{x:20,y:120,z:0,motion:null,walkDistance:0});a.world.moveTo(c,30,120,12,.5);assert.equal(c.walkDistance,6);const phase=a.world.walkPose(c);c.mv=false;assert.equal(a.world.walkPose(c),phase);
  a.world.beginJump(c,40,120,0,6,'play');while(c.motion)a.world.advanceJump(c,1/60);assert.equal(c.walkDistance,6);
  Object.assign(c,{st:'roll',tm:2,motion:null});a.world.updCat(c,1/60);assert.equal(c.walkDistance,6);
  Object.assign(c,{st:'gym',onS:true,gi:7,gp:0,pt:0,x:200,z:40,y:118});a.world.updCat(c,1/60);assert.equal(c.walkDistance,6);
}
console.log('PASS 30/60 FPS identical simulation; gait follows walking distance only, excluding jump/roll/slide');

{
  const {FramePacer}=makeSimulation(html).context.ParkCycle;
  for(const target of [30,60])for(const hz of [59.94,60,75,120,144])for(const jitter of [false,true]){
    const p=new FramePacer(target),duration=60;let count=0;
    for(let frame=0;frame<duration*hz;frame++){
      const timestamp=frame*1000/hz+(jitter?Math.sin(frame*1.713)*2.5:0);
      if(p.shouldRender(timestamp))count++;
      assert.equal(p.shouldRender(timestamp),false,'duplicate callback cannot draw twice');
    }
    const expected=duration*Math.min(target,hz);
    assert(Math.abs(count-expected)<=2,`${target} FPS / ${hz} Hz / jitter=${jitter}: ${count}, expected ${expected}`);
  }
  const delayed=new FramePacer(60);assert(delayed.shouldRender(0));assert(delayed.shouldRender(10000));assert(!delayed.shouldRender(10001));
  delayed.configure(30);assert(delayed.shouldRender(10002));assert(!delayed.shouldRender(10020));assert(delayed.shouldRender(10036));
}
console.log('PASS stable 30/60 render cadence across 59.94/60/75/120/144 Hz, timestamp jitter, duplicates and suspended gaps');

{
  const s=makeSimulation(html),el=s.element,w=s.world;
  el('open-controls').dispatch('click');assert(el('park-controls').open);el('close-controls').dispatch('click');assert(!el('park-controls').open);assert.equal(s.context.document.activeElement.id,'open-controls');
  el('companion-minutes').value='1.5';el('stay-minutes').value='0';el('park-settings').dispatch('submit');assert.equal(el('companion-minutes').attributes['aria-invalid'],'true');assert.equal(el('stay-minutes').attributes['aria-invalid'],'true');assert.equal(s.context.document.activeElement.id,'companion-minutes');assert(el('companion-minutes-label').scrolledIntoView,'invalid field and adjacent error must be brought into view together');
  el('companion-minutes').value='240';el('stay-minutes').value='60';el('park-settings').dispatch('input');el('park-settings').dispatch('submit');assert.equal(w.cycle.settings.companionMinutes,240);assert(el('settings-message').textContent.includes('guardadas'));
  el('panel-pause').dispatch('click');assert(w.api.state().paused);el('panel-hide').dispatch('click');assert(w.api.state().hidden);el('panel-hide').dispatch('click');assert(w.api.state().paused);
}
console.log('PASS controls wiring, error focus, dirty/success feedback and modal actions');
