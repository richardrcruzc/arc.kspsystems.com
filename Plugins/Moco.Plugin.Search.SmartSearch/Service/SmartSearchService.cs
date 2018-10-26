using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;

namespace Moco.Plugin.Search.SmartSearch.Service
{
    using Models;

    public class SmartSearchService
	{
		private readonly IProductService _productService;
		private readonly ICategoryService _categoryService;
		private readonly IRepository<Order> _orderRepository;
		private readonly IPictureService _pictureService;
		private readonly SmartSearchSettings _smartSearchSettings;
		private readonly ILogger _logger;

	    private readonly ICacheManager _cacheManager;
	    private readonly ILocalizationService _localizationService;
	    private readonly MediaSettings _mediaSettings;
	    private readonly IWorkContext _workContext;
	    private readonly IStoreContext _storeContext;
	    private readonly IWebHelper _webHelper;
        
        public SmartSearchService(IProductService productService, ICategoryService categoryService
			, IRepository<Order> orderRepository, IPictureService pictureService
			, SmartSearchSettings smartSearchSettings, ILogger logger,
            ICacheManager cacheManager,
            ILocalizationService localizationService,
            MediaSettings mediaSettings,
            IWorkContext workContext,
            IStoreContext storeContext,
            IWebHelper webHelper)
        {
            _cacheManager = cacheManager;
            _localizationService = localizationService;
            _mediaSettings = mediaSettings;
            _workContext = workContext;
            _storeContext = storeContext;
            _webHelper = webHelper;

			_productService = productService;
			_categoryService = categoryService;
			_orderRepository = orderRepository;
			_pictureService = pictureService;
			_smartSearchSettings = smartSearchSettings;
			_logger = logger;
		}

		public SmartSearchService(){}
        
	    public void GenerateSmartSearchFeed(Stream outputStream)
	    {
	        if (_smartSearchSettings.Debug && null != _logger && _logger.IsEnabled(LogLevel.Information))
	        {
	            const string message = "GenerateSmartSearchFeed() Called: Feed Generation Started...";
	            _logger.Information(message);
	        }

	        RetrieveProductsForSmartSearchFeed(out var products, out var groupDictionary);
            
	        using (XmlWriter writer = XmlWriter.Create(outputStream, new XmlWriterSettings()))
	        {

	            //Start output SmartSearchFeed
                writer.WriteStartDocument();
	            writer.WriteStartElement("ITEMS", string.Empty);

	            XmlSerializer s = new XmlSerializer(typeof(ITEM), string.Empty);
	            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
	            ns.Add(string.Empty, "urn:SmartSearch");

	            foreach (Product product in products)
	            {
	                s.Serialize(writer, CreateFeedItem(product, product, true), ns);
	            }

	            foreach (IList<Product> groupedProducts in groupDictionary.Values)
	            {
	                Product product = groupedProducts.First(p => p.ParentGroupedProductId == 0);
	                Product topVariant = groupedProducts.OrderByDescending(p => p.Id).First(p => p.Id != product.Id);
	                s.Serialize(writer, CreateFeedItem(product, topVariant, false), ns);
	            }

	            //End output SmartSearchFeed
                writer.WriteEndElement();
	            writer.WriteEndDocument();
	            writer.Close();
	        }
        }
        
        public string GenerateSmartSearchFeedFile()
		{
		    if (_smartSearchSettings.Debug && null != _logger && _logger.IsEnabled(LogLevel.Information))
            {
				const string message = "GenerateSmartSearchFeedFile() Called: Feed Generation Started...";
				_logger.Information(message);
			}
			string returnValue;
            
			//Write To File
			try
			{
				string filePath = _smartSearchSettings.GetFullFilePath();
			    if (_smartSearchSettings.Debug && null != _logger && _logger.IsEnabled(LogLevel.Information))
                {
					string message = string.Format("GenerateSmartSearchFeedFile() Called: Writing out to file [{0}]", filePath);
					_logger.Information(message);
				}

			    string folderPath = CommonHelper.MapPath(_smartSearchSettings.FilePath);
                if (!Directory.Exists(folderPath))
				{
					//Create The Directory
					Directory.CreateDirectory(folderPath);
				}

			    filePath = CommonHelper.MapPath(filePath);
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
				{
				    //Get all Products
                    GenerateSmartSearchFeed(fs);
				}

				if (!File.Exists(filePath))
				{
				    const string message = "Smart Search File Was Not Created Due To An Internal Error.";
                    if (null != _logger && _logger.IsEnabled(LogLevel.Error))
					{
						_logger.Error(message);
					}
					throw new HttpException(500, message);
				}

				returnValue = string.Format("File Created In [{0}]", filePath);

			}
			catch (Exception ex)
			{
			    if (null != _logger && _logger.IsEnabled(LogLevel.Error))
                {
					_logger.Error(ex.Message, ex);
				}
				throw new HttpException(500, ex.Message);
			}			
			return returnValue;
		}

		public string InvokeSmartSearchIndexing()
		{
		    if (_smartSearchSettings.Debug && null != _logger && _logger.IsEnabled(LogLevel.Information))
            {
				_logger.Information("Called InvokeSmartSearchIndexing()");
			}
			string returnValue = string.Empty;
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_smartSearchSettings.GetSmartSearchIndexingPath());
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			using (Stream s = resp.GetResponseStream())
			{
			    if (null != s)
			    {
			        using (StreamReader sr = new StreamReader(s))
			        {
			            returnValue = sr.ReadToEnd();
			        }
			    }
			}
			return returnValue;
		}
        
        public void RetrieveSearchResults(ProductSearchModel searchModel)
		{
			string searchTerm = string.Format("searchString={0}", searchModel.q);
			string sortField = string.Empty;
			string resultsPerPage = string.Empty;
			string pageNumber = string.Empty;

		    HttpWebResponse resp = null;
			bool webRequestSuccess = false;
			bool deserializationSuccess = false;
		    SmartSearchResponseModel ssResponse = null;

			//Validate The SearchModel And Build The Search Query
			
			//Build The Sort Field
			if (!string.IsNullOrEmpty(searchModel.SelectedSortField))
			{
			    SearchSortingEnum searchSortingEnum;
			    var sortValue = Enum.TryParse(searchModel.SelectedSortField, out searchSortingEnum) ? EnumExtensions.GetEnumDisplayName(searchSortingEnum) : searchModel.SelectedSortField;
			    sortField = string.Format("&sortField={0}", sortValue);
			}
			//Build The Results Per Page Field
			if (searchModel.SelectedPagingField > 0)
			{
				resultsPerPage = string.Format("&pageSize={0}", searchModel.SelectedPagingField);
			}
			//Build The Current Page Field
			if (searchModel.SelectedPage > 0)
			{
				pageNumber = string.Format("&pageNumber={0}", searchModel.SelectedPage);
			}
			//Build the Url
			string filteredSearchUrl = string.Format("{0}?{1}{2}{3}{4}", _smartSearchSettings.GetSafeSmartSearchPath(), searchTerm, sortField, resultsPerPage, pageNumber);
				
			//Make A Web Call To SmartSearch And Retrieve The Results
			try
			{
				var req = (HttpWebRequest)WebRequest.Create(filteredSearchUrl);
				resp = (HttpWebResponse)req.GetResponse();
				if (resp.StatusCode == HttpStatusCode.OK)
				{
					webRequestSuccess = true;
				}
			}
			catch (Exception ex)
			{
			    if (null != _logger && _logger.IsEnabled(LogLevel.Error))
                {
					_logger.Error(ex.Message, ex);
				}
				webRequestSuccess = false;
			}
			if (webRequestSuccess)
			{
				using (Stream s = resp.GetResponseStream())
				{
					if (!string.IsNullOrEmpty(searchModel.q) && null != s)
					{
						XmlSerializer ser = new XmlSerializer(typeof(SmartSearchResponseModel));
						try
						{
							ssResponse = (SmartSearchResponseModel)ser.Deserialize(s);
							if (ssResponse != null)
							{
								deserializationSuccess = true;
							}
						}
						catch (Exception ex)
						{
						    if (null != _logger && _logger.IsEnabled(LogLevel.Error))
                            {
								_logger.Error(ex.Message, ex);
							}
							webRequestSuccess = false;
						}
					}
				}
			}
			if (webRequestSuccess && deserializationSuccess)
			{
				searchModel.Products = ssResponse;
				searchModel.ResultCount = ssResponse.Count;
				searchModel.InitPaging();
			}
			if (!webRequestSuccess || !deserializationSuccess)
			{
				searchModel.Warning = "Search Unavailable"; //This Should Be A String Resource
			}
			
		}
        
        private void RetrieveProductsForSmartSearchFeed(out IList<Product> products, out Dictionary<int, IList<Product>> groupDictionary)
	    {
            Stopwatch stopwatch = new Stopwatch();
            if (_smartSearchSettings.Debug)
            {
                stopwatch.Start();
            }

            //Get Separate Lists of each
            IList<Product> groupedProducts = _productService.SearchProducts(productType: ProductType.GroupedProduct);
            if (_smartSearchSettings.Debug && null != _logger && _logger.IsEnabled(LogLevel.Information))
            {
                _logger.Information(string.Format("Retrieve GroupedProducts: [{0}ms]", stopwatch.ElapsedMilliseconds));
            }

            IList<Product> simpleProducts = _productService.SearchProducts(productType: ProductType.SimpleProduct);
            if (_smartSearchSettings.Debug && null != _logger && _logger.IsEnabled(LogLevel.Information))
            {
                _logger.Information(string.Format("Retrieve SimpleProducts: [{0}ms]", stopwatch.ElapsedMilliseconds));
            }

            //Create HashTable To Store Grouped Product ID's
            groupDictionary = new Dictionary<int, IList<Product>>();
            foreach (var groupProduct in groupedProducts)
            {
                //setup keys in hashtable
                groupDictionary.Add(groupProduct.Id, new List<Product> { groupProduct });
            }
            if (_smartSearchSettings.Debug && null != _logger && _logger.IsEnabled(LogLevel.Information))
            {
                _logger.Information(string.Format("Build GroupedProducts Dictionary: [{0}ms]", stopwatch.ElapsedMilliseconds));
            }
            groupedProducts.Clear();
            
            if (groupDictionary.Any())
            {
                products = new List<Product>(simpleProducts.Count);
                foreach (Product product in simpleProducts)
                {
                    //if for some reason simple product has parent id but parent id doesn't exist in hashtable then 
                    //we will process the product as a simple product instead of a grouped product
                    if (product.ParentGroupedProductId > 0 && groupDictionary.ContainsKey(product.ParentGroupedProductId))
                    {
                        groupDictionary[product.ParentGroupedProductId].Add(product);
                    }
                    else
                    {
                        products.Add(product);
                    }
                }
                simpleProducts.Clear();
            }
            else
            {
                products = simpleProducts;
            }

            if (_smartSearchSettings.Debug && null != _logger && _logger.IsEnabled(LogLevel.Information))
            {
                _logger.Information(string.Format("Filter SimpleProducts: [{0}ms]", stopwatch.ElapsedMilliseconds));
            }
	        if (_smartSearchSettings.Debug)
	        {
	            stopwatch.Stop();
	        }
        }
        
		private ITEM CreateFeedItem(Product product, Product topVariant, bool isSimpleProduct)
		{
			ITEM ssItem = new ITEM();
			
            
			//GETPRODUCTDATA
			ssItem.ProductID = product.Id;
			ssItem.ProductVariantID = topVariant.Id;
			ssItem.Name = StripNonPrintableAsciiChars(product.Name);                 ////////
		    ssItem.SbName = ssItem.Name;//product.Name;               ////////
            ssItem.SeName = StripNonPrintableAsciiChars(product.GetSeName(0));       ////////

            //Get the image data off of the top variant
		    //var picture = product.ProductPictures.FirstOrDefault();
		    if (_smartSearchSettings.IncludeProductImage && product.ProductPictures.Any())
		    {
		        PictureModel defaultPictureModel = GetDefaultPictureModel(product);

		        //ssItem.ImageID = pictureModel.ImageUrl picture.Id;
		        ssItem.ImageUrl = defaultPictureModel.FullSizeImageUrl; //_pictureService.GetPictureUrl(picture.Id, _smartSearchSettings.ImageSize);
		        ssItem.IconImageUrl = defaultPictureModel.ImageUrl; //_pictureService.GetPictureUrl(picture.Id, _smartSearchSettings.ImageIconSize);
            }
            
			ssItem.ProductUrl = ""; //TODO?!?!?!
			ssItem.Body = StripNonPrintableAsciiChars(product.FullDescription);              ////////
            ssItem.SbBody = StripNonPrintableAsciiChars(product.ShortDescription);           ////////
            ssItem.ShortDescription = StripNonPrintableAsciiChars(product.ShortDescription); ////////
            ssItem.IsFreeShipping = topVariant.IsFreeShipping;

            var productManufacturer = product.ProductManufacturers.OrderByDescending(t => t.Id)
                .FirstOrDefault(t => t.Manufacturer != null);
		    if (null != productManufacturer && null != productManufacturer.Manufacturer)
		    {
		        ssItem.Manufacturer = StripNonPrintableAsciiChars(productManufacturer.Manufacturer.Name);
            }

            ssItem.Price = topVariant.Price;
			ssItem.MinimumPrice = product.Price;
			ssItem.OldPrice = topVariant.OldPrice;

		    TierPrice tierPrice = null;
            if (topVariant.HasTierPrices)
		    {
		        //TODO: Filter by Store and CustomerRole - CustomerRole==null is ALL
                tierPrice = topVariant.TierPrices.First(price => price.CustomerRole == null );
            }

		    if (null != tierPrice)
			{
                //t.StartDateTimeUtc
                //t.EndDateTimeUtc
                //t.Price
                //t.CustomerRole  t.CustomerRoleId
                //t.Product     t.ProductId
                //t.Quantity
                //t.StoreId

				if (!tierPrice.EndDateTimeUtc.HasValue && !tierPrice.StartDateTimeUtc.HasValue)
				{
					ssItem.SpecialPrice = tierPrice.Price;
					ssItem.SortPrice = tierPrice.Price;
				}
				else if (!tierPrice.EndDateTimeUtc.HasValue &&
							DateTime.Now.ToUniversalTime() >= tierPrice.StartDateTimeUtc)
				{
					ssItem.SpecialPrice = tierPrice.Price;
					ssItem.SortPrice = tierPrice.Price;
				}
				else if (DateTime.Now.ToUniversalTime() <= tierPrice.EndDateTimeUtc &&
							!tierPrice.StartDateTimeUtc.HasValue)
				{
					ssItem.SpecialPrice = tierPrice.Price;
					ssItem.SortPrice = tierPrice.Price;
				}
				else if (DateTime.Now.ToUniversalTime() <= tierPrice.EndDateTimeUtc &&
							DateTime.Now.ToUniversalTime() >= tierPrice.StartDateTimeUtc)
				{
					ssItem.SpecialPrice = tierPrice.Price;
					ssItem.SortPrice = tierPrice.Price;
				}
			}
			else
			{
				ssItem.SpecialPrice = 0;
				ssItem.SortPrice = topVariant.Price;
			}
			ssItem.Weight = topVariant.Weight;
			ssItem.Length = topVariant.Length;
			ssItem.Width = topVariant.Width;
			ssItem.Height = topVariant.Height;
			ssItem.ShowBuyButton = !topVariant.DisableBuyButton;
			ssItem.CreatedOn = product.CreatedOnUtc;
		    ssItem.PriceRange = GetPriceRange(topVariant.Price);


		    //GETCATEGORYDATA
		    if (!_smartSearchSettings.ExcludeProductCategory && product.ProductCategories.Any())
		    {
		        //Lets get the count of mapped categories in the product

		        var categoryTree = GetProductCategoryTree(product.ProductCategories);
		        ssItem.CategoryTree = string.Join(",", categoryTree.Select(t => t.Id));

		        Category defaultCategory = product.ProductCategories.OrderBy(t => t.CategoryId)
		            .Where(t => t.Category != null)
		            .Select(t => t.Category)
		            .FirstOrDefault();
		        if (null != defaultCategory)
		        {
		            ssItem.Category = StripNonPrintableAsciiChars(defaultCategory.Name);
		            ssItem.CategoryID = defaultCategory.Id;
                }
		        
		    }
            
		    if (_smartSearchSettings.IncludeProductSaleCount)
		    {
		        ssItem.SalesCount =
		        (from o in _orderRepository.Table
		            from y in o.OrderItems
		            where y.ProductId == topVariant.Id
		            select y.Id).Count();
		    }

		    ssItem.AverageReview = product.ApprovedRatingSum; //ApprovedTotalReviews
			ssItem.TotalReviews = product.ApprovedTotalReviews;
			ssItem.AllowCustomerReviews = product.AllowCustomerReviews;
			ssItem.ContentType = "Product";
			ssItem.HasInventory = topVariant.StockQuantity > 0 ? "In Stock" : "Not in Stock";
			ssItem.SKU = topVariant.Sku;
		    ssItem.GTIN = product.Gtin;
		    ssItem.ManufacturerPartNumber = product.ManufacturerPartNumber;
            
            

			List<Variant> variantList = new List<Variant>();

			if (!isSimpleProduct)
			{
                //Get Associated Products
			    IList<Product> associatedProducts = _productService.GetAssociatedProducts(product.Id);
                foreach (var prodVariant in associatedProducts)
				{
					variantList.Add(BuildVariant(prodVariant, topVariant.Id));
				}
			}

			ssItem.Variants = variantList;
			return ssItem;
		}

	    private int GetPriceRange(decimal price)
	    {
	        if (price >= 0 && price < 50)
	        {
	            return 0;
	        }
	        if (price >= 50 && price < 100)
	        {
	            return 1;
	        }
	        if (price >= 100 && price < 200)
	        {
	            return 2;
	        }
	        if (price >= 200 && price < 300)
	        {
	            return 3;
	        }
	        if (price >= 300 && price < 400)
	        {
	            return 4;
	        }
	        if (price >= 400 && price < 500)
	        {
	            return 5;
	        }
	        
	        return 6;
        }

	    private Variant BuildVariant(Product prodVariant, int topVariantId)
	    {
	        return new Variant
	        {
	            VariantID = prodVariant.Id,
	            IsDefault = prodVariant.Id.Equals(topVariantId),
	            Price = prodVariant.Price,
	            OldPrice = prodVariant.OldPrice,
	            Inventory = prodVariant.StockQuantity,
	            DisplayOrder = prodVariant.DisplayOrder,
	            IsFreeShipping = prodVariant.IsFreeShipping,
	            SKU = prodVariant.Sku,
	            GTIN = prodVariant.Gtin,
	            ManufacturerPartNumber = prodVariant.ManufacturerPartNumber,
                DisplayStockQuantity = prodVariant.DisplayStockQuantity,
	            DisplayStockAvailability = prodVariant.DisplayStockAvailability,
	        };
	    }
        
		private List<Category> GetProductCategoryTree(ICollection<ProductCategory> productCategories)
		{
			List<Category> productCatTree = new List<Category>();

			foreach (var cat in productCategories)
			{
				Category currentCategory = cat.Category;
				if (currentCategory != null)
				{
					do
					{
						productCatTree.Add(currentCategory);
						currentCategory =
							_categoryService.GetCategoryById(currentCategory.ParentCategoryId);
					} while (currentCategory != null);	
				}
			}

			return productCatTree;
		}

	    public string PushFeedToHss(Uri hostedUrl)
	    {
	        string text = string.Empty;
	        Stopwatch stopwatch = new Stopwatch();
            if (_smartSearchSettings.Debug && null != _logger && _logger.IsEnabled(LogLevel.Information))
	        {
	            stopwatch.Start();
	            const string message = "PushFeedToHSS() Called: Sending data to HSS...";
	            _logger.Information(message);
	        }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(hostedUrl);
	        request.Method = "POST";
	        request.ContentType = "text/xml";
	        request.Accept = "application/json";
	        request.Timeout = 600000;

            GenerateSmartSearchFeed(request.GetRequestStream());
            
	        using (var response = (HttpWebResponse) request.GetResponse())
	        {
	            using (var stream = response.GetResponseStream())
	            {
	                if (null != stream)
	                {
	                    using (var sr = new StreamReader(stream))
	                    {
	                        text = sr.ReadToEnd();
	                    }
	                }
	            }
	        }

	        if (_smartSearchSettings.Debug && null != _logger && _logger.IsEnabled(LogLevel.Information))
	        {
	            const string message = "PushFeedToHSS() Called: Sending data to HSS Done...";
	            _logger.Information(message);
	        }
            
            return text;
	    }
        
	    private string StripNonPrintableAsciiChars(string tmpContents) {

            if (string.IsNullOrWhiteSpace(tmpContents)) return string.Empty;
            const string pattern = @"[^\u0020-\u007E]";

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            if (regex.IsMatch(tmpContents))
            {
                return regex.Replace(tmpContents, string.Empty);
            }
            return tmpContents;
        }

	    public PictureModel GetDefaultPictureModel(Product product, bool isAssociatedProduct = false)
	    {
	        string productName = product.GetLocalized(x => x.Name);
            return GetDefaultPictureModel(product.Id, productName, isAssociatedProduct);
	    }

        public PictureModel GetDefaultPictureModel(int productId, string productName, bool isAssociatedProduct = false)
	    {
	        //Borrowed from: Nop.Web.Controllers.ProductController

            var defaultPictureSize = isAssociatedProduct ?
	            _mediaSettings.AssociatedProductPictureSize :
	            _mediaSettings.ProductDetailsPictureSize;
	        //prepare picture models
	        var productPicturesCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DETAILS_PICTURES_MODEL_KEY, productId, defaultPictureSize, isAssociatedProduct, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
	        var cachedPictures = _cacheManager.Get(productPicturesCacheKey, () =>
	        {
	            var pictures = _pictureService.GetPicturesByProductId(productId);
	            var defaultPicture = pictures.FirstOrDefault();
	            var defaultPictureModel = new PictureModel
	            {
	                ImageUrl = _pictureService.GetPictureUrl(defaultPicture, defaultPictureSize, !isAssociatedProduct),
	                FullSizeImageUrl = _pictureService.GetPictureUrl(defaultPicture, 0, !isAssociatedProduct),
	                Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), productName),
	                AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), productName),
	            };
	            //"title" attribute
	            defaultPictureModel.Title = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.TitleAttribute)) ?
	                defaultPicture.TitleAttribute :
	                string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), productName);
	            //"alt" attribute
	            defaultPictureModel.AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
	                defaultPicture.AltAttribute :
	                string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), productName);

	            //all pictures
	            var pictureModels = new List<PictureModel>();
	            foreach (var picture in pictures)
	            {
	                var pictureModel = new PictureModel
	                {
	                    ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage),
	                    FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
	                    Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), productName),
	                    AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), productName),
	                };
	                //"title" attribute
	                pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
	                    picture.TitleAttribute :
	                    string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), productName);
	                //"alt" attribute
	                pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
	                    picture.AltAttribute :
	                    string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), productName);

	                pictureModels.Add(pictureModel);
	            }

	            return new { DefaultPictureModel = defaultPictureModel, PictureModels = pictureModels };
	        });
	        return cachedPictures.DefaultPictureModel;
	    }
        
        #region ftp

	    public string UploadFeed()
	    {
	        if (_smartSearchSettings.Debug && _logger != null && _logger.IsEnabled(LogLevel.Information))
	        {
	            _logger.Information("Called UploadFeed()");
	        }
	        string response;
	        bool uploadSuccess = false;

	        FtpConnectionInfo cInfo = ValidateFtpCredentials(_smartSearchSettings.FtpAddress, _smartSearchSettings.FtpPort, _smartSearchSettings.Username, _smartSearchSettings.Password);

	        if (!cInfo.IsError)
	        {
	            string filePath = CommonHelper.MapPath(_smartSearchSettings.GetFullFilePath());
	            if (File.Exists(filePath))
	            {
	                FileInfo file = new FileInfo(filePath);
	                uploadSuccess = UploadFeed(cInfo, file);
	            }
	            else
	            {
	                cInfo.ErrorMessage = string.Format("SmartSearch Feed File Does Not Exists At The Following Location: {0}", filePath);
	                if (_logger != null && _logger.IsEnabled(LogLevel.Information))
	                {
	                    _logger.Information(cInfo.ErrorMessage);
	                }
	            }
	        }
	        if (!uploadSuccess || cInfo.IsError)
	        {
	            response = string.Format("An Error Occured While Trying To Upload The SmartSearch Feed To The Server. ErrorDetails: {0}", cInfo.ErrorMessage);
	            if (_logger != null && _logger.IsEnabled(LogLevel.Information))
	            {
	                _logger.Information(response);
	            }
	        }
	        else
	        {
	            response = "Upload Succeeded";
	        }
	        return response;
	    }

        private FtpConnectionInfo ValidateFtpCredentials(string ftpAddress, int port, string userName, string password)
        {
            FtpConnectionInfo connectionInfo = new FtpConnectionInfo()
            {
                FtpAddress = ftpAddress,
                Port = port,
                UserName = userName,
                Password = password
            };
            Boolean isError = true;
            FtpWebResponse response = null;

            try
            {
                response = FtpRequest(ftpAddress, port, new NetworkCredential(userName, password), "ListDirectory");
            }
            catch (WebException ex)
            {
                connectionInfo.ErrorMessage = ex.Message;
                //log the exception
            }
            if (response != null && (response.StatusCode == FtpStatusCode.DataAlreadyOpen || response.StatusCode == FtpStatusCode.CommandOK || response.StatusCode == FtpStatusCode.DataAlreadyOpen || response.StatusCode == FtpStatusCode.ConnectionClosed || response.StatusCode == FtpStatusCode.ClosingData))
            {
                isError = false;
            }
            connectionInfo.IsError = isError;

            return connectionInfo;
        }

        private bool UploadFeed(FtpConnectionInfo connectionInfo, FileInfo file)
        {
            bool isSuccess = false;
            string readFileName = file.FullName;
            string writeFileName = file.Name;

            byte[] fileBuffer;
            using (FileStream fs = new FileStream(readFileName, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string dat = sr.ReadToEnd();
                    fileBuffer = Encoding.ASCII.GetBytes(dat);
                }
            }

            try
            {
                var response = FtpRequest(connectionInfo.FtpAddress, connectionInfo.Port, new NetworkCredential(connectionInfo.UserName, connectionInfo.Password), "Upload", fileBuffer, writeFileName);
                if (response != null && (response.StatusCode == FtpStatusCode.DataAlreadyOpen || response.StatusCode == FtpStatusCode.CommandOK || response.StatusCode == FtpStatusCode.DataAlreadyOpen || response.StatusCode == FtpStatusCode.ConnectionClosed || response.StatusCode == FtpStatusCode.ClosingData))
                {
                    isSuccess = true;
                }
            }
            catch (WebException ex)
            {
                if (null != _logger && _logger.IsEnabled(LogLevel.Error))
                {
                    _logger.Error(ex.Message, ex);
                }
                throw;
            }


            return isSuccess;
        }

        private FtpWebResponse FtpRequest(string ftpAddress, int port, NetworkCredential credential, string requestMethod, byte[] content = null, string contentFileName = null)
        {
            FtpWebResponse resp;
            string ftpRequestAddress = string.Format("ftp://{0}:{1}", ftpAddress, port);
            if (!string.IsNullOrEmpty(contentFileName))
            {
                ftpRequestAddress += string.Format("/{0}", contentFileName);
            }
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(ftpRequestAddress);
            req.Credentials = credential;
            switch (requestMethod)
            {
                case "ListDirectory":
                    req.Method = WebRequestMethods.Ftp.ListDirectory;
                    break;
                case "Upload":
                    req.Method = WebRequestMethods.Ftp.UploadFile;
                    break;
                default:
                    throw new NotSupportedException("The Request Method Supplied Is Currently Not Supported");
            }
            if (content != null && content.Length > 0)
            {
                req.ContentLength = content.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(content, 0, content.Length);
                }
            }

            try
            {
                resp = (FtpWebResponse)req.GetResponse();
            }
            catch (WebException ex)
            {
                if (null != _logger && _logger.IsEnabled(LogLevel.Error))
                {
                    _logger.Error(ex.Message, ex);
                }
                throw;
            }

            return resp;
        }
        #endregion
    }

    /// <summary>
    /// I will move this to an appropriate place once I have the functionality worked out
    /// </summary>
    public class FtpConnectionInfo
	{
		public string FtpAddress { get; set; }
		public int Port { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public bool IsError { get; set; }
		public string ErrorMessage { get; set; }
	}
}
