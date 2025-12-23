using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LibrarySystem.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchToBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "loans_book_id_fkey",
                table: "loans");

            migrationBuilder.DropForeignKey(
                name: "loans_user_id_fkey",
                table: "loans");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.AddColumn<int>(
                name: "LibraryBranchId",
                table: "books",
                type: "integer",
                nullable: true);

            
            migrationBuilder.CreateIndex(
                name: "IX_books_LibraryBranchId",
                table: "books",
                column: "LibraryBranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_books_LibraryBranches_LibraryBranchId",
                table: "books",
                column: "LibraryBranchId",
                principalTable: "LibraryBranches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_loans_books_book_id",
                table: "loans",
                column: "book_id",
                principalTable: "books",
                principalColumn: "book_id");

            migrationBuilder.AddForeignKey(
                name: "FK_loans_users_user_id",
                table: "loans",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_books_LibraryBranches_LibraryBranchId",
                table: "books");

            migrationBuilder.DropForeignKey(
                name: "FK_loans_books_book_id",
                table: "loans");

            migrationBuilder.DropForeignKey(
                name: "FK_loans_users_user_id",
                table: "loans");

            migrationBuilder.DropTable(
                name: "LibraryBranches");

            migrationBuilder.DropIndex(
                name: "IX_books_LibraryBranchId",
                table: "books");

            migrationBuilder.DropColumn(
                name: "LibraryBranchId",
                table: "books");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.AddForeignKey(
                name: "loans_book_id_fkey",
                table: "loans",
                column: "book_id",
                principalTable: "books",
                principalColumn: "book_id");

            migrationBuilder.AddForeignKey(
                name: "loans_user_id_fkey",
                table: "loans",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id");
        }
    }
}
