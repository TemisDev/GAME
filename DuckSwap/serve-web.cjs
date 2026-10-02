const http = require('http');
const fs = require('fs');
const path = require('path');
const root = path.resolve(__dirname, 'PatitosWeb');
const types = {'.html':'text/html','.js':'application/javascript','.json':'application/json','.wasm':'application/wasm','.png':'image/png','.css':'text/css'};
http.createServer((req,res)=>{
  const pathname = decodeURIComponent(new URL(req.url, 'http://localhost').pathname);
  const file = path.resolve(root, '.' + (pathname === '/' ? '/index.html' : pathname));
  if(!file.startsWith(root + path.sep)){res.writeHead(403);return res.end();}
  fs.stat(file,(err,stat)=>{
    if(err || !stat.isFile()){res.writeHead(404);return res.end();}
    res.writeHead(200,{'Content-Type':types[path.extname(file)] || 'application/octet-stream','Content-Length':stat.size});
    fs.createReadStream(file).pipe(res);
  });
}).listen(8765,'127.0.0.1',()=>console.log('Preview http://127.0.0.1:8765'));
