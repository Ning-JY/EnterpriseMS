namespace EnterpriseMS.Domain.Enums;

public enum UserStatus      { Disabled = 0, Normal = 1 }
public enum MenuType        { Directory = 0, Menu = 1, Button = 2 }
public enum EmployeeStatus  { Probation = 0, OnJob = 1, Left = 2 }
public enum ContractStatus  { Active = 0, Terminated = 1, Expired = 2 }
public enum CertStatus      { Valid = 0, Expired = 1 }
public enum ProjectStatus
{
    EarlyBiz = 0,       // 前期商务
    PlanToStart = 1,    // 预计启动
    BidMaking = 2,      // 标书制作中
    Bidding = 3,        // 投标/磋商中
    Signing = 4,        // 已中标·签订合同中
    Signed = 5,         // 已签回合同
    Executing = 6,      // 执行中
    Submitted = 7,      // 成果提交
    Completed = 8,      // 已完成
    Terminated = 9      // 已终止
}
public enum MilestoneStatus { NotStarted = 0, InProgress = 1, Done = 2 }
public enum BudgetTaskType
{
    EstimateCompile = 0,    // 概算编制
    BudgetCompile = 1,      // 预算编制
    SettlementCompile = 2,  // 结算编制
    EstimateReview = 3,     // 概算评审
    BudgetReview = 4,       // 预算评审
    SettlementReview = 5    // 结算评审
}
public enum BudgetTaskStatus { Draft = 0, InProgress = 1, InnerReview = 2, Reviewing = 3, Done = 4 }
public enum OpinionType     { Reduce = 0, Adjust = 1, Confirm = 2, Explain = 3 }
public enum ArticleStatus   { Draft = 0, Published = 1, Withdrawn = 2 }
