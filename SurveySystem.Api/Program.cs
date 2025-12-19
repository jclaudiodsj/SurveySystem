using SurveySystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Aplica as migrações do banco de dados automaticamente ao iniciar a aplicação em desenvolvimento.
    //using (var scope = app.Services.CreateScope())
    //{
    //    var dbContext = scope.ServiceProvider.GetRequiredService<SurveySystemDbContext>();
    //    dbContext.Database.Migrate();
    //}
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
