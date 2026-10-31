/* Static browser cycle. Actor movement belongs to the scene adapter, not the UI. */
(()=>{
'use strict';
const DEFAULTS=Object.freeze({cats:8,fps:60,companionMinutes:30,stayMinutes:5});
const KEY='parque-gatitos.preferences.v1';
function valid(s){return !!s&&[1,4,8].includes(s.cats)&&[30,60].includes(s.fps)&&Number.isInteger(s.companionMinutes)&&s.companionMinutes>=1&&s.companionMinutes<=240&&Number.isInteger(s.stayMinutes)&&s.stayMinutes>=1&&s.stayMinutes<=60;}
function load(storage){try{const raw=storage.getItem(KEY);if(raw===null)return {settings:{...DEFAULTS},message:''};const data=JSON.parse(raw);if(data.version!==1||!valid(data.settings))throw Error('invalid');return {settings:{...data.settings},message:''};}catch{return {settings:{...DEFAULTS},message:'No se pudieron leer las preferencias. Se usan 8 gatos, 60 FPS, 30 minutos de compañía y 5 de parque. Podés volver a guardarlas.'};}}
function save(storage,settings){if(!valid(settings))return 'Revisá las preferencias: usá cantidades admitidas y minutos enteros dentro de sus límites.';try{storage.setItem(KEY,JSON.stringify({version:1,settings}));return null;}catch{return 'No se pudo guardar en este navegador. Las preferencias aplicadas siguen iguales. Revisá el permiso de almacenamiento y volvé a intentar.';}}
// Keep the deadline anchored when callbacks wobble around a display refresh.
// At most one drawing per callback; interruptions discard missed drawings.
class FramePacer{
  constructor(target){this.configure(target);}
  configure(target){if(![30,60].includes(target))throw Error('Invalid frame rate');this.target=target;this.reset();}
  reset(){this.next=null;this.last=null;}
  shouldRender(now){
    if(!Number.isFinite(now))throw Error('Invalid timestamp');
    if(now===this.last)return false;
    if(this.next===null||now<this.last||now-this.last>50)this.next=now;
    this.last=now;
    if(now+1e-7<this.next)return false;
    const interval=1000/this.target;
    this.next+=interval;
    // Retain at most one interval of credit: a slightly slower compositor must
    // draw on every callback without periodically losing another whole frame.
    if(this.next<now-interval)this.next=now-interval;
    return true;
  }
}
class Cycle{
  constructor(settings,adapter){if(!valid(settings))throw Error('Invalid settings');this.settings={...settings};this.adapter=adapter;this.phase='visiting';this.elapsed=0;this.opacity=1;this.revisit=false;}
  configure(settings){if(!valid(settings))throw Error('Invalid settings');if(this.phase!=='companion'&&settings.cats!==this.settings.cats)throw Error('Count locked');this.settings={...settings};}
  setPhase(phase){this.phase=phase;this.elapsed=0;this.adapter.phaseChanged?.(phase);}
  visit(){if(this.phase==='leaving'||this.phase==='disappearing'){this.revisit=true;return;}if(this.phase!=='companion')return;this.adapter.prepareVisit();this.setPhase('appearing');}
  back(){this.revisit=false;if(['appearing','entering','visiting'].includes(this.phase)){this.adapter.prepareReturn();this.setPhase('leaving');}}
  advance(dt){
    if(!Number.isFinite(dt)||dt<0||dt>2)throw Error('Invalid timestep');
    this.elapsed+=dt;
    switch(this.phase){
      case 'companion':this.adapter.companion(dt);if(this.elapsed>=this.settings.companionMinutes*60)this.visit();break;
      case 'appearing':this.opacity=Math.min(1,this.opacity+dt/2);this.adapter.finishJumps(dt);if(this.opacity>=1)this.setPhase('entering');break;
      case 'entering':if(this.adapter.enter(dt))this.setPhase('visiting');break;
      case 'visiting':this.adapter.park(dt);if(this.elapsed>=this.settings.stayMinutes*60)this.back();break;
      case 'leaving':if(this.adapter.leave(dt))this.setPhase('disappearing');break;
      case 'disappearing':this.opacity=Math.max(0,this.opacity-dt/2);if(this.opacity<=0){this.adapter.resumeCompanion();this.setPhase('companion');if(this.revisit){this.revisit=false;this.visit();}}break;
    }
  }
  status(){const remaining=minutes=>{let n=Math.max(0,Math.ceil(minutes*60-this.elapsed));const h=Math.floor(n/3600);n%=3600;return (h?String(h).padStart(2,'0')+':':'')+String(Math.floor(n/60)).padStart(2,'0')+':'+String(n%60).padStart(2,'0');};return {companion:'Próxima visita en '+remaining(this.settings.companionMinutes),appearing:'El parque está apareciendo',entering:'Los gatos van al parque',visiting:'Compañía en esta pestaña en '+remaining(this.settings.stayMinutes),leaving:'Los gatos vuelven a la franja',disappearing:this.revisit?'El parque se despide · nueva visita preparada':'El parque se está despidiendo'}[this.phase];}
}
window.ParkCycle={DEFAULTS,KEY,valid,load,save,Cycle,FramePacer};
})();
