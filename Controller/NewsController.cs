using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace sahanaweb.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public NewsController(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        [HttpGet]
        public IActionResult GetAllNews()
        {
            using var cref = _umbracoContextFactory.EnsureUmbracoContext();
            var ctx = cref.UmbracoContext;

            // 🔥 1. Get Home
            var home = ctx.Content.GetAtRoot().FirstOrDefault();
            if (home == null)
                return NotFound("Home not found");

            // 🔥 2. Find News & Announcements page
            var newsRoot = home
                .Descendants()
                .FirstOrDefault(x => x.ContentType.Alias == "newsAndAnnouncements");
            // ⚠️ confirm alias  newsAndAnnouncements

            if (newsRoot == null)
                return NotFound("News page not found");

            // 🔥 3. Get all News Detail pages
            var newsPages = newsRoot.Children()
                .Where(x => x.ContentType.Alias == "newsDetailPage")
                .ToList();

            var response = new List<object>();

            foreach (var item in newsPages)
            {
                // 🔥 4. Block Grid
                var grid = item.Value<BlockGridModel>("content");
                if (grid == null) continue;

                foreach (var block in grid)
                {
                    if (block.Content.ContentType.Alias != "newsItems")
                        continue;

                    var content = block.Content;
                    var pdf = content.Value<IPublishedContent>("pdf");
                    var image = content.Value<IPublishedContent>("image");

                    var imageData = image != null ? new
                    {
                        url = image.Url(),
                        name = image.Name,
                        width = image.Value<int?>("umbracoWidth"),
                        height = image.Value<int?>("umbracoHeight")
                    } : null;
                    var pdfData = pdf != null ? new
                    {
                        url = pdf.Url(),
                        name = pdf.Name,
                        size = pdf.Value<long?>("umbracoBytes")
                    } : null; response.Add(new
                    {
                        title = content.Value<string>("title"),
                        topic = content.Value<string>("topic"),
                        year = content.Value<DateTime?>("year"),
                        summary = content.Value<string>("summary"),
                        pdf = pdfData, // ✅ SAFE
                        image  = imageData,

                        pageTitle = item.Name,
                        url = item.Url()
                    });
                }
            }

            return Ok(response);
        }
    }
}