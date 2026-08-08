using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Net;

namespace HtmlProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            string htmlPath = @"d:\Linked\docs\rp6\Report6_20_07_2026.html";
            string mdPath = @"d:\Linked\docs\rp6\output.md";
            string skippedPath = @"d:\Linked\docs\rp6\skipped.txt";

            var doc = new HtmlDocument();
            doc.Load(htmlPath, Encoding.UTF8);

            var sb = new StringBuilder();
            var skippedSb = new StringBuilder();
            
            int figureCount = 1;
            int tableCount = 1;

            var usedCaptions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            string CleanText(string text)
            {
                if (string.IsNullOrWhiteSpace(text)) return "";
                text = WebUtility.HtmlDecode(text);
                text = text.Replace("\u200B", "");
                text = Regex.Replace(text, @"\s+", " ").Trim();
                return text;
            }

            string GenerateUniqueCaption(string baseCaption)
            {
                baseCaption = CleanText(baseCaption);
                if (string.IsNullOrWhiteSpace(baseCaption)) baseCaption = "Untitled";
                
                string cap = baseCaption;
                if (!usedCaptions.ContainsKey(cap.ToLower()))
                {
                    usedCaptions[cap.ToLower()] = 1;
                    return cap;
                }
                usedCaptions[cap.ToLower()]++;
                return $"{cap} ({usedCaptions[cap.ToLower()]})";
            }

            string currentHeading = "";
            string currentFeatureHeading = "";
            string currentStep = "";
            bool passedTOC = false;

            var nodes = doc.DocumentNode.Descendants().ToList();
            
            var featureImageCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            
            string tempHeading = "";
            string tempFeatureHeading = "";
            bool tempPassedTOC = false;
            foreach(var node in nodes) {
                if (node.Name.StartsWith("h") && node.Name.Length == 2 && char.IsDigit(node.Name[1]))
                {
                    string text = CleanText(node.InnerText);
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (text.ToLower().Contains("mục lục") || text.ToLower().Contains("table of contents")) tempPassedTOC = false;
                        else if (text.ToLower().Contains("1. overall description") || text.ToLower().Contains("1. introduction") || text.StartsWith("1.")) tempPassedTOC = true;
                        
                        if (tempPassedTOC) {
                            string cleanText = Regex.Replace(text, @"^[\d\.]+\s*", "");
                            if (!cleanText.StartsWith("Figure", StringComparison.OrdinalIgnoreCase) &&
                                !cleanText.StartsWith("Hình", StringComparison.OrdinalIgnoreCase) &&
                                !cleanText.StartsWith("Table", StringComparison.OrdinalIgnoreCase) &&
                                !cleanText.StartsWith("Bảng", StringComparison.OrdinalIgnoreCase))
                            {
                                tempHeading = cleanText;
                                if ((node.Name == "h2" || node.Name == "h3" || node.Name == "h4") && 
                                    !cleanText.ToLower().Contains("diagram") && 
                                    !cleanText.ToLower().Contains("biểu đồ"))
                                {
                                    tempFeatureHeading = cleanText;
                                    if (!featureImageCounts.ContainsKey(tempFeatureHeading)) featureImageCounts[tempFeatureHeading] = 0;
                                }
                            }
                        }
                    }
                }
                else if (node.Name == "img" && tempPassedTOC && !string.IsNullOrEmpty(tempFeatureHeading))
                {
                    var parentP = node.Ancestors("p").FirstOrDefault();
                    bool hasText = parentP != null && parentP.Descendants("#text").Any(n => !string.IsNullOrWhiteSpace(CleanText(n.InnerText)));
                    if (!hasText) 
                    {
                        HtmlNode next = parentP != null ? parentP.NextSibling : node.NextSibling;
                        bool hasCaption = false;
                        for (int i=0; i<5 && next != null; i++) {
                            if (next.NodeType == HtmlNodeType.Element && (next.Name == "p" || next.Name.StartsWith("h") || next.Name == "div")) {
                                string nextText = CleanText(next.InnerText);
                                if (nextText.StartsWith("Figure", StringComparison.OrdinalIgnoreCase) || nextText.StartsWith("Hình", StringComparison.OrdinalIgnoreCase)) {
                                    hasCaption = true; break;
                                }
                                if (!string.IsNullOrWhiteSpace(nextText)) break; 
                            }
                            next = next.NextSibling;
                        }
                        if (!hasCaption) {
                            featureImageCounts[tempFeatureHeading]++;
                        }
                    }
                }
            }

            foreach (var node in nodes)
            {
                if (node.Name.StartsWith("h") && node.Name.Length == 2 && char.IsDigit(node.Name[1]))
                {
                    string text = CleanText(node.InnerText);
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (text.ToLower().Contains("mục lục") || text.ToLower().Contains("table of contents"))
                        {
                            passedTOC = false;
                        }
                        else if (text.ToLower().Contains("1. overall description") || text.ToLower().Contains("1. introduction") || text.StartsWith("1."))
                        {
                            passedTOC = true;
                        }
                        
                        if (passedTOC) 
                        {
                            string cleanText = Regex.Replace(text, @"^[\d\.]+\s*", "");
                            
                            if (!cleanText.StartsWith("Figure", StringComparison.OrdinalIgnoreCase) &&
                                !cleanText.StartsWith("Hình", StringComparison.OrdinalIgnoreCase) &&
                                !cleanText.StartsWith("Table", StringComparison.OrdinalIgnoreCase) &&
                                !cleanText.StartsWith("Bảng", StringComparison.OrdinalIgnoreCase))
                            {
                                currentHeading = cleanText;
                                if ((node.Name == "h2" || node.Name == "h3" || node.Name == "h4") && 
                                    !cleanText.ToLower().Contains("diagram") && 
                                    !cleanText.ToLower().Contains("biểu đồ"))
                                {
                                    currentFeatureHeading = cleanText;
                                    currentStep = ""; 
                                }
                            }
                        }
                    }
                }
                
                if (node.Name == "p" || node.Name == "li" || node.Name.StartsWith("h"))
                {
                    string text = CleanText(node.InnerText);
                    var stepMatch = Regex.Match(text, @"^(?:Step|Bước)\s*(\d+)", RegexOptions.IgnoreCase);
                    if (stepMatch.Success)
                    {
                        currentStep = $"step {stepMatch.Groups[1].Value}";
                    }
                }

                if (node.Name == "img")
                {
                    if (!passedTOC)
                    {
                        skippedSb.AppendLine($"- Image skipped (Reason: Nằm ở trang bìa hoặc mục lục)");
                        continue;
                    }

                    bool inlineWithText = false;
                    var parentP = node.Ancestors("p").FirstOrDefault();
                    if (parentP != null)
                    {
                        bool hasText = parentP.Descendants("#text").Any(n => !string.IsNullOrWhiteSpace(CleanText(n.InnerText)));
                        if (hasText)
                        {
                            string pText = CleanText(parentP.InnerText);
                            if (pText.ToLower().Contains("class diagram") || pText.ToLower().Contains("sequence diagram"))
                            {
                                inlineWithText = true;
                            }
                            else
                            {
                                skippedSb.AppendLine($"- Image skipped (Reason: Hình ảnh nằm in-line with text trong đoạn văn: '{CleanText(parentP.InnerText).Substring(0, Math.Min(50, CleanText(parentP.InnerText).Length))}...')");
                                continue;
                            }
                        }
                    }

                    HtmlNode captionNode = null;
                    if (inlineWithText)
                    {
                        captionNode = parentP;
                    }
                    else
                    {
                        HtmlNode next = parentP != null ? parentP.NextSibling : node.NextSibling;
                        for (int i=0; i<5 && next != null; i++) {
                            if (next.NodeType == HtmlNodeType.Element && (next.Name == "p" || next.Name.StartsWith("h") || next.Name == "div"))
                            {
                                string nextText = CleanText(next.InnerText);
                                if (nextText.StartsWith("Figure", StringComparison.OrdinalIgnoreCase) || 
                                    nextText.StartsWith("Hình", StringComparison.OrdinalIgnoreCase))
                                {
                                    captionNode = next;
                                    break;
                                }
                                if (!string.IsNullOrWhiteSpace(nextText)) break; 
                            }
                            next = next.NextSibling;
                        }
                    }

                    string captionText = "";
                    if (captionNode != null)
                    {
                        captionText = CleanText(captionNode.InnerText);
                        captionText = Regex.Replace(captionText, @"^(Figure|Hình)[\s\.]*(\d+)?[\.\:\-]*\s*", "", RegexOptions.IgnoreCase);
                        captionText = Regex.Replace(captionText, @"^[\d\.a-zA-Z]+\.\s*", ""); 
                        
                        if (captionText.Equals("class diagram", StringComparison.OrdinalIgnoreCase) ||
                            captionText.Equals("sequence diagram", StringComparison.OrdinalIgnoreCase) ||
                            captionText.Equals("package diagram", StringComparison.OrdinalIgnoreCase))
                        {
                            captionText = $"{captionText} of {currentFeatureHeading}";
                            captionText = GenerateUniqueCaption(captionText);
                        }
                        captionText = Regex.Replace(captionText, @"^[\d\.]+\s*", "");
                    }
                    else
                    {
                        if (currentHeading.ToLower().Contains("sequence diagram") || currentHeading.ToLower().Contains("biểu đồ tuần tự") || currentHeading.ToLower().Contains("class diagram") || currentHeading.ToLower().Contains("biểu đồ lớp") || currentHeading.ToLower().Contains("package diagram"))
                        {
                            string type = "Diagram";
                            if (currentHeading.ToLower().Contains("sequence") || currentHeading.ToLower().Contains("tuần tự")) type = "Sequence diagram";
                            else if (currentHeading.ToLower().Contains("class") || currentHeading.ToLower().Contains("lớp")) type = "Class diagram";
                            else if (currentHeading.ToLower().Contains("package")) type = "Package diagram";
                            
                            captionText = $"{type} of {currentFeatureHeading}";
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(currentStep))
                            {
                                captionText = $"{currentFeatureHeading} operation";
                            }
                            else
                            {
                                captionText = currentHeading;
                            }
                        }
                        captionText = Regex.Replace(captionText, @"^[\d\.]+\s*", "");
                        captionText = GenerateUniqueCaption(captionText);
                    }

                    string finalCaption = $"Figure {figureCount}. {captionText}";
                    figureCount++;

                    string htmlCaption = $"<h5 style=\"font-family: 'Times New Roman', serif; font-size: 12pt; font-style: italic; color: blue; text-align: center; margin-top: 10px; margin-bottom: 20px;\">{finalCaption}</h5>";

                    sb.AppendLine(node.OuterHtml);
                    sb.AppendLine(htmlCaption);
                    sb.AppendLine("<br>");
                }
                else if (node.Name == "table")
                {
                    if (!passedTOC)
                    {
                        skippedSb.AppendLine($"- Table skipped (Reason: Nằm ở trang bìa hoặc mục lục)");
                        continue;
                    }

                    HtmlNode captionNode = null;
                    HtmlNode prev = node.PreviousSibling;
                    
                    for (int i=0; i<5 && prev != null; i++) {
                        if (prev.NodeType == HtmlNodeType.Element && (prev.Name == "p" || prev.Name.StartsWith("h") || prev.Name == "div"))
                        {
                            string prevText = CleanText(prev.InnerText);
                            if (prevText.StartsWith("Table", StringComparison.OrdinalIgnoreCase) || 
                                prevText.StartsWith("Bảng", StringComparison.OrdinalIgnoreCase))
                            {
                                captionNode = prev;
                                break;
                            }
                            if (!string.IsNullOrWhiteSpace(prevText)) break;
                        }
                        prev = prev.PreviousSibling;
                    }

                    string captionText = "";
                    if (captionNode != null)
                    {
                        captionText = CleanText(captionNode.InnerText);
                        captionText = Regex.Replace(captionText, @"^(Table|Bảng)[\s\.]*(\d+)?[\.\:\-]*\s*", "", RegexOptions.IgnoreCase);
                        captionText = Regex.Replace(captionText, @"^[\d\.a-zA-Z]+\.\s*", ""); 
                        captionText = Regex.Replace(captionText, @"^[\d\.]+\s*", "");
                    }
                    else
                    {
                        captionText = currentHeading;
                        captionText = Regex.Replace(captionText, @"^[\d\.]+\s*", "");
                        captionText = GenerateUniqueCaption(captionText);
                    }

                    string finalCaption = $"Table {tableCount}. {captionText}";
                    tableCount++;

                    string htmlCaption = $"<h6 style=\"font-family: 'Times New Roman', serif; font-size: 12pt; font-style: italic; color: blue; text-align: center; margin-top: 20px; margin-bottom: 10px;\">{finalCaption}</h6>";

                    sb.AppendLine(htmlCaption);
                    sb.AppendLine(node.OuterHtml);
                    sb.AppendLine("<br>");
                }
            }

            File.WriteAllText(mdPath, sb.ToString(), Encoding.UTF8);
            File.WriteAllText(skippedPath, skippedSb.ToString(), Encoding.UTF8);
            Console.WriteLine($"Processed {figureCount-1} figures and {tableCount-1} tables.");
        }
    }
}
