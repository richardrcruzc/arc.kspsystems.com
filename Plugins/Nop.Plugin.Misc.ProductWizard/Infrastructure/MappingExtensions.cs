
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.ProductWizard.Domain;
using Nop.Plugin.Misc.ProductWizard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Infrastructure
{
    public static class MappingExtensions
    {
        //public static TDestination MapTo<TSource, TDestination>(this TSource source)
        //{
        //    return AutoMapperConfiguration.Mapper.Map<TSource, TDestination>(source);
        //}

        //public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        //{
        //    return AutoMapperConfiguration.Mapper.Map(source, destination);
        //}


        public static GroupsModel ToModel(this Groups entity)
        {

            var model = new GroupsModel
            {
                 GroupName = entity.GroupName,
                  Id = entity.Id,
                   Interval = entity.Interval,
                    Percentage = entity.Percentage,
                    CreatedOnUtc = entity.CreatedOnUtc,
                    UpdatedOnUtc = entity.UpdatedOnUtc
                
            };

            return model;

            
            //return entity.MapTo<Groups, GroupsModel>();




        }

        public static Groups ToEntity(this GroupsModel model)
        {
            //return model.MapTo<GroupsModel, Groups>();


            var entity = new Groups
            {
                GroupName = model.GroupName,
                Id = model.Id,
                Interval = model.Interval,
                Percentage = model.Percentage,
                CreatedOnUtc = model.CreatedOnUtc,
                UpdatedOnUtc = model.UpdatedOnUtc
            };

            return entity;

        }

        public static Groups ToEntity(this GroupsModel model, Groups destination)
        {
            destination.GroupName = model.GroupName;
           // destination.Id = model.Id;
            destination.Interval = model.Interval;
            destination.Percentage = model.Percentage;
            destination.CreatedOnUtc = model.CreatedOnUtc;
            destination.UpdatedOnUtc = model.UpdatedOnUtc;

            return destination;
            //return model.MapTo(destination);
        }

    }
}

