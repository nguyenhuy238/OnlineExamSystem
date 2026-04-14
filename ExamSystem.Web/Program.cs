using ExamSystem.Core.Interfaces;
using ExamSystem.Core.Services;
using ExamSystem.Data;
using ExamSystem.Data.Repositories;
using ExamSystem.Web.Configuration;
using ExamSystem.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IResultRepository, ResultRepository>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<AuthFilter>();
builder.Services.Configure<AdminAccountOptions>(builder.Configuration.GetSection(AdminAccountOptions.SectionName));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<AuthFilter>();
});
builder.Services.AddRazorPages(options =>
{
    options.Conventions.ConfigureFilter(new ServiceFilterAttribute(typeof(AuthFilter)));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
