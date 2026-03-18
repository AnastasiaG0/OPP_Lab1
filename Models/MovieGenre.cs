using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Lab1.Models
{
    public class MovieGenre
    {
        [Key]
        public int Id { get; set; }

        // Внешний ключ
        public int MovieId { get; set; }

        // Внешний ключ
        public int GenreId { get; set; }

        // Навигация
        public virtual Movie? Movie { get; set; }
        public virtual Genre? Genre { get; set; }
    }
}
