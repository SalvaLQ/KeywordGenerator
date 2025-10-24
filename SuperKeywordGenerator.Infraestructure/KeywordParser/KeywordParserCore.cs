using Newtonsoft.Json.Linq;
using System.Web;

namespace SuperKeywordGenerator.Infraestructure.KeywordParser
{
    public class KeywordParserCore
    {
        private readonly HttpClient cli;
        public List<string> BaseKeywords { get; set; }
        public int MinDelay { get; set; }
        public int MaxDelay { get; set; }
        public int Threads { get; set; }
        public KeywordParserCore(List<string> BaseKeywords,int MinDelay, int MaxDelay, int Threads)
        {
            cli = new HttpClient();
            cli.DefaultRequestHeaders.UserAgent.TryParseAdd(KeywordParserConstans.UserAgent);
            this.BaseKeywords = BaseKeywords;
            this.MinDelay = MinDelay;
            this.MaxDelay = MaxDelay;
            this.Threads = Threads;
        }        
        public async Task<List<string>> GetKeywordsAsync(string ProviderUrl, CancellationToken token)
        {
            List<string> GeneratedKeywords = new List<string>();
            if (token.IsCancellationRequested)
                return (GeneratedKeywords);
            Random rnd = new Random();
            var throttler = new SemaphoreSlim(Threads);
            var tasks = BaseKeywords.Select(async Keyword =>
            {
                await throttler.WaitAsync();
                var task = ProcessUrl(ProviderUrl, Keyword, token);

                _ = task.ContinueWith(async s =>
                {
                    await Task.Delay(rnd.Next(MinDelay,MaxDelay) * 1000);
                    throttler.Release();
                });

                var ResultKeywords = await task;
                if (ResultKeywords.Any())
                    GeneratedKeywords.AddRange(ResultKeywords);
                 


            });
            try
            {
                await Task.WhenAny(Task.WhenAll(tasks), token.AsTask());
            }
            catch
            {

            }
            return (GeneratedKeywords);
        }
        private async Task<List<string>> ProcessUrl(string ProviderUrl, string Keyword, CancellationToken token)
        {
            List<string> ResultKeywords = new List<string>();
            if (token.IsCancellationRequested)
                return (ResultKeywords);
            string url = ProviderUrl + HttpUtility.UrlEncode(Keyword);
            var response = await cli.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var info = await response.Content.ReadAsStringAsync();
            var parsed = JArray.Parse(info);
            foreach (var item in parsed)
            {
                if (item is JValue)
                {
                    string key = (string)item;
                    if (!string.IsNullOrEmpty(key) && !key.Contains("https"))
                        ResultKeywords.Add(key);
                }

                else if (item is JArray)
                {
                    foreach (var seconditem in item)
                    {
                        string key = (string)seconditem;
                        if (!string.IsNullOrEmpty(key) && !key.Contains("http"))
                            ResultKeywords.Add(key);
                    }
                }
            }
            return (ResultKeywords);
        }
    }
}
