using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AllTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    AttendanceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckInTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    CheckOutTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    ScheduledStartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    ScheduledEndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WeekNumber = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendances", x => x.AttendanceID);
                });

            migrationBuilder.CreateTable(
                name: "CleanroomEntries",
                columns: table => new
                {
                    CEntryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheckInDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckOutDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LocationStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeadCountDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CleanroomEntries", x => x.CEntryID);
                });

            migrationBuilder.CreateTable(
                name: "EICC_Controls",
                columns: table => new
                {
                    ControlID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WeekID = table.Column<int>(type: "int", nullable: false),
                    TotalHours = table.Column<float>(type: "real", nullable: true),
                    DaysWorked = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EICC_Controls", x => x.ControlID);
                });

            migrationBuilder.CreateTable(
                name: "GateEntries",
                columns: table => new
                {
                    GateEntryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntryDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExitDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GateNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Room = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GateStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GateEntries", x => x.GateEntryID);
                });

            migrationBuilder.CreateTable(
                name: "HeadcountTransitions",
                columns: table => new
                {
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmpID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeadcountTransitions", x => x.DateTime);
                });

            migrationBuilder.CreateTable(
                name: "ManpowerPlans",
                columns: table => new
                {
                    PlanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmpID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Attendance = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShiftCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Shift = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlannedHeadcount = table.Column<int>(type: "int", nullable: true),
                    ActualHeadcount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManpowerPlans", x => x.PlanID);
                });

            migrationBuilder.CreateTable(
                name: "ManpowerReqs",
                columns: table => new
                {
                    MPRID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Biz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Process = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Require = table.Column<int>(type: "int", nullable: true),
                    SkillGroup = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManpowerReqs", x => x.MPRID);
                });

            migrationBuilder.CreateTable(
                name: "OJTandInspectionSkills",
                columns: table => new
                {
                    CourseNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseGroup = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Biz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Process = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CerNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<int>(type: "int", nullable: true),
                    SkillGroup = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmpID = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OJTandInspectionSkills", x => x.CourseNo);
                });

            migrationBuilder.CreateTable(
                name: "WeeklySchedules",
                columns: table => new
                {
                    WeekID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Week = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AbsentCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklySchedules", x => x.WeekID);
                });

            migrationBuilder.CreateTable(
                name: "Worktimes",
                columns: table => new
                {
                    WorkTimeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkedHours = table.Column<float>(type: "real", nullable: true),
                    OT_Hours = table.Column<float>(type: "real", nullable: true),
                    EICC_Hours = table.Column<float>(type: "real", nullable: true),
                    OverloadHours = table.Column<float>(type: "real", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Worktimes", x => x.WorkTimeID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendances");

            migrationBuilder.DropTable(
                name: "CleanroomEntries");

            migrationBuilder.DropTable(
                name: "EICC_Controls");

            migrationBuilder.DropTable(
                name: "GateEntries");

            migrationBuilder.DropTable(
                name: "HeadcountTransitions");

            migrationBuilder.DropTable(
                name: "ManpowerPlans");

            migrationBuilder.DropTable(
                name: "ManpowerReqs");

            migrationBuilder.DropTable(
                name: "OJTandInspectionSkills");

            migrationBuilder.DropTable(
                name: "WeeklySchedules");

            migrationBuilder.DropTable(
                name: "Worktimes");
        }
    }
}
