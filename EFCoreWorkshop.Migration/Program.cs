// See https://aka.ms/new-console-template for more information

using DotNet.Testcontainers.Builders;
using EFCoreWorkshop.Migration;
using EFCoreWorkshop.Model;
using Microsoft.EntityFrameworkCore;

await using var services = await Services<WorkshopContext>.Create();

await services.Context.Database.MigrateAsync();