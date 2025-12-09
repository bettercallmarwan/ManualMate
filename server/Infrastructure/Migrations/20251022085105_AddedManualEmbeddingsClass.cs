using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManualMate.Migrations
{
    /// <inheritdoc />
    public partial class AddedManualEmbeddingsClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManualEmbedding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    TextChunk = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChunkIndex = table.Column<int>(type: "int", nullable: false),
                    EmbeddingJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManualEmbedding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManualEmbedding_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManualEmbedding_ProductId",
                table: "ManualEmbedding",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManualEmbedding");
        }
    }
}
