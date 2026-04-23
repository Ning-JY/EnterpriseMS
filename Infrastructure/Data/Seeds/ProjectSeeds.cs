using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Domain.Entities.Project;

namespace EnterpriseMS.Infrastructure.Data.Seeds;

public static class ProjectSeeds
{
    public static void Seed(ModelBuilder mb)
    {
        var dt = new DateTime(2026, 1, 1);

        // ── 示例项目 ──────────────────────────────────────────
        mb.Entity<Project>().HasData(
            new Project
            {
                Id = 1001,
                ProjNo = "PRJ-2024-001",
                ProjName = "成都市某片区控制性详细规划",
                DeptId = 4,
                BizType = "控制性详细规划",
                OwnerName = "成都市规划和自然资源局",
                OwnerContact = "王处长",
                OwnerPhone = "028-12345678",
                ProcurementType = "竞争性磋商",
                ContractAmount = 98m,
                IsJointVenture = false,
                TechLeaderId = 108,
                BizLeaderId = 103,
                SignDate = new DateTime(2024, 3, 15),
                PlanEndDate = new DateTime(2024, 12, 31),
                ProgressStatus = 6,
                Remark = "重点项目，需配合规委会评审",
                CreatedAt = dt,
                CreatedBy = "system"
            },
            new Project
            {
                Id = 1002,
                ProjNo = "PRJ-2024-002",
                ProjName = "某住宅小区结算审核",
                DeptId = 2,
                BizType = "结算评审",
                OwnerName = "成都市某国有投资公司",
                OwnerContact = "张总",
                OwnerPhone = "13900000001",
                ProcurementType = "询价",
                ContractAmount = 45m,
                IsJointVenture = false,
                TechLeaderId = 102,
                BizLeaderId = 101,
                SignDate = new DateTime(2024, 5, 1),
                PlanEndDate = new DateTime(2024, 8, 31),
                ProgressStatus = 7,
                Remark = "送审金额约3200万元",
                CreatedAt = dt,
                CreatedBy = "system"
            },
            new Project
            {
                Id = 1003,
                ProjNo = "PRJ-2024-003",
                ProjName = "某市政道路工程可行性研究报告",
                DeptId = 3,
                BizType = "可行性研究报告",
                OwnerName = "成都市交通运输局",
                OwnerContact = "李科长",
                OwnerPhone = "028-87654321",
                ProcurementType = "公开招标",
                ContractAmount = 160m,
                IsJointVenture = true,
                OurRatio = 60m,
                TechLeaderId = 103,
                BizLeaderId = 107,
                SignDate = new DateTime(2024, 1, 20),
                PlanEndDate = new DateTime(2024, 10, 31),
                ProgressStatus = 6,
                Remark = "联合体项目，牵头方，我方占比60%",
                CreatedAt = dt,
                CreatedBy = "system"
            },
            new Project
            {
                Id = 1004,
                ProjNo = "PRJ-2023-018",
                ProjName = "某工业园区概念规划设计",
                DeptId = 5,
                BizType = "战略咨询",
                OwnerName = "成都高新区管委会",
                OwnerContact = "陈主任",
                OwnerPhone = "13800000099",
                ProcurementType = "单一来源",
                ContractAmount = 32m,
                IsJointVenture = false,
                TechLeaderId = 107,
                BizLeaderId = 104,
                SignDate = new DateTime(2023, 9, 1),
                PlanEndDate = new DateTime(2024, 3, 31),
                ActualEndDate = new DateTime(2024, 4, 15),
                ProgressStatus = 8,
                Remark = "已完成，已全额回款",
                CreatedAt = dt,
                CreatedBy = "system"
            },
            new Project
            {
                Id = 1005,
                ProjNo = "PRJ-2024-004",
                ProjName = "某新城片区节能评估报告",
                DeptId = 2,
                BizType = "节能评估报告",
                OwnerName = "成都天府新区建设局",
                OwnerContact = "刘工",
                OwnerPhone = "13700000001",
                ProcurementType = "竞争性磋商",
                ContractAmount = 28m,
                IsJointVenture = false,
                TechLeaderId = 105,
                BizLeaderId = 101,
                SignDate = new DateTime(2024, 6, 10),
                PlanEndDate = new DateTime(2024, 9, 30),
                ProgressStatus = 3,
                Remark = "投标截止2024-06-10",
                CreatedAt = dt,
                CreatedBy = "system"
            }
        );

        // ── 项目成员 ──────────────────────────────────────────
        mb.Entity<ProjectMember>().HasData(
            new ProjectMember { Id = 3001, ProjectId = 1001, EmployeeId = 108, Role = "项目负责人", DutyDesc = "总体技术把控，规划方案设计", Ratio = 40m, JoinDate = new DateTime(2024, 3, 15), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMember { Id = 3002, ProjectId = 1001, EmployeeId = 104, Role = "参与人员", DutyDesc = "用地分析与指标测算", Ratio = 30m, JoinDate = new DateTime(2024, 3, 15), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMember { Id = 3003, ProjectId = 1001, EmployeeId = 106, Role = "参与人员", DutyDesc = "现状调研与CAD制图", Ratio = 20m, JoinDate = new DateTime(2024, 3, 15), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMember { Id = 3004, ProjectId = 1001, EmployeeId = 103, Role = "商务负责人", DutyDesc = "合同对接、开票及回款跟进", Ratio = 10m, JoinDate = new DateTime(2024, 3, 15), Status = 0, CreatedAt = dt, CreatedBy = "system" },

            new ProjectMember { Id = 3005, ProjectId = 1002, EmployeeId = 102, Role = "项目负责人", DutyDesc = "结算审核技术负责", Ratio = 50m, JoinDate = new DateTime(2024, 5, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMember { Id = 3006, ProjectId = 1002, EmployeeId = 105, Role = "参与人员", DutyDesc = "工程量核算", Ratio = 30m, JoinDate = new DateTime(2024, 5, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMember { Id = 3007, ProjectId = 1002, EmployeeId = 101, Role = "商务负责人", DutyDesc = "商务对接", Ratio = 20m, JoinDate = new DateTime(2024, 5, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" },

            new ProjectMember { Id = 3008, ProjectId = 1003, EmployeeId = 103, Role = "项目负责人", DutyDesc = "可研报告编制", Ratio = 45m, JoinDate = new DateTime(2024, 1, 20), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMember { Id = 3009, ProjectId = 1003, EmployeeId = 107, Role = "商务负责人", DutyDesc = "联合体协调及商务", Ratio = 35m, JoinDate = new DateTime(2024, 1, 20), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMember { Id = 3010, ProjectId = 1003, EmployeeId = 106, Role = "参与人员", DutyDesc = "交通量调查与分析", Ratio = 20m, JoinDate = new DateTime(2024, 1, 20), Status = 0, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 项目里程碑 ────────────────────────────────────────
        mb.Entity<ProjectMilestone>().HasData(
            new ProjectMilestone { Id = 4001, ProjectId = 1001, MilestoneName = "现状调研与基础资料收集", MilestoneType = "现状调研", PlanDate = new DateTime(2024, 4, 30), ActualDate = new DateTime(2024, 4, 25), OwnerId = 108, Status = 2, IsOverdue = false, Sort = 1, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMilestone { Id = 4002, ProjectId = 1001, MilestoneName = "规划方案初稿", MilestoneType = "方案设计", PlanDate = new DateTime(2024, 7, 31), ActualDate = new DateTime(2024, 8, 5), OwnerId = 108, Status = 2, IsOverdue = true, Sort = 2, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMilestone { Id = 4003, ProjectId = 1001, MilestoneName = "专家评审会", MilestoneType = "专家评审", PlanDate = new DateTime(2024, 10, 31), ActualDate = null, OwnerId = 108, Status = 1, IsOverdue = false, Sort = 3, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMilestone { Id = 4004, ProjectId = 1001, MilestoneName = "成果正式交付", MilestoneType = "成果交付", PlanDate = new DateTime(2024, 12, 31), ActualDate = null, OwnerId = 103, AcceptAmount = 98m, Status = 0, IsOverdue = false, Sort = 4, CreatedAt = dt, CreatedBy = "system" },

            new ProjectMilestone { Id = 4005, ProjectId = 1002, MilestoneName = "资料收集与初步核查", MilestoneType = "资料收集", PlanDate = new DateTime(2024, 5, 20), ActualDate = new DateTime(2024, 5, 18), OwnerId = 102, Status = 2, IsOverdue = false, Sort = 1, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMilestone { Id = 4006, ProjectId = 1002, MilestoneName = "结算审核报告初稿", MilestoneType = "方案设计", PlanDate = new DateTime(2024, 7, 15), ActualDate = new DateTime(2024, 7, 20), OwnerId = 102, Status = 2, IsOverdue = true, Sort = 2, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMilestone { Id = 4007, ProjectId = 1002, MilestoneName = "审核报告正式提交", MilestoneType = "成果交付", PlanDate = new DateTime(2024, 8, 31), ActualDate = null, OwnerId = 101, AcceptAmount = 45m, Status = 1, IsOverdue = false, Sort = 3, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 验收记录 ──────────────────────────────────────────
        mb.Entity<ProjectAcceptance>().HasData(
            new ProjectAcceptance { Id = 5001, ProjectId = 1004, AcceptBatch = "第一批（预付款）", AcceptDate = new DateTime(2023, 10, 1), AcceptAmount = 9.6m, InvoiceNo = "INV2023100001", Remark = "预付款30%", CreatedAt = dt, CreatedBy = "system" },
            new ProjectAcceptance { Id = 5002, ProjectId = 1004, AcceptBatch = "第二批（中期款）", AcceptDate = new DateTime(2024, 1, 15), AcceptAmount = 16m, InvoiceNo = "INV2024010001", Remark = "完成中期成果50%", CreatedAt = dt, CreatedBy = "system" },
            new ProjectAcceptance { Id = 5003, ProjectId = 1004, AcceptBatch = "第三批（尾款）", AcceptDate = new DateTime(2024, 5, 20), AcceptAmount = 6.4m, InvoiceNo = "INV2024050001", Remark = "成果交付完成", CreatedAt = dt, CreatedBy = "system" }
        );
    }
}
