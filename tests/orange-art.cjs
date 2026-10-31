const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm'),assert=require('node:assert/strict');
const root=path.join(__dirname,'..'),context={window:{}};
vm.runInNewContext(fs.readFileSync(path.join(root,'orange-art-data.js'),'utf8'),context);
const data=context.window.PARK_ORANGE;
assert.equal(Object.keys(data.frames).length,16);
const bytes=fs.readFileSync(path.join(root,data.url));
assert.equal(bytes.readUInt32BE(16),data.width);assert.equal(bytes.readUInt32BE(20),data.height);
assert.equal(bytes[25],6,'RGBA source preserves alpha');
for(const action of ['knead','settle','rub','spar'])for(let i=0;i<4;i++){
 const r=data.frames[action+i];assert(r&&r.w>0&&r.h>0&&r.x>=0&&r.y>=0);
 assert(r.x+r.w<=data.width&&r.y+r.h<=data.height);assert(r.anchorX>0&&r.anchorX<1&&r.anchorY===1);
 assert(Number.isFinite(data.scale)&&data.scale>0);
}
const fakeImages=[];const runtime={window:{PARK_ORANGE:data},Image:class{constructor(){this.naturalWidth=data.width;this.naturalHeight=data.height;fakeImages.push(this)}set src(value){this.onload()}}};
vm.runInNewContext(fs.readFileSync(path.join(root,'scene-art.js'),'utf8'),runtime);
const art=runtime.window.createParkArt(null);
for(const name of Object.keys(data.frames)){
 assert(art.pose({artIndex:0},name));
 for(let coat=1;coat<8;coat++)assert.equal(art.pose({artIndex:coat},name),null,'New art stays orange-only');
}
assert.deepEqual(art.pose({artIndex:0},'sleep').rect,data.frames.settle3);
console.log('PASS 16 whole-cat frames, bounds, alpha format, orange-only gating and continuous sleep artwork');
