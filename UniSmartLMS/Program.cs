using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Models;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Repositories.Implementations;
using UniSmartLMS.Repositories.Interfaces;
using UniSmartLMS.Services;
using UniSmartLMS.Services.AI;
using UniSmartLMS.Services.Implementations;
using UniSmartLMS.Services.Interfaces;
using UniSmartLMS.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Custom Identity with AppUser
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();



// Generic Repository
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));



builder.Services.AddScoped<IGenericRepository<ItemCopy>, GenericRepository<ItemCopy>>();
builder.Services.AddScoped<IGenericRepository<CirculationRule>, GenericRepository<CirculationRule>>();
builder.Services.AddScoped<IGenericRepository<Party>, GenericRepository<Party>>();

// Services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ICirculationService, CirculationService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IAccessionService, AccessionService>();
builder.Services.AddScoped<IFineService, FineService>();

// Dashboard Services
builder.Services.AddScoped<IStudentDashboardService, StudentDashboardService>();
builder.Services.AddScoped<IFacultyDashboardService, FacultyDashboardService>();
builder.Services.AddScoped<IStaffDashboardService, StaffDashboardService>();
builder.Services.AddScoped<ILibrarianDashboardService, LibrarianDashboardService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IVisitorDashboardService, VisitorDashboardService>();

// AI Assistant
builder.Services.AddScoped<IAIAssistantService, AIAssistantService>();


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IChatHistoryService, ChatHistoryService>();

// Add after other service registrations
builder.Services.AddScoped<IIntentCacheService, IntentCacheService>();

builder.Services.AddScoped<IResearchService, ResearchService>();

// Add these lines after builder.Services.AddScoped<...>
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
//builder.Services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();

builder.Services.AddScoped<IGeneralQAService, GeneralQAService>();
builder.Services.AddScoped<ILibraryHelpService, LibraryHelpService>();
builder.Services.AddScoped<IQueryUnderstandingService, QueryUnderstandingService>();

// Add this line with your other service registrations
builder.Services.AddScoped<IIntelligentAnswerEngine, IntelligentAnswerEngine>();
builder.Services.AddScoped<IDeepThinkEngine, DeepThinkEngine>();

// Add this line after other service registrations
//builder.Services.AddScoped<IFineService, FineService>();

builder.Services.AddScoped<IAuditService, AuditService>();  // ✅ CORRECT// Cookie Configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

var app = builder.Build();

// Seed Roles and Admin User
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.InitializeAsync(services);
    await DbSeeder.SeedAsync(services);

    var intentCache = scope.ServiceProvider.GetRequiredService<IIntentCacheService>();
    await intentCache.LoadIntentsAsync();
    Console.WriteLine($"✅ Loaded {intentCache.GetIntents().Count} AI Action Intents");
}

//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    await SeedData.InitializeAsync(services);
//}
// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "smartLibrary",
    pattern: "SmartLibrary/{action=Index}/{id?}",
    defaults: new { controller = "SmartLibrary", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();