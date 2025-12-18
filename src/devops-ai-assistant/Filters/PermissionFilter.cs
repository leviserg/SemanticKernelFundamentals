using devops_ai_assistant.Plugins;
using Microsoft.SemanticKernel;

namespace devops_ai_assistant.Filters
{
    public class PermissionFilter : IFunctionInvocationFilter
    {
        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {

            if (context.Function.PluginName == nameof(DevopsPlugin)
                && context.Function.Name == Constants.DeployToProdFunctionName
                )
            {
                Console.WriteLine("System Message: The assistant requires an approval to complete this operation. Do you approve (Y/N)");
                Console.Write("User: ");
                string shouldProceed = Console.ReadLine()!;

                if (shouldProceed != "Y")
                {
                    context.Result = new FunctionResult(context.Result, "The operation was not approved by the user");
                    return;
                }
            }
            await next(context);
        }
    }
}
