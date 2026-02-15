using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Delivery')
                INSERT INTO Categories (Id, Name, Description, Icon, Color, IsDefault, UserId, CreatedAt, UpdatedAt)
                VALUES (NEWID(), 'Delivery', 'Food delivery services', N'🛵', '#FF9F43', 1, NULL, GETUTCDATE(), GETUTCDATE())
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Categories WHERE Name = 'Delivery' AND UserId IS NULL");
        }
    }
}
