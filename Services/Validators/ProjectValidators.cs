using FluentValidation;
using EnterpriseMS.Services.DTOs.Project;

namespace EnterpriseMS.Services.Validators;

public class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto>
{
    public CreateProjectDtoValidator()
    {
        RuleFor(x => x.ProjNo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ProjName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BizType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.OwnerName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ContractAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OurRatio).InclusiveBetween(0, 100)
            .When(x => x.OurRatio.HasValue).WithMessage("我方占比需在 0-100 之间");
        RuleFor(x => x.PlanEndDate)
            .GreaterThanOrEqualTo(x => x.SignDate)
            .When(x => x.SignDate.HasValue && x.PlanEndDate.HasValue)
            .WithMessage("计划结束日期不能早于签约日期");
    }
}

public class UpdateProjectDtoValidator : AbstractValidator<UpdateProjectDto>
{
    public UpdateProjectDtoValidator()
    {
        Include(new CreateProjectDtoValidator());
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("项目ID不合法");
    }
}

public class ChangeStatusDtoValidator : AbstractValidator<ChangeStatusDto>
{
    public ChangeStatusDtoValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("项目ID不合法");
        RuleFor(x => x.NewStatus).InclusiveBetween(0, 5).WithMessage("项目状态不合法");
    }
}

public class CreateMemberDtoValidator : AbstractValidator<CreateMemberDto>
{
    public CreateMemberDtoValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("请选择成员");
        RuleFor(x => x.Role).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Ratio).InclusiveBetween(0, 100).WithMessage("占比需在 0-100 之间");
    }
}

public class UpdateMemberDtoValidator : AbstractValidator<UpdateMemberDto>
{
    public UpdateMemberDtoValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("成员ID不合法");
        RuleFor(x => x.Ratio).InclusiveBetween(0, 100).WithMessage("占比需在 0-100 之间");
    }
}

public class CreateMilestoneDtoValidator : AbstractValidator<CreateMilestoneDto>
{
    public CreateMilestoneDtoValidator()
    {
        RuleFor(x => x.MilestoneName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Sort).GreaterThanOrEqualTo(0);
    }
}

public class UpdateMilestoneDtoValidator : AbstractValidator<UpdateMilestoneDto>
{
    public UpdateMilestoneDtoValidator()
    {
        Include(new CreateMilestoneDtoValidator());
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("里程碑ID不合法");
    }
}

public class CreateAcceptanceDtoValidator : AbstractValidator<CreateAcceptanceDto>
{
    public CreateAcceptanceDtoValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0).WithMessage("项目ID不合法");
        RuleFor(x => x.AcceptBatch).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AcceptAmount).GreaterThan(0).WithMessage("验收金额必须大于 0");
    }
}

public class UpdateAcceptanceDtoValidator : AbstractValidator<UpdateAcceptanceDto>
{
    public UpdateAcceptanceDtoValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("验收记录ID不合法");
        RuleFor(x => x.AcceptBatch).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AcceptAmount).GreaterThan(0).WithMessage("验收金额必须大于 0");
    }
}

public class CreateContractDtoValidator : AbstractValidator<CreateContractDto>
{
    public CreateContractDtoValidator()
    {
        RuleFor(x => x.ContractNo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PartyA).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PartyB).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("合同结束日期不能早于开始日期");
    }
}

public class UpdateContractDtoValidator : AbstractValidator<UpdateContractDto>
{
    public UpdateContractDtoValidator()
    {
        Include(new CreateContractDtoValidator());
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("合同ID不合法");
    }
}

public class CreateInvoiceDtoValidator : AbstractValidator<CreateInvoiceDto>
{
    public CreateInvoiceDtoValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0).WithMessage("项目ID不合法");
        RuleFor(x => x.ReceiptName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
    }
}

public class UpdateInvoiceDtoValidator : AbstractValidator<CreateInvoiceDto>
{
    public UpdateInvoiceDtoValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0).WithMessage("项目ID不合法");
        RuleFor(x => x.ReceiptName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
    }
}
