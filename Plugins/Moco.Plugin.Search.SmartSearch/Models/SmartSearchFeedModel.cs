using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Nop.Core.Domain.Catalog;

namespace Moco.Plugin.Search.SmartSearch.Models
{
	[XmlRoot(DataType = "string", ElementName = "ITEMS")]
	public class SmartSearchFeed
	{
		private List<ITEM> _items = new List<ITEM>();

		[XmlElement]
		public List<ITEM> ITEM 
		{
			get { return _items; }
			set { _items = value; }
		}
	}

	public class ITEM
	{
		public int ProductID { get; set; }
		public int ProductVariantID { get; set; }
		public string Name { get; set; }
		public string SbName { get; set; }
		public string SeName { get; set; }
		public int ImageID { get; set; }
		public string ImageUrl { get; set; }
		public string IconImageUrl { get; set; }
		public string ProductUrl { get; set; }
		public string Body { get; set; }
		public string SbBody { get; set; }
		public string ShortDescription { get; set; }
		public bool IsFreeShipping { get; set; }
		public string Manufacturer { get; set; }
		public decimal Price { get; set; }
		public decimal MinimumPrice { get; set; }
		public decimal OldPrice { get; set; }
		public decimal SpecialPrice { get; set; }
		public decimal SortPrice { get; set; }
		public decimal Weight { get; set; }
		public decimal Length { get; set; }
		public decimal Width { get; set; }
		public decimal Height { get; set; }
		public bool ShowBuyButton { get; set; }
		public DateTime CreatedOn { get; set; }
		
		public List<Variant> Variants { get; set; }
		public string Category { get; set; }
		public int CategoryID { get; set; }
		public string CategoryTree { get; set; }
		public int PriceRange { get; set; }
		public int SalesCount { get; set; }
		public int AverageReview { get; set; }
		public int TotalReviews { get; set; }
		public bool AllowCustomerReviews { get; set; }
		public string ContentType { get; set; }
		public string HasInventory { get; set; }
		public string SKU { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public string GTIN { get; set; }

        #region

	    /// <summary>
	    /// Obsolete - Exists for backwards compatible serialization scenarios, use SeName
	    /// </summary>
	    public string SEName
	    {
	        get { return SeName; }
	        set { if (string.IsNullOrWhiteSpace(SeName)) { SeName = value; } }
	    }

        #endregion
    }

	public class Variant
	{
		public int VariantID { get; set; }
		public bool IsDefault { get; set; }
		public decimal Price { get; set; }
		public decimal OldPrice { get; set; }
		public int Inventory { get; set; }
		public int DisplayOrder { get; set; }
		public bool IsFreeShipping { get; set; }
		public string SKU { get; set; }
	    public string ManufacturerPartNumber { get; set; }
	    public string GTIN { get; set; }
        public bool DisplayStockQuantity { get; set; }
		public bool DisplayStockAvailability { get; set; }
	}

	public class CategoryTree
	{
		public Category Category { get; set; }
		public Category ParentCategory { get; set; } 
	}
}
