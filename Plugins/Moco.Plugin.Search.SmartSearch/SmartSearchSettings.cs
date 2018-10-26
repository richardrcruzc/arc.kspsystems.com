using System;
using System.IO;

using Nop.Core.Configuration;

namespace Moco.Plugin.Search.SmartSearch
{
	public class SmartSearchSettings : ISettings
	{
		#region public Members
		public Boolean AppendDateToFileName { get; set; }
		public String FileName { get; set; }
		public String FilePath { get; set; }
		public String SmartSearchUrl { get; set; }
        public String HostedSSUrl { get; set; }
		public String SmartSearchKey { get; set; }
		public Int32 ImageSize { get; set; }
		public Int32 ImageIconSize { get; set; }
		public Boolean AutoFTP { get; set; }
		public String FtpAddress { get; set; }
		public Int32 FtpPort { get; set; }
		public String Username { get; set; }
		public String Password { get; set; }
		public Boolean Enabled { get; set; }
        public Boolean Debug { get; set; }
        public Boolean IncludeProductImage { get; set; }
        public Boolean IncludeProductSaleCount { get; set; }
        public Boolean ExcludeProductCategory { get; set; }

		#endregion

		#region public helpers
		public string GetFullFilePath()
		{
			return Path.Combine(FilePath, FileName);
		}
		public string GetSmartSearchIndexingPath()
		{
			string indexingQueryString = "generateindex";
			string indexingQueryStringValue = "true";
			if (SmartSearchUrl.LastIndexOf("/", StringComparison.CurrentCulture).Equals(SmartSearchUrl.Length))
			{
				SmartSearchUrl.Remove(SmartSearchUrl.Length - 1);
			}
			if (!SmartSearchUrl.ToLower().Contains("http://"))
			{
				if (SmartSearchUrl.ToLower().Contains("https://"))
				{
					//do nothing,, its https
				}
				else
				{
					SmartSearchUrl = string.Format("http://{0}", SmartSearchUrl);
				}
			}
			return string.Format("{0}?{1}={2}", SmartSearchUrl, indexingQueryString, indexingQueryStringValue);
		}
		public string GetSafeSmartSearchPath()
		{
			if (SmartSearchUrl.LastIndexOf("/", StringComparison.CurrentCulture).Equals(SmartSearchUrl.Length))
			{
				SmartSearchUrl.Remove(SmartSearchUrl.Length - 1);
			}
			if (!SmartSearchUrl.ToLower().Contains("http://"))
			{
				if (SmartSearchUrl.ToLower().Contains("https://"))
				{
					//do nothing,, its https
				}
				else
				{
					SmartSearchUrl = string.Format("http://{0}", SmartSearchUrl);
				}
			}
			return SmartSearchUrl;
		}
		#endregion
	}
}
