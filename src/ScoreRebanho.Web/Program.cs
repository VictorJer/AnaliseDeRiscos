using ScoreRebanho.Application.Administracao;
using ScoreRebanho.Web.Comandos;
using Microsoft.AspNetCore.Identity;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Application.Rebanhos;
using ScoreRebanho.Application.Historico;
using ScoreRebanho.Web.Autorizacao;
using ScoreRebanho.Infrastructure;
using ScoreRebanho.Web;
using ScoreRebanho.Web.Mapeamentos;

ComandoContaLocal? comandoLocal = null;
if (args.FirstOrDefault() == "contas")
{
    if (args.SequenceEqual(new[] { "contas", "ajuda" }))
    {
        Console.WriteLine(ComandoContaLocal.Ajuda);
        return 0;
    }
    if (!ComandoContaLocal.TryParse(args, out comandoLocal))
    {
        Console.Error.WriteLine(ComandoContaLocal.Ajuda);
        return 2;
    }
}
var builder = WebApplication.CreateBuilder(comandoLocal is null ? args : Array.Empty<string>());

builder.Services.AddControllersWithViews(options =>
{
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(campo => $"Informe um número válido para {campo}.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((_, campo) => $"O valor informado para {campo} é inválido.");
});
builder.Services.AddAutoMapper(config =>
{
    var chave = builder.Configuration["AutoMapper:LicenseKey"];
    if (!string.IsNullOrWhiteSpace(chave))
        config.LicenseKey = chave;
}, typeof(ContaProfile));
builder.Services.AddScoped<CadastrarConta>();
builder.Services.AddScoped<ExecutarComandoContaLocal>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioAtual, UsuarioAtual>();
builder.Services.AddScoped<CriarRebanho>();
builder.Services.AddScoped<ConferirOcupacao>();
builder.Services.AddScoped<GerarSimulacao>();
builder.Services.AddScoped<ConsultarEdicaoSimulacao>();
builder.Services.AddScoped<ListarRebanhos>();
builder.Services.AddScoped<ConsultarRebanho>();
builder.Services.AddScoped<ConsultarEdicaoRebanho>();
builder.Services.AddScoped<AtualizarRebanho>();
builder.Services.AddScoped<ExcluirRebanho>();
builder.Services.AddScoped<ConsultarHistoricoProprio>();
builder.Services.AddScoped<ListarScoresAdministrativos>();
builder.Services.AddScoped<ConsultarScoreAdministrativo>();
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

await using var app = builder.Build();

if (comandoLocal is not null)
{
    await using var escopo = app.Services.CreateAsyncScope();
    try
    {
        var resultado = await escopo.ServiceProvider.GetRequiredService<ExecutarComandoContaLocal>().ExecutarAsync(comandoLocal);
        if (resultado.CodigoSaida == 0) Console.WriteLine(resultado.Mensagem);
        else Console.Error.WriteLine(resultado.Mensagem);
        return resultado.CodigoSaida;
    }
    catch (Exception erro)
    {
        app.Logger.LogError(erro, "Falha inesperada ao executar comando local de contas.");
        Console.Error.WriteLine("Não foi possível executar o comando. Verifique os logs e a configuração do banco.");
        return 1;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

await app.Services.MigrarContasAsync();
app.UseHttpsRedirection();
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("pt-BR").AddSupportedCultures("pt-BR").AddSupportedUICultures("pt-BR"));
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.Run();
return 0;

public partial class Program { }
