using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ALL2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Worktimes",
                table: "Worktimes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WeeklySchedules",
                table: "WeeklySchedules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OJTandInspectionSkills",
                table: "OJTandInspectionSkills");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ManpowerReqs",
                table: "ManpowerReqs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ManpowerPlans",
                table: "ManpowerPlans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HeadcountTransitions",
                table: "HeadcountTransitions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GateEntries",
                table: "GateEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employees",
                table: "Employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EICC_Controls",
                table: "EICC_Controls");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CleanroomEntries",
                table: "CleanroomEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attendances",
                table: "Attendances");

            migrationBuilder.RenameTable(
                name: "Worktimes",
                newName: "Worktime");

            migrationBuilder.RenameTable(
                name: "WeeklySchedules",
                newName: "WeeklySchedule");

            migrationBuilder.RenameTable(
                name: "OJTandInspectionSkills",
                newName: "OJTandInspectionSkill");

            migrationBuilder.RenameTable(
                name: "ManpowerReqs",
                newName: "ManpowerReq");

            migrationBuilder.RenameTable(
                name: "ManpowerPlans",
                newName: "ManpowerPlan");

            migrationBuilder.RenameTable(
                name: "HeadcountTransitions",
                newName: "HeadcountTransition");

            migrationBuilder.RenameTable(
                name: "GateEntries",
                newName: "GateEntry");

            migrationBuilder.RenameTable(
                name: "Employees",
                newName: "EmployeeInfo");

            migrationBuilder.RenameTable(
                name: "EICC_Controls",
                newName: "EICC_Control");

            migrationBuilder.RenameTable(
                name: "CleanroomEntries",
                newName: "CleanroomEntry");

            migrationBuilder.RenameTable(
                name: "Attendances",
                newName: "Attendance");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Worktime",
                table: "Worktime",
                column: "WorkTimeID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeeklySchedule",
                table: "WeeklySchedule",
                column: "WeekID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OJTandInspectionSkill",
                table: "OJTandInspectionSkill",
                column: "CourseNo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ManpowerReq",
                table: "ManpowerReq",
                column: "MPRID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ManpowerPlan",
                table: "ManpowerPlan",
                column: "PlanID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HeadcountTransition",
                table: "HeadcountTransition",
                column: "DateTime");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GateEntry",
                table: "GateEntry",
                column: "GateEntryID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeInfo",
                table: "EmployeeInfo",
                column: "EmpID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EICC_Control",
                table: "EICC_Control",
                column: "ControlID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CleanroomEntry",
                table: "CleanroomEntry",
                column: "CEntryID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attendance",
                table: "Attendance",
                column: "AttendanceID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Worktime",
                table: "Worktime");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WeeklySchedule",
                table: "WeeklySchedule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OJTandInspectionSkill",
                table: "OJTandInspectionSkill");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ManpowerReq",
                table: "ManpowerReq");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ManpowerPlan",
                table: "ManpowerPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HeadcountTransition",
                table: "HeadcountTransition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GateEntry",
                table: "GateEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeInfo",
                table: "EmployeeInfo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EICC_Control",
                table: "EICC_Control");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CleanroomEntry",
                table: "CleanroomEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attendance",
                table: "Attendance");

            migrationBuilder.RenameTable(
                name: "Worktime",
                newName: "Worktimes");

            migrationBuilder.RenameTable(
                name: "WeeklySchedule",
                newName: "WeeklySchedules");

            migrationBuilder.RenameTable(
                name: "OJTandInspectionSkill",
                newName: "OJTandInspectionSkills");

            migrationBuilder.RenameTable(
                name: "ManpowerReq",
                newName: "ManpowerReqs");

            migrationBuilder.RenameTable(
                name: "ManpowerPlan",
                newName: "ManpowerPlans");

            migrationBuilder.RenameTable(
                name: "HeadcountTransition",
                newName: "HeadcountTransitions");

            migrationBuilder.RenameTable(
                name: "GateEntry",
                newName: "GateEntries");

            migrationBuilder.RenameTable(
                name: "EmployeeInfo",
                newName: "Employees");

            migrationBuilder.RenameTable(
                name: "EICC_Control",
                newName: "EICC_Controls");

            migrationBuilder.RenameTable(
                name: "CleanroomEntry",
                newName: "CleanroomEntries");

            migrationBuilder.RenameTable(
                name: "Attendance",
                newName: "Attendances");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Worktimes",
                table: "Worktimes",
                column: "WorkTimeID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeeklySchedules",
                table: "WeeklySchedules",
                column: "WeekID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OJTandInspectionSkills",
                table: "OJTandInspectionSkills",
                column: "CourseNo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ManpowerReqs",
                table: "ManpowerReqs",
                column: "MPRID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ManpowerPlans",
                table: "ManpowerPlans",
                column: "PlanID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HeadcountTransitions",
                table: "HeadcountTransitions",
                column: "DateTime");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GateEntries",
                table: "GateEntries",
                column: "GateEntryID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employees",
                table: "Employees",
                column: "EmpID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EICC_Controls",
                table: "EICC_Controls",
                column: "ControlID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CleanroomEntries",
                table: "CleanroomEntries",
                column: "CEntryID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attendances",
                table: "Attendances",
                column: "AttendanceID");
        }
    }
}
