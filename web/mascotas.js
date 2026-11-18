/* EmotiData · Mascotas: pingüinos 3D (three.js servido desde tu propio sitio) o familias pixeladas de pingüino, gato, perro y capibara.
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


  /* ================= MASCOTAS PIXELADAS (2D, sin WebGL) ================= */
  const PAL = {
    pinguinopx: { a: [0, "#0b0d10", "#20252d", "#f5f2e8", "#f2b53c", "#f08c3c", 0, "#14171c", "#f6f6f6"], c: [0, "#0b0d10", "#8a9099", "#cdd2d8", "#cdd2d8", "#f08c3c", 0, "#14171c", "#f6f6f6"] },
    gato: { a: [0, "#3b2314", "#f2a65c", "#fff0da", "#d98b3f", "#f19aa5", "#efe3d3"], c: [0, "#3b2314", "#f8d2a6", "#fff6e8", "#e7b680", "#f5a9b3", "#efe3d3"] },
    perro: { a: [0, "#2d1b10", "#c68a50", "#f3dfc3", "#8d5a30", "#2a1d17", "#f08a9a"], c: [0, "#2d1b10", "#e6bd8a", "#fbefdc", "#b98652", "#2a1d17", "#f08a9a"] },
    capibara: { a: [0, "#2b1a10", "#a9764a", "#c99a6c", "#7b5232", "#33241a", "#f2891e", "#4f8f3e"], c: [0, "#2b1a10", "#bb8a5d", "#d8ae82", "#8c6038", "#33241a", "#f2891e", "#4f8f3e"] }
  };
  function elipse(g, cx, cy, rx, ry, c) { for (let y = 0; y < g.h; y++) for (let x = 0; x < g.w; x++) { const dx = (x + .5 - cx) / rx, dy = (y + .5 - cy) / ry; if (dx * dx + dy * dy <= 1) g.p[y * g.w + x] = c; } }
  function rect(g, x, y, w, h, c) { for (let j = y; j < y + h; j++) for (let i = x; i < x + w; i++) if (i >= 0 && j >= 0 && i < g.w && j < g.h) g.p[j * g.w + i] = c; }
  function tri(g, ax, ay, bx, by, cx, cy, c) {
    const s = (x1, y1, x2, y2, x3, y3) => (x1 - x3) * (y2 - y3) - (x2 - x3) * (y1 - y3);
    for (let y = 0; y < g.h; y++) for (let x = 0; x < g.w; x++) { const px = x + .5, py = y + .5, d1 = s(px, py, ax, ay, bx, by), d2 = s(px, py, bx, by, cx, cy), d3 = s(px, py, cx, cy, ax, ay);
      if (!((d1 < 0 || d2 < 0 || d3 < 0) && (d1 > 0 || d2 > 0 || d3 > 0))) g.p[y * g.w + x] = c; }
  }
  function contorno(g, c) { const q = g.p.slice(); for (let y = 0; y < g.h; y++) for (let x = 0; x < g.w; x++) { if (g.p[y * g.w + x]) continue;
    if ((x > 0 && g.p[y * g.w + x - 1]) || (x < g.w - 1 && g.p[y * g.w + x + 1]) || (y > 0 && g.p[(y - 1) * g.w + x]) || (y < g.h - 1 && g.p[(y + 1) * g.w + x])) q[y * g.w + x] = c; } g.p = q; }

  /* Dibuja a la mascota de frente con formas simples en una cuadrícula de 26×24 (la cría se reduce y tiene cabeza más grande). */
  function sprite(esp, cria, frame) {
    const k = cria ? .68 : 1, hs = cria ? 1.18 : 1, w = Math.round(26 * k) + 2, h = Math.round(24 * k) + 2, g = { w, h, p: new Uint8Array(w * h) };
    const X = x => (x - 13) * k + w / 2, Y = y => h - 1 - (24 - y) * k;
    const E = (cx, cy, rx, ry, c) => elipse(g, X(cx), Y(cy), Math.max(.8, rx * k), Math.max(.8, ry * k), c);
    const R = (x, y, ww, hh, c) => rect(g, Math.round(X(x)), Math.round(Y(y)), Math.max(1, Math.round(ww * k)), Math.max(1, Math.round(hh * k)), c);
    const T = (ax, ay, bx, by, cx, cy, c) => tri(g, X(ax), Y(ay), X(bx), Y(by), X(cx), Y(cy), c);
    const fl = frame === 1 ? 1 : 0, fr = frame === 2 ? 1 : 0; let ojos, n = 3, post = null;
    if (esp === "pinguinopx") {
      E(5, 14.5, 1.8, 5, 2); E(21, 14.5, 1.8, 5, 2); E(13, 15, 7.2, 8.6, 2); E(13, 16.2, 4.6, 6.6, 3);
      if (!cria) E(13, 10.6, 3.7, 1.9, 4);
      E(13, 7, 6.3 * hs, 5.6 * hs, 7);
      if (cria) { E(9.6, 7.2, 2.3, 2.5, 8); E(16.4, 7.2, 2.3, 2.5, 8); } else { E(7.9, 8.8, 1.3, 1.8, 4); E(18.1, 8.8, 1.3, 1.8, 4); }
      R(12, 9.2, 2, 2, 5); R(8, 22 - fl, 4, 2, 5); R(14, 22 - fr, 4, 2, 5); ojos = [[8, 5], [15, 5]];
    } else if (esp === "gato") {
      T(5, 1.5, 5.2, 8.5, 10.8, 5.6, 2); T(21, 1.5, 20.8, 8.5, 15.2, 5.6, 2); T(6.4, 3.8, 6.6, 7.2, 9.4, 5.8, 5); T(19.6, 3.8, 19.4, 7.2, 16.6, 5.8, 5);
      E(13, 18, 6.6, 5.6, 2); E(13, 19.3, 3.8, 4, 3); E(13, 9.8, 8.3 * hs, 6.3 * hs, 2);
      R(12.4, 4.6, 1.2, 2.2, 4); R(10, 5.2, 1, 1.6, 4); R(16, 5.2, 1, 1.6, 4); E(13, 12, 3.4, 2.2, 3); R(12, 10.6, 2, 1, 5);
      E(9.2, 22.4 - fl, 2.3, 1.5, 3); E(16.8, 22.4 - fr, 2.3, 1.5, 3); ojos = [[8, 7], [15, 7]];
      post = () => { R(2, 11, 3, 1, 6); R(2, 13, 3, 1, 6); R(21, 11, 3, 1, 6); R(21, 13, 3, 1, 6); };
    } else if (esp === "perro") {
      E(5.2, 10.4, 2.7, 5.6, 4); E(20.8, 10.4, 2.7, 5.6, 4); E(13, 18, 6.4, 5.6, 2); E(13, 19.4, 3.8, 4, 3); E(13, 9, 6.9 * hs, 6 * hs, 2);
      E(16.8, 7.4, 2.6, 2.4, 4); E(13, 11.8, 4, 3, 3); R(12, 10.4, 2, 1.4, 5); R(12.6, 13.4, 1.2, 1.6, 6);
      E(9.2, 22.4 - fl, 2.3, 1.5, 3); E(16.8, 22.4 - fr, 2.3, 1.5, 3); ojos = [[7.6, 6.4], [15, 6.4]];
    } else {
      E(13, 17.8, 10.4, 6, 2); E(13, 20.2, 6.6, 3, 3); R(7, 5.5, 12, 7.5, 2); E(13, 9.2, 8 * hs, 6.2 * hs, 2); E(7.6, 3.8, 1.9, 1.7, 4); E(18.4, 3.8, 1.9, 1.7, 4);
      E(13, 12, 5.4, 3.2, 3); R(10.4, 10.6, 1, 1, 5); R(15.6, 10.6, 1, 1, 5); R(12, 13.4, 2, 1, 4);
      if (!cria) { E(13, 2.6, 2.4, 1.5, 6); R(14.5, .6, 1.6, 1, 7); }
      R(5, 22.6 - fl, 4, 2, 4); R(17, 22.6 - fr, 4, 2, 4); ojos = [[8.2, 7.6], [17.8, 7.6]]; n = 1;
    }
    contorno(g, 1); if (post) post();
    return { g, n, ojos: ojos.map(o => [Math.round(X(o[0])), Math.round(Y(o[1]))]), pal: PAL[esp][cria ? "c" : "a"] };
  }

  function crearMotorPixel(esp) {
    const tm = tamActual(), px = Math.max(2, ({ p: 2, m: 3, g: 4 }[tm]) - (innerWidth < 700 && tm !== "p" ? 1 : 0));
    const cache = {};
    const get = (cria, fr) => { const kk = (cria ? "c" : "a") + fr; if (!cache[kk]) { const sp = sprite(esp, cria, fr), c = document.createElement("canvas"); c.width = sp.g.w; c.height = sp.g.h;
      const x = c.getContext("2d"), im = x.createImageData(sp.g.w, sp.g.h); sp.g.p.forEach((v, i) => { if (v) { const n = parseInt(sp.pal[v].slice(1), 16); im.data.set([n >> 16 & 255, n >> 8 & 255, n & 255, 255], i * 4); } });
      x.putImageData(im, 0, 0); cache[kk] = Object.assign(sp, { c }); } return cache[kk]; };
    const A = get(false, 0), C = get(true, 0);
    S.W = (A.g.w + C.g.w + 10) * px; S.H = (A.g.h + 7) * px; canvas.width = S.W; canvas.height = S.H; canvas.style.imageRendering = "pixelated";
    const ctx = canvas.getContext("2d"); ctx.imageSmoothingEnabled = false;
    const E = { lado: 1, ax: 0, cx: 0, fase: 0, parp: 2.5, tp: -9, ok: false, fa: 0, fc: 0, ya: 0, yc: 0, dx: 0, dy: 0, cerrado: false };
    const base = () => S.H - 2 * px;
    const m = {
      update(dt, t, est) {
        dt = Math.min(dt, .05); const red = est.reducido, w = red ? 0 : clamp(est.vel / 110, 0, 1), dir = est.dir || 0, k = 1 - Math.exp(-5 * dt);
        if (dir !== 0 && w > .15) E.lado = dir > 0 ? -1 : 1; /* la cría queda detrás */
        const izq = 3 * px, der = S.W - 3 * px, aT = E.lado > 0 ? izq : der - A.g.w * px, cT = E.lado > 0 ? der - C.g.w * px : izq;
        if (!E.ok) { E.ax = aT; E.cx = cT; E.ok = true; }
        E.ax = lerp(E.ax, aT, k * .8); E.cx = lerp(E.cx, cT, k * .6); E.fase += dt * (w > .15 ? 8 : 0);
        const paso = w > .15 ? [1, 0, 2, 0][Math.floor(E.fase) % 4] : 0, pasoC = w > .15 ? [2, 0, 1, 0][Math.floor(E.fase + 1) % 4] : 0;
        E.fa = paso; E.fc = pasoC;
        E.ya = paso ? -px : (!red && Math.sin(t * 2.1) > .92 ? -px : 0); E.yc = pasoC ? -px : 0;
        if (!red && est.saludo > 0) E.ya = Math.round(-Math.abs(Math.sin(t * 11)) * 2) * px;
        if (!red && est.salto > 0) E.yc = -Math.round(Math.sin(Math.PI * clamp(1 - est.salto, 0, 1)) * 3) * px;
        E.dx = red ? 0 : est.px > .25 ? 1 : est.px < -.25 ? -1 : 0; E.dy = red ? 0 : est.py > .3 ? 1 : est.py < -.3 ? -1 : 0;
        if (t > E.parp) { E.tp = t; E.parp = t + 2.5 + Math.random() * 4; } E.cerrado = t - E.tp < .13;
      },
      dibujar() {
        ctx.clearRect(0, 0, S.W, S.H); const b = base();
        const pieza = (cria, fr, left, off) => { const s = get(cria, fr), x = Math.round(left / px) * px, y = b - s.g.h * px + off;
          ctx.fillStyle = "rgba(0,0,0,.22)"; [[s.g.w * .8, 0], [s.g.w * .62, 1], [s.g.w * .4, 2]].forEach(r => ctx.fillRect(Math.round((x + (s.g.w * px - r[0] * px) / 2) / px) * px, b + r[1] * px - px, Math.round(r[0]) * px, px));
          ctx.drawImage(s.c, x, y, s.g.w * px, s.g.h * px);
          s.ojos.forEach(o => { const ox = x + o[0] * px, oy = y + o[1] * px;
            if (s.n === 3) { if (E.cerrado) { ctx.fillStyle = s.pal[1]; ctx.fillRect(ox, oy + px, 3 * px, px); } else { ctx.fillStyle = "#fff"; ctx.fillRect(ox, oy, 3 * px, 3 * px); ctx.fillStyle = "#111"; ctx.fillRect(ox + (1 + E.dx) * px, oy + (1 + E.dy) * px, px, px); } }
            else if (!E.cerrado) { ctx.fillStyle = "#111"; ctx.fillRect(ox, oy, px, px); } }); };
        pieza(true, E.fc, E.cx, E.yc); pieza(false, E.fa, E.ax, E.ya);
      },
      cabezaPantalla(q) { const s = q === "h" ? C : A, left = q === "h" ? E.cx : E.ax; return { x: (left + s.g.w * px / 2) / S.W, y: (base() - s.g.h * px + (q === "h" ? E.yc : E.ya) + 2 * px) / S.H }; },
      luzBorde() { }, dispose() { }, redimensionar() { const v = mascActual(); detener(); S.masc = v; iniciar(); }
    };
    return m;
  }

  function fijarTam3D() { S.W = Math.round(({ p: 170, m: 230, g: 290 }[tamActual()]) * (innerWidth < 700 ? .8 : 1)); S.H = Math.round(S.W * .74); }
  function crearMotor3D() {
    fijarTam3D();
    const r = new THREE_.WebGLRenderer({ canvas, alpha: true, antialias: true, powerPreference: "low-power" }); /* puede lanzar si no hay WebGL */
    r.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2)); r.setSize(S.W, S.H, false); r.setClearColor(0x000000, 0);
    canvas.addEventListener("webglcontextlost", e => { e.preventDefault(); estado("Se perdió WebGL: se usan las mascotas pixeladas."); PIN.mascota("pinguinopx", true); });
    const m = crearEscena(THREE_); m.camara.aspect = S.W / S.H; m.camara.updateProjectionMatrix();
    m.dibujar = () => r.render(m.scene, m.camara);
    m.redimensionar = () => { fijarTam3D(); r.setSize(S.W, S.H, false); m.camara.aspect = S.W / S.H; m.camara.updateProjectionMatrix(); aplicarTam(); };
    m.dispose = () => r.dispose();
    return m;
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
  const S = { aviso: "", masc: null, tam: null, modo: null, montado: false, W: 270, H: 200, pos: { x: 0, y: 0 }, ptr: { x: 0, y: 0, t: 0 }, est: { px: 0, py: 0, vel: 0, dir: 0, saludo: 0, salto: 0, reducido: false },
    pendiente: null, ultimoGlobo: 0, silencioHasta: 0, ocultoRol: false, globo: null, ultimaFrase: {} };
  let THREE_, esc, renderer, cont, canvas, raf, tPrev = 0, tReloj = 0;
  const ahora = () => performance.now();

  function modoActual() { return S.modo || (S.modo = lget("emoti_ping") || (innerWidth < 700 ? "quieto" : "seguir")); }
  const MASCOTAS = ["pinguino3d", "pinguinopx", "gato", "perro", "capibara"];
  function mascActual() { const v = S.masc || lget("emoti_masc"); return MASCOTAS.indexOf(v) >= 0 ? v : "pinguino3d"; }
  function tamActual() { const v = S.tam || lget("emoti_masc_t"); return ["p", "m", "g"].indexOf(v) >= 0 ? v : "m"; }
  function estado(msg) { const el = document.getElementById("pingEstado"); if (el) el.textContent = msg; if (msg && /no |Sin /.test(msg)) console.warn("[mascotas] " + msg); }
  function aplicarTam() { cont.style.width = S.W + "px"; cont.style.height = S.H + "px"; }
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
    if (S.montado || modoActual() === "oculto") return; inyectarCSS();
    cont = document.createElement("div"); cont.id = "pinCap"; cont.setAttribute("aria-hidden", "true"); canvas = document.createElement("canvas"); cont.appendChild(canvas);
    let masc = mascActual(), mo = null;
    if (masc === "pinguino3d") { try { mo = crearMotor3D(); } catch (e) { S.aviso = "Este navegador no tiene WebGL, por eso se usan los pingüinos pixelados."; masc = "pinguinopx"; canvas = document.createElement("canvas"); cont.innerHTML = ""; cont.appendChild(canvas); } }
    if (!mo) mo = crearMotorPixel(masc);
    esc = mo; aplicarTam(); S.pos = reposo(); document.body.appendChild(cont); S.montado = true; S.est.reducido = reducido(); if (S.ocultoRol) cont.style.display = "none";
    estado("Mascotas activas: " + ({ pinguino3d: "pingüinos 3D", pinguinopx: "pingüinos pixelados", gato: "gatos", perro: "perros", capibara: "capibaras" }[masc]) + (S.aviso ? ". " + S.aviso : "")); if (S.aviso) console.warn("[mascotas] " + S.aviso); PIN.tema();
    tPrev = ahora(); raf = requestAnimationFrame(bucle);
  }
  function detener() { cancelAnimationFrame(raf); ocultarGlobo(); if (cont) cont.remove(); if (esc && esc.dispose) esc.dispose(); esc = null; S.montado = false; }

  function bucle(ms) {
    raf = requestAnimationFrame(bucle); if (document.hidden || !esc) return;
    const dt = Math.min((ms - tPrev) / 1000, .05); tPrev = ms; tReloj += dt; const e = S.est, red = e.reducido, m = modoActual();
    let tx, ty; const r = reposo(), p = S.ptr, reciente = p.t && ahora() - p.t < 8000;
    if (m === "seguir" && reciente && !red) { tx = clamp(p.x - S.W / 2 + 120, 0, innerWidth - S.W); ty = clamp(p.y - S.H + 70, 0, innerHeight - S.H); } else { tx = r.x; ty = r.y; }
    const dx = tx - S.pos.x, dy = ty - S.pos.y, dist = Math.hypot(dx, dy);
    if (red) { S.pos.x = tx; S.pos.y = ty; e.vel = 0; e.dir = 0; }
    else { const v = Math.min(dist * 3, 260); if (dist > 1) { S.pos.x += dx / dist * v * dt; S.pos.y += dy / dist * v * dt; } e.vel = lerp(e.vel, dist > 6 ? v : 0, .2); e.dir = Math.abs(dx) > Math.abs(dy) * .5 && dist > 6 ? Math.sign(dx) : 0; }
    if (p.t) { e.px = clamp((p.x - (S.pos.x + S.W / 2)) / (innerWidth * .35), -1, 1); e.py = clamp((p.y - (S.pos.y + S.H * .4)) / (innerHeight * .35), -1, 1); } else { e.px = 0; e.py = 0; } /* sin cursor conocido: mirar al frente */
    e.saludo = Math.max(0, e.saludo - dt / 1.6); e.salto = Math.max(0, e.salto - dt / .7);
    cont.style.transform = `translate3d(${S.pos.x.toFixed(1)}px,${S.pos.y.toFixed(1)}px,0)`;
    if (S.pendiente) { const t = ahora() - S.pendiente.t0, llego = m === "seguir" && !red ? (dist < 50 && e.vel < 25) : t > 450;
      if (llego || t > 3500) { decir(S.pendiente.clave); S.pendiente = null; e.saludo = 1; e.salto = 1; } }
    if (S.globo) { S.globo.el.style.marginLeft = (S.pos.x - S.globo.x0).toFixed(1) + "px"; S.globo.el.style.marginTop = (S.pos.y - S.globo.y0).toFixed(1) + "px"; } /* el globo acompaña al grupo */
    esc.update(dt, tReloj, e); esc.dibujar();
  }

  /* ---- API pública ---- */
  const PIN = window.PIN = {
    formas, TIPOS, FRASES, _crearEscena: crearEscena, _svgGlobo: svgGlobo, _S: S, _sprite: sprite, PAL,
    tab(v) { if (!S.montado || S.ocultoRol || modoActual() === "oculto") return; ocultarGlobo(); S.pendiente = { clave: FRASES[v] ? v : "inicio", t0: ahora() }; if (modoActual() === "seguir" && !S.ptr.t) S.ptr.t = ahora() - 7000; },
    evento(n) { if (n === "riesgo") { PIN.silencio(20000); ocultarGlobo(); } else if (S.montado && !S.ocultoRol) { S.pendiente = { clave: n, t0: ahora() - 3000 }; } },
    silencio(ms) { S.silencioHasta = ahora() + ms; },
    sesion(s) { S.ocultoRol = !!(s && s.rol === "psicologa"); if (cont) cont.style.display = S.ocultoRol ? "none" : ""; if (S.ocultoRol) ocultarGlobo(); else if (!s) setTimeout(() => PIN.tab("login"), 1200); },
    mascota(v, noGuardar) { S.aviso = noGuardar ? S.aviso : ""; S.masc = v; if (!noGuardar) lset("emoti_masc", v); const sel = document.getElementById("mascSel"); if (sel) sel.value = v; if (S.montado) detener(); iniciar(); },
    tamano(v) { S.tam = v; lset("emoti_masc_t", v); const sel = document.getElementById("mascTam"); if (sel) sel.value = v; if (S.montado) detener(); iniciar(); },
    tema() { if (!esc) return; const a = getComputedStyle(document.body).getPropertyValue("--acc").trim(); if (a) { try { esc.luzBorde(a); } catch (e) { } } },
    modo(m) { S.modo = m; lset("emoti_ping", m); const sel = document.getElementById("pingSel"); if (sel) sel.value = m; if (m === "oculto") detener(); else if (!S.montado) iniciar(); }
  };
  function aplicarTema() { PIN.tema(); }

  function iniciar() {
    if (modoActual() === "oculto" || S.montado) return;
    if (mascActual() !== "pinguino3d") return montar();
    cargarThree().then(t => { THREE_ = t; montar(); }).catch(() => { S.aviso = "No se encontró three.min.js junto a la página (debe estar dentro de la carpeta web), por eso se usan los pingüinos pixelados."; S.masc = "pinguinopx"; montar(); });
  }
  function arrancar() {
    const sel = document.getElementById("pingSel"); if (sel) { sel.value = modoActual(); sel.onchange = e => PIN.modo(e.target.value); }
    const mover = e => { S.ptr.x = e.clientX; S.ptr.y = e.clientY; S.ptr.t = ahora(); };
    addEventListener("pointermove", mover, { passive: true }); addEventListener("pointerdown", e => { mover(e); if (S.globo) ocultarGlobo(); }, { passive: true });
    let tr; addEventListener("resize", () => { clearTimeout(tr); tr = setTimeout(() => { if (S.montado && esc) esc.redimensionar(); }, 250); });
    const ms = document.getElementById("mascSel"), mt = document.getElementById("mascTam");
    if (ms) { ms.value = mascActual(); ms.onchange = e => PIN.mascota(e.target.value); } if (mt) { mt.value = tamActual(); mt.onchange = e => PIN.tamano(e.target.value); }
    (window.requestIdleCallback || (f => setTimeout(f, 700)))(iniciar);
  }
  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", arrancar); else arrancar();
})();
