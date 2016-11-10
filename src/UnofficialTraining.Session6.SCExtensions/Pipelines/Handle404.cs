using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnofficialTraining.Session6.SCExtensions.Pipelines
{
    public class Handle404 : HttpRequestProcessor
    {
        private List<string> TargetWebsites { get; set; }

        private string StatusDescription { get; set; }

        private List<string> RelativeUrlPrefixesToIgnore { get; set; }

        public Handle404()
        {
            TargetWebsites = new List<string>();

            RelativeUrlPrefixesToIgnore = new List<string>();
        }

        public override void Process(HttpRequestArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            bool shouldExit = Sitecore.Context.Item != null
                                || !TargetWebsites.Contains(Sitecore.Context.Site.Name)
                                || StartsWithPrefixToIgnore(args.Url.FilePath);
            if (shouldExit)
            {
                return;
            }

            string notFoundPageItemPath = Sitecore.Context.Site.Properties["404Page"];
            if (string.IsNullOrWhiteSpace(notFoundPageItemPath))
            {
                Log.Warn(string.Format("{0} property not set for site:{1}", "404Page", Sitecore.Context.Site.Name), this);
                return;
            }

            Database database = GetDatabase();
            if (database == null)
            {
                return;
            }

            //add caching logic - not in scope of this demo
            string notFoundContent = GetNotFoundPageContent(args, database, notFoundPageItemPath);
            if (!string.IsNullOrWhiteSpace(notFoundContent))
            {
                args.Context.Response.TrySkipIisCustomErrors = true;
                args.Context.Response.StatusCode = 404;
                if (!string.IsNullOrWhiteSpace(StatusDescription))
                {
                    args.Context.Response.StatusDescription = StatusDescription;
                }

                args.Context.Response.Write(notFoundContent);
                args.Context.Response.End();
                return;
            }
        }

        protected virtual bool StartsWithPrefixToIgnore(string url)
        {
            return !string.IsNullOrWhiteSpace(url) && RelativeUrlPrefixesToIgnore.Any(prefix => url.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase));
        }

        protected virtual Database GetDatabase()
        {
            return Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
        }

        protected virtual string GetNotFoundPageContent(HttpRequestArgs args, Database database, string notFoundPageItemPath)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(database, "database");
            Assert.ArgumentNotNullOrEmpty(notFoundPageItemPath, "notFoundPageItemPath");
            Item notFoundItem = database.GetItem(notFoundPageItemPath);
            if (notFoundItem == null)
            {
                return string.Empty;
            }

            string domain = GetDomain(args);
            string url = LinkManager.GetItemUrl(notFoundItem);
            try
            {
                return WebUtil.ExecuteWebPage(string.Concat(domain, url));
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("{0} Error - domain: {1}, url: {2}", ToString(), domain, url), ex, this);
            }

            return string.Empty;
        }

        protected virtual string GetDomain(HttpRequestArgs args)
        {
            return args.Context.Request.Url.GetComponents(UriComponents.Scheme | UriComponents.Host, UriFormat.Unescaped);
        }
    }
}
