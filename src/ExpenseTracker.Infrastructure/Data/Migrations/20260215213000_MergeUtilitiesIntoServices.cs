using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MergeUtilitiesIntoServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Move all expenses from Utilities to Services
            migrationBuilder.Sql(@"
                UPDATE e
                SET e.CategoryId = s.Id
                FROM Expenses e
                INNER JOIN Categories u ON e.CategoryId = u.Id AND u.Name = 'Utilities' AND u.UserId IS NULL
                CROSS JOIN (SELECT TOP 1 Id FROM Categories WHERE Name = 'Services' AND UserId IS NULL) s
            ");

            // Delete the Utilities category
            migrationBuilder.Sql("DELETE FROM Categories WHERE Name = 'Utilities' AND UserId IS NULL");

            // Update Services description to reflect merged scope
            migrationBuilder.Sql(@"
                UPDATE Categories
                SET Description = 'Water, gas, electricity, internet, phone, professional services',
                    Icon = N'🔧'
                WHERE Name = 'Services' AND UserId IS NULL
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Re-create Utilities category
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Utilities')
                INSERT INTO Categories (Id, Name, Description, Icon, Color, IsDefault, UserId, CreatedAt, UpdatedAt)
                VALUES (NEWID(), 'Utilities', 'Electric, water, internet, phone', N'💡', '#98D8C8', 1, NULL, GETUTCDATE(), GETUTCDATE())
            ");
        }
    }
}
