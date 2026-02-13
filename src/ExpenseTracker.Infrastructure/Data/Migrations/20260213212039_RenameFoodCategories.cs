using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameFoodCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Categories SET Name = 'Groceries', Description = 'Supermarket, food supplies', Icon = '🛒' WHERE Name = 'Comida'");
            migrationBuilder.Sql("UPDATE Categories SET Name = 'Restaurants & Bars', Description = 'Eating out, delivery, bars, coffee shops', Icon = '🍽️' WHERE Name = 'Food & Dining'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Categories SET Name = 'Comida', Description = 'Almuerzo, delivery, snacks', Icon = '🍕' WHERE Name = 'Groceries'");
            migrationBuilder.Sql("UPDATE Categories SET Name = 'Food & Dining', Description = 'Restaurants, groceries, coffee shops', Icon = '🍔' WHERE Name = 'Restaurants & Bars'");
        }
    }
}
