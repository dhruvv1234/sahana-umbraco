using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace sahanaweb.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {

        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public ServicesController(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        [HttpGet("GetAllServices")]
        public IActionResult GetAllServices()
        {
            using var cref = _umbracoContextFactory.EnsureUmbracoContext();
            var ctx = cref.UmbracoContext;

            // 🔥 1. Get Home
            var home = ctx.Content.GetAtRoot()
                 .Where(x => x.ContentType.Alias == "home")
                 .Skip(1)
                 .FirstOrDefault(); 
            if (home == null)
                return NotFound("Home not found");
            var child = home.Children();

            // 🔥 2. Find Blogs page
            var blogRoot = home
                .Descendants()
                .FirstOrDefault(x => x.ContentType.Alias == "industry");
            // ⚠️ confirm alias

            if (blogRoot == null)
                return NotFound("Blogs page not found");

            // 🔥 3. Get all Blog Detail pages
            var blogPages = blogRoot.Children()
                .Where(x => x.ContentType.Alias == "industriessubpages")
                .ToList();

            var response = new List<object>();

            foreach (var item in blogPages)
            {
                var grid = item.Value<BlockGridModel>("content");
                if (grid == null) continue;

                string heroTitle = null;
                string heroDescription = null;
                object heroImage = null;

                var keyCapabilities = new List<object>();

                foreach (var block in grid)
                {
                    var alias = block.Content.ContentType.Alias;

                    // ✅ 1. Industry Hero Section
                    if (alias == "industryHeroSection")
                    {
                        var content = block.Content;

                        var image = content.Value<IPublishedContent>("heroImage");

                        heroImage = image != null ? new
                        {
                            url = image.Url(),
                            name = image.Name
                        } : null;

                        heroTitle = content.Value<string>("heroTitle");
                        heroDescription = content.Value<string>("heroDescription");
                    }

                    // ✅ 2. Key Capabilities Section
                    if (alias == "keyCapabilitiesSection")
                    {
                        var content = block.Content;

                        // 🔥 Nested Block Grid / Multi-grid
                        var innerGrid = content.Value<BlockGridModel>("block"); // confirm alias

                        if (innerGrid != null)
                        {
                            foreach (var innerBlock in innerGrid)
                            {
                                if (innerBlock.Content.ContentType.Alias == "keyCapabilitiesItems")
                                {
                                    var innerContent = innerBlock.Content;

                                    keyCapabilities.Add(new
                                    {
                                        title = innerContent.Value<string>("title")
                                    });
                                }
                            }
                        }
                    }
                }

                // ✅ Final Response Per Page
                response.Add(new
                {
                    pageTitle = item.Name,
                    url = item.Url(),

                    heroSection = new
                    {
                        heroTitle,
                        heroDescription,
                        heroImage
                    },

                    keyCapabilities = keyCapabilities
                });
            }
            return Ok(response);
        }
    }
}
