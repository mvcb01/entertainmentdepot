using FilmDomain.Entities;
using FilmDomain.Interfaces;

namespace MovieAPIClients
{
    public class MovieActorResult : IExternalEntity
    {
        public int ExternalId { get; set; }

        public string Name { get; set; }

        public static explicit operator CastMember(MovieActorResult movieActorResult)
        {
            return new CastMember() {
                ExternalId = movieActorResult.ExternalId,
                Name = movieActorResult.Name
            };
        }
    }
}