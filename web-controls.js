(()=>{
'use strict';
// Input arbitration is shared by mouse/touch and independent of scene clocks.
// A held or dragged touch consumes its synthetic click before canvas/flower input.
class SceneGesture {
  constructor({background,underwater,menu,exit,now=()=>performance.now(),schedule=(fn,ms)=>setTimeout(fn,ms),cancel=id=>clearTimeout(id)}){Object.assign(this,{background,underwater,menu,exit,now,schedule,cancel});this.press=null;this.timer=null;this.lastTap=null;this.suppressUntil=0;}
  stopTimer(){if(this.timer!==null)this.cancel(this.timer);this.timer=null;}
  clearSuppression(){this.suppressUntil=0;}
  start(x,y,id){this.stopTimer();this.clearSuppression();this.press={x,y,id,background:this.background(x,y),held:false,moved:false};this.timer=this.schedule(()=>this.hold(),550);}
  hold(){this.stopTimer();const p=this.press;if(!p||p.moved||p.held)return;p.held=true;this.lastTap=null;this.suppressUntil=Infinity;if(p.background&&this.background(p.x,p.y))this.menu(p.x,p.y);}
  move(x,y,id){const p=this.press;if(p&&p.id===id&&Math.hypot(x-p.x,y-p.y)>10){p.moved=true;this.stopTimer();this.lastTap=null;this.suppressUntil=Infinity;}}
  end(id,canceled=false){const p=this.press;if(!p||p.id!==id)return;this.stopTimer();if(p.held||p.moved||canceled)this.suppressUntil=this.now()+800;this.press=null;}
  reset(){this.stopTimer();this.press=null;this.lastTap=null;this.suppressUntil=0;}
  tap(x,y,keyboard=false){
    if(keyboard)return false;
    const now=this.now();if(now<this.suppressUntil){this.suppressUntil=0;this.lastTap=null;return true;}
    if(!this.underwater()||!this.background(x,y)){this.lastTap=null;return false;}
    const previous=this.lastTap;this.lastTap={x,y,time:now};
    if(previous&&now-previous.time<=350&&Math.hypot(x-previous.x,y-previous.y)<=24){this.lastTap=null;this.exit();return true;}
    return false;
  }
}
window.SceneGesture=SceneGesture;
window.createWebControls=function(api,initialMessage){
  const el=id=>document.getElementById(id),dialog=el('park-controls'),form=el('park-settings'),menu=el('scene-menu');
  const fields={cats:el('cat-count'),fps:el('frame-rate'),companionMinutes:el('companion-minutes'),stayMinutes:el('stay-minutes')};
  let previousPhase=null;
  const items=()=>['toggle-pause','toggle-hide','open-controls','exit-underwater'].map(el).filter(item=>!item.hidden&&!item.disabled);
  const focusScene=()=>api.focusScene();
  function closeMenu(restore=true){if(menu.open)menu.close();if(restore)focusScene();}
  function openMenu(x,y){
    if(dialog.open)return;
    refresh();if(!menu.open)menu.showModal();
    const size=menu.getBoundingClientRect?.()||{width:240,height:210},width=window.innerWidth||1280,height=window.innerHeight||720;
    const left=Math.max(8,Math.min(width-size.width-8,x)),top=Math.max(8,Math.min(height-size.height-8,y));
    menu.setAttribute('style',`left:${left}px;top:${top}px`);items()[0]?.focus();
  }
  const gesture=new SceneGesture({background:(x,y)=>api.isBackground(x,y),underwater:()=>api.state().underwater&&!api.state().hidden,menu:openMenu,exit:()=>api.exitSecret()});
  const feedback=(message,error=false)=>{el('settings-message').textContent=message;el('settings-message').classList.toggle('save-error',error);};
  const populate=()=>{const s=api.state().settings;for(const key in fields)fields[key].value=String(s[key]);};
  const draft=()=>Object.fromEntries(Object.entries(fields).map(([key,input])=>[key,input.value.trim()===''?NaN:Number(input.value)]));
  const dirty=()=>{const d=draft(),s=api.state().settings;el('settings-dirty').textContent=Object.keys(d).some(k=>d[k]!==s[k])?'Cambios sin guardar':'Preferencias aplicadas en esta sesión';};
  function refresh(){
    const s=api.state(),expanded=s.phase!=='companion',frozen=s.paused||s.hidden||s.background;
    el('toggle-pause').textContent=s.paused?'Reanudar':'Pausar';el('toggle-hide').textContent=s.hidden?'Mostrar':'Ocultar';
    el('exit-underwater').hidden=!s.underwater;
    el('panel-pause').textContent=s.paused?'Reanudar':'Pausar';el('panel-hide').textContent=s.hidden?'Mostrar':'Ocultar';
    el('activity-status').textContent=s.settings.cats+' gato'+(s.settings.cats===1?'':'s')+' · '+(expanded?'Parque':'Compañía en esta pestaña')+' · '+(s.hidden?(s.paused?'ocultos y en pausa':'ocultos'):s.paused?'en pausa':'activos');
    el('schedule-status').textContent=s.status+(frozen?' · reloj detenido':'');
    el('return-companion').disabled=!expanded;
    el('visit-now').textContent=s.revisit?'Visita preparada':'Ir al parque ahora';
    if(expanded&&Number(fields.cats.value)!==s.settings.cats){fields.cats.value=String(s.settings.cats);feedback('La cantidad pendiente no se aplicó: comenzó la visita. Podés elegirla al volver a la compañía.');}
    fields.cats.disabled=expanded||s.fixedCount;
    el('count-help').textContent=s.fixedCount?'Esta vista muestra cuatro gatos naranjas. La cantidad se cambia en el parque habitual.':expanded?'La cantidad se cambia durante la compañía. Volvé a la franja para elegir 1, 4 u 8 gatos.':'La cantidad cambia al guardar. Si conservás la misma, se mantienen los gatos y sus posiciones.';
    if(previousPhase!==s.phase){el('park-status').textContent=s.status;previousPhase=s.phase;}
    dirty();
  }
  el('open-controls').addEventListener('click',()=>{closeMenu(false);refresh();dialog.showModal();});
  el('close-controls').addEventListener('click',()=>dialog.close());
  dialog.addEventListener('close',focusScene);
  el('toggle-pause').addEventListener('click',()=>{api.togglePause();closeMenu();});
  el('toggle-hide').addEventListener('click',()=>{api.toggleHide();closeMenu();});
  el('exit-underwater').addEventListener('click',()=>{api.exitSecret();closeMenu();});
  el('panel-pause').addEventListener('click',()=>api.togglePause());
  el('panel-hide').addEventListener('click',()=>api.toggleHide());
  el('visit-now').addEventListener('click',()=>api.visit());
  el('return-companion').addEventListener('click',()=>api.back());
  menu.addEventListener('cancel',event=>{event.preventDefault();closeMenu();});
  menu.addEventListener('click',event=>{if(event.target===menu){const r=menu.getBoundingClientRect();if(event.clientX<r.left||event.clientX>r.right||event.clientY<r.top||event.clientY>r.bottom){event.preventDefault();closeMenu();}}});
  menu.addEventListener('keydown',event=>{
    const list=items(),index=list.indexOf(document.activeElement);let next=null;
    if(event.key==='ArrowDown')next=(index+1)%list.length;
    if(event.key==='ArrowUp')next=(index+list.length-1)%list.length;
    if(event.key==='Home')next=0;if(event.key==='End')next=list.length-1;
    if(next!==null){event.preventDefault();list[next].focus();}
    if(event.key==='Escape'||event.key==='Tab'){event.preventDefault();event.stopPropagation();closeMenu();}
  });
  const inDialog=event=>!!event.target?.closest?.('dialog');
  document.addEventListener('contextmenu',event=>{
    if(menu.open){event.preventDefault();return;}
    if(inDialog(event)||dialog.open)return;
    event.preventDefault();gesture.lastTap=null;
    if(gesture.press){gesture.hold();return;}
    if(api.isBackground(event.clientX,event.clientY))openMenu(event.clientX,event.clientY);
  });
  document.addEventListener('pointerdown',event=>{
    gesture.clearSuppression();
    if(dialog.open||menu.open||inDialog(event))return;
    if(event.pointerType==='touch'||event.pointerType==='pen'){
      if(event.isPrimary===false){gesture.end(gesture.press?.id,true);return;}
      gesture.start(event.clientX,event.clientY,event.pointerId);
    }
  },true);
  document.addEventListener('pointermove',event=>gesture.move(event.clientX,event.clientY,event.pointerId),true);
  document.addEventListener('pointerup',event=>gesture.end(event.pointerId),true);
  document.addEventListener('pointercancel',event=>gesture.end(event.pointerId,true),true);
  document.addEventListener('click',event=>{
    // Suppression also covers a synthetic click retargeted to the newly opened menu.
    if(performance.now()<gesture.suppressUntil){gesture.tap(event.clientX,event.clientY,event.detail===0);if(event.detail!==0){event.preventDefault();event.stopImmediatePropagation();}return;}
    if(dialog.open||menu.open||inDialog(event))return;
    if(gesture.tap(event.clientX,event.clientY,event.detail===0)){event.preventDefault();event.stopImmediatePropagation();}
  },true);
  document.addEventListener('keydown',event=>{
    if(dialog.open||menu.open||event.defaultPrevented)return;
    if(event.key==='ContextMenu'||event.key==='F10'&&event.shiftKey){event.preventDefault();const r=el('pk').getBoundingClientRect();openMenu(r.left+r.width/2,r.top+r.height/2);}
    else if(event.key==='Escape'&&api.state().underwater){event.preventDefault();api.exitSecret();}
    else if(event.key===' '&&api.state().hidden&&!event.repeat){event.preventDefault();api.togglePause();}
  });
  document.addEventListener('visibilitychange',()=>{gesture.reset();if(document.hidden&&menu.open)closeMenu(false);});
  form.addEventListener('input',()=>{for(const name of ['companion','stay']){el(name+'-error').textContent='';el(name+'-minutes').removeAttribute('aria-invalid');}feedback('');dirty();});
  form.addEventListener('submit',event=>{
    event.preventDefault();const d=draft();let first=null;
    for(const [key,name,max] of [['companionMinutes','companion',240],['stayMinutes','stay',60]]){
      const ok=Number.isInteger(d[key])&&d[key]>=1&&d[key]<=max;el(name+'-error').textContent=ok?'':'Ingresá minutos enteros entre 1 y '+max+'.';
      fields[key].setAttribute('aria-invalid',String(!ok));if(!ok&&!first)first=fields[key];
    }
    if(first){feedback('Revisá los campos indicados. Las preferencias aplicadas siguen iguales.',true);first.focus();first.closest('label').scrollIntoView({block:'nearest'});return;}
    if(api.state().phase!=='companion')d.cats=api.state().settings.cats;
    const error=api.save(d);if(error){feedback(error,true);return;}
    refresh();feedback('Preferencias guardadas. Se conserva el tiempo transcurrido del ciclo actual.');
  });
  populate();refresh();if(initialMessage)feedback(initialMessage,true);
  return {refresh};
};
})();
