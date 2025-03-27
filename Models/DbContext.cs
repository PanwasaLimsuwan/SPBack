using Microsoft.EntityFrameworkCore;

namespace Api.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        // DbSet สำหรับการจัดการข้อมูล ManpowerPlan
        public DbSet<ManpowerPlan> ManpowerPlan { get; set; }

        // DbSet สำหรับการจัดการข้อมูล ManpowerReq
        public DbSet<ManpowerReq> ManpowerReq { get; set; }

        // DbSet สำหรับการจัดการข้อมูล Employee
        public DbSet<EmployeeInfo> EmployeeInfo { get; set; }

        // DbSet สำหรับการจัดการข้อมูล EICC_Control
        public DbSet<EICC_Control> EICC_Control { get; set; }

        // DbSet สำหรับการจัดการข้อมูล Worktime
        public DbSet<Worktime> Worktime { get; set; }

        // DbSet สำหรับการจัดการข้อมูล HeadcountTransition
        public DbSet<HeadcountTransition> HeadcountTransition { get; set; }

        // DbSet สำหรับการจัดการข้อมูล WeeklySchedule
        public DbSet<WeeklySchedule> WeeklySchedule { get; set; }

        // DbSet สำหรับการจัดการข้อมูล GateEntry
        public DbSet<GateEntry> GateEntry { get; set; }

        // DbSet สำหรับการจัดการข้อมูล Attendance
        public DbSet<Attendance> Attendance { get; set; }

        // DbSet สำหรับการจัดการข้อมูล CleanroomEntry
        public DbSet<CleanroomEntry> CleanroomEntry { get; set; }

        // DbSet สำหรับการจัดการข้อมูล OTJandInspectionSkill
        public DbSet<OJTandInspectionSkill> OJTandInspectionSkill { get; set; }

        // DbSet สำหรับการจัดการข้อมูล Skill
        public DbSet<Skill> Skill { get; set; }
    }
}
