#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using TombLib.WPF;

namespace WadTool.Tests;

[TestClass]
public class RangeObservableCollectionTests
{
    [TestMethod]
    public void ReplaceAllRaisesASingleReset()
    {
        var collection = new RangeObservableCollection<int> { 1, 2, 3 };
        var events = 0;
        NotifyCollectionChangedAction? action = null;

        collection.CollectionChanged += (_, e) =>
        {
            events++;
            action = e.Action;
        };

        collection.ReplaceAll([4, 5, 6, 7]);

        Assert.AreEqual(1, events);
        Assert.AreEqual(NotifyCollectionChangedAction.Reset, action);
        CollectionAssert.AreEqual(new[] { 4, 5, 6, 7 }, collection);
    }
}
