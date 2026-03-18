using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Lab1.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя персонажа обязательно")]
        [MaxLength(150, ErrorMessage = "Имя персонажа не может превышать 150 символов")]
        [Display(Name = "Имя персонажа")]
        public string CharacterName { get; set; } = string.Empty;

        // Внешний ключ на фильм
        [Display(Name = "ID фильма")]
        public int MovieId { get; set; }

        // Внешний ключ на актера
        [Display(Name = "ID актера")]
        public int ActorId { get; set; }

        // Навигационные свойства
        public virtual Movie? Movie { get; set; }
        public virtual Actor? Actor { get; set; }

        // Отображение информации о роли
        public string RoleInfo => $"{Actor?.Name} в роли {CharacterName}";
    }
}
