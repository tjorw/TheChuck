# Uppgradera till .NET 9

Tre ställen behöver uppdateras.

## 1. Installera .NET 9 SDK

Ladda ned och installera från [dot.net/download](https://dotnet.microsoft.com/download/dotnet/9.0). Verifiera med:

```bash
dotnet --version  # ska visa 9.x.x
```

## 2. Uppdatera de fyra `.csproj`-filerna

Ändra `net8.0` till `net9.0` i samtliga projektfiler:

- [TheChuck/TheChuck.csproj](../TheChuck/TheChuck.csproj)
- [TheChuck.Core/TheChuck.Core.csproj](../TheChuck.Core/TheChuck.Core.csproj)
- [TheChuck.Infrastructure/TheChuck.Infrastructure.csproj](../TheChuck.Infrastructure/TheChuck.Infrastructure.csproj)
- [TheChuckTests/TheChuckTests.csproj](../TheChuckTests/TheChuckTests.csproj)

```xml
<TargetFramework>net9.0</TargetFramework>
```

Passa även på att uppdatera testpaketen i `TheChuckTests.csproj` till aktuella versioner:

```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
<PackageReference Include="MSTest.TestAdapter" Version="3.6.4" />
<PackageReference Include="MSTest.TestFramework" Version="3.6.4" />
<PackageReference Include="coverlet.collector" Version="6.0.3" />
```

## 3. Uppdatera GitHub Actions-pipelinen

Ändra `dotnet-version` i [.github/workflows/dotnet.yml](../.github/workflows/dotnet.yml):

```yaml
- name: Setup .NET 9
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: 9.0
```

## 4. Verifiera

```bash
dotnet restore
dotnet build
dotnet test
```

Alla tester ska vara gröna. Därefter räcker det med en push för att pipelinen ska deploya med .NET 9.

> Det finns inga breaking changes i det här projektet mellan .NET 8 och 9 – uppgraderingen är rent mekanisk.
