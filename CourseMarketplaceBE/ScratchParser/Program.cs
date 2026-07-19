using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace ScratchParser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: dotnet run <md_path> <cs_tests_path> <html_report_path> [user_initials]");
                return;
            }

            string mdPath = args[0];
            string csTestsPath = args[1];
            string htmlReportPath = args[2];
            string userInitials = args.Length > 3 ? args[3] : "TaiTP";

            if (!File.Exists(mdPath) || !File.Exists(csTestsPath) || !File.Exists(htmlReportPath))
            {
                Console.WriteLine("One or more files do not exist.");
                return;
            }

            string mdContent = File.ReadAllText(mdPath);
            string csContent = File.ReadAllText(csTestsPath);
            string htmlContent = File.ReadAllText(htmlReportPath);

            var testMethodMatches = Regex.Matches(csContent, @"\[(?:Fact|Theory|TestMethod|Test)\]", RegexOptions.IgnoreCase);
            int testMethodsCount = testMethodMatches.Count;
            if (testMethodsCount == 0) testMethodsCount = 1;

            int totalCoveredBranches = 0;
            var coverableLinesData = new Dictionary<int, int>();

            var branchMatches = Regex.Matches(htmlContent, @"<tr class=""coverableline""[^>]*title=""([^""]+)""[^>]*>.*?<a id=""file\d+_line(\d+)""", RegexOptions.Singleline);
            foreach (Match match in branchMatches)
            {
                string title = match.Groups[1].Value;
                int lineNum = int.Parse(match.Groups[2].Value);

                var branchDetailMatch = Regex.Match(title, @"(\d+) of (\d+) branches");
                int uncovered = 0;
                if (branchDetailMatch.Success)
                {
                    int covered = int.Parse(branchDetailMatch.Groups[1].Value);
                    int total = int.Parse(branchDetailMatch.Groups[2].Value);
                    uncovered = total - covered;
                    totalCoveredBranches += covered;
                }
                
                coverableLinesData[lineNum] = uncovered;
            }

            if (totalCoveredBranches == 0) totalCoveredBranches = 1;

            var metricsMatches = Regex.Matches(htmlContent, @"<tr><td title=""([^""]+)""><a href=""#file\d+_line(\d+)""");
            var allMethods = new List<(int StartLine, string FullMethodName)>();
            foreach (Match match in metricsMatches)
            {
                allMethods.Add((int.Parse(match.Groups[2].Value), match.Groups[1].Value));
            }
            allMethods.Sort((a, b) => a.StartLine.CompareTo(b.StartLine));

            var sections = Regex.Split(mdContent, @"(## Function: `[^`]+`)");
            string newMdContent = sections[0];

            for (int i = 1; i < sections.Length; i += 2)
            {
                string header = sections[i];
                string body = sections[i + 1];

                var funcMatch = Regex.Match(header, @"## Function: `([^`]+)`");
                if (!funcMatch.Success)
                {
                    newMdContent += header + body;
                    continue;
                }

                string func = funcMatch.Groups[1].Value;
                int loc = 0;
                int uncoveredBranches = 0;

                for (int j = 0; j < allMethods.Count; j++)
                {
                    var m = allMethods[j];
                    bool isMatch = false;

                    if (m.FullMethodName.StartsWith(func + "(") ||
                        m.FullMethodName.StartsWith(func + "<") ||
                        m.FullMethodName.Contains("." + func + "(") ||
                        m.FullMethodName == func ||
                        m.FullMethodName.Contains($"&lt;{func}&gt;") ||
                        m.FullMethodName.StartsWith($"&lt;{func}("))
                    {
                        isMatch = true;
                    }

                    if (isMatch)
                    {
                        int blockStart = m.StartLine;
                        int blockEnd = j + 1 < allMethods.Count ? allMethods[j + 1].StartLine - 1 : int.MaxValue;

                        loc += coverableLinesData.Keys.Count(k => k >= blockStart && k <= blockEnd);
                        foreach (var kvp in coverableLinesData)
                        {
                            if (kvp.Key >= blockStart && kvp.Key <= blockEnd)
                            {
                                uncoveredBranches += kvp.Value;
                            }
                        }
                    }
                }

                int untested = (int)Math.Round(uncoveredBranches * ((double)testMethodsCount / totalCoveredBranches));

                string testReq = $"Tests for {func}";
                var testReqMatch = Regex.Match(body, @"<td colspan=""2""><strong>Test requirement</strong></td>\s*<td colspan=""6"">(.*?)</td>", RegexOptions.Singleline);
                if (testReqMatch.Success) testReq = testReqMatch.Groups[1].Value.Trim();

                int pCount = 0, fCount = 0, nCount = 0, aCount = 0, bCount = 0;

                var resultLineMatch = Regex.Match(body, @"\|\s*\*\*Result\*\*.*?\n");
                if (resultLineMatch.Success)
                {
                    var cells = resultLineMatch.Value.Split('|').Skip(5).SkipLast(1).ToList();
                    nCount = cells.Count(c => c.ToUpper().Contains("N"));
                    aCount = cells.Count(c => c.ToUpper().Contains("A"));
                    bCount = cells.Count(c => c.ToUpper().Contains("B"));
                }

                var pfLineMatch = Regex.Match(body, @"\|\s*\|\s*\*\*Passed/Failed\*\*.*?\n");
                if (pfLineMatch.Success)
                {
                    var cells = pfLineMatch.Value.Split('|').Skip(5).SkipLast(1).ToList();
                    pCount = cells.Count(c => c.ToUpper().Contains("P"));
                    fCount = cells.Count(c => c.ToUpper().Contains("F"));
                }

                int totalTestCases = pCount + fCount + untested;

                string htmlTable = $@"<table border=""1"" width=""100%"" style=""border-collapse: collapse; text-align: left;"">
  <tr>
    <td colspan=""2""><strong>Function Code</strong></td>
    <td colspan=""3"">{func}</td>
    <td colspan=""6""><strong>Function Name</strong></td>
    <td colspan=""9"">{func}</td>
  </tr>
  <tr>
    <td colspan=""2""><strong>Created By</strong></td>
    <td colspan=""3"">{userInitials}</td>
    <td colspan=""6""><strong>Executed By</strong></td>
    <td colspan=""9"">{userInitials}</td>
  </tr>
  <tr>
    <td colspan=""2""><strong>Lines of code</strong></td>
    <td colspan=""3"">{loc}</td>
    <td colspan=""6""><strong>Lack of test cases</strong></td>
    <td colspan=""9"">=IF(Functions!E6<>""N/A"",SUM(C4*Functions!E6/1000,-O7),""N/A"")</td>
  </tr>
  <tr>
    <td colspan=""2""><strong>Test requirement</strong></td>
    <td colspan=""18"">{testReq}</td>
  </tr>
  <tr>
    <th colspan=""2"" style=""text-align: center;"">Passed</th>
    <th colspan=""3"" style=""text-align: center;"">Failed</th>
    <th colspan=""6"" style=""text-align: center;"">Untested</th>
    <th colspan=""3"" style=""text-align: center;"">N/A/B</th>
    <th colspan=""6"" style=""text-align: center;"">Total Test Cases</th>
  </tr>
  <tr>
    <td colspan=""2"" style=""text-align: center;"">{pCount}</td>
    <td colspan=""3"" style=""text-align: center;"">{fCount}</td>
    <td colspan=""6"" style=""text-align: center;"">{untested}</td>
    <td colspan=""1"" style=""text-align: center;"">{nCount}</td>
    <td colspan=""1"" style=""text-align: center;"">{aCount}</td>
    <td colspan=""1"" style=""text-align: center;"">{bCount}</td>
    <td colspan=""6"" style=""text-align: center;"">{totalTestCases}</td>
  </tr>
</table>";

                body = Regex.Replace(body, @"<table.*?>.*?</table>\s*", "", RegexOptions.Singleline);
                newMdContent += header + "\n" + htmlTable + "\n\n" + body.TrimStart();
            }

            File.WriteAllText(mdPath, newMdContent);
            Console.WriteLine($"Successfully processed {mdPath}");
        }
    }
}
