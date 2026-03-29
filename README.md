# The Chuck

Övningsprojekt för kursen SUVNET. Applikationen hämtar Chuck Norris-skämt från ett öppet API och demonstrerar hur man bygger en ASP.NET Core-applikation med lagerarkitektur, Dependency Injection och enhetstester.

## OBS – avsiktliga defekter

Det här projektet innehåller medvetet inbyggda defekter och ofullständigheter, eftersom det är en bas för en skoluppgift. Förvänta dig att hitta kod som kan förbättras och funktioner som inte är implementerade – det är en del av uppgiften.

## Kom igång

```bash
git clone <repo-url>
cd TheChuck
dotnet restore
dotnet run --project TheChuck
```

Surfa sedan till `https://localhost:7070`.

### Kör testerna

```bash
dotnet test
```

### Querystring-parametrar

| Parameter | Exempel | Effekt |
|-----------|---------|--------|
| `Category` | `/?Category=science` | Hämtar skämt från en specifik kategori |
| `Who` | `/?Who=Björn` | Ersätter "Chuck Norris" med valfritt namn |

Tillgängliga kategorier finns på [api.chucknorris.io](https://api.chucknorris.io).

## Projektstruktur

```
TheChuck/                 – ASP.NET Core Razor Pages (presentationslager)
TheChuck.Core/            – Domänmodeller (Joke)
TheChuck.Infrastructure/  – Tjänster och HTTP-klient
TheChuckTests/            – Enhetstester med MSTest och fake-objekt
```

## Guider

- [Uppgradera till .NET 9](docs/upgrade-dotnet9.md) om det är det ni använder annars?

## Laboration

Se [LABORATION.md](LABORATION.md) för uppgifterna. Använd [FACIT.md](FACIT.md) för självbedömning när du är klar.

## Externa resurser

### Verktyg och hosting
- [MonsterASP.NET](https://www.monsterasp.net/) – gratis ASP.NET-hosting som används i laborationen
- [GitHub Actions](https://docs.github.com/en/actions) – CI/CD-pipeline som bygger, testar och driftsätter automatiskt
- [rasmusbuchholdt/simply-web-deploy](https://github.com/rasmusbuchholdt/simply-web-deploy) – GitHub Action som används för WebDeploy till MonsterASP.NET

### API
- [Chuck Norris API](https://api.chucknorris.io/) – öppet API utan autentisering
  - Slumpmässigt skämt: `GET https://api.chucknorris.io/jokes/random`
  - Skämt från kategori: `GET https://api.chucknorris.io/jokes/random?category=science`

### ASP.NET Core
- [Razor Pages – introduktion](https://learn.microsoft.com/aspnet/core/razor-pages/)
- [Dependency Injection i ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection)
- [HttpClient i .NET](https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient)

### Testning
- [MSTest – komma igång](https://learn.microsoft.com/dotnet/core/testing/unit-testing-with-mstest)
- [Arrange–Act–Assert-mönstret](https://learn.microsoft.com/dotnet/core/testing/unit-testing-best-practices#arranging-your-tests)
- [Test doubles – fake, mock, stub](https://learn.microsoft.com/dotnet/core/testing/unit-testing-best-practices#stub-vs-mock-vs-fake)

### Designprinciper
- [Clean Code – SOLID-principerna](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/architectural-principles)
- [Test-Driven Development (TDD)](https://learn.microsoft.com/dotnet/core/testing/unit-testing-best-practices)
