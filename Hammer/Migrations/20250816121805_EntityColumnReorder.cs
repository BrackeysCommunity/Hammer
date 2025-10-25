using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hammer.Migrations
{
    /// <inheritdoc />
    public partial class EntityColumnReorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "TrackedMessages",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)")
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "TrackedMessages",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeletionTimestamp",
                table: "TrackedMessages",
                type: "DATETIME(6)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreationTimestamp",
                table: "TrackedMessages",
                type: "DATETIME(6)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)")
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "TrackedMessages",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<ulong>(
                name: "ChannelId",
                table: "TrackedMessages",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<ulong>(
                name: "AuthorId",
                table: "TrackedMessages",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Attachments",
                table: "TrackedMessages",
                type: "longblob",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob")
                .Annotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<ulong>(
                name: "Id",
                table: "TrackedMessages",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ExpiresAt",
                table: "TemporaryBan",
                type: "DATETIME(6)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "TemporaryBan",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "TemporaryBan",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<ulong>(
                name: "StaffMemberId",
                table: "StaffMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<ulong>(
                name: "RecipientId",
                table: "StaffMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "StaffMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "StaffMessage",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "StaffMessage",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Rule",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Brief",
                table: "Rule",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "Rule",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Rule",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<ulong>(
                name: "ReporterId",
                table: "ReportedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<ulong>(
                name: "MessageId",
                table: "ReportedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "ReportedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "ReportedMessage",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<ulong>(
                name: "ChannelId",
                table: "ReportedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<ulong>(
                name: "AuthorId",
                table: "ReportedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Attachments",
                table: "ReportedMessage",
                type: "longblob",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob")
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "ReportedMessage",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ExpiresAt",
                table: "Mute",
                type: "DATETIME(6)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "Mute",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Mute",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "MemberNote",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "MemberNote",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "MemberNote",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreationTimestamp",
                table: "MemberNote",
                type: "DATETIME(6)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)")
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "MemberNote",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<ulong>(
                name: "AuthorId",
                table: "MemberNote",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "MemberNote",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Infraction",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Infraction",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<ulong>(
                name: "StaffMemberId",
                table: "Infraction",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "RuleText",
                table: "Infraction",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:ColumnOrder", 11)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "RuleId",
                table: "Infraction",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Infraction",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "IssuedAt",
                table: "Infraction",
                type: "DATETIME(6)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)")
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "Infraction",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalInformation",
                table: "Infraction",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:ColumnOrder", 9)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Infraction",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<ulong>(
                name: "StaffMemberId",
                table: "DeletedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "DeletedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeletionTimestamp",
                table: "DeletedMessage",
                type: "DATETIME(6)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)")
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreationTimestamp",
                table: "DeletedMessage",
                type: "DATETIME(6)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)")
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "DeletedMessage",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<ulong>(
                name: "ChannelId",
                table: "DeletedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<ulong>(
                name: "AuthorId",
                table: "DeletedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Attachments",
                table: "DeletedMessage",
                type: "longblob",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob")
                .Annotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<string>(
                name: "AddedByBot",
                table: "DeletedMessage",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:ColumnOrder", 10)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<ulong>(
                name: "MessageId",
                table: "DeletedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<ulong>(
                name: "StaffMemberId",
                table: "BlockedReporter",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "BlockedAt",
                table: "BlockedReporter",
                type: "DATETIME(6)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)")
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "BlockedReporter",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "BlockedReporter",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<ulong>(
                name: "StaffMemberId",
                table: "AltAccount",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "RegisteredAt",
                table: "AltAccount",
                type: "DATETIME(6)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)")
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<ulong>(
                name: "AltId",
                table: "AltAccount",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "AltAccount",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("Relational:ColumnOrder", 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "TrackedMessages",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)")
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "TrackedMessages",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeletionTimestamp",
                table: "TrackedMessages",
                type: "DATETIME(6)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreationTimestamp",
                table: "TrackedMessages",
                type: "DATETIME(6)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)")
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "TrackedMessages",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<ulong>(
                name: "ChannelId",
                table: "TrackedMessages",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<ulong>(
                name: "AuthorId",
                table: "TrackedMessages",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Attachments",
                table: "TrackedMessages",
                type: "longblob",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob")
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<ulong>(
                name: "Id",
                table: "TrackedMessages",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ExpiresAt",
                table: "TemporaryBan",
                type: "DATETIME(6)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "TemporaryBan",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "TemporaryBan",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<ulong>(
                name: "StaffMemberId",
                table: "StaffMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<ulong>(
                name: "RecipientId",
                table: "StaffMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "StaffMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "StaffMessage",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "StaffMessage",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Rule",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "Brief",
                table: "Rule",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "Rule",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Rule",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<ulong>(
                name: "ReporterId",
                table: "ReportedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<ulong>(
                name: "MessageId",
                table: "ReportedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "ReportedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "ReportedMessage",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<ulong>(
                name: "ChannelId",
                table: "ReportedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<ulong>(
                name: "AuthorId",
                table: "ReportedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Attachments",
                table: "ReportedMessage",
                type: "longblob",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob")
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "ReportedMessage",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ExpiresAt",
                table: "Mute",
                type: "DATETIME(6)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "Mute",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Mute",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "MemberNote",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "MemberNote",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "MemberNote",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreationTimestamp",
                table: "MemberNote",
                type: "DATETIME(6)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)")
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "MemberNote",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<ulong>(
                name: "AuthorId",
                table: "MemberNote",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "MemberNote",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Infraction",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Infraction",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<ulong>(
                name: "StaffMemberId",
                table: "Infraction",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "RuleText",
                table: "Infraction",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<int>(
                name: "RuleId",
                table: "Infraction",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Infraction",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "IssuedAt",
                table: "Infraction",
                type: "DATETIME(6)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)")
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "Infraction",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalInformation",
                table: "Infraction",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Infraction",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<ulong>(
                name: "StaffMemberId",
                table: "DeletedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "DeletedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeletionTimestamp",
                table: "DeletedMessage",
                type: "DATETIME(6)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)")
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreationTimestamp",
                table: "DeletedMessage",
                type: "DATETIME(6)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)")
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "DeletedMessage",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<ulong>(
                name: "ChannelId",
                table: "DeletedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<ulong>(
                name: "AuthorId",
                table: "DeletedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Attachments",
                table: "DeletedMessage",
                type: "longblob",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob")
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<string>(
                name: "AddedByBot",
                table: "DeletedMessage",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<ulong>(
                name: "MessageId",
                table: "DeletedMessage",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<ulong>(
                name: "StaffMemberId",
                table: "BlockedReporter",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "BlockedAt",
                table: "BlockedReporter",
                type: "DATETIME(6)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)")
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "BlockedReporter",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "BlockedReporter",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<ulong>(
                name: "StaffMemberId",
                table: "AltAccount",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "RegisteredAt",
                table: "AltAccount",
                type: "DATETIME(6)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME(6)")
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<ulong>(
                name: "AltId",
                table: "AltAccount",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "AltAccount",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("Relational:ColumnOrder", 1);
        }
    }
}
