using System; 
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Moco.Plugin.Search.SmartSearch.Models
{
	public class ConfigurationModel
	{
		//Feed Settings
		//----------------------------------------
		[NopResourceDisplayName("Plugins.Search.SmartSearch.AppendDateToFileName")]
		public Boolean AppendDateToFileName { get; set; }

		[NopResourceDisplayName("Plugins.Search.SmartSearch.FilePath")]
		public String FilePath { get; set; }

		[NopResourceDisplayName("Plugins.Search.SmartSearch.FileName")]
		public String FileName { get; set; }

		[NopResourceDisplayName("Plugins.Search.SmartSearch.SmartSearchUrl")]
		public String SmartSearchUrl { get; set; }

		[NopResourceDisplayName("Plugins.Search.SmartSearch.SmartSearchKey")]
		public String SmartSearchKey { get; set; }

		[NopResourceDisplayName("Plugins.Search.SmartSearch.AutoFtp")]
		public Boolean AutoFTP { get; set; }

		[NopResourceDisplayName("Plugins.Search.SmartSearch.FtpAddress")]
		public String FtpAddress { get; set; }

		[NopResourceDisplayName("Plugins.Search.SmartSearch.FtpPort")]
		public Int32 FtpPort { get; set; }

		[NopResourceDisplayName("Plugins.Search.SmartSearch.Username")]
		public String Username { get; set; }

		[NopResourceDisplayName("Plugins.Search.SmartSearch.Password")]
		public String Password { get; set; }

		//Product Result Settings
		[NopResourceDisplayName("Plugins.Search.SmartSearch.ImageSize")]
		public Int32 ImageSize { get; set; }

		[NopResourceDisplayName("Plugins.Search.SmartSearch.IconImageSize")]
		public Int32 ImageIconSize { get; set; }

		[NopResourceDisplayName("Plugins.Search.SmartSearch.Enabled")]
		public Boolean Enabled { get; set; }

		[NopResourceDisplayName("Plugins.Search.SmartSearch.ConfigurationStatus")]
		public String ConfigurationStatus { get; set; }

		//Misc
		public String Message { get; set; }
	}
}
