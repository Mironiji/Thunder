using Thunder.Client.CLI;
using Thunder.Client.Interfaces;
using Thunder.Client.Service;

var CLI = new ThunderClientCLI();

Animations.PrintThunderAnimation();
await CLI.RunAsync();