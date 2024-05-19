using EmpresaSegura.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });//AGREGADO
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();//Agregado
builder.Services.AddAuthorizationCore();//Agregado
builder.Services.AddCascadingAuthenticationState();//Agregado	

await builder.Build().RunAsync();
