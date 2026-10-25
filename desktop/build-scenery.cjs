// Development export only; the native application never loads a browser.
const fs = require('node:fs');
const path = require('node:path');
const { createRequire } = require('node:module');
const root = path.resolve(__dirname, '..');
const moduleRoot = process.argv[2];
if (!moduleRoot) throw new Error('Pass the installed global node_modules directory.');
const { chromium } = createRequire(path.join(path.resolve(moduleRoot), '_resolver.cjs'))('playwright');
(async () => {
  const browser = await chromium.launch({ headless: true });
  try {
    const page = await browser.newPage({ viewport: { width: 1536, height: 864 } });
    await page.setContent('<canvas width="1536" height="864"></canvas>');
    await page.addScriptTag({ content: fs.readFileSync(path.join(root, 'park-details.js'), 'utf8') });
    const data = await page.evaluate(async src => {
      const image = new Image(); image.src = src; await image.decode();
      const canvas = document.querySelector('canvas'), context = canvas.getContext('2d');
      context.drawImage(image, 0, 0, 1536, 864); context.scale(6, 6);
      PARK_DETAILS.drawGround(context, 0); PARK_DETAILS.drawGym(context, 0);
      return canvas.toDataURL('image/png').split(',')[1];
    }, 'data:image/png;base64,' + fs.readFileSync(path.join(root, 'assets/parque.png')).toString('base64'));
    fs.mkdirSync(path.join(__dirname, 'assets'), { recursive: true });
    fs.writeFileSync(path.join(__dirname, 'assets/parque-windows.png'), Buffer.from(data, 'base64'));
    console.log('Exported existing park artwork and scenery at 1536 x 864.');
  } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
