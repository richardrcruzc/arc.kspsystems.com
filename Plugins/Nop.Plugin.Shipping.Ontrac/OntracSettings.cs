using Nop.Core.Configuration;
using Nop.Plugin.Shipping.Ontrac.Domain;

namespace Nop.Plugin.Shipping.Ontrac
{
    /// <summary>
    /// Represents settings of the Ontrac shipping plugin
    /// </summary>
    public class OntracSettings : ISettings
    {
        /// <summary>
        /// Gets or sets Ontrac service URL
        /// </summary>
        public string Url { get; set; }

       
        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets an amount of the additional handling charge
        /// </summary>
        public decimal AdditionalHandlingCharge { get; set; }

        /// <summary>
        /// Gets or sets Ontrac customer classification
        /// </summary>
        public OntracCustomerClassification CustomerClassification { get; set; }

        /// <summary>
        /// Gets or sets a pickup type
        /// </summary>
        public OntracPickupType PickupType { get; set; }

        /// <summary>
        /// Gets or sets packaging type
        /// </summary>
        public OntracPackagingType PackagingType { get; set; }

        /// <summary>
        /// Gets or sets offered carrier services
        /// </summary>
        public string CarrierServicesOffered { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to insure packages
        /// </summary>
        public bool InsurePackage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to pass package dimensions
        /// </summary>
        public bool PassDimensions { get; set; }

        /// <summary>
        /// Gets or sets the packing package volume
        /// </summary>
        public int PackingPackageVolume { get; set; }

        /// <summary>
        /// Gets or sets packing type
        /// </summary>
        public PackingType PackingType { get; set; }

     
    }
}