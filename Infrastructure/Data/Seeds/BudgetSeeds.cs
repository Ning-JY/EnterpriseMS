using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Domain.Entities.Budget;

namespace EnterpriseMS.Infrastructure.Data.Seeds;

public static class BudgetSeeds
{
    public static void Seed(ModelBuilder mb)
    {
        var dt = new DateTime(2026, 1, 1);

        // ── 概预算示例任务 ────────────────────────────────────
        mb.Entity<BudgetTask>().HasData(
            new BudgetTask
            {
                Id = 6001,
                TaskNo = "YS-2024-001",
                TaskName = "某住宅小区（一期）工程结算审核",
                TaskType = 5,
                DeptId = 2,
                OwnerName = "成都某房产开发有限公司",
                OwnerContact = "赵总",
                OwnerPhone = "13900000002",
                ContractorName = "中建三局第二建设工程有限责任公司",
                BuildingScale = "住宅12栋，建筑面积约8.6万㎡",
                SubmitAmount = 6850m,
                ApprovedAmount = 6312m,
                ReductionRate = 7.85m,
                QuotaBasis = "四川省2015建设工程工程量清单计价定额",
                FeeStandard = "四川省2015费用定额",
                TechLeaderId = 102,
                BizLeaderId = 101,
                PlanDate = new DateTime(2024, 8, 31),
                Status = 4,
                Remark = "核减率7.85%，共出具评审意见42条",
                CreatedAt = dt,
                CreatedBy = "system"
            },
            new BudgetTask
            {
                Id = 6002,
                TaskNo = "YS-2024-002",
                TaskName = "某市政道路工程预算编制",
                TaskType = 1,
                DeptId = 3,
                OwnerName = "成都市交通运输局",
                OwnerContact = "李科长",
                OwnerPhone = "028-87654321",
                ContractorName = null,
                BuildingScale = "双向四车道，全长3.2km",
                SubmitAmount = 4200m,
                ApprovedAmount = null,
                ReductionRate = null,
                QuotaBasis = "四川省2020市政工程定额",
                FeeStandard = "四川省2020费用定额",
                TechLeaderId = 107,
                BizLeaderId = 103,
                PlanDate = new DateTime(2024, 10, 31),
                Status = 1,
                CreatedAt = dt,
                CreatedBy = "system"
            }
        );

        // ── 费用分部示例 ──────────────────────────────────────
        mb.Entity<BudgetSection>().HasData(
            new BudgetSection { Id = 7001, TaskId = 6001, SectionNo = 1, SectionName = "建筑工程", Category = "土建工程", ContractAmount = 3200m, SubmitAmount = 3100m, ApprovedAmount = 2890m, Status = 2, CreatedAt = dt, CreatedBy = "system" },
            new BudgetSection { Id = 7002, TaskId = 6001, SectionNo = 2, SectionName = "安装工程", Category = "安装工程", ContractAmount = 1800m, SubmitAmount = 1750m, ApprovedAmount = 1650m, Status = 2, CreatedAt = dt, CreatedBy = "system" },
            new BudgetSection { Id = 7003, TaskId = 6001, SectionNo = 3, SectionName = "室外市政配套", Category = "市政管道", ContractAmount = 1100m, SubmitAmount = 1000m, ApprovedAmount = 972m, Status = 2, CreatedAt = dt, CreatedBy = "system" },
            new BudgetSection { Id = 7004, TaskId = 6001, SectionNo = 4, SectionName = "绿化景观工程", Category = "绿化景观", ContractAmount = 800m, SubmitAmount = 0m, ApprovedAmount = 800m, Status = 2, CreatedAt = dt, CreatedBy = "system" }
        );
    }
}
