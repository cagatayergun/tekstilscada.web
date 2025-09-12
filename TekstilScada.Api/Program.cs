// Gerekli using ifadeleri
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TekstilScada.Api.Hubs;
using TekstilScada.Api.Services;
using TekstilScada.Core;
using TekstilScada.Repositories;
using TekstilScada.Services;

// *******************************************************************
// HATA YAKALAMA MEKAN�ZMASI BA�LANGICI
// *******************************************************************
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        // Port numaras�n�n launchSettings.json ile ayn� oldu�undan emin ol
        int httpsPort = 7253;

        serverOptions.ListenLocalhost(httpsPort, listenOptions =>
        {
            listenOptions.UseHttps();
        });
    });
    // 1. ADIM: SERV�SLER� TANIMLA
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    AppConfig.SetConnectionString(connectionString);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Secret").Value)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });

    builder.Services.AddAuthorization();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("BlazorAppPolicy", policyBuilder =>
        {
            policyBuilder.AllowAnyOrigin()
                         .AllowAnyMethod()
                         .AllowAnyHeader();
        });
    });

    builder.Services.AddSignalR();
    builder.Services.AddSingleton<AlarmRepository>();
    builder.Services.AddSingleton<MachineRepository>();
    builder.Services.AddSingleton<ProductionRepository>();
    builder.Services.AddSingleton<RecipeRepository>();
    builder.Services.AddSingleton<ProcessLogRepository>();
    builder.Services.AddSingleton<CostRepository>();
    builder.Services.AddSingleton<UserRepository>();
    builder.Services.AddSingleton<DashboardRepository>();
    builder.Services.AddSingleton<RecipeConfigurationRepository>();
    builder.Services.AddSingleton<PlcOperatorRepository>();
    builder.Services.AddSingleton<AuthService>();
    builder.Services.AddSingleton<PlcPollingService>();
    builder.Services.AddSingleton<IHostedService, PlcPollingHostedService>();
    builder.Services.AddSingleton<IHostedService, SignalRNotifierService>();
    builder.Services.AddSingleton<FtpTransferService>();

    // 2. ADIM: UYGULAMAYI OLU�TUR
    var app = builder.Build();

    // 3. ADIM: HTTP ISTEK HATTINI YAPILANDIR
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors("BlazorAppPolicy");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<MachineHub>("/machine-hub");

    // 4. ADIM: UYGULAMAYI �ALI�TIR
    app.Run();
}
catch (Exception ex)
{
    // E�ER UYGULAMA BA�LARKEN ��KERSE, HATAYI B�R DOSYAYA YAZ.
    File.WriteAllText("UYGULAMA_COKME_HATASI.txt", ex.ToString());
}
// *******************************************************************
// HATA YAKALAMA MEKAN�ZMASI SONU
// *******************************************************************