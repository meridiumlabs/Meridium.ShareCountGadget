using System.Linq;
using System.Text;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Globalization;
using EPiServer.PlugIn;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace Meridium.ShareCountGadget
{
    [ScheduledPlugIn(DisplayName = "Uppdatera antalet delningar")]
    public class ShareCountRetriever
    {
        private static string FacebookKey = "facebook";
        private static string TwitterKey = "twitter";

        public static string Execute()
        {
            var repo = ServiceLocator.Current.GetInstance<IContentRepository>();

            var refs = repo.GetDescendents(ContentReference.StartPage);

            var pages = repo.GetItems(refs,
                new LanguageSelector(ContentLanguage.PreferredCulture.TwoLetterISOLanguageName))
                .OfType<IShareCount>()
                .OrderBy(x => x.Name).ToList();

            var shareCountService = new ShareCountService.ShareCountService();

            foreach (var page in pages)
            {
                var shares = shareCountService.GetShares(page.ExternalUrl());
                var clone = (IShareCount)page.CreateWritableClone();
                clone.ShareCount.TotalShareCount = shares.First().TotalCounts;
                clone.ShareCount.FacebookShareCount = shares.First().Counts.First(x => x.Key == FacebookKey).Value;
                clone.ShareCount.TwitterShareCount = shares.First().Counts.First(x => x.Key == TwitterKey).Value;
                repo.Save(clone,
                    SaveAction.ForceCurrentVersion
                    | SaveAction.Publish
                    | SaveAction.SkipValidation,
                    AccessLevel.NoAccess);                    
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
