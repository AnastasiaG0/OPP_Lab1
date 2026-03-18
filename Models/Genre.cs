using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Lab1.Models
{
    public class Genre
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название жанра обязательно")]
        [MaxLength(100, ErrorMessage = "Название жанра не может превышать 100 символов")]
        [Display(Name = "Название жанра")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Описание жанра не может превышать 500 символов")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        // Навигация
        public virtual ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();

        public override string ToString() => Name;
    }

    public static class PredefinedGenres
    {
        public static readonly List<Genre> All = new List<Genre>
        {
            new Genre { Id = 1, Name = "Фантастика", Description = "Научная фантастика, космос, будущее" },
            new Genre { Id = 2, Name = "Боевик", Description = "Экшн, перестрелки, погони" },
            new Genre { Id = 3, Name = "Драма", Description = "Серьезные, эмоциональные истории" },
            new Genre { Id = 4, Name = "Комедия", Description = "Смешные, развлекательные фильмы" },
            new Genre { Id = 5, Name = "Криминал", Description = "Гангстеры, преступления, детективы" },
            new Genre { Id = 6, Name = "Триллер", Description = "Напряженные, захватывающие сюжеты" },
            new Genre { Id = 7, Name = "Приключения", Description = "Путешествия, поиски, опасности" },
            new Genre { Id = 8, Name = "Ужасы", Description = "Страшные, пугающие фильмы" },
            new Genre { Id = 9, Name = "Мелодрама", Description = "Любовные истории, романтика" },
            new Genre { Id = 10, Name = "Детектив", Description = "Расследования, загадки" },
            new Genre { Id = 11, Name = "Вестерн", Description = "Дикий запад, ковбои" },
            new Genre { Id = 12, Name = "Фэнтези", Description = "Магия, мифические существа" },
            new Genre { Id = 13, Name = "Документальный", Description = "Реальные события, факты" },
            new Genre { Id = 14, Name = "Исторический", Description = "Исторические события, эпохи" },
            new Genre { Id = 15, Name = "Музыкальный", Description = "Мюзиклы, музыка" }
        };

        public static Dictionary<int, string> GetGenresDictionary()
        {
            return All.ToDictionary(g => g.Id, g => g.Name);
        }

        public static Genre? GetById(int id)
        {
            return All.FirstOrDefault(g => g.Id == id);
        }

        public static bool Exists(int id)
        {
            return All.Any(g => g.Id == id);
        }
    }
}
