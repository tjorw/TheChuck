using TheChuck.Core;

namespace TheChuckTests.Fakes
{
    internal class JokeServiceFake : IJokeService
    {
        private readonly Joke joke;

        public JokeServiceFake(Joke joke)
        {
            this.joke = joke;
        }

        public Task<Joke?> GetJokeFromCategory(string category)
        {
            return Task.FromResult<Joke?>(joke);
        }

        public async Task<Joke?> GetRandomJoke()
        {
            return await Task.FromResult(joke);
        }
    }
}


