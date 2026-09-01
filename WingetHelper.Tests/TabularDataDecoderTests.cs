using WingetHelper.Decoders;
using WingetHelper.Models;

namespace WingetHelper.Tests;

public class TabularDataDecoderTests
{
    [Fact]
    public void ParseResultsTable_ParsesRegularTableRows()
    {
        var commandOutput = new[]
        {
            "Name            Id                  Version  Available    Source",
            "----------------------------------------------------------------",
            "AppA            AppA.Id             1.0      1.1          sourceA",
            "AppB            AppB.Id             2.0      2.1          sourceA",
            "2 upgrades available."
        };

        var rows = TabularDataDecoder.ParseResultsTable<WingetPackageEntry>(commandOutput).ToList();

        Assert.Equal(2, rows.Count);
        AssertPackage(rows[0], "AppA", "AppA.Id", "1.0", "1.1", "sourceA");
        AssertPackage(rows[1], "AppB", "AppB.Id", "2.0", "2.1", "sourceA");
    }

    [Fact]
    public void ParseResultsTable_IgnoresTrailingNonTableSummaryLines()
    {
        var commandOutput = new[]
        {
            "Name            Id                  Version  Available    Source",
            "----------------------------------------------------------------",
            "AppA            AppA.Id             1.0      1.1          sourceA",
            "AppB            AppB.Id             2.0      2.1          sourceA",
            "4 upgrades available.",
            "1 package(s) have version numbers that cannot be determined. Use --include-unknown to see all results."
        };

        var rows = TabularDataDecoder.ParseResultsTable<WingetPackageEntry>(commandOutput).ToList();

        Assert.Equal(2, rows.Count);
        AssertPackage(rows[0], "AppA", "AppA.Id", "1.0", "1.1", "sourceA");
        AssertPackage(rows[1], "AppB", "AppB.Id", "2.0", "2.1", "sourceA");
    }

    [Fact]
    public void ParseResultsTable_ParsesPackageSources()
    {
        var commandOutput = new[]
        {
            "Name".PadRight(9) + "Argument".PadRight(45) + "Type",
            new string('-', 58),
            "sourceA".PadRight(9) + "https://source-a.example.test/path".PadRight(45) + "Provider.TypeA",
            "sourceB".PadRight(9) + "https://source-b.example.test/path".PadRight(45) + "Provider.TypeB"
        };

        var rows = TabularDataDecoder.ParseResultsTable<WingetPackageSource>(commandOutput).ToList();

        Assert.Equal(2, rows.Count);
        Assert.Equal("sourceA", rows[0].Name);
        Assert.Equal("https://source-a.example.test/path", rows[0].Argument);
        Assert.Equal("Provider.TypeA", rows[0].Type);
        Assert.Equal("sourceB", rows[1].Name);
        Assert.Equal("https://source-b.example.test/path", rows[1].Argument);
        Assert.Equal("Provider.TypeB", rows[1].Type);
    }

    [Fact]
    public void ParseResultsTable_IgnoresColumnsWithoutMatchingProperties()
    {
        var commandOutput = new[]
        {
            "Name            Id                  Version  Available    Source  Pinned",
            "------------------------------------------------------------------------",
            "AppA            AppA.Id             1.0      1.1          sourceA true"
        };

        var rows = TabularDataDecoder.ParseResultsTable<WingetPackageEntry>(commandOutput).ToList();

        var package = Assert.Single(rows);
        AssertPackage(package, "AppA", "AppA.Id", "1.0", "1.1", "sourceA");
    }

    [Fact]
    public void ParseResultsTable_ParsesUnicodeWhitespaceAndTextElements()
    {
        const string emSpace = "\u2003";
        var commandOutput = new[]
        {
            $"Name{emSpace}{emSpace}{emSpace}Id{emSpace}{emSpace}{emSpace}Version{emSpace}{emSpace}{emSpace}Available{emSpace}{emSpace}{emSpace}Source",
            new string('-', 50),
            $"Café{emSpace}{emSpace}{emSpace}Foo.😀{"1.0".PadRight(10, '\u2003')}{"1.1".PadRight(12, '\u2003')}sourceA"
        };

        var package = Assert.Single(TabularDataDecoder.ParseResultsTable<WingetPackageEntry>(commandOutput));

        AssertPackage(package, "Café", "Foo.😀", "1.0", "1.1", "sourceA");
    }

    private static void AssertPackage(
        WingetPackageEntry package,
        string name,
        string id,
        string version,
        string available,
        string source)
    {
        Assert.Equal(name, package.Name);
        Assert.Equal(id, package.Id);
        Assert.Equal(version, package.Version);
        Assert.Equal(available, package.Available);
        Assert.Equal(source, package.Source);
    }
}
