using System.Linq;
using System.Text;
using System.Threading;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Globalization;
using EPiServer.PlugIn;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using log4net;

namespace Meridium.ShareCountGadget
{
    [ScheduledPlugIn(DisplayName = "Uppdatera antalet delningar")]
    public class ShareCountRetriever
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (ShareCountRetriever));

        private static string FacebookKey = "facebook";
        private static string TwitterKey = "twitter";

        public static string Execute()
        {
            Log.Info("Start retrieving shares counts");

            var repo = ServiceLocator.Current.GetInstance<IContentRepository>();

            var refs = repo.GetDescendents(ContentReference.StartPage);

            var pages = repo.GetItems(refs,
                new LanguageSelector(ContentLanguage.PreferredCulture.TwoLetterISOLanguageName))
                .OfType<IShareCount>()
                .OrderBy(x => x.Name).ToList();

            var shareCountService = new ShareCountService.ShareCountService();

            foreach (var page in pages)
            {                
                var url = page.ExternalUrl();
                Log.Info("Retrieving shares for: " + url);
                var shares = shareCountService.GetShares(url);
                Log.Info("Shares retrieved for: " + url);

                var shareCounts = shares.FirstOrDefault();

                if (shareCounts == null)
                {
                    Log.Info("Shares for url " + url + " is empty");
                    continue;
                }

                var facebookShares = shareCounts.Counts.First(x => x.Key.ToLower() == FacebookKey).Value;
                var twitterShares = shareCounts.Counts.First(x => x.Key.ToLower() == TwitterKey).Value;

                Log.Info("Total share count for url " + url + " is " + shareCounts.TotalCounts);
                Log.Info("Facebook share count for url " + url + " is " + facebookShares);
                Log.Info("Twitter share count for url " + url + " is " + twitterShares);

                var clone = (IShareCount)page.CreateWritableClone();
                clone.ShareCount.TotalShareCount = shareCounts.TotalCounts;
                clone.ShareCount.FacebookShareCount = shareCounts.Counts.First(x => x.Key == FacebookKey).Value;
                clone.ShareCount.TwitterShareCount = shareCounts.Counts.First(x => x.Key == TwitterKey).Value;
                repo.Save(clone,
                    SaveAction.ForceCurrentVersion
                    | SaveAction.Publish
                    | SaveAction.SkipValidation,
                    AccessLevel.NoAccess);
                Thread.Sleep(1000);
            }   
            return "OK";
        }
    }

    internal static class ContentExtension
    {
        public static string ExternalUrl(this IContent content)
        {
            var internalUrl = UrlResolver.Current.GetUrl(content.ContentLink);

            var url = new UrlBuilder(internalUrl);
            Global.UrlRewriteProvider.ConvertToExternal(url, null, Encoding.UTF8);

            return UriSupport.AbsoluteUrlBySettings(url.ToString());
        }
    }
}
