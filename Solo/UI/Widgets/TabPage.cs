namespace Solo.UI.Widgets;

/// <summary>
/// One tab inside a <see cref="TabbedPanelWidget"/>.
/// </summary>
/// <param name="Title">Label shown in the tab strip.</param>
/// <param name="Content">Widget rendered when this tab is active. Becomes a child
/// of the <see cref="TabbedPanelWidget"/> on construction.</param>
public sealed record TabPage(string Title, Widget Content);
