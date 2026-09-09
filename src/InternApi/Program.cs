using InternApi.Services;

// Dapper の列名マッチングを snake_case (DB) ↔ PascalCase (C#) に対応させる。
// これにより SELECT で "AS PascalName" のエイリアスを書かずに済む。
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<GachaService>();
builder.Services.AddScoped<UserItemService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Intern-Server-2026 API",
        Version = "v1",
        Description = "Coly 2026 1day インターン向けサンプルAPI"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Intern-Server-2026 API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
