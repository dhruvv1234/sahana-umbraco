using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace sahanaweb.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class CaseStudyController : ControllerBase
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public CaseStudyController(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        [HttpGet]
        public IActionResult GetAllCaseStudies()
        {
            using var cref = _umbracoContextFactory.EnsureUmbracoContext();
            var ctx = cref.UmbracoContext;

            // 🔥 1. Get Home (ROOT)
            var home = ctx.Content.GetAtRoot().FirstOrDefault();

            if (home == null)
                return NotFound("Home not found");

            // 🔥 2. Get Case Studies under Home
        

            var caseStudiesRoot = home
               .Descendants()
               .FirstOrDefault(x => x.ContentType.Alias == "caseStudiesPage");

            if (caseStudiesRoot == null)
                return NotFound("Case Studies page not found");

            // 🔥 3. Get all Case Study Detail Pages
            var caseStudies = caseStudiesRoot.Children()
                .Where(x => x.ContentType.Alias == "caseStudiesDetailPage")
                .ToList();

            var response = new List<object>();

            foreach (var item in caseStudies)
            {
                // 🔥 4. Block Grid
                var grid = item.Value<BlockGridModel>("content");

                if (grid == null) continue;

                foreach (var block in grid)
                {
                    if (block.Content.ContentType.Alias != "caseStudiesItems")
                        continue;

                    var content = block.Content;

                    var image = content.Value<IPublishedContent>("image");

                    response.Add(new
                    {
                        title = item.Name,
                        url = item.Url(),

                        industry = content.Value<string>("industry"),
                        clientName = content.Value<string>("clientsName"),
                        tags = content.Value<IEnumerable<string>>("tags"),

                        challenges = content.Value<string>("challenges"),
                        solutions = content.Value<string>("solutions"),
                        results = content.Value<IEnumerable<string>>("results"),

                        image = image?.Url()
                    });
                }
            }

            return Ok(response);
        }
    }
}
