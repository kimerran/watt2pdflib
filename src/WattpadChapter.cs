using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Watt2PDFLibrary
{
    public class WattpadChapter
    {
        public string Title { get; set; }
        public IList<WattpadPage> Pages { get; set; }
        public string Url { get; set; }

        private HtmlDocument _htmlDoc;

        public string Content { get; set; }
        public string Wattcode { get; set; }

        public void GeneratePages()
        {
            HtmlWeb htmlWeb = new HtmlWeb();
            _htmlDoc = htmlWeb.Load(this.Url);

            int MAX_PAGE = this.GetMaxPage(_htmlDoc);

            this.Pages = new List<WattpadPage>();

            for (int i = 1; i <= MAX_PAGE; i++)
            {
                _htmlDoc = htmlWeb.Load(this.Url + "/page/" + i);

                IEnumerable<HtmlNode> bodyList = _htmlDoc.DocumentNode.SelectNodes("//div[@id='storyText']");

                string content = "";
                if (bodyList != null)
                {
                    HtmlNode body = bodyList.ToList().First();
                    content = body.InnerHtml;
                }

                this.Pages.Add(new WattpadPage
                {
                    PageNumber = i,
                    Content = content
                });

            }


        }

        private int GetMaxPage(HtmlDocument doc)
        {
            IEnumerable<HtmlNode> pagingFormList = _htmlDoc.DocumentNode.SelectNodes("//div[@id='paging']");

            if (pagingFormList == null) return 1;

            HtmlNode pagingForm = pagingFormList.First();

            string input = pagingForm.InnerHtml;

            Match match = Regex.Match(input, @"of\s(\d)", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                string key = match.Groups[1].Value;
                int ret = 0;
                int.TryParse(key, out ret);
                return ret;
            }
            return 0;
        }
    }
}