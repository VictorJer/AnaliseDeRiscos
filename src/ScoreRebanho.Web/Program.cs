using Microsoft.AspNetCore.Identity;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Infrastructure;
using ScoreRebanho.Web;
using ScoreRebanho.Web.Mapeamentos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(config =>
{
    var chave = builder.Configuration["AutoMapper:LicenseKey"];
    if (!string.IsNullOrWhiteSpace(chave))
        config.LicenseKey = chave;
}, typeof(ContaProfile));
builder.Services.AddScoped<CadastrarConta>();
builder.Services.AddSingleton(new CalcularScoreRebanho(TimeProvider.System));
builder.Services.AdicionarPersistenciaContas(
    builder.Configuration.GetConnectionString("Contas") ?? "Data Source=contas.db",
    options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version1;
        options.Stores.MaxLengthForKeys = 128;
        options.Password.RequiredLength = 8;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    }).AddErrorDescriber<ErrosIdentityPortugues>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Conta/Entrar";
    options.AccessDeniedPath = "/Conta/AcessoNegado";
    options.Cookie.Name = "ScoreRebanho.Sessao";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

await app.Services.MigrarContasAsync();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.Run();

public partial class Program { }
