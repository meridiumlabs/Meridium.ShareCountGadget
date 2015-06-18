using EPiServer.Core;
using EPiServer.Data.Entity;
using Meridium.ShareCountGadget.Models;

namespace Meridium.ShareCountGadget
{
    public interface IShareCount : IContent, IReadOnly<PageData>
    {
        ShareCountBlock ShareCount { get; set; }
    }
}
