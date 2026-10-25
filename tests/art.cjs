const fs=require('node:fs');
const path=require('node:path');
const vm=require('node:vm');
const assert=require('node:assert/strict');
const root=path.resolve(__dirname,'..');
const dimensions=new Map();
for(const name of fs.readdirSync(path.join(root,'assets')).filter(name=>name.endsWith('.png')&&!name.startsWith('referencia'))){
  const png=fs.readFileSync(path.join(root,'assets',name));
  assert.equal(png.subarray(1,4).toString(),'PNG');
  dimensions.set('assets/'+name,[png.readUInt32BE(16),png.readUInt32BE(20)]);
  if(name!=='parque.png')assert.equal(png[25],6,'cat PNG must have a real alpha channel');
}
function load(fail=false){
  class Image{
    set src(value){[this.naturalWidth,this.naturalHeight]=dimensions.get(value)||[0,0];if(fail)this.onerror();else this.onload();}
  }
  const context={window:{},Image};vm.createContext(context);
  for(const name of ['art-data.js','scene-art.js'])vm.runInContext(fs.readFileSync(path.join(root,name),'utf8'),context);
  return context.window;
}
const window=load(),data=window.PARK_ART,art=window.createParkArt(data);
assert.equal(data.coats.length,8,'eight distinct illustrated coats');
assert.equal(new Set(data.coats.map(coat=>coat.name)).size,8,'coat names must be unique');
let count=0,rectangles=0;
for(const coat of data.coats)for(const rect of Object.values(coat.poses)){
  rectangles++;
  const [width,height]=dimensions.get(data.sheets[rect.sheet]);
  assert(rect.x>=0&&rect.y>=0&&rect.x+rect.w<=width&&rect.y+rect.h<=height);
  for(const [x,y] of rect.clip||[])assert(x>=0&&y>=0&&x<=rect.w&&y<=rect.h);
}
for(let actor=0;actor<8;actor++)for(const name of Object.keys(data.coats[actor].poses)){
  const pose=art.pose({artIndex:actor},name);assert(pose);
  assert(pose.width>0&&pose.width<30&&pose.height>0&&pose.height<24);
  for(const direction of [-1,1])for(const x of [0,9,58,128,194,256]){
    const feet=118,stack=[];let tx=0,ty=0,sx=1,sy=1,corners;
    const g={save(){stack.push([tx,ty,sx,sy])},restore(){[tx,ty,sx,sy]=stack.pop()},translate(x,y){tx+=x*sx;ty+=y*sy},scale(x,y){sx*=x;sy*=y},beginPath(){},moveTo(){},lineTo(){},closePath(){},clip(){},drawImage(image,rx,ry,rw,rh,dx,dy,dw,dh){corners=[[tx+dx*sx,ty+dy*sy],[tx+(dx+dw)*sx,ty+(dy+dh)*sy]]}};
    art.draw(g,pose,x,feet,direction);
    const mirror=direction===pose.facing?1:-1;
    assert(Math.abs(corners[0][0]+pose.width*pose.anchorX*mirror-x)<1e-9,'mirroring must retain the declared body anchor');
    assert(Math.abs(corners[0][1]+pose.height*pose.anchorY-feet)<1e-9,'all poses must stay registered to the support anchor');
    assert.deepEqual([tx,ty,sx,sy],[0,0,1,1],'drawing must restore caller transform');count++;
  }
}
assert.equal(art.pose({artIndex:0},'missing'),null);
for(let actor=0;actor<8;actor++){
  for(const name of ['crouch','jumpRise','jumpTuck','jumpFall','landing','scratchA','scratchB'])assert(art.pose({artIndex:actor},name),'all motion poses or declared fallbacks load');
  const a=art.pose({artIndex:actor},'scratchA'),b=art.pose({artIndex:actor},'scratchB');
  assert(Math.abs(a.width/a.rect.w-b.width/b.rect.w)<1e-9,'scratch pair shares one source scale');
}
const legacy=structuredClone(data);delete legacy.coats[0].poses.jumpRise;
assert(window.createParkArt(legacy).pose({artIndex:0},'jumpRise'),'a missing supplemental pose uses a legacy pose');
const missing=load(true);assert.equal(missing.createParkArt(missing.PARK_ART).pose({artIndex:0},'sit'),null);
assert.equal(window.createParkArt(null).pose({artIndex:0},'sit'),null);
for(const value of [-100,-1,Infinity,NaN,100000]){
  const invalid=structuredClone(data);invalid.coats[0].poses.sit.x=value;
  assert.equal(window.createParkArt(invalid).pose({artIndex:0},'sit'),null,'invalid crop must use fallback');
}
console.log('PASS eight unique coats, PNG alpha, '+rectangles+' source rectangles, '+count+' anchor/mirror/transform cases, motion poses and missing assets/crops fallback');
