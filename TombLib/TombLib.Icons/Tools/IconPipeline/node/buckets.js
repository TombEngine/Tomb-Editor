// Rasterizes every SVG into the @1x..@4x PNG buckets with resvg (works without a desktop session).
const fs=require('fs'),path=require('path');const {Resvg}=require('@resvg/resvg-js');const {PNG}=require('pngjs');
const svgRoot=path.join(__dirname,'..','..','..','Svg'),pngRoot=path.join(__dirname,'..','..','..','Png');
function walk(d){return fs.readdirSync(d,{withFileTypes:true}).flatMap(e=>e.isDirectory()?walk(path.join(d,e.name)):e.name.endsWith('.svg')?[path.join(d,e.name)]:[]);}
let n=0;
for(const f of walk(svgRoot)){const rel=path.relative(svgRoot,f).replace(/\.svg$/,'.png');const svg=fs.readFileSync(f,'utf8');
 for(const s of [1,2,3,4]){const size=16*s;const r=new Resvg(svg,{fitTo:{mode:'width',value:size}});let img=r.render();
  // fit into the square canvas: choose the limiting dimension
  const vb=svg.match(/viewBox="0 0 ([\d.]+) ([\d.]+)"/);const w=+vb[1],h=+vb[2];const scale=Math.min(size/w,size/h);
  const rr=new Resvg(svg,{fitTo:{mode:w>=h?'width':'height',value:Math.round((w>=h?w:h)*scale)}});img=rr.render();
  const out=new PNG({width:size,height:size});out.data.fill(0);const ox=Math.floor((size-img.width)/2),oy=Math.floor((size-img.height)/2);
  // resvg pixels are premultiplied: un-premultiply into the PNG
  for(let y=0;y<img.height;y++)for(let x=0;x<img.width;x++){const i=(y*img.width+x)*4,o=((y+oy)*size+x+ox)*4;const a=img.pixels[i+3];if(a===0)continue;for(let c=0;c<3;c++)out.data[o+c]=Math.min(255,Math.round(img.pixels[i+c]*255/a));out.data[o+3]=a;}
  const dest=path.join(pngRoot,'@'+s+'x',rel);fs.mkdirSync(path.dirname(dest),{recursive:true});const data=PNG.sync.write(out);for(let attempt=0;;attempt++){try{fs.writeFileSync(dest,data);break;}catch(e){if(attempt>20)throw e;Atomics.wait(new Int32Array(new SharedArrayBuffer(4)),0,0,250);}}}
 n++;}
console.log('rasterized',n);
