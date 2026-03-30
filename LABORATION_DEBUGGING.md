# Laboration – Debugging & Felsökning

## Introduktion

Någon har implementerat tre nya funktioner i TheChuck-projektet:

- **Category** – hämta skämt från en specifik kategori via `/?Category=science`
- **Who** – byt ut "Chuck Norris" mot ett eget namn via `/?Who=Björn`
- **WordCount** – visar antal ord i skämtet på sidan

Tyvärr innehåller koden **fyra planterade buggar**. Tre av dem gör att tester misslyckas i CI-pipelinen. En fjärde syns bara när du kör applikationen – den fångas aldrig av testerna.

Det finns också ett **falskt grönt test** – ett test som alltid passerar trots att det inte testar något meningsfullt. Din uppgift är att hitta det också.

Din uppgift: hitta och fixa alla buggar med hjälp av VS Code debugger.

---

## Kom igång

### 1. Kör testerna och se hur många som failar

Öppna en terminal och kör:

```bash
dotnet test
```

Notera vilka tester som misslyckas och vad felmeddelandena säger. Det är din startpunkt.

### 2. Starta debuggern i VS Code

- Öppna **Run and Debug** (`Ctrl+Shift+D`)
- Välj **TheChuck** i listan och tryck `F5`
- Applikationen startar och du kan sätta breakpoints

### 3. Grundläggande VS Code-debugger

| Tangent | Vad den gör |
|---------|-------------|
| `F5` | Starta / fortsätt körning |
| `F9` | Sätt/ta bort breakpoint på markerad rad |
| `F10` | Stega över (kör en rad, gå inte in i metoder) |
| `F11` | Stega in (gå in i metodanrop) |
| `Shift+F11` | Stega ut (fortsätt tills nuvarande metod returnerar) |

I **Debug-panelen** till vänster ser du:
- **Variables** – alla lokala variabler och deras värden just nu
- **Watch** – variabler och uttryck du vill hålla koll på (högerklicka → Add to Watch)
- **Call Stack** – vilka metod-anrop som ledde hit

---

## Uppgifterna

---

### Uppgift 1 – Hitta buggarna via failing tests

Kör `dotnet test` och analysera output.

**Steg:**
1. Läs felmeddelandena – vilka tester failar och vad förväntar de sig?
2. Öppna `IndexModel.cs` och sätt en breakpoint i `OnGet()`.
3. Starta debuggern (`F5`) och surfa till sidan. Stega igenom koden med `F10`/`F11`.
4. Titta i **Variables**-panelen – stämmer värdena med vad du förväntade dig?

**Frågor att besvara:**
- Vilket påstående (Assert) failar i varje test?
- Vad är det faktiska värdet och vad förväntades?
- På vilken rad i koden uppstår felet?

---

### Uppgift 2 – Fixa buggarna en i taget

Fixa **en bugg i taget** och kör `dotnet test` efter varje fix. Arbeta i den här ordningen:

1. **Bugg i Category-logiken** – Vilken metod anropas när `Category` är satt respektive inte satt? Är det rätt?
2. **Bugg i Who-ersättningen** – Sätt ett watch-uttryck på `DisplayText` och se vad som händer steg för steg. Varför hittar inte `Replace` det den letar efter?
3. **Bugg i WordCount** – Hur beräknas `WordCount`? Testa med ett enkelt ord som "Word" – vad borde resultatet vara?

> **Tips:** Lägg ett watch-uttryck (`DisplayText`) i Watch-panelen så ser du värdet förändras för varje rad du stegar igenom.

---

### Uppgift 3 – Buggen som testerna INTE fångar

En bugg i projektet syns **aldrig** i `dotnet test` eftersom alla tester använder fake-objekt som aldrig når det riktiga nätverksanropet.

**Steg:**
1. Starta applikationen (`F5`) och surfa till `/?Category=science`.
2. Sätt en breakpoint i `JokeService.cs` i metoden `GetJokeFromCategory`.
3. Titta på variabeln `url` i **Variables**-panelen.
4. Jämför URL:en med API-dokumentationen: `https://api.chucknorris.io/jokes/random?category=science`

**Frågor att besvara:**
- Vad är det för skillnad mellan URL:en i koden och den korrekta URL:en?
- Varför fångas inte det här felet av enhetstesterna?
- Vad säger det om vikten av att testa på olika nivåer (enhetstest vs. integrationstest)?

---

### Uppgift 4 – Det falskt gröna testet

I `IndexModelTests.cs` finns ett test som **alltid är grönt** oavsett om Who-ersättningen fungerar eller inte.

**Steg:**
1. Läs igenom alla tester i `IndexModelTests.cs`.
2. Hitta testet som verifierar Who-ersättningen med en assertion som egentligen inte bevisar något.
3. Förklara varför testet alltid passerar.
4. Skriv om testet så att det faktiskt fångar buggen (eller bekräftar att den är fixad).

> **Fråga:** Vad kan hända i ett riktigt projekt om man har många falskt gröna tester i CI-pipelinen?

---

## Är jag klar?

Bocka av varje punkt du genomfört:

**Uppgift 1 – Analysera**
- [ ] Jag har kört `dotnet test` och vet exakt vilka tester som failar
- [ ] Jag har satt breakpoints och stegat igenom `OnGet()` med debuggern
- [ ] Jag har förklarat vad varje failing assertion förväntar sig vs. vad den fick

**Uppgift 2 – Fixa**
- [ ] Category-buggen är fixad och relaterade tester är gröna
- [ ] Who-buggen är fixad och relaterade tester är gröna
- [ ] WordCount-buggen är fixad och relaterade tester är gröna
- [ ] `dotnet test` är helt grön

**Uppgift 3 – Nätverksbugg**
- [ ] Jag hittade buggen i `JokeService.cs` via debuggern
- [ ] Jag kan förklara varför enhetstester inte fångar det här felet

**Uppgift 4 – Falskt grönt test**
- [ ] Jag identifierade vilket test som är falskt grönt och varför
- [ ] Jag har skrivit om testet så att det faktiskt testar rätt sak

---

## Bugg-logg

Fyll i den här tabellen när du hittar och fixar varje bugg:

| # | Fil | Rad (ungefär) | Beskrivning | Hur hittade du den? | Fix |
|---|-----|---------------|-------------|----------------------|-----|
| 1 | | | | | |
| 2 | | | | | |
| 3 | | | | | |
| 4 | | | | | |
| 5 (falskt grönt) | | | | | |
