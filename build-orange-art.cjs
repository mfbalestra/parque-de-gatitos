const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const {chromium} = require(path.join(process.argv[2], 'playwright'));

(async () => {
  const bytes = fs.readFileSync(path.join(__dirname, 'assets/acciones-naranja.png'));
  const browser = await chromium.launch({headless: true});
  try {
    const page = await browser.newPage();
    const data = await page.evaluate(async url => {
      const image = new Image(); image.src = url; await image.decode();
      const canvas = document.createElement('canvas'); canvas.width = image.width; canvas.height = image.height;
      const context = canvas.getContext('2d'); context.drawImage(image, 0, 0);
      const rgba = context.getImageData(0, 0, image.width, image.height).data;
      // Generated whiskers cross nominal grid lines. These gutters separate whole animals.
      const columns = [[0,403,791,1171,1536],[0,412,793,1170,1536],[0,405,795,1173,1536],[0,401,798,1169,1536]];
      const rows = [0,256,512,764,1024];
      const frames = {}; let largest = 0;
      for (let row = 0; row < 4; row++) for (let col = 0; col < 4; col++) {
        const x0 = columns[row][col], x1 = columns[row][col+1], y0 = rows[row], y1 = rows[row+1];
        let l=x1, t=y1, r=x0, b=y0;
        for (let y=y0; y<y1; y++) for (let x=x0; x<x1; x++) {
          if (rgba[(y*image.width+x)*4+3] >= 24) { l=Math.min(l,x); r=Math.max(r,x); t=Math.min(t,y); b=Math.max(b,y); }
        }
        if (l>=r) throw Error('Empty frame');
        l=Math.max(x0,l-2); t=Math.max(y0,t-2); r=Math.min(x1-1,r+2); b=Math.min(y1-1,b+2);
        const w=r-l+1, h=b-t+1; largest=Math.max(largest,w);
        frames[['knead','settle','rub','spar'][row]+col]={x:l,y:t,w,h,anchorX:(col*384+207.36-l)/w,anchorY:1};
      }
      if (rgba[3]!==0) throw Error('Opaque background');
      return {url:'assets/acciones-naranja.png',width:image.width,height:image.height,scale:23/largest,frames};
    }, 'data:image/png;base64,'+bytes.toString('base64'));
    fs.writeFileSync(path.join(__dirname,'orange-art-data.js'),'window.PARK_ORANGE = '+JSON.stringify(data,null,2)+';\n');
    console.log({frames:Object.keys(data.frames).length,sha256:crypto.createHash('sha256').update(bytes).digest('hex')});
  } finally { await browser.close(); }
})().catch(error=>{ console.error(error); process.exitCode=1; });
