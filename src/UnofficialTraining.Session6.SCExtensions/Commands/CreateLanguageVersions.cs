using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnofficialTraining.Session6.SCExtensions.Commands
{
    [Serializable]
    public class CreateLanguageVersions : Command
    {
        #region Public Methods

        public override void Execute(CommandContext context)
        {
            Assert.IsNotNull(context, "context");
            Assert.IsNotNull(context.Items, "items");
            bool recursive = false;

            //check if the parameter is specified in the command to update children
            if (!string.IsNullOrEmpty(context.Parameters["recursive"]))
            {
                //if so set the flag and shoe a message
                recursive = true;
                SheerResponse.Alert("Updated all Children.");
            }

            //for each item, there should only be one but in future we could select multiple, process them.
            foreach (Item item in context.Items)
            {
                Generate(item, recursive, Context.ContentDatabase);
            }
        }

        #endregion
        public void Generate(Item root, bool updateChildren, Database db)
        {
            //get a list of languages
            List<Sitecore.Globalization.Language> langs = LanguageManager.GetLanguages(db).Where(l => l.Name != "en").ToList();
            foreach (var lang in langs)
            {
                //for each language get that languge version
                var langItem = db.GetItem(root.ID, lang);

                //if there are no language versions for this language item, create one
                if (langItem.Versions.Count == 0)
                    langItem.Versions.AddVersion();

                //if update children is true and the item has children, process them for language versions
                if (root.HasChildren && updateChildren)
                {
                    foreach (Item child in root.Children)
                    {
                        Generate(child, updateChildren, db);
                    }
                }
            }
        }

    }
}
