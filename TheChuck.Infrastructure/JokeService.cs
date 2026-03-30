using TheChuck.Core;
using TheChuck.Infrastructure.Services;

namespace TheChuck.Infrastructure;

public class JokeService : IJokeService
{
    private readonly IWebClient _webClient;

    public JokeService(IWebClient webClient)
    {
        _webClient = webClient;
    }

    public async Task<Joke?> GetJokeFromCategory(string category)
    {
        // BUGG 4: Fel versalisering på query-parametern – API:et förväntar sig "category" (gemener),
        // inte "Category". Felet syns inte i enhetstester eftersom de använder fake-objekt.
        string url = "https://api.chucknorris.io/jokes/random?Category=" + category;

        try
        {
            return await _webClient.Get<Joke>(url) ?? new Joke();
        }
        catch (Exception ex)
        {
            throw new Exception("Det gick inte att hämta joke via kategori", ex);
        }
    }

    public async Task<Joke?> GetRandomJoke()
    {
        string url = "https://api.chucknorris.io/jokes/random";

        try
        {
            return await _webClient.Get<Joke>(url) ?? new Joke();
        }
        catch (Exception ex)
        {
            throw new Exception("Det gick inte att hämta joke", ex);
        }
    }

}
