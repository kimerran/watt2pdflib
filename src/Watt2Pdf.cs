using HtmlAgilityPack;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Watt2PDFLibrary
{
    public class Watt2Pdf
    {

        public static string GetTitle(string Url)
        {
            string title = "";
            HtmlAgilityPack.HtmlWeb htmlWeb = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument htmldoc = htmlWeb.Load(Url);

            IEnumerable<HtmlNode> titleList = htmldoc.DocumentNode.SelectNodes("//h1[@id='title']");

            if (titleList != null)
            {
                HtmlNode body = titleList.ToList().First();
                title = body.InnerHtml;
            }

            return Regex.Replace(title, @"\t|\n|\r", "");
        }


        public static string GetWattcodeFromUrl(string Url)
        {
            string wattcode = "0";
            try
            {
                Match match = Regex.Match(Url, @"(\d+)", RegexOptions.IgnoreCase);


                if (match.Success)
                {
                    wattcode = match.Groups[1].Value;
                }
            }
            catch (Exception)
            {

                return "0";
            }

            return wattcode;
        }

        private static string ReadTextFromZipFile(string zipFile, string fileName)
        {
            MemoryStream data = new MemoryStream();

            using (ZipFile zip = ZipFile.Read(zipFile))
            {
                data.Seek(0, SeekOrigin.Begin);
                zip[fileName].Extract(data);

                data.Position = 0;
                var sr = new StreamReader(data);
                var myStr = sr.ReadToEnd();

                return myStr;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="wattcode"></param>
        public static void DownloadJar(string path, string wattcode)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFile("http://m.wattpad.com/offline/wattpad-" + wattcode + ".jar",
                path + @"/" + wattcode + ".jar");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Location of JAR files</param>
        /// <param name="wattcode">Wattcode of the story to generate</param>
        /// <returns></returns>
        public static WattpadChapter GenerateChapterFromJar(string path, string wattcode)
        {
            WattpadChapter chapter = new WattpadChapter();
            string zipFilePath = path + @"/" + wattcode + ".jar";

            chapter.Wattcode = wattcode;

            // title
            chapter.Title = ReadTextFromZipFile(zipFilePath, wattcode + ".t");
            
            //length
            string strLength = ReadTextFromZipFile(zipFilePath, wattcode + ".l");
            int length = 0;
            int.TryParse(strLength, out length);

            int iterations = length / 10000;
            for (int i = 0; i <= iterations; i++)
            {
                string filename = "";

                if (i == 0)
                {
                    filename = wattcode;
                }
                else
                {
                    filename = wattcode + "-" + i * 10000;
                }

                chapter.Content += ReadTextFromZipFile(zipFilePath, filename);
            }


            return chapter;

        }


        public static List<string> GetChapterUrls(string url)
        {

            HtmlAgilityPack.HtmlWeb htmlWeb = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument htdoc = htmlWeb.Load(url);

            List<string> ret = new List<string>();

            IEnumerable<HtmlAgilityPack.HtmlNode> selectList = htdoc.DocumentNode.Descendants("select")
                                        .Where(x => x.Attributes["class"].Value == "selectBox");

            if (selectList.ToList().Count == 0) return null;
            if (selectList == null) return null;

            var selectElement = selectList.Single();

            foreach (var cNode in selectElement.ChildNodes)
            {
                if (cNode.Name == "option")
                {
                    ret.Add(cNode.GetAttributeValue("value", "NO_URL"));
                }
            }

            //cleanups
            ret.Remove("#");

            return ret;
        }
    }
}
