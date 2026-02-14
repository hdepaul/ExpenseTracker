using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGymCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Gym')
                INSERT INTO Categories (Id, Name, Description, Icon, Color, IsDefault, UserId, CreatedAt, UpdatedAt)
                VALUES (NEWID(), 'Gym', 'Gym memberships and fitness', N'🏋️', '#FF6347', 1, NULL, GETUTCDATE(), GETUTCDATE())
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Categories WHERE Name = 'Gym' AND UserId IS NULL");
        }
    }
}
