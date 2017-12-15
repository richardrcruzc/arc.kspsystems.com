using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.ProductWizard.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)] 
   public  class GenericAttributesController : BasePluginController
    {
        #region Fields
        private IGenericAttributeService _genericAttributeService;
        private readonly IProductService _productService;
        #endregion

        #region Constructors

        public GenericAttributesController(IGenericAttributeService genericAttributeService, IProductService productService)
        {
            this._productService = productService;
            this._genericAttributeService = genericAttributeService;
        }


        public virtual IActionResult Edit(int id)
        {
            var model = new GenericModel {Id=id };
            var gAttrs=   _genericAttributeService.GetAttributesForEntity(id, "Product").ToList();
            foreach (var attr in gAttrs)
            {
                switch (attr.Key)
                {
                    case "ExcludeGoogleFeed":
                        model.ExcludeGoogleFeed =bool.Parse(attr.Value);
                        break;
                    case "Color":
                        model.Color = attr.Value;
                        break; 
                }


                
            }
             

            return View("~/Plugins/Misc.ProductWizard/Views/GenericAttribs/Edit.cshtml", model);

            
        }
        [HttpPost]
        public virtual IActionResult Edit(GenericModel model)
        {

            var product = _productService.GetProductById(model.Id);
            _genericAttributeService.SaveAttribute<bool>(product, "ExcludeGoogleFeed",model.ExcludeGoogleFeed );
            _genericAttributeService.SaveAttribute<string>(product, "Color", model.Color);


            return View(model);

        }

        #endregion
    }
}
