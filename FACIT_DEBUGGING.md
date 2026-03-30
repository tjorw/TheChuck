# Facit – Debugging & Felsökning

Öppna det här dokumentet *efter* att du är klar. Poängen är inte att kopiera svaren – det är att förstå *varför* buggarna uppstår och vad de lär dig om att skriva robust kod.

---

## Uppgift 1 – Analysera failing tests

Kör `dotnet test` på den buggiga koden och du ska se ungefär sex tester som failar. Här är vad de berättar:

| Test | Förväntat | Faktiskt | Orsak |
|------|-----------|----------|-------|
| `OnGet_WithCategory_ShouldDisplayJokeFromCategory` | `"JOKE FROM CATEGORY SCIENCE"` | `"RANDOM JOKE"` | Bugg 1 – fel metod anropas |
| `OnGet_WithoutCategory_ShouldDisplayRandomJoke` | `"RANDOM JOKE"` | `"JOKE FROM CATEGORY "` | Bugg 1 – fel metod anropas |
| `OnGet_WithWho_ShouldReplaceChuckNorrisWithWho` | Text innehåller `"BJÖRN"` | `"BJÖRN"` saknas | Bugg 2 – Replace hittar fel |
| `OnGet_WithWho_ChuckNorrisShouldNotRemainInText` | `"CHUCK NORRIS"` borta | `"CHUCK NORRIS"` finns kvar | Bugg 2 – Replace hittar fel |
| `OnGet_SingleWordJoke_WordCountShouldBeOne` | `1` | `0` | Bugg 3 – off-by-one |
| `OnGet_ThreeWordJoke_WordCountShouldBeThree` | `3` | `2` | Bugg 3 – off-by-one |

**Nyckelinsikt:** Felmeddelandena i terminalen talar om *vad* som gick fel. Debuggern talar om *var* och *varför*. Du behöver båda.

---

## Uppgift 2 – Buggarna och hur man fixar dem

---

### Bugg 1 – Inverterat villkor i Category-logiken

**Var:** `IndexModel.cs`, i `OnGet()`.

**Vad som är fel:**

```csharp
// BUGGIG KOD
if (string.IsNullOrEmpty(Category))         // anropar GetJokeFromCategory när Category SAKNAS
    joke = await _jokeService.GetJokeFromCategory(Category!);
else                                         // anropar GetRandomJoke när Category ÄR satt
    joke = await _jokeService.GetRandomJoke();
```

Utropstecknet (`!`) på `Category!` är ett tecken på att något luktar fel – det berättar för kompilatorn "lita på mig, det här är inte null" trots att villkoret precis bekräftade att det *är* null eller tomt.

**Hur du hittar det med debuggern:**
1. Sätt en breakpoint på `if`-raden.
2. Surfa till `/?Category=science`.
3. Titta i **Variables**: `Category = "science"`.
4. `string.IsNullOrEmpty("science")` är `false` → koden tar `else`-grenen → anropar `GetRandomJoke()`. Fel!

**Fix:**

```csharp
if (!string.IsNullOrEmpty(Category))
    joke = await _jokeService.GetJokeFromCategory(Category);
else
    joke = await _jokeService.GetRandomJoke();
```

Eller mer kompakt:

```csharp
var joke = string.IsNullOrEmpty(Category)
    ? await _jokeService.GetRandomJoke()
    : await _jokeService.GetJokeFromCategory(Category);
```

---

### Bugg 2 – ToUpper() sker före Who-ersättning

**Var:** `IndexModel.cs`, i `OnGet()`.

**Vad som är fel:**

```csharp
// BUGGIG KOD – ordningen är fel
DisplayText = DisplayText.ToUpper();                         // "chuck norris wins" → "CHUCK NORRIS WINS"

if (!string.IsNullOrEmpty(Who))
    DisplayText = DisplayText.Replace("Chuck Norris", Who);  // letar efter "Chuck Norris" i "CHUCK NORRIS WINS"
                                                             // hittar ingenting – Replace är case-sensitive
```

`string.Replace` i C# är som standard skiftlägeskänsligt. `"CHUCK NORRIS WINS".Replace("Chuck Norris", "Björn")` ger `"CHUCK NORRIS WINS"` – oförändrat.

**Hur du hittar det med debuggern:**
1. Lägg `DisplayText` i **Watch**.
2. Sätt en breakpoint på `DisplayText = DisplayText.ToUpper()`.
3. Stega med `F10` och se hur `DisplayText` förändras rad för rad.
4. Du ser att `"Chuck Norris wins"` → `"CHUCK NORRIS WINS"` *innan* Replace körs. Replace hittar aldrig sin söktext.

**Fix:** Byt ordning – ersätt *innan* du versaliserar:

```csharp
if (!string.IsNullOrEmpty(Who))
    DisplayText = DisplayText.Replace("Chuck Norris", Who);

DisplayText = DisplayText.ToUpper();
```

**Varför spelar ordningen roll?** Texten är antingen skriven med blandad versalisering (API-svaret) eller helt versaliserad (din `ToUpper()`). Ersättningen måste matcha texten *som den ser ut just då*. Gör ersättningen alltid innan du transformerar formatet.

---

### Bugg 3 – Off-by-one i WordCount

**Var:** `IndexModel.cs`, sista raden i `OnGet()`.

**Vad som är fel:**

```csharp
// BUGGIG KOD
WordCount = DisplayText.Split(' ').Length - 1;
```

`"WORD".Split(' ')` ger arrayen `["WORD"]` med `Length = 1`. Sedan dras 1 av → `WordCount = 0`. Fel.
`"ONE TWO THREE".Split(' ')` ger `Length = 3`, minus 1 → `WordCount = 2`. Fel.

**Hur du hittar det med debuggern:**
1. Lägg `DisplayText.Split(' ').Length` i **Watch**.
2. Kör med ett enkelt skämt, t.ex. `{ Value = "Word" }`.
3. Se att `Split` ger `Length = 1` men `WordCount` sätts till `0`.

**Fix:**

```csharp
WordCount = DisplayText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
```

`StringSplitOptions.RemoveEmptyEntries` ignorerar tomma strängar som uppstår om det finns flera mellanslag i rad eller om `DisplayText` är tom. För en tom sträng ger det `Length = 0` – korrekt.

**Varför uppstår off-by-one?** Ofta ett tecken på att man tänkt på separatorer istället för element. Antalet element i en delad sträng är alltid *antalet separatorer + 1*, inte antalet separatorer. Kom ihåg: `"a b c"` har 2 mellanslag men 3 ord.

---

## Uppgift 3 – Buggen som testerna inte fångar

**Var:** `JokeService.cs`, i metoden `GetJokeFromCategory`.

**Vad som är fel:**

```csharp
// BUGGIG KOD
string url = "https://api.chucknorris.io/jokes/random?Category=" + category;
//                                                     ^^^^^^^^
//                                                     Stor C – API:et förväntar sig liten c
```

Chuck Norris-API:et är skiftlägeskänsligt. `?category=science` fungerar. `?Category=science` ignoreras – API:et returnerar ett slumpmässigt skämt oavsett kategori.

**Hur du hittar det med debuggern:**
1. Starta applikationen (`F5`).
2. Sätt en breakpoint i `GetJokeFromCategory`.
3. Surfa till `/?Category=science`.
4. Titta på `url` i **Variables**: `"https://api.chucknorris.io/jokes/random?Category=science"`.
5. Jämför med korrekt URL: `"https://api.chucknorris.io/jokes/random?category=science"`.

**Fix:**

```csharp
string url = "https://api.chucknorris.io/jokes/random?category=" + category;
```

**Varför fångar inte enhetstesterna det här?**

Alla tester i `TheChuckTests` använder fake-objekt som implementerar `IJokeService`. Faken returnerar ett hårdkodat svar utan att någonsin bygga en URL eller göra ett nätverksanrop. `JokeService.GetJokeFromCategory` anropas aldrig i testerna – bara faken anropas.

Det här är den fundamentala begränsningen med enhetstester: de testar varje del i isolation. Det är deras styrka (snabba, deterministiska, kontrollerade) men också deras svaghet – integrationsproblem som felaktiga URL:er, felaktiga API-parametrar och nätverksfel syns bara i integrationstester eller när du kör applikationen på riktigt.

**Vad lär vi oss?**
- Enhetstester och integrationstester kompletterar varandra – ingen av dem räcker ensam.
- Debuggern är ett ovärderligt verktyg för att hitta fel som testerna inte fångar.
- Strängar som byggs ihop (URL:er, SQL-frågor, filsökvägar) är vanliga felkällor – var noggrann med dem.

---

## Uppgift 4 – Det falskt gröna testet

**Vilket test är det?**

```csharp
[TestMethod()]
public async Task OnGet_WithWho_DisplayTextShouldNotBeEmpty()
{
    var joke = new Joke() { Value = "Chuck Norris is amazing" };
    var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));
    sut.Who = "Björn";

    await sut.OnGet();

    Assert.IsTrue(sut.DisplayText.Length > 0);  // Alltid sant!
}
```

**Varför är det falskt grönt?**

`sut.DisplayText.Length > 0` är sant så länge `DisplayText` inte är en tom sträng. Det kan den bara bli om `joke.Value` är tom *och* felet inte inträffar. Med `Value = "Chuck Norris is amazing"` är `DisplayText` alltid minst `"CHUCK NORRIS IS AMAZING"` – oavsett om Who-ersättningen fungerar eller inte.

Testet berättar alltså bara att sidan inte är blank. Det testar inte att `"Björn"` faktiskt dök upp i texten.

**Prova:** Med Who-buggen kvar, vad är `DisplayText`? `"CHUCK NORRIS IS AMAZING"` – `Length = 23 > 0`. Testet är grönt. Med buggen fixad? `"BJÖRN IS AMAZING"` – `Length = 16 > 0`. Testet är fortfarande grönt. Samma resultat båda vägerna → testet bevisar ingenting.

**Hur skriver man det rätt?**

```csharp
[TestMethod()]
public async Task OnGet_WithWho_DisplayTextShouldContainWho()
{
    var joke = new Joke() { Value = "Chuck Norris is amazing" };
    var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));
    sut.Who = "Björn";

    await sut.OnGet();

    StringAssert.Contains(sut.DisplayText, "BJÖRN");
    Assert.IsFalse(sut.DisplayText.Contains("CHUCK NORRIS"),
        "Chuck Norris borde ha ersatts med Björn men finns fortfarande kvar i texten.");
}
```

Det här testet är *rött* med buggen och *grönt* utan. Det är ett meningsfullt test.

**Vad lär vi oss?**

En assertion som alltid är sann är inte en assertion – det är en kommentar som råkade hamna i testkod. Ställ alltid frågan: *Under vilka omständigheter kan det här testet bli rött?* Om svaret är "aldrig" är testet värdelöst.

Falskt gröna tester är farliga i CI eftersom de skapar en falsk känsla av trygghet. Pipelinen är grön, alla är nöjda – men koden är trasig.

---

## Bugg-logg – facit

| # | Fil | Rad | Beskrivning | Hur hitta | Fix |
|---|-----|-----|-------------|-----------|-----|
| 1 | `Index.cshtml.cs` | ~35 | Inverterat `IsNullOrEmpty`-villkor | Breakpoint + stega med F10, kolla vilken gren som tas | Ändra till `!string.IsNullOrEmpty(Category)` |
| 2 | `Index.cshtml.cs` | ~50–53 | `ToUpper()` före `Replace()` | Watch på `DisplayText`, stega rad för rad | Flytta `Replace` till före `ToUpper()` |
| 3 | `Index.cshtml.cs` | ~56 | `Length - 1` (off-by-one) | Watch på `DisplayText.Split(' ').Length` | Ta bort `- 1`, lägg till `RemoveEmptyEntries` |
| 4 | `JokeService.cs` | ~17 | `?Category=` istället för `?category=` | Breakpoint i `GetJokeFromCategory`, inspektera `url` | Gemen `c` i `?category=` |
| 5 | `IndexModelTests.cs` | ~89 | `Assert.IsTrue(Length > 0)` – alltid sant | Fråga: när kan detta bli rött? (aldrig) | Kontrollera att `"BJÖRN"` finns och `"CHUCK NORRIS"` saknas |

---

## Reflektion

Innan du stänger facit – svara ärligt:

- Hittade du alla buggar enbart med felmeddelandena, eller behövde du debuggern aktivt?
- Förstod du bugg 1 direkt när du såg villkoret, eller behövde du stega igenom för att se effekten?
- Hade du hittat bugg 4 (det falskt gröna testet) om du inte visste att det fanns ett?
- Vad säger det om värdet av *code review* – att ha en annan person läsa din kod?
