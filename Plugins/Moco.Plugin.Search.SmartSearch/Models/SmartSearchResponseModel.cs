using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Moco.Plugin.Search.SmartSearch.Models
{
	[XmlRoot(DataType = "string", ElementName = "hits")]
	public class SmartSearchResponseModel
	{
		private Int32 _count;
		private List<Suggest> _suggests = new List<Suggest>();
		private List<Hit> _hit = new List<Hit>();
		private String _facetGroups = string.Empty;
		private FacetQueries _facetQueries = new FacetQueries();
			
		[XmlElement(DataType = "int", ElementName = "count")]
		public Int32 Count { get { return _count; } set { _count = value; } }

		[XmlElement(ElementName = "suggest")]
		public List<Suggest> Suggest { get { return _suggests; } set { _suggests = value; } }

		[XmlElement(ElementName = "hit")]
		public List<Hit> Hit { get { return _hit; } set { _hit = value; } }

		[XmlElement(DataType = "string", ElementName = "facetgroups")]
		public String FacetGroups { get { return _facetGroups; } set { _facetGroups = value; } }

		[XmlElement(ElementName = "facetqueries")]
		public FacetQueries FacetQueries { get { return _facetQueries; } set { _facetQueries = value; } }
	}

	public class Suggest
	{
		[XmlElement(DataType = "string", ElementName = "original")]
		public String Original { get; set; }
		
		[XmlElement(DataType = "string", ElementName = "correction")]
		public String Correction { get; set; }
	}

	public class Hit
	{
		[XmlElement(DataType = "int", ElementName = "id")]
		public Int32 Id { get; set; }

		[XmlElement(ElementName = "document")]
		public Document Document { get; set; }
	}

	public class FacetQueries
	{
		[XmlElement(ElementName = "facetquery")]
		public List<FacetQuery> FacetQuery { get; set; } 
	}

	public class FacetQuery
	{
		[XmlElement(DataType = "string", ElementName = "field")]
		public String Field { get; set; }

		[XmlElement(DataType = "string", ElementName = "term")]
		public String Term { get; set; }
	}

	public class Document
	{
		[XmlElement(DataType = "int", ElementName = "ProductID")]
		public Int32 ProductId { get; set; }

		[XmlElement(DataType = "string", ElementName = "Name")]
		public String Name { get; set; }

		[XmlElement(DataType = "string", ElementName = "SeName")]
		public String SeName { get; set; }

		[XmlElement(DataType = "int", ElementName = "ImageID")]
		public Int32 ImageId { get; set; }

		[XmlElement(DataType = "string", ElementName = "ImageUrl")]
		public String ImageUrl { get; set; }

		[XmlElement(DataType = "string", ElementName = "IconImageUrl")]
		public String IconImageUrl { get; set; }

		[XmlElement(DataType = "decimal", ElementName = "Price")]
		public Decimal Price { get; set; }

		[XmlElement(DataType = "string", ElementName = "ShortDescription")] 
		public String ShortDescription;

		[XmlElement(DataType = "decimal", ElementName = "MinimumPrice")]
		public Decimal MinimumPrice { get; set; }

		[XmlElement(DataType = "decimal", ElementName = "SpecialPrice")]
		public Decimal SpecialPrice { get; set; }

		public DateTime CreatedOn { get; set; }

		public List<Variant> Variants { get; set; }

		[XmlElement(DataType = "string", ElementName = "Category")]
		public String Category { get; set; }

		[XmlElement(DataType = "int", ElementName = "CategoryID")]
		public Int32 CategoryId { get; set; }

		[XmlElement(DataType = "int", ElementName = "SalesCount")]
		public Int32 SalesCount { get; set; }

		[XmlElement(DataType = "int", ElementName = "AverageReview")]
		public Int32 AverageReview { get; set; }

		[XmlElement(DataType = "int", ElementName = "TotalReviews")]
		public Int32 TotalReviews { get; set; }

		[XmlElement(DataType = "boolean", ElementName = "AllowCustomerReviews")]
		public Boolean AllowCustomerReviews { get; set; }
				
		[XmlElement(DataType = "string", ElementName = "HasInventory")]
		public String HasInventory { get; set; }

	    public Boolean ShowBuyButton { get; set; }
        //TODO: Is this the same as ShowBuyButton?
        public bool DisableBuyButton { get; set; }

        public bool DisableWishlistButton { get; set; }

        public string PriceDisplay { get; set; }
	    public string MinimumPriceDisplay { get; set; }
        public string SpecialPriceDisplay { get; set; }
        #region

        /// <summary>
        /// Obsolete - Exists for backwards compatible serialization scenarios, use SeName
        /// </summary>
        [XmlElement(DataType = "string", ElementName = "SEName")]
	    public String SEName
	    {
	        get { return SeName; }
	        set
	        {
	            if (string.IsNullOrWhiteSpace(SeName)) { SeName = value; }
	        }
	    }

        #endregion
    }
}
