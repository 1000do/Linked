$input_file = "d:\Linked\docs\use_case_description.html"
$output_file = "d:\Linked\docs\use_case_diagrams.html"

$content = Get-Content $input_file -Raw
$pattern = '<tr><td class="center-col">UC-\d+</td><td>(.*?)</td>'
$matches = [regex]::Matches($content, $pattern)

$html_template = @"
<!DOCTYPE html>
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
</style>
</head>
<body>
"@

Set-Content -Path $output_file -Value $html_template -Encoding UTF8

$i = 1
$fig_num = 6
foreach ($match in $matches) {
    $name = $match.Groups[1].Value
    $h3_title = "3.$i $name"
    $fig_class = $fig_num
    $fig_seq = $fig_num + 1
    $fig_num += 2

    $section_html = @"
<h3>$h3_title</h3>
<p style="font-size: 12pt; font-weight: normal; color: black; margin-top: 10px; margin-bottom: 5px; mso-outline-level: body-text;">a. Class Diagram</p>
<p style="font-size: 13pt; text-decoration: underline; font-style: italic; text-align: center; color: #1155cc; margin-top: 5px; margin-bottom: 15px; mso-outline-level: body-text;">Figure ${fig_class}. Class Diagram of $name</p>
<p style="font-size: 12pt; font-weight: normal; color: black; margin-top: 10px; margin-bottom: 5px; mso-outline-level: body-text;">b. Sequence Diagram</p>
<p style="font-size: 13pt; text-decoration: underline; font-style: italic; text-align: center; color: #1155cc; margin-top: 5px; margin-bottom: 15px; mso-outline-level: body-text;">Figure ${fig_seq}. Sequence Diagram of $name</p>
"@
    Add-Content -Path $output_file -Value $section_html -Encoding UTF8
    $i++
}

Add-Content -Path $output_file -Value "</body>`n</html>" -Encoding UTF8
Write-Host "Done"
