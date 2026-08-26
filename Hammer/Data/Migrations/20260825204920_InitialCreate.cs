using System;
using Hammer.Data;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hammer.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "hammer");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:hammer.infraction_type", "ban,gag,kick,message_deletion,mute,temporary_ban,temporary_mute,warning");

            migrationBuilder.CreateTable(
                name: "alt_accounts",
                schema: "hammer",
                columns: table => new
                {
                    alt_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    registered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    staff_member_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alt_accounts", x => new { x.user_id, x.alt_id });
                });

            migrationBuilder.CreateTable(
                name: "blocked_reporters",
                schema: "hammer",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    blocked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    staff_member_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blocked_reporters", x => new { x.user_id, x.guild_id });
                });

            migrationBuilder.CreateTable(
                name: "deleted_messages",
                schema: "hammer",
                columns: table => new
                {
                    message_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    attachments = table.Column<byte[]>(type: "bytea", nullable: false),
                    author_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    content = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    creation_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deletion_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    staff_member_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deleted_messages", x => x.message_id);
                });

            migrationBuilder.CreateTable(
                name: "infractions",
                schema: "hammer",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    additional_information = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    rule_id = table.Column<int>(type: "integer", nullable: true),
                    rule_text = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    staff_member_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    type = table.Column<InfractionType>(type: "hammer.infraction_type", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_infractions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "member_notes",
                schema: "hammer",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    author_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    content = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    creation_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_member_notes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mutes",
                schema: "hammer",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mutes", x => new { x.user_id, x.guild_id });
                });

            migrationBuilder.CreateTable(
                name: "reported_messages",
                schema: "hammer",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    attachments = table.Column<byte[]>(type: "bytea", nullable: false),
                    author_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    message_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    reporter_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reported_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rules",
                schema: "hammer",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    brief = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rules", x => new { x.id, x.guild_id });
                });

            migrationBuilder.CreateTable(
                name: "staff_messages",
                schema: "hammer",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    recipient_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    staff_member_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_staff_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "temporary_bans",
                schema: "hammer",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_temporary_bans", x => new { x.user_id, x.guild_id });
                });

            migrationBuilder.CreateTable(
                name: "tracked_messages",
                schema: "hammer",
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    attachments = table.Column<byte[]>(type: "bytea", nullable: false),
                    author_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    creation_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deletion_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tracked_messages", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alt_accounts",
                schema: "hammer");

            migrationBuilder.DropTable(
                name: "blocked_reporters",
                schema: "hammer");

            migrationBuilder.DropTable(
                name: "deleted_messages",
                schema: "hammer");

            migrationBuilder.DropTable(
                name: "infractions",
                schema: "hammer");

            migrationBuilder.DropTable(
                name: "member_notes",
                schema: "hammer");

            migrationBuilder.DropTable(
                name: "mutes",
                schema: "hammer");

            migrationBuilder.DropTable(
                name: "reported_messages",
                schema: "hammer");

            migrationBuilder.DropTable(
                name: "rules",
                schema: "hammer");

            migrationBuilder.DropTable(
                name: "staff_messages",
                schema: "hammer");

            migrationBuilder.DropTable(
                name: "temporary_bans",
                schema: "hammer");

            migrationBuilder.DropTable(
                name: "tracked_messages",
                schema: "hammer");
        }
    }
}
