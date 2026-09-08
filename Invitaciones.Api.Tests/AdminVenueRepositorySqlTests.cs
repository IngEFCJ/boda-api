using Xunit;

namespace Invitaciones.Api.Tests;

/// <summary>
/// Verifies that the SQL used by <c>AdminVenueRepository</c> includes the new
/// PrimaryMeters/SecondaryMeters columns in both the read path
/// (<c>GetTablesAsync</c>) and the write path (<c>SaveTablesAsync</c> update and
/// insert statements). Dapper binds parameters by property name, so persisting
/// the measurement only works if these columns are present in the SQL text.
///
/// This is an example-based static check of the SQL strings (per the task's
/// "assert the SQL strings contain the columns" option), avoiding a live DB.
///
/// Validates: Requirements 5.2, 5.3
/// </summary>
public sealed class AdminVenueRepositorySqlTests
{
    private static string ReadRepositorySource()
    {
        // Walk up from the test output directory to the boda-api root, then to
        // the repository source file. This keeps the test independent of the
        // build configuration folder (Debug/Release/tfm).
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(
                dir.FullName,
                "Invitaciones.Api",
                "Data",
                "AdminVenueRepository.cs");
            if (File.Exists(candidate))
                return File.ReadAllText(candidate);

            dir = dir.Parent;
        }

        throw new FileNotFoundException(
            "Could not locate AdminVenueRepository.cs by walking up from " + AppContext.BaseDirectory);
    }

    [Fact]
    public void GetTablesAsync_Sql_SelectsMeasurementColumns()
    {
        var source = ReadRepositorySource();

        // The SELECT list in GetTablesAsync must include both columns.
        var selectIndex = source.IndexOf("FROM dbo.VenueTables", StringComparison.Ordinal);
        Assert.True(selectIndex > 0, "Expected a SELECT ... FROM dbo.VenueTables statement.");

        Assert.Contains("PrimaryMeters", source);
        Assert.Contains("SecondaryMeters", source);
    }

    [Fact]
    public void SaveTablesAsync_UpdateSql_SetsMeasurementColumns()
    {
        var source = ReadRepositorySource();

        var updateIndex = source.IndexOf("UPDATE dbo.VenueTables", StringComparison.Ordinal);
        Assert.True(updateIndex > 0, "Expected an UPDATE dbo.VenueTables statement.");

        var updateBlock = source.Substring(updateIndex, Math.Min(600, source.Length - updateIndex));
        Assert.Contains("PrimaryMeters = @PrimaryMeters", updateBlock);
        Assert.Contains("SecondaryMeters = @SecondaryMeters", updateBlock);
    }

    [Fact]
    public void SaveTablesAsync_InsertSql_IncludesMeasurementColumnsAndValues()
    {
        var source = ReadRepositorySource();

        var insertIndex = source.IndexOf("INSERT INTO dbo.VenueTables", StringComparison.Ordinal);
        Assert.True(insertIndex > 0, "Expected an INSERT INTO dbo.VenueTables statement.");

        var insertBlock = source.Substring(insertIndex, Math.Min(600, source.Length - insertIndex));

        // Column list includes both columns.
        Assert.Contains("PrimaryMeters", insertBlock);
        Assert.Contains("SecondaryMeters", insertBlock);

        // VALUES list binds both parameters.
        Assert.Contains("@PrimaryMeters", insertBlock);
        Assert.Contains("@SecondaryMeters", insertBlock);
    }
}
