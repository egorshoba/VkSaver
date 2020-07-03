using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VkApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var token = File.ReadAllText("token.txt");

            //string token = "d2643da2ebfc5bc7067087017226a7b93933b7349dd954c33e97a72bd606815cb520abcdd3cd8c2f2b4f0";

            var htmlCode = File.ReadAllText("bb.html");

            String[] substrings = htmlCode.Split(new string[] { "showPhoto" }, StringSplitOptions.None);

            string temp = "";
            string id = "";
            string res = "";
            foreach (var sub in substrings)
            {
                if (sub.StartsWith("(&#39;"))
                {
                    temp = sub.Substring(6);
                    id = temp.Substring(0, temp.IndexOf('&'));
                    res += id;
                    res += Environment.NewLine;
                }
            }
            File.AppendAllText("ids.txt", res);

            var lines = File.ReadLines("ids.txt");
            int i = 0;
            foreach (var line in lines)
            {
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.Encoding = Encoding.UTF8;
                        string request = "https://api.vk.com/method/photos.getById?v=5.80&access_token=" + token + "&photos=";
                        request += line;
                        var json = webClient.DownloadString(request);

                        Thread.Sleep(500);
                        JToken maxPhoto = null;
                        var parsedJson = JObject.Parse(json);
                        var resToParse = parsedJson["response"].First["sizes"];

                        foreach (var photo in resToParse)
                        {
                            maxPhoto = photo;
                        }

                        webClient.DownloadFile(maxPhoto["url"].ToString(), line + ".jpg");
                        i++;
                        Console.WriteLine(line);
                        File.AppendAllText("completed.txt", line + Environment.NewLine);

                    }
                }
                catch (Exception)
                {
                    File.AppendAllText("completed.txt", line + " error!" + Environment.NewLine);
                }

            }

            /*
            while (true)
            {
                using (WebClient webClient = new WebClient())
                {
                    string token = "**********************************************************************";
                    webClient.Encoding = Encoding.UTF8;
                    string request = "https://api.vk.com/method/fave.getPhotos?v=5.80&access_token=" + token + "&count=2000";
                    var json = webClient.DownloadString(request);

                    var parsedJson = JObject.Parse(json);
                    var items = parsedJson["response"]["items"];
                    foreach (var photo in items)
                    {
                        string unLikeRequest = "https://api.vk.com/method/likes.delete?v=5.80&access_token=" + token + "&item_id=" + photo["id"] + "&type=photo&owner_id=" + photo["owner_id"];
                        int timeToSleep = 0;

                        while (true)
                        {
                            try
                            {
                                Thread.Sleep(5000);
                                var unLikeJson = webClient.DownloadString(unLikeRequest);

                                var parsedUnLikeJson = JObject.Parse(unLikeJson);
                                var response = parsedUnLikeJson["response"]["likes"];

                                var res = Int32.Parse(response.ToString());

                                Console.WriteLine(photo["sizes"].Last["url"] + " unliked");
                                break;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                Console.WriteLine("Sleeping " + (timeToSleep / 1000).ToString() + " seconds");
                                Thread.Sleep(timeToSleep);
                                timeToSleep += 5000;
                                continue;
                            }
                        }
                    }
                }
            }

            */
        }
    }
}

