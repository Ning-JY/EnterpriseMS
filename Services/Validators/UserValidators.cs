using FluentValidation;
using EnterpriseMS.Services.DTOs.User;

namespace EnterpriseMS.Services.Validators;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
        RuleFor(x => x.RealName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("手机号长度不能超过20个字符");
        RuleFor(x => x.Phone)
            .Matches(@"^1[3-9]\d{9}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("手机号格式不正确");
    }
}

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("用户ID不合法");
        RuleFor(x => x.RealName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("手机号长度不能超过20个字符");
        RuleFor(x => x.Phone)
            .Matches(@"^1[3-9]\d{9}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("手机号格式不正确");
    }
}

public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.OldPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).MaximumLength(100);
        RuleFor(x => x.NewPassword).NotEqual(x => x.OldPassword)
            .WithMessage("新密码不能与旧密码相同");
    }
}
