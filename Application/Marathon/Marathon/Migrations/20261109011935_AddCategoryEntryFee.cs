using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marathon.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryEntryFee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The Category model gained EntryFee, but no earlier migration ever
            // created the backing column, so existing databases are missing it.
            migrationBuilder.AddColumn<decimal>(
                name: "EntryFee",
                table: "categories",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "EntryFee", table: "categories");
        }
    }
}
