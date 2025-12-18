using Microsoft.SemanticKernel;

namespace devops_ai_assistant.Plugins
{
    public class DevopsPlugin
    {

        private const string DeployProductionMessage = "Production site deployed successfully.";
        private const string BuildStageMessage = "Stage build completed.";

        [KernelFunction(Constants.BuildStageEnvironmentFunctionName)]
        public string BuildStageEnvironment()
        {
            return BuildStageMessage;
        }

        [KernelFunction(Constants.DeployToProdFunctionName)]
        public static string DeployToProd()
        {
            return DeployProductionMessage;
        }

        [KernelFunction(Constants.CreateNewBranchFunctionName)]
        public static string CreateNewBranch(string branchName, string baseBranch)
        {
            return $"Created new branch `{branchName}` from `{baseBranch}`";
        }

        [KernelFunction(Constants.ReadLogFileFunctionName)]
        public static string ReadLogFile()
        {
            string content = File.ReadAllText($"Files/build.log");
            return content;
        }
    }
}
