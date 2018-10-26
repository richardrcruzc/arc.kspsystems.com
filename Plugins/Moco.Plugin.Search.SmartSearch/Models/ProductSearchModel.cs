using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq; 

namespace Moco.Plugin.Search.SmartSearch.Models
{
	public class ProductSearchModel
	{
		private Int32 _selectedPagingField = 10;
		private String _selectedSortField = "Relevance";
		private Int32 _selectedPage = 1;
		private String _viewMode = "grid";

		public String SelectedSortField 
		{
			get { return _selectedSortField; }
			set { _selectedSortField = value; }
		}
		public Int32 SelectedPagingField {
			get { return _selectedPagingField; }
			set { _selectedPagingField = value; }
		}
		public Int32 SelectedPage 
		{
			get { return _selectedPage; }
			set { _selectedPage = value; }
		}

		public String ViewMode
		{
			get { return _viewMode; }
			set { _viewMode = value; }
		}

		public String q { get; set; }
		public String Warning { get; set; }
		public Int32 ResultCount { get; set; }
		public List<SelectListItem> SortFields { get; set; }
		public List<SelectListItem> PagingFields { get; set; }
		public Dictionary<String, Int32> Pages { get; set; }
		public Int32 NextPageValue { get; set; }
		public Int32 PreviousPageValue { get; set; }

	    public bool DisableAddToCompareListButton { get; set; }
        public bool DisableBuyButton { get; set; }
	    public bool DisableWishlistButton { get; set; }

        public SmartSearchResponseModel Products { get; set; }

		public ProductSearchModel()
		{
			ResultCount = 0;
			Products = new SmartSearchResponseModel();
			SortFields = new List<SelectListItem>()
			{
				new SelectListItem() {Text = "Relevance", Value = "Relevance"},
				new SelectListItem() {Text = "Highest Rated", Value = "Highest Rated"},
				new SelectListItem() {Text = "Best Sellers", Value = "Best Sellers"},
				new SelectListItem() {Text = "New Arrivals", Value = "New Arrivals"},
				new SelectListItem() {Text = "Lowest Price", Value = "Lowest Price"},
				new SelectListItem() {Text = "Highest Price", Value = "Highest Price"}
			};
			PagingFields = new List<SelectListItem>()
				{
					new SelectListItem() {Text = "3", Value = "3"},
					new SelectListItem() {Text = "6", Value = "6"},
					new SelectListItem() {Text = "9", Value = "9"},
					new SelectListItem() {Text = "18", Value = "18"}
				};
			InitPaging();
		}
 
		public ProductSearchModel(SmartSearchResponseModel ssResponseModel)
		{
			ResultCount = ssResponseModel.Count;
			Products = ssResponseModel;
			SortFields = new List<SelectListItem>()
			{
				new SelectListItem() {Text = "Relevance", Value = "Relevance"},
				new SelectListItem() {Text = "Highest Rated", Value = "Highest Rated"},
				new SelectListItem() {Text = "Best Sellers", Value = "Best Sellers"},
				new SelectListItem() {Text = "New Arrivals", Value = "New Arrivals"},
				new SelectListItem() {Text = "Lowest Price", Value = "Lowest Price"},
				new SelectListItem() {Text = "Highest Price", Value = "Highest Price"}
			};

			PagingFields = new List<SelectListItem>()
				{
					new SelectListItem() {Text = "3", Value = "3"},
					new SelectListItem() {Text = "6", Value = "6"},
					new SelectListItem() {Text = "9", Value = "9"},
					new SelectListItem() {Text = "18", Value = "18"}
				};
			InitPaging();
		}

		public void InitPaging()
		{
			Pages = new Dictionary<string, int>();
			decimal pagesToDisplay = ResultCount == 0 ? 1 : Math.Ceiling(((decimal)ResultCount / SelectedPagingField));
			for (int i = 0; i < pagesToDisplay; i++)
			{
				Pages.Add(i.ToString(), i + 1);
			}

			if (Pages.Values.Count.Equals(0))
			{
				NextPageValue = 1;
			}
			else if (_selectedPage + 1 > Pages.Values.Last())
			{
				NextPageValue = Pages.Values.Last();
			}
			else
			{
				NextPageValue = _selectedPage + 1;
			}

			if (Pages.Values.Count.Equals(0))
			{
				PreviousPageValue = 1;
			}
			else if (_selectedPage - 1 < Pages.Values.First())
			{
				PreviousPageValue = Pages.Values.First();
			}
			else
			{
				PreviousPageValue = _selectedPage - 1;
			}
		}
	}
}
