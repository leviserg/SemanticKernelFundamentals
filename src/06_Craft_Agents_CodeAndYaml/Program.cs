using _06_Craft_Agents_CodeAndYaml;

Console.WriteLine("Starting CraftAgentService...");
Console.WriteLine();

await CraftAgentService.Execute();

Console.WriteLine();
Console.WriteLine("Completed CraftAgentService");
Console.WriteLine();
/*
Console.WriteLine("Starting AdvancedAgentService...");
Console.WriteLine();

await AdvancedAgentService.Execute();

Console.WriteLine();
Console.WriteLine("Completed AdvancedAgentService");
*/

Console.ReadKey();
