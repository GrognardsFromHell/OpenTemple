using System.Globalization;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace OpenTemple.DynamicScripting;

public class CompletionHelper
{
    private static readonly MethodInfo GetHelperMethod;
    private static readonly MethodInfo MatchesPatternMethod;

    private readonly object _helper;

    static CompletionHelper()
    {
        var assembly = Assembly.Load("Microsoft.CodeAnalysis.Features");
        var completionHelper = assembly.GetType("Microsoft.CodeAnalysis.Completion.CompletionHelper");
        GetHelperMethod = completionHelper.GetMethod("GetHelper", BindingFlags.Static | BindingFlags.Public);
        MatchesPatternMethod = completionHelper.GetMethod("MatchesPattern");
    }

    private CompletionHelper(object helper)
    {
        _helper = helper;
    }

    public static CompletionHelper Create(Document document)
    {
        var helper = GetHelperMethod.Invoke(null, new object[] {document});
        return new CompletionHelper(helper);
    }

    public bool MatchesPattern(string text, string pattern, CultureInfo culture)
    {
        return (bool) MatchesPatternMethod.Invoke(_helper, new object[] {text, pattern, culture});
    }
}