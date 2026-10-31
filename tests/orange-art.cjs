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
console.log('PASS historical orange sheet: 16 whole-cat frames, bounds and alpha format');
