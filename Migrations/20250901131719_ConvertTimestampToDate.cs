using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Debt_Tracking_System.Migrations
{
    /// <inheritdoc />
    public partial class ConvertTimestampToDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""Customers"" ALTER COLUMN ""CreatedAt"" TYPE date USING ""CreatedAt""::date;");
            migrationBuilder.Sql(@"ALTER TABLE ""Transactions"" ALTER COLUMN ""Date"" TYPE date USING ""Date""::date;");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""Customers"" ALTER COLUMN ""CreatedAt"" TYPE timestamp with time zone USING ""CreatedAt""::timestamp;");
            migrationBuilder.Sql(@"ALTER TABLE ""Transactions"" ALTER COLUMN ""Date"" TYPE timestamp with time zone USING ""Date""::timestamp;");
        }
    }
}
