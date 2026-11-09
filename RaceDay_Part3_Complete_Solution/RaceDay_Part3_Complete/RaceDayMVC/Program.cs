using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(90);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddHttpClient<RaceDayApiClient>(client =>
{
    var configured = builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7230/";
    client.BaseAddress = new Uri(configured.EndsWith('/') ? configured : configured + "/");
    client.Timeout = TimeSpan.FromSeconds(25);
});

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
