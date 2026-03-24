using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace sahanaweb.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public BlogController(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        [HttpGet]
        public IActionResult GetAllBlogs()
        {
            using var cref = _umbracoContextFactory.EnsureUmbracoContext();
            var ctx = cref.UmbracoContext;

            // 🔥 1. Get Home
            var home = ctx.Content.GetAtRoot().FirstOrDefault();
            if (home == null)
                return NotFound("Home not found");

            // 🔥 2. Find Blogs page
            var blogRoot = home
                .Descendants()
                .FirstOrDefault(x => x.ContentType.Alias == "blogsPage");
            // ⚠️ confirm alias

            if (blogRoot == null)
                return NotFound("Blogs page not found");

            // 🔥 3. Get all Blog Detail pages
            var blogPages = blogRoot.Children()
                .Where(x => x.ContentType.Alias == "blogDetailPage")
                .ToList();

            var response = new List<object>();

            foreach (var item in blogPages)
            {
                // 🔥 4. Block Grid
                var grid = item.Value<BlockGridModel>("content");
                if (grid == null) continue;

                foreach (var block in grid)
                {
                    if (block.Content.ContentType.Alias != "blogItems")
                        continue;

                    var content = block.Content;

                    // ✅ Image (MULTIPLE picker)
                    var image = content.Value<IPublishedContent>("image");


                    var imageData = image != null ? new
                    {
                        url = image.Url(),
                        name = image.Name,
                        width = image.Value<int?>("umbracoWidth"),
                        height = image.Value<int?>("umbracoHeight")
                    } : null;

                    response.Add(new
                    {
                        title = content.Value<string>("title"),
                        category = content.Value<string>("category"),
                        date = content.Value<DateTime?>("date"),
                        author = content.Value<string>("author"),
                        summary = content.Value<string>("summary"),

                        image = imageData,

                        pageTitle = item.Name,
                        url = item.Url()
                    });
                }
            }

            return Ok(response);
        }
    }
}