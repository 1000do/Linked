import sys
import os
import re

def main():
    if len(sys.argv) < 4:
        print("Usage: python generate_summary_tables.py <md_path> <cs_tests_path> <html_report_path> [user_initials]")
        sys.exit(1)
        
    md_path = sys.argv[1]
    cs_tests_path = sys.argv[2]
    html_report_path = sys.argv[3]
    user_initials = sys.argv[4] if len(sys.argv) > 4 else "AnHK"
    
    if not os.path.exists(md_path) or not os.path.exists(cs_tests_path) or not os.path.exists(html_report_path):
        print("One or more files do not exist.")
        sys.exit(1)
        
    with open(md_path, 'r', encoding='utf-8') as f:
        md_content = f.read()
        
    with open(cs_tests_path, 'r', encoding='utf-8') as f:
        cs_content = f.read()
        
    with open(html_report_path, 'r', encoding='utf-8') as f:
        html_content = f.read()
        
    # Count test methods
    test_methods_count = len(re.findall(r'\[(?:Fact|Theory|TestMethod|Test)\]', cs_content, re.IGNORECASE))
    if test_methods_count == 0:
        test_methods_count = 1 
        
    # Total covered branches across entire HTML
    total_covered_branches = 0
    coverable_lines_data = {}
    for match in re.finditer(r'<tr class="coverableline"[^>]*title="([^"]+)"[^>]*>.*?<a id="file\d+_line(\d+)">', html_content, re.DOTALL):
        title = match.group(1)
        line_num = int(match.group(2))
        
        branch_match = re.search(r'(\d+) of (\d+) branches', title)
        if branch_match:
            covered = int(branch_match.group(1))
            total = int(branch_match.group(2))
            uncovered = total - covered
            total_covered_branches += covered
        else:
            uncovered = 0
            
        coverable_lines_data[line_num] = uncovered
        
    if total_covered_branches == 0:
        total_covered_branches = 1
        
    metrics_matches = re.finditer(r'<tr><td title="([^"]+)"><a href="#file\d+_line(\d+)"', html_content)
    all_methods = []
    for match in metrics_matches:
        full_method_name = match.group(1) 
        start_line = int(match.group(2))
        all_methods.append((start_line, full_method_name))
        
    all_methods.sort(key=lambda x: x[0])
    
    sections = re.split(r'(## Function: `[^`]+`)', md_content)
    new_md_content = sections[0]
    
    for i in range(1, len(sections), 2):
        header = sections[i]
        body = sections[i+1]
        
        func_match = re.search(r'## Function: `([^`]+)`', header)
        if not func_match:
            new_md_content += header + body
            continue
            
        func = func_match.group(1)
        
        loc = 0
        uncovered_branches = 0
        for j, (start_line, full_method_name) in enumerate(all_methods):
            is_match = False
            
            if full_method_name.startswith(func + '(') or \
               full_method_name.startswith(func + '<') or \
               '.' + func + '(' in full_method_name or \
               full_method_name == func:
                is_match = True
            elif f"&lt;{func}&gt;" in full_method_name or full_method_name.startswith(f"&lt;{func}("):
                is_match = True
                
            if is_match:
                block_start = start_line
                block_end = all_methods[j+1][0] - 1 if j + 1 < len(all_methods) else float('inf')
                
                loc += sum(1 for line_num in coverable_lines_data.keys() if block_start <= line_num <= block_end)
                for line_num, unc in coverable_lines_data.items():
                    if block_start <= line_num <= block_end:
                        uncovered_branches += unc
                        
        untested = round(uncovered_branches * (test_methods_count / total_covered_branches))
        
        # Extract existing test requirement if present, or use default
        test_req_match = re.search(r'<td colspan="2"><strong>Test requirement</strong></td>\s*<td colspan="6">(.*?)</td>', body, re.DOTALL)
        test_req = test_req_match.group(1).strip() if test_req_match else f"Tests for {func}"
        
        result_line_match = re.search(r'\|\s*\*\*Result\*\*.*?\n', body)
        if result_line_match:
            result_cells = result_line_match.group(0).split('|')[5:-1]
            n_count = sum(1 for cell in result_cells if 'N' in cell.upper())
            a_count = sum(1 for cell in result_cells if 'A' in cell.upper())
            b_count = sum(1 for cell in result_cells if 'B' in cell.upper())
        else:
            n_count = a_count = b_count = 0
            
        pf_line_match = re.search(r'\|\s*\|\s*\*\*Passed/Failed\*\*.*?\n', body)
        if pf_line_match:
            pf_cells = pf_line_match.group(0).split('|')[5:-1]
            p_count = sum(1 for cell in pf_cells if 'P' in cell.upper())
            f_count = sum(1 for cell in pf_cells if 'F' in cell.upper())
        else:
            p_count = f_count = 0
        
        total_test_cases = p_count + f_count + untested
        
        html_table = f'''<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
  <tr>
    <td colspan="2"><strong>Function Code</strong></td>
    <td colspan="3">{func}</td>
    <td colspan="6"><strong>Function Name</strong></td>
    <td colspan="9">{func}</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Created By</strong></td>
    <td colspan="3">{user_initials}</td>
    <td colspan="6"><strong>Executed By</strong></td>
    <td colspan="9">{user_initials}</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Lines of code</strong></td>
    <td colspan="3">{loc}</td>
    <td colspan="6"><strong>Lack of test cases</strong></td>
    <td colspan="9">=IF(Functions!E6<>"N/A",SUM(C4*Functions!E6/1000,-O7),"N/A")</td>
  </tr>
  <tr>
    <td colspan="2"><strong>Test requirement</strong></td>
    <td colspan="18">{test_req}</td>
  </tr>
  <tr>
    <th colspan="2" style="text-align: center;">Passed</th>
    <th colspan="3" style="text-align: center;">Failed</th>
    <th colspan="6" style="text-align: center;">Untested</th>
    <th colspan="3" style="text-align: center;">N/A/B</th>
    <th colspan="6" style="text-align: center;">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan="2" style="text-align: center;">{p_count}</td>
    <td colspan="3" style="text-align: center;">{f_count}</td>
    <td colspan="6" style="text-align: center;">{untested}</td>
    <td colspan="1" style="text-align: center;">{n_count}</td>
    <td colspan="1" style="text-align: center;">{a_count}</td>
    <td colspan="1" style="text-align: center;">{b_count}</td>
    <td colspan="6" style="text-align: center;">{total_test_cases}</td>
  </tr>
</table>'''
        
        # Remove any existing table in the body if it's already there
        body = re.sub(r'<table.*?>.*?</table>\s*', '', body, flags=re.DOTALL)
        
        new_md_content += header + "\n" + html_table + "\n\n" + body.lstrip()
        
    with open(md_path, 'w', encoding='utf-8') as f:
        f.write(new_md_content)
        
    print(f"Successfully processed {md_path}")

if __name__ == '__main__':
    main()
