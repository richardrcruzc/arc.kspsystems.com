using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Components
{ 
    public class SearchBoxExtViewComponent  : NopViewComponent
    {
        private readonly ICatalogModelFactory _catalogModelFactory;

        public SearchBoxExtViewComponent(ICatalogModelFactory catalogModelFactory)
        {
            this._catalogModelFactory = catalogModelFactory;
        }

        public IViewComponentResult Invoke()
        {
            var model = _catalogModelFactory.PrepareSearchBoxModel();
            return View("~/Plugins/Misc.ProductWizard/Views/SearchBoxExt/default.cshtml", model);
        }
    }
}
