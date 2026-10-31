const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm'),assert=require('node:assert/strict');
const {makeSimulation}=require('./simulation.cjs');
const root=path.join(__dirname,'..'),html=fs.readFileSync(path.join(root,'index.html'),'utf8');
const context={window:{}};vm.runInNewContext(fs.readFileSync(path.join(root,'interaction-art-data.js'),'utf8'),context);
const coats=context.window.PARK_INTERACTIONS.coats;
assert.equal(coats.length,8);
for(const coat of coats){
 const png=fs.readFileSync(path.join(root,coat.url));assert.equal(png.readUInt32BE(16),coat.width);assert.equal(png.readUInt32BE(20),coat.height);assert.equal(png[25],6);
 for(const action of ['knead','settle','rub','spar'])for(let i=0;i<4;i++){
  const r=coat.frames[action+i];assert(r&&r.x>=0&&r.y>=0&&r.w>0&&r.h>0&&r.x+r.w<=coat.width&&r.y+r.h<=coat.height);
  assert(Number.isFinite(r.anchorX)&&Number.isFinite(r.anchorY)&&Number.isFinite(coat.scale)&&coat.scale>0);
 }
}
const runtime={window:{PARK_INTERACTIONS:context.window.PARK_INTERACTIONS},Image:class{set src(url){const c=coats.find(c=>c.url===url);this.naturalWidth=c.width;this.naturalHeight=c.height;this.onload();}}};
vm.runInNewContext(fs.readFileSync(path.join(root,'scene-art.js'),'utf8'),runtime);const art=runtime.window.createParkArt(null);
for(let i=0;i<8;i++){
 for(const name of Object.keys(coats[i].frames))assert(art.pose({artIndex:i},name));
 assert.equal(art.pose({artIndex:i},'sleep').rect,coats[i].frames.settle3);
}
const old=coats[1].frames.knead0.x;coats[1].frames.knead0.x=coats[1].width;assert.equal(art.pose({artIndex:1},'knead0'),null);coats[1].frames.knead0.x=old;
const sim=makeSimulation(html,{interactionPreview:true}),w=sim.world,demo=sim.context.interactionDemo;
assert.equal(w.cats.length,8);assert.deepEqual(Array.from(w.cats,c=>c.artIndex),[0,1,2,3,4,5,6,7]);
for(let coat=0;coat<8;coat++)for(const action of ['knead','settle','rub','cuddle','spar']){
 assert(demo.choose(action,coat));const actor=w.cats[coat];
 if(action==='settle'){for(let i=0;i<1000&&actor.st==='settle';i++)w.updCat(actor,.1);assert.equal(actor.st,'sleep');}
 if(['rub','cuddle','spar'].includes(action)){assert(actor.pair);assert.notEqual(actor.pair.cats.find(c=>c!==actor).artIndex,coat);}
}
assert.equal(sim.storage.size,0);assert(sim.world.api.save(sim.world.cycle.settings));assert.equal(sim.storage.size,0);
for(let coat=0;coat<8;coat++){
 assert(demo.choose('cuddle',coat));const pair=w.cats[coat].pair,seen=new Set();
 for(let step=0;step<500&&pair.cats[0].fk!=='sleep';step++){
  w.updatePair(pair,.1);if(pair.phase==='contact'){seen.add(pair.cats[0].fk);assert.equal(pair.cats[0].fk,pair.cats[1].fk);}
 }
 for(let frame=0;frame<4;frame++)assert(seen.has('settle'+frame),'Both partners settle completely before sleeping');
 assert.equal(pair.cats[0].fk,'sleep');assert(pair.time>=60-.1,'Preparation does not consume the shared siesta');
}
console.log('PASS eight coat artwork, fallbacks, mixed pairs and mandatory settling');
