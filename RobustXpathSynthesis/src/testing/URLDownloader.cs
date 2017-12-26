using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RobustXpathSynthesis.src.testing
{
    class URLDownloader
    {

        public static String GetHtmlOfURL(String inURL)
        {
            if (File.Exists("cache/" + TestSites.getMD5(inURL) + ".html"))
            {
                return File.ReadAllText("cache/" + TestSites.getMD5(inURL) + ".html");
            }

            WebClientModified wc = new WebClientModified();
            wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            Directory.CreateDirectory("cache/");
            var res = wc.DownloadString(inURL);
            File.WriteAllText("cache/" + TestSites.getMD5(inURL) + ".html", res);
            return res;
        }

        public static String UrlFixIfRelative(String newURL,String oldURL)
        {
            /* if (newURL.ToLower().Contains("http"))
             {
                 return newURL;
             }

             if (newURL.StartsWith("/"))
             {
                 var urlSplit = oldURL.Split(new string[] { "//" },StringSplitOptions.RemoveEmptyEntries);
                 var urlFinal = urlSplit[0]+"//"+urlSplit[1].Substring(0, urlSplit[1].IndexOf("/")) + newURL;
                 return urlFinal;
             }

             return (oldURL.Substring(0, oldURL.LastIndexOf("/") + 1) + newURL);*/
            return new Uri(new Uri(oldURL), newURL).ToString();
        }


    }
}
