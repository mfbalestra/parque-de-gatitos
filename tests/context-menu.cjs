const assert=require('node:assert/strict');
const fs=require('node:fs');
const path=require('node:path');
const {makeSimulation}=require('./simulation.cjs');
const html=fs.readFileSync(path.join(__dirname,'../index.html'),'utf8');
const sim=makeSimulation(html),Gesture=sim.context.SceneGesture;
function setup(){
  let time=0,timer=null,background=true,underwater=false,menus=0,exits=0;
  const gesture=new Gesture({background:()=>background,underwater:()=>underwater,menu:()=>menus++,exit:()=>exits++,now:()=>time,schedule:fn=>(timer=fn,1),cancel:()=>{timer=null;}});
  return {gesture,background:value=>background=value,underwater:value=>underwater=value,advance:ms=>time+=ms,hold:()=>{time+=550;timer?.();},get menus(){return menus;},get exits(){return exits;}};
}
{
  const f=setup(),g=f.gesture;
  assert.equal(g.tap(20,20),false);f.advance(100);assert.equal(g.tap(20,20),false);assert.equal(f.exits,0,'ordinary double clicks remain ordinary input');
  g.start(20,20,1);f.hold();assert.equal(f.menus,1);g.hold();assert.equal(f.menus,1,'native contextmenu cannot repeat the held gesture');
  g.end(1);assert.equal(g.tap(20,20),true,'long press consumes its synthetic click before yarn or flower handlers');
  g.start(20,20,2);g.end(2);assert.equal(g.tap(20,20),false,'a new short tap works immediately');
}
{
  const f=setup(),g=f.gesture;f.background(false);g.start(20,20,1);f.background(true);f.hold();g.end(1);
  assert.equal(f.menus,0,'an actor moving away never turns an initial actor hold into background');
  assert.equal(g.tap(20,20),true,'holding an interactive flower does not select it on release');
  g.start(20,20,2);g.move(31,20,2);f.hold();g.end(2);assert.equal(f.menus,0);assert.equal(g.tap(31,20),true,'drag consumes accidental click');
  g.start(20,20,3);g.end(3,true);assert.equal(g.tap(20,20),true,'canceled touch cannot activate');
}
{
  const f=setup(),g=f.gesture;f.underwater(true);
  assert.equal(g.tap(20,20),false);f.advance(200);assert.equal(g.tap(25,22),true);assert.equal(f.exits,1,'nearby empty double tap exits');
  f.background(false);g.tap(20,20);f.advance(100);g.tap(20,20);assert.equal(f.exits,1,'repeated actor input never exits');
  f.background(true);g.tap(20,20);f.background(false);f.advance(100);g.tap(20,20);f.background(true);f.advance(100);g.tap(20,20);assert.equal(f.exits,1,'actor tap breaks empty sequence');
  g.reset();g.tap(20,20);f.advance(100);g.tap(80,20);assert.equal(f.exits,1,'distant empty taps are not a double tap');
  g.reset();g.tap(20,20);f.advance(351);g.tap(20,20);assert.equal(f.exits,1,'slow taps are not a double tap');
  g.reset();g.start(20,20,1);f.hold();g.end(1);g.tap(20,20);assert.equal(f.menus,1);assert.equal(f.exits,1,'long press opens menu without draining');
  g.reset();g.tap(20,20,true);f.advance(100);g.tap(20,20,true);assert.equal(f.exits,1,'keyboard button clicks cannot become double background taps');
}
{
  const el=sim.element,w=sim.world;
  el('scene-menu').showModal();el('toggle-pause').dispatch('click');assert(w.api.state().paused);assert(!el('scene-menu').open);assert.equal(sim.context.document.activeElement.id,'pk');
  el('scene-menu').showModal();el('toggle-hide').dispatch('click');assert(w.api.state().hidden);assert.equal(sim.context.document.activeElement.id,'body');
  assert(w.api.isBackground(20,20),'hidden scene allows context-menu recovery over any background');
  el('scene-menu').showModal();el('toggle-hide').dispatch('click');assert(!w.api.state().hidden);assert(w.api.state().paused,'showing never resumes a paused scene');
  el('scene-menu').showModal();el('open-controls').dispatch('click');assert(!el('scene-menu').open);assert(el('park-controls').open);
  el('close-controls').dispatch('click');assert.equal(sim.context.document.activeElement.id,'pk');
}
{
  const instrumented=html.replace('<script>',`<script>const originalAdd=document.addEventListener.bind(document);document.addEventListener=(name,fn,...args)=>{if(name==='contextmenu')window.recordedContextMenu=fn;originalAdd(name,fn,...args);};`);
  const s=makeSimulation(instrumented),menu=s.element('scene-menu');let prevented=0;
  menu.showModal();s.context.recordedContextMenu({preventDefault:()=>prevented++,target:{closest:()=>menu}});
  assert.equal(prevented,1,'an already open scene menu suppresses the browser context menu');assert(menu.open);
  menu.close();const settings=s.element('park-controls');settings.showModal();
  s.context.recordedContextMenu({preventDefault:()=>prevented++,target:{closest:()=>settings}});
  assert.equal(prevented,1,'configuration fields retain their browser copy/paste menu');
}
console.log('PASS context gestures: empty double taps, actor exclusion, long press suppression, drag/cancel, keyboard exclusion and hidden recovery/focus');
