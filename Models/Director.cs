using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Lab1.Models
{
    public class Director
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя режиссера обязательно")]
        [MaxLength(150, ErrorMessage = "Имя не может превышать 150 символов")]
        [Display(Name = "Имя режиссера")]
        public string Name { get; set; } = string.Empty;

        [Range(0, 120, ErrorMessage = "Возраст не может быть отрицательным числом")]
        [Display(Name = "Возраст")]
        public int Age { get; set; }

        [MaxLength(500, ErrorMessage = "Описание наград не может превышать 500 символов")]
        [Display(Name = "Награды")]
        public string? Awards { get; set; }

        // Связь с фильмами
        public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();

        // Отображение полной информации
        public string DirectorInfo => $"{Name} (Возраст: {Age})";
    }
}
