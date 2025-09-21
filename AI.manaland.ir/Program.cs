using AI.manaland.ir.Hubs;
using AI.manaland.ir.Services;

var builder = WebApplication.CreateBuilder(args);

// افزودن سرویس‌ها
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAIProvider, ArvanAIProvider>(); // ثبت ArvanAIProvider
builder.Services.AddScoped<IMultiAIChatService, MultiAIChatService>();

var app = builder.Build();

// تنظیمات middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();
app.MapHub<ChatHub>("/chathub");

app.Run();