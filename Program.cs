using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace VkSaver2
{
    class Program
    {
        static void Main(string[] args)
        {
            HtmlDocument htmlDoc = new HtmlDocument();

            var webClient = new WebClient();
            var helpers = new Helpers();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var encoding = Encoding.GetEncoding("windows-1251");
            Console.OutputEncoding = Encoding.Unicode;
            var fileNumber = 0;
            foreach (var folder in Directory.GetDirectories("/Projects/VkSaver2/Archive/messages"))
            {

                foreach (var file in Directory.GetFiles(folder).Where(file => file.EndsWith(".html")))
                {
                    fileNumber++;
                    var fullHtml = File.ReadAllText(file, encoding);
                    htmlDoc.LoadHtml(fullHtml);

                    var nameNode = htmlDoc.DocumentNode.Descendants(0)
                        .Where(n => n.HasClass("ui_crumb") && n.Name == "div");

                    string name = "";
                    if (nameNode.Any())
                        name = nameNode.FirstOrDefault().InnerText;

                    Console.WriteLine(fileNumber + ": " + name);

                    var nodes = htmlDoc.DocumentNode.Descendants(0)
                        .Where(n => n.HasClass("attachment__link"));

                    bool needToSave = false;

                    foreach (var node in nodes.ToList())
                    {
                        var link = node.InnerText;

                        if (link.EndsWith(".jpg") && !node.ParentNode.ChildNodes.Any(m => m.Name == "img"))
                        {
                            var imageName = Guid.NewGuid().ToString() + ".jpg";

                            var downloaded = helpers.Download(webClient, link, folder + "/" + imageName);

                            if (!downloaded)
                                continue;

                            var imgHtml = @"<img src=""" + imageName + @""">";

                            var imgNode = HtmlNode.CreateNode(imgHtml);

                            node.ParentNode.AppendChild(imgNode);
                            needToSave = true;
                        }
                    }

                    if (needToSave)
                    {
                        string result = null;
                        using (StringWriter writer = new StringWriter())
                        {
                            htmlDoc.Save(writer);
                            result = writer.ToString();
                        }

                        File.WriteAllText(file, result, encoding);
                    }

                }
            }

        }

        public class Helpers
        {
            public bool Download(WebClient webClient, string link, string path)
            {
                int sleep = 0;

                try
                {
                    webClient.DownloadFile(new Uri(link), path);
                    Thread.Sleep(sleep);
                    return true;
                }
                catch
                {
                    return false;
                }

            }
        }


    }
}
