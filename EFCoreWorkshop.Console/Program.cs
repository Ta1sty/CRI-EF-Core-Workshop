using System.IO.Pipes;

foreach (var arg in args)
{
    Console.WriteLine(arg);
}
var stream = new NamedPipeClientStream(".",args[0], PipeDirection.In);
await stream.ConnectAsync();

var reader = new StreamReader(stream, leaveOpen: true);
string? line;
while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync()))
{
    Console.WriteLine(line);
}
Console.WriteLine("Press any key to quit");
Console.ReadLine();