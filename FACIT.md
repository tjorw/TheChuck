# Facit – The Chuck

Använd det här dokumentet för att rätta dig själv när du är klar med laborationen.
Poängen med självbedömning är inte att få rätt svar inmatade – det är att förstå *varför* svaret är rätt. Läs förklaringarna även när du hade rätt.

---

## Del 1 – Driftsättning

### Vad händer om ett test misslyckas – driftsätts applikationen ändå?

**Nej.** GitHub Actions avbryter körningen så fort ett steg misslyckas. Eftersom `dotnet test` körs före deploy-steget når trasig kod aldrig servern. Det är en av de viktigaste poängerna med CI/CD: du kan inte råka driftsätta kod som inte passerar testerna.

Hade du det rätt? Tänk på: det räcker inte att säga "nej" – kan du förklara *varför* det fungerar så? (Svar: stegen körs sekventiellt och varje steg måste returnera exitkod 0 för att nästa ska starta.)

---

### I vilken ordning körs stegen, och varför är den ordningen logisk?

| Steg | Varför just här |
|------|-----------------|
| `checkout` | Koden måste finnas lokalt – ingenting annat kan göras utan den |
| `setup-dotnet` | Verktyget måste vara installerat innan det används |
| `dotnet restore` | Laddar ned NuGet-paket som koden beror på |
| `dotnet build --no-restore` | Kompilerar; `--no-restore` undviker att ladda ned paket en gång till |
| `dotnet publish` | Skapar det deployrbara artefaktet; kräver ett lyckat bygge |
| `dotnet test` | Kör testerna; körs *efter* publish men *innan* deploy |
| Deploy | Skickar artefaktet till servern; körs sist eftersom alla föregående måste lyckas |

En bra tumregel: billiga operationer (kompilering, tester) körs före dyra (driftsättning). Man vill "fail fast" – hellre avbryta tidigt än att deploya och sedan inse att testerna var röda.

---

### Varför används GitHub Secrets istället för lösenord direkt i YAML-filen?

YAML-filen är incheckad i git och synlig för alla med tillgång till repot – och för hela internet om repot är publikt. Secrets lagras krypterat i GitHub, syns aldrig i loggar och injiceras som miljövariabler bara under körning. De är också enkla att rotera utan att ändra ett enda tecken i koden.

---

## Del 2 – Kodstruktur och Clean Code

### Fråga 1: Varför är koden uppdelad i lager?

Varje projekt har ett tydligt ansvar:

- **`TheChuck.Core`** – domänmodellen `Joke`. Inga beroenden till något annat projekt. Kärnan ska vara stabil.
- **`TheChuck.Infrastructure`** – kommunikation med omvärlden. `JokeService` vet vilka URL:er som ska anropas. `WebClient` gör det faktiska HTTP-anropet och deserialiserar JSON.
- **`TheChuck`** – presentationslagret. Razor Pages visar data för användaren och ska inte innehålla affärslogik eller HTTP-anrop.

**Vad händer om man blandar?** Om `IndexModel` skulle skapa en `HttpClient` och anropa API:et direkt går det inte längre att testa sidan utan att göra ett riktigt nätverksanrop. Koden blir också svårare att återanvända och att ändra – en ändring i hur API:et anropas kräver att man rör presentationslagret.

Hade du med minst ett konkret exempel på vad som *går sönder* om lagren blandas? Det är skillnaden mellan att kunna rabbla uppdelningen och att faktiskt förstå den.

---

### Fråga 2: Vad är Dependency Injection och varför används ett interface?

Dependency Injection innebär att en klass inte skapar sina egna beroenden – de levereras utifrån. I `Program.cs` registreras bindningen:

```csharp
builder.Services.AddScoped<IJokeService, JokeService>();
```

ASP.NET Core skapar sedan en `JokeService` och skickar in den i `IndexModel`s konstruktor automatiskt. `IndexModel` ber om ett `IJokeService` (interfacet) – inte en `JokeService` (den konkreta klassen). Det innebär att man i testprojektet kan skicka in något helt annat utan att ändra ett tecken i `IndexModel`:

```csharp
var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));
```

Hade du med *varför* det är ett interface och inte en konkret klass? Det är kärnan i svaret.

---

### Fråga 3: Hur möjliggör interfaces testbarhet?

`IJokeService` definierar ett kontrakt utan att säga något om hur det uppfylls. Det ger tre utbytbara implementeringar:

| Klass | Används i | Beteende |
|-------|-----------|----------|
| `JokeService` | Produktion | Anropar `api.chucknorris.io` på riktigt |
| `JokeServiceFake` | Tester – lyckade scenarion | Returnerar ett hårdkodat skämt |
| `JokeServiceBrokenFake` | Tester – felscenarier | Kastar ett undantag |

Utan interfaces hade varje test gjort ett riktigt HTTP-anrop. Det är problematiskt av tre skäl:
1. Testerna blir långsamma
2. De är icke-deterministiska – resultatet beror på om API:et är uppe
3. Du kan inte kontrollera vad tjänsten returnerar, vilket gör det omöjligt att testa felhantering

Hade du med alla tre problemen, eller bara ett? Alla tre är relevanta.

---

### Fråga 4: Varför är HTTP-anropen i en separat `WebClient`-klass?

`JokeService` har ett ansvar: att veta *vilka URL:er* som ska anropas och vad svaret betyder.
`WebClient` har ett ansvar: att göra ett generiskt HTTP GET-anrop och deserialisera JSON.

Om HTTP-logiken låg i `JokeService` skulle man behöva ändra `JokeService` när man byter JSON-bibliotek (t.ex. från Newtonsoft till `System.Text.Json`). Det bryter mot Single Responsibility-principen – klassen har då två anledningar att ändras.

Dessutom kan `IWebClient` ersättas med ett fake-objekt i tester av `JokeService`, precis som `IJokeService` ersätts i tester av `IndexModel`. Mönstret är konsekvent genom hela kodbasen.

---

### Sekvensdiagram – vad det ska innehålla

Ditt diagram ska visa följande anropskedja:

```
Webbläsare
    → IndexModel.OnGet()
        → IJokeService.GetRandomJoke()           (via interface)
            → JokeService.GetRandomJoke()
                → IWebClient.Get<Joke>(url)      (via interface)
                    → WebClient.Get<Joke>(url)
                        → api.chucknorris.io     (HTTP GET)
                    ← Joke (JSON deserialiserat)
                ← Joke
            ← Joke
        ← Joke
    → DisplayText = joke.Value.ToUpper()
← HTML med skämtet
```

Nyckelpunkterna: anropen går via *interfaces* (inte konkreta klasser), svaret flödar tillbaka uppåt, och `DisplayText` sätts i `IndexModel` – inte i tjänstelagret.

---

## Del 3 – TDD

### Hur du bedömer din egen TDD-process

Innan du tittar på referensimplementationerna, svara ärligt på dessa frågor:

- [ ] Skrev du ett *rött* test innan du implementerade?
- [ ] Implementerade du minimal kod för att göra testet grönt – inte mer?
- [ ] Refaktorerade du efteråt medan testerna fortfarande var gröna?
- [ ] Använde du fake-objekt i alla tester (inget anrop till det riktiga API:et)?

Om du svarade nej på något av dem – det är okej, men reflektera över varför. TDD är en vana som tar tid att bygga.

---

### 3.1 – Skämt från kategori

**`JokeServiceFake` behövde uppdateras** så att `GetJokeFromCategory` inte längre kastar `NotImplementedException`:

```csharp
public Task<Joke?> GetJokeFromCategory(string category)
{
    return Task.FromResult<Joke?>(joke);
}
```

**Referensimplementation – `IndexModel.OnGet()`:**

```csharp
public async Task OnGet()
{
    try
    {
        var joke = string.IsNullOrEmpty(Category)
            ? await _jokeService.GetRandomJoke()
            : await _jokeService.GetJokeFromCategory(Category);

        DisplayText = joke?.Value ?? "";
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, ex.Message);
        DisplayText = "Något gick fel. Försök igen lite senare.";
    }

    DisplayText = DisplayText.ToUpper();
}
```

**Tester du borde ha:**

```csharp
[TestMethod]
public async Task OnGet_WithCategory_ShouldDisplayJokeFromCategory()
{
    var joke = new Joke() { Value = "Category joke" };
    var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));
    sut.Category = "science";

    await sut.OnGet();

    Assert.AreEqual("CATEGORY JOKE", sut.DisplayText);
}

[TestMethod]
public async Task OnGet_WithoutCategory_ShouldDisplayRandomJoke()
{
    var joke = new Joke() { Value = "Random joke" };
    var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));

    await sut.OnGet();

    Assert.AreEqual("RANDOM JOKE", sut.DisplayText);
}

[TestMethod]
public async Task OnGet_WithCategory_WhenServiceFails_ShouldDisplayErrorMessage()
{
    var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceBrokenFake());
    sut.Category = "science";

    await sut.OnGet();

    Assert.AreEqual("Något gick fel. Försök igen lite senare.".ToUpper(), sut.DisplayText.ToUpper());
}
```

**Vanliga misstag:**
- Felhanteringen täcker bara `GetRandomJoke` men inte `GetJokeFromCategory` – kolla att `try/catch` omsluter hela blocket
- `GetJokeFromCategory` i `JokeServiceFake` kastade fortfarande `NotImplementedException` när testerna kördes

---

### Extrauppgift – testar testerna verkligen rätt sak?

**Nej – testet bevisar inte att `GetJokeFromCategory` anropas.**

Det beror på hur `JokeServiceFake` är implementerad. Eftersom den returnerar *samma* joke från både `GetRandomJoke` och `GetJokeFromCategory`, skulle testet vara grönt även om `IndexModel` ignorerar `Category` och alltid anropar `GetRandomJoke`. Testet verifierar bara att `DisplayText` innehåller rätt text – inte *vilken väg* koden tog.

Prova själv: ta bort villkoret i `OnGet()` och anropa alltid `GetRandomJoke()`. Testet blir fortfarande grönt.

**Hur fixar man det?**

Det enklaste sättet utan externa bibliotek är att ge `JokeServiceFake` två separata skämt – ett för varje metod – och sedan verifiera att *rätt* skämt visas:

```csharp
internal class JokeServiceFake : IJokeService
{
    private readonly Joke _randomJoke;
    private readonly Joke _categoryJoke;

    public JokeServiceFake(Joke randomJoke, Joke categoryJoke)
    {
        _randomJoke = randomJoke;
        _categoryJoke = categoryJoke;
    }

    public Task<Joke?> GetRandomJoke() => Task.FromResult<Joke?>(_randomJoke);
    public Task<Joke?> GetJokeFromCategory(string category) => Task.FromResult<Joke?>(_categoryJoke);
}
```

Testet blir nu meningsfullt – det *kan* bara bli grönt om rätt metod anropas:

```csharp
[TestMethod]
public async Task OnGet_WithCategory_ShouldDisplayJokeFromCategory()
{
    var randomJoke = new Joke() { Value = "Random joke" };
    var categoryJoke = new Joke() { Value = "Category joke" };
    var sut = new IndexModel(NullLogger<IndexModel>.Instance,
        new JokeServiceFake(randomJoke, categoryJoke));
    sut.Category = "science";

    await sut.OnGet();

    Assert.AreEqual("CATEGORY JOKE", sut.DisplayText); // misslyckas om GetRandomJoke anropades
}
```

**Vad lär vi oss av det här?**

Ett test som alltid är grönt oavsett implementation ger falsk trygghet – det verifierar ingenting. Det kallas ett *meningslöst test*. Bra tester ska kunna *misslyckas* om koden är fel. Det är därför man skriver det röda testet först i TDD: om testet aldrig kan bli rött kan det inte heller bevisa att implementationen fungerar.

---

### 3.2 – Byt ut namn i skämtet

**Ordningen är avgörande.** Ersättningen måste ske *innan* `ToUpper()` – annars söker du efter `"Chuck Norris"` i en sträng som redan är `"CHUCK NORRIS"` och ingenting ersätts.

**Referensimplementation – tillägg i `OnGet()` efter `DisplayText = joke?.Value ?? ""`:**

```csharp
if (!string.IsNullOrEmpty(Who))
    DisplayText = DisplayText.Replace("Chuck Norris", Who);

DisplayText = DisplayText.ToUpper();
```

**Tester du borde ha:**

```csharp
[TestMethod]
public async Task OnGet_WithWho_ShouldReplaceChuckNorrisWithWho()
{
    var joke = new Joke() { Value = "Chuck Norris can divide by zero" };
    var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));
    sut.Who = "Björn";

    await sut.OnGet();

    StringAssert.Contains(sut.DisplayText, "BJÖRN");
    StringAssert.DoesNotMatch(sut.DisplayText,
        new System.Text.RegularExpressions.Regex("CHUCK NORRIS"));
}

[TestMethod]
public async Task OnGet_WithoutWho_ShouldNotModifyJokeText()
{
    var joke = new Joke() { Value = "Chuck Norris can divide by zero" };
    var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));

    await sut.OnGet();

    StringAssert.Contains(sut.DisplayText, "CHUCK NORRIS");
}

[TestMethod]
public async Task OnGet_WithWho_ResultShouldBeUppercase()
{
    var joke = new Joke() { Value = "Chuck Norris can divide by zero" };
    var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));
    sut.Who = "Björn";

    await sut.OnGet();

    Assert.AreEqual(sut.DisplayText, sut.DisplayText.ToUpper());
}
```

**Vanliga misstag:**
- `ToUpper()` körs före `Replace()` – ingenting ersätts eftersom `"Chuck Norris" != "CHUCK NORRIS"`
- Testet kontrollerar bara att `Who` finns i texten, men inte att `"CHUCK NORRIS"` är *borta*

---

### 3.3 – Räkna ord

**Referensimplementation:**

Ny egenskap i `IndexModel`:
```csharp
public int WordCount { get; private set; }
```

Beräkning i `OnGet()`, efter all annan textbearbetning:
```csharp
WordCount = string.IsNullOrWhiteSpace(DisplayText)
    ? 0
    : DisplayText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
```

**Tester du borde ha:**

```csharp
[TestMethod]
public async Task OnGet_ShouldCountWordsInDisplayText()
{
    var joke = new Joke() { Value = "Chuck Norris can divide by zero" }; // 6 ord
    var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));

    await sut.OnGet();

    Assert.AreEqual(6, sut.WordCount);
}

[TestMethod]
public async Task OnGet_WhenServiceFails_WordCountShouldMatchErrorMessage()
{
    var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceBrokenFake());

    await sut.OnGet();

    // "Något gick fel. Försök igen lite senare." = 6 ord
    Assert.AreEqual(6, sut.WordCount);
}

[TestMethod]
public async Task OnGet_WordCountShouldReflectDisplayTextAfterTransformations()
{
    var joke = new Joke() { Value = "Chuck Norris wins" }; // 3 ord
    var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));
    sut.Who = "Björn Borg"; // ersätter 2 ord med 2 ord – fortfarande 3 ord

    await sut.OnGet();

    Assert.AreEqual(3, sut.WordCount);
}
```

**Vanliga misstag:**
- Räknar ord på `joke.Value` istället för `DisplayText` – testet för transformationer avslöjar detta
- `WordCount` beräknas innan `Who`-ersättningen eller `ToUpper()` – spelar ingen roll för antalet ord, men visar att man inte tänkt på var i flödet beräkningen sker
- Hanterar inte extra mellanslag – `StringSplitOptions.RemoveEmptyEntries` löser det

---

## Sammanfattning – vad förstod du?

Gå igenom dessa påståenden och bedöm dig själv ärligt:

| Påstående | Ja | Delvis | Nej |
|-----------|:--:|:------:|:---:|
| Jag kan förklara varför pipelinen inte deployar vid ett misslyckat test | | | |
| Jag kan förklara skillnaden mellan ett interface och en konkret klass | | | |
| Jag förstår varför `JokeServiceFake` finns och vad den ersätter | | | |
| Jag skrev tester *innan* jag implementerade (TDD-cykeln) | | | |
| Mina tester testar *beteende*, inte implementationsdetaljer | | | |
| Alla tester – befintliga och mina egna – är gröna (`dotnet test`) | | | |
