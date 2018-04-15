using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Components
{
    public class OrderExtraInfoViewComponent : NopViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IProfileModelFactory _profileModelFactory;
        private readonly IWorkContext _workContext;

        private readonly IGenericAttributeService _genericAttributeService;

        public OrderExtraInfoViewComponent(IGenericAttributeService genericAttributeService, IWorkContext workContext, 
            ICustomerService customerService, IProfileModelFactory profileModelFactory)
        {
            this._customerService = customerService;
            this._profileModelFactory = profileModelFactory;
            this._workContext = workContext;
            this._genericAttributeService = genericAttributeService;
        }

        public IViewComponentResult Invoke(bool showNote)
        {
            //#region Shipping notes
            //public string ShippingNote { get; set; }
            //public bool DropShip { get; set; }
            //public bool ResidentialAddress { get; set; }
            //public string ShipToCompanyName { get; set; }
            //#endregion


            var customerId = _workContext.CurrentCustomer.Id;
            var orderNoteExt = string.Empty;
            var shipToCompanyNameExt = string.Empty;
            var dropShipExt = false;
            var residentialAddressExt = false;

            var gAttrs = _genericAttributeService.GetAttributesForEntity(customerId, "Customer").ToList();
            foreach(var attr in gAttrs)
            {

                if (attr.Key == "OrderNoteExt")
                    orderNoteExt = attr.Value;
                else if (attr.Key == "ShipToCompanyNameExt")
                    shipToCompanyNameExt = attr.Value;
                else if (attr.Key == "DropShipExt")
                    dropShipExt =  attr.Value == "true"?true:false;
                else if (attr.Key == "ResidentialAddressExt")
                    residentialAddressExt = attr.Value == "true" ? true : false;


            }


            //_genericAttributeService.SaveAttribute(eventMessage.Customer, SystemCustomerAttributeNames.FirstName, firstName);
            
            var model = new ExtraOrderInfoModel
            { 
                CustomerId= customerId,
                 ShowNote = showNote,
                  DropShip = dropShipExt,
                  Note=orderNoteExt,
                  ResidentialAddress= residentialAddressExt,
                  ShipToCompanyName = shipToCompanyNameExt,
                 
            };
            
            return View(model);
        }
    }
}
