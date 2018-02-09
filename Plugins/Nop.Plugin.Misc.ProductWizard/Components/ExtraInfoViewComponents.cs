using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.ProductWizard.Models;
using Nop.Services.Catalog;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Components
{
    public class ExtraInfoViewComponent : NopViewComponent
    {
        private readonly IProductService _ps;

        public ExtraInfoViewComponent(IProductService productService)
        {
            _ps = productService;
        }
        public IViewComponentResult Invoke(int id)
        {
            var model = new LegacyModel();

            var peoduct = _ps.GetProductById(id);
            if (peoduct != null)
            {
                model.PartNumber = peoduct.Sku;
                model.ManufacturerName = peoduct.ManufacturerPartNumber;

            }

            return View("~/Plugins/Misc.ProductWizard/Views/ExtraInfo.cshtml", model);
        }
    }
}
