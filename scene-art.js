/* Presentation only: coordinates, decisions and randomness belong to the simulation. */
(()=>{
'use strict';
window.createParkArt=function(data){
  const images=new Map();
  function preload(key,url){
    if(typeof url!=='string'||!url)return;
    const image=new Image(),entry={image,ready:false};
    images.set(key,entry);
    image.onload=()=>{entry.ready=image.naturalWidth>0&&image.naturalHeight>0};
    image.onerror=()=>{entry.ready=false};
    image.src=url;
  }
  if(data){
    preload('background',data.background);
    Object.entries(data.sheets||{}).forEach(([key,url])=>preload(key,url));
  }
  // Height gives each action a consistent body size even across different sheets.
  const heights={sit:18,sleep:11,belly:13,groomA:18,groomB:18,stretch:17,scratch:21};
  const sourceFacing={sit:-1,sleep:-1,belly:-1,groomA:-1,groomB:-1};
  const sizes=[1,0.96,1.04,0.98,1.05,0.95,1.02,1];
  function pose(actor,name){
    const coats=data&&data.coats;
    if(!Array.isArray(coats)||!coats.length)return null;
    const coat=coats[actor.artIndex%coats.length];
    const rect=coat&&coat.poses&&coat.poses[name],entry=rect&&images.get(rect.sheet);
    if(!entry||!entry.ready)return null;
    const {x,y,w,h}=rect;
    if(![x,y,w,h].every(Number.isFinite)||x<0||y<0||w<=0||h<=0||
       x+w>entry.image.naturalWidth||y+h>entry.image.naturalHeight)return null;
    const size=sizes[actor.artIndex%sizes.length]||1;
    const scale=(heights[name]?heights[name]/h:23/w)*size;
    return {image:entry.image,rect,width:w*scale,height:h*scale,
      facing:sourceFacing[name]||1};
  }
  return {
    background(g,width,height){
      const entry=images.get('background');
      if(!entry||!entry.ready)return false;
      g.drawImage(entry.image,0,0,width,height);return true;
    },
    pose,
    draw(g,p,x,feet,direction){
      const {rect:r,width:w,height:h}=p;
      g.save();g.translate(x,feet);
      g.scale(direction===p.facing?1:-1,1);
      if(r.clip){
        g.beginPath();
        r.clip.forEach(([cx,cy],i)=>g[i?'lineTo':'moveTo'](-w/2+cx/r.w*w,-h+cy/r.h*h));
        g.closePath();g.clip();
      }
      g.drawImage(p.image,r.x,r.y,r.w,r.h,-w/2,-h,w,h);
      g.restore();
    }
  };
};
})();
