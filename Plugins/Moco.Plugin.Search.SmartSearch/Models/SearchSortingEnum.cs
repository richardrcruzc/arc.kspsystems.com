using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Moco.Plugin.Search.SmartSearch.Models
{
    public enum SearchSortingEnum
    {
        /// <summary>
        /// Relevance.
        /// </summary>
        [Display(Name = "Relevance", Description = "Relevance")]
        Relevance,

        /// <summary>
        /// Highest Rated.
        /// </summary>
        [Display(Name = "Highest Rated", Description = "Highest Rated")]
        HighestRated,

        /// <summary>
        /// Best Sellers.
        /// </summary>
        [Display(Name = "Best Sellers", Description = "Best Sellers")]
        BestSellers,

        /// <summary>
        /// New Arrivals.
        /// </summary>
        [Display(Name = "New Arrivals", Description = "New Arrivals")]
        NewArrivals,

        /// <summary>
        /// Lowest Price.
        /// </summary>
        [Display(Name = "Lowest Price", Description = "Lowest Price")]
        LowestPrice,

        /// <summary>
        /// Highest Price
        /// </summary>
        [Display(Name = "Highest Price", Description = "Highest Price")]
        HighestPrice
    }

    public static class EnumExtensions {
        public static string GetEnumDisplayName<TEnum>(TEnum value)
        {
            string retValue = value.GetType().GetAttributeValue<TEnum, DisplayAttribute, string>(value, x => x.Name);
            if (string.IsNullOrWhiteSpace(retValue)) return value.ToString();
            return retValue;
        }

        public static TExpected GetAttributeValue<TEnum, T, TExpected>(this Type enumeration, TEnum val, Func<T, TExpected> expression)
            where T : Attribute
        {
            T attribute = enumeration.GetMember(val.ToString())[0].GetCustomAttributes(typeof(T), false).Cast<T>().SingleOrDefault();

            if (attribute == null)
                return default(TExpected);

            return expression(attribute);
        }
    }
}
