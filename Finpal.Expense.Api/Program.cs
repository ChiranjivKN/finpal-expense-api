using FinPal.Expense.Api.Data;
using Microsoft.EntityFrameworkCore;
using FinPal.Expense.Api.Middleware;
using FinPal.Expense.Api.Services.Categories;
using FinPal.Expense.Api.Services.Expense;
using FinPal.Expense.Api.Services.Budgets;
using FinPal.Expense.Api.Services.Auth;
using FinPal.Expense.Api.Services.Security;


var builder = WebApplication.CreateBuilder(args);

//Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();

//Initializing connection string
builder.Services.AddDbContext<FinPalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();


