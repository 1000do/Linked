const fs = require('fs');
const path = require('path');

const dirs = [
    'diagrams/sequence/cart',
    'diagrams/sequence/chat',
    'diagrams/sequence/manage_chat',
    'diagrams/sequence/manage_instructor_applications',
    'diagrams/sequence/notification'
];

function processDir(dir) {
    if (!fs.existsSync(dir)) return;
    const files = fs.readdirSync(dir);
    for (const file of files) {
        const fullPath = path.join(dir, file);
        if (fs.statSync(fullPath).isDirectory()) {
            processDir(fullPath);
        } else if (fullPath.endsWith('.plantuml')) {
            processFile(fullPath);
        }
    }
}

function processFile(filePath) {
    let content = fs.readFileSync(filePath, 'utf8');
    let changed = false;

    const pRegex = /participant\s+":(?![I])([A-Za-z0-9_]+(Service|Repository))"/g;
    if (pRegex.test(content)) {
        content = content.replace(pRegex, 'participant ":I$1"');
        changed = true;
    }
    
    const lines = content.split('\n');
    let newLines = [];
    let insideUnauthorized = false;
    let addedErrorPage = false;
    let fileChanged = false;
    
    for (let i = 0; i < lines.length; i++) {
        let line = lines[i];
        
        if (/be_ctrl\s*-->>\s*fe_ctrl.*?(403|401|Forbidden|Unauthorized)/i.test(line) || /alt.*?Unauthorized/i.test(line) || /else.*?Unauthorized/i.test(line) || /else.*?403/i.test(line)) {
            insideUnauthorized = true;
        }
        if (/(else|end)/i.test(line.trim()) && !/Unauthorized/i.test(line) && !/403/i.test(line)) {
             insideUnauthorized = false;
        }
        
        if (insideUnauthorized || /Return.*?403/.test(line)) {
            let feCtrlToViewMatch = line.match(/^(\s*)fe_ctrl\s*-->>\s*view.*?Return.*/i);
            if (feCtrlToViewMatch) {
                if (i + 1 < lines.length && /view\s*-->>\s*act/i.test(lines[i+1])) {
                    let indent = feCtrlToViewMatch[1];
                    let actLineMatch = lines[i+1].match(/view\s*-->>\s*act.*?:\s*(Display.*)/i);
                    if (actLineMatch) {
                        let displayMsg = actLineMatch[1];
                        newLines.push(indent + "fe_ctrl-->>error_page++: Redirect to ErrorPage");
                        newLines.push(indent + "error_page-->>act--: " + displayMsg);
                        fileChanged = true;
                        addedErrorPage = true;
                        i++; 
                        continue;
                    }
                }
            }
        }
        newLines.push(line);
    }
    
    content = newLines.join('\n');
    
    if (addedErrorPage && !content.includes('participant "ErrorPage"')) {
        content = content.replace(/(participant\s+".*?"\s+as\s+view.*?\r?\n)/, '$1participant "ErrorPage" as error_page\n');
        fileChanged = true;
    }
    
    if (changed || fileChanged) {
        fs.writeFileSync(filePath, content, 'utf8');
        console.log('Updated: ' + filePath);
    }
}

dirs.forEach(processDir);
