using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Tests.Ui;

public static class WidgetBaseExtensions
{
    public static WidgetBaseAssertions Should(this WidgetBase instance)
    {
        return new WidgetBaseAssertions(instance);
    }
}

public class WidgetBaseAssertions : ReferenceTypeAssertions<WidgetBase, WidgetBaseAssertions>
{
    public WidgetBaseAssertions(WidgetBase subject) : base(subject)
    {
    }

    protected override string Identifier => "widget";

    public AndConstraint<WidgetBaseAssertions> HaveId(
        string id, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(!string.IsNullOrEmpty(id))
            .FailWith("Pass an ID to check if a widget has the given id")
            .Then
            .Given(() => Subject?.Id)
            .ForCondition(subjectId => subjectId == id)
            .FailWith("Expected {context:widget} to have id {0}{reason}, but found {1}.",
                _ => id, widgetId => widgetId);

        return new AndConstraint<WidgetBaseAssertions>(this);
    }
}