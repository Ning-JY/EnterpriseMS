using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Domain.Entities.Hr;

namespace EnterpriseMS.Infrastructure.Data.Seeds;

public static class HrSeeds
{
    public static void Seed(ModelBuilder mb)
    {
        var dt = new DateTime(2026, 1, 1);

        // ── 员工档案 ──────────────────────────────────────────
        mb.Entity<Employee>().HasData(
            new Employee { Id = 101, EmpNo = "EMP20230001", RealName = "甯金元", Gender = 1, Phone = "13800000001", Email = "zhangsan@company.com", DeptId = 2, PostId = 3, Status = 1, EntryDate = new DateTime(2020, 3, 1), FormalDate = new DateTime(2020, 6, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 102, EmpNo = "EMP20230002", RealName = "曹丽君", Gender = 2, Phone = "13800000002", Email = "lisi@company.com", DeptId = 2, PostId = 4, Status = 1, EntryDate = new DateTime(2019, 7, 1), FormalDate = new DateTime(2019, 10, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 103, EmpNo = "EMP20230003", RealName = "刘润泽", Gender = 1, Phone = "13800000003", Email = "wangwu@company.com", DeptId = 3, PostId = 3, Status = 1, EntryDate = new DateTime(2021, 1, 1), FormalDate = new DateTime(2021, 4, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 104, EmpNo = "EMP20230004", RealName = "王帅伟", Gender = 1, Phone = "13800000004", Email = "zhaoliu@company.com", DeptId = 4, PostId = 6, Status = 1, EntryDate = new DateTime(2018, 5, 1), FormalDate = new DateTime(2018, 8, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 105, EmpNo = "EMP20230005", RealName = "杨通", Gender = 1, Phone = "13800000005", Email = "sunqi@company.com", DeptId = 2, PostId = 7, Status = 1, EntryDate = new DateTime(2022, 3, 1), FormalDate = new DateTime(2022, 6, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 106, EmpNo = "EMP20230006", RealName = "郭家松", Gender = 1, Phone = "13800000006", Email = "zhouba@company.com", DeptId = 3, PostId = 7, Status = 1, EntryDate = new DateTime(2021, 9, 1), FormalDate = new DateTime(2021, 12, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 107, EmpNo = "EMP20230007", RealName = "陈俊童", Gender = 1, Phone = "13800000007", Email = "wujiu@company.com", DeptId = 5, PostId = 4, Status = 1, EntryDate = new DateTime(2020, 6, 1), FormalDate = new DateTime(2020, 9, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 108, EmpNo = "EMP20230008", RealName = "舒影", Gender = 2, Phone = "13800000008", Email = "zhengshi@company.com", DeptId = 4, PostId = 3, Status = 1, EntryDate = new DateTime(2017, 4, 1), FormalDate = new DateTime(2017, 7, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 109, EmpNo = "EMP20230009", RealName = "肖玲", Gender = 2, Phone = "13800000009", Email = "chenxm@company.com", DeptId = 2, PostId = 8, Status = 0, EntryDate = new DateTime(2026, 1, 1), ProbationEndDate = new DateTime(2026, 4, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 110, EmpNo = "EMP20230010", RealName = "魏利", Gender = 2, Phone = "13800000010", Email = "linxy@company.com", DeptId = 6, PostId = 9, Status = 1, EntryDate = new DateTime(2023, 5, 1), FormalDate = new DateTime(2023, 8, 1), CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 员工合同 ──────────────────────────────────────────
        mb.Entity<EmployeeContract>().HasData(
            new EmployeeContract { Id = 1001, EmployeeId = 101, ContractNo = "HT2020-001", ContractType = "固定期限", StartDate = new DateTime(2020, 6, 1), EndDate = new DateTime(2023, 5, 31), SignDate = new DateTime(2020, 6, 1), Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1002, EmployeeId = 101, ContractNo = "HT2023-001", ContractType = "固定期限", StartDate = new DateTime(2023, 6, 1), EndDate = new DateTime(2026, 5, 31), SignDate = new DateTime(2023, 6, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1003, EmployeeId = 102, ContractNo = "HT2019-001", ContractType = "固定期限", StartDate = new DateTime(2019, 10, 1), EndDate = new DateTime(2022, 9, 30), SignDate = new DateTime(2019, 10, 1), Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1004, EmployeeId = 102, ContractNo = "HT2022-001", ContractType = "无固定期限", StartDate = new DateTime(2022, 10, 1), EndDate = new DateTime(2099, 12, 31), SignDate = new DateTime(2022, 10, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1005, EmployeeId = 103, ContractNo = "HT2021-001", ContractType = "固定期限", StartDate = new DateTime(2021, 4, 1), EndDate = new DateTime(2024, 3, 31), SignDate = new DateTime(2021, 4, 1), Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1006, EmployeeId = 103, ContractNo = "HT2024-001", ContractType = "固定期限", StartDate = new DateTime(2024, 4, 1), EndDate = new DateTime(2027, 3, 31), SignDate = new DateTime(2024, 4, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1007, EmployeeId = 104, ContractNo = "HT2018-001", ContractType = "无固定期限", StartDate = new DateTime(2018, 8, 1), EndDate = new DateTime(2099, 12, 31), SignDate = new DateTime(2018, 8, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1008, EmployeeId = 105, ContractNo = "HT2022-002", ContractType = "固定期限", StartDate = new DateTime(2022, 6, 1), EndDate = new DateTime(2025, 5, 31), SignDate = new DateTime(2022, 6, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1009, EmployeeId = 106, ContractNo = "HT2021-002", ContractType = "固定期限", StartDate = new DateTime(2021, 12, 1), EndDate = new DateTime(2024, 11, 30), SignDate = new DateTime(2021, 12, 1), Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1010, EmployeeId = 106, ContractNo = "HT2024-002", ContractType = "固定期限", StartDate = new DateTime(2024, 12, 1), EndDate = new DateTime(2027, 11, 30), SignDate = new DateTime(2024, 12, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 员工证书 ──────────────────────────────────────────
        mb.Entity<EmployeeCertificate>().HasData(
            new EmployeeCertificate { Id = 2001, EmployeeId = 101, CertName = "注册城乡规划师", CertType = "注册规划师", CertNo = "2019ABCD0001", IssueOrg = "住建部", IssueDate = new DateTime(2019, 11, 1), ExpireDate = new DateTime(2025, 10, 31), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeCertificate { Id = 2002, EmployeeId = 102, CertName = "一级造价工程师", CertType = "造价工程师", CertNo = "2018ABCD0002", IssueOrg = "住建部", IssueDate = new DateTime(2018, 6, 1), ExpireDate = new DateTime(2026, 5, 31), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeCertificate { Id = 2003, EmployeeId = 103, CertName = "注册城乡规划师", CertType = "注册规划师", CertNo = "2021ABCD0003", IssueOrg = "住建部", IssueDate = new DateTime(2021, 11, 1), ExpireDate = new DateTime(2025, 10, 31), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeCertificate { Id = 2004, EmployeeId = 104, CertName = "二级建造师", CertType = "建造师", CertNo = "2016ABCD0004", IssueOrg = "四川省住建厅", IssueDate = new DateTime(2016, 9, 1), ExpireDate = null, Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeCertificate { Id = 2005, EmployeeId = 104, CertName = "注册建筑师", CertType = "注册建筑师", CertNo = "2020ABCD0005", IssueOrg = "住建部", IssueDate = new DateTime(2020, 3, 1), ExpireDate = new DateTime(2026, 2, 28), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeCertificate { Id = 2006, EmployeeId = 105, CertName = "助理工程师职称", CertType = "职称证书", CertNo = "2022ABCD0006", IssueOrg = "四川省人社厅", IssueDate = new DateTime(2022, 8, 1), ExpireDate = null, Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeCertificate { Id = 2007, EmployeeId = 107, CertName = "一级造价工程师", CertType = "造价工程师", CertNo = "2019ABCD0007", IssueOrg = "住建部", IssueDate = new DateTime(2019, 6, 1), ExpireDate = new DateTime(2026, 2, 28), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeCertificate { Id = 2008, EmployeeId = 108, CertName = "注册城乡规划师", CertType = "注册规划师", CertNo = "2016ABCD0008", IssueOrg = "住建部", IssueDate = new DateTime(2016, 11, 1), ExpireDate = new DateTime(2026, 5, 31), Status = 0, CreatedAt = dt, CreatedBy = "system" }
        );
    }
}
