/* EmotiData · Familia de pingüinos 3D (three.js servido desde tu propio sitio, sin servicios externos).
   Un pingüino emperador y su cría siguen tu cursor (o tu último toque), caminan hacia la pestaña que eliges y,
   al llegar, dicen una frase breve en un globo. Opciones en Ajustes: seguir, quedarse, sin globos u ocultar.
   Respeta «reducir movimiento»: sin caminar ni saltos, solo el globo. */
(function () {
  "use strict";
  const BASE = document.currentScript ? document.currentScript.src.replace(/[^\/]*$/, "") : "";
  const lget = k => { try { return localStorage.getItem(k); } catch (e) { return null; } };
  const lset = (k, v) => { try { localStorage.setItem(k, v); } catch (e) { } };
  const clamp = (v, a, b) => Math.max(a, Math.min(b, v));
  const lerp = (a, b, k) => a + (b - a) * k;
  const reducido = () => !!(window.matchMedia && matchMedia("(prefers-reduced-motion: reduce)").matches);

  /* ================= FORMAS DE LOS GLOBOS (puras) ================= */
  const formas = {
    ovalo(W, H) { const rx = W / 2 - 1.5, ry = H / 2 - 1.5, cx = W / 2, cy = H / 2; return `M${cx - rx},${cy} A${rx},${ry} 0 1,0 ${cx + rx},${cy} A${rx},${ry} 0 1,0 ${cx - rx},${cy} Z`; },
    nube(W, H) { /* contorno de nube: arcos hacia afuera a lo largo del perímetro */
      const m = 9, w = W - 2 * m, h = H - 2 * m, nx = Math.max(3, Math.round(w / 34)), ny = Math.max(2, Math.round(h / 34));
      const rx = w / nx / 2 * 1.06, ry = h / ny / 2 * 1.06; let d = `M${m},${m}`;
      for (let i = 1; i <= nx; i++) d += ` A${rx},${rx} 0 0 1 ${m + i * w / nx},${m}`;
      for (let i = 1; i <= ny; i++) d += ` A${ry},${ry} 0 0 1 ${W - m},${m + i * h / ny}`;
      for (let i = 1; i <= nx; i++) d += ` A${rx},${rx} 0 0 1 ${W - m - i * w / nx},${H - m}`;
      for (let i = 1; i <= ny; i++) d += ` A${ry},${ry} 0 0 1 ${m},${H - m - i * h / ny}`;
      return d + " Z"; },
    pico(W, H) { /* contorno de picos para gritos y exclamaciones */
      const n = Math.max(14, Math.round((W + H) / 14)) * 2, cx = W / 2, cy = H / 2; let d = "";
      for (let i = 0; i < n; i++) { const a = i / n * Math.PI * 2, k = i % 2 ? .82 : 1; d += (i ? " L" : "M") + (cx + Math.cos(a) * (W / 2 - 2) * k).toFixed(1) + "," + (cy + Math.sin(a) * (H / 2 - 2) * k).toFixed(1); }
      return d + " Z"; },
    caja(W, H) { return `M2,2 H${W - 2} V${H - 2} H2 Z`; }
  };
  const TIPOS = ["dialogo", "pensamiento", "grito", "susurro", "narracion"];
  const COLA = { dialogo: 18, pensamiento: 20, grito: 20, susurro: 18, narracion: 0 };

  function svgGlobo(tipo, W, H, ax) {
    const T = COLA[tipo], st = 'fill="var(--bg)" stroke="var(--ink)" stroke-width="2.5" stroke-linejoin="round"';
    ax = clamp(ax, W * .28, W * .72); const ty = H + T - 2; let cuerpo, cola = "";
    if (tipo === "dialogo" || tipo === "susurro") {
      const dash = tipo === "susurro" ? ' stroke-dasharray="6 5"' : "";
      cuerpo = `<path d="${formas.ovalo(W, H)}" ${st}${dash}/>`;
      cola = `<path d="M${ax - 11},${H - 7} L${ax + 5},${ty} L${ax + 11},${H - 7}" ${st}${dash}/>`;
    } else if (tipo === "pensamiento") {
      cuerpo = `<path d="${formas.nube(W, H)}" ${st}/>`;
      cola = [[ax - 4, H + 3, 6], [ax, H + 10, 4.2], [ax + 3, H + 16, 2.8]].map(c => `<circle cx="${c[0]}" cy="${c[1]}" r="${c[2]}" ${st}/>`).join("");
    } else if (tipo === "grito") {
      const s = 'fill="var(--acc)" stroke="var(--ink)" stroke-width="2.5" stroke-linejoin="miter"';
      cuerpo = `<path d="${formas.pico(W, H)}" ${s}/>`;
      cola = `<path d="M${ax - 12},${H - 9} L${ax + 8},${ty + 2} L${ax + 8},${H - 9}" ${s}/>`;
    } else {
      cuerpo = `<path d="${formas.caja(W, H)}" ${st}/><rect x="2" y="2" width="8" height="${H - 4}" fill="var(--acc)" stroke="var(--ink)" stroke-width="2.5"/>`;
    }
    return `<svg xmlns="http://www.w3.org/2000/svg" width="${W}" height="${H + T}" viewBox="0 0 ${W} ${H + T}" aria-hidden="true">${cuerpo}${cola}</svg>`;
  }

  /* ================= MODELO 3D ================= */
  function pinguino(THREE, o) {
    const raiz = new THREE.Group(), cuerpo = new THREE.Group(); raiz.add(cuerpo);
    const mat = (c, r, ex) => new THREE.MeshStandardMaterial(Object.assign({ color: c, roughness: r === undefined ? .6 : r, metalness: 0 }, ex || {}));
    const gx = o.gordo || 1, ay = o.alto || 1;
    const perfil = new THREE.SplineCurve([[0, 0], [.34, .02], [.50, .22], [.56, .55], [.52, .95], [.42, 1.30], [.30, 1.55], [.22, 1.72]].map(p => new THREE.Vector2(p[0] * gx, p[1] * ay))).getPoints(40);
    cuerpo.add(new THREE.Mesh(new THREE.LatheGeometry(perfil, 44), mat(o.cuerpo, o.felpa || .55)));
    /* vientre: franja clara al frente, con degradé dorado hacia el cuello (emperador) */
    const pv = perfil.filter(p => p.y <= 1.5 * ay).map(p => new THREE.Vector2(p.x * 1.022, p.y));
    const gv = new THREE.LatheGeometry(pv, 44, -1.0, 2.0), pos = gv.attributes.position, col = new Float32Array(pos.count * 3);
    const c1 = new THREE.Color(o.vientre), c2 = new THREE.Color(o.oro || o.vientre), cc = new THREE.Color();
    for (let i = 0; i < pos.count; i++) { cc.copy(c1).lerp(c2, clamp((pos.getY(i) / ay - .92) / .55, 0, 1)); col.set([cc.r, cc.g, cc.b], i * 3); }
    gv.setAttribute("color", new THREE.BufferAttribute(col, 3));
    cuerpo.add(new THREE.Mesh(gv, mat(0xffffff, o.felpa || .7, { vertexColors: true, polygonOffset: true, polygonOffsetFactor: -2, polygonOffsetUnits: -2 })));
    /* cabeza */
    const cab = new THREE.Group(); cab.position.set(0, 1.70 * ay, .02); cab.scale.setScalar(o.cabezaEsc || 1); cuerpo.add(cab);
    const craneo = new THREE.Mesh(new THREE.SphereGeometry(.27, 32, 24), mat(o.cabeza, .45)); craneo.position.set(0, .2, .03); craneo.scale.set(1, 1.04, 1.12); cab.add(craneo);
    [-1, 1].forEach(s => {
      if (o.mejilla) { const m = new THREE.Mesh(new THREE.SphereGeometry(.12, 16, 12), mat(o.mejilla, .7)); m.scale.set(.35, 1, .95); m.position.set(s * .2, .12, .1); m.rotation.y = s * .45; cab.add(m); }
      if (o.mascara) { const m = new THREE.Mesh(new THREE.SphereGeometry(.13, 16, 12), mat(o.mascara, .9)); m.scale.set(.9, 1, .6); m.position.set(s * .12, .2, .2); cab.add(m); }
    });
    const ojos = [-1, 1].map(s => { const g = new THREE.Group(); g.position.set(s * .115, .24, .22);
      g.add(new THREE.Mesh(new THREE.SphereGeometry(.036, 14, 12), mat(0x050505, .15)));
      const b = new THREE.Mesh(new THREE.SphereGeometry(.012, 8, 8), new THREE.MeshBasicMaterial({ color: 0xffffff })); b.position.set(.012, .014, .028); g.add(b); cab.add(g); return g; });
    const pk = o.pico || 1;
    const pico = new THREE.Mesh(new THREE.ConeGeometry(.05 * (o.picoAncho || 1), .40 * pk, 14), mat(o.picoColor || 0x1d1d22, .4)); pico.rotation.x = Math.PI / 2 + .14; pico.position.set(0, .17, .30 + .20 * pk); cab.add(pico);
    if (!o.cria) { const man = new THREE.Mesh(new THREE.ConeGeometry(.034, .33, 12), mat(0xff915c, .5)); man.rotation.x = Math.PI / 2 + .22; man.position.set(0, .12, .46); cab.add(man); }
    /* aletas (pivote en el hombro), pies y cola */
    const aletas = [-1, 1].map(s => { const p = new THREE.Group(); p.position.set(s * .5 * gx, 1.30 * ay, 0); p.rotation.z = s * .25;
      const a = new THREE.Mesh(new THREE.SphereGeometry(.5, 20, 14), mat(o.cuerpo, .5)); a.scale.set(.07, .62 * (o.aleta || 1), .24 * (o.aleta || 1)); a.position.y = -.3 * (o.aleta || 1); p.add(a); cuerpo.add(p); return p; });
    const pies = [-1, 1].map(s => { const p = new THREE.Group(); p.position.set(s * .2 * gx, .06, .28);
      const f = new THREE.Mesh(new THREE.SphereGeometry(.5, 16, 12), mat(o.pies || 0x2a2623, .8)); f.scale.set(.34, .10, .56); f.position.z = .12; p.add(f); raiz.add(p); return p; });
    const cola = new THREE.Mesh(new THREE.ConeGeometry(.12, .35, 8), mat(o.cuerpo, .6)); cola.rotation.x = -Math.PI / 2 - .4; cola.position.set(0, .18, -.46); cuerpo.add(cola);
    raiz.scale.setScalar(o.escala || 1);
    return { raiz, cuerpo, cab, aletas, pies, ojos };
  }

  function crearEscena(THREE) {
    const scene = new THREE.Scene(), camara = new THREE.PerspectiveCamera(30, 1.4, .1, 60);
    camara.position.set(0, 1.55, 8.4); camara.lookAt(0, 1.05, 0);
    const hemi = new THREE.HemisphereLight(0xcfe3ff, 0x2b2338, .95), clave = new THREE.DirectionalLight(0xfff0dc, 1.05), borde = new THREE.DirectionalLight(0xffa860, .8);
    clave.position.set(2.5, 3.5, 4); borde.position.set(-3, 2.5, -3); scene.add(hemi, clave, borde);
    const adulto = pinguino(THREE, { escala: 1, cuerpo: 0x1e232b, vientre: 0xf6f3ea, oro: 0xf2b53c, cabeza: 0x14171c, mejilla: 0xf0a43a, pico: 1.0 });
    const cria = pinguino(THREE, { escala: .5, cria: true, gordo: 1.22, alto: .86, cabezaEsc: 1.28, cuerpo: 0x80868f, vientre: 0xa3a9b2, cabeza: 0x15171c, mascara: 0xf3f3f3, pico: .5, picoAncho: 1.3, aleta: .75, felpa: .95, pies: 0x2b2724 });
    adulto.raiz.position.set(-.55, 0, 0); cria.raiz.position.set(1.25, 0, .25); scene.add(adulto.raiz, cria.raiz);
    /* sombra suave bajo cada pingüino */
    let tex = null; try { const c = document.createElement("canvas"); c.width = c.height = 64; const x = c.getContext("2d");
      if (x) { const g = x.createRadialGradient(32, 32, 2, 32, 32, 32); g.addColorStop(0, "rgba(0,0,0,.45)"); g.addColorStop(1, "rgba(0,0,0,0)"); x.fillStyle = g; x.fillRect(0, 0, 64, 64); tex = new THREE.CanvasTexture(c); } } catch (e) { }
    const sombras = tex ? [[adulto, 1.7], [cria, 1.0]].map(([p, tam]) => { const m = new THREE.Mesh(new THREE.PlaneGeometry(tam, tam), new THREE.MeshBasicMaterial({ map: tex, transparent: true, depthWrite: false })); m.rotation.x = -Math.PI / 2; m.position.y = .005; scene.add(m); return m; }) : [];
    const E = { lado: 1, fase: 0, faseC: 1.3, parp: [2, 3.5], tp: [-9, -9] };
    function mover(p, i, t, w, red) {
      if (t > E.parp[i]) { E.tp[i] = t; E.parp[i] = t + 2.5 + Math.random() * 4; }
      const cerrado = t - E.tp[i] < .13; p.ojos.forEach(o => o.scale.y = cerrado ? .1 : 1);
    }
    function update(dt, t, est) {
      dt = Math.min(dt, .05); const red = est.reducido, w = red ? 0 : clamp(est.vel / 110, 0, 1), dir = est.dir || 0, k = 1 - Math.exp(-6 * dt);
      const px = red ? 0 : est.px, py = red ? 0 : est.py;
      E.fase += dt * (5 + 7 * w); E.faseC += dt * (6 + 9 * w);
      if (dir !== 0 && w > .15) E.lado = dir > 0 ? -1 : 1; /* la cría queda detrás */
      adulto.raiz.position.x = lerp(adulto.raiz.position.x, -.55 * E.lado, 1 - Math.exp(-4 * dt));
      cria.raiz.position.x = lerp(cria.raiz.position.x, 1.25 * E.lado, 1 - Math.exp(-2.8 * dt));
      [[adulto, E.fase, 0, 1], [cria, E.faseC, 1, 1.35]].forEach(([p, f, i, vivo]) => {
        const giro = w > .15 && dir ? dir * 1.1 : clamp(px * .55, -.55, .55);
        p.raiz.rotation.y = lerp(p.raiz.rotation.y, giro, k);
        p.cab.rotation.y = lerp(p.cab.rotation.y, clamp(px * .9 - p.raiz.rotation.y * .6, -.9, .9), k);
        p.cab.rotation.x = lerp(p.cab.rotation.x, clamp(py * .45, -.4, .4), k);
        const resp = Math.sin(t * 1.9 + i);
        p.cuerpo.scale.set(1 + .012 * resp, 1 + .018 * resp, 1 + .012 * resp);
        p.cuerpo.rotation.z = w * .13 * vivo * Math.sin(f) + (1 - w) * .02 * Math.sin(t * .9 + i);
        p.cuerpo.position.y = w * .05 * vivo * Math.abs(Math.sin(f));
        p.pies[0].rotation.x = w * .8 * Math.sin(f); p.pies[1].rotation.x = -w * .8 * Math.sin(f);
        const bal = w * .35 * Math.sin(f + 1.57);
        p.aletas[0].rotation.z = -(.25 + bal); p.aletas[1].rotation.z = .25 + bal;
        mover(p, i, t, w, red);
      });
      if (!red && est.saludo > 0) adulto.aletas[1].rotation.z = lerp(adulto.aletas[1].rotation.z, 2.4 + .3 * Math.sin(t * 14), clamp(est.saludo * 3, 0, 1));
      cria.raiz.position.y = !red && est.salto > 0 ? Math.sin(Math.PI * clamp(1 - est.salto, 0, 1)) * .4 : 0;
      if (sombras[0]) { sombras[0].position.x = adulto.raiz.position.x; sombras[1].position.x = cria.raiz.position.x; sombras[1].position.z = cria.raiz.position.z; }
    }
    const v = new THREE.Vector3();
    function cabezaPantalla(quien) { const p = quien === "h" ? cria : adulto; p.cab.getWorldPosition(v); v.y += .15; v.project(camara); return { x: v.x * .5 + .5, y: -v.y * .5 + .5 }; }
    return { scene, camara, adulto, cria, update, cabezaPantalla, luzBorde: hex => borde.color.set(hex) };
  }

  /* ================= FRASES POR PESTAÑA (p = pingüino, h = cría) ================= */
  const FRASES = {
    login: [["dialogo", "p", "¡Hola! Aquí puedes escribir con calma."], ["pensamiento", "h", "¿Entramos juntos?"]],
    inicio: [["dialogo", "p", "Qué bueno verte por aquí."], ["pensamiento", "p", "¿Cómo te sentirás hoy?"], ["narracion", "p", "Tu espacio tranquilo para escribir cómo te sientes."], ["susurro", "h", "Hoy puede ser un buen día…"]],
    registrar: [["dialogo", "p", "Es bueno expresar cómo nos sentimos."], ["susurro", "p", "Aquí no hay respuestas incorrectas…"], ["pensamiento", "p", "Con una frase basta para empezar."], ["dialogo", "h", "¡Tú puedes contarlo!"]],
    historial: [["dialogo", "p", "Cada color cuenta una parte de tu historia."], ["pensamiento", "p", "¿Qué día se ve más brillante?"], ["narracion", "p", "Tu mes en colores: así ves cómo vas."]],
    cambio: [["dialogo", "p", "Pequeños pasos, grandes cambios."], ["grito", "h", "¡Una victoria al día!"], ["pensamiento", "p", "¿Cuál será mi micro-meta de hoy?"]],
    logros: [["grito", "h", "¡Mira cuántas medallas!"], ["dialogo", "p", "Cada logro cuenta, por pequeño que sea."]],
    chat: [["susurro", "p", "Puedes contarme lo que quieras…"], ["dialogo", "p", "Aquí te escucho, con calma."], ["narracion", "p", "Soy una guía sencilla: no sustituyo a una persona."]],
    relax: [["susurro", "p", "Inhala… y exhala…"], ["pensamiento", "h", "Mmm, qué calma."], ["dialogo", "p", "Respiremos juntos."]],
    guardado: [["dialogo", "p", "¡Gracias por contarlo!"], ["grito", "h", "¡Muy bien hecho!"], ["dialogo", "h", "Eso estuvo genial."]]
  };

  /* ================= INTEGRACIÓN CON LA PÁGINA ================= */
  const S = { modo: null, montado: false, W: 270, H: 200, pos: { x: 0, y: 0 }, ptr: { x: 0, y: 0, t: 0 }, est: { px: 0, py: 0, vel: 0, dir: 0, saludo: 0, salto: 0, reducido: false },
    pendiente: null, ultimoGlobo: 0, silencioHasta: 0, ocultoRol: false, globo: null, ultimaFrase: {} };
  let THREE_, esc, renderer, cont, canvas, raf, tPrev = 0, tReloj = 0;
  const ahora = () => performance.now();

  function modoActual() { return S.modo || (S.modo = lget("emoti_ping") || (innerWidth < 700 ? "quieto" : "seguir")); }
  function tam() { S.W = innerWidth < 700 ? 190 : 270; S.H = Math.round(S.W * .74); }
  function reposo() { return { x: innerWidth - S.W - 8, y: innerHeight - S.H - 8 }; }

  function inyectarCSS() {
    if (document.getElementById("pinCSS")) return;
    const st = document.createElement("style"); st.id = "pinCSS";
    st.textContent = `#pinCap{position:fixed;left:0;top:0;z-index:4;pointer-events:none;will-change:transform}#pinCap canvas{display:block;width:100%;height:100%}
.pg{position:fixed;left:0;top:0;z-index:5;pointer-events:none;color:var(--ink);font:500 .95rem/1.35 var(--fb);transform-origin:50% 100%;animation:pgIn .22s ease-out both}
.pg svg{position:absolute;left:0;top:0;overflow:visible;filter:drop-shadow(0 3px 6px rgba(0,0,0,.28))}.pg-t{position:absolute;display:block;text-align:center}
.pg-grito .pg-t{font-weight:800;color:var(--bt)}.pg-susurro .pg-t{font-style:italic;color:var(--mut);font-size:.88rem}.pg-pensamiento .pg-t{font-style:italic}.pg-narracion .pg-t{text-align:left}
.pg-medida{position:fixed;left:-9999px;top:0;font:500 .95rem/1.35 var(--fb);visibility:hidden}.pg-medida .pg-t{position:static;width:max-content;max-width:208px}
@keyframes pgIn{from{opacity:0;transform:scale(.85)}to{opacity:1;transform:none}}@media (prefers-reduced-motion:reduce){.pg{animation:none}}`;
    document.head.appendChild(st);
  }

  function ocultarGlobo() { if (S.globo) { S.globo.el.remove(); clearTimeout(S.globo.tm); S.globo = null; } }
  function mostrarGlobo(tipo, quien, texto) {
    ocultarGlobo(); if (!esc) return;
    const med = document.createElement("div"); med.className = "pg-medida pg-" + tipo; med.innerHTML = '<span class="pg-t"></span>'; med.firstChild.textContent = texto; document.body.appendChild(med);
    const tw = med.firstChild.offsetWidth, th = med.firstChild.offsetHeight; med.remove();
    const f = tipo === "narracion" ? 1 : tipo === "pensamiento" ? 1.16 : tipo === "grito" ? 1.34 : 1.3;
    const W = Math.ceil(tw * f + (tipo === "narracion" ? 34 : 30)), H = Math.ceil(th * f + (tipo === "narracion" ? 18 : 22)), T = COLA[tipo];
    const c = esc.cabezaPantalla(quien), sx = S.pos.x + c.x * S.W, sy = S.pos.y + c.y * S.H;
    const abajo = sy - (H + T) - 14 < 4, left = clamp(sx - W / 2, 8, innerWidth - W - 8), top = abajo ? sy + 70 : sy - (H + T) - 6;
    const el = document.createElement("div"); el.className = "pg pg-" + tipo; el.setAttribute("role", "status");
    el.style.cssText = `width:${W}px;height:${H + T}px;left:${left}px;top:${clamp(top, 4, innerHeight - H - T - 4)}px`;
    el.innerHTML = svgGlobo(tipo, W, H, sx - left); if (abajo && T) el.firstChild.style.transform = "scaleY(-1)";
    const t = document.createElement("span"); t.className = "pg-t"; t.textContent = texto; t.style.cssText = `left:${(W - tw) / 2 + (tipo === "narracion" ? 5 : 0)}px;top:${(H - th) / 2}px;width:${tw + 2}px`; el.appendChild(t);
    document.body.appendChild(el);
    S.globo = { el, x0: S.pos.x, y0: S.pos.y, tm: setTimeout(ocultarGlobo, 3800 + texto.length * 55) }; S.ultimoGlobo = ahora();
  }

  function decir(clave) {
    if (modoActual() === "mudo" || ahora() < S.silencioHasta || !FRASES[clave]) return;
    if (clave !== "guardado" && S.ultimoGlobo && ahora() - S.ultimoGlobo < 6000) return; /* pausa mínima: no saturar */
    const lista = FRASES[clave]; let i = Math.floor(Math.random() * lista.length); if (lista.length > 1 && i === S.ultimaFrase[clave]) i = (i + 1) % lista.length;
    S.ultimaFrase[clave] = i; const [tipo, quien, texto] = lista[i]; mostrarGlobo(tipo, quien, texto);
  }

  function cargarThree() {
    return new Promise((ok, mal) => { if (window.THREE) return ok(window.THREE);
      const s = document.createElement("script"); s.src = BASE + "three.min.js"; s.onload = () => ok(window.THREE); s.onerror = mal; document.head.appendChild(s); });
  }

  function montar() {
    if (S.montado || modoActual() === "oculto") return; tam(); inyectarCSS();
    cont = document.createElement("div"); cont.id = "pinCap"; cont.setAttribute("aria-hidden", "true"); canvas = document.createElement("canvas"); cont.appendChild(canvas);
    try { renderer = new THREE_.WebGLRenderer({ canvas, alpha: true, antialias: true, powerPreference: "low-power" }); }
    catch (e) { return; } /* sin WebGL: la página funciona igual, solo sin pingüinos */
    renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2)); renderer.setSize(S.W, S.H, false); renderer.setClearColor(0x000000, 0);
    canvas.addEventListener("webglcontextlost", e => { e.preventDefault(); detener(); });
    esc = crearEscena(THREE_); esc.camara.aspect = S.W / S.H; esc.camara.updateProjectionMatrix(); aplicarTema();
    cont.style.width = S.W + "px"; cont.style.height = S.H + "px"; S.pos = reposo(); document.body.appendChild(cont); S.montado = true; S.est.reducido = reducido(); S.ocultoRol && (cont.style.display = "none");
    tPrev = ahora(); raf = requestAnimationFrame(bucle);
  }
  function detener() { cancelAnimationFrame(raf); ocultarGlobo(); if (cont) cont.remove(); if (renderer) { renderer.dispose(); renderer = null; } esc = null; S.montado = false; }

  function bucle(ms) {
    raf = requestAnimationFrame(bucle); if (document.hidden || !renderer) return;
    const dt = Math.min((ms - tPrev) / 1000, .05); tPrev = ms; tReloj += dt; const e = S.est, red = e.reducido, m = modoActual();
    let tx, ty; const r = reposo(), p = S.ptr, reciente = p.t && ahora() - p.t < 8000;
    if (m === "seguir" && reciente && !red) { tx = clamp(p.x - S.W / 2 + 120, 0, innerWidth - S.W); ty = clamp(p.y - S.H + 70, 0, innerHeight - S.H); } else { tx = r.x; ty = r.y; }
    const dx = tx - S.pos.x, dy = ty - S.pos.y, dist = Math.hypot(dx, dy);
    if (red) { S.pos.x = tx; S.pos.y = ty; e.vel = 0; e.dir = 0; }
    else { const v = Math.min(dist * 3, 260); if (dist > 1) { S.pos.x += dx / dist * v * dt; S.pos.y += dy / dist * v * dt; } e.vel = lerp(e.vel, dist > 6 ? v : 0, .2); e.dir = Math.abs(dx) > Math.abs(dy) * .5 && dist > 6 ? Math.sign(dx) : 0; }
    e.px = clamp((p.x - (S.pos.x + S.W / 2)) / (innerWidth * .35), -1, 1); e.py = clamp((p.y - (S.pos.y + S.H * .4)) / (innerHeight * .35), -1, 1);
    e.saludo = Math.max(0, e.saludo - dt / 1.6); e.salto = Math.max(0, e.salto - dt / .7);
    cont.style.transform = `translate3d(${S.pos.x.toFixed(1)}px,${S.pos.y.toFixed(1)}px,0)`;
    if (S.pendiente) { const t = ahora() - S.pendiente.t0, llego = m === "seguir" && !red ? (dist < 50 && e.vel < 25) : t > 450;
      if (llego || t > 3500) { decir(S.pendiente.clave); S.pendiente = null; e.saludo = 1; e.salto = 1; } }
    if (S.globo) { S.globo.el.style.marginLeft = (S.pos.x - S.globo.x0).toFixed(1) + "px"; S.globo.el.style.marginTop = (S.pos.y - S.globo.y0).toFixed(1) + "px"; } /* el globo acompaña al grupo */
    esc.update(dt, tReloj, e); renderer.render(esc.scene, esc.camara);
  }

  /* ---- API pública ---- */
  const PIN = window.PIN = {
    formas, TIPOS, FRASES, _crearEscena: crearEscena, _svgGlobo: svgGlobo, _S: S,
    tab(v) { if (!S.montado || S.ocultoRol || modoActual() === "oculto") return; ocultarGlobo(); S.pendiente = { clave: FRASES[v] ? v : "inicio", t0: ahora() }; if (modoActual() === "seguir" && !S.ptr.t) S.ptr.t = ahora() - 7000; },
    evento(n) { if (n === "riesgo") { PIN.silencio(20000); ocultarGlobo(); } else if (S.montado && !S.ocultoRol) { S.pendiente = { clave: n, t0: ahora() - 3000 }; } },
    silencio(ms) { S.silencioHasta = ahora() + ms; },
    sesion(s) { S.ocultoRol = !!(s && s.rol === "psicologa"); if (cont) cont.style.display = S.ocultoRol ? "none" : ""; if (S.ocultoRol) ocultarGlobo(); else if (!s) setTimeout(() => PIN.tab("login"), 1200); },
    tema() { if (!esc) return; const a = getComputedStyle(document.body).getPropertyValue("--acc").trim(); if (a) { try { esc.luzBorde(a); } catch (e) { } } },
    modo(m) { S.modo = m; lset("emoti_ping", m); const sel = document.getElementById("pingSel"); if (sel) sel.value = m; if (m === "oculto") detener(); else if (!S.montado) iniciar(); }
  };
  function aplicarTema() { PIN.tema(); }

  function iniciar() {
    if (modoActual() === "oculto") return;
    cargarThree().then(t => { THREE_ = t; montar(); }).catch(() => { });
  }
  function arrancar() {
    const sel = document.getElementById("pingSel"); if (sel) { sel.value = modoActual(); sel.onchange = e => PIN.modo(e.target.value); }
    const mover = e => { S.ptr.x = e.clientX; S.ptr.y = e.clientY; S.ptr.t = ahora(); };
    addEventListener("pointermove", mover, { passive: true }); addEventListener("pointerdown", e => { mover(e); if (S.globo) ocultarGlobo(); }, { passive: true });
    addEventListener("resize", () => { if (!S.montado) return; tam(); renderer.setSize(S.W, S.H, false); esc.camara.aspect = S.W / S.H; esc.camara.updateProjectionMatrix(); cont.style.width = S.W + "px"; cont.style.height = S.H + "px"; });
    (window.requestIdleCallback || (f => setTimeout(f, 700)))(iniciar);
  }
  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", arrancar); else arrancar();
})();
