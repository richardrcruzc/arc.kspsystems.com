using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Controllers
{
    public class UploadDataController : BasePluginController
    {
        //it's quite fast hash (to cheaply distinguish between objects)
        private const string IMAGE_HASH_ALGORITHM = "SHA1";


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
            IDbContext dbContext,  IPermissionService permissionService )
        {
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
        }

        public virtual IActionResult Index()
        {

            return View("~/Plugins/Misc.ProductWizard/Views/UploadData.cshtml");
        }
        [HttpPost]
        public virtual IActionResult ImportCategoryFromXlsx(IFormFile importexcelfile)
        {
            //if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
            //    return AccessDeniedView();

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
                        var endRow = 2;
                        var countCategorysInFile = 0;
                        // get the second worksheet in the workbook
                        var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                            throw new NopException("No worksheet found");

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

                                sql += "INSERT INTO [dbo].[Category] (Id,[Name], UpdatedOnUtc,CreatedOnUtc, CategoryTemplateId, ParentCategoryId, " +
                                      "PictureId, PageSize, AllowCustomersToSelectPageSize,ShowOnHomePage,IncludeInTopMenu,SubjectToAcl,LimitedToStores,Deleted,DisplayOrder,Published) " +
                                      $" SELECT {id},'{name}',getdate(),getdate(), 1,0,0,1,0,0,0,0,0,0,0,1; ";
                                 

                                countCategorysInFile += 1;

                                endRow++;
                                continue;
                            }
                            catch(Exception ex)
                            {
                                ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile")+" "+ ex.Message);
                                continue;
                            }
                        } 

                        sql += "SET IDENTITY_INSERT [dbo].[Category] OFF;";
                        _dbContext.ExecuteSqlCommand(sql);
                        SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Imported"));
                    }



                }
                else
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Imported"));
                return RedirectToAction("Index");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public virtual IActionResult ImportManufacturerFromXlsx(IFormFile importexcelfile)
        {
            //if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
            //    return AccessDeniedView();

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
                        var endRow = 2;
                        var countCategorysInFile = 0;
                        // get the second worksheet in the workbook
                        var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                            throw new NopException("No worksheet found");

                        var sql = "SET IDENTITY_INSERT [dbo].[Manufacturer] ON;";
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

                                sql += "INSERT INTO [dbo].[Manufacturer] (Id,[Name], UpdatedOnUtc,CreatedOnUtc, " +
                                       " PictureId, PageSize, AllowCustomersToSelectPageSize,SubjectToAcl,LimitedToStores,Deleted,DisplayOrder,Published,ManufacturerTemplateId) " +
                                       $" SELECT {id},'{name}',getdate(),getdate(), 0,1,0,0,0,0,0,1,0; ";


                                countCategorysInFile += 1;

                                endRow++;
                                continue;
                            }
                            catch (Exception ex)
                            {
                                ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile") + " " + ex.Message);
                                continue;
                            }
                        }

                        sql += "SET IDENTITY_INSERT [dbo].[Manufacturer] OFF;";
                        _dbContext.ExecuteSqlCommand(sql);
                        SuccessNotification(_localizationService.GetResource("Admin.Catalog.Manufacturer.Imported"));
                    }



                }
                else
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Imported"));
                return RedirectToAction("Index");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public virtual IActionResult ImportProductFromXlsx(IFormFile importexcelfile)
        {
            //if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
            //    return AccessDeniedView();

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
                        // get the second worksheet in the workbook
                        var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                            throw new NopException("No worksheet found");

                        var sql = "SET IDENTITY_INSERT [dbo].[Product] ON;";
                        var id = 0;
                        var name = string.Empty;
                        var Vendor = 0;
                        var SKU = string.Empty;
                        var Gtin = string.Empty;
                        var StockQuantity = 0;
                        decimal Price = 0M;
                        var Weight = 0;
                        var Length = 0;
                        var Height = 0;
                        var Width = 0;
                        var Categories = 0;
                        var Manufacturers = 0;
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

                                var product = _productService.GetProductById(id);
                                if (product != null)
                                {
                                    endRow++;
                                    continue;
                                }

                                Active = worksheet.Cells[endRow, 2].Value.ToString()=="Y"?1:0;

                                int.TryParse(worksheet.Cells[endRow, 3].Value.ToString(),out Categories);

                                int.TryParse(worksheet.Cells[endRow, 4].Value.ToString(),out BrandID);

                                SKU = worksheet.Cells[endRow, 5].Value.ToString().Trim().Replace("'","''"); 
                            
                                name = worksheet.Cells[endRow, 6].Value.ToString().Replace("'", "''");


                                int.TryParse(worksheet.Cells[endRow, 7].Value.ToString(), out Weight);
                                int.TryParse(worksheet.Cells[endRow, 8].Value.ToString(), out Length);
                                int.TryParse(worksheet.Cells[endRow, 9].Value.ToString(), out Width);
                                int.TryParse(worksheet.Cells[endRow, 10].Value.ToString(), out Height);

                                 int.TryParse(worksheet.Cells[endRow, 11].Value.ToString(), out StockQuantity);

                                int.TryParse(worksheet.Cells[endRow, 14].Value.ToString(), out DropShip);

                                Gtin = worksheet.Cells[endRow, 15].Value.ToString().Trim().Replace("'", "''");

                                if(worksheet.Cells[endRow, 16].Value.ToString() != "NULL")
                                ExcludeGoogleFeed = worksheet.Cells[endRow, 16].Value.ToString() == "Y" ? 1 : 0;

                                color = worksheet.Cells[endRow, 17].Value.ToString().Replace("'", "''");

                                //Vendor = int.Parse(worksheet.Cells[endRow, 5].Value.ToString()); 
                                //var t = decimal.TryParse(worksheet.Cells[endRow, 12].Value.ToString(),out Price); 
                                //Manufacturers = int.Parse(worksheet.Cells[endRow, 20].Value.ToString()); 
                                //picture = worksheet.Cells[endRow, 21].Value.ToString();
                             



                                sql += "INSERT INTO [dbo].[Product] ( "+
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
                                " 1,1,1,1,1,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, 0,0,0,0,0,0 ,0,0,1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0  ,0 go;";



                                countCategorysInFile += 1;
                                endRow++;

                                maximunRows++;

                                if(maximunRows>100)
                                { 
                                    sql += "SET IDENTITY_INSERT [dbo].[Product] OFF;";
                                    _dbContext.ExecuteSqlCommand(sql);
                                    sql = "SET IDENTITY_INSERT [dbo].[Product] ON;";
                                    maximunRows = 0;
                                }


                                continue;
                            }
                            catch (Exception ex)
                            {
                                ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile") + " " + ex.Message);
                                continue;
                            }
                        }

                        sql += "SET IDENTITY_INSERT [dbo].[Product] OFF;";
                        _dbContext.ExecuteSqlCommand(sql);
                        SuccessNotification(_localizationService.GetResource("Admin.Catalog.Product.Imported"));



 
                          worksheet = xlPackage.Workbook.Worksheets.LastOrDefault();
                        if (worksheet == null)
                            throw new NopException("No worksheet found");



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
                                    sql =string.Empty;
                                    maximunRows = 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile") + " " + ex.Message);
                                continue;
                            }
                        }

                    
                        _dbContext.ExecuteSqlCommand(sql);

                    }



                }
                else
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Imported"));
                return RedirectToAction("Index");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public virtual void ImportCategorysFromXlsx(Stream stream)
        {

            using (var xlPackage = new ExcelPackage(stream))
            {
                var endRow = 2;
                var countCategorysInFile = 0;
                // get the second worksheet in the workbook
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new NopException("No worksheet found");

                //find end of data
                while (true)
                {
                    if (worksheet.Row(endRow).OutlineLevel == 0)
                    {
                        break;
                    }
                    if (worksheet == null || worksheet.Cells == null)
                        break;

                    var id = int.Parse(worksheet.Cells[endRow, 1].Value.ToString());
                    var name = worksheet.Cells[endRow, 2].Value.ToString();

                    var category = new Category
                    { 
                        Id = id,
                        Name = name,
                        UpdatedOnUtc = DateTime.Now,
                        CreatedOnUtc = DateTime.Now,
                        CategoryTemplateId = 1,
                        ParentCategoryId = 0,
                        PictureId = 0,
                        PageSize = 0,
                        AllowCustomersToSelectPageSize = false,
                        ShowOnHomePage = false,
                        IncludeInTopMenu = false,
                        SubjectToAcl = false,
                        LimitedToStores = false,
                        Deleted = false,
                        DisplayOrder = 0,
                        Published = true 
                    }; 

                    _dbContext.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Category] ON");

                    _categoryService.InsertCategory(category);

                    _dbContext.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Category] OFF");
                    countCategorysInFile += 1;

                    endRow++;
                    continue;
                }
           }

                
             
            

        }

    }
}
