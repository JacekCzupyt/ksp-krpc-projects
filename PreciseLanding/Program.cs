// See https://aka.ms/new-console-template for more information

using System.Threading.Channels;
using PreciseLanding;

var controller = new PreciseLandingController();

await controller.Guide();

Console.WriteLine("done");
