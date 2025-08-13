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

// Middleware sıralaması
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Hata/Index");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // burada olmak zorunda

app.UseAuthorization();

// Varsayılan route (ilk açılan sayfa: TTOrtam Ana Sayfa)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
