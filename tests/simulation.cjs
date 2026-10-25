const fs=require('node:fs');
const path=require('node:path');
const vm=require('node:vm');
const crypto=require('node:crypto');
const assert=require('node:assert/strict');
const root=path.resolve(__dirname,'..');

function makeSimulation(html){
  const listeners={},mediaListeners=[];
  const context2d=new Proxy({}, {get:(o,k)=>k in o?o[k]:(()=>{}),set:(o,k,v)=>(o[k]=v,true)});
  const rectangle={left:0,top:0,width:1280,height:720};
  const canvas={width:1536,height:864,getContext:()=>context2d,getBoundingClientRect:()=>rectangle,addEventListener:(name,fn)=>{listeners[name]=fn}};
  const status={textContent:''};
  let frame;
  const context={console,performance:{now:()=>0},requestAnimationFrame:fn=>{frame=fn},document:{getElementById:id=>id==='pk'?canvas:status,createElement:()=>({...canvas})},matchMedia:()=>({matches:false,addEventListener:(name,fn)=>mediaListeners.push(fn)})};
  context.window=context;
  vm.createContext(context);
  const inline=[...html.matchAll(/<script>([\s\S]*?)<\/script>/g)].map(m=>m[1]).join('\n');
  assert(inline.includes('function step(dt)'), 'The real simulation must be loaded');
  const hook=`globalThis.__test={step,render,cats,balls,flies,box,GYM,getTime:()=>t,getSeed:()=>seed};`;
  vm.runInContext(inline.replace(/\}\)\(\);\s*$/,hook+'})();'),context,{timeout:3000});
  return {world:context.__test,click:(x,y)=>listeners.click({clientX:rectangle.left+x*rectangle.width,clientY:rectangle.top+y*rectangle.height}),key:key=>listeners.keydown({key,repeat:false,preventDefault(){}}),rectangle,frame:now=>frame(now),status,media:matches=>mediaListeners.forEach(fn=>fn({matches}))};
}
function snapshot(w){
  const objects=[...w.cats,...w.balls,...w.flies];
  const cats=w.cats.map(cat=>Object.fromEntries(Object.keys(cat).filter(k=>k!=='fr'&&k!=='artIndex').sort().map(k=>[k,k==='tgt'?objects.indexOf(cat[k]):cat[k]])));
  return {seed:w.getSeed(),t:w.getTime(),cats,balls:w.balls,flies:w.flies,box:w.cats.indexOf(w.box.cat)};
}
function trace(html,throws){
  const sim=makeSimulation(html),hash=crypto.createHash('sha256'),states=new Set(),gym=new Set();
  for(let frame=0;frame<12000;frame++){
    if(throws&&frame%240===0)sim.click(((frame/240)*37%100)/100,.78+(frame%3)*.08);
    sim.world.step(1/60);
    for(const cat of sim.world.cats){states.add(cat.st);if(cat.onS)gym.add(cat.gi);}
    if(frame%30===0)hash.update(JSON.stringify(snapshot(sim.world)));
  }
  return {digest:hash.digest('hex'),states:[...states].sort(),gymWaypoints:[...gym].sort((a,b)=>a-b)};
}
function run(){
  const html=fs.readFileSync(path.join(root,'index.html'),'utf8');
  const reference=JSON.parse(fs.readFileSync(path.join(__dirname,'original-behavior.json'),'utf8'));
  for(const scenario of ['autonomous','throws']){
    const actual=trace(html,scenario==='throws');
    assert.deepEqual(actual,reference[scenario],scenario+' must preserve original simulation');
    console.log('PASS original trajectory:',scenario,actual.states.join(', '),'gym',actual.gymWaypoints.join(','));
  }
  for(const width of [320,390,768,1280,1440,1920])for(const height of [200,844,1080]){
    const sim=makeSimulation(html);Object.assign(sim.rectangle,{left:17,top:31,width,height});
    for(let i=0;i<20;i++){
      const x=i/19,y=(i*7%20)/19;sim.click(x,y);
      const ball=sim.world.balls.at(-1);
      assert(Math.abs(ball.x-Math.max(6,Math.min(250,x*256)))<1e-10);
      assert(Math.abs(ball.y-Math.max(106,Math.min(141,y*144)))<1e-10);
      assert(sim.world.balls.length<=7);
    }
  }
  console.log('PASS pointer coordinates and seven-ball limit: 360 generated viewport/input cases');
  const sim=makeSimulation(html);sim.key('ArrowRight');sim.key('Enter');
  assert.equal(sim.world.balls.at(-1).x,134);
  sim.key(' ');const time=sim.world.getTime();sim.frame(100);assert.equal(sim.world.getTime(),time);
  sim.key(' ');sim.frame(116);assert(sim.world.getTime()>time);
  sim.media(true);const paused=sim.world.getTime();sim.frame(132);assert.equal(sim.world.getTime(),paused);
  console.log('PASS keyboard aim, throw, pause/resume and reduced motion');
}
if(require.main===module)run();
module.exports={makeSimulation,trace};
