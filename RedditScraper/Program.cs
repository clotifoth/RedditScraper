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

            
            string mainContentXPath = "//div[@class='expando']/div[@class='usertext-body may-blank-within md-container ']/div[@class='md']"; // Text content of a reddit post.
            string commentsXPath = "//div[@class='commentarea']/div[@class='sitetable nestedlisting']"; // Comments content of a reddit post.
            HtmlWeb web = new HtmlWeb();
            
            foreach(string redditURL in URLs)
            {
                HtmlDocument doc;
                if (redditURL.ToCharArray()[redditURL.Length - 1] == '/')
                {
                    doc = web.Load(redditURL + "?ref=search_posts"); /* Reddit sometimes rejects posts with no referral tag. */
                }
                else
                {
                    doc = web.Load(redditURL); /* If you added your own tag, you must know what you're doing... */
                }

                if(doc == null)
                {
                    Console.WriteLine("Problem with " + redditURL + "!");
                    continue;
                }


                string resultsPath = Path.Combine(Environment.CurrentDirectory);

                string subRedditName = GetSubRedditName(redditURL);
                string subRedditDirectory = subRedditName;
                Directory.CreateDirectory(subRedditDirectory);
                Directory.CreateDirectory(subRedditDirectory + Path.DirectorySeparatorChar + GetThreadName(redditURL));

                resultsPath += Path.DirectorySeparatorChar + subRedditDirectory;
                resultsPath += Path.DirectorySeparatorChar + GetThreadName(redditURL);

                HtmlNode mainContent = doc.DocumentNode.SelectSingleNode(mainContentXPath);
                File.WriteAllText((resultsPath + Path.DirectorySeparatorChar + "content.html"), mainContent.InnerHtml);

                Console.WriteLine(subRedditName + '/' + GetThreadName(redditURL) + " main content pulled successfully!");

                HtmlNode comments = doc.DocumentNode.SelectSingleNode(commentsXPath);
                File.WriteAllText((resultsPath + Path.DirectorySeparatorChar + "comments.html"), comments.InnerHtml);

                Console.WriteLine(subRedditName + '/' + GetThreadName(redditURL) + " comments pulled successfully!");
            }

            Console.WriteLine(URLs.Count + " reddit thread(s) processed.");
        }

        public static string GetSubRedditName(string URL)
        {
            int subRedditNameStart = 0;
            int subRedditNameEnd = 0;
            char[] sArray = URL.ToCharArray();


            /* Find the last '/' that is involved in the URL structure; from
             * this point forward, it'redditURL the topic.
             */

            for (int i = 23; // https://www.reddit.com/r/ length
                    i < sArray.Length - 2;
                    i++)
            {
                if (sArray[i] == '/')
                {
                    if (subRedditNameStart == 0) // I should use a boolean, but this is prob. faster
                    {                        // It'redditURL 0 when the first / hasn't been found yet
                        subRedditNameStart = i + 1;
                    }
                    else if (subRedditNameEnd == 0)
                    {
                        subRedditNameEnd = i;
                        break;
                    }
                }
            }
            return URL.Substring(subRedditNameStart, subRedditNameEnd - subRedditNameStart);
        }

        public static string GetThreadName(string URL)
        {
            int topicNameLength = 0;
            char[] sArray = URL.ToCharArray();

            for (int i = 23; // https://www.reddit.com/r/ length
                    i < sArray.Length - 2;
                    i++)
            {
                if (sArray[i] == '/')
                {
                    topicNameLength = i + 1;
                }
            }
            return URL.Substring(topicNameLength).Replace('/', '_');
        }
    }
}
