var builder = WebApplication.CreateBuilder(args);

// Razor views i�in
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

// Middleware s�ralamas�
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

// Varsay�lan route (ilk a��lan sayfa: TTOrtam Ana Sayfa)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
