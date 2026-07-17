import os
import glob
import re

doc_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
files = glob.glob(os.path.join(doc_dir, "*.md"))

output = []
output.append("| No | Function code | Passed | Failed | Untested | N | A | B | Total Test Cases |")
output.append("|---|---|---|---|---|---|---|---|---|")

no = 1

for file in files:
    with open(file, 'r', encoding='utf-8') as f:
        content = f.read()
        
        # Split by function
        functions = content.split("## Function: `")
        for func_block in functions[1:]:
            func_name = func_block.split("`")[0].strip()
            
            # Find the summary table rows
            # We are looking for the last <tr> before </table>
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
                    else:
                        pass # probably not the right table or row

print("\n".join(output))
