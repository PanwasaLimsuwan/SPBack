using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEmpIDToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attendance",
                table: "ManpowerPlan");

            migrationBuilder.DropColumn(
                name: "HeadCountDate",
                table: "CleanroomEntry");

            migrationBuilder.DropColumn(
                name: "LocationStatus",
                table: "CleanroomEntry");

            migrationBuilder.DropColumn(
                name: "ScheduledEndTime",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "ScheduledStartTime",
                table: "Attendance");

            migrationBuilder.AlterColumn<int>(
                name: "EmpID",
                table: "Worktime",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeInfoEmpID",
                table: "Worktime",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EmpID",
                table: "OJTandInspectionSkill",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeInfoEmpID",
                table: "OJTandInspectionSkill",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ManpowerReqMPRID",
                table: "OJTandInspectionSkill",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManpowerPlanPlanID",
                table: "ManpowerReq",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EmpID",
                table: "HeadcountTransition",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeInfoEmpID",
                table: "HeadcountTransition",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EmpID",
                table: "GateEntry",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeInfoEmpID",
                table: "GateEntry",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ManpowerPlanPlanID",
                table: "EmployeeInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlanID",
                table: "EmployeeInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EmpID",
                table: "EICC_Control",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeInfoEmpID",
                table: "EICC_Control",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WeeklyScheduleWeekID",
                table: "EICC_Control",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EmpID",
                table: "CleanroomEntry",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeInfoEmpID",
                table: "CleanroomEntry",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EmpID",
                table: "Attendance",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeInfoEmpID",
                table: "Attendance",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Worktime_EmployeeInfoEmpID",
                table: "Worktime",
                column: "EmployeeInfoEmpID");

            migrationBuilder.CreateIndex(
                name: "IX_OJTandInspectionSkill_EmployeeInfoEmpID",
                table: "OJTandInspectionSkill",
                column: "EmployeeInfoEmpID");

            migrationBuilder.CreateIndex(
                name: "IX_OJTandInspectionSkill_ManpowerReqMPRID",
                table: "OJTandInspectionSkill",
                column: "ManpowerReqMPRID");

            migrationBuilder.CreateIndex(
                name: "IX_ManpowerReq_ManpowerPlanPlanID",
                table: "ManpowerReq",
                column: "ManpowerPlanPlanID");

            migrationBuilder.CreateIndex(
                name: "IX_HeadcountTransition_EmployeeInfoEmpID",
                table: "HeadcountTransition",
                column: "EmployeeInfoEmpID");

            migrationBuilder.CreateIndex(
                name: "IX_GateEntry_EmployeeInfoEmpID",
                table: "GateEntry",
                column: "EmployeeInfoEmpID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeInfo_ManpowerPlanPlanID",
                table: "EmployeeInfo",
                column: "ManpowerPlanPlanID");

            migrationBuilder.CreateIndex(
                name: "IX_EICC_Control_EmployeeInfoEmpID",
                table: "EICC_Control",
                column: "EmployeeInfoEmpID");

            migrationBuilder.CreateIndex(
                name: "IX_EICC_Control_WeeklyScheduleWeekID",
                table: "EICC_Control",
                column: "WeeklyScheduleWeekID");

            migrationBuilder.CreateIndex(
                name: "IX_CleanroomEntry_EmployeeInfoEmpID",
                table: "CleanroomEntry",
                column: "EmployeeInfoEmpID");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_EmployeeInfoEmpID",
                table: "Attendance",
                column: "EmployeeInfoEmpID");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_EmployeeInfo_EmployeeInfoEmpID",
                table: "Attendance",
                column: "EmployeeInfoEmpID",
                principalTable: "EmployeeInfo",
                principalColumn: "EmpID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CleanroomEntry_EmployeeInfo_EmployeeInfoEmpID",
                table: "CleanroomEntry",
                column: "EmployeeInfoEmpID",
                principalTable: "EmployeeInfo",
                principalColumn: "EmpID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EICC_Control_EmployeeInfo_EmployeeInfoEmpID",
                table: "EICC_Control",
                column: "EmployeeInfoEmpID",
                principalTable: "EmployeeInfo",
                principalColumn: "EmpID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EICC_Control_WeeklySchedule_WeeklyScheduleWeekID",
                table: "EICC_Control",
                column: "WeeklyScheduleWeekID",
                principalTable: "WeeklySchedule",
                principalColumn: "WeekID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeInfo_ManpowerPlan_ManpowerPlanPlanID",
                table: "EmployeeInfo",
                column: "ManpowerPlanPlanID",
                principalTable: "ManpowerPlan",
                principalColumn: "PlanID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GateEntry_EmployeeInfo_EmployeeInfoEmpID",
                table: "GateEntry",
                column: "EmployeeInfoEmpID",
                principalTable: "EmployeeInfo",
                principalColumn: "EmpID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HeadcountTransition_EmployeeInfo_EmployeeInfoEmpID",
                table: "HeadcountTransition",
                column: "EmployeeInfoEmpID",
                principalTable: "EmployeeInfo",
                principalColumn: "EmpID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ManpowerReq_ManpowerPlan_ManpowerPlanPlanID",
                table: "ManpowerReq",
                column: "ManpowerPlanPlanID",
                principalTable: "ManpowerPlan",
                principalColumn: "PlanID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OJTandInspectionSkill_EmployeeInfo_EmployeeInfoEmpID",
                table: "OJTandInspectionSkill",
                column: "EmployeeInfoEmpID",
                principalTable: "EmployeeInfo",
                principalColumn: "EmpID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OJTandInspectionSkill_ManpowerReq_ManpowerReqMPRID",
                table: "OJTandInspectionSkill",
                column: "ManpowerReqMPRID",
                principalTable: "ManpowerReq",
                principalColumn: "MPRID");

            migrationBuilder.AddForeignKey(
                name: "FK_Worktime_EmployeeInfo_EmployeeInfoEmpID",
                table: "Worktime",
                column: "EmployeeInfoEmpID",
                principalTable: "EmployeeInfo",
                principalColumn: "EmpID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_EmployeeInfo_EmployeeInfoEmpID",
                table: "Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_CleanroomEntry_EmployeeInfo_EmployeeInfoEmpID",
                table: "CleanroomEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_EICC_Control_EmployeeInfo_EmployeeInfoEmpID",
                table: "EICC_Control");

            migrationBuilder.DropForeignKey(
                name: "FK_EICC_Control_WeeklySchedule_WeeklyScheduleWeekID",
                table: "EICC_Control");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeInfo_ManpowerPlan_ManpowerPlanPlanID",
                table: "EmployeeInfo");

            migrationBuilder.DropForeignKey(
                name: "FK_GateEntry_EmployeeInfo_EmployeeInfoEmpID",
                table: "GateEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_HeadcountTransition_EmployeeInfo_EmployeeInfoEmpID",
                table: "HeadcountTransition");

            migrationBuilder.DropForeignKey(
                name: "FK_ManpowerReq_ManpowerPlan_ManpowerPlanPlanID",
                table: "ManpowerReq");

            migrationBuilder.DropForeignKey(
                name: "FK_OJTandInspectionSkill_EmployeeInfo_EmployeeInfoEmpID",
                table: "OJTandInspectionSkill");

            migrationBuilder.DropForeignKey(
                name: "FK_OJTandInspectionSkill_ManpowerReq_ManpowerReqMPRID",
                table: "OJTandInspectionSkill");

            migrationBuilder.DropForeignKey(
                name: "FK_Worktime_EmployeeInfo_EmployeeInfoEmpID",
                table: "Worktime");

            migrationBuilder.DropIndex(
                name: "IX_Worktime_EmployeeInfoEmpID",
                table: "Worktime");

            migrationBuilder.DropIndex(
                name: "IX_OJTandInspectionSkill_EmployeeInfoEmpID",
                table: "OJTandInspectionSkill");

            migrationBuilder.DropIndex(
                name: "IX_OJTandInspectionSkill_ManpowerReqMPRID",
                table: "OJTandInspectionSkill");

            migrationBuilder.DropIndex(
                name: "IX_ManpowerReq_ManpowerPlanPlanID",
                table: "ManpowerReq");

            migrationBuilder.DropIndex(
                name: "IX_HeadcountTransition_EmployeeInfoEmpID",
                table: "HeadcountTransition");

            migrationBuilder.DropIndex(
                name: "IX_GateEntry_EmployeeInfoEmpID",
                table: "GateEntry");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeInfo_ManpowerPlanPlanID",
                table: "EmployeeInfo");

            migrationBuilder.DropIndex(
                name: "IX_EICC_Control_EmployeeInfoEmpID",
                table: "EICC_Control");

            migrationBuilder.DropIndex(
                name: "IX_EICC_Control_WeeklyScheduleWeekID",
                table: "EICC_Control");

            migrationBuilder.DropIndex(
                name: "IX_CleanroomEntry_EmployeeInfoEmpID",
                table: "CleanroomEntry");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_EmployeeInfoEmpID",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "EmployeeInfoEmpID",
                table: "Worktime");

            migrationBuilder.DropColumn(
                name: "EmployeeInfoEmpID",
                table: "OJTandInspectionSkill");

            migrationBuilder.DropColumn(
                name: "ManpowerReqMPRID",
                table: "OJTandInspectionSkill");

            migrationBuilder.DropColumn(
                name: "ManpowerPlanPlanID",
                table: "ManpowerReq");

            migrationBuilder.DropColumn(
                name: "EmployeeInfoEmpID",
                table: "HeadcountTransition");

            migrationBuilder.DropColumn(
                name: "EmployeeInfoEmpID",
                table: "GateEntry");

            migrationBuilder.DropColumn(
                name: "ManpowerPlanPlanID",
                table: "EmployeeInfo");

            migrationBuilder.DropColumn(
                name: "PlanID",
                table: "EmployeeInfo");

            migrationBuilder.DropColumn(
                name: "EmployeeInfoEmpID",
                table: "EICC_Control");

            migrationBuilder.DropColumn(
                name: "WeeklyScheduleWeekID",
                table: "EICC_Control");

            migrationBuilder.DropColumn(
                name: "EmployeeInfoEmpID",
                table: "CleanroomEntry");

            migrationBuilder.DropColumn(
                name: "EmployeeInfoEmpID",
                table: "Attendance");

            migrationBuilder.AlterColumn<string>(
                name: "EmpID",
                table: "Worktime",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "EmpID",
                table: "OJTandInspectionSkill",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Attendance",
                table: "ManpowerPlan",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "EmpID",
                table: "HeadcountTransition",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "EmpID",
                table: "GateEntry",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "EmpID",
                table: "EICC_Control",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "EmpID",
                table: "CleanroomEntry",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "HeadCountDate",
                table: "CleanroomEntry",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationStatus",
                table: "CleanroomEntry",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmpID",
                table: "Attendance",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ScheduledEndTime",
                table: "Attendance",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ScheduledStartTime",
                table: "Attendance",
                type: "time",
                nullable: true);
        }
    }
}
