using SurveyBasket;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseCors("MyPolicy");
app.UseCors(); // defualt policy

app.UseAuthorization();
app.MapControllers();
app.UseExceptionHandler();

app.Run();
