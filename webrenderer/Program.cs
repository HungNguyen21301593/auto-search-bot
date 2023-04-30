using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.Use(async (context, next) =>
{
    if (context.Request.Path.Value.Contains("/Home/Index"))
    {
        await next();
        return;
    }
    context.Response.Redirect($"/Home/Index?link=https://alonhadat.com.vn{context.Request.Path.Value}");
});
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}"
    );

//app.MapGet("/", context =>
//{
//    var query = context.Request.QueryString.ToString();
//    context.Response.Redirect("/Home/Index?link=test");
//    return Task.CompletedTask;
//});
app.Run();
