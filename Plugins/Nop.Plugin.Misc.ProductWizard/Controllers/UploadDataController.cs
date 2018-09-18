using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Plugin.Misc.ProductWizard.Domain;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Framework.Controllers;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Controllers
{
    public class UploadDataController : BasePluginController
    {

        private readonly ICustomNumberFormatter _customNumberFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IAddressService _addressService; 
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly CustomerSettings _customerSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;

        //it's quite fast hash (to cheaply distinguish between objects)
        private const string IMAGE_HASH_ALGORITHM = "SHA1";

        private readonly IRepository<Groups> _gpRepository;
        private readonly IRepository<GroupsItems> _gpiRepository;
        private readonly IRepository<RelationsGroupsItems> _rgpRepository;
        private readonly IRepository<ItemsCompatability> _iRepository;
        private readonly IRepository<LegacyId> _lRepository;


        private readonly IRepository<Category> _categoryRepository;

        private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreContext _storeContext;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IEncryptionService _encryptionService;
        private readonly IDataProvider _dataProvider;
        private readonly MediaSettings _mediaSettings;
        private readonly IVendorService _vendorService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IShippingService _shippingService;
        private readonly IDateRangeService _dateRangeService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IMeasureService _measureService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IProductTagService _productTagService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly VendorSettings _vendorSettings;
        private readonly ISpecificationAttributeService _specificationAttributeService;

        private readonly IPermissionService _permissionService;
        private readonly IDbContext _dbContext;
        public UploadDataController(
             ICustomNumberFormatter customNumberFormatter,
            IOrderProcessingService orderProcessingService,
            IAddressService addressService,
            ICustomerRegistrationService  customerRegistrationService,
            CustomerSettings customerSettings,
            IGenericAttributeService  genericAttributeService,
            ICustomerService  customerService,
            IOrderService orderService,
               IRepository<LegacyId> lRepository,
             IRepository<ItemsCompatability> iRepository,
            IRepository<Groups> gpRepository,
              IRepository<GroupsItems> gpiRepository,
                IRepository<RelationsGroupsItems> rgpRepository,
            IRepository<Category> categoryRepository,
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IPictureService pictureService,
            IUrlRecordService urlRecordService,
            IStoreContext storeContext,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IEncryptionService encryptionService,
            IDataProvider dataProvider,
            MediaSettings mediaSettings,
            IVendorService vendorService,
            IProductTemplateService productTemplateService,
            IShippingService shippingService,
            IDateRangeService dateRangeService,
            ITaxCategoryService taxCategoryService,
            IMeasureService measureService,
            IProductAttributeService productAttributeService,
            CatalogSettings catalogSettings,
            IProductTagService productTagService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            VendorSettings vendorSettings,
            ISpecificationAttributeService specificationAttributeService,
            IDbContext dbContext, IPermissionService permissionService)
        {
            this._customNumberFormatter = customNumberFormatter;
            this._orderProcessingService = orderProcessingService;
            this._addressService = addressService;
            this._customerRegistrationService = customerRegistrationService;
            this._customerSettings = customerSettings;
            this._genericAttributeService = genericAttributeService;
            this._customerService = customerService;
            this._orderService = orderService;
            this._categoryRepository = categoryRepository;
            this._productService = productService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._pictureService = pictureService;
            this._urlRecordService = urlRecordService;
            this._storeContext = storeContext;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._encryptionService = encryptionService;
            this._dataProvider = dataProvider;
            this._mediaSettings = mediaSettings;
            this._vendorService = vendorService;
            this._productTemplateService = productTemplateService;
            this._shippingService = shippingService;
            this._dateRangeService = dateRangeService;
            this._taxCategoryService = taxCategoryService;
            this._measureService = measureService;
            this._productAttributeService = productAttributeService;
            this._catalogSettings = catalogSettings;
            this._productTagService = productTagService;
            this._workContext = workContext;
            this._localizationService = localizationService;
            this._customerActivityService = customerActivityService;
            this._vendorSettings = vendorSettings;
            this._specificationAttributeService = specificationAttributeService;

            this._dbContext = dbContext;
            this._permissionService = permissionService;

            this._lRepository = lRepository;
            this._iRepository = iRepository;
            this._gpRepository = gpRepository;
            this._gpiRepository = gpiRepository;
            this._rgpRepository = rgpRepository;

        }

        public virtual IActionResult Index()
        {

            return View("~/Plugins/Misc.ProductWizard/Views/UploadData.cshtml");
        }
        [HttpPost]
        public virtual IActionResult ImportProductsImgesFromFile(string imagePath = @"D:\data\items")
        {

            imagePath = System.IO.Directory.GetCurrentDirectory() + "/items"; // Server.MapPath("~/items");


            DirectoryInfo d = new DirectoryInfo(imagePath);//Assuming Test is your Folder
            var allDirectory = d.EnumerateDirectories().ToList();
            foreach (var dir in allDirectory)
            {
                string idString = dir.Name.ToString();
                idString = idString.Replace("itemid_", "");

                int.TryParse(idString, out int id);

                if (id > 0)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {

                        var txt = dir.GetFiles("*.txt").FirstOrDefault();

                        var description = System.IO.File.ReadAllText(txt.FullName);
                        description = description.Replace("&lt;", "").Replace("p&gt;", "").Replace("/", "");

                        FileInfo[] Files = dir.GetFiles("*.jpg");


                        //take only image with not thumbnail
                        foreach (FileInfo file in Files.Where(x => !x.FullName.Contains("thumbnail")))
                        {
                            if (file.FullName.Contains("thumbnail"))
                                continue;

                            var picturePath = file.FullName;

                            var mimeType = GetMimeTypeFromFilePath(file.FullName);
                            var newPictureBinary = System.IO.File.ReadAllBytes(picturePath);

                            var pictureAlreadyExists = false;

                            //compare with existing product pictures
                            var existingPictures = _pictureService.GetPicturesByProductId(product.Id);
                            foreach (var existingPicture in existingPictures)
                            {
                                var existingBinary = _pictureService.LoadPictureBinary(existingPicture);
                                //picture binary after validation (like in database)
                                var validatedPictureBinary = _pictureService.ValidatePicture(newPictureBinary, mimeType);
                                if (!existingBinary.SequenceEqual(validatedPictureBinary) &&
                                    !existingBinary.SequenceEqual(newPictureBinary))
                                    continue;
                                //the same picture content
                                pictureAlreadyExists = true;
                                break;
                            }

                            if (!pictureAlreadyExists)
                            {
                                var newPicture = _pictureService.InsertPicture(newPictureBinary, mimeType, _pictureService.GetPictureSeName(description));
                                product.ProductPictures.Add(new ProductPicture
                                {
                                    //EF has some weird issue if we set "Picture = newPicture" instead of "PictureId = newPicture.Id"
                                    //pictures are duplicated
                                    //maybe because entity size is too large
                                    PictureId = newPicture.Id,
                                    DisplayOrder = 1,
                                });
                                _productService.UpdateProduct(product);

                            }


                        }
                    }
                }
            }
            return View("~/Plugins/Misc.ProductWizard/Views/UploadData.cshtml");
        }
        protected virtual string GetMimeTypeFromFilePath(string filePath)
        {
            //TODO test ne implementation
            new FileExtensionContentTypeProvider().TryGetContentType(filePath, out string mimeType);
            //set to jpeg in case mime type cannot be found
            if (mimeType == null)
                mimeType = MimeTypes.ImageJpeg;
            return mimeType;
        }


        [HttpPost]
        public virtual IActionResult ImportProductFromXlsx(IFormFile importexcelfile)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor cannot import categories
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    Stream stream = importexcelfile.OpenReadStream();
                    using (var xlPackage = new ExcelPackage(stream))
                    {
                        var maximunRows = 0;
                        var endRow = 2;
                        var countCategorysInFile = 0;


                        //category
                        // get the second worksheet in the workbook
                        var worksheet = xlPackage.Workbook.Worksheets[2];
                        if (worksheet == null)
                            throw new NopException("No worksheet found");

                        try
                        {
                            var tmpSql = "delete [dbo].[Manufacturer] where [Name]='Y'; delete [dbo].[Manufacturer] where [Name]='N';";
                            _dbContext.ExecuteSqlCommand(tmpSql);
                        }
                        catch
                        { }

                        var sql = "SET IDENTITY_INSERT [dbo].[Category] ON;";
                        var id = 0;
                        var name = string.Empty;
                        //find end of data
                        while (true)
                        {
                            try
                            {
                                //if (worksheet.Row(endRow).OutlineLevel == 0)
                                //{
                                //    break;
                                //}
                                if (worksheet == null || worksheet.Cells == null)
                                    break;
                                if (worksheet.Cells[endRow, 1].Value == null)
                                    break;

                                id = int.Parse(worksheet.Cells[endRow, 1].Value.ToString());
                                name = worksheet.Cells[endRow, 2].Value.ToString();

                                //var exist = _categoryService.GetCategoryById(id);

                                //if (exist == null)


                                sql += $"update [dbo].[Category] set    [Name] ='{name}' , PageSize=6  where id =  {id}";
                                sql += " IF @@ROWCOUNT = 0 ";
                                sql += " INSERT INTO [dbo].[Category] (Id,[Name], UpdatedOnUtc,CreatedOnUtc, CategoryTemplateId, ParentCategoryId, " +
                                       "PictureId,  AllowCustomersToSelectPageSize,ShowOnHomePage,IncludeInTopMenu,SubjectToAcl,LimitedToStores,Deleted,DisplayOrder,Published, PageSize) " +
                                       $" SELECT {id},'{name}',getdate(),getdate(), 1,0,0,1,0,0,0,0,0,0,1,5; ";


                                countCategorysInFile += 1;

                                endRow++;
                                continue;
                            }
                            catch (Exception ex)
                            {
                                endRow++;
                                ErrorNotification("Admin.Common.UploadFile" + ex.Message);
                                continue;
                            }
                        }

                        sql += "SET IDENTITY_INSERT [dbo].[Category] OFF;";
                        try
                        {
                            _dbContext.ExecuteSqlCommand(sql);
                        }
                        catch (Exception ex)
                        {
                            ErrorNotification("Admin.Common.Category" + ex.Message);

                        }
                        SuccessNotification("Admin.Catalog.Categories.Imported");
                        //Manufacturer
                        // get the 3er worksheet in the workbook
                        worksheet = xlPackage.Workbook.Worksheets[3];
                        if (worksheet == null)
                            throw new NopException("No worksheet found");
                        endRow = 2;
                        sql = "SET IDENTITY_INSERT [dbo].[Manufacturer] ON;";
                        id = 0;
                        name = string.Empty;
                        //find end of data
                        while (true)
                        {
                            try
                            {
                                //if (worksheet.Row(endRow).OutlineLevel == 0)
                                //{
                                //    break;
                                //}
                                if (worksheet == null || worksheet.Cells == null)
                                    break;
                                if (worksheet.Cells[endRow, 1].Value == null)
                                    break;

                                id = int.Parse(worksheet.Cells[endRow, 1].Value.ToString());
                                name = worksheet.Cells[endRow, 2].Value.ToString();

                                //var exist = _manufacturerService.GetManufacturerById(id);
                                //if (exist == null)

                                sql += $" update [dbo].[Manufacturer] set [Name] ='{name}', UpdatedOnUtc=getdate() where id ={id} ";
                                sql += " IF @@ROWCOUNT = 0 ";
                                sql += " INSERT INTO [dbo].[Manufacturer] (Id,[Name], UpdatedOnUtc,CreatedOnUtc, " +
                                           " PictureId, PageSize, AllowCustomersToSelectPageSize,SubjectToAcl,LimitedToStores,Deleted,DisplayOrder,Published,ManufacturerTemplateId) " +
                                           $" SELECT {id},'{name}',getdate(),getdate(), 0,1,0,0,0,0,0,1,0; ";



                                countCategorysInFile += 1;

                                endRow++;
                                continue;
                            }
                            catch (Exception ex)
                            {
                                endRow++;
                                ErrorNotification("Admin.Common.UploadFile" + ex.Message);
                                continue;
                            }
                        }

                        sql += "SET IDENTITY_INSERT [dbo].[Manufacturer] OFF;";
                        try
                        {
                            _dbContext.ExecuteSqlCommand(sql);
                        }
                        catch (Exception ex)
                        {
                            ErrorNotification("Admin.Common.Manufacturer" + ex.Message);

                        }
                        SuccessNotification("Admin.Catalog.Manufacturer.Imported");




                        //products
                        // get the second worksheet in the workbook
                        worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                            throw new NopException("No worksheet found");
                        endRow = 2;
                        sql = "SET IDENTITY_INSERT [dbo].[Product] ON;";
                        id = 0;
                        name = string.Empty; 
                        var SKU = string.Empty;
                        var Gtin = string.Empty;
                        var StockQuantity = 0;
                        decimal Price = 0M;
                        var Weight = 0;
                        var Length = 0;
                        var Height = 0;
                        var Width = 0;
                        var Categories = 0; 
                        var picture = string.Empty;
                        var ExcludeGoogleFeed = 1;
                        var Active = 1;
                        var BrandID = 0;
                        var DropShip = 0;
                        var color = string.Empty;
                        //find end of data
                        while (true)
                        {
                            try
                            {
                                //if (worksheet.Row(endRow).OutlineLevel == 0)
                                //{
                                //    break;
                                //}
                                if (worksheet == null || worksheet.Cells == null)
                                    break;
                                if (worksheet.Cells[endRow, 1].Value == null)
                                    break;

                                int.TryParse(worksheet.Cells[endRow, 1].Value.ToString(), out id);


                                Active = worksheet.Cells[endRow, 2].Value.ToString() == "Y" ? 1 : 0;

                                int.TryParse(worksheet.Cells[endRow, 3].Value.ToString(), out Categories);

                                int.TryParse(worksheet.Cells[endRow, 4].Value.ToString(), out BrandID);

                                SKU = worksheet.Cells[endRow, 5].Value.ToString().Trim().Replace("'", "''");

                                name = worksheet.Cells[endRow, 6].Value.ToString().Replace("'", "''");


                                int.TryParse(worksheet.Cells[endRow, 7].Value.ToString(), out Weight);
                                int.TryParse(worksheet.Cells[endRow, 8].Value.ToString(), out Length);
                                int.TryParse(worksheet.Cells[endRow, 9].Value.ToString(), out Width);
                                int.TryParse(worksheet.Cells[endRow, 10].Value.ToString(), out Height);

                                int.TryParse(worksheet.Cells[endRow, 11].Value.ToString(), out StockQuantity);

                                int.TryParse(worksheet.Cells[endRow, 14].Value.ToString(), out DropShip);

                                Gtin = worksheet.Cells[endRow, 15].Value.ToString().Trim().Replace("'", "''");

                                if (worksheet.Cells[endRow, 16].Value.ToString() != "NULL")
                                    ExcludeGoogleFeed = worksheet.Cells[endRow, 16].Value.ToString() == "Y" ? 1 : 0;

                                color = worksheet.Cells[endRow, 17].Value.ToString().Replace("'", "''");

                                //Vendor = int.Parse(worksheet.Cells[endRow, 5].Value.ToString()); 
                                //var t = decimal.TryParse(worksheet.Cells[endRow, 12].Value.ToString(),out Price); 
                                //Manufacturers = int.Parse(worksheet.Cells[endRow, 20].Value.ToString()); 
                                //picture = worksheet.Cells[endRow, 21].Value.ToString();

                                ///  var exist = _productService.GetProductById(id);

                                sql += $" Update [dbo].[Product] set [Name]='{name}', SKU='{SKU}', Gtin='{Gtin}', StockQuantity={StockQuantity}, Price={Price}, [Weight]={Weight}, [Length]={Length}, Width={Width}, Height={Height}, ExcludeGoogleFeed={ExcludeGoogleFeed},color='{color}', VendorId={BrandID},[Drop] ={DropShip},Published=1 where id ={id} ";
                                sql += "IF @@ROWCOUNT=0 ";
                                sql += "INSERT INTO [dbo].[Product] ( " +
                                    " Id,[Name], SKU, Gtin, StockQuantity, CallForPrice, Price, OldPrice, ProductCost,    [Weight],   [Length], Width, Height, ExcludeGoogleFeed,color, VendorId,[Drop],CreatedOnUtc, UpdatedOnUtc, " +
                                    " ProductTypeId, ParentGroupedProductId, VisibleIndividually, ProductTemplateId, ShowOnHomePage, AllowCustomerReviews, ApprovedRatingSum, NotApprovedRatingSum, ApprovedTotalReviews, NotApprovedTotalReviews, SubjectToAcl, LimitedToStores, " +
                                    " IsGiftCard, GiftCardTypeId, RequireOtherProducts, AutomaticallyAddRequiredProducts, IsDownload, DownloadId, UnlimitedDownloads, MaxNumberOfDownloads, " +
                                    " DownloadActivationTypeId, HasSampleDownload, SampleDownloadId, HasUserAgreement, IsRecurring, RecurringCycleLength, RecurringCyclePeriodId, " +
                                    " RecurringTotalCycles, IsRental, RentalPriceLength, RentalPricePeriodId, IsShipEnabled, IsFreeShipping, ShipSeparately, AdditionalShippingCharge, " +
                                    " DeliveryDateId, IsTaxExempt, TaxCategoryId, IsTelecommunicationsOrBroadcastingOrElectronicServices, ManageInventoryMethodId, ProductAvailabilityRangeId, " +
                                    " UseMultipleWarehouses, WarehouseId, DisplayStockAvailability, DisplayStockQuantity, MinStockQuantity, LowStockActivityId, NotifyAdminForQuantityBelow, BackorderModeId, " +
                                    " AllowBackInStockSubscriptions, OrderMinimumQuantity, OrderMaximumQuantity, AllowAddingOnlyExistingAttributeCombinations, NotReturnable, DisableBuyButton, DisableWishlistButton, " +
                                    " AvailableForPreOrder, CustomerEntersPrice, MinimumCustomerEnteredPrice, MaximumCustomerEnteredPrice, BasepriceEnabled, BasepriceAmount, BasepriceUnitId, BasepriceBaseAmount, " +
                                    " MarkAsNew, HasTierPrices, HasDiscountsApplied, DisplayOrder, Published, Deleted, BasepriceBaseUnitId) " +
                                    $" select {id},'{name}','{SKU}','{Gtin}',{StockQuantity},0,{Price},0,0,{Weight},{Length},{Width},{Height},{ExcludeGoogleFeed},'{color}',{BrandID},{DropShip}, GETDATE(),GETDATE() , " +
                                    " 5,1,1,1,1,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, 0,0,0,0,0,0 ,0,0,1,0,1,0,0,0,0,0,1,1000,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0  ,0; ";






                                countCategorysInFile += 1;
                                endRow++;

                                maximunRows++;

                                if (maximunRows > 300)
                                {
                                    //  sql += "SET IDENTITY_INSERT [dbo].[Product] OFF;";
                                    _dbContext.ExecuteSqlCommand(sql);
                                    //  sql = "SET IDENTITY_INSERT [dbo].[Product] ON;";
                                    maximunRows = 0;
                                }


                                continue;
                            }
                            catch (Exception ex)
                            {
                                endRow++;
                                ErrorNotification("Admin.Common.UploadFile" + ex.Message);
                                continue;
                            }
                        }

                        sql += "SET IDENTITY_INSERT [dbo].[Product] OFF;";
                        try
                        {
                            _dbContext.ExecuteSqlCommand(sql);
                        }
                        catch (Exception ex)
                        {
                            ErrorNotification("Admin.Common.Product" + ex.Message);

                        }
                        SuccessNotification("Admin.Catalog.Product.Imported");


                        //update product category 
                        worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                        endRow = 2;
                        id = 0;
                        BrandID = 0;
                        Categories = 0;
                        sql = string.Empty;
                        while (true)
                        {
                            try
                            {
                                //if (worksheet.Row(endRow).OutlineLevel == 0)
                                //{
                                //    break;
                                //}
                                if (worksheet == null || worksheet.Cells == null)
                                    break;
                                if (worksheet.Cells[endRow, 1].Value == null)
                                    break;

                                int.TryParse(worksheet.Cells[endRow, 1].Value.ToString(), out id);


                                Active = worksheet.Cells[endRow, 2].Value.ToString() == "Y" ? 1 : 0;

                                int.TryParse(worksheet.Cells[endRow, 3].Value.ToString(), out Categories);

                                //int.TryParse(worksheet.Cells[endRow, 4].Value.ToString(), out BrandID);


                                if (Categories > 0)
                                {
                                    //var exist = _productService.GetProductById(id);
                                    //if (exist != null)
                                    //{
                                    //    var cate = exist.ProductCategories.Where(x => x.CategoryId != Categories).FirstOrDefault();
                                    //    if(cate==null)

                                    sql += $" UPDATE [dbo].[Product_Category_Mapping] set [ProductId]={id},[CategoryId]={Categories}, [IsFeaturedProduct]=0,[DisplayOrder]=0 where [ProductId]={id} and [CategoryId]={Categories}  ";
                                    sql += "IF @@ROWCOUNT=0 ";
                                    sql += " INSERT INTO [dbo].[Product_Category_Mapping] ([ProductId],[CategoryId],[IsFeaturedProduct],[DisplayOrder])" +
                                           $" select {id}, {Categories},0,0; ";
                                    //  }
                                }
                                if (maximunRows > 1000)
                                {
                                    if (sql != string.Empty)
                                        _dbContext.ExecuteSqlCommand(sql);
                                    sql = string.Empty;
                                    maximunRows = 0;
                                }
                                maximunRows++;
                                endRow++;

                            }
                            catch (Exception ex)
                            {
                                endRow++;
                                ErrorNotification("Admin.Common.UploadFile.Product_Category_Mapping" + $" select {id}, {Categories},0,0 " + ex.Message);
                                continue;
                            }
                        }
                        sql += "SET IDENTITY_INSERT [dbo].[Product_Category_Mapping] OFF;";
                        try
                        {
                            _dbContext.ExecuteSqlCommand(sql);
                        }
                        catch (Exception ex)
                        {
                            ErrorNotification("Admin.Common.Product_Category_Mapping" + ex.Message);

                        }
                        SuccessNotification("Admin.Catalog.Product_Category_Mapping.Imported");


                        //update product  vendor
                        worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                        endRow = 2;
                        id = 0;
                        BrandID = 0;
                        Categories = 0;
                        sql = "SET IDENTITY_INSERT [dbo].[Product_Manufacturer_Mapping] ON;";
                        while (true)
                        {
                            try
                            {
                                //if (worksheet.Row(endRow).OutlineLevel == 0)
                                //{
                                //    break;
                                //}
                                if (worksheet == null || worksheet.Cells == null)
                                    break;
                                if (worksheet.Cells[endRow, 1].Value == null)
                                    break;

                                int.TryParse(worksheet.Cells[endRow, 1].Value.ToString(), out id);


                                Active = worksheet.Cells[endRow, 2].Value.ToString() == "Y" ? 1 : 0;

                                // int.TryParse(worksheet.Cells[endRow, 3].Value.ToString(), out Categories);

                                int.TryParse(worksheet.Cells[endRow, 4].Value.ToString(), out BrandID);


                                if (BrandID > 0)
                                {
                                    //var exist = _productService.GetProductById(id);
                                    //if (exist != null)
                                    //{
                                    //    var manu = exist.ProductManufacturers.Where(x => x.ManufacturerId != BrandID).FirstOrDefault();
                                    //    if(manu==null)
                                    sql += $" UPDATE [dbo].[Product_Manufacturer_Mapping] set [ProductId]={id},[ManufacturerId]={BrandID}, [IsFeaturedProduct]=0,[DisplayOrder]=0 where [ProductId]={id} and [ManufacturerId]={BrandID}  ";
                                    sql += "IF @@ROWCOUNT=0 ";
                                    sql += "INSERT INTO [dbo].[Product_Manufacturer_Mapping] ([ProductId],[ManufacturerId],[IsFeaturedProduct],[DisplayOrder])" +
                                           $" select {id}, {BrandID},0,0; ";
                                    //}
                                }
                                if (maximunRows > 1000)
                                {
                                    if (sql != string.Empty)
                                        _dbContext.ExecuteSqlCommand(sql);
                                    sql = string.Empty;
                                    maximunRows = 0;
                                }
                                maximunRows++;
                                endRow++;

                            }
                            catch (Exception ex)
                            {
                                endRow++;
                                ErrorNotification("Admin.Common.UploadFile.Product_Manufacturer_Mapping" + $" select {id}, {BrandID},0,0 " + ex.Message);
                                continue;
                            }
                        }
                        sql += "SET IDENTITY_INSERT [dbo].[Product_Manufacturer_Mapping] OFF;";
                        try
                        {
                            _dbContext.ExecuteSqlCommand(sql);
                        }
                        catch (Exception ex)
                        {
                            ErrorNotification("Admin.Common.Product_Manufacturer_Mapping" + ex.Message);

                        }
                        SuccessNotification("Admin.Catalog.Product_Manufacturer_Mapping.Imported");



                        // 




                        endRow = 2;
                        worksheet = xlPackage.Workbook.Worksheets[4];
                        if (worksheet == null)
                            throw new NopException("No worksheet found");

                        //price update

                        sql = string.Empty;
                        id = 0;
                        endRow = 2;
                        maximunRows = 1;
                        while (true)
                        {
                            try
                            {
                                //if (worksheet.Row(endRow).OutlineLevel == 0)
                                //{
                                //    break;
                                //}
                                if (worksheet == null || worksheet.Cells == null)
                                    break;
                                if (worksheet.Cells[endRow, 1].Value == null)
                                    break;

                                int.TryParse(worksheet.Cells[endRow, 1].Value.ToString(), out id);

                                decimal.TryParse(worksheet.Cells[endRow, 3].Value.ToString(), out Price);


                                sql += $"update  [dbo].[Product] set price ={Price} where id ={id};";
                                maximunRows++;

                                endRow++;

                                if (maximunRows > 1000)
                                {

                                    _dbContext.ExecuteSqlCommand(sql);
                                    sql = string.Empty;
                                    maximunRows = 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                endRow++;
                                ErrorNotification("Admin.Common.UploadFile" + ex.Message);
                                continue;
                            }
                        }


                        try
                        {
                            _dbContext.ExecuteSqlCommand(sql);
                        }
                        catch (Exception ex)
                        {
                            ErrorNotification("Admin.Common.Product" + ex.Message);

                        }


                        // groups 

                        worksheet = xlPackage.Workbook.Worksheets[5];
                        if (worksheet == null)
                            throw new NopException("No worksheet found");
                        sql = "SET IDENTITY_INSERT [dbo].[Groups] ON;";
                        endRow = 2;
                        id = 0;
                        decimal interval = 0;
                        decimal percentage = 0;
                        name = string.Empty;
                        endRow = 2;
                        maximunRows = 1;
                        while (true)
                        {
                            try
                            {
                                //if (worksheet.Row(endRow).OutlineLevel == 0)
                                //{
                                //    break;
                                //}
                                if (worksheet == null || worksheet.Cells == null)
                                    break;
                                if (worksheet.Cells[endRow, 1].Value == null)
                                    break;

                                int.TryParse(worksheet.Cells[endRow, 1].Value.ToString(), out id);
                                name = worksheet.Cells[endRow, 2].Value.ToString();
                                decimal.TryParse(worksheet.Cells[endRow, 3].Value.ToString(), out interval);
                                decimal.TryParse(worksheet.Cells[endRow, 4].Value.ToString(), out percentage);


                                //var exist = _gpRepository.TableNoTracking.Where(x => x.Id == id).FirstOrDefault();
                                //if(exist==null)
                                sql += $" update  [dbo].[Groups] set GroupName ={Price} , Interval = {interval}, Percentage={percentage}, UpdatedOnUtc=getdate() where id ={id} ";
                                sql += "IF @@ROWCOUNT=0 ";
                                sql += $" insert into [dbo].[Groups] (Id, GroupName,Interval,    Percentage , CreatedOnUtc,            UpdatedOnUtc,            Deleted) " +
                                        $" select {id}, '{name}',{interval},{percentage}, getdate(), getdate(), 0; ";

                                //   else


                                maximunRows++;

                                endRow++;

                                if (maximunRows > 1000)
                                {

                                    //  sql += "SET IDENTITY_INSERT [dbo].[Groups] OFF;";
                                    _dbContext.ExecuteSqlCommand(sql);
                                    //   sql = "SET IDENTITY_INSERT [dbo].[Groups] ON;";
                                    maximunRows = 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                endRow++;
                                ErrorNotification("Admin.Common.Groups" + ex.Message);
                                continue;
                            }
                        }


                        sql += "SET IDENTITY_INSERT [dbo].[Groups] OFF;";
                        try
                        {
                            sql += "SET IDENTITY_INSERT [dbo].[Groups] Off;";
                            _dbContext.ExecuteSqlCommand(sql);
                        }
                        catch (Exception ex)
                        {
                            ErrorNotification("Admin.Common.Category" + ex.Message);

                        }


                        ///Groups items 
                        endRow = 2;
                        worksheet = xlPackage.Workbook.Worksheets[6];
                        if (worksheet == null)
                            throw new NopException("No worksheet found");
                        sql = string.Empty;
                        var groupID = 0;
                        var ItemID = 0;

                        maximunRows = 1;
                        sql = "SET IDENTITY_INSERT [dbo].[groups-items] ON;";
                        while (true)
                        {
                            try
                            {
                                //if (worksheet.Row(endRow).OutlineLevel == 0)
                                //{
                                //    break;
                                //}
                                if (worksheet == null || worksheet.Cells == null)
                                    break;
                                if (worksheet.Cells[endRow, 1].Value == null)
                                    break;

                                int.TryParse(worksheet.Cells[endRow, 1].Value.ToString(), out groupID);
                                int.TryParse(worksheet.Cells[endRow, 2].Value.ToString(), out ItemID);


                                //var exist = _gpiRepository.TableNoTracking.Where(x => x.GroupId == groupID && x.ItemId == ItemID).FirstOrDefault();
                                //if (exist == null)
                                //{

                                sql += $" update [dbo].[groups-items] set GroupId= {groupID}, ItemId={ItemID}, RelationShip=null, Deleted=0  where GroupId={groupID} and ItemId={ItemID} ";
                                sql += "IF @@ROWCOUNT=0 ";
                                sql += $"insert into [dbo].[groups-items] (GroupId, ItemId, RelationShip, Deleted) " +
                                    $" select {groupID},{ItemID},null,0; ";
                                maximunRows++;


                                //  }

                                endRow++;
                                if (maximunRows > 1000)
                                {


                                    _dbContext.ExecuteSqlCommand(sql);

                                    maximunRows = 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                ErrorNotification("Admin.Common.UploadFile" + ex.Message);
                                continue;
                            }
                        }


                        sql += "SET IDENTITY_INSERT [dbo].[groups-items] OFF;";
                        try
                        {
                            _dbContext.ExecuteSqlCommand(sql);
                        }
                        catch (Exception ex)
                        {
                            ErrorNotification("Admin.Common.groups-items" + ex.Message);

                        }


                        //item comppactibility
                        worksheet = xlPackage.Workbook.Worksheets[7];
                        if (worksheet == null)
                            throw new NopException("No worksheet found");
                        sql = "SET IDENTITY_INSERT [dbo].[ItemsCompatability] ON;";
                        // var ItemID = 0;
                        var ItemIDPart = 0;
                        endRow = 2;
                        maximunRows = 1;
                        while (true)
                        {
                            try
                            {
                                //if (worksheet.Row(endRow).OutlineLevel == 0)
                                //{
                                //    break;
                                //}
                                if (worksheet == null || worksheet.Cells == null)
                                    break;
                                if (worksheet.Cells[endRow, 1].Value == null)
                                    break;

                                int.TryParse(worksheet.Cells[endRow, 1].Value.ToString(), out ItemID);
                                int.TryParse(worksheet.Cells[endRow, 2].Value.ToString(), out ItemIDPart);


                                //var exist = _iRepository.TableNoTracking.Where(x => x.ItemId== ItemID && x.ItemIdPart == ItemIDPart).FirstOrDefault();
                                //if (exist == null)
                                sql += $" update [dbo].[ItemsCompatability] set ItemId={ItemID}, ItemIdPart={ItemIDPart},  UpdatedOnUtc=getdate(), Deleted=0 where ItemId={ItemID} and ItemIdPart={ItemIDPart} ";
                                sql += "IF @@ROWCOUNT=0 ";
                                sql += $" insert into [dbo].[ItemsCompatability] (ItemId, ItemIdPart, CreatedOnUtc, UpdatedOnUtc, Deleted) " +
                                        $" select {ItemID},{ItemIDPart},getdate(),getdate(),0; ";


                                maximunRows++;

                                endRow++;

                                if (maximunRows > 1000)
                                {
                                    if (sql != string.Empty)
                                        _dbContext.ExecuteSqlCommand(sql);
                                    maximunRows = 0;
                                    sql = string.Empty;
                                }
                            }
                            catch (Exception ex)
                            {
                                endRow++;
                                ErrorNotification("Admin.Common.UploadFile" + ex.Message);
                                continue;
                            }
                        }


                        sql += "SET IDENTITY_INSERT [dbo].[ItemsCompatability] OFF;";
                        try
                        {
                            _dbContext.ExecuteSqlCommand(sql);
                        }
                        catch (Exception ex)
                        {
                            ErrorNotification("Admin.Common.ItemsCompatability" + ex.Message);

                        }

                        //relation group items
                        worksheet = xlPackage.Workbook.Worksheets[8];
                        if (worksheet == null)
                            throw new NopException("No worksheet found");
                        sql = "SET IDENTITY_INSERT [dbo].[Relations-Groups-Items] ON;";
                        // var ItemID = 0;
                        endRow = 2;
                        var GroupId = 0;
                        var dire = string.Empty;
                        maximunRows = 1;
                        while (true)
                        {
                            try
                            {
                                //if (worksheet.Row(endRow).OutlineLevel == 0)
                                //{
                                //    break;
                                //}
                                if (worksheet == null || worksheet.Cells == null)
                                    break;
                                if (worksheet.Cells[endRow, 1].Value == null)
                                    break;

                                int.TryParse(worksheet.Cells[endRow, 1].Value.ToString(), out GroupId);
                                int.TryParse(worksheet.Cells[endRow, 2].Value.ToString(), out ItemID);
                                dire = worksheet.Cells[endRow, 3].Value.ToString();


                                sql += $"update [dbo].[Relations-Groups-Items] set Direction='{dire}' where  GroupId ={GroupId} and  ItemId ={ItemID} ";
                                sql += "IF @@ROWCOUNT=0 ";
                                sql += $"insert into [dbo].[Relations-Groups-Items] ( GroupId,  ItemId ,   Direction ,Deleted) " +
                                        $" select {GroupId},{ItemID},'{dire}',0; ";

                                maximunRows++;

                                endRow++;



                                if (maximunRows > 1000)
                                {
                                    _dbContext.ExecuteSqlCommand(sql);
                                    maximunRows = 0;
                                    sql = string.Empty;
                                }
                            }
                            catch (Exception ex)
                            {
                                endRow++;
                                ErrorNotification("Admin.Common.UploadFile" + ex.Message);
                                continue;
                            }
                        }


                        sql += "SET IDENTITY_INSERT [dbo].[Relations-Groups-Items] OFF;";
                        try
                        {
                            _dbContext.ExecuteSqlCommand(sql);
                        }
                        catch (Exception ex)
                        {
                            ErrorNotification("Admin.Common.Relations-Groups-Items " + ex.Message);

                        }

                        //legacy

                        //relation group items
                        worksheet = xlPackage.Workbook.Worksheets[9];
                        if (worksheet == null)
                            throw new NopException("No worksheet found");
                        sql = "SET IDENTITY_INSERT [dbo].[LegacyIds] ON;";
                        var legacyId = string.Empty;
                        endRow = 2;
                        maximunRows = 1;
                        while (true)
                        {
                            try
                            {
                                //if (worksheet.Row(endRow).OutlineLevel == 0)
                                //{
                                //    break;
                                //}
                                if (worksheet == null || worksheet.Cells == null)
                                    break;
                                if (worksheet.Cells[endRow, 1].Value == null)
                                    break;

                                id = int.Parse(worksheet.Cells[endRow, 1].Value.ToString());
                                legacyId = worksheet.Cells[endRow, 2].Value.ToString();


                                //var exist = _lRepository.TableNoTracking.Where(x => x.ItemId == id && x.LegacyCode == legacyId).FirstOrDefault();
                                //if (exist == null)

                                sql += $" update [dbo].[LegacyIds] set [ItemId] = {id},[LegacyCode]='{legacyId}',[Deleted]=0  where [ItemId] ={id} and [LegacyCode]='{legacyId}'  ";
                                sql += "IF @@ROWCOUNT=0 ";
                                sql += $" Insert into  [dbo].[LegacyIds] ([ItemId],[LegacyCode],[Deleted]) select {id},'{legacyId}',0; ";



                                maximunRows++;

                                endRow++;



                                if (maximunRows > 1000)
                                {
                                    if (sql != string.Empty)
                                        _dbContext.ExecuteSqlCommand(sql);
                                    maximunRows = 0;
                                    sql = string.Empty;
                                }
                            }
                            catch (Exception ex)
                            {
                                endRow++;
                                ErrorNotification("Admin.Common.UploadFile" + ex.Message);
                                continue;
                            }
                        }


                        sql += "SET IDENTITY_INSERT [dbo].[LegacyIds] OFF;";
                        try
                        {
                            _dbContext.ExecuteSqlCommand(sql);
                        }
                        catch (Exception ex)
                        {
                            ErrorNotification("Admin.Common.LegacyIds" + ex.Message);

                        }
                    }



                }
                else
                {
                    ErrorNotification("Admin.Common.UploadFile");
                    return RedirectToAction("List");
                }
                SuccessNotification("Admin.Catalog.Categories.Imported");
                return RedirectToAction("Index");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public virtual IActionResult ImportOrdersFromXlsx(IFormFile importexcelfile)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    Stream stream = importexcelfile.OpenReadStream();
                    using (var xlPackage = new ExcelPackage(stream))
                    {
                        var endRow = 2;

                        // get the 1st worksheet in the workbook
                        var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                            throw new NopException("No worksheet found");

                        // get the second worksheet in the workbook
                        var worksheetO = xlPackage.Workbook.Worksheets[2];
                        if (worksheetO == null)
                            throw new NopException("No worksheet found");


                        var orderid = 0;
                        var username = string.Empty;
                        var orderstatus = string.Empty;
                        var transactionid = string.Empty;
                        var useremail = string.Empty;
                        var userrole = string.Empty;
                        var fname = string.Empty;
                        var lname = string.Empty;
                        var billaddr = string.Empty;
                        var billcity = string.Empty;
                        var billstate = string.Empty;
                        var billzip = string.Empty;
                        var billcountry = string.Empty;
                        var shipaddr = string.Empty;
                        var shipcity = string.Empty;
                        var shipstate = string.Empty;
                        var shipzip = string.Empty;
                        var shipcountry = string.Empty;
                        var userphone = string.Empty;
                        var userphone2 = string.Empty;
                        var paymethod = string.Empty;
                        var cctype = string.Empty;
                        var ccnumber = string.Empty;
                        var ccexpiration = string.Empty;
                        var ccsecurity = string.Empty;
                        var shipcarrier = string.Empty;
                        var shiptype = string.Empty;
                        var shipamount = string.Empty;
                        var itemcount = string.Empty;
                        var subtotal = string.Empty;
                        var tax = string.Empty;
                        var total = string.Empty;
                        var trackingnumber = string.Empty;
                        var year = string.Empty;
                        var month = string.Empty;
                        var date = string.Empty;
                        var hour = string.Empty;
                        var min = string.Empty;
                        var info = string.Empty;

                        //find end of data
                        while (true)
                        {
                            try
                            {
                                if (worksheet == null || worksheet.Cells == null)
                                    break;
                                if(endRow>24070)
                                if (worksheet.Cells[endRow, 1].Value == null)
                                    break;

                                username = worksheet.Cells[endRow, 2].Value as string;
                                useremail = worksheet.Cells[endRow, 5].Value as string;
                                fname = worksheet.Cells[endRow, 7].Value as string;
                                lname = worksheet.Cells[endRow, 8].Value as string;


                                var customer = _customerService.GetCustomerByEmail(useremail);
                                if (customer == null)
                                    customer = _customerService.GetCustomerByUsername(username);
                                if (customer == null)
                                {
                                    if (fname == null && lname == null)
                                    {
                                        endRow++;
                                        continue;
                                    }
                                    useremail = $"guest{fname.Trim()}@{lname.Trim()}.com";
                                    customer = _customerService.GetCustomerByEmail(useremail);
                                }

                                orderid = int.Parse(worksheet.Cells[endRow, 1].Value.ToString());

                                orderstatus = worksheet.Cells[endRow, 3].Value.ToString();
                                transactionid = worksheet.Cells[endRow, 4].Value.ToString();

                                userrole = worksheet.Cells[endRow, 6].Value.ToString();

                                billaddr = worksheet.Cells[endRow, 9].Value as string;
                                billcity = worksheet.Cells[endRow, 10].Value as string;
                                billstate = worksheet.Cells[endRow, 11].Value as string;
                                billzip = worksheet.Cells[endRow, 12].Value as string;
                                billcountry = worksheet.Cells[endRow, 13].Value as string;
                                shipaddr = worksheet.Cells[endRow, 14].Value as string;
                                shipcity = worksheet.Cells[endRow, 15].Value as string;
                                shipstate = worksheet.Cells[endRow, 16].Value as string;
                                shipzip = worksheet.Cells[endRow, 17].Value as string;
                                shipcountry = worksheet.Cells[endRow, 18].Value as string;
                                userphone = worksheet.Cells[endRow, 19].Value  as string;
                                userphone2 = worksheet.Cells[endRow, 20].Value as string;
                                paymethod = worksheet.Cells[endRow, 21].Value as string;
                                cctype = worksheet.Cells[endRow, 22].Value as string;
                                ccnumber = worksheet.Cells[endRow, 23].Value as string;
                                ccexpiration = worksheet.Cells[endRow, 24].Value as string;
                                ccsecurity = worksheet.Cells[endRow, 25].Value as string;
                                shipcarrier = worksheet.Cells[endRow, 26].Value as string;
                                shiptype = worksheet.Cells[endRow, 27].Value as string;
                                shipamount = worksheet.Cells[endRow, 28].Value.ToString();
                                itemcount = worksheet.Cells[endRow, 29].Value.ToString();
                                subtotal = worksheet.Cells[endRow, 30].Value.ToString();
                                tax = worksheet.Cells[endRow, 31].Value.ToString();
                                total = worksheet.Cells[endRow, 32].Value.ToString();
                                trackingnumber = worksheet.Cells[endRow, 33].Value as string;
                                year = worksheet.Cells[endRow, 34].Value.ToString();
                                month = worksheet.Cells[endRow, 35].Value.ToString();
                                date = worksheet.Cells[endRow, 36].Value.ToString();
                                hour = worksheet.Cells[endRow, 37].Value.ToString();
                                min = worksheet.Cells[endRow, 38].Value.ToString();
                                info = worksheet.Cells[endRow, 39].Value as string;

                                var dateTimeAsUtc = DateTime.UtcNow;

                                try
                                {
                                    var dateToConvert = $"{year}-{month}-{date} {hour}:{min}:00"; 

                                    //var dateTimeWithUnspecifiedKind = DateTime.ParseExact(dateToConvert, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                    dateTimeAsUtc = DateTime.SpecifyKind(DateTime.Parse(dateToConvert), DateTimeKind.Utc);
                                }
                                catch(Exception ex) {
                                    var tt = ex.Message;
                                }

                                if (customer == null)
                                {
                                    var newCustomer = new Customer
                                    {

                                        VendorId = 0,
                                        CustomerGuid = Guid.NewGuid(),
                                        Active = true,
                                        AffiliateId = 0,
                                        CreatedOnUtc = dateTimeAsUtc,
                                        Email = useremail,
                                        Username = username,
                                        IsSystemAccount = false,
                                        Deleted = false,
                                        LastActivityDateUtc = dateTimeAsUtc,

                                    };
                                    _customerService.InsertCustomer(newCustomer);

                                    try
                                    {
                                        _genericAttributeService.SaveAttribute(newCustomer, SystemCustomerAttributeNames.FirstName, fname);
                                    }
                                    catch { }
                                    try
                                    {
                                        _genericAttributeService.SaveAttribute(newCustomer, SystemCustomerAttributeNames.LastName, lname);
                                    }
                                    catch { }

                                    var billingAddress = new Address
                                    {
                                        FirstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                                        LastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                                        Email = customer.Email,
                                        Company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company),
                                        CountryId = customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId) > 0
                                                                    ? (int?)customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId)
                                                                    : null,
                                        StateProvinceId = customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId) > 0
                                                                    ? (int?)customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId)
                                                                    : null,
                                        City = billcity,
                                        Address1 = billaddr,
                                        ZipPostalCode = billzip,
                                        PhoneNumber = userphone,
                                        FaxNumber = userphone2,
                                        CreatedOnUtc = customer.CreatedOnUtc
                                    };

                                    var billinState = _countryService.GetCountryByThreeLetterIsoCode("US").StateProvinces.Where(x => x.Abbreviation == billstate).FirstOrDefault();


                                    if (this._addressService.IsAddressValid(billingAddress))
                                    {
                                        //some validation                                        
                                        billingAddress.CountryId = billinState.CountryId;
                                        billingAddress.StateProvinceId = billinState.Id;
                                        //set default address
                                        customer.Addresses.Add(billingAddress);
                                        customer.BillingAddress = billingAddress;
                                        _customerService.UpdateCustomer(customer);
                                    }



                                    var shippingAddress = new Address
                                    {
                                        FirstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                                        LastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                                        Email = customer.Email,
                                        Company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company),
                                        CountryId = customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId) > 0
                                                                    ? (int?)customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId)
                                                                    : null,
                                        StateProvinceId = customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId) > 0
                                                                    ? (int?)customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId)
                                                                    : null,
                                        City = billcity,
                                        Address1 = billaddr,
                                        ZipPostalCode = billzip,
                                        PhoneNumber = userphone,
                                        FaxNumber = userphone2,
                                        CreatedOnUtc = customer.CreatedOnUtc
                                    };

                                    var shippingState = _countryService.GetCountryByThreeLetterIsoCode("US").StateProvinces.Where(x => x.Abbreviation == shipstate).FirstOrDefault();


                                    if (this._addressService.IsAddressValid(shippingAddress))
                                    {
                                        //some validation                                        
                                        shippingAddress.CountryId = shippingState.CountryId;
                                        shippingAddress.StateProvinceId = shippingState.Id;
                                        //set default address
                                        customer.Addresses.Add(shippingAddress);
                                        customer.ShippingAddress = shippingAddress;
                                        _customerService.UpdateCustomer(customer);
                                    }

                                }

                                var order = _orderService.GetOrderByAuthorizationTransactionIdAndPaymentMethod(transactionid ,null);
                                if (order == null)
                                {
                                    order = new Order
                                    {
                                        Id = 1,
                                        OrderGuid = Guid.NewGuid(),
                                        CustomerId = customer.Id,
                                        Customer = customer,
                                        StoreId = 1,
                                        OrderStatus = OrderStatus.Complete,
                                        ShippingStatus = ShippingStatus.Shipped,
                                        PaymentStatus = PaymentStatus.Paid,
                                     //   PaymentMethodSystemName = "PaymentMethodSystemName1",
                                     //   CustomerCurrencyCode = "US",
                                    //    CurrencyRate = 1.1M,
                                      //  CustomerTaxDisplayType = TaxDisplayType.ExcludingTax,
                                        //VatNumber = "123456789",
                                        OrderSubtotalInclTax = 2.1M,
                                        OrderSubtotalExclTax = 3.1M,
                                        OrderSubTotalDiscountInclTax = 4.1M,
                                        OrderSubTotalDiscountExclTax = 5.1M,
                                        OrderShippingInclTax = 6.1M,
                                        OrderShippingExclTax = 7.1M,
                                        PaymentMethodAdditionalFeeInclTax = 8.1M,
                                        PaymentMethodAdditionalFeeExclTax = 9.1M,
                                       // TaxRates = "1,3,5,7",
                                        OrderTax = decimal.Parse(tax),
                                        OrderDiscount = 0M,
                                        OrderTotal = decimal.Parse(total),
                                        RefundedAmount = 0M,
                                    //    CheckoutAttributeDescription = "CheckoutAttributeDescription1",
                                      //  CheckoutAttributesXml = "CheckoutAttributesXml1",
                                     //   CustomerLanguageId = 1,
                                        AffiliateId = 0,
                                   //     CustomerIp = "CustomerIp1",
                                   //     AllowStoringCreditCardNumber = false,
                                        //CardType = "Visa",
                                        //CardName = "John Smith",
                                        //CardNumber = "4111111111111111",
                                        //MaskedCreditCardNumber = "************1111",
                                        //CardCvv2 = "123",
                                        //CardExpirationMonth = "12",
                                        //CardExpirationYear = "2010",
                                        AuthorizationTransactionId = transactionid,
                                        AuthorizationTransactionCode = transactionid,
                                        AuthorizationTransactionResult = transactionid,
                                        CaptureTransactionId = transactionid,
                                        CaptureTransactionResult = transactionid,
                                        SubscriptionTransactionId = transactionid,
                                        PaidDateUtc = dateTimeAsUtc,
                                       // CustomValuesXml = "<test>test</test>",
                                        BillingAddress = customer.BillingAddress,
                                        ShippingAddress = customer.ShippingAddress,
                                        ShippingMethod = shipcarrier,
                                      //  ShippingRateComputationMethodSystemName = "ShippingRateComputationMethodSystemName1",
                                        Deleted = false,
                                        CreatedOnUtc = dateTimeAsUtc,
                                        CustomOrderNumber = string.Empty
                                    };
                                    _orderService.InsertOrder(order);
                                    //generate and set custom order number
                                    order.CustomOrderNumber = _customNumberFormatter.GenerateOrderCustomNumber(order);
                                    _customerService.UpdateCustomer(customer); 

                                }

                                var endRowO = 2;

                               

                                var firstTime = true;
                                //find end of data

                                while (true)
                                {
                                    try
                                    {
                                        if (worksheetO == null || worksheet.Cells == null)
                                            break;
                                        if (endRowO > 41681)
                                            if (worksheetO.Cells[endRowO, 1].Value == null)
                                                break;

                                        int orderIdO = 0;
                                        var orderString = worksheetO.Cells[endRowO, 2].Value.ToString();
                                          int.TryParse(orderString, out  orderIdO);
                                        if (orderIdO != orderid)
                                        {
                                            if (!firstTime)
                                                break;
                                            firstTime = true;
                                            endRowO++;
                                            continue;
                                        }
                                        

                                       firstTime = false;

                                        int itemId = int.Parse(worksheetO.Cells[endRowO, 3].Value.ToString());
                                        var ProdName = worksheetO.Cells[endRowO, 4].Value as String;
                                        var sku = worksheetO.Cells[endRowO, 5].Value.ToString();
                                        int qty = int.Parse(worksheetO.Cells[endRowO, 6].Value.ToString());
                                        decimal prodPrice = decimal.Parse(worksheetO.Cells[endRowO, 7].Value.ToString());


                                        var itemExist = order.OrderItems.Where(x => x.ProductId == itemId).FirstOrDefault();

                                        if (itemExist == null)
                                        {
                                            var product = _productService.GetProductById(itemId);

                                            if (product == null)
                                            {
                                                product = new Product
                                                {
                                                    Name = ProdName,
                                                    Sku = sku,
                                                    Price = prodPrice,
                                                    UpdatedOnUtc = dateTimeAsUtc,
                                                    CreatedOnUtc = dateTimeAsUtc,
                                                    Published = false,
                                                    ProductType = ProductType.GroupedProduct,
                                                    ParentGroupedProductId = 2,
                                                    VisibleIndividually = true,
                                                  
                                                    ShortDescription = ProdName,
                                                    FullDescription = ProdName,
                                                   // AdminComment = "AdminComment 1",
                                                    VendorId = 0,
                                                    ProductTemplateId =1,
                                                    ShowOnHomePage = false,
                                                    //MetaKeywords = "Meta keywords",
                                                    //MetaDescription = "Meta description",
                                                    //MetaTitle = "Meta title",
                                                  //  AllowCustomerReviews = false,
                                                    //ApprovedRatingSum = 2,
                                                   // NotApprovedRatingSum = 3,
                                                   // ApprovedTotalReviews = 4,
                                                  //  NotApprovedTotalReviews = 5,
                                                   // SubjectToAcl = true,
                                                  //  LimitedToStores = true, 
                                                 //   ManufacturerPartNumber = "manufacturerPartNumber",
                                                 //   Gtin = "GTIN 1",
                                                 //   IsGiftCard = false,
                                                 //   GiftCardTypeId = 1,
                                                 //   OverriddenGiftCardAmount = 1,
                                                 //   IsDownload = false,
                                                 //   DownloadId = 2,
                                                 //   UnlimitedDownloads = false,
                                                  //  MaxNumberOfDownloads = 3,
                                                  //  DownloadExpirationDays = 4,
                                                 //   DownloadActivationTypeId = 5,
                                                 //   HasSampleDownload = true,
                                                //    SampleDownloadId = 6,
                                                //    HasUserAgreement = false,
                                                 //   UserAgreementText = "userAgreementText",
                                                 //   IsRecurring = true,
                                                  //  RecurringCycleLength = 7,
                                                 //   RecurringCyclePeriodId = 8,
                                                 //   RecurringTotalCycles = 9,
                                                 //   IsRental = true,
                                                 //   RentalPriceLength = 9,
                                                //    RentalPricePeriodId = 10,
                                                //    IsShipEnabled = true,
                                                //    IsFreeShipping = true,
                                                 //   ShipSeparately = true,
                                                  //  AdditionalShippingCharge = 10.1M,
                                                //    DeliveryDateId = 5,
                                                    IsTaxExempt = true,
                                               //     TaxCategoryId = 11,
                                                //    IsTelecommunicationsOrBroadcastingOrElectronicServices = true,
                                              //      ManageInventoryMethodId = 12,
                                               //     ProductAvailabilityRangeId = 1,
                                                    UseMultipleWarehouses = true,
                                                //    WarehouseId = 6,
                                                    StockQuantity = 13,
                                                    DisplayStockAvailability = true,
                                                    DisplayStockQuantity = true,
                                                    MinStockQuantity = 14,
                                                    LowStockActivityId = 15,
                                                    NotifyAdminForQuantityBelow = 16,
                                                //    BackorderModeId = 17,
                                                    AllowBackInStockSubscriptions = true,
                                                    OrderMinimumQuantity = 18,
                                                    OrderMaximumQuantity = 19,
                                                    AllowedQuantities = "1, 5,6,10",
                                                    AllowAddingOnlyExistingAttributeCombinations = true,
                                                    NotReturnable = true,
                                                    DisableBuyButton = true,
                                                    DisableWishlistButton = true,
                                                    AvailableForPreOrder = true,
                                                    PreOrderAvailabilityStartDateTimeUtc = new DateTime(2010, 01, 01),
                                             //       CallForPrice = true,
                                                  
                                                    OldPrice = 22.1M,
                                                    ProductCost = 23.1M,
                                            //        CustomerEntersPrice = true,
                                                    MinimumCustomerEnteredPrice = 24.1M,
                                                    MaximumCustomerEnteredPrice = 25.1M,
                                                    BasepriceEnabled = true,
                                                    BasepriceAmount = 33.1M,
                                                    BasepriceUnitId = 4,
                                                    BasepriceBaseAmount = 34.1M,
                                                    BasepriceBaseUnitId = 5,
                                                    MarkAsNew = true,
                                                    MarkAsNewStartDateTimeUtc = new DateTime(2010, 01, 07),
                                                    MarkAsNewEndDateTimeUtc = new DateTime(2010, 01, 08),
                                                    HasTierPrices = true,
                                                    HasDiscountsApplied = true,
                                                    Weight = 26.1M,
                                                    Length = 27.1M,
                                                    Width = 28.1M,
                                                    Height = 29.1M,
                                                    AvailableStartDateTimeUtc = new DateTime(2010, 01, 01),
                                                    AvailableEndDateTimeUtc = new DateTime(2010, 01, 02),
                                                    RequireOtherProducts = true,
                                                  //  RequiredProductIds = "1,2,3",
                                                    AutomaticallyAddRequiredProducts = true,
                                                    DisplayOrder = 30,
                                                  
                                                  

                                                };
                                                _productService.InsertProduct(product);

                                            }
                                            var newItem = new OrderItem
                                            {
                                                Order = order,
                                                PriceExclTax = prodPrice,
                                                OrderItemGuid = Guid.NewGuid(),
                                                Product = product,
                                                Quantity = qty,
                                                DownloadCount = 0,
                                                IsDownloadActivated = false,
                                                LicenseDownloadId = 0,
                                            };

                                            order.OrderItems.Add(newItem);
                                            _orderService.UpdateOrder(order);
                                        }

                                    }
                                    catch(Exception ex)
                                    {

                                        ErrorNotification($"orderid: {orderid} error {ex}");

                                    }

                                    endRowO++;
                                }



                            }
                            catch (Exception ex)
                            {

                                ErrorNotification($"orderid: {orderid} error {ex}");
                            }
                            endRow++;
                        }
                    }
                }
            }
            catch 
            { }
            SuccessNotification("Admin.Catalog.Categories.Imported");
            return RedirectToAction("Index");
        }

        //[HttpPost]
        //public virtual IActionResult ImportCategoryFromXlsx(IFormFile importexcelfile)
        //{


        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
        //        return AccessDeniedView();

        //    //a vendor cannot import categories
        //    if (_workContext.CurrentVendor != null)
        //        return AccessDeniedView();

        //    try
        //    {
        //        if (importexcelfile != null && importexcelfile.Length > 0)
        //        {
        //            Stream stream = importexcelfile.OpenReadStream();
        //            using (var xlPackage = new ExcelPackage(stream))
        //            {
        //                var endRow = 2;
        //                var countCategorysInFile = 0;
        //                // get the second worksheet in the workbook
        //                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
        //                if (worksheet == null)
        //                    throw new NopException("No worksheet found");

        //                var sql = "SET IDENTITY_INSERT [dbo].[Category] ON;";
        //                var id = 0;
        //                var name = string.Empty;
        //                //find end of data
        //                while (true)
        //                {
        //                    try
        //                    {
        //                        //if (worksheet.Row(endRow).OutlineLevel == 0)
        //                        //{
        //                        //    break;
        //                        //}
        //                        if (worksheet == null || worksheet.Cells == null)
        //                            break;
        //                        if (worksheet.Cells[endRow, 1].Value == null)
        //                            break;

        //                        id = int.Parse(worksheet.Cells[endRow, 1].Value.ToString());
        //                        name = worksheet.Cells[endRow, 2].Value.ToString();

        //                        var exist = _categoryService.GetCategoryById(id);

        //                        if (exist == null)
        //                            sql += "INSERT INTO [dbo].[Category] (Id,[Name], UpdatedOnUtc,CreatedOnUtc, CategoryTemplateId, ParentCategoryId, " +
        //                               "PictureId,  AllowCustomersToSelectPageSize,ShowOnHomePage,IncludeInTopMenu,SubjectToAcl,LimitedToStores,Deleted,DisplayOrder,Published, PageSize) " +
        //                               $" SELECT {id},'{name}',getdate(),getdate(), 1,0,0,1,0,0,0,0,0,0,0,1,5; ";

        //                        else
        //                            sql += $"update [dbo].[Category] set    [Name] ='{name}'  where id =  {id}; ";

        //                        countCategorysInFile += 1;

        //                        endRow++;
        //                        continue;
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        endRow++;
        //                        ErrorNotification("Admin.Common.UploadFile " + ex.Message);
        //                        continue;
        //                    }
        //                }

        //                sql += "SET IDENTITY_INSERT [dbo].[Category] OFF;";
        //                try
        //                {
        //                    _dbContext.ExecuteSqlCommand(sql);
        //                }
        //                catch (Exception ex)
        //                {
        //                    ErrorNotification("Admin.Common.Category  " + ex.Message);

        //                }

        //                SuccessNotification("Admin.Catalog.Categories.Imported");
        //            }



        //        }
        //        else
        //        {
        //            ErrorNotification("Admin.Common.UploadFile");
        //            return RedirectToAction("List");
        //        }
        //        SuccessNotification("Admin.Catalog.Categories.Imported");
        //        return RedirectToAction("Index");
        //    }
        //    catch (Exception exc)
        //    {
        //        ErrorNotification(exc);
        //        return RedirectToAction("Index");
        //    }
        //}


        //[HttpPost]
        //public virtual IActionResult ImportManufacturerFromXlsx(IFormFile importexcelfile)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
        //        return AccessDeniedView();

        //    //a vendor cannot import categories
        //    if (_workContext.CurrentVendor != null)
        //        return AccessDeniedView();

        //    var tmpSql = "delete [dbo].[Manufacturer] where [Name]='Y'; delete [dbo].[Manufacturer] where [Name]='N';";
        //    _dbContext.ExecuteSqlCommand(tmpSql);


        //    try
        //    {
        //        if (importexcelfile != null && importexcelfile.Length > 0)
        //        {
        //            Stream stream = importexcelfile.OpenReadStream();
        //            using (var xlPackage = new ExcelPackage(stream))
        //            {
        //                var endRow = 2;
        //                var countCategorysInFile = 0;
        //                // get the second worksheet in the workbook
        //                var worksheet = xlPackage.Workbook.Worksheets[3];
        //                if (worksheet == null)
        //                    throw new NopException("No worksheet found");

        //                var sql = "SET IDENTITY_INSERT [dbo].[Manufacturer] ON;";
        //                var id = 0;
        //                var name = string.Empty;
        //                //find end of data
        //                while (true)
        //                {
        //                    try
        //                    {
        //                        //if (worksheet.Row(endRow).OutlineLevel == 0)
        //                        //{
        //                        //    break;
        //                        //}
        //                        if (worksheet == null || worksheet.Cells == null)
        //                            break;
        //                        if (worksheet.Cells[endRow, 1].Value == null)
        //                            break;

        //                        id = int.Parse(worksheet.Cells[endRow, 1].Value.ToString());
        //                        name = worksheet.Cells[endRow, 2].Value.ToString();

        //                        var exist = _manufacturerService.GetManufacturerById(id);
        //                        if (exist == null)

        //                            sql += "INSERT INTO [dbo].[Manufacturer] (Id,[Name], UpdatedOnUtc,CreatedOnUtc, " +
        //                                   " PictureId, PageSize, AllowCustomersToSelectPageSize,SubjectToAcl,LimitedToStores,Deleted,DisplayOrder,Published,ManufacturerTemplateId) " +
        //                                   $" SELECT {id},'{name}',getdate(),getdate(), 0,1,0,0,0,0,0,1,0; ";

        //                        else
        //                            sql += $" update [dbo].[Manufacturer] set [Name] ='{name}' where id ={id};";

        //                        countCategorysInFile += 1;

        //                        endRow++;
        //                        continue;
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        endRow++;
        //                        ErrorNotification("Admin.Common.UploadFile" + ex.Message);
        //                        continue;
        //                    }
        //                }

        //                sql += "SET IDENTITY_INSERT [dbo].[Manufacturer] OFF;";
        //                _dbContext.ExecuteSqlCommand(sql);
        //                SuccessNotification("Admin.Catalog.Manufacturer.Imported");
        //            }



        //        }
        //        else
        //        {
        //            ErrorNotification("Admin.Common.UploadFile");
        //            return RedirectToAction("List");
        //        }
        //        SuccessNotification("Admin.Catalog.Categories.Imported");
        //        return RedirectToAction("Index");
        //    }
        //    catch (Exception exc)
        //    {
        //        ErrorNotification(exc);
        //        return RedirectToAction("Index");
        //    }
        //}

        //[HttpPost]
        //public virtual IActionResult ImportLegacyFromXlsx(IFormFile importexcelfile)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
        //        return AccessDeniedView();

        //    //a vendor cannot import categories
        //    if (_workContext.CurrentVendor != null)
        //        return AccessDeniedView();
        //    var maximunRows = 0;
        //    try
        //    {
        //        if (importexcelfile != null && importexcelfile.Length > 0)
        //        {
        //            Stream stream = importexcelfile.OpenReadStream();
        //            using (var xlPackage = new ExcelPackage(stream))
        //            {
        //                var endRow = 2;
        //                var countCategorysInFile = 0;
        //                // get the second worksheet in the workbook
        //                var worksheet = xlPackage.Workbook.Worksheets[5];
        //                if (worksheet == null)
        //                    throw new NopException("No worksheet found");

        //                var sql = ""; // "SET IDENTITY_INSERT [dbo].[LegacyIds] ON;";
        //                var id = 0;
        //                var name = string.Empty;
        //                var legacyId = string.Empty;
        //                //find end of data
        //                while (true)
        //                {
        //                    try
        //                    {
        //                        //if (worksheet.Row(endRow).OutlineLevel == 0)
        //                        //{
        //                        //    break;
        //                        //}
        //                        if (worksheet == null || worksheet.Cells == null)
        //                            break;
        //                        if (worksheet.Cells[endRow, 1].Value == null)
        //                            break;

        //                        id = int.Parse(worksheet.Cells[endRow, 1].Value.ToString());
        //                        legacyId = worksheet.Cells[endRow, 2].Value.ToString();

        //                        sql += $" Insert into  [dbo].[LegacyIds] ([ItemId],[LegacyCode],[Deleted]) select {id},'{legacyId}',0; ";


        //                        countCategorysInFile += 1;

        //                        endRow++;
        //                        maximunRows++;
        //                        if (maximunRows > 1)
        //                        {

        //                            _dbContext.ExecuteSqlCommand(sql);
        //                            sql = string.Empty;
        //                            maximunRows = 0;
        //                        }

        //                        continue;
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        endRow++;
        //                        ErrorNotification("Admin.Common.UploadFile" + ex.Message);
        //                        continue;
        //                    }
        //                }

        //                //sql += "SET IDENTITY_INSERT [dbo].[LegacyIds] OFF;";
        //                _dbContext.ExecuteSqlCommand(sql);
        //                SuccessNotification("Admin.Catalog.LegacyIds.Imported");
        //            }



        //        }
        //        else
        //        {
        //            ErrorNotification("Admin.Common.UploadFile");
        //            return RedirectToAction("List");
        //        }
        //        SuccessNotification("Admin.Catalog.LegacyIds.Imported");
        //        return RedirectToAction("Index");
        //    }
        //    catch (Exception exc)
        //    {
        //        ErrorNotification(exc);
        //        return RedirectToAction("Index");
        //    }
        //}



        //[HttpPost]
        //public virtual IActionResult ImportItemsCompatabilityFromXlsx(IFormFile importexcelfile)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
        //        return AccessDeniedView();

        //    //a vendor cannot import categories
        //    if (_workContext.CurrentVendor != null)
        //        return AccessDeniedView();
        //    var maximunRows = 0;
        //    try
        //    {
        //        if (importexcelfile != null && importexcelfile.Length > 0)
        //        {
        //            Stream stream = importexcelfile.OpenReadStream();
        //            using (var xlPackage = new ExcelPackage(stream))
        //            {
        //                var endRow = 2;
        //                var countCategorysInFile = 0;
        //                // get the second worksheet in the workbook
        //                var worksheet = xlPackage.Workbook.Worksheets[6];
        //                if (worksheet == null)
        //                    throw new NopException("No worksheet found");

        //                var sql = "";// "SET IDENTITY_INSERT [dbo].[ItemsCompatability] ON;";
        //                var id = 0;
        //                var name = string.Empty;
        //                var legacyId = string.Empty;
        //                //find end of data
        //                while (true)
        //                {
        //                    try
        //                    {
        //                        //if (worksheet.Row(endRow).OutlineLevel == 0)
        //                        //{
        //                        //    break;
        //                        //}
        //                        if (worksheet == null || worksheet.Cells == null)
        //                            break;
        //                        if (worksheet.Cells[endRow, 1].Value == null)
        //                            break;

        //                        id = int.Parse(worksheet.Cells[endRow, 1].Value.ToString());
        //                        legacyId = worksheet.Cells[endRow, 2].Value.ToString();

        //                        sql += $" Insert into  [dbo].[LegacyIds] ([ItemId],[LegacyCode],[Deleted]) select {id},'{legacyId}',0; ";


        //                        countCategorysInFile += 1;

        //                        endRow++;
        //                        if (maximunRows > 1000)
        //                        {
        //                            if(sql !=string.Empty)
        //                            _dbContext.ExecuteSqlCommand(sql);
        //                            sql = string.Empty;
        //                            maximunRows = 0;
        //                        }
        //                        continue;
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        endRow++;
        //                        ErrorNotification("Admin.Common.UploadFile" + ex.Message);
        //                        continue;
        //                    }
        //                }

        //              //  sql += "SET IDENTITY_INSERT [dbo].[ItemsCompatability] OFF;";
        //                _dbContext.ExecuteSqlCommand(sql);
        //                SuccessNotification("Admin.Catalog.[ItemsCompatability].Imported");
        //            }



        //        }
        //        else
        //        {
        //            ErrorNotification("Admin.Common.UploadFile");
        //            return RedirectToAction("List");
        //        }
        //        SuccessNotification("Admin.Catalog.[ItemsCompatability].Imported");
        //        return RedirectToAction("Index");
        //    }
        //    catch (Exception exc)
        //    {
        //        ErrorNotification(exc);
        //        return RedirectToAction("Index");
        //    }
        //}

    }
}
