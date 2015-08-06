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
                    doc = web.Load(s + "?ref=search_posts"); /* Reddit sometimes rejects posts with no referral tag. */
                }
                else
                {
                    doc = web.Load(s); /* If you added your own tag, you must know what you're doing... */
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

                int topicNameLength = 0;
                int subRedditNameStart = 0;
                int subRedditNameEnd = 0;
                bool subRedditEnd = false;
                char[] sArray = s.ToCharArray();

                /* Find the last '/' that is involved in the URL structure; from
                 * this point forward, it's the topic.
                 */

                for (int i = 23; // https://www.reddit.com/r/ length
                    i < sArray.Length - 2;
                    i++)
                {
                    if(sArray[i] == '/')
                    {
                        if(topicNameLength == 0) // I should use a boolean, but this is prob. faster
                        {                        // It's 0 when the first / hasn't been found yet
                            subRedditNameStart = i + 1;
                        }
                        else if(!subRedditEnd)
                        {
                            subRedditNameEnd = i + 1;
                            subRedditEnd = true;
                        }
                        topicNameLength = i + 1;
                    }
                }
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, s.Substring(subRedditNameStart, subRedditNameEnd - subRedditNameStart)));

                    resultsPath += Path.DirectorySeparatorChar + s.Substring(subRedditNameStart, subRedditNameEnd - subRedditNameStart);
                    resultsPath += Path.DirectorySeparatorChar + s.Substring(topicNameLength).Replace('/', '_') + ".txt";

                File.WriteAllText((resultsPath), result.InnerHtml);
            }
        }
    }
}
