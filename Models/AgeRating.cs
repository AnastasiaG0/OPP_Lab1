using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Lab1.Models
{
    public enum AgeRating
    {
        [Display(Name = "0+")]
        ZeroPlus = 0,

        [Display(Name = "6+")]
        SixPlus = 6,

        [Display(Name = "12+")]
        TwelvePlus = 12,

        [Display(Name = "16+")]
        SixteenPlus = 16,

        [Display(Name = "18+")]
        EighteenPlus = 18
    }

    // Для отображения
    public static class AgeRatingExtensions
    {
        public static string GetDisplayName(this AgeRating rating)
        {
            return rating switch
            {
                AgeRating.ZeroPlus => "0+",
                AgeRating.SixPlus => "6+",
                AgeRating.TwelvePlus => "12+",
                AgeRating.SixteenPlus => "16+",
                AgeRating.EighteenPlus => "18+",
                _ => rating.ToString()
            };
        }
    }
}
