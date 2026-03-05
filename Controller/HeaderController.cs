using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using static sahanaweb.Constants.UmbracoAliases;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.PublishedModels;
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

            var home = ctx.Content.GetAtRoot()
                .FirstOrDefault(x => x.ContentType.Alias == "home");

            if (home == null)
                return NotFound("Home node not found.");

            var props = home.Value<IPublishedElement>("header");

            var headerData = new
            {
                siteNames = home?.Value<string>(HeaderAliases.Properties.SiteNames),
                whiteLogo = home?.Value<IPublishedContent>(HeaderAliases.Properties.WhiteLogo)?.Url(),
                logo = home?.Value<IPublishedContent>(HeaderAliases.Properties.Logo)?.Url(),
                links = home?.Value<IEnumerable<Link>>(HeaderAliases.Properties.Links),
                usefulLinkTitle = home?.Value<string>(HeaderAliases.Properties.UsefulLinkTitle),
                copyrightText = home?.Value<string>(HeaderAliases.Properties.CopyrightText),
                siteUrl = home?.Value<Link>(HeaderAliases.Properties.SiteUrl),
                icon = home?.Value<IEnumerable<IPublishedContent>>(HeaderAliases.Properties.Icon),
                subHeading = home?.Value<string>(HeaderAliases.Properties.SubHeading),
                submitButton = home?.Value<string>(HeaderAliases.Properties.SubmitButton),
                newslettterSection = home?.Value<string>(HeaderAliases.Properties.NewslettterSection),
                search = home?.Value<string>(HeaderAliases.Properties.Search),
                account = home?.Value<string>(HeaderAliases.Properties.Account),
                socialMediaLinks = home?.Value<IEnumerable<Link>>(HeaderAliases.Properties.SocialMediaLinks)
            };

            var navItems = BuildNavigation(home.Children());

            return Ok(new { header = headerData, navMenu = navItems });
        }

        private IEnumerable<object> BuildNavigation(IEnumerable<IPublishedContent> nodes)
        {
            foreach (var node in nodes)
            {
                bool show = node.Value<bool>(HeaderAliases.Navigation.ShowInNavigation);
                if (!show) continue;

                bool clickable = node.Value<bool>(HeaderAliases.Navigation.isClickable);

                yield return new
                {
                    id = node.Id,
                    title = node.Value<string>(HeaderAliases.Navigation.NavigationTitle) ?? node.Name,
                    url = clickable ? node.Url() : "#",
                    clickable,
                    showInNavigation = true,
                    children = BuildNavigation(node.Children())
                };
            }
        }
    }
}
