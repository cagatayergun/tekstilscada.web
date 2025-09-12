// Dosya: TekstilScada.WebApp/Program.cs

using TekstilScada.WebApp.Components;
using TekstilScada.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- DO�RU YAPILANDIRMA ---

// 1. "WebApiClient" ad�yla �zel bir HttpClient yap�land�r�yoruz.
builder.Services.AddHttpClient("WebApiClient", client =>
{
    // L�TFEN WebAPI projenizin �al��t��� PORT numaras�n� burada kontrol edin!
    // Genellikle 7000'li bir say�d�r.
    client.BaseAddress = new Uri("https://localhost:7039");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    // Geli�tirme ortam�nda SSL sertifika hatalar�n� g�rmezden gel
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };
});

// 2. ScadaDataService'i, yukar�da yap�land�rd���m�z �zel HttpClient'� alacak �ekilde kaydediyoruz.
builder.Services.AddSingleton(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("WebApiClient"); // �smine g�re do�ru istemciyi istiyoruz.
    return new ScadaDataService(httpClient);
});

// --- YAPILANDIRMA SONU ---


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Uygulama ba�larken ScadaDataService'i ba�lat�yoruz.
var scadaDataService = app.Services.GetRequiredService<ScadaDataService>();
await scadaDataService.InitializeAsync();

app.Run();