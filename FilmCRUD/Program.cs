﻿using CommandLine;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using FilmDataAccess.EFCore.UnitOfWork;
using FilmDomain.Entities;
using FilmDomain.Interfaces;
using FilmDataAccess.EFCore;
using FilmCRUD.Verbs;
using FilmCRUD.Interfaces;
using FilmCRUD.Helpers;
using ConfigUtils;
using ConfigUtils.Interfaces;
using MovieAPIClients.Interfaces;
using MovieAPIClients.TheMovieDb;


namespace FilmCRUD
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ServiceCollection services = new();
            ConfigureServices(services);
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            IUnitOfWork unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            IFileSystemIOWrapper fileSystemIOWrapper = serviceProvider.GetRequiredService<IFileSystemIOWrapper>();
            IAppSettingsManager appSettingsManager = serviceProvider.GetRequiredService<IAppSettingsManager>();
            IMovieAPIClient movieAPIClient = serviceProvider.GetRequiredService<IMovieAPIClient>();

            VisitCRUDManager visitCrudManager = new(unitOfWork, fileSystemIOWrapper, appSettingsManager);
            ScanRipsManager scanRipsManager = new(unitOfWork);
            RipToMovieLinker ripToMovieLinker = new(unitOfWork, fileSystemIOWrapper, appSettingsManager, movieAPIClient);
            ScanMoviesManager scanMoviesManager = new(unitOfWork);

            ParserResult<object> parsed = Parser
                .Default
                .ParseArguments<VisitOptions, ScanRipsOptions, ScanMoviesOptions, LinkOptions, FetchOptions>(args);
            parsed.WithParsed<VisitOptions>(opts => HandleVisitOptions(opts, visitCrudManager));
            parsed.WithParsed<ScanRipsOptions>(opts => HandleScanRipsOptions(opts, scanRipsManager));
            parsed.WithParsed<ScanMoviesOptions>(opts => HandleScanMoviesOptions(opts, scanMoviesManager));
            await parsed.WithParsedAsync<LinkOptions>(async opts => await HandleLinkOptions(opts, ripToMovieLinker));
            await parsed.WithParsedAsync<FetchOptions>(async opts => await HandleFetchOptions(opts, unitOfWork, movieAPIClient));
            parsed.WithNotParsed(HandleParseError);

            {}
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IUnitOfWork, SQLiteUnitOfWork>(_ => new SQLiteUnitOfWork(new SQLiteAppContext()));

            services.AddSingleton<IFileSystemIOWrapper, FileSystemIOWrapper>();

            services.AddSingleton<IAppSettingsManager, AppSettingsManager>();

            AppSettingsManager _appSettingsManager = new();
            string apiKey = _appSettingsManager.GetApiKey("TheMovieDb");
            services.AddSingleton<IMovieAPIClient, TheMovieDbAPIClient>(_ => new TheMovieDbAPIClient(apiKey));
        }

        private static void HandleVisitOptions(VisitOptions visitOpts, VisitCRUDManager visitCrudManager)
        {
            if (visitOpts.ListContents)
            {
                System.Console.WriteLine("-------------");
                System.Console.WriteLine($"Movie warehouse: {visitCrudManager.MovieWarehouseDirectory}");
                System.Console.WriteLine("Garantir que o disco está ligado... Y para sim, outra para não.");
                bool toContinue = Console.ReadLine().Trim().ToLower() == "y";
                if (!toContinue)
                {
                    System.Console.WriteLine("A sair...");
                    return;
                }
                visitCrudManager.WriteMovieWarehouseContentsToTextFile();
            }
            else if (!string.IsNullOrEmpty(visitOpts.PersistContents))
            {
                visitCrudManager.ReadWarehouseContentsAndRegisterVisit(visitOpts.PersistContents, failOnParsingErrors: false);
            }
            else
            {
                System.Console.WriteLine("No action requested...");
            }
        }

        private static void HandleScanRipsOptions(ScanRipsOptions opts, ScanRipsManager scanRipsManager)
        {
            System.Console.WriteLine("----------");
            if (opts.CountByReleaseDate)
            {
                System.Console.WriteLine("Scan: contagem por ReleaseDate\n");
                Dictionary<string, int> countByRelaseDate = scanRipsManager.GetRipCountByReleaseDate();
                foreach (var kv in countByRelaseDate.OrderBy(kv => kv.Key))
                {
                    System.Console.WriteLine($"{kv.Key}: {kv.Value}");
                }
            }
            else if (opts.WithReleaseDate != null)
            {
                System.Console.WriteLine($"Scan: rips com ReleaseDate {opts.WithReleaseDate}\n");
                List<string> ripFileNames = scanRipsManager.GetAllRipsWithReleaseDate(opts.WithReleaseDate).ToList();

                System.Console.WriteLine($"Contagem: {ripFileNames.Count()}\n");

                foreach (var fileName in ripFileNames.OrderBy(r => r))
                {
                    System.Console.WriteLine(fileName);
                }
            }
            else if (opts.CountByVisit)
            {
                System.Console.WriteLine("Scan: contagem por visita\n");
                Dictionary<DateTime, int> countByVisit = scanRipsManager.GetRipCountByVisit();
                foreach (var item in countByVisit.OrderBy(kvp => kvp.Key))
                {
                    string visitStr = item.Key.ToString("yyyyMMdd");
                    System.Console.WriteLine($"{visitStr} : {item.Value}");
                }
            }
            else if (opts.LastVisitDiff)
            {
                System.Console.WriteLine("Scan: diff da última visita\n");
                Dictionary<string, IEnumerable<string>> lastVisitDiff = scanRipsManager.GetLastVisitDiff();
                foreach (var item in lastVisitDiff.OrderBy(kvp => kvp.Key))
                {
                    System.Console.WriteLine("\n----------");
                    System.Console.WriteLine(item.Key + "\n");
                    System.Console.WriteLine(String.Join('\n', item.Value.OrderBy(s => s)));
                }
            }
            else
            {
                System.Console.WriteLine("No action requested...");
            }
            System.Console.WriteLine();
        }

        private static void HandleScanMoviesOptions(ScanMoviesOptions opts, ScanMoviesManager scanMoviesManager)
        {
            System.Console.WriteLine("-------------");

            if (opts.ListVisits)
            {
                System.Console.WriteLine("Dates for all warehouse visits:");
                scanMoviesManager.ListVisitDates()
                    .OrderByDescending(dt => dt)
                    .ToList()
                    .ForEach(dt => System.Console.WriteLine(dt.ToString("yyyyMMdd")));
                return;
            }

            MovieWarehouseVisit visit;
            if (opts.Visit == null)
            {
                visit = scanMoviesManager.GetClosestVisit();
            }
            else
            {
                DateTime visitDate = DateTime.ParseExact(opts.Visit, "yyyyMMdd", null);
                visit = scanMoviesManager.GetClosestVisit(visitDate);
            }

            string printDateFormat = "MMMM dd yyyy";
            System.Console.WriteLine($"Visit: {visit.VisitDateTime.ToString(printDateFormat)}");
            if (opts.WithGenres.Any())
            {
                // finds the Genre entities for each string in opts.WithGenres, then flattens
                IEnumerable<Genre> genres = opts.WithGenres
                    .Select(name => scanMoviesManager.GenresFromName(name))
                    .SelectMany(g => g);
                IEnumerable<Movie> moviesWithGenres = scanMoviesManager.GetMoviesWithGenres(visit, genres.ToArray());
                string genreNames = string.Join(" | ", genres.Select(g => g.Name));
                System.Console.WriteLine($"Movies with genres: {genreNames} \n");
                moviesWithGenres.ToList().ForEach(m => System.Console.WriteLine(m));
            }
            else if (opts.WithActors.Any())
            {
                // finds the Actor entities for each string in opts.WithActors, then flattens
                IEnumerable<Actor> actors = opts.WithActors
                    .Select(name => scanMoviesManager.GetActorsFromName(name))
                    .SelectMany(a => a);
                IEnumerable<Movie> moviesWithActors = scanMoviesManager.GetMoviesWithActors(visit, actors.ToArray());
                string actorNames = string.Join(" | ", actors.Select(a => a.Name));
                System.Console.WriteLine($"Movies with actors: {actorNames} \n");
                moviesWithActors.ToList().ForEach(m => System.Console.WriteLine(m));
            }
            else if (opts.WithDirectors.Any())
            {
                // finds the Director entities for each string in opts.WithDirectors, then flattens
                IEnumerable<Director> directors = opts.WithDirectors
                    .Select(name => scanMoviesManager.GetDirectorsFromName(name))
                    .SelectMany(a => a);
                IEnumerable<Movie> moviesWithDirectors = scanMoviesManager.GetMoviesWithDirectors(visit, directors.ToArray());
                string directorNames = string.Join(" | ", directors.Select(d => d.Name));
                System.Console.WriteLine($"Movies with directors: {directorNames} \n");
                moviesWithDirectors.ToList().ForEach(m => System.Console.WriteLine(m));
            }
            else if (opts.ByGenre)
            {
                System.Console.WriteLine("Count by genre:\n");
                IEnumerable<KeyValuePair<Genre, int>> genreCount = scanMoviesManager.GetCountByGenre(visit);
                int toTake = opts.Top ?? genreCount.Count();
                genreCount.OrderByDescending(kvp => kvp.Value).ThenBy(kvp => kvp.Key.Name)
                    .Take(toTake)
                    .ToList()
                    .ForEach(kvp => System.Console.WriteLine($"{kvp.Key.Name}: {kvp.Value}"));
            }
            else if (opts.ByActor)
            {
                System.Console.WriteLine("Count by actor:\n");
                IEnumerable<KeyValuePair<Actor, int>> actorCount = scanMoviesManager.GetCountByActor(visit);
                int toTake = opts.Top ?? actorCount.Count();
                actorCount.OrderByDescending(kvp => kvp.Value).ThenBy(kvp => kvp.Key.Name)
                    .Take(toTake)
                    .ToList()
                    .ForEach(kvp => System.Console.WriteLine($"{kvp.Key.Name}: {kvp.Value}"));
            }
            else if (opts.ByDirector)
            {
                System.Console.WriteLine("Count by director:\n");
                IEnumerable<KeyValuePair<Director, int>> directorCount = scanMoviesManager.GetCountByDirector(visit);
                int toTake = opts.Top ?? directorCount.Count();
                directorCount.OrderByDescending(kvp => kvp.Value).ThenBy(kvp => kvp.Key.Name)
                    .Take(toTake)
                    .ToList()
                    .ForEach(kvp => System.Console.WriteLine($"{kvp.Key.Name}: {kvp.Value}"));
            }
            else
            {
                System.Console.WriteLine("No action requested...");
            }
            System.Console.WriteLine();
        }
        private static async Task HandleLinkOptions(LinkOptions opts, RipToMovieLinker ripToMovieLinker)
        {
            System.Console.WriteLine("-------------");
            if (opts.Search)
            {
                System.Console.WriteLine($"A linkar...");
                await ripToMovieLinker.SearchAndLinkAsync();
            }
            else if (opts.FromManualExtIds)
            {
                System.Console.WriteLine($"A linkar a partir de external ids manuais...");
                await ripToMovieLinker.LinkFromManualExternalIdsAsync();
            }
            else if (opts.GetUnlinkedRips)
            {
                IEnumerable<string> unlinked = ripToMovieLinker.GetAllUnlinkedMovieRips();
                System.Console.WriteLine($"MovieRips não linkados:");
                System.Console.WriteLine();
                unlinked.ToList().ForEach(s => System.Console.WriteLine(s));

            }
            else if (opts.ValidateManualExtIds)
            {
                System.Console.WriteLine($"A validar external ids manuais:");
                System.Console.WriteLine();
                Dictionary<string, Dictionary<string, int>> validStatus = await ripToMovieLinker.ValidateManualExternalIdsAsync();
                foreach (var item in validStatus)
                {
                    Dictionary<string, int> innerDict = item.Value;

                    System.Console.WriteLine(item.Key);
                    IEnumerable<string> linesToPrint = innerDict.Select(kvp => $"{kvp.Key} : {kvp.Value}");
                    System.Console.WriteLine(string.Join('\n', linesToPrint));
                    System.Console.WriteLine();
                }
            }
            else
            {
                System.Console.WriteLine("No action requested...");
            }
            System.Console.WriteLine();

        }

        public static async Task HandleFetchOptions(FetchOptions opts, IUnitOfWork unitOfWork, IMovieAPIClient movieAPIClient)
        {
            if (opts.Genres)
            {
                var genresFetcher = new MovieDetailsFetcherGenres(unitOfWork, movieAPIClient);
                System.Console.WriteLine("fetching genres for movies...");
                await genresFetcher.PopulateDetails();
            }
            else if (opts.Actors)
            {
                var actorsFetcher = new MovieDetailsFetcherActors(unitOfWork, movieAPIClient);
                System.Console.WriteLine("fetching actors for movies...");
                await actorsFetcher.PopulateDetails();
            }
            else if (opts.Directors)
            {
                var directorsFetcher = new MovieDetailsFetcherDirectors(unitOfWork, movieAPIClient);
                System.Console.WriteLine("fetching directors for movies...");
                await directorsFetcher.PopulateDetails();
            }
            else if (opts.Keywords)
            {
                var keywordsFetcher = new MovieDetailsFetcherSimple(unitOfWork, movieAPIClient);
                System.Console.WriteLine("fetching keywords for movies...");
                await keywordsFetcher.PopulateMovieKeywords();
            }
            else if (opts.IMDBIds)
            {
                var IMDBIdFetcher = new MovieDetailsFetcherSimple(unitOfWork, movieAPIClient);
                System.Console.WriteLine("fetching imdb ids for movies...");
                await IMDBIdFetcher.PopulateMovieIMDBIds();
            }
            else
            {
                System.Console.WriteLine("No fetch request...");
            }
            System.Console.WriteLine();
        }

        private static void HandleParseError(IEnumerable<Error> errors)
        {
            foreach (var errorObj in errors)
            {
                System.Console.WriteLine(errorObj.Tag);
            }
        }

    }
}
