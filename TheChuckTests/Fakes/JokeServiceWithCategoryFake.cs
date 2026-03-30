using TheChuck.Core;

namespace TheChuckTests.Fakes
{
    /// <summary>
    /// Returnerar olika skämt beroende på om GetJokeFromCategory eller GetRandomJoke anropas.
    /// Gör det möjligt att i test verifiera VILKEN metod som faktiskt anropades.
    /// </summary>
    internal class JokeServiceWithCategoryFake : IJokeService
    {
        public Task<Joke?> GetJokeFromCategory(string category)
        {
            return Task.FromResult<Joke?>(new Joke { Value = $"Joke from category {category}" });
        }

        public Task<Joke?> GetRandomJoke()
        {
            return Task.FromResult<Joke?>(new Joke { Value = "Random joke" });
        }
    }
}
