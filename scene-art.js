/* Presentation only: coordinates, decisions and randomness belong to the simulation. */
(()=>{
'use strict';
window.createParkArt=function(data){
  const images=new Map();
  function preload(key,url){
    if(typeof url!=='string'||!url)return;
    const image=new Image(),entry={image,ready:false};
    images.set(key,entry);
    image.onload=()=>{entry.ready=image.naturalWidth>0&&image.naturalHeight>0;window.parkArtChanged?.()};
    image.onerror=()=>{entry.ready=false;window.parkArtChanged?.()};
    image.src=url;
  }
  if(data){
    preload('background',data.background);
    Object.entries(data.sheets||{}).forEach(([key,url])=>preload(key,url));
  }
  const walking=window.PARK_WALK;
  if(walking)walking.coats.forEach((coat,i)=>preload('walk-'+i,coat.url));
  // Logical body envelopes are independent of the pixel density of each sheet.
  // Optional anchorX/Y can register a hand-reviewed crop to its rear paws.
  const envelopes={standing:[23,17],walkA:[23,17],walkB:[23,17],sit:[23,18],sleep:[23,11],belly:[23,13],groomA:[23,18],groomB:[23,18],stretch:[25,17],scratch:[23,21],scratchA:[23,21],scratchB:[23,21],crouch:[23,13],jumpRise:[24,17],jumpTuck:[23,14],jumpFall:[24,17],landing:[23,13]};
  const fallbacks={crouch:['stretch','standing'],landing:['crouch','stretch'],jumpRise:['walkB','standing'],jumpTuck:['belly','walkB'],jumpFall:['walkA','standing'],scratchA:['scratch'],scratchB:['scratch']};
  const sourceFacing={sit:-1,sleep:-1,belly:-1,groomA:-1,groomB:-1};
  const sizes=[1,0.96,1.04,0.98,1.05,0.95,1.02,1];
  function pose(actor,name){
    if(/^walk[0-7]$/.test(name)){
      const frame=Number(name.slice(4)),index=actor.artIndex%8;
      const coat=walking&&walking.coats[index],entry=images.get('walk-'+index),rect=coat&&coat.frames[frame];
      if(entry?.ready&&rect&&Number.isFinite(coat.scale)&&coat.scale>0&&
         [rect.x,rect.y,rect.w,rect.h,rect.anchorX,rect.anchorY].every(Number.isFinite)&&
         rect.x>=0&&rect.y>=0&&rect.w>0&&rect.h>0&&rect.x+rect.w<=entry.image.naturalWidth&&rect.y+rect.h<=entry.image.naturalHeight){
        return {image:entry.image,rect,width:rect.w*coat.scale,height:rect.h*coat.scale,
          facing:1,anchorX:rect.anchorX,anchorY:rect.anchorY};
      }
      name=frame%2?'walkB':'walkA';
    }
    const coats=data&&data.coats;
    if(!Array.isArray(coats)||!coats.length)return null;
    const coat=coats[actor.artIndex%coats.length];
    let actual=name;
    if(coat&&coat.poses&&!coat.poses[actual])actual=(fallbacks[name]||[]).find(key=>coat.poses[key])||name;
    const rect=coat&&coat.poses&&coat.poses[actual],entry=rect&&images.get(rect.sheet);
    if(!entry||!entry.ready)return null;
    const {x,y,w,h}=rect;
    if(![x,y,w,h].every(Number.isFinite)||x<0||y<0||w<=0||h<=0||
       x+w>entry.image.naturalWidth||y+h>entry.image.naturalHeight)return null;
    const size=sizes[actor.artIndex%sizes.length]||1;
    const [maxW,maxH]=envelopes[actual]||[23,18];
    let scale=Math.min(maxW/w,maxH/h)*size;
    if(/^scratch[AB]$/.test(actual)){
      const pair=['scratchA','scratchB'].map(key=>coat.poses[key]).filter(Boolean);
      scale=Math.min(maxW/Math.max(...pair.map(r=>r.w)),maxH/Math.max(...pair.map(r=>r.h)))*size;
    }
    return {image:entry.image,rect,width:w*scale,height:h*scale,
      facing:rect.facing||sourceFacing[actual]||1,
      anchorX:Number.isFinite(rect.anchorX)?rect.anchorX:.5,
      anchorY:Number.isFinite(rect.anchorY)?rect.anchorY:1};
  }
  return {
    background(g,width,height){
      const entry=images.get('background');
      if(!entry||!entry.ready)return false;
      g.drawImage(entry.image,0,0,width,height);return true;
    },
    pose,
    draw(g,p,x,feet,direction,deform){
      const {rect:r,width:w,height:h}=p;
      g.save();g.translate(x,feet);
      if(deform){g.rotate(deform.angle||0);g.scale(deform.sx||1,deform.sy||1);}
      g.scale(direction===p.facing?1:-1,1);
      const dx=-w*p.anchorX,dy=-h*p.anchorY;
      if(r.clip){
        g.beginPath();
        r.clip.forEach(([cx,cy],i)=>g[i?'lineTo':'moveTo'](dx+cx/r.w*w,dy+cy/r.h*h));
        g.closePath();g.clip();
      }
      g.drawImage(p.image,r.x,r.y,r.w,r.h,dx,dy,w,h);
      g.restore();
    }
  };
};
})();
