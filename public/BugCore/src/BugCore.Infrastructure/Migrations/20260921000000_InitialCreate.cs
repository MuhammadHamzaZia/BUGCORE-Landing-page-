using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugCore.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. AspNetRoles
        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            });

        // 2. AspNetUsers (ApplicationUser)
        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                RealName = table.Column<string>(type: "TEXT", maxLength: 191, nullable: false),
                GlobalAccessLevel = table.Column<int>(type: "INTEGER", nullable: false),
                Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                Protected = table.Column<bool>(type: "INTEGER", nullable: false),
                DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                LastVisit = table.Column<DateTime>(type: "TEXT", nullable: true),
                UserName = table.Column<string>(type: "TEXT", maxLength: 191, nullable: true),
                NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "TEXT", maxLength: 191, nullable: true),
                NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            });

        // 3. BugCoreProjects
        migrationBuilder.CreateTable(
            name: "BugCoreProjects",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                ViewState = table.Column<int>(type: "INTEGER", nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                InheritCategories = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BugCoreProjects", x => x.Id);
            });

        // 4. BugCoreCustomFields
        migrationBuilder.CreateTable(
            name: "BugCoreCustomFields",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Type = table.Column<int>(type: "INTEGER", nullable: false),
                PossibleValues = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                DefaultValue = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                ValidRegexp = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                AccessLevelRead = table.Column<int>(type: "INTEGER", nullable: false),
                AccessLevelWrite = table.Column<int>(type: "INTEGER", nullable: false),
                LengthMin = table.Column<int>(type: "INTEGER", nullable: false),
                LengthMax = table.Column<int>(type: "INTEGER", nullable: false),
                RequireReport = table.Column<bool>(type: "INTEGER", nullable: false),
                RequireUpdate = table.Column<bool>(type: "INTEGER", nullable: false),
                RequireResolved = table.Column<bool>(type: "INTEGER", nullable: false),
                RequireClosed = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BugCoreCustomFields", x => x.Id);
            });

        // 5. BugCoreTags
        migrationBuilder.CreateTable(
            name: "BugCoreTags",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                UserId = table.Column<int>(type: "INTEGER", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BugCoreTags", x => x.Id);
                table.ForeignKey(
                    name: "FK_BugCoreTags_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        // 6. AspNetRoleClaims
        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                RoleId = table.Column<int>(type: "INTEGER", nullable: false),
                ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // 7. AspNetUserClaims
        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                UserId = table.Column<int>(type: "INTEGER", nullable: false),
                ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // 8. AspNetUserLogins
        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                UserId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // 9. AspNetUserRoles
        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                UserId = table.Column<int>(type: "INTEGER", nullable: false),
                RoleId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // 10. AspNetUserTokens
        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                UserId = table.Column<int>(type: "INTEGER", nullable: false),
                LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                Value = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // 11. BugCoreCategories
        migrationBuilder.CreateTable(
            name: "BugCoreCategories",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                ProjectId = table.Column<int>(type: "INTEGER", nullable: true),
                Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                DefaultAssigneeId = table.Column<int>(type: "INTEGER", nullable: true),
                Status = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BugCoreCategories", x => x.Id);
                table.ForeignKey(
                    name: "FK_BugCoreCategories_AspNetUsers_DefaultAssigneeId",
                    column: x => x.DefaultAssigneeId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_BugCoreCategories_BugCoreProjects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "BugCoreProjects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // 12. BugCoreCustomFieldProjects
        migrationBuilder.CreateTable(
            name: "BugCoreCustomFieldProjects",
            columns: table => new
            {
                CustomFieldId = table.Column<int>(type: "INTEGER", nullable: false),
                ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                Sequence = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BugCoreCustomFieldProjects", x => new { x.CustomFieldId, x.ProjectId });
                table.ForeignKey(
                    name: "FK_BugCoreCustomFieldProjects_BugCoreCustomFields_CustomFieldId",
                    column: x => x.CustomFieldId,
                    principalTable: "BugCoreCustomFields",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_BugCoreCustomFieldProjects_BugCoreProjects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "BugCoreProjects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // 13. BugCoreProjectHierarchies
        migrationBuilder.CreateTable(
            name: "BugCoreProjectHierarchies",
            columns: table => new
            {
                ParentProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                ChildProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                InheritChild = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BugCoreProjectHierarchies", x => new { x.ParentProjectId, x.ChildProjectId });
                table.ForeignKey(
                    name: "FK_BugCoreProjectHierarchies_BugCoreProjects_ChildProjectId",
                    column: x => x.ChildProjectId,
                    principalTable: "BugCoreProjects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_BugCoreProjectHierarchies_BugCoreProjects_ParentProjectId",
                    column: x => x.ParentProjectId,
                    principalTable: "BugCoreProjects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        // 14. BugCoreProjectUsers
        migrationBuilder.CreateTable(
            name: "BugCoreProjectUsers",
            columns: table => new
            {
                ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                UserId = table.Column<int>(type: "INTEGER", nullable: false),
                AccessLevel = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BugCoreProjectUsers", x => new { x.ProjectId, x.UserId });
                table.ForeignKey(
                    name: "FK_BugCoreProjectUsers_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_BugCoreProjectUsers_BugCoreProjects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "BugCoreProjects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // 15. BugCoreProjectVersions
        migrationBuilder.CreateTable(
            name: "BugCoreProjectVersions",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                Version = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                Released = table.Column<bool>(type: "INTEGER", nullable: false),
                Obsolete = table.Column<bool>(type: "INTEGER", nullable: false),
                DateOrder = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BugCoreProjectVersions", x => x.Id);
                table.ForeignKey(
                    name: "FK_BugCoreProjectVersions_BugCoreProjects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "BugCoreProjects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // 16. BugCoreIssues
        migrationBuilder.CreateTable(
            name: "BugCoreIssues",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                ReporterId = table.Column<int>(type: "INTEGER", nullable: false),
                HandlerId = table.Column<int>(type: "INTEGER", nullable: true),
                DuplicateId = table.Column<int>(type: "INTEGER", nullable: true),
                Priority = table.Column<int>(type: "INTEGER", nullable: false),
                Severity = table.Column<int>(type: "INTEGER", nullable: false),
                Reproducibility = table.Column<int>(type: "INTEGER", nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                Resolution = table.Column<int>(type: "INTEGER", nullable: false),
                ViewState = table.Column<int>(type: "INTEGER", nullable: false),
                CategoryId = table.Column<int>(type: "INTEGER", nullable: true),
                Summary = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                Description = table.Column<string>(type: "TEXT", nullable: false),
                StepsToReproduce = table.Column<string>(type: "TEXT", nullable: false),
                AdditionalInformation = table.Column<string>(type: "TEXT", nullable: false),
                TargetVersion = table.Column<string>(type: "TEXT", nullable: false),
                FixedInVersion = table.Column<string>(type: "TEXT", nullable: false),
                Build = table.Column<string>(type: "TEXT", nullable: false),
                DateSubmitted = table.Column<DateTime>(type: "TEXT", nullable: false),
                LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BugCoreIssues", x => x.Id);
                table.ForeignKey(
                    name: "FK_BugCoreIssues_AspNetUsers_HandlerId",
                    column: x => x.HandlerId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_BugCoreIssues_AspNetUsers_ReporterId",
                    column: x => x.ReporterId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_BugCoreIssues_BugCoreCategories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "BugCoreCategories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_BugCoreIssues_BugCoreProjects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "BugCoreProjects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        // 17. BugCoreCustomFieldValues
        migrationBuilder.CreateTable(
            name: "BugCoreCustomFieldValues",
            columns: table => new
            {
                CustomFieldId = table.Column<int>(type: "INTEGER", nullable: false),
                IssueId = table.Column<int>(type: "INTEGER", nullable: false),
                Value = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BugCoreCustomFieldValues", x => new { x.CustomFieldId, x.IssueId });
                table.ForeignKey(
                    name: "FK_BugCoreCustomFieldValues_BugCoreCustomFields_CustomFieldId",
                    column: x => x.CustomFieldId,
                    principalTable: "BugCoreCustomFields",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_BugCoreCustomFieldValues_BugCoreIssues_IssueId",
                    column: x => x.IssueId,
                    principalTable: "BugCoreIssues",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // 18. BugCoreIssueAttachments
        migrationBuilder.CreateTable(
            name: "BugCoreIssueAttachments",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                IssueId = table.Column<int>(type: "INTEGER", nullable: false),
                UserId = table.Column<int>(type: "INTEGER", nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                DiskFileName = table.Column<string>(type: "TEXT", nullable: false),
                FileName = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                FileFolder = table.Column<string>(type: "TEXT", nullable: false),
                FileSize = table.Column<int>(type: "INTEGER", nullable: false),
                FileType = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                Content = table.Column<byte[]>(type: "BLOB", nullable: true),
                DateAdded = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BugCoreIssueAttachments", x => x.Id);
                table.ForeignKey(
                    name: "FK_BugCoreIssueAttachments_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_BugCoreIssueAttachments_BugCoreIssues_IssueId",
                    column: x => x.IssueId,
                    principalTable: "BugCoreIssues",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // 19. BugCoreIssueHistories
        migrationBuilder.CreateTable(
            name: "BugCoreIssueHistories",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                IssueId = table.Column<int>(type: "INTEGER", nullable: false),
                UserId = table.Column<int>(type: "INTEGER", nullable: false),
                FieldName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                OldValue = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                NewValue = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                Type = table.Column<int>(type: "INTEGER", nullable: false),
                DateModified = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BugCoreIssueHistories", x => x.Id);
                table.ForeignKey(
                    name: "FK_BugCoreIssueHistories_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_BugCoreIssueHistories_BugCoreIssues_IssueId",
                    column: x => x.IssueId,
                    principalTable: "BugCoreIssues",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // 20. BugCoreIssueMonitors
        migrationBuilder.CreateTable(
            name: "BugCoreIssueMonitors",
            columns: table => new
            {
                IssueId = table.Column<int>(type: "INTEGER", nullable: false),
                UserId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BugCoreIssueMonitors", x => new { x.IssueId, x.UserId });
                table.ForeignKey(
                    name: "FK_BugCoreIssueMonitors_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_BugCoreIssueMonitors_BugCoreIssues_IssueId",
                    column: x => x.IssueId,
                    principalTable: "BugCoreIssues",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // 21. BugCoreIssueNotes
        migrationBuilder.CreateTable(
            name: "BugCoreIssueNotes",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                IssueId = table.Column<int>(type: "INTEGER", nullable: false),
                ReporterId = table.Column<int>(type: "INTEGER", nullable: false),
                Note = table.Column<string>(type: "TEXT", nullable: false),
                ViewState = table.Column<int>(type: "INTEGER", nullable: false),
                TimeTrackingMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                DateSubmitted = table.Column<DateTime>(type: "TEXT", nullable: false),
                LastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BugCoreIssueNotes", x => x.Id);
                table.ForeignKey(
                    name: "FK_BugCoreIssueNotes_AspNetUsers_ReporterId",
                    column: x => x.ReporterId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_BugCoreIssueNotes_BugCoreIssues_IssueId",
                    column: x => x.IssueId,
                    principalTable: "BugCoreIssues",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // 22. BugCoreIssueRelationships
        migrationBuilder.CreateTable(
            name: "BugCoreIssueRelationships",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                SourceIssueId = table.Column<int>(type: "INTEGER", nullable: false),
                DestinationIssueId = table.Column<int>(type: "INTEGER", nullable: false),
                Type = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BugCoreIssueRelationships", x => x.Id);
                table.ForeignKey(
                    name: "FK_BugCoreIssueRelationships_BugCoreIssues_DestinationIssueId",
                    column: x => x.DestinationIssueId,
                    principalTable: "BugCoreIssues",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_BugCoreIssueRelationships_BugCoreIssues_SourceIssueId",
                    column: x => x.SourceIssueId,
                    principalTable: "BugCoreIssues",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        // 23. BugCoreIssueTags
        migrationBuilder.CreateTable(
            name: "BugCoreIssueTags",
            columns: table => new
            {
                IssueId = table.Column<int>(type: "INTEGER", nullable: false),
                TagId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BugCoreIssueTags", x => new { x.IssueId, x.TagId });
                table.ForeignKey(
                    name: "FK_BugCoreIssueTags_BugCoreIssues_IssueId",
                    column: x => x.IssueId,
                    principalTable: "BugCoreIssues",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_BugCoreIssueTags_BugCoreTags_TagId",
                    column: x => x.TagId,
                    principalTable: "BugCoreTags",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // Indexes
        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "NormalizedName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "AspNetUsers",
            column: "NormalizedUserName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreProjects_Name",
            table: "BugCoreProjects",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreCategories_ProjectId_Name",
            table: "BugCoreCategories",
            columns: new[] { "ProjectId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreCategories_DefaultAssigneeId",
            table: "BugCoreCategories",
            column: "DefaultAssigneeId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreCustomFields_Name",
            table: "BugCoreCustomFields",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreCustomFieldProjects_ProjectId",
            table: "BugCoreCustomFieldProjects",
            column: "ProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreProjectHierarchies_ChildProjectId",
            table: "BugCoreProjectHierarchies",
            column: "ChildProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreProjectUsers_UserId",
            table: "BugCoreProjectUsers",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreProjectVersions_ProjectId_Version",
            table: "BugCoreProjectVersions",
            columns: new[] { "ProjectId", "Version" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreIssues_ProjectId",
            table: "BugCoreIssues",
            column: "ProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreIssues_ReporterId",
            table: "BugCoreIssues",
            column: "ReporterId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreIssues_HandlerId",
            table: "BugCoreIssues",
            column: "HandlerId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreIssues_CategoryId",
            table: "BugCoreIssues",
            column: "CategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreIssueAttachments_IssueId",
            table: "BugCoreIssueAttachments",
            column: "IssueId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreIssueAttachments_UserId",
            table: "BugCoreIssueAttachments",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreIssueHistories_IssueId",
            table: "BugCoreIssueHistories",
            column: "IssueId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreIssueHistories_UserId",
            table: "BugCoreIssueHistories",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreIssueMonitors_UserId",
            table: "BugCoreIssueMonitors",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreIssueNotes_IssueId",
            table: "BugCoreIssueNotes",
            column: "IssueId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreIssueNotes_ReporterId",
            table: "BugCoreIssueNotes",
            column: "ReporterId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreIssueRelationships_DestinationIssueId",
            table: "BugCoreIssueRelationships",
            column: "DestinationIssueId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreIssueRelationships_SourceIssueId",
            table: "BugCoreIssueRelationships",
            column: "SourceIssueId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreIssueTags_TagId",
            table: "BugCoreIssueTags",
            column: "TagId");

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreTags_Name",
            table: "BugCoreTags",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_BugCoreTags_UserId",
            table: "BugCoreTags",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AspNetRoleClaims");
        migrationBuilder.DropTable(name: "AspNetUserClaims");
        migrationBuilder.DropTable(name: "AspNetUserLogins");
        migrationBuilder.DropTable(name: "AspNetUserRoles");
        migrationBuilder.DropTable(name: "AspNetUserTokens");
        migrationBuilder.DropTable(name: "BugCoreCustomFieldValues");
        migrationBuilder.DropTable(name: "BugCoreCustomFieldProjects");
        migrationBuilder.DropTable(name: "BugCoreCustomFields");
        migrationBuilder.DropTable(name: "BugCoreIssueAttachments");
        migrationBuilder.DropTable(name: "BugCoreIssueHistories");
        migrationBuilder.DropTable(name: "BugCoreIssueMonitors");
        migrationBuilder.DropTable(name: "BugCoreIssueNotes");
        migrationBuilder.DropTable(name: "BugCoreIssueRelationships");
        migrationBuilder.DropTable(name: "BugCoreIssueTags");
        migrationBuilder.DropTable(name: "BugCoreTags");
        migrationBuilder.DropTable(name: "BugCoreIssues");
        migrationBuilder.DropTable(name: "BugCoreCategories");
        migrationBuilder.DropTable(name: "BugCoreProjectHierarchies");
        migrationBuilder.DropTable(name: "BugCoreProjectUsers");
        migrationBuilder.DropTable(name: "BugCoreProjectVersions");
        migrationBuilder.DropTable(name: "BugCoreProjects");
        migrationBuilder.DropTable(name: "AspNetRoles");
        migrationBuilder.DropTable(name: "AspNetUsers");
    }
}
