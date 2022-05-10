using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;

namespace MovieAPIClients.TheMovieDb
{
    public class TheMovieDbAPIClient
    {
        private string _apiKey { get; init; }

        private HttpClient _httpClient { get; init; }

        public const string MovieDbBaseAddress = "https://api.themoviedb.org/3/";

        public TheMovieDbAPIClient(string apiKey)
        {
            this._apiKey = apiKey;
            HttpClient client = new();
            client.BaseAddress = new Uri(MovieDbBaseAddress);
            this._httpClient = client;
        }

        public async Task<IEnumerable<MovieSearchResult>> SearchMovie(string title)
        {
            var movieTitle = title.Trim().ToLower();

            // exemplo: converte "where, art thou!" para o array ["where", "art", "thou"]
            char[] punctuation = title.Where(Char.IsPunctuation).Distinct().ToArray();
            string[] titleWords = movieTitle.Split().Select(s => s.Trim(punctuation)).ToArray();

            // para o search da query string, por exemplo
            //      query=where+art+thou
            string searchQuery = string.Join('+', titleWords);

            // por enquanto fica martelada a primeira página no fim da query string
            string resultString = await _httpClient.GetStringAsync($"search/movie?api_key={_apiKey}&query={searchQuery}&page=1");

            var searchResult = JsonSerializer.Deserialize<SearchResult>(resultString);
            return searchResult.Results.Where(r => r.Title.Trim().ToLower() == movieTitle);
        }

    }
}
