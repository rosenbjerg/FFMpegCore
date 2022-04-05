using System.Threading;
using System.Threading.Tasks;
using Instances;

namespace FFMpegCore
{
    public static class ProcessArgumentsExtensions
    {
        public static IProcessResult StartAndWaitForExit(this ProcessArguments processArguments)
        {
            using var instance = processArguments.Start();
            return instance.WaitForExit();
        }
        public static async Task<IProcessResult> StartAndWaitForExitAsync(this ProcessArguments processArguments, CancellationToken cancellationToken = default)
        {
            using var instance = processArguments.Start();
            return await instance.WaitForExitAsync(cancellationToken);
        }
    }
}