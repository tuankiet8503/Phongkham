using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = "733894631320-3d5mdqhtroqu317a37bo8l707grh5mta.apps.googleusercontent.com";
        options.ClientSecret = "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";
    });

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDBcontext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Thêm dịch vụ xác thực Identity và cấu hình cookie
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddDefaultTokenProviders()
    .AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDBcontext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

// Thêm middleware xác thực và phân quyền vào pipeline
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDBcontext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    context.Database.Migrate();

    // Tạo vai trò admin nếu chưa tồn tại
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    if (!await roleManager.RoleExistsAsync("Dentist"))
    {
        await roleManager.CreateAsync(new IdentityRole("Dentist"));
    }
}// Middleware để chọn layout dựa trên vai trò
// Trong file Startup.cs
app.Use(async (context, next) =>
{
    if (context.User.Identity.IsAuthenticated)
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.User);

        if (user != null)
        {
            var roles = await userManager.GetRolesAsync(user);

            // Lựa chọn layout dựa trên vai trò của người dùng
            string layout = "DefaultLayout"; // Layout mặc định
            if (roles.Contains("Admin"))
            {
                layout = "_AdLayout"; // Layout cho Admin
            }
            else if (roles.Contains("Dentist"))
            {
                layout = "_LayoutDentist"; // Layout cho Dentist
            }
            // Các vai trò khác có thể được xử lý ở đây

            // Lưu layout vào HttpContext để sử dụng sau này
            context.Items["SelectedLayout"] = layout;
        }
    }

    await next();
});

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

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
