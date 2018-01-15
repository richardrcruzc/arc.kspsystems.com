using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.ProductWizard.Models;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Components
{
     
    public class WaterMarkedPictureViewComponent : NopViewComponent
    {
        private readonly ISettingService _settingService;
        private readonly IPictureService _pictureService;
        private readonly IHostingEnvironment _hostingEnvironment;
        public WaterMarkedPictureViewComponent(IPictureService pictureService,
            IHostingEnvironment hostingEnvironment,
            ISettingService settingService)
        {
            this._hostingEnvironment = hostingEnvironment;
            this._pictureService = pictureService;
            this._settingService = settingService;
        }
        public IViewComponentResult BackupInvoke(string alternateText, string imageUrl, string title, int id)
        {

            string[] pictureNameArray = imageUrl.Split('/');
            string pictureName = pictureNameArray[pictureNameArray.Length - 1];

            //string cAltText, cImg;

           // string wmFile = Path.Combine(_hostingEnvironment.WebRootPath, "images\\thumbs\\wm-" + pictureName);
            string file = Path.Combine(_hostingEnvironment.WebRootPath, "images\\thumbs\\" + pictureName);
            if (File.Exists(file))
            {
                    System.Drawing.Image image = System.Drawing.Image.FromFile(file);//This is the background image 

                    System.Drawing.Image logo = System.Drawing.Image.FromFile(Path.Combine(_hostingEnvironment.WebRootPath, "images\\WaterMarkImages\\ARC_Logo_214.png")); //This is your watermark 
                    Graphics g = System.Drawing.Graphics.FromImage(image); //Create graphics object of the background image //So that you can draw your logo on it
                    Bitmap TransparentLogo = new Bitmap(logo.Width, logo.Height); //Create a blank bitmap object //to which we //draw our transparent logo
                    Graphics TGraphics = Graphics.FromImage(TransparentLogo);//Create a graphics object so that //we can draw //on the blank bitmap image object
                    ColorMatrix ColorMatrix = new ColorMatrix(); //An image is represenred as a 5X4 matrix(i.e 4 //columns and 5 //rows) 
                    ColorMatrix.Matrix33 = 0.40F;//the 3rd element of the 4th row represents the transparency 
                    ImageAttributes ImgAttributes = new ImageAttributes();//an ImageAttributes object is used to set all //the alpha //values.This is done by initializing a color matrix and setting the alpha scaling value in the matrix.The address of //the color matrix is passed to the SetColorMatrix method of the //ImageAttributes object, and the //ImageAttributes object is passed to the DrawImage method of the Graphics object.
                    ImgAttributes.SetColorMatrix(ColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap); TGraphics.DrawImage(logo, new Rectangle(0, 0, TransparentLogo.Width, TransparentLogo.Height), 0, 0, TransparentLogo.Width, TransparentLogo.Height, GraphicsUnit.Pixel, ImgAttributes);
                    TGraphics.Dispose();

                    g.DrawImage(TransparentLogo, (image.Width / 2.5f), (image.Height / 2.5f));
                    // g.DrawImage(TransparentLogo, (image.Width / 2) - 140, (image.Height / 2) - 80);

                    MemoryStream smImg = new MemoryStream();
                    image.Save(smImg, ImageFormat.Jpeg);

                    byte[] bImg = smImg.ToArray();
                    imageUrl = "data:image/jpeg;base64," + Convert.ToBase64String(bImg); 

                }
            

            PictureModel model = new PictureModel {AlternateText=alternateText, ImageUrl = imageUrl, Title= title, Id =id };
            return View("~/Plugins/Misc.ProductWizard/Views/WaterMarkedPicture.cshtml", model);
        }
        public IViewComponentResult Invoke(string imageUrl )
        {

            string[] pictureNameArray = imageUrl.Split('/');
            string pictureName = pictureNameArray[pictureNameArray.Length - 1];

            //string cAltText, cImg;

            // string wmFile = Path.Combine(_hostingEnvironment.WebRootPath, "images\\thumbs\\wm-" + pictureName);
            string file = Path.Combine(_hostingEnvironment.WebRootPath, "images\\thumbs\\" + pictureName);
            if (File.Exists(file))
            {
                System.Drawing.Image image = System.Drawing.Image.FromFile(file);//This is the background image 

                System.Drawing.Image logo = System.Drawing.Image.FromFile(Path.Combine(_hostingEnvironment.WebRootPath, "images\\WaterMarkImages\\ARC_Logo_214.png")); //This is your watermark 
                Graphics g = System.Drawing.Graphics.FromImage(image); //Create graphics object of the background image //So that you can draw your logo on it
                Bitmap TransparentLogo = new Bitmap(logo.Width, logo.Height); //Create a blank bitmap object //to which we //draw our transparent logo
                Graphics TGraphics = Graphics.FromImage(TransparentLogo);//Create a graphics object so that //we can draw //on the blank bitmap image object
                ColorMatrix ColorMatrix = new ColorMatrix(); //An image is represenred as a 5X4 matrix(i.e 4 //columns and 5 //rows) 
                ColorMatrix.Matrix33 = 0.40F;//the 3rd element of the 4th row represents the transparency 
                ImageAttributes ImgAttributes = new ImageAttributes();//an ImageAttributes object is used to set all //the alpha //values.This is done by initializing a color matrix and setting the alpha scaling value in the matrix.The address of //the color matrix is passed to the SetColorMatrix method of the //ImageAttributes object, and the //ImageAttributes object is passed to the DrawImage method of the Graphics object.
                ImgAttributes.SetColorMatrix(ColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap); TGraphics.DrawImage(logo, new Rectangle(0, 0, TransparentLogo.Width, TransparentLogo.Height), 0, 0, TransparentLogo.Width, TransparentLogo.Height, GraphicsUnit.Pixel, ImgAttributes);
                TGraphics.Dispose();

                g.DrawImage(TransparentLogo, (image.Width / 2.5f), (image.Height / 2.5f));
                // g.DrawImage(TransparentLogo, (image.Width / 2) - 140, (image.Height / 2) - 80);

                MemoryStream smImg = new MemoryStream();
                image.Save(smImg, ImageFormat.Jpeg);

                byte[] bImg = smImg.ToArray();
                imageUrl = "data:image/jpeg;base64," + Convert.ToBase64String(bImg);

            }
            else
            { imageUrl = ""; }

            
            // PictureModel model = new PictureModel { AlternateText = alternateText, ImageUrl = imageUrl, Title = title, Id = id };
            
            return View("~/Plugins/Misc.ProductWizard/Views/WaterMarkedPicture.cshtml", imageUrl);
        }
    }
}
