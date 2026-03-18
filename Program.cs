//using Azure;
using Lab1.Data;
using Lab1.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Text;


namespace Lab1;

class Program
{
    private static Actor? _tempActor;
    private static Director? _tempDirector;
    private static List<Actor> _selectedActors = new();
    private static List<Role> _selectedRoles = new();
    private static List<Genre> _selectedGenres = new();
    private static bool _directorConfirmed = false;
    private static bool _movieInfoFilled = false;
    private static bool _ageRatingFilled = false;
    private static bool _countryFilled = false;
    private static bool _descriptionFilled = false;

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("============ Онлайн-кинотеатр ============");

        using (var context = new AppDbContext())
        {
            try
            {
                // Применяем миграции
                await context.Database.MigrateAsync();
                //Console.WriteLine("✓ Миграции применены");

                // Добавляем или обновляем жанры
                await SyncGenresAsync(context);
                // Инициализируем базу данных начальными данными
                await context.InitializeDatabaseAsync();////////////////////////////////////

                // Показываем все жанры
                /*var genresCount = await context.Genres.CountAsync();
                Console.WriteLine($"\nВсего жанров в БД: {genresCount}");

                var allGenres = await context.Genres
                    .Include(g => g.MovieGenres)
                    .OrderBy(g => g.Id)
                    .ToListAsync();

                foreach (var g in allGenres)
                {
                    int moviesCount = g.MovieGenres?.Count ?? 0;
                    Console.WriteLine($"   [{g.Id}] {g.Name} - фильмов: {moviesCount}");
                }

                Console.WriteLine();*/
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка при инициализации БД: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Внутренняя ошибка: {ex.InnerException.Message}");
                }
                return;
            }
        }

        bool exit = false;
        while (!exit)
        {
            Console.WriteLine("\n╔═════════════════════════════════════════╗");
            Console.WriteLine("║               ГЛАВНОЕ МЕНЮ              ║");
            Console.WriteLine("╠═════════════════════════════════════════╣");
            Console.WriteLine("║ 1. Показать все фильмы                  ║");
            Console.WriteLine("║ 2. Показать все жанры                   ║");
            Console.WriteLine("║ 3. Показать всех режиссеров             ║");
            Console.WriteLine("║ 4. Показать всех актеров                ║");
            Console.WriteLine("║ 5. Добавить новый фильм                 ║");
            Console.WriteLine("║ 6. Редактировать информацию о фильме    ║");
            Console.WriteLine("║ 7. Редактировать информацию о режиссере ║");
            Console.WriteLine("║ 8. Редактировать информацию об актере   ║");
            Console.WriteLine("║ 9. Удалить фильм                        ║");
            Console.WriteLine("║ 10. Показать фильмы по жанру            ║");
            Console.WriteLine("║ 0. Выход                                ║");
            Console.WriteLine("╚═════════════════════════════════════════╝");
            Console.Write("Выберите пункт: ");

            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await ShowAllMovies();
                        break;
                    case "2":
                        await ShowAllGenres();
                        break;
                    case "3":
                        await ShowAllDirectors();
                        break;
                    case "4":
                        await ShowAllActors();
                        break;
                    case "5":
                        await AddNewMovie();
                        break;
                    case "6":
                        await EditMovie();
                        break;
                    case "7":
                        await EditDirector();
                        break;
                    case "8":
                        await EditActor();
                        break;
                    case "9":
                        await DeleteMovie();
                        break;
                    case "10":
                        await ShowMoviesByGenre();
                        break;
                    case "0":
                        exit = true;
                        Console.WriteLine("Выход из программы...");
                        break;
                    default:
                        Console.WriteLine("❌ Неверный ввод. Пожалуйста, выберите пункт из меню.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
            }
        }
    }

    /*private static async Task SeedPredefinedGenresAsync(AppDbContext context)
    {
        await context.Genres.AddRangeAsync(PredefinedGenres.All);
        await context.SaveChangesAsync();
        Console.WriteLine("✅ Предопределенные жанры успешно добавлены в базу данных.");
    }*/

    // ==================== МЕТОДЫ ОТОБРАЖЕНИЯ ====================

    private static async Task ShowAllMovies()
    {
        using var context = new AppDbContext();
        var movies = await context.Movies
            .Include(m => m.Director)
            .Include(m => m.Roles)
                .ThenInclude(r => r.Actor)
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .OrderBy(m => m.Year)
            .ToListAsync();

        if (!movies.Any())
        {
            Console.WriteLine("\n📢 Фильмы не найдены.");
            return;
        }

        Console.WriteLine($"\n{"=".PadRight(99, '=')}");
        Console.WriteLine($"{"СПИСОК ВСЕХ ФИЛЬМОВ",59}");
        Console.WriteLine($"{"=".PadRight(99, '=')}");

        foreach (var movie in movies)
        {
            Console.WriteLine($"\n   [{movie.Id}] {movie.Title} ({movie.Year})");
            Console.WriteLine($"   🌍 Страна: {movie.Country}");
            Console.WriteLine($"   ⏱  Длительность: {movie.FormattedDuration}");
            Console.WriteLine($"   🚫 Возрастное ограничение: {movie.AgeRating.GetDisplayName()}");
            Console.WriteLine($"   🎬 Режиссер: {movie.Director?.Name ?? "Неизвестен"}");

            if (movie.MovieGenres?.Any() == true)
            {
                var genres = movie.MovieGenres.Select(mg => mg.Genre?.Name);
                Console.WriteLine($"   📚 Жанры: {string.Join(", ", genres)}");
            }

            if (!string.IsNullOrEmpty(movie.Description))
            {
                Console.WriteLine($"   📝 Описание:");
                PrintWrappedText(movie.Description, 8, 80);
            }

            if (movie.Roles?.Any() == true)
            {
                Console.WriteLine($"   🎭 В ролях ({movie.Roles.Count}):");
                foreach (var role in movie.Roles.OrderBy(r => r.Actor?.Name))
                {
                    Console.WriteLine($"        • {role.Actor?.Name} — {role.CharacterName}");
                }
            }
            Console.WriteLine($"{"-".PadRight(99, '-')}");
        }

        Console.WriteLine($"\n   Всего фильмов: {movies.Count}\n");
    }

    private static async Task ShowAllGenres()
    {
        using var context = new AppDbContext();
        var genres = await context.Genres
            .Include(g => g.MovieGenres)
            .OrderBy(g => g.Name)
            .ToListAsync();

        if (!genres.Any())
        {
            Console.WriteLine("\n📢 Жанры не найдены.");
            return;
        }

        Console.WriteLine($"\n{"=".PadRight(62, '=')}");
        Console.WriteLine($"{"СПИСОК ВСЕХ ЖАНРОВ",40}");
        Console.WriteLine($"{"=".PadRight(62, '=')}");

        foreach (var genre in genres)
        {
            int moviesCount = genre.MovieGenres?.Count ?? 0;
            Console.WriteLine($"\n   [{genre.Id}] {genre.Name}");
            Console.WriteLine($"   📝 Описание: {genre.Description ?? "Нет описания"}");
            Console.WriteLine($"   🎬 Фильмов в жанре: {moviesCount}");
            Console.WriteLine($"{"-".PadRight(62, '-')}");
        }

        Console.WriteLine($"\n   Всего жанров: {genres.Count}");
    }

    private static async Task ShowAllDirectors()
    {
        using var context = new AppDbContext();
        var directors = await context.Directors
            .Include(d => d.Movies)
            .OrderBy(d => d.Name)
            .ToListAsync();

        if (!directors.Any())
        {
            Console.WriteLine("\n📢 Режиссеры не найдены.");
            return;
        }

        Console.WriteLine($"\n{"=".PadRight(80, '=')}");
        Console.WriteLine($"{"СПИСОК ВСЕХ РЕЖИССЕРОВ",51}");
        Console.WriteLine($"{"=".PadRight(80, '=')}");

        foreach (var director in directors)
        {
            Console.WriteLine($"\n   [{director.Id}] {director.Name}");
            Console.WriteLine($"   🗓  Возраст: {director.Age} лет");

            if (!string.IsNullOrEmpty(director.Awards))
            {
                Console.WriteLine($"   🏆 Награды: {director.Awards}");
            }

            int moviesCount = director.Movies?.Count ?? 0;
            Console.WriteLine($"   🎥 Снял фильмов: {moviesCount}");

            if (moviesCount > 0)
            {
                var movieTitles = director.Movies?.Select(m => m.Title);
                Console.WriteLine($"   🎬 Последние фильмы: {string.Join(", ", movieTitles ?? new List<string>())}");
                //if (moviesCount > 3) Console.WriteLine($"      ... и ещё {moviesCount - 3}");
            }
            Console.WriteLine($"{"-".PadRight(80, '-')}");
        }

        Console.WriteLine($"\n   Всего режиссеров: {directors.Count}");
    }

    private static async Task ShowAllActors()
    {
        using var context = new AppDbContext();
        var actors = await context.Actors
            .Include(a => a.Roles)
                .ThenInclude(r => r.Movie)
            .OrderBy(a => a.Name)
            .ToListAsync();

        if (!actors.Any())
        {
            Console.WriteLine("\n📢 Актеры не найдены.");
            return;
        }

        Console.WriteLine($"\n{"=".PadRight(59, '=')}");
        Console.WriteLine($"{"СПИСОК ВСЕХ АКТЕРОВ",40}");
        Console.WriteLine($"{"=".PadRight(59, '=')}");

        foreach (var actor in actors)
        {
            Console.WriteLine($"\n   [{actor.Id}] {actor.Name}");
            Console.WriteLine($"   🌍 Страна: {actor.Country}");
            Console.WriteLine($"   🎂 Дата рождения: {actor.BirthDate:dd.MM.yyyy} ({actor.Age} лет)");

            int rolesCount = actor.Roles?.Count ?? 0;
            Console.WriteLine($"   🎬 Снялся(ась) в фильмах: {rolesCount}");

            if (rolesCount > 0)
            {
                var movies = actor.Roles?
                    .Where(r => r.Movie != null)
                    .Select(r => $"        • {r.CharacterName} ({r.Movie?.Title})\n");

                if (movies?.Any() == true)
                {
                    Console.WriteLine($"   🎭 Роли: \n{string.Join("", movies)}");
                    //if (rolesCount > 3) Console.WriteLine($"      ... и ещё {rolesCount - 3}");
                }
            }
            Console.WriteLine($"{"-".PadRight(59, '-')}");
        }

        Console.WriteLine($"\n   Всего актеров: {actors.Count}");
    }

    /*private static async Task SyncGenresAsync(AppDbContext context)
    {
        var dbGenres = await context.Genres.ToDictionaryAsync(g => g.Id);
        var predefinedGenres = PredefinedGenres.All.ToDictionary(g => g.Id);

        // 1. Удаляем жанры, которых нет в PredefinedGenres и которые не используются
        foreach (var dbGenre in dbGenres.Values.ToList()) // ToList() важно, чтобы не изменять коллекцию во время итерации
        {
            if (!predefinedGenres.ContainsKey(dbGenre.Id))
            {
                var isGenreUsed = await context.MovieGenres
                    .AnyAsync(mg => mg.GenreId == dbGenre.Id);

                if (isGenreUsed)
                {
                    Console.WriteLine($"⚠ Жанр '{dbGenre.Name}' (Id={dbGenre.Id}) используется в фильмах и НЕ МОЖЕТ быть удалён");
                }
                else
                {
                    context.Genres.Remove(dbGenre);
                    Console.WriteLine($"➖ Удалён неиспользуемый жанр: {dbGenre.Name} (Id={dbGenre.Id})");
                }
            }
        }

        // 2. Добавляем новые жанры
        foreach (var kvp in predefinedGenres)
        {
            if (!dbGenres.ContainsKey(kvp.Key))
            {
                await context.Genres.AddAsync(new Genre
                {
                    Id = kvp.Key,
                    Name = kvp.Value.Name,
                    Description = kvp.Value.Description
                });
                Console.WriteLine($"➕ Добавлен новый жанр: {kvp.Value.Name}");
            }
            else
            {
                // 3. Обновляем существующие (на случай, если изменили название или описание)
                var existingGenre = dbGenres[kvp.Key];
                if (existingGenre.Name != kvp.Value.Name ||
                    existingGenre.Description != kvp.Value.Description)
                {
                    existingGenre.Name = kvp.Value.Name;
                    existingGenre.Description = kvp.Value.Description;
                    context.Genres.Update(existingGenre);
                    Console.WriteLine($"🔄 Обновлён жанр: {kvp.Value.Name}");
                }
            }
        }

        // Сохраняем все изменения
        await context.SaveChangesAsync();
    }*/

    private static async Task SyncGenresAsync(AppDbContext context)
    {
        var dbGenres = await context.Genres.ToDictionaryAsync(g => g.Id);
        var predefinedGenres = PredefinedGenres.All.ToDictionary(g => g.Id);

        // 1. Добавляем новые жанры
        foreach (var kvp in predefinedGenres)
        {
            if (!dbGenres.ContainsKey(kvp.Key))
            {
                await context.Genres.AddAsync(new Genre
                {
                    Id = kvp.Key,
                    Name = kvp.Value.Name,
                    Description = kvp.Value.Description
                });
                //Console.WriteLine($"➕ Добавлен новый жанр: {kvp.Value.Name}");
            }
            else
            {
                // Обновляем существующие
                var existingGenre = dbGenres[kvp.Key];
                if (existingGenre.Name != kvp.Value.Name ||
                    existingGenre.Description != kvp.Value.Description)
                {
                    existingGenre.Name = kvp.Value.Name;
                    existingGenre.Description = kvp.Value.Description;
                    context.Genres.Update(existingGenre);
                    Console.WriteLine($"🔄 Обновлён жанр: {kvp.Value.Name}");
                }
            }
        }

        // 2. Удаляем неиспользуемые жанры
        foreach (var dbGenre in dbGenres.Values)
        {
            if (!predefinedGenres.ContainsKey(dbGenre.Id))
            {
                var isGenreUsed = await context.MovieGenres
                    .AnyAsync(mg => mg.GenreId == dbGenre.Id);

                if (isGenreUsed)
                {
                    Console.WriteLine($"⚠ Жанр '{dbGenre.Name}' (Id={dbGenre.Id}) используется в фильмах и НЕ МОЖЕТ быть удалён");
                }
                else
                {
                    context.Genres.Remove(dbGenre);
                    Console.WriteLine($"➖ Удалён неиспользуемый жанр: {dbGenre.Name} (Id={dbGenre.Id})");
                }
            }
        }

        await context.SaveChangesAsync();
    }

    /*private static async Task InitializeDatabaseAsync(AppDbContext context)
    {
        // 1. Сначала синхронизируем жанры (они не зависят от других таблиц)
        await SyncGenresAsync(context);

        // 2. Проверяем, есть ли уже данные
        if (await context.Directors.AnyAsync() || await context.Actors.AnyAsync())
        {
            Console.WriteLine("✓ В базе уже есть данные, пропускаем инициализацию");
            return;
        }

        // 3. Добавляем режиссеров
        var directors = new List<Director>
        {
            new Director { Id = 1, Name = "Кристофер Нолан", Age = 53, Awards = "Оскар, Золотой глобус, BAFTA" },
            new Director { Id = 2, Name = "Гай Ричи", Age = 55, Awards = "Премия BAFTA" },
            new Director { Id = 3, Name = "Квентин Тарантино", Age = 60, Awards = "Золотая пальмовая ветвь, Оскар" },
            new Director { Id = 4, Name = "Стивен Спилберг", Age = 77, Awards = "Оскар, Золотой глобус, BAFTA" }
        };
        await context.Directors.AddRangeAsync(directors);
        await context.SaveChangesAsync();

        // 4. Добавляем актеров
        var actors = new List<Actor>
        {
            new Actor { Id = 1, Name = "Леонардо ДиКаприо", BirthDate = new DateTime(1974, 11, 11), Country = "США" },
            new Actor { Id = 2, Name = "Мэттью МакКонахи", BirthDate = new DateTime(1969, 11, 4), Country = "США" },
            new Actor { Id = 3, Name = "Джейсон Стэйтем", BirthDate = new DateTime(1967, 7, 26), Country = "Великобритания" },
            new Actor { Id = 4, Name = "Брэд Питт", BirthDate = new DateTime(1963, 12, 18), Country = "США" },
            new Actor { Id = 5, Name = "Том Хэнкс", BirthDate = new DateTime(1956, 7, 9), Country = "США" }
        };
        await context.Actors.AddRangeAsync(actors);
        await context.SaveChangesAsync();

        // 5. Добавляем фильмы (уже есть DirectorId)
        var movies = new List<Movie>
        {
            new Movie { Id = 1, Title = "Интерстеллар", Year = 2014, DurationMinutes = 169, AgeRating = AgeRating.TwelvePlus, Country = "США, Великобритания", DirectorId = 1 },
            new Movie { Id = 2, Title = "Начало", Year = 2010, DurationMinutes = 148, AgeRating = AgeRating.TwelvePlus, Country = "США, Великобритания", DirectorId = 1 },
            new Movie { Id = 3, Title = "Большой куш", Year = 2000, DurationMinutes = 104, AgeRating = AgeRating.SixteenPlus, Country = "Великобритания, США", DirectorId = 2 },
            new Movie { Id = 4, Title = "Джентльмены", Year = 2019, DurationMinutes = 113, AgeRating = AgeRating.EighteenPlus, Country = "Великобритания, США", DirectorId = 2 },
            new Movie { Id = 5, Title = "Криминальное чтиво", Year = 1994, DurationMinutes = 154, AgeRating = AgeRating.EighteenPlus, Country = "США", DirectorId = 3 },
            new Movie { Id = 6, Title = "Однажды в Голливуде", Year = 2019, DurationMinutes = 161, AgeRating = AgeRating.SixteenPlus, Country = "США, Великобритания", DirectorId = 3 },
            new Movie { Id = 7, Title = "Спасти рядового Райана", Year = 1998, DurationMinutes = 169, AgeRating = AgeRating.SixteenPlus, Country = "США", DirectorId = 4 }
        };
        await context.Movies.AddRangeAsync(movies);
        await context.SaveChangesAsync();

        // 6. Добавляем роли (уже есть MovieId и ActorId)
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
        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();

        // 7. Добавляем связи с жанрами (жанры уже есть после SyncGenresAsync)
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
        await context.MovieGenres.AddRangeAsync(movieGenres);
        await context.SaveChangesAsync();
    }*/

    private static async Task ShowMoviesByGenre()
    {
        using var context = new AppDbContext();

        var genres = await context.Genres.OrderBy(g => g.Name).ToListAsync();
        if (!genres.Any())
        {
            Console.WriteLine("\n📢 Жанры не найдены.");
            return;
        }

        Console.WriteLine("\n📚 Доступные жанры:");
        foreach (var genre in genres)
        {
            Console.WriteLine($"   [{genre.Id}] {genre.Name}");
        }

        Console.Write("\nВведите ID жанра: ");
        if (!int.TryParse(Console.ReadLine(), out int genreId))
        {
            Console.WriteLine("❌ Неверный ID\n");
            return;
        }

        var selectedGenre = await context.Genres
            .Include(g => g.MovieGenres)
                .ThenInclude(mg => mg.Movie)
                    .ThenInclude(m => m.Director)
            .Include(g => g.MovieGenres)
                .ThenInclude(mg => mg.Movie)
                    .ThenInclude(m => m.Roles)
                        .ThenInclude(r => r.Actor)
            .Include(g => g.MovieGenres)
                .ThenInclude(mg => mg.Movie)
                    .ThenInclude(m => m.MovieGenres)
                        .ThenInclude(mg => mg.Genre)
            .FirstOrDefaultAsync(g => g.Id == genreId);

        if (selectedGenre == null)
        {
            Console.WriteLine("❌ Жанр не найден\n");
            return;
        }

        var movies = selectedGenre.MovieGenres?
            .Where(mg => mg.Movie != null)
            .Select(mg => mg.Movie)
            .OrderBy(m => m.Year)
            .ToList();

        if (movies == null || !movies.Any())
        {
            Console.WriteLine($"\n📢 В жанре '{selectedGenre.Name}' пока нет фильмов.\n");
            return;
        }

        Console.WriteLine($"\n{"=".PadRight(100, '=')}");
        Console.WriteLine($"{"ФИЛЬМЫ В ЖАНРЕ: " + selectedGenre.Name.ToUpper(),63}");
        Console.WriteLine($"{"=".PadRight(100, '=')}");

        foreach (var movie in movies)
        {
            Console.WriteLine($"\n   [{movie.Id}] {movie.Title} ({movie.Year})");
            Console.WriteLine($"   🌍 Страна: {movie.Country}");
            Console.WriteLine($"   ⏱  Длительность: {movie.FormattedDuration}");
            Console.WriteLine($"   🚫 Возрастное ограничение: {movie.AgeRating.GetDisplayName()}");
            Console.WriteLine($"   🎬 Режиссер: {movie.Director?.Name ?? "Неизвестен"}");

            if (movie.MovieGenres?.Any() == true)
            {
                var allGenres = movie.MovieGenres
                    .Where(mg => mg.Genre != null)
                    .Select(mg => mg.Genre.Name);
                Console.WriteLine($"   📚 Жанр: {string.Join(", ", allGenres)}");
            }

            if (!string.IsNullOrEmpty(movie.Description))
            {
                Console.WriteLine($"   📝 Описание:");
                PrintWrappedText(movie.Description, 8, 80);
            }

            if (movie.Roles?.Any() == true)
            {
                Console.WriteLine($"   🎭 В ролях ({movie.Roles.Count}):");
                foreach (var role in movie.Roles.OrderBy(r => r.Actor?.Name))
                {
                    Console.WriteLine($"        • {role.Actor?.Name} — {role.CharacterName}");
                }
            }
            Console.WriteLine($"{"-".PadRight(100, '-')}");
        }

        Console.WriteLine($"\n   Всего фильмов в жанре: {movies.Count}");
    }

    // ==================== МЕТОДЫ ДОБАВЛЕНИЯ ФИЛЬМА ====================

    private static async Task AddNewMovie()
    {
        Console.WriteLine("\n" + "=".PadRight(62, '='));
        Console.WriteLine("ДОБАВЛЕНИЕ НОВОГО ФИЛЬМА".PadLeft(43));
        Console.WriteLine("=".PadRight(62, '='));

        // Сброс временных данных
        ResetTempData();

        var movie = new Movie();
        var addMoreActors = true;
        var addMoreGenres = true;

        while (true)
        {
            //Console.WriteLine($"\n{"-".PadRight(50, '-')}");
            Console.WriteLine("ЗАПОЛНЕНИЕ ИНФОРМАЦИИ О ФИЛЬМЕ:");
            //Console.WriteLine($"{"-".PadRight(50, '-')}");

            if (!_movieInfoFilled) Console.WriteLine("1. Заполнить основную информацию");
            if (_tempDirector == null) Console.WriteLine("2. Заполнить поле 'Режиссер'");
            if (!_ageRatingFilled) Console.WriteLine("3. Заполнить поле 'Возрастное ограничение'");
            if (!_countryFilled) Console.WriteLine("4. Заполнить поле 'Страна производства'");
            if (addMoreGenres) Console.WriteLine("5. Заполнить поле 'Жанр фильма'");
            if (addMoreActors) Console.WriteLine("6. Заполнить поле 'Актеры'");
            if (!_descriptionFilled) Console.WriteLine("7. Заполнить поле 'Описание'");
            Console.WriteLine("8. Завершить и подтвердить добавление фильма");
            Console.WriteLine("9. Отмена (вернуться в главное меню)");

            Console.Write("\nВыберите пункт: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    if (!_movieInfoFilled)
                        await FillMovieInfo(movie);
                    else
                        Console.WriteLine("✅ Информация о фильме уже заполнена\n");
                    break;

                case "2":
                    if (_tempDirector == null)
                        await SelectOrAddDirector();
                    else
                        Console.WriteLine($"✅ Режиссер уже выбран: {_tempDirector.Name}\n");
                    break;

                case "3":
                    if (!_ageRatingFilled)
                        await SelectAgeRating(movie);
                    else
                        Console.WriteLine($"✅ Возрастное ограничение уже выбрано: {movie.AgeRating.GetDisplayName()}\n");
                    break;

                case "4":
                    if (!_countryFilled)
                        await FillCountry(movie);
                    else
                        Console.WriteLine($"✅ Страна уже указана: {movie.Country}\n");
                    break;

                case "5":
                    if (addMoreGenres)
                        addMoreGenres = await SelectGenres();
                    else
                        Console.WriteLine("✅ Жанры уже добавлены\n");
                    break;

                case "6":
                    if (addMoreActors)
                        addMoreActors = await SelectActorsAndRoles();
                    else
                        Console.WriteLine("✅ Актеры уже добавлены\n");
                    break;

                case "7":
                    if (!_descriptionFilled)
                        await FillDescription(movie);
                    else
                        Console.WriteLine("✅ Описание уже заполнено\n");
                    break;

                case "8":
                    if (await ConfirmAndSaveMovie(movie))
                    {
                        Console.WriteLine("\n✅ Фильм успешно добавлен в базу данных!\n");
                        return;
                    }
                    break;

                case "9":
                    Console.WriteLine("Операция отменена.\n");
                    return;

                default:
                    Console.WriteLine("❌ Неверный пункт меню\n");
                    break;
            }
        }
    }

    private static void ResetTempData()
    {
        _tempActor = null;
        _tempDirector = null;
        _selectedActors.Clear();
        _selectedRoles.Clear();
        _selectedGenres.Clear();
        _directorConfirmed = false;
        _movieInfoFilled = false;
        _ageRatingFilled = false;
        _countryFilled = false;
        _descriptionFilled = false;
    }

    private static async Task FillMovieInfo(Movie movie)
    {
        Console.WriteLine("\n----- ЗАПОЛНЕНИЕ ИНФОРМАЦИИ О ФИЛЬМЕ -----");

        Console.Write("Введите название фильма: ");
        var title = Console.ReadLine()?.Trim();
        while (string.IsNullOrWhiteSpace(title))
        {
            Console.Write("❌ Название не может быть пустым. Введите название: ");
            title = Console.ReadLine()?.Trim();
        }
        movie.Title = title;

        Console.Write("Введите год выпуска: ");
        int year;
        while (!int.TryParse(Console.ReadLine(), out year) || year < 1890 || year > 2050)
        {
            Console.Write("❌ Неверный год. Введите год выпуска: ");
        }
        movie.Year = year;

        Console.Write("Введите длительность (в минутах): ");
        int duration;
        while (!int.TryParse(Console.ReadLine(), out duration) || duration <= 0)
        {
            Console.Write("❌ Длительность фильма должна быть положительна. Введите длительность (в минутах): ");
        }
        movie.DurationMinutes = duration;

        /*if (hours == 0 && minutes == 0)
        {
            Console.WriteLine("❌ Длительность должна быть больше 0. Установлено значение по умолчанию: 90 мин");
            movie.DurationMinutes = 90;
        }
        else
        {
            movie.DurationMinutes = hours * 60 + minutes;
        }*/

        _movieInfoFilled = true;
        Console.WriteLine("✅ Основная информация заполнена\n");
    }

    private static async Task SelectOrAddDirector()
    {
        using var context = new AppDbContext();

        Console.WriteLine("\n----- ДОБАВЛЕНИЕ ИНФОРМАЦИИ О РЕЖИССЕРЕ -----");

        // Показываем существующих режиссеров
        var directors = await context.Directors.OrderBy(d => d.Name).ToListAsync();
        if (directors.Any())
        {
            Console.WriteLine("\nСуществующие режиссеры:");
            foreach (var d in directors)
            {
                Console.WriteLine($"   [{d.Id}] {d.Name} ({d.Age} лет)");
                if (!string.IsNullOrEmpty(d.Awards))
                    Console.WriteLine($"        Награды: {d.Awards}");
            }
        }

        Console.WriteLine("\nВыберите действие:");
        Console.WriteLine("1. Выбрать из существующих");
        Console.WriteLine("2. Добавить нового");
        Console.Write("Ваш выбор: ");

        var choice = Console.ReadLine();

        if (choice == "1")
        {
            Console.Write("Введите ID режиссера: ");
            if (int.TryParse(Console.ReadLine(), out int directorId))
            {
                var director = await context.Directors.FindAsync(directorId);
                if (director != null)
                {
                    _tempDirector = director;
                    Console.WriteLine($"✅ Выбран режиссер: {director.Name}\n");
                    return;
                }
            }
            Console.WriteLine("❌ Режиссер не найден\n");
        }
        else if (choice == "2")
        {
            await AddNewDirector();
        }
    }

    private static async Task AddNewDirector()
    {
        var director = new Director();

        Console.WriteLine("\n----- ДОБАВЛЕНИЕ НОВОГО РЕЖИССЕРА -----");

        Console.Write("Введите ФИО режиссера: ");
        var name = Console.ReadLine()?.Trim();
        while (string.IsNullOrWhiteSpace(name))
        {
            Console.Write("❌ ФИО не может быть пустым. Введите ФИО режиссера: ");
            name = Console.ReadLine()?.Trim();
        }
        director.Name = name;

        Console.Write("Введите возраст: ");
        int age;
        while (!int.TryParse(Console.ReadLine(), out age) || age < 0)
        {
            Console.Write("❌ Неверный возраст. Введите возраст: ");
        }
        director.Age = age;

        Console.Write("Введите награды (можно пропустить, Enter для пропуска): ");
        var awards = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(awards))
        {
            director.Awards = awards;
        }

        using var context = new AppDbContext();

        // Проверяем, есть ли уже такой режиссер
        var existingDirector = await context.Directors
            .FirstOrDefaultAsync(d => d.Name == director.Name && d.Age == director.Age);

        if (existingDirector != null)
        {
            Console.WriteLine($"\nНайден существующий режиссер:");
            Console.WriteLine($"   Имя: {existingDirector.Name}");
            Console.WriteLine($"   Возраст: {existingDirector.Age}");
            if (!string.IsNullOrEmpty(existingDirector.Awards))
                Console.WriteLine($"   Награды: {existingDirector.Awards}");

            Console.Write("\nИспользовать этого режиссера? (д/н): ");
            if (Console.ReadLine()?.ToLower() == "д")
            {
                _tempDirector = existingDirector;
                Console.WriteLine("✅ Режиссер выбран\n");
                return;
            }
        }

        await context.Directors.AddAsync(director);
        await context.SaveChangesAsync();
        _tempDirector = director;
        Console.WriteLine($"✅ Новый режиссер добавлен с ID {director.Id}\n");
    }

    private static async Task SelectAgeRating(Movie movie)
    {
        Console.WriteLine("\n----- ВЫБОР ВОЗРАСТНОГО ОГРАНИЧЕНИЯ -----");

        var ratings = Enum.GetValues(typeof(AgeRating)).Cast<AgeRating>().ToList();
        foreach (var rating in ratings)
        {
            Console.WriteLine($"   [{(int)rating}] {rating.GetDisplayName()}");
        }

        Console.Write("Выберите номер возрастного ограничения: ");
        if (int.TryParse(Console.ReadLine(), out int ratingValue) &&
            Enum.IsDefined(typeof(AgeRating), ratingValue))
        {
            movie.AgeRating = (AgeRating)ratingValue;
            _ageRatingFilled = true;
            Console.WriteLine($"✅ Выбрано: {movie.AgeRating.GetDisplayName()}\n");
        }
        else
        {
            Console.WriteLine("❌ Неверный выбор. Установлено значение по умолчанию: 12+\n");
            movie.AgeRating = AgeRating.TwelvePlus;
            _ageRatingFilled = true;
        }
    }

    private static async Task FillCountry(Movie movie)
    {
        Console.WriteLine("\n----- ЗАПОЛНЕНИЕ СТРАНЫ ПРОИЗВОДСТВА -----");

        Console.Write("Введите страну производства (например: США, Великобритания): ");
        var country = Console.ReadLine()?.Trim();

        while (string.IsNullOrWhiteSpace(country))
        {
            Console.Write("❌ Страна не может быть пустой. Введите страну производства: ");
            country = Console.ReadLine()?.Trim();
        }

        movie.Country = country;
        _countryFilled = true;
        Console.WriteLine($"✅ Страна указана: {movie.Country}\n");
    }

    private static async Task<bool> SelectGenres()
    {
        using var context = new AppDbContext();
        var genres = await context.Genres.OrderBy(g => g.Name).ToListAsync();

        Console.WriteLine("\n----- ВЫБОР ЖАНРОВ ФИЛЬМА -----");
        Console.WriteLine("Доступные жанры:");

        // Показываем уже выбранные жанры
        if (_selectedGenres.Any())
        {
            Console.WriteLine($"\n✅ Уже выбрано: {string.Join(", ", _selectedGenres.Select(g => g.Name))}");
            Console.WriteLine();
        }

        // Показываем все доступные жанры
        foreach (var genre in genres)
        {
            string selected = _selectedGenres.Any(g => g.Id == genre.Id) ? "✓ " : "  ";
            Console.WriteLine($"   {selected}[{genre.Id}] {genre.Name} - {genre.Description ?? "Нет описания"}");
        }

        Console.WriteLine("\nВыберите действие:");
        Console.WriteLine("1. Добавить жанр (введите ID)");
        Console.WriteLine("2. Убрать жанр (введите ID)");
        Console.WriteLine("3. Закончить выбор жанров");
        Console.Write("Ваш выбор: ");

        var choice = Console.ReadLine();

        if (choice == "1")
        {
            Console.Write("Введите ID жанра для добавления: ");
            if (int.TryParse(Console.ReadLine(), out int genreId))
            {
                var genre = genres.FirstOrDefault(g => g.Id == genreId);
                if (genre != null)
                {
                    if (!_selectedGenres.Any(g => g.Id == genreId))
                    {
                        _selectedGenres.Add(genre);
                        Console.WriteLine($"✅ Жанр '{genre.Name}' добавлен\n");
                    }
                    else
                    {
                        Console.WriteLine("❌ Этот жанр уже выбран\n");
                    }
                }
                else
                {
                    Console.WriteLine("❌ Жанр не найден\n");
                }
            }
            return true;
        }
        else if (choice == "2")
        {
            Console.Write("Введите ID жанра для удаления: ");
            if (int.TryParse(Console.ReadLine(), out int genreId))
            {
                var genre = _selectedGenres.FirstOrDefault(g => g.Id == genreId);
                if (genre != null)
                {
                    _selectedGenres.Remove(genre);
                    Console.WriteLine($"✅ Жанр '{genre.Name}' удален из выбранных\n");
                }
                else
                {
                    Console.WriteLine("❌ Этот жанр не был выбран\n");
                }
            }
            return true;
        }

        // Проверяем, выбран ли хотя бы один жанр
        if (_selectedGenres.Count == 0)
        {
            Console.WriteLine("❌ Нужно выбрать хотя бы один жанр\n");
            return true;
        }

        Console.WriteLine($"\n✅ Выбрано жанров: {_selectedGenres.Count}\n");
        return false;
    }

    private static async Task<bool> SelectActorsAndRoles()
    {
        Console.WriteLine("\n----- ДОБАВЛЕНИЕ АКТЕРОВ И РОЛЕЙ -----");

        Console.WriteLine("Выберите действие:");
        Console.WriteLine("1. Добавить актера и роль");
        Console.WriteLine("2. Закончить добавление актеров");
        Console.Write("Ваш выбор: ");

        var choice = Console.ReadLine();

        if (choice == "1")
        {
            await AddActorAndRole();
            return true; // Продолжаем добавление
        }

        if (_selectedActors.Count == 0)
        {
            Console.WriteLine("❌ Нужно добавить хотя бы одного актера\n");
            return true;
        }

        Console.WriteLine($"\n✅ Добавлено актеров: {_selectedActors.Count}\n");
        return false; // Заканчиваем добавление
    }

    private static async Task AddActorAndRole()
    {
        using var context = new AppDbContext();

        Console.WriteLine("\n----- ДОБАВЛЕНИЕ АКТЕРА -----");

        // Показываем существующих актеров
        var actors = await context.Actors.OrderBy(a => a.Name).ToListAsync();
        if (actors.Any())
        {
            Console.WriteLine("\nСуществующие актеры:");
            foreach (var a in actors)
            {
                Console.WriteLine($"   [{a.Id}] {a.Name} ({a.Country}), {a.Age} лет");
            }
            /*if (actors.Count > 5)
                Console.WriteLine($"   ... и ещё {actors.Count - 5} актеров");*/
        }

        Console.WriteLine("\nВыберите действие:");
        Console.WriteLine("1. Выбрать из существующих");
        Console.WriteLine("2. Добавить нового");
        Console.Write("Ваш выбор: ");

        Actor? selectedActor = null;

        var choice = Console.ReadLine();

        if (choice == "1")
        {
            Console.Write("Введите ID актера: ");
            if (int.TryParse(Console.ReadLine(), out int actorId))
            {
                selectedActor = await context.Actors.FindAsync(actorId);
                if (selectedActor == null)
                {
                    Console.WriteLine("❌ Актер не найден\n");
                    return;
                }
            }
        }
        else if (choice == "2")
        {
            selectedActor = await AddNewActor();
        }
        else
        {
            return;
        }

        if (selectedActor == null) return;

        // Добавляем роль для актера
        Console.WriteLine($"\n----- ДОБАВЛЕНИЕ РОЛИ ДЛЯ {selectedActor.Name.ToUpper()} -----");

        Console.Write("Введите имя персонажа: ");
        var characterName = Console.ReadLine()?.Trim();

        while (string.IsNullOrWhiteSpace(characterName))
        {
            Console.Write("❌ Имя персонажа не может быть пустым. Введите имя: ");
            characterName = Console.ReadLine()?.Trim();
        }

        // Проверяем, не добавлен ли уже этот актер
        if (_selectedActors.Any(a => a.Id == selectedActor.Id))
        {
            Console.WriteLine("❌ Этот актер уже добавлен в фильм\n");
            return;
        }

        _selectedActors.Add(selectedActor);

        var role = new Role
        {
            Actor = selectedActor,
            ActorId = selectedActor.Id,
            CharacterName = characterName
        };
        _selectedRoles.Add(role);

        Console.WriteLine($"✅ Актер {selectedActor.Name} добавлен в роли {characterName}\n");
    }

    private static async Task<Actor?> AddNewActor()
    {
        var actor = new Actor();

        Console.WriteLine("\n----- ДОБАВЛЕНИЕ НОВОГО АКТЕРА -----");

        Console.Write("Введите ФИО актера: ");
        var name = Console.ReadLine()?.Trim();
        while (string.IsNullOrWhiteSpace(name))
        {
            Console.Write("❌ Имя не может быть пустым. Введите ФИО: ");
            name = Console.ReadLine()?.Trim();
        }
        actor.Name = name;

        Console.Write("Введите дату рождения (гггг-мм-дд): ");
        DateTime birthDate;
        while (!DateTime.TryParse(Console.ReadLine(), out birthDate) || birthDate > DateTime.Now)
        {
            Console.Write("❌ Неверная дата. Введите дату рождения (гггг-мм-дд): ");
        }
        actor.BirthDate = birthDate;

        Console.Write("Введите страну: ");
        var country = Console.ReadLine()?.Trim();
        while (string.IsNullOrWhiteSpace(country))
        {
            Console.Write("❌ Страна не может быть пустой. Введите страну: ");
            country = Console.ReadLine()?.Trim();
        }
        actor.Country = country;

        using var context = new AppDbContext();

        // Проверяем, есть ли уже такой актер
        var existingActor = await context.Actors
            .FirstOrDefaultAsync(a => a.Name == actor.Name &&
                                     a.BirthDate == actor.BirthDate &&
                                     a.Country == actor.Country);

        if (existingActor != null)
        {
            Console.WriteLine($"\nНайден существующий актер:");
            Console.WriteLine($"   Имя: {existingActor.Name}");
            Console.WriteLine($"   Дата рождения: {existingActor.BirthDate:dd.MM.yyyy}");
            Console.WriteLine($"   Страна: {existingActor.Country}");
            Console.WriteLine($"   Возраст: {existingActor.Age} лет");

            Console.Write("\nИспользовать этого актера? (д/н): ");
            if (Console.ReadLine()?.ToLower() == "д")
            {
                return existingActor;
            }
        }

        await context.Actors.AddAsync(actor);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ Новый актер добавлен с ID {actor.Id}\n");
        return actor;
    }

    private static async Task FillDescription(Movie movie)
    {
        Console.WriteLine("\n----- ЗАПОЛНЕНИЕ ОПИСАНИЯ ФИЛЬМА -----");
        Console.WriteLine("(Введите описание, для завершения введите пустую строку)");

        var descriptionLines = new List<string>();
        string? line;

        while (!string.IsNullOrEmpty(line = Console.ReadLine()))
        {
            descriptionLines.Add(line);
        }

        if (descriptionLines.Any())
        {
            movie.Description = string.Join("\n", descriptionLines);
            _descriptionFilled = true;
            Console.WriteLine("✅ Описание добавлено\n");
        }
        else
        {
            Console.Write("Описание не добавлено. Продолжить? (д/н): ");
            if (Console.ReadLine()?.ToLower() == "д")
            {
                _descriptionFilled = true;
            }
        }
    }

    private static async Task<bool> ConfirmAndSaveMovie(Movie movie)
    {
        // Проверяем, все ли обязательные поля заполнены
        if (string.IsNullOrWhiteSpace(movie.Title) || movie.Year == 0 ||
            movie.DurationMinutes == 0 || _tempDirector == null ||
            string.IsNullOrWhiteSpace(movie.Country) || _selectedGenres.Count == 0 ||
            _selectedActors.Count == 0)
        {
            Console.WriteLine("\n❌ Не все обязательные поля заполнены!");
            Console.WriteLine($"   Название: {(string.IsNullOrWhiteSpace(movie.Title) ? "не заполнено" : "✓")}");
            Console.WriteLine($"   Год: {(movie.Year == 0 ? "не заполнен" : "✓")}");
            Console.WriteLine($"   Длительность: {(movie.DurationMinutes == 0 ? "не заполнена" : "✓")}");
            Console.WriteLine($"   Режиссер: {(_tempDirector == null ? "не выбран" : "✓")}");
            Console.WriteLine($"   Страна: {(string.IsNullOrWhiteSpace(movie.Country) ? "не заполнена" : "✓")}");
            Console.WriteLine($"   Жанр: {(_selectedGenres.Count == 0 ? "не выбраны" : "✓")}");
            Console.WriteLine($"   Актеры: {(_selectedActors.Count == 0 ? "не добавлены" : "✓")}");
            return false;
        }

        Console.WriteLine("\n" + "=".PadRight(99, '='));
        Console.WriteLine("ПОДТВЕРЖДЕНИЕ ДОБАВЛЕНИЯ ФИЛЬМА".PadLeft(65));
        Console.WriteLine("=".PadRight(99, '='));

        Console.WriteLine($"\n🎬 НАЗВАНИЕ: {movie.Title}");
        Console.WriteLine($"🗓  ГОД ВЫПУСКА: {movie.Year}");
        Console.WriteLine($"🌍 СТРАНА: {movie.Country}");
        Console.WriteLine($"⏱  ДЛИТЕЛЬНОСТЬ: {movie.FormattedDuration}");
        Console.WriteLine($"🚫 ВОЗРАСТНОЕ ОГРАНИЧЕНИЕ: {movie.AgeRating.GetDisplayName()}");

        Console.WriteLine($"\n🎬 РЕЖИССЕР:");
        Console.WriteLine($"   {_tempDirector.Name} ({_tempDirector.Age} лет)");
        /*if (!string.IsNullOrEmpty(_tempDirector.Awards))
            Console.WriteLine($"   Награды: {_tempDirector.Awards}");*/

        Console.WriteLine($"\n📚 ЖАНРЫ:");
        foreach (var genre in _selectedGenres)
        {
            Console.WriteLine($"   • {genre.Name}");
        }

        if (!string.IsNullOrEmpty(movie.Description))
        {
            Console.WriteLine($"\n📝 ОПИСАНИЕ:");
            PrintWrappedText(movie.Description, 3, 80);
        }

        Console.WriteLine($"\n🎭 АКТЕРЫ И РОЛИ ({_selectedActors.Count}):");
        foreach (var role in _selectedRoles)
        {
            Console.WriteLine($"   • {role.Actor?.Name} — {role.CharacterName}");
            /*if (role.Actor != null)
            {
                Console.WriteLine($"     ({role.Actor.Country}, {role.Actor.Age} лет)");
            }*/
        }

        Console.WriteLine("\n" + "=".PadRight(99, '='));
        Console.Write("\nСохранить фильм в базу данных? (д/н): ");

        if (Console.ReadLine()?.ToLower() != "д")
        {
            Console.WriteLine("❌ Сохранение отменено\n");
            return false;
        }

        using var context = new AppDbContext();

        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // 1. Добавляем фильм
            movie.DirectorId = _tempDirector.Id;
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync(); // Сохраняем, чтобы получить ID фильма

            // 2. Получаем максимальный существующий Id для MovieGenre
            int maxMovieGenreId = 0;
            if (await context.MovieGenres.AnyAsync())
            {
                maxMovieGenreId = await context.MovieGenres.MaxAsync(mg => mg.Id);
            }

            // 3. Добавляем связи с жанрами (с явным указанием Id)
            int nextId = maxMovieGenreId + 1;
            foreach (var genre in _selectedGenres)
            {
                // Проверяем, не существует ли уже такая связь
                bool exists = await context.MovieGenres
                    .AnyAsync(mg => mg.MovieId == movie.Id && mg.GenreId == genre.Id);

                if (!exists)
                {
                    var movieGenre = new MovieGenre
                    {
                        Id = nextId++, // Явно указываем Id
                        MovieId = movie.Id,
                        GenreId = genre.Id
                    };
                    await context.MovieGenres.AddAsync(movieGenre);
                }
            }

            // 4. Получаем максимальный существующий Id для Role
            int maxRoleId = 0;
            if (await context.Roles.AnyAsync())
            {
                maxRoleId = await context.Roles.MaxAsync(r => r.Id);
            }

            // 5. Добавляем роли (с явным указанием Id)
            nextId = maxRoleId + 1;
            foreach (var role in _selectedRoles)
            {
                // Проверяем, не существует ли уже такая роль
                bool exists = await context.Roles
                    .AnyAsync(r => r.MovieId == movie.Id && r.ActorId == role.ActorId);

                if (!exists)
                {
                    var newRole = new Role
                    {
                        Id = nextId++, // Явно указываем Id
                        CharacterName = role.CharacterName,
                        MovieId = movie.Id,
                        ActorId = role.ActorId
                    };
                    await context.Roles.AddAsync(newRole);
                }
            }

            // 6. Сохраняем все связи
            await context.SaveChangesAsync();

            // 7. ПОДТВЕРЖДАЕМ ТРАНЗАКЦИЮ
            await transaction.CommitAsync();

            Console.WriteLine("\n✅ Фильм успешно добавлен в базу данных!\n");
            return true;
        }
        catch (Exception ex)
        {
            // ОТКАТ ТРАНЗАКЦИИ В СЛУЧАЕ ОШИБКИ
            await transaction.RollbackAsync();

            Console.WriteLine($"\n❌ Ошибка при сохранении: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Подробности: {ex.InnerException.Message}\n");
            }
            return false;
        }
    }

    // ==================== МЕТОДЫ РЕДАКТИРОВАНИЯ ====================
    private static async Task EditMovie()
    {
        using var context = new AppDbContext();

        Console.WriteLine("\n" + "=".PadRight(60, '='));
        Console.WriteLine("РЕДАКТИРОВАНИЕ ФИЛЬМА".PadLeft(35));
        Console.WriteLine("=".PadRight(60, '='));

        // Показываем список фильмов
        var movies = await context.Movies
            .Include(m => m.Director)
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .OrderBy(m => m.Title)
            .ToListAsync();

        if (!movies.Any())
        {
            Console.WriteLine("\n📢 Фильмы не найдены.");
            return;
        }

        Console.WriteLine("\nДоступные фильмы:");
        foreach (var m in movies)
        {
            //Console.WriteLine($"   [{m.Id}] {m.Title} ({m.Year}) - {m.Director?.Name ?? "Неизвестен"}");
            string genres = m.MovieGenres != null && m.MovieGenres.Any()
                ? $" - Жанры: {string.Join(", ", m.MovieGenres.Select(mg => mg.Genre?.Name))}"
                : "";
            Console.WriteLine($"   [{m.Id}] {m.Title} ({m.Year}), {genres} - {m.Director?.Name ?? "Неизвестен"}");
        }

        Console.Write("\nВведите ID фильма для редактирования: ");
        if (!int.TryParse(Console.ReadLine(), out int movieId))
        {
            Console.WriteLine("❌ Неверный ID");
            return;
        }

        var movie = await context.Movies
            .Include(m => m.Director)
            .Include(m => m.Roles)
                .ThenInclude(r => r.Actor)
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .FirstOrDefaultAsync(m => m.Id == movieId);

        if (movie == null)
        {
            Console.WriteLine($"❌ Фильм с ID {movieId} не найден");
            return;
        }

        /*Console.WriteLine($"\nРедактирование фильма: {movie.Title} ({movie.Year})");
        Console.WriteLine("(Оставьте поле пустым, чтобы оставить без изменений)");

        // Название
        Console.Write($"\nИзмененное название [{movie.Title}]: ");
        var title = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(title))
        {
            movie.Title = title;
        }

        // Год
        Console.Write($"Измененный год [{movie.Year}]: ");
        var yearStr = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(yearStr) && int.TryParse(yearStr, out int year))
        {
            movie.Year = year;
        }

        // Страна
        Console.Write($"Измененная страна [{movie.Country}]: ");
        var country = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(country))
        {
            movie.Country = country;
        }

        // Длительность
        Console.WriteLine($"\nТекущая длительность: {movie.FormattedDuration}");
        Console.Write("Измененная длительность в минутах: ");
        var durationStr = Console.ReadLine()?.Trim();

        if (!string.IsNullOrEmpty(durationStr) && int.TryParse(durationStr, out int newDuration))
        {
            if (newDuration > 0)
            {
                movie.DurationMinutes = newDuration;
                Console.WriteLine($"   Новая длительность: {movie.FormattedDuration}");
            }
            else
            {
                Console.WriteLine("❌ Длительность должна быть положительна, значение не изменено");
            }
        }

        // Возрастное ограничение
        Console.WriteLine($"\nТекущее возрастное ограничение: {movie.AgeRating.GetDisplayName()}");
        Console.WriteLine("Доступные варианты:");
        foreach (AgeRating rating in Enum.GetValues(typeof(AgeRating)))
        {
            Console.WriteLine($"   [{(int)rating}] {rating.GetDisplayName()}");
        }
        Console.Write("Введите номер измененного возрастного ограничения: ");
        var ratingStr = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(ratingStr) && int.TryParse(ratingStr, out int ratingValue) &&
            Enum.IsDefined(typeof(AgeRating), ratingValue))
        {
            movie.AgeRating = (AgeRating)ratingValue;
        }

        // Описание
        Console.WriteLine($"\nТекущее описание:");
        if (!string.IsNullOrEmpty(movie.Description))
        {
            PrintWrappedText(movie.Description, 3, 80);
        }
        else
        {
            Console.WriteLine("   Описание отсутствует");
        }
        Console.WriteLine("\nВведите новое описание (Enter чтобы оставить, 'DELETE' чтобы удалить):");
        var description = Console.ReadLine();
        if (description == "DELETE")
        {
            movie.Description = null;
        }
        else if (!string.IsNullOrEmpty(description))
        {
            movie.Description = description;
        }

        await context.SaveChangesAsync();
        Console.WriteLine("\n✅ Информация о фильме успешно обновлена");*/

        bool exitEdit = false;
        while (!exitEdit)
        {
            // Показываем текущую информацию о фильме
            Console.WriteLine($"\n{"-".PadRight(70, '-')}");
            Console.WriteLine($"ТЕКУЩАЯ ИНФОРМАЦИЯ О ФИЛЬМЕ:");
            Console.WriteLine($"{"-".PadRight(70, '-')}");
            Console.WriteLine($"   Название: {movie.Title}");
            Console.WriteLine($"   Год: {movie.Year}");
            Console.WriteLine($"   Страна: {movie.Country}");
            Console.WriteLine($"   Длительность: {movie.FormattedDuration}");
            Console.WriteLine($"   Возрастное ограничение: {movie.AgeRating.GetDisplayName()}");
            Console.WriteLine($"   Режиссер: {movie.Director?.Name ?? "Не выбран"}");

            // Текущие жанры
            if (movie.MovieGenres != null && movie.MovieGenres.Any())
            {
                var currentGenres = movie.MovieGenres.Select(mg => mg.Genre?.Name);
                Console.WriteLine($"   Жанры: {string.Join(", ", currentGenres)}");
            }
            else
            {
                Console.WriteLine($"   Жанры: не выбраны");
            }

            if (!string.IsNullOrEmpty(movie.Description))
            {
                Console.WriteLine($"\n   Описание:");
                PrintWrappedText(movie.Description, 6, 70);
            }

            if (movie.Roles != null && movie.Roles.Any())
            {
                Console.WriteLine($"\n   Актеры ({movie.Roles.Count}):");
                foreach (var role in movie.Roles.OrderBy(r => r.Actor?.Name))
                {
                    Console.WriteLine($"      • {role.Actor?.Name} — {role.CharacterName}");
                }
            }
            else
            {
                Console.WriteLine($"\n   Актеры не добавлены");
            }

            Console.WriteLine($"\n{"-".PadRight(70, '-')}");
            Console.WriteLine("ВЫБЕРИТЕ ДЕЙСТВИЕ:");
            Console.WriteLine("   1. Редактировать название");
            Console.WriteLine("   2. Редактировать год выпуска");
            Console.WriteLine("   3. Редактировать страну");
            Console.WriteLine("   4. Редактировать длительность");
            Console.WriteLine("   5. Редактировать возрастное ограничение");
            Console.WriteLine("   6. Редактировать режиссера");
            Console.WriteLine("   7. Редактировать жанры");
            Console.WriteLine("   8. Редактировать описание");
            Console.WriteLine("   9. Редактировать актеров и роли");
            Console.WriteLine("   0. Завершить редактирование");
            Console.Write("Ваш выбор: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await EditMovieTitle(movie);
                    await context.SaveChangesAsync();
                    break;
                case "2":
                    await EditMovieYear(movie);
                    await context.SaveChangesAsync();
                    break;
                case "3":
                    await EditMovieCountry(movie);
                    await context.SaveChangesAsync();
                    break;
                case "4":
                    await EditMovieDuration(movie);
                    await context.SaveChangesAsync();
                    break;
                case "5":
                    await EditMovieAgeRating(movie);
                    await context.SaveChangesAsync();
                    break;
                case "6":
                    await EditMovieDirector(movie, context);
                    break;
                case "7":
                    await EditMovieGenres(movie, context);
                    break;
                case "8":
                    await EditMovieDescription(movie);
                    await context.SaveChangesAsync();
                    break;
                case "9":
                    await ManageMovieActorsAndRoles(movie, context);
                    break;
                case "0":
                    exitEdit = true;
                    break;
                default:
                    Console.WriteLine("❌ Неверный выбор");
                    break;
            }

            // Сохраняем изменения после каждого действия
            if (choice != "0" && choice != "6" && choice != "7")
            {
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Изменения сохранены\n");
            }
        }
    }

    private static async Task EditDirector()
    {
        using var context = new AppDbContext();

        Console.WriteLine("\n" + "=".PadRight(60, '='));
        Console.WriteLine("РЕДАКТИРОВАНИЕ РЕЖИССЕРА".PadLeft(35));
        Console.WriteLine("=".PadRight(60, '='));

        var directors = await context.Directors
            .Include(d => d.Movies)
            .OrderBy(d => d.Name)
            .ToListAsync();

        if (!directors.Any())
        {
            Console.WriteLine("\n📢 Режиссеры не найдены.");
            return;
        }

        Console.WriteLine("\nДоступные режиссеры:");
        foreach (var d in directors)
        {
            Console.WriteLine($"   [{d.Id}] {d.Name} ({d.Age} лет) - фильмов: {d.Movies?.Count ?? 0}");
        }

        Console.Write("\nВведите ID режиссера для редактирования: ");
        if (!int.TryParse(Console.ReadLine(), out int directorId))
        {
            Console.WriteLine("❌ Неверный ID");
            return;
        }

        var director = await context.Directors.FindAsync(directorId);
        if (director == null)
        {
            Console.WriteLine($"❌ Режиссер с ID {directorId} не найден\n");
            return;
        }

        Console.WriteLine($"\nРедактирование режиссера: {director.Name}");
        Console.WriteLine("(Оставьте поле пустым, чтобы оставить без изменений)");

        Console.Write($"\nИзмененное имя [{director.Name}]: ");
        var name = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(name))
        {
            director.Name = name;
        }

        Console.Write($"Измененный возраст [{director.Age}]: ");
        var ageStr = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(ageStr) && int.TryParse(ageStr, out int age))
        {
            director.Age = age;
        }

        Console.Write($"Измененные награды [{director.Awards ?? "нет"}]: ");
        var awards = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(awards))
        {
            director.Awards = awards;
        }
        else if (awards == "")
        {
            director.Awards = null;
        }

        await context.SaveChangesAsync();
        Console.WriteLine("\n✅ Режиссер успешно обновлен");
    }

    private static async Task EditActor()
    {
        using var context = new AppDbContext();

        Console.WriteLine("\n" + "=".PadRight(60, '='));
        Console.WriteLine("РЕДАКТИРОВАНИЕ АКТЕРА".PadLeft(35));
        Console.WriteLine("=".PadRight(60, '='));

        var actors = await context.Actors
            .Include(a => a.Roles)
            .ThenInclude(r => r.Movie)
            .OrderBy(a => a.Name)
            .ToListAsync();

        if (!actors.Any())
        {
            Console.WriteLine("\n📢 Актеры не найдены.");
            return;
        }

        Console.WriteLine("\nДоступные актеры:");
        foreach (var a in actors)
        {
            Console.WriteLine($"   [{a.Id}] {a.Name} ({a.Country}) - {a.Age} лет, ролей: {a.Roles?.Count ?? 0}");
        }
        /*if (actors.Count > 10)
        {
            Console.WriteLine($"   ... и ещё {actors.Count - 10} актеров");
        }*/

        Console.Write("\nВведите ID актера для редактирования: ");
        if (!int.TryParse(Console.ReadLine(), out int actorId))
        {
            Console.WriteLine("❌ Неверный ID\n");
            return;
        }

        var actor = await context.Actors
            .Include(a => a.Roles)
            .ThenInclude(r => r.Movie)
            .FirstOrDefaultAsync(a => a.Id == actorId);

        if (actor == null)
        {
            Console.WriteLine($"❌ Актер с ID {actorId} не найден\n");
            return;
        }

        Console.WriteLine($"\nРедактирование актера: {actor.Name}");
        //Console.WriteLine("(Оставьте поле пустым, чтобы оставить без изменений)");

        bool exitActorEdit = false;
        while (!exitActorEdit)
        {
            Console.WriteLine($"\nТекущая информация об актере:");
            Console.WriteLine($"   Имя: {actor.Name}");
            Console.WriteLine($"   Дата рождения: {actor.BirthDate:yyyy-MM-dd} ({actor.Age} лет)");
            Console.WriteLine($"   Страна: {actor.Country}");

            if (actor.Roles != null && actor.Roles.Any())
            {
                Console.WriteLine($"\n   Роли актера ({actor.Roles.Count}):");
                foreach (var role in actor.Roles.OrderBy(r => r.Movie?.Year))
                {
                    Console.WriteLine($"      • {role.CharacterName} - в фильме \"{role.Movie?.Title}\" ({role.Movie?.Year})");
                }
            }
            else
            {
                Console.WriteLine($"\n   Роли актера: отсутствуют");
            }

            Console.WriteLine("\nВыберите действие:");
            Console.WriteLine("   1. Редактировать персональные данные");
            Console.WriteLine("   2. Управление ролями");
            Console.WriteLine("   0. Вернуться в главное меню");
            Console.Write("Ваш выбор: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await EditActorPersonalData(actor, context);
                    break;
                case "2":
                    await ManageActorRoles(actor, context);
                    break;
                case "0":
                    exitActorEdit = true;
                    break;
                default:
                    Console.WriteLine("❌ Неверный выбор");
                    break;
            }
        }
    }

    // Метод для редактирования режиссера
    private static async Task EditMovieDirector(Movie movie, AppDbContext context)
    {
        Console.WriteLine("\n--- РЕДАКТИРОВАНИЕ РЕЖИССЕРА ---");

        bool exitDirectorEdit = false;
        while (!exitDirectorEdit)
        {
            // Показываем текущего режиссера
            if (movie.Director != null)
            {
                Console.WriteLine($"\nТекущий режиссер: {movie.Director.Name} ({movie.Director.Age} лет)");
                if (!string.IsNullOrEmpty(movie.Director.Awards))
                    Console.WriteLine($"   Награды: {movie.Director.Awards}");
            }
            else
            {
                Console.WriteLine("\n⚠ Режиссер не выбран\n");
            }

            // Показываем всех доступных режиссеров
            var directors = await context.Directors.OrderBy(d => d.Name).ToListAsync();
            if (directors.Any())
            {
                Console.WriteLine("\nДоступные режиссеры:");
                foreach (var d in directors)
                {
                    string selected = (movie.DirectorId == d.Id) ? "✓ " : "  ";
                    Console.WriteLine($"   {selected}[{d.Id}] {d.Name} ({d.Age} лет)");
                    if (!string.IsNullOrEmpty(d.Awards))
                        Console.WriteLine($"        Награды: {d.Awards}");
                }
            }

            Console.WriteLine("\nВыберите действие:");
            Console.WriteLine("   1. Выбрать режиссера из списка");
            Console.WriteLine("   2. Добавить нового режиссера");
            Console.WriteLine("   3. Убрать режиссера (сделать не выбранным)");
            Console.WriteLine("   0. Вернуться назад");
            Console.Write("Ваш выбор: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Введите ID режиссера: ");
                    if (int.TryParse(Console.ReadLine(), out int directorId))
                    {
                        var director = await context.Directors.FindAsync(directorId);
                        if (director != null)
                        {
                            movie.DirectorId = directorId;
                            movie.Director = director;
                            await context.SaveChangesAsync();
                            Console.WriteLine($"✅ Режиссер изменен на: {director.Name}\n");
                        }
                        else
                        {
                            Console.WriteLine("❌ Режиссер не найден\n");
                        }
                    }
                    break;

                case "2":
                    var newDirector = await AddNewDirectorForMovie(context);
                    if (newDirector != null)
                    {
                        movie.DirectorId = newDirector.Id;
                        movie.Director = newDirector;
                        await context.SaveChangesAsync();
                        Console.WriteLine($"✅ Новый режиссер добавлен и назначен фильму\n");
                    }
                    break;

                case "3":
                    movie.DirectorId = 0;
                    movie.Director = null;
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Режиссер убран\n");
                    break;

                case "0":
                    exitDirectorEdit = true;
                    break;

                default:
                    Console.WriteLine("❌ Неверный выбор\n");
                    break;
            }
        }
    }

    // Метод для редактирования жанров
    private static async Task EditMovieGenres(Movie movie, AppDbContext context)
    {
        Console.WriteLine("\n----- РЕДАКТИРОВАНИЕ ЖАНРОВ -----");

        bool exitGenreEdit = false;
        while (!exitGenreEdit)
        {
            // Показываем текущие жанры
            if (movie.MovieGenres != null && movie.MovieGenres.Any())
            {
                var currentGenres = movie.MovieGenres.Select(mg => mg.Genre?.Name);
                Console.WriteLine($"\nТекущие жанры: {string.Join(", ", currentGenres)}");
            }
            else
            {
                Console.WriteLine("\n⚠ Жанры не выбраны");
            }

            // Получаем все доступные жанры
            var allGenres = await context.Genres.OrderBy(g => g.Name).ToListAsync();

            Console.WriteLine("\nДоступные жанры:");
            foreach (var genre in allGenres)
            {
                bool isSelected = movie.MovieGenres?.Any(mg => mg.GenreId == genre.Id) ?? false;
                string selected = isSelected ? "✓ " : "  ";
                Console.WriteLine($"   {selected}[{genre.Id}] {genre.Name} - {genre.Description ?? "Нет описания"}");
            }

            Console.WriteLine("\nВыберите действие:");
            Console.WriteLine("   1. Добавить жанр");
            Console.WriteLine("   2. Удалить жанр");
            Console.WriteLine("   0. Вернуться назад");
            Console.Write("Ваш выбор: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await AddGenreToMovie(movie, context, allGenres);
                    break;
                case "2":
                    await RemoveGenreFromMovie(movie, context, allGenres);
                    break;
                case "0":
                    exitGenreEdit = true;
                    break;
                default:
                    Console.WriteLine("❌ Неверный выбор");
                    break;
            }
        }
    }

    // Метод для редактирования актеров и ролей
    private static async Task ManageMovieActorsAndRoles(Movie movie, AppDbContext context)
    {
        bool exitActorManagement = false;
        while (!exitActorManagement)
        {
            // Перезагружаем актеров и роли из БД
            await context.Entry(movie)
                .Collection(m => m.Roles)
                .Query()
                .Include(r => r.Actor)
                .LoadAsync();

            Console.WriteLine($"\n{"-".PadRight(70, '-')}");
            Console.WriteLine("УПРАВЛЕНИЕ АКТЕРАМИ И РОЛЯМИ");
            Console.WriteLine($"{"-".PadRight(70, '-')}");

            if (movie.Roles != null && movie.Roles.Any())
            {
                Console.WriteLine($"\nТекущие актеры в фильме \"{movie.Title}\":");
                foreach (var role in movie.Roles.OrderBy(r => r.Actor?.Name))
                {
                    Console.WriteLine($"   [{role.Id}] {role.Actor?.Name} — {role.CharacterName}");
                }
            }
            else
            {
                Console.WriteLine($"\nВ фильме \"{movie.Title}\" пока нет актеров.");
            }

            Console.WriteLine("\nВыберите действие:");
            Console.WriteLine("   1. Добавить нового актера с ролью");
            Console.WriteLine("   2. Редактировать роль существующего актера");
            Console.WriteLine("   3. Удалить актера из фильма");
            Console.WriteLine("   0. Вернуться к редактированию фильма");
            Console.Write("Ваш выбор: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await AddActorWithRoleToMovie(movie, context);
                    break;
                case "2":
                    await EditActorRoleInMovie(movie, context);
                    break;
                case "3":
                    await RemoveActorFromMovie(movie, context);
                    break;
                case "0":
                    exitActorManagement = true;
                    break;
                default:
                    Console.WriteLine("❌ Неверный выбор");
                    break;
            }
        }
    }

    // Метод для редактирования персональных данных актера
    private static async Task EditActorPersonalData(Actor actor, AppDbContext context)
    {
        Console.WriteLine("\n--- РЕДАКТИРОВАНИЕ ПЕРСОНАЛЬНЫХ ДАННЫХ ---");
        Console.WriteLine("(Оставьте поле пустым, чтобы оставить без изменений)");

        Console.Write($"\nИзмененное имя [{actor.Name}]: ");
        var name = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(name))
        {
            actor.Name = name;
        }

        Console.Write($"Измененная дата рождения [{actor.BirthDate:yyyy-MM-dd}]: ");
        var birthDateStr = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(birthDateStr) && DateTime.TryParse(birthDateStr, out DateTime birthDate))
        {
            actor.BirthDate = birthDate;
        }

        Console.Write($"Измененная страна [{actor.Country}]: ");
        var country = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(country))
        {
            actor.Country = country;
        }

        await context.SaveChangesAsync();
        Console.WriteLine("\n✅ Персональные данные актера успешно обновлены");
    }

    // Метод для управления ролями актера
    private static async Task ManageActorRoles(Actor actor, AppDbContext context)
    {
        bool exitRoleManagement = false;
        while (!exitRoleManagement)
        {
            Console.WriteLine("\n--- УПРАВЛЕНИЕ РОЛЯМИ АКТЕРА ---");

            if (actor.Roles != null && actor.Roles.Any())
            {
                Console.WriteLine($"\nТекущие роли актера {actor.Name}:");
                foreach (var role in actor.Roles.OrderBy(r => r.Movie?.Year))
                {
                    Console.WriteLine($"   [{role.Id}] {role.CharacterName} - в фильме \"{role.Movie?.Title}\" ({role.Movie?.Year})");
                }
            }
            else
            {
                Console.WriteLine($"\nУ актера {actor.Name} пока нет ролей.");
            }

            Console.WriteLine("\nВыберите действие:");
            Console.WriteLine("   1. Добавить новую роль");
            Console.WriteLine("   2. Редактировать существующую роль");
            Console.WriteLine("   3. Удалить роль");
            Console.WriteLine("   0. Вернуться назад");
            Console.Write("Ваш выбор: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await AddRoleToActor(actor, context);
                    break;
                case "2":
                    await EditActorRole(actor, context);
                    break;
                case "3":
                    await DeleteActorRole(actor, context);
                    break;
                case "0":
                    exitRoleManagement = true;
                    break;
                default:
                    Console.WriteLine("❌ Неверный выбор");
                    break;
            }
        }
    }

    // Метод для добавления роли актеру
    private static async Task AddRoleToActor(Actor actor, AppDbContext context)
    {
        Console.WriteLine("\n--- ДОБАВЛЕНИЕ НОВОЙ РОЛИ ---");

        // Показываем список фильмов для выбора
        var movies = await context.Movies
            .OrderBy(m => m.Title)
            .ToListAsync();

        if (!movies.Any())
        {
            Console.WriteLine("❌ Нет доступных фильмов. Сначала добавьте фильм.");
            return;
        }

        Console.WriteLine("\nДоступные фильмы:");
        foreach (var m in movies)
        {
            Console.WriteLine($"   [{m.Id}] {m.Title} ({m.Year})");
        }

        Console.Write("\nВведите ID фильма: ");
        if (!int.TryParse(Console.ReadLine(), out int movieId))
        {
            Console.WriteLine("❌ Неверный ID фильма");
            return;
        }

        var selectedMovie = movies.FirstOrDefault(m => m.Id == movieId);
        if (selectedMovie == null)
        {
            Console.WriteLine("❌ Фильм не найден");
            return;
        }

        // Проверяем, есть ли уже у актера роль в этом фильме
        bool hasRole = actor.Roles?.Any(r => r.MovieId == movieId) ?? false;
        if (hasRole)
        {
            Console.WriteLine($"❌ Актер уже имеет роль в фильме \"{selectedMovie.Title}\"");
            return;
        }

        Console.Write("Введите имя персонажа: ");
        var characterName = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(characterName))
        {
            Console.WriteLine("❌ Имя персонажа не может быть пустым");
            return;
        }

        // Получаем максимальный Id для новой роли
        int maxRoleId = 0;
        if (await context.Roles.AnyAsync())
        {
            maxRoleId = await context.Roles.MaxAsync(r => r.Id);
        }

        var newRole = new Role
        {
            Id = maxRoleId + 1,
            CharacterName = characterName,
            MovieId = movieId,
            ActorId = actor.Id
        };

        await context.Roles.AddAsync(newRole);
        await context.SaveChangesAsync();

        // Обновляем навигационное свойство актера
        /*actor.Roles ??= new List<Role>();
        actor.Roles.Add(newRole);*/

        Console.WriteLine($"\n✅ Роль '{characterName}' в фильме \"{selectedMovie.Title}\" успешно добавлена актеру {actor.Name}");

        // После добавления роли, перезагружаем актера из БД, чтобы получить актуальный список ролей
        var updatedActor = await context.Actors
            .Include(a => a.Roles)
                .ThenInclude(r => r.Movie)
            .FirstOrDefaultAsync(a => a.Id == actor.Id);

        if (updatedActor != null)
        {
            // Обновляем все свойства актера
            actor.Name = updatedActor.Name;
            actor.BirthDate = updatedActor.BirthDate;
            actor.Country = updatedActor.Country;
            actor.Roles = updatedActor.Roles;
        }
    }

    // Метод для редактирования роли актера
    private static async Task EditActorRole(Actor actor, AppDbContext context)
    {
        if (actor.Roles == null || !actor.Roles.Any())
        {
            Console.WriteLine("❌ У актера нет ролей для редактирования");
            return;
        }

        Console.WriteLine("\n--- РЕДАКТИРОВАНИЕ РОЛИ ---");

        Console.WriteLine("\nРоли актера:");
        foreach (var r in actor.Roles.OrderBy(r => r.Movie?.Year))
        {
            Console.WriteLine($"   [{r.Id}] {r.CharacterName} - в фильме \"{r.Movie?.Title}\" ({r.Movie?.Year})");
        }

        Console.Write("\nВведите ID роли для редактирования: ");
        if (!int.TryParse(Console.ReadLine(), out int roleId))
        {
            Console.WriteLine("❌ Неверный ID роли");
            return;
        }

        var selectedRole = actor.Roles.FirstOrDefault(r => r.Id == roleId);
        if (selectedRole == null)
        {
            Console.WriteLine("❌ Роль не найдена");
            return;
        }

        Console.WriteLine($"\nРедактирование роли: {selectedRole.CharacterName}");
        Console.WriteLine("(Оставьте поле пустым, чтобы оставить без изменений)");

        Console.Write($"Измененное имя персонажа [{selectedRole.CharacterName}]: ");
        var newCharacterName = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(newCharacterName))
        {
            selectedRole.CharacterName = newCharacterName;
        }

        // Можно также изменить фильм для роли
        Console.Write("\nХотите изменить фильм для этой роли? (д/н): ");
        if (Console.ReadLine()?.ToLower() == "д")
        {
            var movies = await context.Movies
                .OrderBy(m => m.Title)
                .ToListAsync();

            Console.WriteLine("\nДоступные фильмы:");
            foreach (var m in movies)
            {
                Console.WriteLine($"   [{m.Id}] {m.Title} ({m.Year})");
            }

            Console.Write("Введите ID нового фильма: ");
            if (int.TryParse(Console.ReadLine(), out int newMovieId))
            {
                var newMovie = movies.FirstOrDefault(m => m.Id == newMovieId);
                if (newMovie != null)
                {
                    // Проверяем, нет ли уже такой роли в новом фильме
                    bool hasRole = actor.Roles.Any(r => r.MovieId == newMovieId && r.Id != roleId);
                    if (hasRole)
                    {
                        Console.WriteLine($"❌ Актер уже имеет роль в фильме \"{newMovie.Title}\"");
                    }
                    else
                    {
                        selectedRole.MovieId = newMovieId;
                    }
                }
                else
                {
                    Console.WriteLine("❌ Фильм не найден");
                }
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine("\n✅ Роль успешно обновлена");
    }

    // ==================== МЕТОДЫ УДАЛЕНИЯ ====================

    private static async Task DeleteMovie()
    {
        using var context = new AppDbContext();

        Console.WriteLine("\n" + "=".PadRight(60, '='));
        Console.WriteLine("УДАЛЕНИЕ ФИЛЬМА".PadLeft(35));
        Console.WriteLine("=".PadRight(60, '='));

        var movies = await context.Movies
            .Include(m => m.Director)
            .Include(m => m.Roles)
            .OrderBy(m => m.Title)
            .ToListAsync();

        if (!movies.Any())
        {
            Console.WriteLine("\n📢 Фильмы не найдены.");
            return;
        }

        Console.WriteLine("\nДоступные фильмы:");
        foreach (var m in movies)
        {
            string actors = m.Roles != null && m.Roles.Any()
                ? $", актеров: {m.Roles.Count}"
                : "";
            Console.WriteLine($"   [{m.Id}] {m.Title} ({m.Year}) - {m.Director?.Name ?? "Неизвестен"}{actors}");
        }

        Console.Write("\nВведите ID фильма для удаления: ");
        if (!int.TryParse(Console.ReadLine(), out int movieId))
        {
            Console.WriteLine("❌ Неверный ID");
            return;
        }

        var movie = await context.Movies
            .Include(m => m.Roles)
            .Include(m => m.MovieGenres)
            .FirstOrDefaultAsync(m => m.Id == movieId);

        if (movie == null)
        {
            Console.WriteLine($"❌ Фильм с ID {movieId} не найден");
            return;
        }

        Console.WriteLine($"\nВы действительно хотите удалить фильм?");
        Console.WriteLine($"   Название: {movie.Title} ({movie.Year})");
        Console.WriteLine($"   Длительность: {movie.FormattedDuration}");
        Console.WriteLine($"   Будет удалено ролей: {movie.Roles?.Count ?? 0}");
        Console.WriteLine($"   Будет удалено связей с жанрами: {movie.MovieGenres?.Count ?? 0}");

        Console.Write("\nПодтвердите удаление (д/н): ");
        if (Console.ReadLine()?.ToLower() != "д")
        {
            Console.WriteLine("❌ Удаление отменено");
            return;
        }

        context.Movies.Remove(movie);
        await context.SaveChangesAsync();
        Console.WriteLine($"\n✅ Фильм '{movie.Title}' успешно удален");
    }

    // Метод для удаления роли актера
    private static async Task DeleteActorRole(Actor actor, AppDbContext context)
    {
        if (actor.Roles == null || !actor.Roles.Any())
        {
            Console.WriteLine("❌ У актера нет ролей для удаления");
            return;
        }

        Console.WriteLine("\n--- УДАЛЕНИЕ РОЛИ ---");

        Console.WriteLine("\nРоли актера:");
        foreach (var r in actor.Roles.OrderBy(r => r.Movie?.Year))
        {
            Console.WriteLine($"   [{r.Id}] {r.CharacterName} - в фильме \"{r.Movie?.Title}\" ({r.Movie?.Year})");
        }

        Console.Write("\nВведите ID роли для удаления: ");
        if (!int.TryParse(Console.ReadLine(), out int roleId))
        {
            Console.WriteLine("❌ Неверный ID роли");
            return;
        }

        var selectedRole = actor.Roles.FirstOrDefault(r => r.Id == roleId);
        if (selectedRole == null)
        {
            Console.WriteLine("❌ Роль не найдена");
            return;
        }

        Console.WriteLine($"\nВы действительно хотите удалить роль '{selectedRole.CharacterName}' в фильме \"{selectedRole.Movie?.Title}\"?");
        Console.Write("Подтвердите удаление (д/н): ");

        if (Console.ReadLine()?.ToLower() != "д")
        {
            Console.WriteLine("❌ Удаление отменено");
            return;
        }

        context.Roles.Remove(selectedRole);
        actor.Roles.Remove(selectedRole);
        await context.SaveChangesAsync();

        Console.WriteLine($"\n✅ Роль '{selectedRole.CharacterName}' успешно удалена");
    }

    // ==================== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ====================

    private static async Task AddActorWithRoleToMovie(Movie movie, AppDbContext context)
    {
        Console.WriteLine("\n--- ДОБАВЛЕНИЕ АКТЕРА В ФИЛЬМ ---");

        // Получаем всех актеров из БД
        var allActors = await context.Actors
            .OrderBy(a => a.Name)
            .ToListAsync();

        if (!allActors.Any())
        {
            Console.WriteLine("❌ В базе нет актеров. Сначала добавьте актера.\n");
            return;
        }

        // Получаем ID актеров, которые уже есть в фильме
        var existingActorIds = movie.Roles?.Select(r => r.ActorId).ToHashSet() ?? new HashSet<int>();

        Console.WriteLine("\nДоступные актеры:");
        foreach (var actor in allActors)
        {
            string alreadyInMovie = existingActorIds.Contains(actor.Id) ? " (УЖЕ В ФИЛЬМЕ)" : "";
            Console.WriteLine($"   [{actor.Id}] {actor.Name} ({actor.Country}){alreadyInMovie}");
        }

        Console.Write("\nВведите ID актера для добавления: ");
        if (!int.TryParse(Console.ReadLine(), out int actorId))
        {
            Console.WriteLine("❌ Неверный ID\n");
            return;
        }

        // Проверяем, есть ли уже такой актер в фильме
        if (existingActorIds.Contains(actorId))
        {
            Console.WriteLine("❌ Этот актер уже есть в фильме\n");
            return;
        }

        var selectedActor = allActors.FirstOrDefault(a => a.Id == actorId);
        if (selectedActor == null)
        {
            Console.WriteLine("❌ Актер не найден\n");
            return;
        }

        Console.Write("Введите имя персонажа: ");
        var characterName = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(characterName))
        {
            Console.WriteLine("❌ Имя персонажа не может быть пустым\n");
            return;
        }

        // Получаем максимальный ID для роли
        int maxRoleId = 0;
        if (await context.Roles.AnyAsync())
        {
            maxRoleId = await context.Roles.MaxAsync(r => r.Id);
        }

        var newRole = new Role
        {
            Id = maxRoleId + 1,
            CharacterName = characterName,
            MovieId = movie.Id,
            ActorId = actorId
        };

        await context.Roles.AddAsync(newRole);
        await context.SaveChangesAsync();

        Console.WriteLine($"\n✅ Актер {selectedActor.Name} добавлен в фильм в роли {characterName}\n");
    }

    private static async Task EditActorRoleInMovie(Movie movie, AppDbContext context)
    {
        if (movie.Roles == null || !movie.Roles.Any())
        {
            Console.WriteLine("❌ В фильме нет актеров для редактирования\n");
            return;
        }

        Console.WriteLine("\n--- РЕДАКТИРОВАНИЕ РОЛИ АКТЕРА ---");

        Console.WriteLine("\nАктеры в фильме:");
        foreach (var role in movie.Roles.OrderBy(r => r.Actor?.Name))
        {
            Console.WriteLine($"   [{role.Id}] {role.Actor?.Name} — {role.CharacterName}");
        }

        Console.Write("\nВведите ID роли для редактирования: ");
        if (!int.TryParse(Console.ReadLine(), out int roleId))
        {
            Console.WriteLine("❌ Неверный ID\n");
            return;
        }

        var selectedRole = movie.Roles.FirstOrDefault(r => r.Id == roleId);
        if (selectedRole == null)
        {
            Console.WriteLine("❌ Роль не найдена\n");
            return;
        }

        Console.WriteLine($"\nРедактирование роли: {selectedRole.CharacterName}");
        Console.WriteLine("(Оставьте поле пустым, чтобы оставить без изменений)");

        Console.Write($"Измененное имя персонажа [{selectedRole.CharacterName}]: ");
        var newCharacterName = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(newCharacterName))
        {
            selectedRole.CharacterName = newCharacterName;
        }

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Роль успешно обновлена\n");
    }

    private static async Task RemoveActorFromMovie(Movie movie, AppDbContext context)
    {
        if (movie.Roles == null || !movie.Roles.Any())
        {
            Console.WriteLine("❌ В фильме нет актеров для удаления\n");
            return;
        }

        Console.WriteLine("\n--- УДАЛЕНИЕ АКТЕРА ИЗ ФИЛЬМА ---");

        Console.WriteLine("\nАктеры в фильме:");
        foreach (var role in movie.Roles.OrderBy(r => r.Actor?.Name))
        {
            Console.WriteLine($"   [{role.Id}] {role.Actor?.Name} — {role.CharacterName}");
        }

        Console.Write("\nВведите ID роли для удаления: ");
        if (!int.TryParse(Console.ReadLine(), out int roleId))
        {
            Console.WriteLine("❌ Неверный ID\n");
            return;
        }

        var selectedRole = movie.Roles.FirstOrDefault(r => r.Id == roleId);
        if (selectedRole == null)
        {
            Console.WriteLine("❌ Роль не найдена\n");
            return;
        }

        Console.WriteLine($"\nВы действительно хотите удалить актера {selectedRole.Actor?.Name} (роль {selectedRole.CharacterName}) из фильма?");
        Console.Write("Подтвердите удаление (д/н): ");

        if (Console.ReadLine()?.ToLower() != "д")
        {
            Console.WriteLine("❌ Удаление отменено\n");
            return;
        }

        context.Roles.Remove(selectedRole);
        await context.SaveChangesAsync();

        Console.WriteLine($"\n✅ Актер {selectedRole.Actor?.Name} удален из фильма\n");
    }

    private static async Task<Director?> AddNewDirectorForMovie(AppDbContext context)
    {
        var director = new Director();

        Console.WriteLine("\n--- ДОБАВЛЕНИЕ НОВОГО РЕЖИССЕРА ---");

        Console.Write("Введите ФИО режиссера: ");
        var name = Console.ReadLine()?.Trim();
        while (string.IsNullOrWhiteSpace(name))
        {
            Console.Write("❌ ФИО не может быть пустым. Введите ФИО режиссера: ");
            name = Console.ReadLine()?.Trim();
        }
        director.Name = name;

        Console.Write("Введите возраст: ");
        int age;
        while (!int.TryParse(Console.ReadLine(), out age) || age < 0)
        {
            Console.Write("❌ Неверный возраст. Введите возраст: ");
        }
        director.Age = age;

        Console.Write("Введите награды (можно пропустить, Enter для пропуска): ");
        var awards = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(awards))
        {
            director.Awards = awards;
        }

        // Проверяем, есть ли уже такой режиссер
        var existingDirector = await context.Directors
            .FirstOrDefaultAsync(d => d.Name == director.Name && d.Age == director.Age);

        if (existingDirector != null)
        {
            Console.WriteLine($"\nНайден существующий режиссер:");
            Console.WriteLine($"   Имя: {existingDirector.Name}");
            Console.WriteLine($"   Возраст: {existingDirector.Age}");
            if (!string.IsNullOrEmpty(existingDirector.Awards))
                Console.WriteLine($"   Награды: {existingDirector.Awards}");

            Console.Write("\nИспользовать этого режиссера? (д/н): ");
            if (Console.ReadLine()?.ToLower() == "д")
            {
                return existingDirector;
            }
        }

        await context.Directors.AddAsync(director);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ Новый режиссер добавлен с ID {director.Id}");
        return director;
    }

    private static async Task AddGenreToMovie(Movie movie, AppDbContext context, List<Genre> allGenres)
    {
        Console.Write("\nВведите ID жанра для добавления: ");
        if (int.TryParse(Console.ReadLine(), out int genreId))
        {
            var genre = allGenres.FirstOrDefault(g => g.Id == genreId);
            if (genre != null)
            {
                // Проверяем, не добавлен ли уже этот жанр
                bool alreadyExists = movie.MovieGenres?.Any(mg => mg.GenreId == genreId) ?? false;

                if (!alreadyExists)
                {
                    // Получаем максимальный ID для MovieGenre
                    int maxId = 0;
                    if (await context.MovieGenres.AnyAsync())
                    {
                        maxId = await context.MovieGenres.MaxAsync(mg => mg.Id);
                    }

                    var movieGenre = new MovieGenre
                    {
                        Id = maxId + 1,
                        MovieId = movie.Id,
                        GenreId = genreId
                    };

                    await context.MovieGenres.AddAsync(movieGenre);
                    await context.SaveChangesAsync();

                    // Обновляем навигационное свойство
                    if (movie.MovieGenres == null)
                        movie.MovieGenres = new List<MovieGenre>();
                    movie.MovieGenres.Add(movieGenre);

                    Console.WriteLine($"✅ Жанр '{genre.Name}' добавлен к фильму");
                }
                else
                {
                    Console.WriteLine("❌ Этот жанр уже добавлен к фильму");
                }
            }
            else
            {
                Console.WriteLine("❌ Жанр не найден");
            }
        }
        else
        {
            Console.WriteLine("❌ Неверный ID");
        }
    }

    private static async Task EditMovieTitle(Movie movie)
    {
        Console.Write($"\nВведите измененное название [{movie.Title}]: ");
        var title = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(title))
        {
            movie.Title = title;
        }
    }

    private static async Task EditMovieYear(Movie movie)
    {
        Console.Write($"\nВведите измененный год [{movie.Year}]: ");
        var yearStr = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(yearStr) && int.TryParse(yearStr, out int year))
        {
            if (year >= 1890 && year <= 2050)
            {
                movie.Year = year;
            }
            else
            {
                Console.WriteLine("❌ Год должен быть между 1890 и 2050\n");
            }
        }
    }

    private static async Task EditMovieCountry(Movie movie)
    {
        Console.Write($"\nВведите измененную страну [{movie.Country}]: ");
        var country = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(country))
        {
            movie.Country = country;
        }
    }

    private static async Task EditMovieDuration(Movie movie)
    {
        Console.WriteLine($"\nТекущая длительность: {movie.FormattedDuration}");
        Console.Write("Введите измененную длительность (в минутах): ");
        var durationStr = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(durationStr) && int.TryParse(durationStr, out int newDuration))
        {
            if (newDuration > 0)
            {
                movie.DurationMinutes = newDuration;
                Console.WriteLine($"   Измененная длительность: {movie.FormattedDuration}\n");
            }
            else
            {
                Console.WriteLine("❌ Длительность должна быть положительна\n");
            }
        }
    }

    private static async Task EditMovieAgeRating(Movie movie)
    {
        Console.WriteLine($"\nТекущее возрастное ограничение: {movie.AgeRating.GetDisplayName()}");
        Console.WriteLine("Доступные варианты:");
        foreach (AgeRating rating in Enum.GetValues(typeof(AgeRating)))
        {
            Console.WriteLine($"   [{(int)rating}] {rating.GetDisplayName()}");
        }
        Console.Write("Введите номер измененного возрастного ограничения: ");
        var ratingStr = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(ratingStr) && int.TryParse(ratingStr, out int ratingValue) &&
            Enum.IsDefined(typeof(AgeRating), ratingValue))
        {
            movie.AgeRating = (AgeRating)ratingValue;
        }
    }

    private static async Task EditMovieDescription(Movie movie)
    {
        Console.WriteLine($"\nТекущее описание:");
        if (!string.IsNullOrEmpty(movie.Description))
        {
            PrintWrappedText(movie.Description, 3, 80);
        }
        else
        {
            Console.WriteLine("   Описание отсутствует\n");
        }

        Console.WriteLine("\nВведите измененное описание (Enter чтобы оставить, 'DELETE' чтобы удалить):");
        var description = Console.ReadLine();
        if (description == "DELETE")
        {
            movie.Description = null;
            Console.WriteLine("✅ Описание удалено\n");
        }
        else if (!string.IsNullOrEmpty(description))
        {
            movie.Description = description;
            Console.WriteLine("✅ Описание обновлено\n");
        }
    }

    private static async Task RemoveGenreFromMovie(Movie movie, AppDbContext context, List<Genre> allGenres)
    {
        if (movie.MovieGenres == null || !movie.MovieGenres.Any())
        {
            Console.WriteLine("❌ У фильма нет жанров для удаления");
            return;
        }

        Console.Write("\nВведите ID жанра для удаления: ");
        if (int.TryParse(Console.ReadLine(), out int genreId))
        {
            var genre = allGenres.FirstOrDefault(g => g.Id == genreId);
            if (genre != null)
            {
                var movieGenre = movie.MovieGenres.FirstOrDefault(mg => mg.GenreId == genreId);

                if (movieGenre != null)
                {
                    context.MovieGenres.Remove(movieGenre);
                    movie.MovieGenres.Remove(movieGenre);
                    await context.SaveChangesAsync();

                    Console.WriteLine($"✅ Жанр '{genre.Name}' удален из фильма");
                }
                else
                {
                    Console.WriteLine("❌ Этот жанр не привязан к фильму");
                }
            }
            else
            {
                Console.WriteLine("❌ Жанр не найден");
            }
        }
        else
        {
            Console.WriteLine("❌ Неверный ID");
        }
    }

    private static void PrintWrappedText(string text, int indent = 0, int width = 70)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var paragraphs = text.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        string indentStr = new string(' ', indent);

        foreach (var paragraph in paragraphs)
        {
            var words = paragraph.Split(' ');
            var line = new List<string>();
            var lineLength = 0;

            foreach (var word in words)
            {
                if (lineLength + word.Length + (line.Count > 0 ? 1 : 0) > width)
                {
                    Console.WriteLine($"{indentStr}{string.Join(" ", line)}");
                    line.Clear();
                    lineLength = 0;
                }

                line.Add(word);
                lineLength += word.Length + (line.Count > 1 ? 1 : 0);
            }

            if (line.Any())
            {
                Console.WriteLine($"{indentStr}{string.Join(" ", line)}");
            }

            if (paragraphs.Length > 1)
            {
                Console.WriteLine();
            }
        }
    }
}