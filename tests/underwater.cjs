const fs=require('node:fs');
const path=require('node:path');
const vm=require('node:vm');
const assert=require('node:assert/strict');
const {createHash}=require('node:crypto');
const {makeSimulation,snapshot}=require('./simulation.cjs');
const root=path.resolve(__dirname,'..');
const source=fs.readFileSync(path.join(root,'underwater.js'),'utf8');
const html=fs.readFileSync(path.join(root,'index.html'),'utf8');
const context={};context.window=context;vm.createContext(context);vm.runInContext(source,context);
const {Secret,FLOWERS,loadArt}=context.ParkUnderwater;
const plain=x=>JSON.parse(JSON.stringify(x));
const click=(sim,i)=>sim.click(FLOWERS[i].x/256,FLOWERS[i].y/144);
const advance=(sim,seconds)=>{for(let i=0;i<Math.round(seconds*60);i++)sim.world.step(1/60);};
const normal=sim=>JSON.stringify({actors:snapshot(sim.world),elapsed:sim.world.cycle.elapsed,phase:sim.world.cycle.phase,opacity:sim.world.cycle.opacity,revisit:sim.world.cycle.revisit});
async function pilot(options={},fixture={}){
  // The existing real-scene harness only loads the normal modules. Add the secret
  // module and an already-decoded art fixture; asset failures are tested below.
  const setup=source+`;location.search=${JSON.stringify(fixture.normal?'':'?orange-preview=1&snail-preview=1')};
  window.PARK_SNAIL_ART={};
  window.artLoads=0;window.ParkUnderwater.loadArt=()=>{window.artLoads++;const art={background:{},coats:Array.from({length:8},()=>({image:{},scale:1,margin:12,frames:Object.fromEntries(['crawl0','crawl1','crawl2','crawl3','retract0','retract1','retract2','hidden'].map(k=>[k,{x:0,y:0,w:20,h:16,anchorX:.5,anchorY:1}]))}))};return ${fixture.delayed?'new Promise(resolve=>window.finishArt=()=>resolve(art))':'Promise.resolve(art)'};};`;
  const prepared=fixture.delayed?html:html.replace('controls=window.createWebControls(api,loaded.message);','if(underwater){underwater.ready=true;underwater.margins=Array(8).fill(12);}controls=window.createWebControls(api,loaded.message);');
  const sim=makeSimulation(prepared.replace('<script>','<script>'+setup),options);
  await Promise.resolve();await Promise.resolve();return sim;
}
async function run(){
  const marked=await pilot();
  const flower=FLOWERS[0],ballsBefore=marked.world.balls.length;
  marked.click(flower.x/256,(flower.y-5)/144);
  assert.equal(marked.context.snailDemo.snapshot().selected.length,1,'upper petals of the enlarged flower accept clicks');
  assert.equal(marked.world.balls.length,ballsBefore,'enlarged petals do not throw yarn');
  advance(marked,3);
  assert.equal(marked.context.snailDemo.snapshot().selected.length,1,'selection persists beyond the transient bubble');
  assert.equal(marked.element('secret-flower-0').attributes['aria-pressed'],'true');
  assert.equal(marked.element('secret-flower-1').attributes['aria-pressed'],'false');
  marked.world.api.back();
  assert.equal(marked.element('secret-flower-0').attributes['aria-pressed'],'false','leaving clears the counted state');
  console.log('PASS enlarged petals, persistent counted state, accessible feedback and reset');
  for(const order of [[0,1,2],[2,0,1],[1,2,0]]){
    const sim=await pilot();const before=normal(sim),balls=sim.world.balls.length;
    click(sim,order[0]);click(sim,order[0]);assert.equal(sim.context.snailDemo.snapshot().selected.length,1);
    click(sim,order[1]);assert.equal(sim.context.snailDemo.snapshot().phase,'dormant');
    click(sim,order[2]);assert.equal(sim.context.snailDemo.snapshot().phase,'flooding');
    assert.equal(sim.world.balls.length,balls,'flower input never also throws yarn');
    advance(sim,4);assert.equal(sim.context.snailDemo.snapshot().phase,'underwater');
    assert.equal(sim.context.snailDemo.snapshot().actors.length,sim.world.cats.length);
    advance(sim,20);assert.equal(normal(sim),before,'normal actors, RNG, pairs, objects and both clocks freeze');
    sim.context.snailDemo.exit();advance(sim,3.2);
    assert.equal(sim.context.snailDemo.snapshot().phase,'dormant');
    assert.equal(normal(sim),before,'exit restores exact normal state before any next normal tick');
    assert.equal(sim.context.snailDemo.snapshot().selected.length,0);
    sim.world.step(1/60);assert(sim.world.cycle.elapsed>0);
  }
  console.log('PASS any-order distinct flowers, no dual yarn, independent actors and exact restoration');
  const sim=await pilot();
  // Explicit social/rest/gym data must survive, even if inaccessible to random play.
  sim.world.cats[0].st='sleep';sim.world.cats[0].tm=55;
  const pair={action:'cuddle',phase:'hold',time:7,cats:[sim.world.cats[1],sim.world.cats[2]]};
  sim.world.pairs.push(pair);pair.cats.forEach(c=>{c.pair=pair;c.st='social';});
  Object.assign(sim.world.cats[3],{st:'gym',onS:true,z:24,gi:4,gp:.6});
  const preserved=normal(sim);[0,1,2].forEach(i=>click(sim,i));advance(sim,4);
  const a=sim.context.snailDemo.snapshot().actors[0];sim.click(a.x/256,(a.y-7)/144);
  assert.equal(sim.context.snailDemo.snapshot().actors[0].frame,'retract0');advance(sim,.6);
  assert.equal(sim.context.snailDemo.snapshot().actors[0].frame,'hidden');advance(sim,1.2);
  assert.match(sim.context.snailDemo.snapshot().actors[0].frame,/retract/);advance(sim,.5);
  assert.match(sim.context.snailDemo.snapshot().actors[0].frame,/crawl/);
  for(const toggle of [()=>sim.world.api.togglePause(),()=>sim.world.api.toggleHide(),()=>sim.visibility(!sim.context.document.hidden)]){
    toggle();const before=JSON.stringify(sim.context.snailDemo.snapshot());advance(sim,2);
    assert.equal(JSON.stringify(sim.context.snailDemo.snapshot()),before,'pause/hide/background freezes secret');toggle();
  }
  sim.world.api.visit();assert.equal(sim.world.cycle.revisit,false,'visit while submerged does not queue visit');
  const elapsed=sim.world.cycle.elapsed;
  assert.equal(sim.world.api.save({...sim.world.cycle.settings,fps:30,stayMinutes:10}),null);
  assert.equal(sim.world.cycle.elapsed,elapsed);assert.equal(sim.world.cycle.settings.fps,30);
  assert(sim.storage.size>0,'pilot settings retain normal save contract');
  sim.context.snailDemo.exit();advance(sim,3.2);assert.equal(normal(sim),preserved);
  console.log('PASS retract/emerge, sleep/pair/gym preservation, all freezes and live saved preferences');
  const paused=await pilot();[0,1,2].forEach(i=>click(paused,i));advance(paused,1);
  paused.world.api.togglePause();paused.world.api.back();assert.equal(paused.context.snailDemo.snapshot().phase,'draining');
  advance(paused,4);assert.equal(paused.world.cycle.phase,'visiting');assert(paused.world.api.state().paused);
  paused.world.api.togglePause();advance(paused,1.1);assert.equal(paused.world.cycle.phase,'leaving');
  const reduced=await pilot({reducedMotion:true});click(reduced,0);assert.equal(reduced.context.snailDemo.snapshot().selected.length,0);
  reduced.world.api.togglePause();[0,1,2].forEach(i=>click(reduced,i));advance(reduced,.3);
  assert.equal(reduced.context.snailDemo.snapshot().phase,'underwater');
  reduced.context.snailDemo.exit();advance(reduced,.3);assert.equal(reduced.context.snailDemo.snapshot().phase,'dormant');
  console.log('PASS paused drain queues normal return; reduced motion respects initial pause and short transition');
  const keyboard=await pilot();
  // Arrow grid from the existing (128,124) aim reaches every target tolerance.
  for(let i=0;i<17;i++)keyboard.key('ArrowLeft');for(let i=0;i<3;i++)keyboard.key('ArrowUp');keyboard.key('Enter');
  for(let i=0;i<16;i++)keyboard.key('ArrowRight');for(let i=0;i<3;i++)keyboard.key('ArrowDown');keyboard.key('Enter');
  for(let i=0;i<19;i++)keyboard.key('ArrowRight');for(let i=0;i<2;i++)keyboard.key('ArrowDown');keyboard.key('Enter');
  assert.equal(keyboard.context.snailDemo.snapshot().phase,'flooding');
  const partial=await pilot();click(partial,0);partial.world.api.back();assert.equal(partial.context.snailDemo.snapshot().selected.length,0);
  const reload=await pilot();assert.equal(reload.context.snailDemo.snapshot().phase,'dormant');assert.equal(reload.context.snailDemo.snapshot().selected.length,0);
  for(const options of [{orangePreview:true},{interactionPreview:true}]){
    const ordinary=makeSimulation(html.replace('<script>','<script>'+source),options);assert.equal(ordinary.context.snailDemo,undefined);
    const count=ordinary.world.balls.length;click(ordinary,0);assert.equal(ordinary.world.balls.length,count+1);
  }
  console.log('PASS keyboard target grid, clear on leaving/reload, art-only preview routes unchanged');
  for(const count of [1,4,8]){
    const settings={cats:count,fps:60,companionMinutes:30,stayMinutes:5};
    const ordinary=await pilot({storage:{'parque-gatitos.preferences.v1':JSON.stringify({version:1,settings})}},{normal:true});
    const before=normal(ordinary),coats=Array.from(ordinary.world.cats,c=>c.artIndex);
    assert.equal(ordinary.context.location.search,'');assert.equal(ordinary.world.api.state().fixedCount,false);
    [0,1,2].forEach(i=>click(ordinary,i));advance(ordinary,4);
    assert.equal(ordinary.context.snailDemo.snapshot().phase,'underwater');
    assert.deepEqual(plain(ordinary.context.snailDemo.snapshot().actors.map(a=>a.coat)),coats);
    assert.equal(coats.length,count);assert.deepEqual(coats,Array.from({length:count},(_,i)=>i));
    ordinary.context.snailDemo.exit();advance(ordinary,3.2);assert.equal(normal(ordinary),before);
    ordinary.world.api.back();for(let i=0;i<6000&&ordinary.world.cycle.phase!=='companion';i++)ordinary.world.step(1/60);
    assert.equal(ordinary.world.cycle.phase,'companion');assert.equal(ordinary.element('cat-count').disabled,false);
    for(const next of [1,4,8]){
      assert.equal(ordinary.world.api.save({...settings,cats:next}),null);assert.equal(ordinary.world.cats.length,next);
      assert.deepEqual(Array.from(ordinary.world.cats,c=>c.artIndex),Array.from({length:next},(_,i)=>i));
    }
    ordinary.world.api.visit();for(let i=0;i<6000&&ordinary.world.cycle.phase!=='visiting';i++)ordinary.world.step(1/60);
    assert.equal(ordinary.world.cycle.phase,'visiting');[0,1,2].forEach(i=>click(ordinary,i));
    assert.deepEqual(plain(ordinary.context.snailDemo.snapshot().actors.map(a=>a.coat)),[0,1,2,3,4,5,6,7]);
  }
  console.log('PASS normal route enabled without query flags, exact 1/4/8 coat identity, restoration and count changes through companion');
  const lazy=await pilot({}, {normal:true,delayed:true});
  assert.equal(lazy.context.artLoads,0,'ordinary entry must not download underwater images');
  lazy.world.api.togglePause();click(lazy,0);assert.equal(lazy.context.artLoads,0,'paused touch must not start preload');lazy.world.api.togglePause();
  [0,1,2,0].forEach(i=>click(lazy,i));assert.equal(lazy.context.artLoads,1);
  assert.equal(lazy.context.snailDemo.snapshot().phase,'dormant');assert.equal(lazy.context.snailDemo.snapshot().selected.length,3);
  lazy.world.api.togglePause();lazy.context.finishArt();await Promise.resolve();await Promise.resolve();
  assert.equal(lazy.context.snailDemo.snapshot().phase,'dormant','late loading must not activate while paused');
  lazy.world.api.togglePause();lazy.world.step(1/60);assert.equal(lazy.context.snailDemo.snapshot().phase,'flooding');
  assert.equal(lazy.context.document.activeElement.id,'pk','late activation moves focus away from the hidden flower targets');
  const leftWhileLoading=await pilot({}, {normal:true,delayed:true});[0,1,2].forEach(i=>click(leftWhileLoading,i));
  leftWhileLoading.world.api.back();leftWhileLoading.context.finishArt();await Promise.resolve();await Promise.resolve();
  assert.equal(leftWhileLoading.context.snailDemo.snapshot().phase,'dormant');assert.equal(leftWhileLoading.context.snailDemo.snapshot().selected.length,0);
  console.log('PASS first-flower deferred load once, pending selection, paused completion and leaving while loading');
  const fixed=await pilot();fixed.world.api.back();
  for(let i=0;i<6000&&fixed.world.cycle.phase!=='companion';i++)fixed.world.step(1/60);
  assert.equal(fixed.world.cycle.phase,'companion');assert.equal(fixed.world.api.state().settings.cats,4);
  assert.equal(fixed.element('cat-count').disabled,true);assert.match(fixed.element('count-help').textContent,/cuatro gatos naranjas/);
  const savedBefore=JSON.stringify([...fixed.storage]);
  for(const cats of [1,8])assert.match(fixed.world.api.save({...fixed.world.cycle.settings,cats}),/cuatro gatos/);
  assert.equal(fixed.world.cats.length,4);assert.equal(fixed.world.api.state().settings.cats,4);
  assert.equal(JSON.stringify([...fixed.storage]),savedBefore,'rejected pilot count never persists');
  assert.equal(fixed.world.api.save({...fixed.world.cycle.settings,fps:30,companionMinutes:15}),null);
  console.log('PASS pilot count remains four in companion; FPS/time saving remains available');
  const unavailable=new Secret();assert.equal(unavailable.select(0,[],true),true);
  assert.equal(unavailable.selected.size,1);assert.equal(unavailable.phase,'dormant');
  const missingCoat=new Secret();missingCoat.ready=true;missingCoat.margins=[12];
  [0,1,2].forEach(i=>missingCoat.select(i,[{artIndex:7}],true));
  assert.equal(missingCoat.phase,'dormant');assert.match(missingCoat.error,/variedad/,'missing coat cannot silently wrap to orange');
  await assert.rejects(loadArt(null,class {}),/Falta/);
  class BrokenImage{set src(value){this.onerror();}}
  await assert.rejects(loadArt({background:'bg',coats:[{url:'snail'}]},BrokenImage),/cargar/);
  class DecodedImage{constructor(){this.naturalWidth=100;this.naturalHeight=100;}set src(value){this.onload();}}
  await assert.rejects(loadArt({background:'bg',coats:[{url:'snail',scale:1,frames:{}}]},DecodedImage),/preparar/);
  const frames=Object.fromEntries(['crawl0','crawl1','crawl2','crawl3','retract0','retract1','retract2','hidden'].map(name=>[name,{x:0,y:0,w:20,h:20,anchorX:.4,anchorY:1}]));
  assert.equal((await loadArt({background:'bg',coats:[{url:'snail',scale:1,frames}]},DecodedImage)).coats.length,1);
  console.log('PASS preload gate, decode and missing/invalid artwork failures');
  vm.runInContext(fs.readFileSync(path.join(root,'snail-art-data.js'),'utf8'),context);
  class LocalPngImage{set src(url){try{const bytes=fs.readFileSync(path.join(root,url));assert.equal(bytes.subarray(1,4).toString(),'PNG');this.naturalWidth=bytes.readUInt32BE(16);this.naturalHeight=bytes.readUInt32BE(20);this.onload();}catch{this.onerror();}}}
  const real=await loadArt(context.PARK_SNAIL_ART,LocalPngImage);
  assert.deepEqual(Array.from(real.coats,c=>c.id),['naranja','gris','calico','negro','blanco','tuxedo','siames','atigrado']);
  const provenance=JSON.parse(fs.readFileSync(path.join(root,'assets/snail-provenance.json'),'utf8'));
  assert.deepEqual(provenance.map(({id,url})=>({id,url})),plain(real.coats.map(({id,url})=>({id,url}))),'reviewed asset identities and URLs match all eight descriptor entries');
  for(const {id,url,sha256} of provenance){
    assert.match(sha256,/^[a-f0-9]{64}$/);
    assert.equal(createHash('sha256').update(fs.readFileSync(path.join(root,url))).digest('hex'),sha256,`${id}: image must match the reviewed whole-character artwork`);
  }
  console.log('PASS eight reviewed SHA256 hashes and exact descriptor/resource correspondence');
  const edge=new Secret();edge.phase='underwater';edge.margins=real.coats.map(c=>c.margin);
  for(const [index,coat] of real.coats.entries())for(const x of [6,9,15,241,247])for(const dir of [-1,1])for(const r of Object.values(coat.frames)){
    const actor={x,y:130,coat:index,dir,retract:-1};edge.actors=[actor];
    const drawX=edge.drawX(actor);
    const left=drawX-r.w*coat.scale*(dir===1?r.anchorX:1-r.anchorX),right=drawX+r.w*coat.scale*(dir===1?1-r.anchorX:r.anchorX);
    assert(left>=1-1e-9&&right<=255+1e-9,'all real poses and both directions stay inside the canvas');
    edge.touch(drawX,123,4.5,true);assert.equal(actor.retract,0,'touch matches the displayed edge position');
  }
  console.log('PASS real PNG dimensions, all eight coats × eight crop bounds, fractional anchors and both-direction edge touch');
}
run().catch(error=>{console.error(error);process.exitCode=1;});
