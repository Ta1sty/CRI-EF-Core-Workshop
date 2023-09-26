// See https://aka.ms/new-console-template for more information

using EFCoreWorkshop.Helper;
using EFCoreWorkshop.Model;

await using var services = await Services<WorkshopContext>.Create();


await services.WaitForShutdownAsync();
