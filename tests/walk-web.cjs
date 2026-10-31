const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm');
const assert=require('node:assert/strict'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'..');
const provenance=JSON.parse(fs.readFileSync(path.join(root,'assets/walk-provenance.json')));
const dimensions=new Map();
for(const file of fs.readdirSync(path.join(root,'assets')).filter(f=>f.endsWith('.png'))){
  const png=fs.readFileSync(path.join(root,'assets',file));
  dimensions.set('assets/'+file,[png.readUInt32BE(16),png.readUInt32BE(20)]);
}
for(const coat of provenance.coats)for(const [file,expected] of [[coat.url,coat.sha256],[coat.source,coat.sourceSha256]]){
  assert.equal(crypto.createHash('sha256').update(fs.readFileSync(path.join(root,file))).digest('hex'),expected);
}
function load(failWalk=false){
  const context={window:{},Image:class{
    set src(value){[this.naturalWidth,this.naturalHeight]=dimensions.get(value)||[0,0];
      if(failWalk&&value.includes('/walk-'))this.onerror();else this.onload();}
  }};
  vm.createContext(context);
  for(const file of ['art-data.js','walk-data.js','scene-art.js'])vm.runInContext(fs.readFileSync(path.join(root,file),'utf8'),context);
  return context.window;
}
const window=load(),art=window.createParkArt(window.PARK_ART),walk=window.ParkWalk;
const nativeGait=fs.readFileSync(path.join(root,'desktop/WalkGait.cs'),'utf8');
const nativeSizes=nativeGait.match(/Sizes\s*=\s*\{([^}]+)\}/)[1].split(',').map(Number);
const nativeStride=Number(nativeGait.match(/StrideLength\(int coat\)\s*=>\s*([\d.]+)\s*\*/)[1]);
assert.equal(window.PARK_WALK.coats.length,8);
let checks=0;
for(let coat=0;coat<8;coat++){
  const data=window.PARK_WALK.coats[coat];assert.equal(data.frames.length,8);
  assert.equal(data.size,nativeSizes[coat],'native/web size correspondence');
  assert(Math.abs(data.stride-nativeStride*.23*nativeSizes[coat])<1e-10,'same stride after web scale adaptation');
  for(let frame=0;frame<8;frame++){
    const pose=art.pose({artIndex:coat},'walk'+frame),r=pose.rect;
    assert(pose&&pose.width>0&&pose.width<=23*walk.size(coat)+1e-9);
    assert(pose.height<=15.64*walk.size(coat)+1e-9);
    assert(Math.abs(pose.width/r.w-pose.height/r.h)<1e-12,'whole image must retain proportions');
    assert(Math.abs((r.x%224+r.w*pose.anchorX)-224*800/1536)<1e-8,'source anchor independent of crop');
    for(const direction of [-1,1]){
      const stack=[];let x=0,y=0,sx=1,sy=1,corners;
      const g={save(){stack.push([x,y,sx,sy])},restore(){[x,y,sx,sy]=stack.pop()},translate(a,b){x+=a*sx;y+=b*sy},scale(a,b){sx*=a;sy*=b},drawImage(image,rx,ry,rw,rh,dx,dy,w,h){corners=[x+dx*sx,y+dy*sy,x+(dx+w)*sx,y+(dy+h)*sy]}};
      art.draw(g,pose,128,118,direction);
      assert(Math.abs(corners[3]-118)<1e-9,'feet stay on support');
      assert.deepEqual([x,y,sx,sy],[0,0,1,1]);checks++;
    }
  }
  const seen=new Set();
  for(let i=0;i<2000;i++){
    const distance=i*.137,frame=walk.frame(distance,coat);seen.add(frame);
    assert(Number.isInteger(frame)&&frame>=0&&frame<8);
    assert.equal(walk.frame(distance+walk.stride(coat)*3,coat),frame,'periodic stride');
    assert.equal(walk.frame(distance,coat),frame,'rest never changes phase');checks+=3;
  }
  assert.equal(seen.size,8);
  const missing=load(true),fallback=missing.createParkArt(missing.PARK_ART);
  for(let frame=0;frame<8;frame++)assert(fallback.pose({artIndex:coat},'walk'+frame),'missing walking image uses existing full pose');
}
console.log('PASS eight coats / 64 whole poses, source hashes, scale, anchors, mirror, fallback and periodic distance phase:',checks,'checks');
