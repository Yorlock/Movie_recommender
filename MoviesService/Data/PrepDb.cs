using MoviesService.Models;
using System.IO.Compression;

namespace MoviesService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProd, IConfiguration config)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), isProd, config);
            }
        }

        private static void SeedData(AppDbContext? context, bool isProd, IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(config);

            if (isProd)
            {
                try
                {
                    // Console.WriteLine("--> Attempting to apply migrations...");
                    // context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    // Console.WriteLine($"--> Could not run migrations: {ex.Message}");
                }
            }

            if (context.Movies.Any() && context.Serials.Any()) return;

            DataLoader dataLoader = new(context, config);

            dataLoader.Load();

            context.SaveChanges();
        }

        private class DataLoader
        {
            private readonly AppDbContext _context;
            private readonly IConfiguration _config;
            private readonly bool IsMovieLimit;
            private readonly bool IsSerialLimit;
            private readonly int MaxMovies;
            private readonly int MaxSerials;
            private int CurrentMovies = 0;
            private int CurrentSerials = 0;

            public DataLoader(AppDbContext context, IConfiguration config)
            {
                _context = context;
                _config = config;

                ArgumentNullException.ThrowIfNull(_config);

                if (!bool.TryParse(_config["LoadingData:IsMovieLimit"], out IsMovieLimit))
                {
                    ArgumentNullException.ThrowIfNull(IsMovieLimit);
                }

                if (!bool.TryParse(_config["LoadingData:IsSerialLimit"], out IsSerialLimit))
                {
                    ArgumentNullException.ThrowIfNull(IsSerialLimit);
                }

                if (!int.TryParse(_config["LoadingData:MaxMovies"], out MaxMovies))
                {
                    ArgumentNullException.ThrowIfNull(MaxMovies);
                }

                if (!int.TryParse(_config["LoadingData:MaxSerials"], out MaxSerials))
                {
                    ArgumentNullException.ThrowIfNull(MaxSerials);
                }
            }

            public void Load() 
            {
                if (GetDataFromPath(_config["LoadingData:DataPath"])) return;

                if (GetDataFromURL()) return;

                GetSampleData();
            }

            private bool GetDataFromPath(string dataPath) 
            {
                ArgumentNullException.ThrowIfNull(dataPath);
                if (string.IsNullOrEmpty(dataPath))
                {
                    return false;
                }

                if (!File.Exists(dataPath) || Path.GetExtension(dataPath) == ".zip")
                {
                    return false;
                }

                try
                {
                    using (FileStream fileStream = new FileStream(dataPath, FileMode.Open, FileAccess.Read))
                    using (GZipStream gZipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                    using (StreamReader reader = new StreamReader(gZipStream)) 
                    {
                        ArgumentNullException.ThrowIfNull(reader);

                        while (!reader.EndOfStream)
                        {
                            if (IsMovieLimit && MaxMovies < CurrentMovies &&
                                IsSerialLimit && MaxSerials < CurrentSerials)
                            {
                                break;
                            }

                            string line = reader.ReadLine();
                            if (line is null) break;
                            ProcessLine(line);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log exception
                    if (CurrentMovies > 0 && CurrentSerials > 0) return true;
                    return false;
                }

                return true;
            }

            private bool GetDataFromURL()
            {
                if (string.IsNullOrEmpty(_config["LoadingData:DataURL"]))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(_config["LoadingData:DataPath"]))
                {
                    return false;
                }

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var task = Task.Run(() => client.GetAsync(_config["LoadingData:DataURL"]));
                        task.Wait();
                        HttpResponseMessage response = task.Result;

                        response.EnsureSuccessStatusCode();

                        using (Stream contentStream = response.Content.ReadAsStream(),
                            fileStream = new FileStream(_config["LoadingData:DataPath"], 
                            FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            contentStream.CopyTo(fileStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log exception
                    return false;
                }

                return GetDataFromPath(_config["LoadingData:DataPath"]);
            }

            private void GetSampleData()
            {
                _context.Movies.AddRange( new List<Movie>() {
                    new()
                    {
                        PrimaryTitle = "Miss Jerry",
                        OriginalTitle = "Miss Jerry",
                        IsAdult = false,
                        Year = 1894,
                        RuntimeMinutes = 45,
                        Genres = new List<string>() { "Romance" }
                    },
                    new()
                    {
                        PrimaryTitle = "Chico Albuquerque",
                        OriginalTitle = "Chico Albuquerque",
                        IsAdult = false,
                        Year = 2013,
                        RuntimeMinutes = 49,
                        Genres = new List<string>() { "Documentary" }
                    },
                    new()
                    {

                        PrimaryTitle = "6 Gun",
                        OriginalTitle = "6 Gun",
                        IsAdult = false,
                        Year = 2017,
                        RuntimeMinutes = 116,
                        Genres = new List<string>() { "Drama" }
                    }
                });

                _context.Serials.AddRange( new List<Serial>() {
                    new()
                    {
                        PrimaryTitle = "Acelerados",
                        OriginalTitle = "Acelerados",
                        IsAdult = false,
                        StartYear = 2019,
                        EndYear = null,
                        RuntimeMinutes = null,
                        Genres = new List<string>() { "Comedy" }
                    },
                    new()
                    {
                        PrimaryTitle = "Meie aasta Aafrikas",
                        OriginalTitle = "Meie aasta Aafrikas",
                        IsAdult = false,
                        StartYear = 2019,
                        EndYear = null,
                        RuntimeMinutes = 43,
                        Genres = new List<string>() { "Adventure", "Comedy", "Family" }
                    },
                    new()
                    {
                        PrimaryTitle = "Atkexotics",
                        OriginalTitle = "Atkexotics",
                        IsAdult = true,
                        StartYear = 2003,
                        EndYear = null,
                        RuntimeMinutes = null,
                        Genres = new List<string>() { "Adult", "Short" }
                    }
                });
            }

            private void ProcessLine(string line) 
            {
                string[] fields = line.Split('\t');

                VideoObjectType videoObjectType;

                if (!Enum.TryParse(fields[1], true, out videoObjectType))
                {
                    return;
                }

                switch (videoObjectType)
                {
                    case VideoObjectType.movie:
                    case VideoObjectType.tvMovie:
                    case VideoObjectType.video:
                        if (IsMovieLimit && MaxMovies < CurrentMovies) 
                        {
                            break;
                        }

                        _context.Movies.Add(new Movie()
                        {
                            PrimaryTitle = fields[2] != @"\N" ? fields[2] : null,
                            OriginalTitle = fields[3],
                            IsAdult = fields[4] == "1",
                            Year = fields[5] != @"\N" ? int.Parse(fields[5]) : null,
                            RuntimeMinutes = fields[7] != @"\N" ? int.Parse(fields[7]) : null,
                            Genres = fields[8] != @"\N" ? new List<string>(fields[8].Split(',')) : null
                        });

                        CurrentMovies++;
                        break;
                    case VideoObjectType.tvSeries:
                    case VideoObjectType.tvMiniSeries:
                        if (IsSerialLimit && MaxSerials < CurrentSerials)
                        {
                            break;
                        }

                        _context.Serials.Add(new Serial()
                        {
                            PrimaryTitle = fields[2] != @"\N" ? fields[2] : null,
                            OriginalTitle = fields[3],
                            IsAdult = fields[4] == "1",
                            StartYear = fields[5] != @"\N" ? int.Parse(fields[5]) : null,
                            EndYear = fields[6] != @"\N" ? int.Parse(fields[6]) : null,
                            RuntimeMinutes = fields[7] != @"\N" ? int.Parse(fields[7]) : null,
                            Genres = fields[8] != @"\N" ? new List<string>(fields[8].Split(',')) : null
                        });

                        CurrentSerials++;
                        break;
                    default:
                        break;
                }
            }

            public enum VideoObjectType
            {
                movie = 1,
                tvMovie = 2,
                video = 3,
                tvSeries = 4,
                tvMiniSeries = 5
            }
        }
    }

}
