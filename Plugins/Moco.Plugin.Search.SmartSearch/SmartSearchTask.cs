using Nop.Core.Plugins;
using Nop.Services.Tasks;

namespace Moco.Plugin.Search.SmartSearch
{
    
    public class SmartSearchTask : IScheduleTask
    {
		private readonly IPluginFinder _pluginFinder;

		public SmartSearchTask(IPluginFinder pluginFinder)
		{
			this._pluginFinder = pluginFinder;
		}

		public void Execute()
		{
			//is plugin installed?
			var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("Moco.SmartSearch");
			if (null == pluginDescriptor)
				return;

			//plugin
			var plugin = pluginDescriptor.Instance() as SmartSearchPlugin;
			if (null == plugin)
				return;

			//TODO: We Should Stop The Execution Trail If An Error Is Occured In The Process
			plugin.GenerateSmartSearchFeed();
			plugin.UploadFeed();
			plugin.InvokeSmartSearchIndexing();
		}
	}
}
