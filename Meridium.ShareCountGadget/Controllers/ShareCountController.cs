using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Shell.Gadgets;

namespace Meridium.ShareCountGadget.Controllers
{
    [Gadget(
        Name = "Sidor med flest antal delningar i sociala medier",
        Description = "Visar de sidor som har flest antal delningar i sociala medier.",
        Title = "Sidor med flest antal delningar i sociala medier")]
    public class ShareCountController : Controller
    {
        private readonly IContentRepository repository;

        public ShareCountController(IContentRepository repository)
        {
            this.repository = repository;
        }

        public ActionResult Index()
        {
            var refs = repository.GetDescendents(ContentReference.StartPage);

            var pages = repository.GetItems(refs,
                new LanguageSelector(ContentLanguage.PreferredCulture.TwoLetterISOLanguageName))
                .OfType<ISocialMediaTrackable>()
                .OrderBy(x => x.ShareCount.TotalShareCount)
                .Take(10)
                .ToList();

            return View("Index", pages);
        }

    }
}