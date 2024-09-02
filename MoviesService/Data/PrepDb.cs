using MoviesService.Models;
using Serilog;
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
                    Log.Information("Attempting to apply migrations");
                    // context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Log.Error($"Could not run migrations: {ex.Message}");
                }
            }

            if (context.Movies.Any() && context.Serials.Any()) return;

            DataLoader dataLoader = new(context, config);

            Log.Information("Loading data...");
            dataLoader.Load();

            Log.Information("Saving data...");
            context.SaveChanges();
            Log.Information("Data loaded");
        }

        private class DataLoader
        {
            private readonly AppDbContext _context;
            private readonly IConfiguration _config;
            private readonly string filePath;
            private readonly bool isMovieLimit;
            private readonly bool isSerialLimit;
            private readonly int maxMovies;
            private readonly int maxSerials;
            private int CurrentMovies = 0;
            private int CurrentSerials = 0;

            public DataLoader(AppDbContext context, IConfiguration config)
            {
                _context = context;
                _config = config;

                ArgumentNullException.ThrowIfNull(_config);

                if (!bool.TryParse(_config["LoadingData:IsMovieLimit"], out isMovieLimit))
                {
                    ArgumentNullException.ThrowIfNull(isMovieLimit);
                }

                if (!bool.TryParse(_config["LoadingData:IsSerialLimit"], out isSerialLimit))
                {
                    ArgumentNullException.ThrowIfNull(isSerialLimit);
                }

                if (!int.TryParse(_config["LoadingData:MaxMovies"], out maxMovies))
                {
                    ArgumentNullException.ThrowIfNull(maxMovies);
                }

                if (!int.TryParse(_config["LoadingData:MaxSerials"], out maxSerials))
                {
                    ArgumentNullException.ThrowIfNull(maxSerials);
                }

                ArgumentException.ThrowIfNullOrEmpty(_config["LoadingData:DataFileName"]);
                ArgumentException.ThrowIfNullOrEmpty(_config["LoadingData:DownloadFolder"]);

                filePath = Path.Combine(Directory.GetCurrentDirectory(), _config["LoadingData:DownloadFolder"], _config["LoadingData:DataFileName"]);
            }

            public void Load() 
            {
                Log.Information("Getting data from file...");
                if (GetDataFromPath()) return;
                
                Log.Information("No file found, downloading data...");
                if (GetDataFromURL()) return;

                Log.Information("Could not get data from URL, creating sample data...");
                GetSampleData();
            }

            private bool GetDataFromPath() 
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return false;
                }

                if (!File.Exists(filePath) || Path.GetExtension(filePath) == ".zip")
                {
                    return false;
                }

                try
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (GZipStream gZipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                    using (StreamReader reader = new StreamReader(gZipStream)) 
                    {
                        ArgumentNullException.ThrowIfNull(reader);

                        while (!reader.EndOfStream)
                        {
                            if (isMovieLimit && maxMovies < CurrentMovies &&
                                isSerialLimit && maxSerials < CurrentSerials)
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
                    Log.Error($"Could not open a file: {ex.Message}");
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

                if (string.IsNullOrEmpty(filePath))
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
                            fileStream = new FileStream(filePath, 
                            FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            contentStream.CopyTo(fileStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Could not download file: {ex.Message}");
                    return false;
                }

                return GetDataFromPath();
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
                        if (isMovieLimit && maxMovies < CurrentMovies) 
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
                        if (isSerialLimit && maxSerials < CurrentSerials)
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
