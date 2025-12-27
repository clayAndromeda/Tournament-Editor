using Microsoft.EntityFrameworkCore;
using TournamentEditor.Components;
using TournamentEditor.Core.Data;
using TournamentEditor.Core.Data.Repositories;
using TournamentEditor.Core.Services;
using TournamentEditor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// HTTP Context Accessor (for getting user ID in services)
builder.Services.AddHttpContextAccessor();

// Database
builder.Services.AddDbContext<TournamentDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=tournament.db"));

// Repositories
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();
builder.Services.AddScoped<ITournamentRepository, TournamentRepository>();

// Services
builder.Services.AddSingleton<TournamentService>();  // 互換性のため残す（既存コードで使用中）
builder.Services.AddScoped<TournamentServiceV2>();    // DB永続化対応版
builder.Services.AddSingleton<TournamentSimulationService>();
builder.Services.AddScoped<ParticipantManagementService>();
builder.Services.AddScoped<LotteryService>();
builder.Services.AddScoped<TournamentPersistenceService>();
builder.Services.AddScoped<UserContextService>();

var app = builder.Build();

// Initialize database with migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TournamentDbContext>();
    // 開発環境では自動マイグレーション、本番環境では手動マイグレーションを推奨
    if (app.Environment.IsDevelopment())
    {
        dbContext.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Session middleware
app.UseSession();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
