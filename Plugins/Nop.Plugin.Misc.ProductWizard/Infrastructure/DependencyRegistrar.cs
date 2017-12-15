using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Misc.ProductWizard.Data;
using Nop.Plugin.Misc.ProductWizard.Domain;
using Nop.Web.Framework.Infrastructure;


namespace Nop.Plugin.Misc.ProductWizard.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_product_wizard";

        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            //builder.RegisterType<ViewTrackingService>().As<IViewTrackingService>().InstancePerLifetimeScope();

            //data context
            this.RegisterPluginDataContext<ProductWizardObjectContext>(builder, CONTEXT_NAME);

            //override required repository with our custom context
            builder.RegisterType<EfRepository<Groups>>()
                .As<IRepository<Groups>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<GroupsItems>>()
              .As<IRepository<GroupsItems>>()
              .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
              .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<RelationsGroupsItems>>()
              .As<IRepository<RelationsGroupsItems>>()
              .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
              .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<ItemsCompatability>>()
             .As<IRepository<ItemsCompatability>>()
             .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
             .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<LegacyId>>()
           .As<IRepository<LegacyId>>()
           .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
           .InstancePerLifetimeScope();

        }

        public int Order
        {
            get { return 1; }
        }
    }
}
