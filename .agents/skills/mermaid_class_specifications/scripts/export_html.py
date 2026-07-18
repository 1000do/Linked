import re

import sys

md_file = sys.argv[1] if len(sys.argv) > 1 else r"class_specifications.md"
html_file = sys.argv[2] if len(sys.argv) > 2 else r"class_specifications_exported.html"

html_header = """<html><head><style>
body { font-family: 'Times New Roman', serif; font-size: 12pt; margin: 20px; }
table { width: 100%; border-collapse: collapse; margin-bottom: 30px; }
th, td { border: 1px solid black; padding: 5px; text-align: left; vertical-align: top; }
th { background-color: #fce4d6; }
h2 { font-weight: bold; font-size: 14pt; margin-top: 30px; border-bottom: 2px solid #000; padding-bottom: 5px;}
h3 { font-style: italic; margin-bottom: 5px; font-weight: bold; font-size: 12pt; }
p.desc { margin-top: 0; margin-bottom: 5px; font-style: italic; color: blue; }
</style></head><body style="font-family: 'Times New Roman', serif; font-size: 12pt;">
"""

html_footer = """</body></html>"""

with open(md_file, "r", encoding="utf-8") as f:
    lines = f.readlines()

output = [html_header]

global_class_idx = 1
in_table = False

for line in lines:
    line = line.strip()
    
    if line.startswith("## Layer:"):
        continue
        
    if line.startswith("### "):
        # e.g., ### 1. ICourseModerationService Interface
        # we want to extract "ICourseModerationService Interface"
        match = re.search(r'### \d+\.\s*(.+)', line)
        if match:
            class_name_with_type = match.group(1)
            output.append(f'<h3 style="font-style: normal; margin-top: 20px; margin-bottom: 5px; font-weight: bold; font-size: 12pt; font-family: \'Times New Roman\', serif;">4.{global_class_idx} {class_name_with_type}</h3>')
            global_class_idx += 1
        continue
        
    if line.startswith("|") and not line.startswith("|---"):
        parts = [p.strip() for p in line.split("|")][1:-1] # trim empty ends
        
        if parts[0] == "No" and parts[1] == "Method / Property" and parts[2] == "Description":
            if in_table:
                output.append("</table><br/>\n")
            in_table = True
            output.append('<table width="100%" style="width: 100%; border-collapse: collapse; margin-bottom: 30px; font-family: \'Times New Roman\', serif; font-size: 12pt; table-layout: fixed;">')
            output.append('<tr><th width="5%" style="width: 5%; border: 1px solid black; padding: 5px; text-align: left; vertical-align: top; background-color: #fce4d6;">No</th><th width="60%" style="width: 60%; border: 1px solid black; padding: 5px; text-align: left; vertical-align: top; background-color: #fce4d6;">Method / Property</th><th width="35%" style="width: 35%; border: 1px solid black; padding: 5px; text-align: left; vertical-align: top; background-color: #fce4d6;">Description</th></tr>')
            continue
            
        if in_table and len(parts) >= 3:
            col1 = parts[0]
            col2 = parts[1]
            col3 = parts[2]
            output.append(f'<tr><td width="5%" style="width: 5%; border: 1px solid black; padding: 5px; text-align: left; vertical-align: top; word-break: break-word;">{col1}</td><td width="60%" style="width: 60%; border: 1px solid black; padding: 5px; text-align: left; vertical-align: top; word-break: break-word;">{col2}</td><td width="35%" style="width: 35%; border: 1px solid black; padding: 5px; text-align: left; vertical-align: top; word-break: break-word;">{col3}</td></tr>')
            
    elif line == "" and in_table:
        output.append("</table><br/>\n")
        in_table = False

if in_table:
    output.append("</table><br/>\n")

output.append(html_footer)

with open(html_file, "w", encoding="utf-8") as f:
    f.write("\n".join(output))

print(f"HTML exported successfully to {html_file}")
