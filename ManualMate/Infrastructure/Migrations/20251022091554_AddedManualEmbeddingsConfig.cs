using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManualMate.Migrations
{
    /// <inheritdoc />
    public partial class AddedManualEmbeddingsConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ManualEmbedding_Products_ProductId",
                table: "ManualEmbedding");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ManualEmbedding",
                table: "ManualEmbedding");

            migrationBuilder.RenameTable(
                name: "ManualEmbedding",
                newName: "ManualEmbeddings");

            migrationBuilder.RenameIndex(
                name: "IX_ManualEmbedding_ProductId",
                table: "ManualEmbeddings",
                newName: "IX_ManualEmbeddings_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ManualEmbeddings",
                table: "ManualEmbeddings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ManualEmbeddings_Products_ProductId",
                table: "ManualEmbeddings",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ManualEmbeddings_Products_ProductId",
                table: "ManualEmbeddings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ManualEmbeddings",
                table: "ManualEmbeddings");

            migrationBuilder.RenameTable(
                name: "ManualEmbeddings",
                newName: "ManualEmbedding");

            migrationBuilder.RenameIndex(
                name: "IX_ManualEmbeddings_ProductId",
                table: "ManualEmbedding",
                newName: "IX_ManualEmbedding_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ManualEmbedding",
                table: "ManualEmbedding",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ManualEmbedding_Products_ProductId",
                table: "ManualEmbedding",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
