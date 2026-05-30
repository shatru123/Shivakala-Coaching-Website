namespace Shivakala.Core.ViewModels;

public sealed class AdminListPageViewModel<T>
{
    public string Title { get; set; } = string.Empty;

    public IReadOnlyList<T> Items { get; set; } = [];
}
