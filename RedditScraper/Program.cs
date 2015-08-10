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
using System.Timers;

namespace RedditScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            /* Read through URLs.ini to gather a list of reddit URLs to be
             * scraped.
             */
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

            
            string mainContentXPath = "//div[@class='entry unvoted']"; // Text content of a reddit post.
            string commentsXPath = "//div[@class='commentarea']/div[@class='sitetable nestedlisting']"; // Comments content of a reddit post.
            HtmlWeb web = new HtmlWeb();

            web.UserAgent = "Spooky Creepy Crawlies!"; // Reddit slows down non-default UserAgents that make automated requests

            /* As per https://github.com/reddit/reddit/wiki/API, all Reddit
             * scrapers are limited to 30 requests per minute. These constants
             * establish this limit and are used to enforce this limit.
             * 
             * I use a conservative 62 seconds and 29 requests so that even if
             * there is significant lag, the limit will not be breached.
             */
            DateTime timeTracker = DateTime.Now;
            TimeSpan LimitDuration = new TimeSpan(0, 0, 62);
            int maxRequestsPerLimitDuration = 29;
            int numberOfRequests = 0;
            
            foreach(string redditURL in URLs)
            {
                HtmlDocument doc;
                if (redditURL.ToCharArray()[redditURL.Length - 1] == '/')
                {
                    doc = web.Load(redditURL + "?ref=search_posts"); /* Reddit sometimes rejects URLs with no referral tag. */
                    numberOfRequests++;
                }
                else
                {
                    doc = web.Load(redditURL); /* If you added your own tag, you must know what you're doing... */
                    numberOfRequests++;
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

                if (mainContent == null)
                {
                    Console.WriteLine("Problem with " + redditURL + "!");
                    continue;
                }

                File.WriteAllText((resultsPath + Path.DirectorySeparatorChar + "content.html"), mainContent.InnerHtml);

                Console.WriteLine(subRedditName + '/' + GetThreadName(redditURL) + " main content pulled successfully!");

                HtmlNode comments = doc.DocumentNode.SelectSingleNode(commentsXPath);
                File.WriteAllText((resultsPath + Path.DirectorySeparatorChar + "comments.html"), comments.InnerHtml);

                Console.WriteLine(subRedditName + '/' + GetThreadName(redditURL) + " comments pulled successfully!");

                /* If max requests per minute is reached, delay until a new
                 * minute is reached, so that more requests may be made.
                 */
                if(DateTime.Now.Subtract(timeTracker) < LimitDuration && numberOfRequests >= maxRequestsPerLimitDuration)
                {
                    Console.WriteLine("Reached maximum requests per hour!");
                    Timer timer = new Timer();
                    timer.Interval = 1000.0;
                    timer.Elapsed += (Object sender, ElapsedEventArgs e) =>
                    {
                        Console.WriteLine("Time until more requests can be processed: " + DateTime.Now.Subtract(LimitDuration).Subtract(timeTracker));
                    };
                    timer.Start();
                    while(DateTime.Now.Subtract(LimitDuration).Subtract(timeTracker) < TimeSpan.Zero)
                    {

                    }
                    timer.Stop();
                    timeTracker = DateTime.Now;
                    numberOfRequests = 0;
                }
            }

            Console.WriteLine(URLs.Count + " reddit thread(s) processed.");
            Console.ReadKey();
        }


        public static string GetSubRedditName(string URL)
        {
            int subRedditNameStart = 0;
            int subRedditNameEnd = 0;
            char[] sArray = URL.ToCharArray();


            /* Find the last '/' that is involved in the URL structure; from
             * this point forward, it's the topic.
             */

            for (int i = 23; // https://www.reddit.com/r/ length
                    i < sArray.Length - 2;
                    i++)
            {
                if (sArray[i] == '/')
                {
                    if (subRedditNameStart == 0) // I should use a boolean, but this is prob. faster
                    {                        // It's 0 when the first / hasn't been found yet
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
