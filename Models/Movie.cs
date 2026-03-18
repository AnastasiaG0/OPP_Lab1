using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace Lab1.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название фильма обязательно")]
        [MaxLength(200, ErrorMessage = "Название не может превышать 200 символов")]
        [Display(Name = "Название фильма")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Год выпуска обязателен")]
        [Range(1890, 2050, ErrorMessage = "Год выпуска должен быть между 1890 и 2050")]
        [Display(Name = "Год выпуска")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Длительность обязательна")]
        [Range(1, 1000, ErrorMessage = "Длительность должна быть от 1 до 1000 минут")]
        [Display(Name = "Длительность (мин)")]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "Возрастное ограничение обязательно")]
        [Display(Name = "Возрастное ограничение")]
        public AgeRating AgeRating { get; set; }

        [Required(ErrorMessage = "Страна производства обязательна")]
        [MaxLength(100, ErrorMessage = "Название страны не может превышать 100 символов")]
        [Display(Name = "Страна производства")]
        public string Country { get; set; } = string.Empty;

        [MaxLength(2000, ErrorMessage = "Описание не может превышать 2000 символов")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Описание фильма")]
        public string? Description { get; set; }

        // Внешний ключ
        [Display(Name = "Режиссер")]
        public int DirectorId { get; set; }

        // Навигация
        public virtual Director? Director { get; set; }
        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
        public virtual ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();

        // Отображение информации о фильме
        public int Hours => DurationMinutes / 60;
        public int Minutes => DurationMinutes % 60;
        public string FormattedDuration => Hours > 0
            ? $"{Hours} ч {Minutes:00} мин"
            : $"{Minutes} мин";

        // Полная информация о фильме
        public string MovieInfo => $"{Title} ({Year}), {Country}, {FormattedDuration}, {AgeRating.GetDisplayName()}";
    }
}
