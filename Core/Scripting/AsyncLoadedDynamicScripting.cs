using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using OpenTemple.Core.Logging;

namespace OpenTemple.Core.Scripting;

/// <summary>
/// A proxy dynamic scripting implementation that will load the actual implementation in the background.
/// </summary>
public class AsyncLoadedDynamicScripting : IDynamicScripting
{
    private const string ImplementationClass = "OpenTemple.DynamicScripting.DynamicScripting";

    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly Task<IDynamicScripting> _scripting;

    private IDynamicScripting Delegate => _scripting.Result;

    public AsyncLoadedDynamicScripting()
    {
        _scripting = Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();

            // The dynamic scripting assembly is optional so it doesn't need to be loaded during normal gameplay
            try
            {
                var dynamicScriptingAssembly = Assembly.Load("DynamicScripting");
                var dynamicScriptingType =
                    dynamicScriptingAssembly.GetType(ImplementationClass);

                if (dynamicScriptingType == null)
                {
                    throw new Exception("The dynamic scripting assembly was loaded but didn't contain the type: " + ImplementationClass);
                }

                var result = (IDynamicScripting) Activator.CreateInstance(dynamicScriptingType);
                Debug.Assert(result != null);

                Logger.Info("Loaded dynamic scripting subsystem in {0}", sw.Elapsed);

                return result;
            }
            catch (Exception e)
            {
                Logger.Info("Unable to activate dynamic scripting: {0}", e);
                return new DisabledDynamicScripting();
            }
        });
    }

    public object? EvaluateExpression(string command)
    {
        return Delegate.EvaluateExpression(command);
    }

    public string Complete(string command)
    {
        return Delegate.Complete(command);
    }

    public Task<object?> RunScriptAsync(string path)
    {
        return Delegate.RunScriptAsync(path);
    }

    public void RunStartupScripts()
    {
        Delegate.RunStartupScripts();
    }

    public void AddAssembly(Assembly assembly)
    {
        Delegate.AddAssembly(assembly);
    }
}