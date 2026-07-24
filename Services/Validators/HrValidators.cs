using FluentValidation;
using EnterpriseMS.Services.DTOs.Hr;

namespace EnterpriseMS.Services.Validators;

public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
{
    public CreateEmployeeDtoValidator()
    {
        RuleFor(x => x.RealName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Gender).InclusiveBetween(0, 2).WithMessage("性别不合法");
        RuleFor(x => x.Phone).MaximumLength(20)
            .Matches(@"^1[3-9]\d{9}$").When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("手机号格式不正确");
        RuleFor(x => x.Email).MaximumLength(100)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.IdCard).MaximumLength(18);
        RuleFor(x => x.EntryDate).NotNull().WithMessage("入职日期不能为空");
    }
}

public class UpdateEmployeeDtoValidator : AbstractValidator<UpdateEmployeeDto>
{
    public UpdateEmployeeDtoValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("员工ID不合法");
        RuleFor(x => x.RealName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Phone).MaximumLength(20)
            .Matches(@"^1[3-9]\d{9}$").When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("手机号格式不正确");
        RuleFor(x => x.Email).MaximumLength(100)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public class CreateEducationDtoValidator : AbstractValidator<CreateEducationDto>
{
    public CreateEducationDtoValidator()
    {
        RuleFor(x => x.SchoolName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Major).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Degree).NotEmpty().MaximumLength(50);
    }
}

public class CreateWorkExpDtoValidator : AbstractValidator<CreateWorkExpDto>
{
    public CreateWorkExpDtoValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Position).NotEmpty().MaximumLength(100);
    }
}
