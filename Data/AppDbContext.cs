using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Lab1.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Lab1.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Director> Directors { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }

        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlite(connectionString);
                //optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка связи Movie -> Director
            modelBuilder.Entity<Movie>()
                .HasOne(m => m.Director)
                .WithMany(d => d.Movies)
                .HasForeignKey(m => m.DirectorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройка связи Role -> Movie
            modelBuilder.Entity<Role>()
                .HasOne(r => r.Movie)
                .WithMany(m => m.Roles)
                .HasForeignKey(r => r.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка связи Role -> Actor
            modelBuilder.Entity<Role>()
                .HasOne(r => r.Actor)
                .WithMany(a => a.Roles)
                .HasForeignKey(r => r.ActorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Составной уникальный индекс для Role
            modelBuilder.Entity<Role>()
                .HasIndex(r => new { r.MovieId, r.ActorId })
                .IsUnique()
                .HasDatabaseName("IX_Unique_Movie_Actor");

            // Movie -> Genre (многие ко многим)
            modelBuilder.Entity<MovieGenre>()
                .HasOne(mg => mg.Movie)
                .WithMany(m => m.MovieGenres)
                .HasForeignKey(mg => mg.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MovieGenre>()
                .HasOne(mg => mg.Genre)
                .WithMany(g => g.MovieGenres)
                .HasForeignKey(mg => mg.GenreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Составной уникальный индекс для MovieGenre (один фильм не может иметь два одинаковых жанра)
            modelBuilder.Entity<MovieGenre>()
                .HasIndex(mg => new { mg.MovieId, mg.GenreId })
                .IsUnique()
                .HasDatabaseName("IX_Unique_Movie_Genre");

            // Индексы для ускорения поиска
            modelBuilder.Entity<Movie>()
                .HasIndex(m => m.Year)
                .HasDatabaseName("IX_Movie_Year");

            modelBuilder.Entity<Movie>()
                .HasIndex(m => m.Title)
                .HasDatabaseName("IX_Movie_Title");

            modelBuilder.Entity<Actor>()
                .HasIndex(a => a.Name)
                .HasDatabaseName("IX_Actor_Name");

            modelBuilder.Entity<Director>()
                .HasIndex(d => d.Name)
                .HasDatabaseName("IX_Director_Name");

            modelBuilder.Entity<Genre>()
                .HasIndex(g => g.Name)
                .IsUnique()
                .HasDatabaseName("IX_Genre_Name");

            // Сидирование данных
            //SeedData(modelBuilder);
        }

        public async Task InitializeDatabaseAsync()
        {
            // Проверяем, есть ли уже данные
            if (await Directors.AnyAsync() || await Actors.AnyAsync())
            {
                return;
            }

            // Добавляем режиссеров
            var directors = new List<Director>
            {
                new Director { Id = 1, Name = "Кристофер Нолан", Age = 53, Awards = "Оскар, Золотой глобус, BAFTA" },
                new Director { Id = 2, Name = "Гай Ричи", Age = 55, Awards = "Премия BAFTA" },
                new Director { Id = 3, Name = "Квентин Тарантино", Age = 60, Awards = "Золотая пальмовая ветвь, Оскар" },
                new Director { Id = 4, Name = "Стивен Спилберг", Age = 77, Awards = "Оскар, Золотой глобус, BAFTA" }
            };
            await Directors.AddRangeAsync(directors);
            await SaveChangesAsync();

            // Добавляем актеров
            var actors = new List<Actor>
            {
                new Actor { Id = 1, Name = "Леонардо ДиКаприо", BirthDate = new DateTime(1974, 11, 11), Country = "США" },
                new Actor { Id = 2, Name = "Мэттью МакКонахи", BirthDate = new DateTime(1969, 11, 4), Country = "США" },
                new Actor { Id = 3, Name = "Джейсон Стэйтем", BirthDate = new DateTime(1967, 7, 26), Country = "Великобритания" },
                new Actor { Id = 4, Name = "Брэд Питт", BirthDate = new DateTime(1963, 12, 18), Country = "США" },
                new Actor { Id = 5, Name = "Том Хэнкс", BirthDate = new DateTime(1956, 7, 9), Country = "США" }
            };
            await Actors.AddRangeAsync(actors);
            await SaveChangesAsync();

            // Добавляем фильмы
            var movies = new List<Movie>
            {
                new Movie {
                    Id = 1,
                    Title = "Интерстеллар",
                    Year = 2014,
                    DurationMinutes = 169,
                    AgeRating = AgeRating.TwelvePlus,
                    Country = "США, Великобритания",
                    Description = "Когда засуха и пыльные бури делают Землю непригодной для жизни, бывший пилот Купер отправляется в рискованную космическую миссию через червоточину, чтобы найти новый дом для человечества. Фильм исследует темы любви, времени и связи между поколениями, где каждое решение имеет долгосрочные последствия.",
                    DirectorId = 1 },
                
                new Movie {
                    Id = 2,
                    Title = "Начало",
                    Year = 2010,
                    DurationMinutes = 148,
                    AgeRating = AgeRating.TwelvePlus,
                    Country = "США, Великобритания",
                    Description = "Дом Кобб — талантливый вор, который крадет ценные секреты из глубин подсознания во время сна. Ему предлагают последнее дело: вместо кражи внедрить идею в сознание человека. В случае успеха он сможет вернуться к нормальной жизни и увидеть своих детей.",
                    DirectorId = 1 },

                new Movie {
                    Id = 3,
                    Title = "Большой куш",
                    Year = 2000,
                    DurationMinutes = 104,
                    AgeRating = AgeRating.SixteenPlus,
                    Country = "Великобритания, США",
                    Description = "История вращается вокруг украденного алмаза в 84 карата и множества персонажей, которые хотят его заполучить: от незадачливых промоутеров Турецкого и Томми до цыгана-боксера Микки, русского гангстера Бориса 'Бритва' и американского мафиози Ави.",
                    DirectorId = 2 },

                new Movie {
                    Id = 4,
                    Title = "Джентльмены",
                    Year = 2019,
                    DurationMinutes = 113,
                    AgeRating = AgeRating.EighteenPlus,
                    Country = "Великобритания, США",
                    Description = "Американский эмигрант Микки Пирсон построил в Лондоне империю по выращиванию марихуаны. Когда он решает продать бизнес и отойти от дел, начинается череда шантажа, интриг и криминальных разборок с участием китайской мафии, русских олигархов и наемного детектива.",
                    DirectorId = 2 },

                new Movie {
                    Id = 5,
                    Title = "Криминальное чтиво",
                    Year = 1994,
                    DurationMinutes = 154,
                    AgeRating = AgeRating.EighteenPlus,
                    Country = "США",
                    Description = "Три переплетающиеся истории из криминального мира Лос-Анджелеса: двое наемных убийц Винсент и Джулс обсуждают философские темы перед 'работой', боксер Буч пытается сбежать от мафиози после нечестного боя, а жена босса Мия проводит вечер с Винсентом.",
                    DirectorId = 3 },

                new Movie {
                    Id = 6,
                    Title = "Однажды в Голливуде",
                    Year = 2019,
                    DurationMinutes = 161,
                    AgeRating = AgeRating.SixteenPlus,
                    Country = "США, Великобритания",
                    Description = "Лос-Анджелес, 1969 год. Затухающая звезда телевестернов Рик Далтон и его дублер Клифф Бут пытаются найти свое место в стремительно меняющемся Голливуде. Их соседями оказываются режиссер Роман Полански и его жена Шэрон Тейт, что приводит к альтернативной версии реальных событий.",
                    DirectorId = 3 },

                new Movie {
                    Id = 7,
                    Title = "Спасти рядового Райана",
                    Year = 1998,
                    DurationMinutes = 169,
                    AgeRating = AgeRating.SixteenPlus,
                    Country = "США",
                    Description = "После высадки союзников в Нормандии капитан Джон Миллер получает задание найти рядового Джеймса Райана, чьи три брата погибли в бою. Отряд из восьми солдат отправляется в тыл врага, чтобы спасти одного человека и вернуть его домой любой ценой.",
                    DirectorId = 4 }
            };
            await Movies.AddRangeAsync(movies);
            await SaveChangesAsync();

            // Добавляем роли
            var roles = new List<Role>
            {
                new Role { Id = 1, CharacterName = "Купер", MovieId = 1, ActorId = 1 },
                new Role { Id = 2, CharacterName = "Амелия Бранд", MovieId = 1, ActorId = 2 },
                new Role { Id = 3, CharacterName = "Дом Кобб", MovieId = 2, ActorId = 1 },
                new Role { Id = 4, CharacterName = "Турецкий", MovieId = 3, ActorId = 3 },
                new Role { Id = 5, CharacterName = "Микки Пирсон", MovieId = 4, ActorId = 3 },
                new Role { Id = 6, CharacterName = "Винсент Вега", MovieId = 5, ActorId = 1 },
                new Role { Id = 7, CharacterName = "Джулс Виннфилд", MovieId = 5, ActorId = 4 },
                new Role { Id = 8, CharacterName = "Рик Далтон", MovieId = 6, ActorId = 1 },
                new Role { Id = 9, CharacterName = "Капитан Миллер", MovieId = 7, ActorId = 5 }
            };
            await Roles.AddRangeAsync(roles);
            await SaveChangesAsync();

            // Добавляем связи с жанрами
            var movieGenres = new List<MovieGenre>
            {
                new MovieGenre { Id = 1, MovieId = 1, GenreId = 1 }, // Интерстеллар - Фантастика
                new MovieGenre { Id = 2, MovieId = 1, GenreId = 3 }, // Интерстеллар - Драма
                new MovieGenre { Id = 3, MovieId = 1, GenreId = 7 }, // Интерстеллар - Приключения
                new MovieGenre { Id = 4, MovieId = 2, GenreId = 1 }, // Начало - Фантастика
                new MovieGenre { Id = 5, MovieId = 2, GenreId = 6 }, // Начало - Триллер
                new MovieGenre { Id = 6, MovieId = 3, GenreId = 2 }, // Большой куш - Боевик
                new MovieGenre { Id = 7, MovieId = 3, GenreId = 4 }, // Большой куш - Комедия
                new MovieGenre { Id = 8, MovieId = 3, GenreId = 5 }, // Большой куш - Криминал
                new MovieGenre { Id = 9, MovieId = 4, GenreId = 2 }, // Джентльмены - Боевик
                new MovieGenre { Id = 10, MovieId = 4, GenreId = 4 }, // Джентльмены - Комедия
                new MovieGenre { Id = 11, MovieId = 4, GenreId = 5 }, // Джентльмены - Криминал
                new MovieGenre { Id = 12, MovieId = 5, GenreId = 3 }, // Криминальное чтиво - Драма
                new MovieGenre { Id = 13, MovieId = 5, GenreId = 5 }, // Криминальное чтиво - Криминал
                new MovieGenre { Id = 14, MovieId = 6, GenreId = 3 }, // Однажды в Голливуде - Драма
                new MovieGenre { Id = 15, MovieId = 6, GenreId = 4 }, // Однажды в Голливуде - Комедия
                new MovieGenre { Id = 16, MovieId = 7, GenreId = 3 }, // Спасти рядового Райана - Драма
                new MovieGenre { Id = 17, MovieId = 7, GenreId = 7 }, // Спасти рядового Райана - Приключения
                new MovieGenre { Id = 18, MovieId = 7, GenreId = 2 }  // Спасти рядового Райана - Боевик
            };
            await MovieGenres.AddRangeAsync(movieGenres);
            await SaveChangesAsync();
        }
    }
}