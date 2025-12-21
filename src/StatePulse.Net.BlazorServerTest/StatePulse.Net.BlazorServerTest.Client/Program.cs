using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StatePulse.Net;
using StatePulse.Net.Configuration;
var builder = WebAssemblyHostBuilder.CreateDefault(args);

await builder.Build().RunAsync();
