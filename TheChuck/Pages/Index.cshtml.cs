using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TheChuck.Core;

namespace TheChuck.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IJokeService _jokeService;

        [BindProperty(SupportsGet = true)]
        public string? Who { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IJokeService jokeService)
        {
            _logger = logger;
            _jokeService = jokeService;
        }

        public string DisplayText { get; private set; } = "";
        public int WordCount { get; private set; }

        public async Task OnGet()
        {
            try
            {
                Joke? joke;

                // BUGG 1: Villkoret är inverterat – anropar GetJokeFromCategory när Category SAKNAS
                // och GetRandomJoke när Category ÄR satt. Borde vara !string.IsNullOrEmpty(Category).
                if (string.IsNullOrEmpty(Category))
                    joke = await _jokeService.GetJokeFromCategory(Category!);
                else
                    joke = await _jokeService.GetRandomJoke();

                DisplayText = joke?.Value ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                DisplayText = "Något gick fel. Försök igen lite senare.";
            }

            // BUGG 2: ToUpper() körs INNAN Who-ersättningen.
            // "Chuck Norris" i texten blir "CHUCK NORRIS" och Replace("Chuck Norris", Who) hittar inget.
            DisplayText = DisplayText.ToUpper();

            if (!string.IsNullOrEmpty(Who))
                DisplayText = DisplayText.Replace("Chuck Norris", Who);

            // BUGG 3: Off-by-one – Length - 1 ger fel antal ord (ett för lite).
            WordCount = DisplayText.Split(' ').Length - 1;
        }
    }
}
