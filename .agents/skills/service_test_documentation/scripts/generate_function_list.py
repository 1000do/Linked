import os
import re
import glob

# Dynamically compute project root and doc directory
script_dir = os.path.dirname(os.path.abspath(__file__))
project_root = os.path.abspath(os.path.join(script_dir, "..", "..", "..", ".."))
doc_dir = os.path.join(project_root, "docs", "unit_test_docs")
md_files = glob.glob(os.path.join(doc_dir, '*_TestDoc.md'))
md_files.sort()

def summarize(precond, func_name):
    p = precond.lower()
    if not p:
        return ""
    if 'cache hit' in p or 'cache miss' in p:
        return 'Redis cache hit and miss scenarios'
    if 'repository works normally' in p and 'throws' in p:
        return 'Repository success and exception scenarios'
    if 'repository returns items' in p or 'repository returns list' in p:
        return 'Repository returns populated list, empty list, or null'
    if 'exists in db' in p or ('exists' in p and 'does not exist' in p):
        return 'Entity existence and non-existence in database'
    if 'valid request' in p and 'not found' in p:
        return 'Valid request, entity not found, and validation failure scenarios'
    if 'status rejected' in p or 'status resolved' in p:
        return 'Various report resolution statuses and exception scenarios'
    if 'course is null' in p or 'under 3 strikes' in p:
        return 'Various strike counts and instructor existence scenarios'
    if 'media embedding' in p and 'text embedding' in p:
        return 'Media and text embedding existence and repository exception scenarios'
    if 'materials list is null' in p or 'cache entry is null' in p:
        return 'Various combinations of material list, cache states, and embedding types'
    if 'moderationthreshold is null' in p:
        return 'Various ModerationThreshold JSON states (null, empty, valid, missing properties)'
    if 'integrations are null' in p or 'models already exist' in p:
        return 'Various AI integration assignment and matching scenarios'
    if len(precond) > 60:
        return f'Various validation, state, and exception scenarios for {func_name}'
    return precond

functions_data = []

for filepath in md_files:
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    filename = os.path.basename(filepath)
    class_name = filename.replace('_TestDoc.md', '')
    
    sections = re.split(r'## Function: `([^`]+)`', content)
    
    for i in range(1, len(sections), 2):
        func_name = sections[i].strip()
        body = sections[i+1]
        
        desc = ""
        html_desc_match = re.search(r'<strong>Test requirement</strong>.*?<td[^>]*>(.*?)</td>', body, re.DOTALL | re.IGNORECASE)
        if html_desc_match:
            desc = html_desc_match.group(1).strip()
        else:
            md_desc_match = re.search(r'\-\s*\*\*Test requirement:\*\*\s*(.+)', body)
            if md_desc_match:
                desc = md_desc_match.group(1).strip()
        
        pre_conditions = []
        in_precondition = False
        for line in body.split('\n'):
            if '| **Condition** | **Precondition** |' in line:
                in_precondition = True
                continue
            if in_precondition:
                if '| **Input** |' in line or '| **Confirm** |' in line or '| **Exception** |' in line or '| **State** |' in line or '| **Result** |' in line:
                    in_precondition = False
                    continue
                
                parts = [p.strip() for p in line.split('|')]
                if len(parts) > 3:
                    detail_1 = parts[3]
                    if detail_1 and detail_1 != 'Detail 1':
                        pre_conditions.append(detail_1.replace('`', ''))
                        
        raw_pre_cond_str = ', '.join(pre_conditions)
        summarized_precond = summarize(raw_pre_cond_str, func_name)
        
        functions_data.append({
            'class': class_name,
            'func_name': func_name,
            'desc': desc,
            'pre_cond': summarized_precond
        })

md_table = []
md_table.append("| No | Requirement Name | Class Name | Function Name | Function Code | Sheet Name | Description | Pre-Condition |")
md_table.append("|:---|:---|:---|:---|:---|:---|:---|:---|")

for idx, d in enumerate(functions_data):
    row = f"| {idx+1} | | {d['class']} | {d['func_name']} | {d['func_name']} | {d['func_name']} | {d['desc']} | {d['pre_cond']} |"
    md_table.append(row)

output_file = os.path.join(doc_dir, 'FunctionList.md')
with open(output_file, 'w', encoding='utf-8') as f:
    f.write('\n'.join(md_table))

print(f"Table successfully generated with {len(functions_data)} functions and saved to {output_file}")
