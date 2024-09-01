using MoviesService.Models;

namespace MoviesService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProd)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), isProd);
            }
        }

        private static void SeedData(AppDbContext? context, bool isProd)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (isProd)
            {
                try
                {
                    Console.WriteLine("--> Attempting to apply migrations...");
                    //context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not run migrations: {ex.Message}");
                }
            }

            if (!context.Movies.Any())
            {
                Console.WriteLine("--> Adding Movies...");
                context.Movies.AddRange(
                    new Movie() { Id = 1, 
                        PrimaryTitle = "Miss Jerry", 
                        OriginalTitle = "Miss Jerry",
                        IsAdult = false,
                        Year = 1894,
                        RuntimeMinutes = 45,
                        Genres = new List<string>() { "Romance" }
                    },
                    new Movie()
                    {
                        Id = 2,
                        PrimaryTitle = "Chico Albuquerque",
                        OriginalTitle = "Chico Albuquerque",
                        IsAdult = false,
                        Year = 2013,
                        RuntimeMinutes = 49,
                        Genres = new List<string>() { "Documentary" }
                    },
                    new Movie()
                    {
                        Id = 3,
                        PrimaryTitle = "6 Gun",
                        OriginalTitle = "6 Gun",
                        IsAdult = false,
                        Year = 2017,
                        RuntimeMinutes = 116,
                        Genres = new List<string>() { "Drama" }
                    }
                );

                context.SaveChanges();
            }

            if (!context.Serials.Any())
            {
                Console.WriteLine("--> Adding Serials...");
                context.Serials.AddRange(
                    new Serial()
                    {
                        Id = 1,
                        PrimaryTitle = "Acelerados",
                        OriginalTitle = "Acelerados",
                        IsAdult = false,
                        StartYear = 2019,
                        EndYear = null,
                        RuntimeMinutes = null,
                        Genres = new List<string>() { "Comedy" }
                    },
                    new Serial()
                    {
                        Id = 2,
                        PrimaryTitle = "Meie aasta Aafrikas",
                        OriginalTitle = "Meie aasta Aafrikas",
                        IsAdult = false,
                        StartYear = 2019,
                        EndYear = null,
                        RuntimeMinutes = 43,
                        Genres = new List<string>() { "Adventure", "Comedy", "Family" }
                    },
                    new Serial()
                    {
                        Id = 3,
                        PrimaryTitle = "Atkexotics",
                        OriginalTitle = "Atkexotics",
                        IsAdult = true,
                        StartYear = 2003,
                        EndYear = null,
                        RuntimeMinutes = null,
                        Genres = new List<string>() { "Adult", "Short" }
                    }
                );

                context.SaveChanges();
            }
        }
    }
}
