// Dosya: TekstilScada.WebAPI/Program.cs

using TekstilScada.Repositories;
using TekstilScada.Services;
using TekstilScada.WebAPI.Hubs;
using TekstilScada.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// === TekstilScada Servislerini Buraya Ekliyoruz ===
// Proje boyunca tek bir �rne�i olacak t�m servisleri Singleton olarak kaydediyoruz.
// Bu, arka plan servislerinin ve anl�k veri ak���n�n tutarl� �al��mas� i�in gereklidir.
builder.Services.AddSingleton<MachineRepository>();
builder.Services.AddSingleton<AlarmRepository>();
builder.Services.AddSingleton<ProductionRepository>();
builder.Services.AddSingleton<ProcessLogRepository>();
builder.Services.AddSingleton<RecipeRepository>();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<DashboardRepository>();
builder.Services.AddSingleton<PlcPollingService>();
builder.Services.AddSingleton<SignalRBridgeService>();

// PLC Polling servisini arka planda �al��acak bir hizmet olarak ekliyoruz.
builder.Services.AddHostedService<PlcPollingBackgroundService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
// === Servis Ekleme Sonu ===

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHub<ScadaHub>("/scadaHub");

// K�pr� servisinin uygulama ba�larken aktif olmas�n� sa�l�yoruz.
app.Services.GetRequiredService<SignalRBridgeService>();

app.Run();