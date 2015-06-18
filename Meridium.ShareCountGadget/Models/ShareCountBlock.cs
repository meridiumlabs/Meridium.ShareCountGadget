using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace Meridium.ShareCountGadget.Models
{
    [ContentType(DisplayName = "ShareCountBlock", GUID = "57e83bb2-0768-4c8b-9f5e-60e1468a6313", Description = "")]
    public class ShareCountBlock : BlockData
    {
        public virtual double TotalShareCount { get; set; }
        public virtual string FacebookShareCount { get; set; }
        public virtual string TwitterShareCount { get; set; }
    }
}