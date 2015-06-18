using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Shell.Gadgets;

namespace Meridium.ShareCountGadget.Controllers
{
    [Gadget(
        Name = "Totalt antal delningar",
        Description = "Visar totalt antal delningar i sociala medier för alla sidor på webbplatsen.",
        Title = "Totalt antal delningar")]
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
                .OfType<IShareCount>()
                .OrderBy(x => x.Name)
                .ToList();

            return View("Index", pages);
        }

    }
}