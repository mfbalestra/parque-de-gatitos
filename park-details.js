/* Scenic drawing only. Coordinates stay in the park's 256 × 144 world. */
(() => {
  'use strict';
  const TAU = Math.PI * 2;
  const palette = {
    wood: '#b98551', edge: '#795b3c', light: '#edc991', rope: '#ddc79b',
    cream: '#f2dfb9', sage: '#9dac80', terra: '#c78065', lavender: '#afa1bd'
  };

  function rounded(g, x, y, w, h, r) {
    r = Math.min(r, w / 2, h / 2);
    g.beginPath();
    g.moveTo(x + r, y); g.lineTo(x + w - r, y);
    g.quadraticCurveTo(x + w, y, x + w, y + r);
    g.lineTo(x + w, y + h - r);
    g.quadraticCurveTo(x + w, y + h, x + w - r, y + h);
    g.lineTo(x + r, y + h);
    g.quadraticCurveTo(x, y + h, x, y + h - r);
    g.lineTo(x, y + r); g.quadraticCurveTo(x, y, x + r, y);
    g.closePath();
  }

  function line(g, points, colour, width) {
    g.beginPath(); g.moveTo(points[0][0], points[0][1]);
    for (let i = 1; i < points.length; i++) g.lineTo(points[i][0], points[i][1]);
    g.strokeStyle = colour; g.lineWidth = width; g.stroke();
  }

  function ellipse(g, x, y, rx, ry, colour) {
    g.beginPath(); g.ellipse(x, y, rx, ry, 0, 0, TAU);
    g.fillStyle = colour; g.fill();
  }

  // width/height are full diameters, including the entirely transparent edge.
  function shadow(g, x, y, width, height, opacity = .25) {
    if (![x, y, width, height, opacity].every(Number.isFinite) || width <= 0 || height <= 0) return;
    opacity = Math.max(0, Math.min(1, opacity));
    g.save();
    g.translate(x, y); g.scale(width / 2, height / 2);
    const fade = g.createRadialGradient(0, 0, 0, 0, 0, 1);
    fade.addColorStop(0, `rgba(45,61,34,${opacity})`);
    fade.addColorStop(.38, `rgba(45,61,34,${opacity * .7})`);
    fade.addColorStop(.72, `rgba(45,61,34,${opacity * .24})`);
    fade.addColorStop(1, 'rgba(45,61,34,0)');
    ellipse(g, 0, 0, 1, 1, fade);
    g.restore();
  }

  function blanket(g, x, y, width, height, angle, colour, pattern) {
    shadow(g, x, y + .55, width + 3, height + 2, .15);
    g.save(); g.translate(x, y); g.rotate(angle);
    const left = -width / 2, top = -height / 2;
    rounded(g, left, top + .45, width, height, 1.45);
    g.fillStyle = '#8e8063'; g.fill();
    rounded(g, left, top, width, height, 1.45);
    g.fillStyle = colour; g.fill();
    g.save(); g.clip();
    // Uneven weaving lines remain delicate at the six-times drawing resolution.
    for (let row = 0; row < height * 2; row++) {
      const yy = top + row * .5;
      line(g, [[left, yy], [0, yy + .1], [-left, yy]], 'rgba(255,245,213,.14)', .12);
    }
    for (let i = -width / 2 + 2.6; i < width / 2; i += 5) {
      line(g, [[i, top], [i + .6, -top]], 'rgba(253,239,205,.65)', pattern ? .8 : .48);
      if (pattern) line(g, [[i + 1, top], [i + 1.6, -top]], 'rgba(109,93,73,.18)', .2);
    }
    for (let yy = top + 1.5; yy < -top; yy += 2.6) {
      line(g, [[left, yy], [-left, yy + .2]], 'rgba(253,239,205,.62)', .48);
    }
    g.restore();
    rounded(g, left + .65, top + .55, width - 1.3, height - 1.1, 1);
    g.strokeStyle = 'rgba(255,244,215,.7)'; g.lineWidth = .2;
    g.setLineDash([.65, .55]); g.stroke(); g.setLineDash([]);
    for (let side of [-1, 1]) {
      for (let j = 0; j < 7; j++) {
        const yy = top + .65 + j * (height - 1.3) / 6;
        const xx = side * (width / 2 - .1);
        line(g, [[xx, yy], [xx + side * .7, yy + .1], [xx + side * 1.1, yy + .35]], palette.cream, .22);
      }
    }
    // One relaxed turned corner gives the cloth thickness without hiding its pattern.
    g.beginPath(); g.moveTo(-left - 3.5, -top); g.quadraticCurveTo(-left - 1.4, -top - .8, -left, -top - 2);
    g.quadraticCurveTo(-left + .1, -top - .1, -left - 1.4, -top);
    g.closePath(); g.fillStyle = 'rgba(249,229,190,.7)'; g.fill();
    g.restore();
  }

  function drawGround(g, t) {
    g.save(); g.lineCap = 'round'; g.lineJoin = 'round';
    blanket(g, 27, 120, 28, 8.5, -.055, palette.terra, 1);
    blanket(g, 96, 137, 30, 8, .035, palette.lavender, 0);
    blanket(g, 211, 132, 29, 9, -.04, palette.sage, 1);
    g.restore();
  }

  function timber(g, x, y, w, h) {
    const wood = g.createLinearGradient(x, y, x + w, y);
    wood.addColorStop(0, palette.light); wood.addColorStop(.38, '#c89d65'); wood.addColorStop(1, palette.wood);
    rounded(g, x, y, w, h, .7); g.fillStyle = wood; g.fill();
    g.strokeStyle = palette.edge; g.lineWidth = .3; g.stroke();
    line(g, [[x + w * .7, y + 1], [x + w * .64, y + h * .55], [x + w * .72, y + h - .8]], 'rgba(114,77,44,.25)', .22);
  }

  function ropePost(g, x, top, bottom) {
    timber(g, x, top, 4, bottom - top);
    rounded(g, x + .12, top + 1.2, 3.75, bottom - top - 2.2, .6);
    g.fillStyle = palette.rope; g.fill();
    g.save(); g.clip();
    for (let yy = top + 1.5; yy < bottom - .5; yy += .95) {
      g.beginPath(); g.moveTo(x, yy); g.quadraticCurveTo(x + 2, yy + .75, x + 4, yy + .1);
      g.strokeStyle = '#aa8e60'; g.lineWidth = .28; g.stroke();
      g.beginPath(); g.moveTo(x + .3, yy - .2); g.quadraticCurveTo(x + 1.5, yy + .08, x + 2.5, yy + .02);
      g.strokeStyle = '#f4e0b6'; g.lineWidth = .18; g.stroke();
    }
    g.restore();
  }

  // y is the existing route's exact walking surface. All thickness extends below it.
  function platform(g, x, y, width, colour) {
    rounded(g, x, y + .85, width, 2.45, .85);
    g.fillStyle = palette.wood; g.fill(); g.strokeStyle = palette.edge; g.lineWidth = .32; g.stroke();
    rounded(g, x, y, width, 2.25, .85);
    g.fillStyle = colour; g.fill();
    line(g, [[x + 1, y + .37], [x + width - 1, y + .37]], '#f7e7ca', .35);
    line(g, [[x + 1, y + 1.75], [x + width - 1, y + 1.75]], 'rgba(82,61,49,.2)', .25);
    g.setLineDash([.55, .65]);
    line(g, [[x + .9, y + 1.2], [x + width - .9, y + 1.2]], 'rgba(255,245,220,.55)', .18);
    g.setLineDash([]);
  }

  function drawGym(g, t = 0) {
    g.save(); g.lineCap = 'round'; g.lineJoin = 'round';
    shadow(g, 178, 118.6, 86, 5.6, .2);
    shadow(g, 220, 119, 43, 4.2, .15);
    rounded(g, 137, 117.35, 68, 1.8, .85);
    g.fillStyle = '#b79864'; g.fill();
    line(g, [[138, 117.6], [203, 117.6]], '#e4cc98', .35);

    // Braces and supports sit behind the preserved route and its padded ledges.
    timber(g, 164, 96, 2.8, 21.8);
    line(g, [[166, 113], [173, 98]], '#947246', 2.3);
    line(g, [[166, 112.5], [172.6, 98]], '#d2ac72', .75);
    ropePost(g, 172, 94, 118);
    ropePost(g, 190, 80, 118);
    line(g, [[194, 107], [200, 82]], '#987449', 2.5);
    line(g, [[193.7, 106], [199.6, 83]], '#cfa975', .65);

    // Slope stays exactly on z = 40 * (1 - (x - 200) / 37).
    for (const x of [214, 228]) {
      const y = 78 + (x - 200) * 40 / 37;
      timber(g, x, y + 3.3, 2, 118 - y - 3.3);
      ellipse(g, x + 1, 118.1, 2.1, .65, '#b79664');
    }
    line(g, [[200, 77.1], [237, 117.1]], '#997857', 1.8);
    line(g, [[200, 76.8], [237, 116.8]], '#e1bd8a', .7);
    g.beginPath(); g.moveTo(200, 78); g.lineTo(237, 118);
    g.quadraticCurveTo(239.4, 119.4, 241, 118.5);
    g.lineTo(241, 120.1); g.quadraticCurveTo(237.8, 121, 235.8, 119.5);
    g.lineTo(199.5, 81.5); g.closePath();
    const slide = g.createLinearGradient(198, 78, 238, 121);
    slide.addColorStop(0, '#e8bc78'); slide.addColorStop(.45, '#f6d79b'); slide.addColorStop(1, '#d5a260');
    g.fillStyle = slide; g.fill();
    line(g, [[200.2, 78.4], [237, 118.2], [240, 118.9]], '#fff0c6', .55);
    line(g, [[200, 81.1], [236, 119.6], [240.2, 120]], '#a97a4b', .75);
    // Low front rim suggests a shallow trough; it never projects above the cat's feet.
    line(g, [[200.4, 80.1], [236.9, 119.1], [240.5, 119.5]], '#c18c62', 1.2);
    line(g, [[200.4, 79.8], [236.9, 118.8], [240.5, 119.2]], '#edcba0', .4);

    timber(g, 140, 111.3, 12, 6.6);
    timber(g, 152, 103.2, 10, 14.7);
    // An inset arched cubby keeps the second step light and recognizably feline.
    g.beginPath(); g.moveTo(154.2, 117.7); g.lineTo(154.2, 112);
    g.bezierCurveTo(154.2, 108.1, 159.8, 108.1, 159.8, 112);
    g.lineTo(159.8, 117.7); g.closePath();
    g.fillStyle = '#786147'; g.fill(); g.strokeStyle = '#d9b582'; g.lineWidth = .45; g.stroke();
    ellipse(g, 157, 117, 2.3, .65, '#b79972');
    platform(g, 140, 110, 12, palette.lavender);
    platform(g, 152, 102, 10, palette.sage);
    platform(g, 160, 94, 27, palette.terra);
    platform(g, 176, 78, 25, palette.lavender);

    const swing = Math.sin(t * 2.2) * .5;
    const ex = 185 + Math.sin(swing) * 8, ey = 81 + Math.cos(swing) * 8;
    g.beginPath(); g.moveTo(185, 81); g.quadraticCurveTo(185 + Math.sin(swing) * 3, 85.4, ex, ey);
    g.strokeStyle = '#a18359'; g.lineWidth = .3; g.stroke();
    ellipse(g, ex, ey + 1, 1.75, 1.9, '#bc7e79');
    ellipse(g, ex - .45, ey + .45, .65, .65, '#eed0b3');
    line(g, [[ex - 1.2, ey + 1.1], [ex + .7, ey + 2]], '#dca99a', .2);
    g.restore();
  }

  function drawButterfly(g, f, t) {
    g.save(); g.translate(f.x, f.y);
    g.rotate(Math.sin(f.a || 0) * .22);
    g.lineCap = 'round'; g.lineJoin = 'round';
    const phase = Number.isFinite(f.t) ? f.t : (t || 0);
    const flutter = .27 + .73 * (.5 + .5 * Math.sin(phase * 17));
    for (const side of [-1, 1]) {
      g.save(); g.scale(side * flutter, 1);
      g.beginPath(); g.moveTo(.08, -.15);
      g.bezierCurveTo(.4, -1.3, 1.75, -1.8, 2, -1.02);
      g.bezierCurveTo(2.25, -.33, 1.55, .05, 1.04, .2);
      g.bezierCurveTo(1.9, .53, 1.5, 1.37, .87, 1.18);
      g.bezierCurveTo(.33, 1.03, .15, .52, .08, -.15);
      g.closePath(); g.fillStyle = f.c || palette.lavender; g.fill();
      g.strokeStyle = 'rgba(87,66,57,.65)'; g.lineWidth = .16; g.stroke();
      ellipse(g, 1.25, -.85, .34, .26, 'rgba(255,246,216,.85)');
      ellipse(g, .93, .65, .22, .18, 'rgba(104,74,66,.38)');
      line(g, [[.18, -.14], [1.23, -.5]], 'rgba(96,66,53,.32)', .1);
      g.restore();
    }
    ellipse(g, 0, .06, .16, .83, '#624f44');
    ellipse(g, 0, -.66, .22, .22, '#735647');
    for (const side of [-1, 1]) {
      g.beginPath(); g.moveTo(side * .08, -.7);
      g.quadraticCurveTo(side * .2, -1.22, side * .47, -1.18);
      g.strokeStyle = '#735647'; g.lineWidth = .11; g.stroke();
    }
    g.restore();
  }

  window.PARK_DETAILS = Object.freeze({ drawGround, drawGym, drawButterfly, shadow });
})();
