using System;
using System.Collections.Generic;
using NLog;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Radish.Models;

namespace Radish.Support
{
    public class FindAPhotoClient
    {
        static private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public string Host { get; set; }

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

        public IList<MediaMetadata> Search(string query)
        {
            if (Host == null)
            {
                LastError = "Host not set";
                return null;
            }

            LastError = "";
            var list = new List<MediaMetadata>();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Host);
                    int first = 0;
                    int count = 100;

                    bool searchAgain = true;
                    do
                    {
                        searchAgain = false;
                        var requestUrl = string.Format("api/search?f={0}&c={1}&q={2}", first, count, query);
                        var result = client.GetAsync(requestUrl).Result;
                        if (result.IsSuccessStatusCode)
                        {
                            var body = result.Content.ReadAsStringAsync().Result;
                            dynamic response = JObject.Parse(body);

                            if (response["matches"] != null)
                            {
                                first += count;
                                foreach (var m in response["matches"])
                                {
                                    searchAgain = true;
                                    var mimeType = m["mimeType"].ToString();
                                    if (mimeType != null && mimeType.StartsWith("image"))
                                    {
                                        Location location = null;
                                        if (m["latitude"].Type == JTokenType.Float && m["longitude"].Type == JTokenType.Float)
                                        {
                                            double latitude = m["latitude"];
                                            double longitude = m["longitude"];
                                            location = new Location(latitude, longitude);
                                        }
                                        DateTime createdDate = m["createdDate"];
                                        var item = new FindAPhotoMetadata(
                                            client.BaseAddress + m["fullUrl"].ToString(),
                                            createdDate,
                                            location);
                                        list.Add(item);
                                    }
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
                            break;
                        }
                    } while (searchAgain);
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
            return list;
        }
    }
}

