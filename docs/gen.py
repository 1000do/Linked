import re

input_file = r'd:\Linked\docs\use_case_description.html'
output_file = r'd:\Linked\docs\use_case_diagrams.html'

with open(input_file, 'r', encoding='utf-8') as f:
    content = f.read()

# Extract use cases: <tr><td class="center-col">UC-01</td><td>Register</td>
matches = re.findall(r'<tr><td class="center-col">UC-\d+</td><td>(.*?)</td>', content)

html_template = """<!DOCTYPE html>
<html>
<head>
<meta charset="UTF-8">
<title>Use Case Diagrams</title>
<style>
    body {
        font-family: 'Times New Roman', serif;
    }
    h3 {
        font-size: 13pt;
        font-weight: bold;
        color: black;
        margin-top: 20px;
        margin-bottom: 10px;
    }
    .subsection {
        font-size: 12pt;
        font-weight: normal;
        color: black;
        margin-top: 10px;
        margin-bottom: 5px;
    }
    .figure-caption {
        font-size: 13pt;
        text-decoration: underline;
        font-style: italic;
        text-align: center;
        color: #1155cc;
        margin-top: 5px;
        margin-bottom: 15px;
    }
</style>
</head>
<body>
"""

with open(output_file, 'w', encoding='utf-8') as f:
    f.write(html_template)
    for i, name in enumerate(matches, start=1):
        h3_title = f"3.{i} {name}"
        section_html = f"""
<h3>{h3_title}</h3>
<div class="subsection">a. Class Diagram</div>
<div class="figure-caption">Figure. Class Diagram of {name}</div>
<div class="subsection">b. Sequence Diagram</div>
<div class="figure-caption">Figure. Sequence Diagram of {name}</div>
"""
        f.write(section_html)
    
    f.write("</body>\n</html>")

print(f"Generated {len(matches)} use case diagrams in {output_file}")
