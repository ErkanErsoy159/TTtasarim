var builder = WebApplication.CreateBuilder(args);

// Razor views için
builder.Services.AddControllersWithViews();

// HttpClient servisi
builder.Services.AddHttpClient();

// Session servisi
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Middleware sýralamasý
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // burada olmak zorunda

app.UseAuthorization();

// Varsayýlan route (ilk açýlan sayfa: Giriþ)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Bayiler}/{action=Index}/{id?}");

app.Run();
