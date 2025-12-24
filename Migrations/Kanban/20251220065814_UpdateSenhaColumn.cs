using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kanban.Migrations.Kanban
{
    /// <inheritdoc />
    public partial class UpdateSenhaColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) { migrationBuilder.AlterColumn<string>(name: "Senha", table: "Users", type: "varchar(255)", nullable: false, oldClrType: typeof(string), oldType: "longtext"); }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
