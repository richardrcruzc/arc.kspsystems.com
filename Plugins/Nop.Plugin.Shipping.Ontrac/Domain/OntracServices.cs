//------------------------------------------------------------------------------
// Contributor(s):richard-cruz@hotmail.com 3/20/2018, WA 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Shipping.Ontrac.Domain
{
    /// <summary>
    /// Class for Ontrac services
    /// </summary>
    public static class OntracServices
    {
        #region Fields

        /// <summary>
        /// Ontrac Service names
        /// </summary>
        private static readonly Dictionary<string, string> _services = new Dictionary<string, string>
        {
            {"Ground", "C"},
            {"Ontrac Sunrise - 10:30AM Delivery", "S"},
            {"Ontrac Sunrise Gold - 8:00AM Delivery", "G"},            
        };

        #endregion
    
        #region Utilities

        /// <summary>
        /// Gets the Service ID for a service
        /// </summary>
        /// <param name="service">service name</param>
        /// <returns>service id or empty string if not found</returns>
        public static string GetServiceId(string service)
        {
            var serviceId = "";
            if (string.IsNullOrEmpty(service))
                return serviceId;

            if (_services.ContainsKey(service))
                serviceId = _services[service];

            return serviceId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Ontrac services string names
        /// </summary>
        public static string[] Services
        {
            get
            {
                return _services.Keys.Select(x => x).ToArray();
            }
        }

        #endregion

    }
}
