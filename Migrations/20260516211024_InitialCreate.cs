using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuardianPet.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_pets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Species = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Breed = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Age = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Weight = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    AgeType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PetSize = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_pets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Password = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Cpf = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Phone = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_veterinarians",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Crmv = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Specialty = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Phone = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    YearsExperience = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_veterinarians", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pet_user",
                columns: table => new
                {
                    PetsId = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    UsersId = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pet_user", x => new { x.PetsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_pet_user_tb_pets_PetsId",
                        column: x => x.PetsId,
                        principalTable: "tb_pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pet_user_tb_users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "tb_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_consultations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    UserId = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    PetId = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    VeterinarianId = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ConsultationDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Symptoms = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Diagnosis = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Treatment = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Observations = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_consultations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_consultations_tb_pets_PetId",
                        column: x => x.PetId,
                        principalTable: "tb_pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_consultations_tb_users_UserId",
                        column: x => x.UserId,
                        principalTable: "tb_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_consultations_tb_veterinarians_VeterinarianId",
                        column: x => x.VeterinarianId,
                        principalTable: "tb_veterinarians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pet_user_UsersId",
                table: "pet_user",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_consultations_PetId",
                table: "tb_consultations",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_consultations_UserId",
                table: "tb_consultations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_consultations_VeterinarianId",
                table: "tb_consultations",
                column: "VeterinarianId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_users_Cpf",
                table: "tb_users",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_users_Email",
                table: "tb_users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_veterinarians_Crmv",
                table: "tb_veterinarians",
                column: "Crmv",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_veterinarians_Email",
                table: "tb_veterinarians",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pet_user");

            migrationBuilder.DropTable(
                name: "tb_consultations");

            migrationBuilder.DropTable(
                name: "tb_pets");

            migrationBuilder.DropTable(
                name: "tb_users");

            migrationBuilder.DropTable(
                name: "tb_veterinarians");
        }
    }
}
