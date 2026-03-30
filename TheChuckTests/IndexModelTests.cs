using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging.Abstractions;
using TheChuckTests.Fakes;
using TheChuck.Core;

namespace TheChuck.Pages.Tests
{
    [TestClass()]
    public class IndexModelTests
    {
        // ── Befintliga tester ────────────────────────────────────────────────────

        [TestMethod()]
        public async Task OnGet_ShouldDisplayTextFromService()
        {
            var joke = new Joke() { Value = "Works" };
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));

            await sut.OnGet();

            Assert.AreEqual("Works".ToUpper(), sut.DisplayText.ToUpper());
        }

        [TestMethod()]
        public async Task OnGet_ShouldDisplayTextTryAgainWhenApiIsNotWorking()
        {
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceBrokenFake());

            await sut.OnGet();

            Assert.AreEqual("Något gick fel. Försök igen lite senare.".ToUpper(), sut.DisplayText.ToUpper());
        }

        [TestMethod()]
        public async Task OnGet_ShouldBeUppecase()
        {
            var joke = new Joke() { Value = "Works" };
            var pageModel = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));

            await pageModel.OnGet();

            Assert.AreEqual("WORKS", pageModel.DisplayText);
        }

        // ── Tester för Category-funktionen ──────────────────────────────────────

        [TestMethod()]
        public async Task OnGet_WithCategory_ShouldDisplayJokeFromCategory()
        {
            // Använder JokeServiceWithCategoryFake som returnerar OLIKA text beroende på metod.
            // På så sätt kan vi verifiera att rätt metod faktiskt anropades.
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceWithCategoryFake());
            sut.Category = "science";

            await sut.OnGet();

            Assert.AreEqual("JOKE FROM CATEGORY SCIENCE", sut.DisplayText);
        }

        [TestMethod()]
        public async Task OnGet_WithoutCategory_ShouldDisplayRandomJoke()
        {
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceWithCategoryFake());

            await sut.OnGet();

            Assert.AreEqual("RANDOM JOKE", sut.DisplayText);
        }

        // ── Tester för Who-funktionen ────────────────────────────────────────────

        [TestMethod()]
        public async Task OnGet_WithWho_ShouldReplaceChuckNorrisWithWho()
        {
            var joke = new Joke() { Value = "Chuck Norris can divide by zero" };
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));
            sut.Who = "Björn";

            await sut.OnGet();

            Assert.IsTrue(sut.DisplayText.Contains("BJÖRN"),
                $"Förväntade att 'BJÖRN' skulle finnas i texten men fick: '{sut.DisplayText}'");
        }

        [TestMethod()]
        public async Task OnGet_WithWho_ChuckNorrisShouldNotRemainInText()
        {
            var joke = new Joke() { Value = "Chuck Norris can divide by zero" };
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));
            sut.Who = "Björn";

            await sut.OnGet();

            Assert.IsFalse(sut.DisplayText.Contains("CHUCK NORRIS"),
                $"Förväntade att 'CHUCK NORRIS' INTE skulle finnas kvar i texten men fick: '{sut.DisplayText}'");
        }

        /// <summary>
        /// FALSKT GRÖNT TEST – Det här testet verifierar ingenting meningsfullt.
        /// Det kontrollerar bara att DisplayText inte är tom, vilket alltid stämmer.
        /// Testet är grönt även fast Who-ersättningen är helt trasig.
        /// Kan du se vad som är fel och skriva ett test som faktiskt fångar buggen?
        /// </summary>
        [TestMethod()]
        public async Task OnGet_WithWho_DisplayTextShouldNotBeEmpty()
        {
            var joke = new Joke() { Value = "Chuck Norris is amazing" };
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));
            sut.Who = "Björn";

            await sut.OnGet();

            Assert.IsTrue(sut.DisplayText.Length > 0);
        }

        // ── Tester för WordCount-funktionen ─────────────────────────────────────

        [TestMethod()]
        public async Task OnGet_SingleWordJoke_WordCountShouldBeOne()
        {
            var joke = new Joke() { Value = "Word" };
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));

            await sut.OnGet();

            Assert.AreEqual(1, sut.WordCount,
                $"Förväntade WordCount = 1 men fick {sut.WordCount}");
        }

        [TestMethod()]
        public async Task OnGet_ThreeWordJoke_WordCountShouldBeThree()
        {
            var joke = new Joke() { Value = "One two three" };
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));

            await sut.OnGet();

            Assert.AreEqual(3, sut.WordCount,
                $"Förväntade WordCount = 3 men fick {sut.WordCount}");
        }

        [TestMethod()]
        public async Task OnGet_EmptyJoke_WordCountShouldBeZero()
        {
            var joke = new Joke() { Value = "" };
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));

            await sut.OnGet();

            Assert.AreEqual(0, sut.WordCount,
                $"Förväntade WordCount = 0 men fick {sut.WordCount}");
        }
    }
}
