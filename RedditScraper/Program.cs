using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using HtmlAgilityPack;

namespace RedditScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            List<String> URLs = new List<String>();

            string txtPath = Path.Combine(Environment.CurrentDirectory, "URLs.ini");

            if(File.Exists(txtPath))
            {
                TextReader textReader = File.OpenText(txtPath);
                
                while(true)
                {
                    String s = textReader.ReadLine();
                    if(s == null) // EOF
                    {
                        textReader.Close();
                        break;
                    }


                    URLs.Add(s);
                }
            }
            else
            {
                Console.WriteLine("URLS.ini not found / invalid!");
                System.Environment.Exit(-1);
            }

            string XPath = "//div[@class='expando']/div[@class='usertext-body may-blank-within md-container ']/div[@class='md']"; // Text content of a reddit post.
            HtmlWeb web = new HtmlWeb();
            
            foreach(String s in URLs)
            {
                HtmlDocument doc;
                if (s.ToCharArray()[s.Length - 1] == '/')
                {
                    doc = web.Load(s + "?ref=search_posts");
                }
                else
                {
                    doc = web.Load(s);
                }
                HtmlNode result = doc.DocumentNode.SelectSingleNode(XPath);

                String resultsPath = Path.Combine(Environment.CurrentDirectory);

                for (int i = resultsPath.Length -1; i < 0; i--)
                {
                    if (resultsPath[i] == '/' || !Char.IsLetterOrDigit(resultsPath[i]))
                    {
                        resultsPath.ToCharArray()[i] = '_';
                    }
                }

                resultsPath += Path.DirectorySeparatorChar + (s.Substring(62).Replace('/', '_')) + ".txt";

                File.WriteAllText((resultsPath), result.InnerHtml);
            }
        }
    }
}
