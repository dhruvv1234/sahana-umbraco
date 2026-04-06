using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace sahanaweb.Controller
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class HeaderController : ControllerBase
    {
        private readonly IUmbracoContextFactory _umbraco;

        public HeaderController(IUmbracoContextFactory umbraco)
        {
            _umbraco = umbraco;
        }

        [HttpGet]
        public IActionResult GetHeaderData()
        {
            using var refCtx = _umbraco.EnsureUmbracoContext();
            var ctx = refCtx.UmbracoContext;
            var roots = ctx.Content.GetAtRoot();

            var home = ctx.Content.GetAtRoot()
      .Where(x => x.ContentType.Alias == "home")
      .Skip(1)
      .FirstOrDefault();
            var home1 = ctx.Content.GetAtRoot()
            .FirstOrDefault(x => x.Name == "Home Header");
            if (home == null)   
                return NotFound();
            var child = home.Children();
            var servicesNode = home.Children()
    .FirstOrDefault(x => x.ContentType.Alias == "industry");
            var ecosystemNode = home.Children()
    .FirstOrDefault(x => x.ContentType.Alias == "ecoSystemSubPages"); // check alias
            var ecosystemTree = ecosystemNode?.Children()
     ?.Select(item => new
     {
         label = item.Name,
         href = item.Url(),
         children = item.Children()
             .Select(child => new
             {
                 label = child.Name,
                 href = child.Url()
             })
             .ToList()
     })
     .ToList();

            var servicesTree = servicesNode?.Children()
                .Select(service => new
                {
                    label = service.Name,
                    href = service.Url(),
                    children = service.Children()
                        .Select(child => new
                        {
                            label = child.Name,
                            href = child.Url()
                        })
                        .ToList()
                })
                .ToList();
            // ✅ HEADER
            var headerData = new
            {
                siteName = home.Value<string>("siteName"),
                logo = home.Value<IPublishedContent>("logo")?.Url(),
                whiteLogo = home.Value<IPublishedContent>("whiteLogo")?.Url(),
                servicesLinks = servicesTree,
                ecosystemLinks = ecosystemTree,   // ✅ NEW

                companyLinks = MapLinks(home, "companyLinks"),
                recognitionLinks = MapLinks(home, "recognitionLinks"),
                knowledgeHubLinks = MapLinks(home, "knowledgeHubLinks"),
                latestLinks = MapLinks(home, "latestLinks"),
                productsPlatformsLinks = MapLinks(home, "productsPlatformsLinks"),
                investorInformationLinks = MapLinks(home, "investorInformationLinks"),
                investorGalleryLinks = MapLinks(home, "investorGalleryLinks"),
                industryLinks = MapLinks(home, "industryLinks"),
                implementedRoutesLinks = MapLinks(home, "implementedRoutesLinks"),
                featuredItems = MapFeaturedBlock(home),
            };
    {
    };

            // ✅ FOOTER DATA
            var footer = new
            {
                subHeading = home.Value<string>("subHeading"),
                icon = home.Value<IPublishedContent>("icon")?.Url(),
                siteUrl = home.Value<string>("siteUrl"),
                copyrightText = home.Value<string>("copyrightText"),

                // 🔗 Social
                socialMediaLinks = MapLinks(home, "socialMediaLinks"),

                // 🔗 Footer columns
                whatWeDoLinks = MapLinks(home, "servicesLinks"),
                industriesLinks = MapLinks(home, "industriesLinks"),
                companiesLinks = MapLinks(home, "companyLinks"),
                insightsLinks = MapLinks(home, "knowledgeHubLinks"),

                // 📍 Address & Contact (manual fields OR string)
                address = home.Value<string>("address"),
                phone = home.Value<string>("phone"),
                email = home.Value<string>("email"),
                addressAndContact = home.Value<string>("address"),
                contactNumber = home.Value<string>("contactNumber"),
                contactNumber2 = home.Value<string>("contactNumber2"),
                mail1 = home.Value<string>("mail1"),
                mail2 = home.Value<string>("mail2"),


                // 📝 Form
                formHeadline = home.Value<string>("formHeadline"),
                formSubHeadline = home.Value<string>("fromSubHeadline"),

                // 🔗 Other Links
                otherLinks = MapLinks(home, "otherLinks")
            };

            return Ok(new
            {
                header = headerData,
                footer
            });
        }        // ✅ Fetches links AND their children from a nested content property
        private List<MegaMenuLink> MapLinks(IPublishedContent home, string alias)
        {
            var links = home.Value<IEnumerable<Link>>(alias)?.ToList();
                
            if (links == null || links.Count == 0)
                return new List<MegaMenuLink>();

            return links.Select(link => new MegaMenuLink
            {
                Label = link.Name,
                Href = link.Url ?? "#"
            }).ToList();
        }
       
        public class FeaturedItem
        {
            public string Label { get; set; }
            public string Description { get; set; }
            public string Image { get; set; }
            public string Link { get; set; }
        }
        private List<FeaturedItem> MapFeaturedBlock(IPublishedContent home)
        {
            var block = home.Value<BlockListModel>("featuredBlock");

            if (block == null || !block.Any())
                return new List<FeaturedItem>();

            return block.Select(x =>
            {
                var content = x.Content;

                return new FeaturedItem
                {
                    Label = content.Value<string>("label"),
                    Description = content.Value<string>("description"),
                    Image = content.Value<IPublishedContent>("image")?.Url(),
                    Link = content.Value<Link>("link")?.Url
                };
            }).ToList();
        }
        public class MegaMenuItem
        {
            public string Label { get; set; }
            public List<MegaMenuColumn> Cols { get; set; } = new();
        }

        public class MegaMenuColumn
        {
            public string Heading { get; set; }
            public List<MegaMenuLink> Items { get; set; } = new();
        }

        public class MegaMenuLink
        {
            public string Label { get; set; }
            public string Href { get; set; }
        }
    }
}