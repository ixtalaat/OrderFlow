using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductFullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The stock SQL Server Linux image ships no Full-Text Engine
            // (error 7609 on CREATE FULLTEXT INDEX), so this is a no-op
            // there. On FTS-capable servers the catalog/index are created.
            migrationBuilder.Sql(@"
-- Dynamic SQL: CREATE FULLTEXT is validated at batch-compile time, so it
-- must only ever compile on servers with the engine installed.
IF (FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') = 1)
    EXEC(N'
    IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = ''FTCatalog'')
        CREATE FULLTEXT CATALOG FTCatalog AS DEFAULT;
    IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N''[dbo].[Products]''))
        CREATE FULLTEXT INDEX ON [dbo].[Products]([Name] LANGUAGE 1033, [Description] LANGUAGE 1033, [Sku] LANGUAGE 1033)
        KEY INDEX [PK_Products] ON FTCatalog WITH CHANGE_TRACKING AUTO;
    ');
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'[dbo].[Products]'))
    DROP FULLTEXT INDEX ON [dbo].[Products];
IF EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'FTCatalog')
    DROP FULLTEXT CATALOG FTCatalog;
");
        }
    }
}
