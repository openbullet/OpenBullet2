using OpenBullet2.Native.Extensions;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace OpenBullet2.Native.Tests;

[Collection("WPF")]
public sealed class ListBoxExtensionsTests(WpfAppFixture fixture)
{
    [Fact]
    public async Task GetSelectedItemsInDisplayOrder_ReturnsItemsInVisibleOrder()
    {
        await fixture.InvokeAsync(_ =>
        {
            var items = new[]
            {
                new TestItem("first"),
                new TestItem("second"),
                new TestItem("third")
            };

            var listView = new ListView
            {
                SelectionMode = SelectionMode.Extended,
                ItemsSource = items
            };

            listView.SelectedItems.Add(items[2]);
            listView.SelectedItems.Add(items[0]);

            var selected = listView.GetSelectedItemsInDisplayOrder<TestItem>();

            Assert.Equal([items[0], items[2]], selected);
        });
    }

    [Fact]
    public async Task GetSelectedItemsInDisplayOrder_ExcludesItemsNoLongerInTheList()
    {
        await fixture.InvokeAsync(_ =>
        {
            var items = new ObservableCollection<TestItem>
            {
                new("first"),
                new("second"),
                new("third")
            };

            var listView = new ListView
            {
                SelectionMode = SelectionMode.Extended,
                ItemsSource = items
            };

            listView.SelectedItems.Add(items[1]);
            items.RemoveAt(1);

            var selected = listView.GetSelectedItemsInDisplayOrder<TestItem>();

            Assert.Empty(selected);
        });
    }

    private sealed record TestItem(string Value);
}
