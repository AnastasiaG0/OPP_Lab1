using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Lab1.Models
{
    public class Actor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя актера обязательно")]
        [MaxLength(150, ErrorMessage = "Имя не может превышать 150 символов")]
        [Display(Name = "Имя актера")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Дата рождения обязательна")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата рождения")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Страна обязательна")]
        [MaxLength(100, ErrorMessage = "Название страны не может превышать 100 символов")]
        [Display(Name = "Страна")]
        public string Country { get; set; } = string.Empty;

        // Фильмы, где снимался актер
        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

        public int Age => DateTime.Now.Year - BirthDate.Year;

        // Отображение полной информации
        public string ActorInfo => $"{Name} ({Country}), {Age} лет";
    }
}
