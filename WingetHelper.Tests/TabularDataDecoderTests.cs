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
            "AppA            AppA.Id             1.0      1.1          winget",
            "AppB            AppB.Id             2.0      2.1          winget",
            "2 upgrades available."
        };

        var rows = TabularDataDecoder.ParseResultsTable<WingetPackageEntry>(commandOutput).ToList();

        Assert.Equal(2, rows.Count);
        AssertPackage(rows[0], "AppA", "AppA.Id", "1.0", "1.1", "winget");
        AssertPackage(rows[1], "AppB", "AppB.Id", "2.0", "2.1", "winget");
    }

    [Fact]
    public void ParseResultsTable_IgnoresTrailingNonTableSummaryLines()
    {
        var commandOutput = new[]
        {
            "Name            Id                  Version  Available    Source",
            "----------------------------------------------------------------",
            "AppA            AppA.Id             1.0      1.1          winget",
            "AppB            AppB.Id             2.0      2.1          winget",
            "4 upgrades available.",
            "1 package(s) have version numbers that cannot be determined. Use --include-unknown to see all results."
        };

        var rows = TabularDataDecoder.ParseResultsTable<WingetPackageEntry>(commandOutput).ToList();

        Assert.Equal(2, rows.Count);
        AssertPackage(rows[0], "AppA", "AppA.Id", "1.0", "1.1", "winget");
        AssertPackage(rows[1], "AppB", "AppB.Id", "2.0", "2.1", "winget");
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
