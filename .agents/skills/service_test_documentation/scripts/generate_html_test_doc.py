import os
import glob
import re

def parse_markdown_table(markdown_table):
    lines = markdown_table.strip().split('\n')
    if not lines:
        return []
    
    # Parse headers
    headers = [cell.strip() for cell in lines[0].split('|')[1:-1]]
    
    # Skip alignment row (lines[1])
    
    # Parse rows
    rows = []
    for line in lines[2:]:
        if not line.strip():
            continue
        row = [cell.strip() for cell in line.split('|')[1:-1]]
        rows.append(row)
        
    return headers, rows

def generate_html():
    script_dir = os.path.dirname(os.path.abspath(__file__))
    project_root = os.path.abspath(os.path.join(script_dir, "..", "..", "..", ".."))
    doc_dir = os.path.join(project_root, "docs", "unit_test_docs")
    html_out_dir = os.path.join(doc_dir, "html_test_docs")
    if not os.path.exists(html_out_dir):
        os.makedirs(html_out_dir)
        
    md_files = glob.glob(os.path.join(doc_dir, "*_TestDoc.md"))
    
    for filepath in md_files:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
            
        filename = os.path.basename(filepath)
        html_filename = filename.replace('.md', '.html')
        html_filepath = os.path.join(html_out_dir, html_filename)
        
        sections = re.split(r'(## Function: `[^`]+`)', content)
        
        html_content = """<html>
<head>
<meta charset="utf-8">
<style>
    body { font-family: Tahoma, sans-serif; font-size: 8pt; }
    table { border-collapse: collapse; width: 100%; margin-bottom: 20px; font-size: 8pt; }
    th, td { border: 1px solid #ccc; padding: 4px; }
    .header-dark { background-color: #000080; color: white; font-weight: bold; }
    .col-category { background-color: #000080; color: white; font-weight: bold; border-top: none; border-bottom: none; border-left: 1px solid black; border-right: 1px solid black; }
    .empty-th { background-color: #000080; border-top: 1px solid black; border-bottom: 1px solid black; border-left: none; border-right: none; }
    .empty-category { background-color: #000080; border-top: none; border-bottom: none; border-left: 1px solid black; border-right: 1px solid black; }
    .subcat-detail { border-top: 1px solid black; border-bottom: 1px solid black; border-left: none; border-right: none; }
    .subcat-left { border-left: 1px solid black; font-weight: bold; }
    .detail-right { border-right: 1px solid black; }
    .o-mark { font-size: 11pt; font-weight: bold; text-align: center; border: 1px solid black; }
    .utc-header { background-color: #000080; color: white; font-weight: bold; text-align: center; writing-mode: vertical-rl; border: 1px solid black; }
    .vertical-text { text-align: center; writing-mode: vertical-rl; border: 1px solid black; }
    .normal-cell { border: 1px solid black; }
    .summary-table th, .summary-table td { border: 1px solid black; }
</style>
</head>
<body>
"""
        
        if len(sections) > 0 and not sections[0].startswith('## Function:'):
            # There might be some leading text
            pass
            
        for i in range(1, len(sections), 2):
            func_header = sections[i]
            body = sections[i+1]
            
            func_name_match = re.search(r'## Function: `([^`]+)`', func_header)
            func_name = func_name_match.group(1) if func_name_match else "Unknown"
            
            html_content += f"<h3>Function: {func_name}</h3>\n"
            
            # Extract HTML summary table
            summary_table_match = re.search(r'<table.*?>.*?</table>', body, re.DOTALL)
            if summary_table_match:
                summary_table = summary_table_match.group(0)
                # Ensure the summary table has the right styling class
                summary_table = summary_table.replace('<table', '<table class="summary-table"')
                html_content += summary_table + "<br/>\n"
                
            # Extract Markdown matrix
            matrix_match = re.search(r'(\| Category \| Sub-category \|.*?\n(?:\|.*?\|\n)+)', body)
            if matrix_match:
                matrix_md = matrix_match.group(1)
                headers, rows = parse_markdown_table(matrix_md)
                
                html_content += "<table>\n"
                
                # Header row
                html_content += "  <tr>\n"
                for idx, h in enumerate(headers):
                    if idx < 4:
                        html_content += '    <th class="empty-th"></th>\n'
                    else:
                        html_content += f'    <th class="utc-header">{h.strip()}</th>\n'
                html_content += "  </tr>\n"
                
                # Body rows
                for row in rows:
                    html_content += "  <tr>\n"
                    
                    # Track row subcategory for vertical text logic
                    row_subcat = ""
                    if len(row) > 1:
                        row_subcat = row[1].replace('**', '').strip()
                        
                    for idx, cell in enumerate(row):
                        cell_clean = cell.replace('**', '').strip()
                        
                        if idx == 0:
                            if cell_clean in ['Condition', 'Confirm', 'Result']:
                                html_content += f'    <td class="col-category">{cell_clean}</td>\n'
                            else:
                                html_content += f'    <td class="empty-category"></td>\n'
                        elif idx == 1:
                            html_content += f'    <td class="subcat-detail subcat-left">{cell_clean}</td>\n'
                        elif idx == 2:
                            html_content += f'    <td class="subcat-detail"></td>\n'
                        elif idx == 3:
                            detail1_content = row[2].replace('**', '').strip() if len(row) > 2 else ""
                            html_content += f'    <td class="subcat-detail detail-right" style="text-align: right;">{detail1_content}</td>\n'
                        else:
                            # idx >= 4
                            if cell_clean == 'O':
                                html_content += f'    <td class="o-mark">O</td>\n'
                            elif row_subcat in ['Executed Date', 'Defect ID']:
                                html_content += f'    <td class="vertical-text">{cell_clean}</td>\n'
                            else:
                                html_content += f'    <td class="normal-cell">{cell_clean}</td>\n'
                    html_content += "  </tr>\n"
                
                html_content += "</table>\n"
                
        html_content += "</body></html>"
        
        with open(html_filepath, 'w', encoding='utf-8') as f:
            f.write(html_content)
            
    print(f"Generated HTML test docs in {html_out_dir}")

if __name__ == '__main__':
    generate_html()
