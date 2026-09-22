const assert = require('node:assert/strict');
const fs = require('node:fs');
const ts = require('typescript');
const vm = require('node:vm');
const path = require('node:path');

process.env.TZ = 'America/Sao_Paulo';
const source = fs.readFileSync(path.join(__dirname, '../src/app/shared/data-hora-br.pipe.ts'), 'utf8');
const compiled = ts.transpileModule(source, {
  compilerOptions: { module: ts.ModuleKind.CommonJS, experimentalDecorators: true }
}).outputText;
const context = {
  exports: {}, Date, Intl,
  require: () => ({ Pipe: () => target => target })
};
vm.runInNewContext(compiled, context);
const pipe = new context.exports.DataHoraBrPipe();
assert.equal(pipe.transform('2026-09-22T13:46:00'), '22/09/2026 10:46');
assert.equal(pipe.transform('2026-09-22T13:46:00Z'), '22/09/2026 10:46');
assert.equal(pipe.transform('2026-09-22T10:46:00-03:00'), '22/09/2026 10:46');
assert.equal(pipe.transform('2026-09-22T01:00:00'), '21/09/2026 22:00');
assert.equal(pipe.transform(null), '');
assert.equal(pipe.transform('invalido'), 'invalido');
console.log('6 verificacoes de data/hora aprovadas.');
