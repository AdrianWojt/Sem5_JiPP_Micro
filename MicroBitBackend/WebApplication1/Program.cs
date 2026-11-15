using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja klienta HTTP dla Supabase
builder.Services.AddHttpClient("Supabase", client =>
{
    // Wpisz swój URL projektu Supabase (np. https://xyz.supabase.co/rest/v1/)
    client.BaseAddress = new Uri("https://TWOJ_PROJEKT_ID.supabase.co/rest/v1/");

    // Klucz API z panelu Supabase (Anon / Service Role Key)
    string apiKey = "TWOJ_SUPABASE_ANON_KEY";

    client.DefaultRequestHeaders.Add("apikey", apiKey);
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    client.DefaultRequestHeaders.Add("Prefer", "return=minimal");
});

var app = builder.Build();

app.MapPost("/api/score", async ([FromBody] ScoreRequest request, IHttpClientFactory clientFactory) =>
{
    // Walidacja podstawowa
    if (request == null)
    {
        return Results.BadRequest(new { error = "Nieprawid³owy format danych." });
    }

    var client = clientFactory.CreateClient("Supabase");

    // Mapowanie na kolumny Twojej tabeli w Supabase (score oraz client_id)
    var dbPayload = new
    {
        client_id = request.Id.ToString(),
        score = request.Wynik
    };

    // Wys³anie danych do tabeli o nazwie "game_scores" w Supabase
    var response = await client.PostAsJsonAsync("game_scores", dbPayload);

    if (response.IsSuccessStatusCode)
    {
        return Results.Ok(new
        {
            success = true,
            message = "Wynik zosta³ pomyœlnie zapisany w bazie danych."
        });
    }

    // Obs³uga b³êdu ze strony Supabase
    var errorDetails = await response.Content.ReadAsStringAsync();
    return Results.BadRequest(new
    {
        error = "B³¹d podczas zapisu do bazy danych.",
        details = errorDetails
    });
});

app.Run();

// DTO (Data Transfer Object) odzwierciedlaj¹ce wymagane wejœcie JSON
// Body: { "id": <wartoœæ>, "wynik": <wartoœæ> }
public record ScoreRequest(int Id, int Wynik);