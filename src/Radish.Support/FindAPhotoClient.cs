using System;
using NLog;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Radish.Support
{
    public class FindAPhotoClient
    {
        const string DefaultFields = "id,keywords,thumbUrl,mimeType,latitude,longitude,createdDate,fullUrl";

        static private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public string Host { get; set; }

        public Func<bool> ShouldCancel { get; set; }

        public bool HasError { get  { return !String.IsNullOrWhiteSpace(LastError); } }
        public string LastError { get; private set; }

        public bool TestConnection()
        {
            if (Host == null)
            {
                LastError = "Host not set";
                return false;
            }

            LastError = "";
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Host);
                    var result = client.GetAsync("api/search?q=test%20connection%20for%20Radish").Result;
                    if (result.IsSuccessStatusCode)
                    {
                        return true;
                    }

                    LastError = result.ReasonPhrase;
                    logger.Error("Error testing FindAPhoto connection to '{0}': {1}, {2}", Host, result.StatusCode, result.ReasonPhrase);
                }
            }
            catch (Exception e)
            {
                var message = e.Message;
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                    message += "; " + e.Message;
                }

                LastError = message;
                logger.Error("Error testing FindAPhoto connection to '{0}': {1}", Host, message);
            }

            return false;
        }

        public bool Search(string query, Action<int,int,dynamic> handleMatch)
        {
            if (Host == null)
            {
                LastError = "Host not set";
                return false;
            }

            if (ShouldCancel == null)
            {
                ShouldCancel = () => false;
            }

            LastError = "";
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Host);
                    int first = 0;
                    const int count = 50;
                    int visiting = 0;

                    bool searchAgain = true;
                    do
                    {
                        searchAgain = false;
                        var requestUrl = string.Format("api/search?g=n&f={0}&c={1}&q={2}&p={3}", first, count, query, DefaultFields);
                        var result = client.GetAsync(requestUrl).Result;
                        if (result.IsSuccessStatusCode)
                        {
                            var body = result.Content.ReadAsStringAsync().Result;
                            dynamic response = JObject.Parse(body);
                            int totalMatches = response["totalMatches"];

                            if (response["groups"] != null)
                            {
                                var firstGroup = response["groups"][0];
                                first += count;
                                foreach (var m in firstGroup["images"])
                                {
                                    visiting += 1;
                                    searchAgain = true;
                                    handleMatch(totalMatches, visiting, m);
                                }
                            }
                        }
                        else
                        {
                            LastError = result.ReasonPhrase;
                            logger.Error(
                                "FindAPhoto search failed: {0}; {1}; {2}", 
                                result.StatusCode, 
                                result.ReasonPhrase, 
                                result.Content.ReadAsStringAsync().Result);

                            return false;
                        }
                    } while (searchAgain && !ShouldCancel());

                    return true;
                }
            }
            catch (Exception e)
            {
                var message = e.Message;
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                    message += "; " + e.Message;
                }

                LastError = message;
                logger.Error("Error searching FindAPhoto server '{0}': {1}: {2}", Host, message, e);
            }

            return false;
        }
    }
}

