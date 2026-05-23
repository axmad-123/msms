import fs from 'fs';
const wrong = '</' + 'motion' + '>';
const right = '</' + 'div' + '>';
const file = 'src/components/layout/DashboardLayout.tsx';
let t = fs.readFileSync(file, 'utf8');
t = t.split(wrong).join(right);
fs.writeFileSync(file, t);
console.log('fixed', (t.match(/<\/div>/g) || []).length);
