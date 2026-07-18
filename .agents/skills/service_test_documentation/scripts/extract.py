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
output.append("<html><head><meta charset='utf-8'><style>")
output.append("body { font-family: Tahoma, sans-serif; font-size: 10pt; }")
output.append("table { border-collapse: collapse; width: 100%; margin-bottom: 20px; font-size: 10pt; }")
output.append("th, td { border: 1px solid #ccc; padding: 4px; }")
output.append("th { background-color: #000080; color: white; font-weight: bold; text-align: center; }")
output.append(".sub-total { background-color: #000080; color: white; font-weight: bold; }")
output.append(".sub-total td { text-align: center; }")
output.append("</style></head><body>")
output.append("<table>")
output.append("<tr><th>No</th><th>Function code</th><th>Passed</th><th>Failed</th><th>Untested</th><th>N</th><th>A</th><th>B</th><th>Total Test Cases</th></tr>")

no = 1
total_passed = 0
total_failed = 0
total_untested = 0
total_n = 0
total_a = 0
total_b = 0
total_all = 0

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
                        passed = int(tds[0].strip() or 0)
                        failed = int(tds[1].strip() or 0)
                        untested = int(tds[2].strip() or 0)
                        n_val = int(tds[3].strip() or 0)
                        a_val = int(tds[4].strip() or 0)
                        b_val = int(tds[5].strip() or 0)
                        t_all = int(tds[6].strip() or 0)
                        
                        total_passed += passed
                        total_failed += failed
                        total_untested += untested
                        total_n += n_val
                        total_a += a_val
                        total_b += b_val
                        total_all += t_all
                        
                        output.append(f"<tr><td style='text-align:center;'>{no}</td>")
                        output.append(f"<td>{func_name}</td>")
                        output.append(f"<td style='text-align:center;'>{passed}</td>")
                        output.append(f"<td style='text-align:center;'>{failed}</td>")
                        output.append(f"<td style='text-align:center;'>{untested}</td>")
                        output.append(f"<td style='text-align:center;'>{n_val}</td>")
                        output.append(f"<td style='text-align:center;'>{a_val}</td>")
                        output.append(f"<td style='text-align:center;'>{b_val}</td>")
                        output.append(f"<td style='text-align:center;'>{t_all}</td></tr>")
                        
                        no += 1

output.append(f"<tr class='sub-total'><td colspan='2' style='text-align:center;'>Sub total</td>")
output.append(f"<td>{total_passed}</td><td>{total_failed}</td><td>{total_untested}</td>")
output.append(f"<td>{total_n}</td><td>{total_a}</td><td>{total_b}</td><td>{total_all}</td></tr>")
output.append("</table></body></html>")

output_file = os.path.join(doc_dir, "unit_test_summary_table.html")
with open(output_file, 'w', encoding='utf-8') as f:
    f.write("\n".join(output))

print(f"Summary table saved to {output_file}")
