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
        Assert.Equal("AppA", rows[0].Name);
        Assert.Equal("AppA.Id", rows[0].Id);
        Assert.Equal("1.0", rows[0].Version);
        Assert.Equal("1.1", rows[0].Available);
        Assert.Equal("winget", rows[0].Source);
        Assert.Equal("AppB", rows[1].Name);
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
        Assert.Equal("AppA", rows[0].Name);
        Assert.Equal("AppB", rows[1].Name);
    }
}
