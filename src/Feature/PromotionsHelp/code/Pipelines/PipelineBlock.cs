using System.Threading.Tasks;
using Sitecore.Commerce.Core;

namespace Sitecore.Commerce.Plugin.PromotionsHelp.Pipelines
{
    public abstract class PipelineBlock<TInput, TOutput, TContext> : Sitecore.Framework.Pipelines.AsyncPipelineBlock<TInput, TOutput, TContext> where TContext : CommercePipelineExecutionContext where TOutput : class where TInput : class
    {
        public override Task<TOutput> RunAsync(TInput arg, TContext context)
        {
            return RunBlock(arg, context);
        }

        public abstract Task<TOutput> RunBlock(TInput arg, TContext context);

    }
}