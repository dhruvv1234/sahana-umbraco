using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace sahanaweb.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class IndustriesController : ControllerBase
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public IndustriesController(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        [HttpGet("GetAllIndustries")]
        public IActionResult GetAllIndustries()
        {
            using var cref = _umbracoContextFactory.EnsureUmbracoContext();
            var ctx = cref.UmbracoContext;

            var home = ctx.Content.GetAtRoot()
                .Where(x => x.ContentType.Alias == "home")
                .Skip(1)
                .FirstOrDefault();

            if (home == null)
                return NotFound("Home not found");

            var blogRoot = home
                .Descendants()
                .Where(x => x.ContentType.Alias == "industry")
                .Skip(1)
                .FirstOrDefault();

            if (blogRoot == null)
                return NotFound("Blogs page not found");

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
                string eyebrow = null;
                object heroImage = null;

                foreach (var block in grid)
                {
                    var alias = block.Content.ContentType.Alias;

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
                        eyebrow = content.Value<string>("eyebrow");
                        heroDescription = content.Value<string>("heroDescription");
                    }
                }

                // ✅ moved outside inner loop
                response.Add(new
                {
                    pageTitle = item.Name,
                    url = item.Url(),
                    heroSection = new
                    {
                        heroTitle,
                        heroDescription,
                        heroImage
                    }
                });
            }

            // ✅ FINAL RETURN (outside loop)
            return Ok(response);
        }
    }
}
