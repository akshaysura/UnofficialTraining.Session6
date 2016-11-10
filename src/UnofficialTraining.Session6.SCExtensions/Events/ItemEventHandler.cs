using Sitecore.Data.Items;
using Sitecore.Events;
using Sitecore.SecurityModel;
using System;
using System.Linq;

namespace UnofficialTraining.Session6.SCExtensions.Events
{
    public class ItemEventHandler
    {
        protected void HandleItemName(object sender, EventArgs args)
        {
            var item = (Item)Event.ExtractParameter(args, 0);
            string processedName;
            processedName = item.Name.Replace('-', ' ');

            if (item.Database.Name != "master"
            || (!item.Paths.Path.StartsWith("/sitecore/content/") && !item.Paths.Path.StartsWith("/sitecore/media library/"))
            || !item.Name.Contains('-'))
            {
                return;
            }

            using (new SecurityDisabler())
            {
                item.Editing.BeginEdit();
                try
                {
                    item.Name = processedName;
                }
                finally
                {
                    item.Editing.EndEdit();
                }

            }
        }
    }
}
