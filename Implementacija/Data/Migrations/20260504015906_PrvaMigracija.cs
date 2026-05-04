using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bibliotecha.Data.Migrations
{
    /// <inheritdoc />
    public partial class PrvaMigracija : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Autor",
                columns: table => new
                {
                    IdAutora = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Autor", x => x.IdAutora);
                });

            migrationBuilder.CreateTable(
                name: "Korisnik",
                columns: table => new
                {
                    IdKorisnika = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrojClanskeKartice = table.Column<int>(type: "int", nullable: true),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumZaposlenja = table.Column<DateOnly>(type: "date", nullable: true),
                    DatumRegistracije = table.Column<DateOnly>(type: "date", nullable: true),
                    Uloga = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Korisnik", x => x.IdKorisnika);
                });

            migrationBuilder.CreateTable(
                name: "Knjiga",
                columns: table => new
                {
                    IdKnjige = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ISBN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Naslov = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AutorId = table.Column<int>(type: "int", nullable: false),
                    Zanr = table.Column<int>(type: "int", nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumIzdavanja = table.Column<DateOnly>(type: "date", nullable: false),
                    Izdavac = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BrojStranica = table.Column<int>(type: "int", nullable: false),
                    Jezik = table.Column<int>(type: "int", nullable: false),
                    KoricaKnjige = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProsjecnaOcjena = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Knjiga", x => x.IdKnjige);
                    table.ForeignKey(
                        name: "FK_Knjiga_Autor_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Autor",
                        principalColumn: "IdAutora",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Obavjestenje",
                columns: table => new
                {
                    IdObavjestenja = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KorisnikId = table.Column<int>(type: "int", nullable: false),
                    Poruka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumSlanja = table.Column<DateOnly>(type: "date", nullable: false),
                    VrstaObavjestenja = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Obavjestenje", x => x.IdObavjestenja);
                    table.ForeignKey(
                        name: "FK_Obavjestenje_Korisnik_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Korisnik",
                        principalColumn: "IdKorisnika",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Primjerak",
                columns: table => new
                {
                    IdPrimjerka = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    KnjigaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Primjerak", x => x.IdPrimjerka);
                    table.ForeignKey(
                        name: "FK_Primjerak_Knjiga_KnjigaId",
                        column: x => x.KnjigaId,
                        principalTable: "Knjiga",
                        principalColumn: "IdKnjige",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rezervacija",
                columns: table => new
                {
                    IdRezervacije = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KnjigaId = table.Column<int>(type: "int", nullable: false),
                    KorisnikId = table.Column<int>(type: "int", nullable: false),
                    DatumRezervacije = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PozicijaURedu = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rezervacija", x => x.IdRezervacije);
                    table.ForeignKey(
                        name: "FK_Rezervacija_Knjiga_KnjigaId",
                        column: x => x.KnjigaId,
                        principalTable: "Knjiga",
                        principalColumn: "IdKnjige",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Rezervacija_Korisnik_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Korisnik",
                        principalColumn: "IdKorisnika",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Posudba",
                columns: table => new
                {
                    IdPosudbe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrimjerakId = table.Column<int>(type: "int", nullable: false),
                    KorisnikId = table.Column<int>(type: "int", nullable: false),
                    DatumOnlinePosudbe = table.Column<DateOnly>(type: "date", nullable: true),
                    DatumPreuzimanja = table.Column<DateOnly>(type: "date", nullable: true),
                    RokVracanja = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    BrojProduzenja = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posudba", x => x.IdPosudbe);
                    table.ForeignKey(
                        name: "FK_Posudba_Korisnik_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Korisnik",
                        principalColumn: "IdKorisnika",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Posudba_Primjerak_PrimjerakId",
                        column: x => x.PrimjerakId,
                        principalTable: "Primjerak",
                        principalColumn: "IdPrimjerka",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Knjiga_AutorId",
                table: "Knjiga",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Obavjestenje_KorisnikId",
                table: "Obavjestenje",
                column: "KorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_Posudba_KorisnikId",
                table: "Posudba",
                column: "KorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_Posudba_PrimjerakId",
                table: "Posudba",
                column: "PrimjerakId");

            migrationBuilder.CreateIndex(
                name: "IX_Primjerak_KnjigaId",
                table: "Primjerak",
                column: "KnjigaId");

            migrationBuilder.CreateIndex(
                name: "IX_Rezervacija_KnjigaId",
                table: "Rezervacija",
                column: "KnjigaId");

            migrationBuilder.CreateIndex(
                name: "IX_Rezervacija_KorisnikId",
                table: "Rezervacija",
                column: "KorisnikId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Obavjestenje");

            migrationBuilder.DropTable(
                name: "Posudba");

            migrationBuilder.DropTable(
                name: "Rezervacija");

            migrationBuilder.DropTable(
                name: "Primjerak");

            migrationBuilder.DropTable(
                name: "Korisnik");

            migrationBuilder.DropTable(
                name: "Knjiga");

            migrationBuilder.DropTable(
                name: "Autor");
        }
    }
}
