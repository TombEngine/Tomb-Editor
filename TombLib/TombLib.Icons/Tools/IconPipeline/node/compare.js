// Usage: node compare.js --legacy-root <export dir> --sources <csv from the IconPipeline legacy-sources command> --ids "General/trash,Actions/cut" --out sheet.png
// Columns: original (8x) | svg rendered at the original size (8x) | overlay (red = original only, cyan = svg only, white = both) | svg rendered large.
const fs = require('fs');
const path = require('path');
const { Resvg } = require('@resvg/resvg-js');
const { PNG } = require('pngjs');

const args = {};
for (let i = 2; i < process.argv.length; i += 2) args[process.argv[i].replace(/^--/, '')] = process.argv[i + 1];
const svgRoot = path.join(__dirname, '..', '..', '..', 'Svg');
const rows = fs.readFileSync(args.sources, 'utf8').split(/\r?\n/).slice(1).filter(Boolean).map(l => {
  const m = l.match(/"([^"]*)"/g).map(s => s.replace(/"/g, ''));
  return { id: m[0], legacy: path.join(args['legacy-root'], m[1].replace(/\//g, path.sep)), w: +m[2], h: +m[3] };
});
const wanted = (args.ids || '').split(',').map(s => s.trim()).filter(Boolean);
const zoom = 8;
const items = wanted.map(id => rows.find(r => r.id.toLowerCase() === id.toLowerCase())).filter(Boolean);
if (items.length !== wanted.length) console.log('unknown ids: ' + wanted.filter(id => !rows.find(r => r.id.toLowerCase() === id.toLowerCase())).join(','));

function renderSvg(file, w, h) {
  const svg = fs.readFileSync(file, 'utf8');
  const r = new Resvg(svg, { fitTo: { mode: 'width', value: w }, background: 'rgba(0,0,0,0)' });
  const img = r.render();
  // resvg keeps aspect ratio; if the height differs pad/crop.
  const out = new Uint8Array(w * h * 4);
  const iw = img.width, ih = img.height, px = img.pixels;
  for (let y = 0; y < Math.min(h, ih); y++) for (let x = 0; x < Math.min(w, iw); x++) for (let c = 0; c < 4; c++) out[(y * w + x) * 4 + c] = px[(y * iw + x) * 4 + c];
  return out;
}

const cell = 16 * zoom;
const gap = 12;
const rowH = cell + 14;
const sheet = new PNG({ width: 4 * (cell + gap) + gap, height: rowH * Math.max(items.length, 1) + 6 });
sheet.data.fill(0);
for (let i = 0; i < sheet.data.length; i += 4) { sheet.data[i] = 45; sheet.data[i + 1] = 47; sheet.data[i + 2] = 49; sheet.data[i + 3] = 255; }

function put(img, w, h, ox, oy, z) {
  for (let y = 0; y < h * z; y++) for (let x = 0; x < w * z; x++) {
    const sx = Math.floor(x / z), sy = Math.floor(y / z);
    const a = img[(sy * w + sx) * 4 + 3] / 255;
    const dx = ox + x, dy = oy + y;
    if (dx < 0 || dy < 0 || dx >= sheet.width || dy >= sheet.height) continue;
    const i = (dy * sheet.width + dx) * 4;
    for (let c = 0; c < 3; c++) sheet.data[i + c] = Math.min(255, Math.round(img[(sy * w + sx) * 4 + c] + 24 * (1 - a)));
  }
}

items.forEach((it, idx) => {
  const png = PNG.sync.read(fs.readFileSync(it.legacy));
  const legacy = new Uint8Array(png.data);
  for (let i = 0; i < legacy.length; i += 4) { const a = legacy[i + 3] / 255; legacy[i] = Math.round(legacy[i] * a); legacy[i + 1] = Math.round(legacy[i + 1] * a); legacy[i + 2] = Math.round(legacy[i + 2] * a); }
  const file = path.join(svgRoot, it.id.replace(/\//g, path.sep) + '.svg');
  let rendered;
  try { rendered = renderSvg(file, it.w, it.h); } catch (e) { console.log(it.id + '\tSVG ERROR ' + e.message); rendered = new Uint8Array(it.w * it.h * 4); }
  const overlay = new Uint8Array(it.w * it.h * 4);
  let err = 0;
  for (let i = 0; i < it.w * it.h; i++) {
    const la = legacy[i * 4 + 3] / 255, sa = rendered[i * 4 + 3] / 255;
    const l = Math.max(legacy[i * 4], legacy[i * 4 + 1], legacy[i * 4 + 2]);
    const s = Math.max(rendered[i * 4], rendered[i * 4 + 1], rendered[i * 4 + 2]);
    overlay[i * 4] = l; overlay[i * 4 + 1] = s; overlay[i * 4 + 2] = s; overlay[i * 4 + 3] = 255;
    for (let c = 0; c < 3; c++) err += Math.abs(legacy[i * 4 + c] - rendered[i * 4 + c]);
    err += Math.abs(legacy[i * 4 + 3] - rendered[i * 4 + 3]);
  }
  err /= it.w * it.h * 4;
  const y0 = idx * rowH + 4;
  const z = Math.max(1, Math.floor(cell / Math.max(it.w, it.h)));
  put(legacy, it.w, it.h, gap, y0, z);
  put(rendered, it.w, it.h, gap + (cell + gap), y0, z);
  put(overlay, it.w, it.h, gap + 2 * (cell + gap), y0, z);
  const big = renderSvg(file, it.w * z, it.h * z);
  put(big, it.w * z, it.h * z, gap + 3 * (cell + gap), y0, 1);
  console.log(it.id + '\t' + err.toFixed(1));
});
fs.writeFileSync(args.out, PNG.sync.write(sheet));
