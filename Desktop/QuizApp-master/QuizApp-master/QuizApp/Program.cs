using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR(); 

// 1. Veritabanı Bağlantısı (DbContext)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. MVC servislerini ekle
builder.Services.AddControllersWithViews();

// 3. DOKÜMAN GEREĞİ: Session (Oturum) servisini ekle
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 dakika işlem yapılmazsa oturum düşer
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 4. HTTP Context Accessor (Session'a her yerden erişmek için gerekebilir)
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

// Development ortamı ayarları
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 5. KRİTİK SIRALAMA: Session, Authentication'dan önce gelmelidir
app.UseSession();

app.UseAuthorization();

// 6. Varsayılan Route (Giriş ekranı Login olsun istersen burayı değiştirebilirsin)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapHub<QuizHub>("/quizHub"); 
app.Run();