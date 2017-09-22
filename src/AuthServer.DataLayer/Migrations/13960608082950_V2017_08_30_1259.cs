using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace AuthServer.DataLayer.Migrations
{
     public partial class V2017_08_30_1259 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TitleFa=table.Column<string>(type:"nvarchar(450)",maxLength:450,nullable:false),
                    IsActive=table.Column<bool>(type:"bit",nullable:false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FirstName=table.Column<string>(type:"nvarchar(450)",maxLength:450,nullable:false),
                    LastName=table.Column<string>(type:"nvarchar(450)",maxLength:450,nullable:false),
                    Email=table.Column<string>(type:"nvarchar(450)",maxLength:450,nullable:false),
                    EmailConfirmed=table.Column<bool>(type:"bit",nullable:false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoggedIn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    JoinTimespan=table.Column<DateTimeOffset>(type:"datetimeoffset",nullable:false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    InvalidLoginAttemptCount= table.Column<int>(type:"int",nullable:false),
                    IsLocked=table.Column<bool>(type:"bit",nullable:false),
                    LockTimespan=table.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true),
                    RequireRecaptcha= table.Column<bool>(type:"bit",nullable:false),
                    IncludeThisRecord= table.Column<bool>(type:"bit",nullable:false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccessTokenExpiresDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AccessTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiresDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RefreshTokenIdHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    IsActive=table.Column<bool>(type:"bit",nullable:false),
                    InsertBy=table.Column<Guid>(type:"uniqueidentifier",nullable:false),                   
                    InsertTimespan=table.Column<DateTimeOffset>(type:"datetimeoffset",nullable:false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthLevel1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AppBoundaryCode = table.Column<int>(type: "int", nullable: false),
                    AppBoundaryTitle = table.Column<int>(type: "nvarchar(255)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthLevel1", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthLevel2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AuthLeve1Id = table.Column<int>(type: "int", nullable: false),
                    IconClass = table.Column<int>(type: "nvarchar(255)", nullable: false),
                    Title = table.Column<int>(type: "nvarchar(255)", nullable: false),
                    ElementId = table.Column<int>(type: "nvarchar(255)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthLevel2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthLevel1_AuthLevel2_AuthLevel1Id",
                        column: x => x.AuthLeve1Id,
                        principalTable: "AuthLevel1",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthLevel3",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AuthLevel2Id = table.Column<int>(type: "int", nullable: false),
                    Domain = table.Column<int>(type: "nvarchar(255)", nullable: false),
                    PreRoute = table.Column<int>(type: "nvarchar(255)", nullable: false),
                    Parameters = table.Column<int>(type: "nvarchar(255)", nullable: false),
                    Controller = table.Column<int>(type: "nvarchar(255)", nullable: false),
                    Action = table.Column<int>(type: "nvarchar(255)", nullable: false),
                    Title = table.Column<int>(type: "nvarchar(255)", nullable: false),
                    ElementId = table.Column<int>(type: "nvarchar(255)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthLevel3", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthLevel2_AuthLevel3_AuthLevel2Id",
                        column: x => x.AuthLevel2Id,
                        principalTable: "AuthLevel2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthLevel4",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AuthLevel3Id = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<int>(type: "nvarchar(255)", nullable: false),
                    Value = table.Column<int>(type: "nvarchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthLevel4", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthLevel3_AuthLevel4_AuthLevel3Id",
                        column: x => x.AuthLevel3Id,
                        principalTable: "AuthLevel3",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EnableValidIpRecaptcha = table.Column<bool>(type: "bit", nullable: false),
                    RequireRecaptchaInvalidAttempts = table.Column<int>(type: "int", nullable: false),
                    LockInvalidAttempts = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MinPasswordLength=table.Column<int>(type:"int",nullable:false),
                    PasswordContainsNumber = table.Column<bool>(type: "bit", nullable: false),
                    PasswordContainsLowercase = table.Column<bool>(type: "bit", nullable: false),
                    PasswordContainsUppercase = table.Column<bool>(type: "bit", nullable: false),
                    PasswordContainsNonAlphaNumeric = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id);                   
                });

            migrationBuilder.CreateTable(
                name: "Browsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    TitleEng = table.Column<string>(type: "nvarchar(31)", nullable: false),
                    TitleFa = table.Column<string>(type: "nvarchar(31)", nullable: false),
                    IconClass=table.Column<string>(type:"varchar(31)",nullable:false),
                    AcceptRequestFrom = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Browser", x => x.Id);                   
                });

            migrationBuilder.CreateTable(
                name: "OSes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(31)", nullable: false),                   
                    IconClass=table.Column<string>(type:"varchar(31)",nullable:false),
                    AcceptRequestFrom = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Os", x => x.Id);                   
                });

            migrationBuilder.CreateTable(
                name: "Logins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrowserId = table.Column<int>(type: "int", nullable: true),
                    LoginTimespan = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LoginIP = table.Column<string>(type: "varchar(63)", nullable: false),
                    WasSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    OsId=table.Column<int>(type:"int",nullable:true),
                    BrowserVersion=table.Column<string>(type:"nvarchar(31)",nullable:true),
                    OsVersion=table.Column<string>(type:"nvarchar(31)",nullable:true),
                    OsTitle=table.Column<string>(type:"nvarchar(31)",nullable:true),
                    BrowserTitle=table.Column<string>(type:"nvarchar(31)",nullable:true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Logins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Logins_Browsers_BrowserId",
                        column: x => x.BrowserId,
                        principalTable: "Browsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Logins_OSes_OsId",
                        column: x => x.OsId,
                        principalTable: "OSes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
          
            //----------------------------------------------- Index----------------------------------------
            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_UserId",
                table: "UserTokens",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Logins_UserId",
                table: "Logins",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name:"UserClaims"
            );

            migrationBuilder.DropTable(
                name:"AuthLevel1"
            );
            migrationBuilder.DropTable(
                name:"AuthLevel2"
            );
            migrationBuilder.DropTable(
                name:"AuthLevel3"
            );
            migrationBuilder.DropTable(
                name:"AuthLevel4"
            );
             migrationBuilder.DropTable(
                name:"Policies"
            );
             migrationBuilder.DropTable(
                name:"Browsers"
            );
             migrationBuilder.DropTable(
                name:"Logins"
            );
             migrationBuilder.DropTable(
                name:"OSes"
            );
        }
    }
}
