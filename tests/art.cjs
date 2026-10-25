const fs=require('node:fs');
const path=require('node:path');
const vm=require('node:vm');
const assert=require('node:assert/strict');
const root=path.resolve(__dirname,'..');
const dimensions=new Map();
for(const name of ['parque.png','gatos.png','gatos-acciones.png']){
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
let count=0;
for(const coat of data.coats)for(const rect of Object.values(coat.poses)){
  const [width,height]=dimensions.get(data.sheets[rect.sheet]);
  assert(rect.x>=0&&rect.y>=0&&rect.x+rect.w<=width&&rect.y+rect.h<=height);
  for(const [x,y] of rect.clip||[])assert(x>=0&&y>=0&&x<=rect.w&&y<=rect.h);
}
for(let actor=0;actor<8;actor++)for(const name of Object.keys(data.coats[actor%3].poses)){
  const pose=art.pose({artIndex:actor},name);assert(pose);
  assert(pose.width>0&&pose.width<30&&pose.height>0&&pose.height<24);
  for(const direction of [-1,1])for(const x of [0,9,58,128,194,256]){
    const feet=118,stack=[];let tx=0,ty=0,sx=1,sy=1,corners;
    const g={save(){stack.push([tx,ty,sx,sy])},restore(){[tx,ty,sx,sy]=stack.pop()},translate(x,y){tx+=x*sx;ty+=y*sy},scale(x,y){sx*=x;sy*=y},beginPath(){},moveTo(){},lineTo(){},closePath(){},clip(){},drawImage(image,rx,ry,rw,rh,dx,dy,dw,dh){corners=[[tx+dx*sx,ty+dy*sy],[tx+(dx+dw)*sx,ty+(dy+dh)*sy]]}};
    art.draw(g,pose,x,feet,direction);
    assert(Math.abs((corners[0][0]+corners[1][0])/2-x)<1e-9,'mirroring must retain horizontal registration');
    assert.equal(Math.max(corners[0][1],corners[1][1]),feet,'all poses must stay registered to feet');
    assert.deepEqual([tx,ty,sx,sy],[0,0,1,1],'drawing must restore caller transform');count++;
  }
}
assert.equal(art.pose({artIndex:0},'missing'),null);
const missing=load(true);assert.equal(missing.createParkArt(missing.PARK_ART).pose({artIndex:0},'sit'),null);
assert.equal(window.createParkArt(null).pose({artIndex:0},'sit'),null);
for(const value of [-100,-1,Infinity,NaN,100000]){
  const invalid=structuredClone(data);invalid.coats[0].poses.sit.x=value;
  assert.equal(window.createParkArt(invalid).pose({artIndex:0},'sit'),null,'invalid crop must use fallback');
}
console.log('PASS PNG alpha, 30 source rectangles, '+count+' anchor/mirror/transform cases, missing assets and invalid crop fallback');
