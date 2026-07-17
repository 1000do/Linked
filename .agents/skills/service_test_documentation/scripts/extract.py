import os
import glob
import re

# Dynamically compute project root and doc directory
script_dir = os.path.dirname(os.path.abspath(__file__))
project_root = os.path.abspath(os.path.join(script_dir, "..", "..", "..", ".."))
doc_dir = os.path.join(project_root, "docs", "unit_test_docs")
files = glob.glob(os.path.join(doc_dir, "*_TestDoc.md"))
files.sort()

output = []
output.append("| No | Function code | Passed | Failed | Untested | N | A | B | Total Test Cases |")
output.append("|---|---|---|---|---|---|---|---|---|")

no = 1

for file in files:
    with open(file, 'r', encoding='utf-8') as f:
        content = f.read()
        
        functions = content.split("## Function: `")
        for func_block in functions[1:]:
            func_name = func_block.split("`")[0].strip()
            
            table_match = re.search(r'<table.*?</table>', func_block, re.DOTALL)
            if table_match:
                table_content = table_match.group(0)
                trs = re.findall(r'<tr>(.*?)</tr>', table_content, re.DOTALL)
                if len(trs) >= 2:
                    last_tr = trs[-1]
                    tds = re.findall(r'<td[^>]*>(.*?)</td>', last_tr, re.DOTALL)
                    if len(tds) == 7:
                        output.append(f"| {no} | {func_name} | {tds[0].strip()} | {tds[1].strip()} | {tds[2].strip()} | {tds[3].strip()} | {tds[4].strip()} | {tds[5].strip()} | {tds[6].strip()} |")
                        no += 1

output_file = os.path.join(doc_dir, "unit_test_summary_table.md")
with open(output_file, 'w', encoding='utf-8') as f:
    f.write("\n".join(output))

print(f"Summary table saved to {output_file}")
