using Nop.Core.Domain.Customers;
using Nop.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Factories
{
    public partial class WatermarkFactory : IWatermarkFactory    
    {

        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly CustomerSettings _customerSettings;

        #endregion

        #region Ctor

        public WatermarkFactory(ILocalizationService localizationService,
            CustomerSettings customerSettings)
        {
            this._localizationService = localizationService;
            this._customerSettings = customerSettings;
        }

        #endregion

    }
}
