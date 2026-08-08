using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace HtmlProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            string htmlPath = @"d:\Linked\docs\rp4\Report4_21.05.2026.html";
            string mdPath = @"d:\Linked\docs\rp4\output.md";
            string skippedPath = @"d:\Linked\docs\rp4\skipped.txt";

            var doc = new HtmlDocument();
            doc.Load(htmlPath, Encoding.UTF8);

            var sb = new StringBuilder();
            var skippedSb = new StringBuilder();
            
            int figureCount = 1;
            int tableCount = 1;

            var usedCaptions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            string GenerateUniqueCaption(string baseCaption)
            {
                if (string.IsNullOrWhiteSpace(baseCaption)) baseCaption = "Untitled";
                baseCaption = Regex.Replace(baseCaption, @"\s+", " ").Trim();
                
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
            bool passedTOC = false;
            
            int packageTableCounter = 0;

            var nodes = doc.DocumentNode.Descendants().ToList();
            foreach (var node in nodes)
            {
                if (node.Name.StartsWith("h") && node.Name.Length == 2 && char.IsDigit(node.Name[1]))
                {
                    string text = node.InnerText.Trim();
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
                                }
                            }
                        }
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
                        bool hasText = parentP.Descendants("#text").Any(n => !string.IsNullOrWhiteSpace(n.InnerText.Replace("&nbsp;", "").Replace("\u200B", "")));
                        if (hasText)
                        {
                            string pText = parentP.InnerText.Replace("&nbsp;", "").Replace("\u200B", "").Trim();
                            if (pText.ToLower().Contains("class diagram") || pText.ToLower().Contains("sequence diagram"))
                            {
                                inlineWithText = true;
                            }
                            else
                            {
                                skippedSb.AppendLine($"- Image skipped (Reason: Hình ảnh nằm in-line with text trong đoạn văn: '{parentP.InnerText.Trim().Substring(0, Math.Min(50, parentP.InnerText.Trim().Length))}...')");
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
                                string nextText = next.InnerText.Trim();
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
                        captionText = captionNode.InnerText.Trim();
                        captionText = Regex.Replace(captionText, @"^(Figure|Hình)[\s\.]*(\d+)?[\.\:\-]*\s*", "", RegexOptions.IgnoreCase);
                        // Strip leading list items like "a. ", "1. "
                        captionText = Regex.Replace(captionText, @"^[\d\.a-zA-Z]+\.\s*", ""); 
                        
                        // If the text is just "Class Diagram" or "Sequence Diagram", append the feature name
                        if (captionText.Equals("class diagram", StringComparison.OrdinalIgnoreCase) ||
                            captionText.Equals("sequence diagram", StringComparison.OrdinalIgnoreCase) ||
                            captionText.Equals("package diagram", StringComparison.OrdinalIgnoreCase))
                        {
                            captionText = $"{captionText} of {currentFeatureHeading}";
                            captionText = GenerateUniqueCaption(captionText); // Generate unique for these generic ones
                        }
                        // Also strip simple leading numbers if left
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
                            captionText = currentHeading;
                        }
                        captionText = Regex.Replace(captionText, @"^[\d\.]+\s*", "");
                        captionText = GenerateUniqueCaption(captionText);
                    }

                    string finalCaption = $"Figure {figureCount}. {captionText}";
                    figureCount++;

                    string htmlCaption = $"<h5 style=\"font-family: 'Times New Roman', serif; font-size: 12pt; font-style: italic; color: blue; text-align: center; margin-top: 10px; margin-bottom: 20px;\">{finalCaption}</h5>";

                    // Remove the text from node if it was inline so we don't output "a. Class Diagram" then the image then the caption
                    if (inlineWithText && parentP != null)
                    {
                        // To extract just the image and not output the parent text, we output node.OuterHtml instead of parentP.OuterHtml.
                        // Actually, I am already outputting node.OuterHtml.
                    }

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
                            string prevText = prev.InnerText.Trim();
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
                        captionText = captionNode.InnerText.Trim();
                        captionText = Regex.Replace(captionText, @"^(Table|Bảng)[\s\.]*(\d+)?[\.\:\-]*\s*", "", RegexOptions.IgnoreCase);
                        captionText = Regex.Replace(captionText, @"^[\d\.a-zA-Z]+\.\s*", ""); 
                        captionText = Regex.Replace(captionText, @"^[\d\.]+\s*", "");
                    }
                    else
                    {
                        captionText = currentHeading;
                        captionText = Regex.Replace(captionText, @"^[\d\.]+\s*", "");
                        
                        if (captionText.ToLower().Contains("package diagram")) 
                        {
                            packageTableCounter++;
                            if (packageTableCounter == 1) captionText = "Package Description of Frontend";
                            else if (packageTableCounter == 2) captionText = "Package Description of Backend";
                            else if (packageTableCounter == 3) captionText = "Package Description of AI Moderation FastAPI";
                            else captionText = "Package Description";
                        }
                        else
                        {
                            captionText = GenerateUniqueCaption(captionText);
                        }
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
