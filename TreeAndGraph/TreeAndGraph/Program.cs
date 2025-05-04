using Radzen;
using TreeAndGraph.Components;
using TreeAndGraph.Services;
using TreeAndGraph.Services.Sorting;
using TreeAndGraph.Services.GraphService;
using TreeAndGraph.Services.TreeAndBSTService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

//Add các service của thuật toán đồ thị

//Cây gia phả
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();


// --- Đăng ký Radzen Services ---
builder.Services.AddScoped<Radzen.DialogService>();
builder.Services.AddScoped<Radzen.NotificationService>();
builder.Services.AddScoped<Radzen.ContextMenuService>();
builder.Services.AddScoped<Radzen.TooltipService>();
builder.Services.AddScoped<ITreeService, TreeService>();

builder.Services.AddRadzenComponents();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

//app.MapBlazorHub();
//app.MapFallbackToPage("/_Host");

app.Run();