(()=>{
'use strict';
const profile=window.PARK_BEHAVIOR;
function init(cat,seed){cat.behaviorSeed=(seed%2147483646)+1;cat.energy=.5;cat.recent='';cat.cooldown=0;cat.socialCooldown=0;}
function random(cat){cat.behaviorSeed=cat.behaviorSeed*16807%2147483647;return (cat.behaviorSeed-1)/2147483646;}
function duration(cat,action){const [min,max]=profile.durations[action];return min+random(cat)*(max-min);}
function canSleep(group,participants){
 const others=group.filter(c=>!participants.includes(c));
 return (others.length===0&&participants.length===1)||others.some(c=>c.st!=='sleep'&&c.pair?.action!=='cuddle');
}
function groupFactor(action,quietFraction,energy){
 const rule=profile.groupBalance;
 const pressure=Math.max(0,Math.min(1,(quietFraction-rule.quietThreshold)/(1-rule.quietThreshold)))*(rule.minimumEnergyInfluence+(1-rule.minimumEnergyInfluence)*Math.max(0,Math.min(1,energy)));
 return 1+((rule.multipliers[action]??1)-1)*pressure;
}
function select(cat,scene,eligible,others=[]){
  const weights={...(scene==='companion'?profile.weightsCompanion:profile.weightsPark)};
  weights.sleep*=1.5-cat.energy;weights.wander*=.5+cat.energy;
  if(cat.recent in weights)weights[cat.recent]*=.15;
 const quiet=others.length?others.filter(o=>!o.restTravel&&(['sleep','sit','settle'].includes(o.st)||(o.pair?.action==='cuddle'&&o.pair.phase==='contact'))).length/others.length:0;
 for(const name of Object.keys(weights))weights[name]*=groupFactor(name,quiet,cat.energy);
  for(const name of Object.keys(weights))if(!eligible.includes(name))delete weights[name];
  const total=Object.values(weights).reduce((a,b)=>a+b,0);let n=random(cat)*total;
  for(const [name,weight] of Object.entries(weights)){n-=weight;if(n<0)return name;}
  return 'sit';
}
function tick(cat,dt){cat.cooldown=Math.max(0,cat.cooldown-dt);cat.socialCooldown=Math.max(0,cat.socialCooldown-dt);cat.energy=Math.max(0,Math.min(1,cat.energy+(cat.st==='sleep'?.008:cat.st==='sit'||cat.st==='groom'?.003:cat.mv?-.006:0)*dt));}
window.CatBehavior={init,random,duration,select,tick,groupFactor,canSleep};
})();
