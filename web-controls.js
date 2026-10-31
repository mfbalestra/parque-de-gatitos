(()=>{
'use strict';
window.createWebControls=function(api,initialMessage){
  const el=id=>document.getElementById(id),dialog=el('park-controls'),form=el('park-settings');
  const fields={cats:el('cat-count'),fps:el('frame-rate'),companionMinutes:el('companion-minutes'),stayMinutes:el('stay-minutes')};
  let previousPhase=null;
  const feedback=(message,error=false)=>{el('settings-message').textContent=message;el('settings-message').classList.toggle('save-error',error);};
  const populate=()=>{const s=api.state().settings;for(const key in fields)fields[key].value=String(s[key]);};
  const draft=()=>Object.fromEntries(Object.entries(fields).map(([key,input])=>[key,input.value.trim()===''?NaN:Number(input.value)]));
  const dirty=()=>{const d=draft(),s=api.state().settings;el('settings-dirty').textContent=Object.keys(d).some(k=>d[k]!==s[k])?'Cambios sin guardar':'Preferencias aplicadas en esta sesión';};
  function refresh(){
    const s=api.state(),expanded=s.phase!=='companion',frozen=s.paused||s.hidden||s.background;
    el('toggle-pause').textContent=s.paused?'Reanudar':'Pausar';el('toggle-hide').textContent=s.hidden?'Mostrar':'Ocultar';
    el('panel-pause').textContent=s.paused?'Reanudar':'Pausar';el('panel-hide').textContent=s.hidden?'Mostrar':'Ocultar';
    el('activity-status').textContent=s.settings.cats+' gato'+(s.settings.cats===1?'':'s')+' · '+(expanded?'Parque':'Compañía en esta pestaña')+' · '+(s.hidden?(s.paused?'ocultos y en pausa':'ocultos'):s.paused?'en pausa':'activos');
    el('schedule-status').textContent=s.status+(frozen?' · reloj detenido':'');
    el('return-companion').disabled=!expanded;
    el('visit-now').textContent=s.revisit?'Visita preparada':'Ir al parque ahora';
    if(expanded&&Number(fields.cats.value)!==s.settings.cats){fields.cats.value=String(s.settings.cats);feedback('La cantidad pendiente no se aplicó: comenzó la visita. Podés elegirla al volver a la compañía.');}
    fields.cats.disabled=expanded;
    el('count-help').textContent=expanded?'La cantidad se cambia durante la compañía. Volvé a la franja para elegir 1, 4 u 8 gatos.':'La cantidad cambia al guardar. Si conservás la misma, se mantienen los gatos y sus posiciones.';
    if(previousPhase!==s.phase){el('park-status').textContent=s.status;previousPhase=s.phase;}
    dirty();
  }
  el('open-controls').addEventListener('click',()=>{refresh();dialog.showModal();});
  el('close-controls').addEventListener('click',()=>dialog.close());
  dialog.addEventListener('close',()=>el('open-controls').focus());
  el('toggle-pause').addEventListener('click',()=>api.togglePause());
  el('toggle-hide').addEventListener('click',()=>api.toggleHide());
  el('panel-pause').addEventListener('click',()=>api.togglePause());
  el('panel-hide').addEventListener('click',()=>api.toggleHide());
  el('visit-now').addEventListener('click',()=>api.visit());
  el('return-companion').addEventListener('click',()=>api.back());
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
