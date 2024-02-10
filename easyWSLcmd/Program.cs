// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
var app = builder.Build();

app.Start();

app.WaitForShutdown();