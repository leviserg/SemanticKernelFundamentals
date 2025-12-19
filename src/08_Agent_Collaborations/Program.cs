using _08_Agent_Collaborations;

Console.WriteLine("Starting...");
var service = new AgentCollaborationService();
await service.Execute();
service.Dispose();
Console.WriteLine("Completed...");
Console.ReadKey();