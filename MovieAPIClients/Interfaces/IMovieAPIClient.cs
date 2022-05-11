using System.Threading.Tasks;
using System.Collections.Generic;

namespace MovieAPIClients.Interfaces
{
    public interface IMovieAPIClient
    {
        Task<IEnumerable<MovieSearchResult>> SearchMovieAsync(string title);

        Task<IEnumerable<string>> GetMovieGenresAsync(int externalId);
    }
}