using FluentValidation;
using EnterpriseMS.Services.DTOs.System;

namespace EnterpriseMS.Services.Validators;

// ── Role ──────────────────────────────────────────────────
public class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
{
    public CreateRoleDtoValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.RoleCode).NotEmpty().MaximumLength(50)
            .Matches("^[A-Za-z0-9_]+$").WithMessage("角色编码只能包含字母、数字和下划线");
        RuleFor(x => x.DataScope).InclusiveBetween(1, 5).WithMessage("数据权限范围不合法");
        RuleFor(x => x.Sort).GreaterThanOrEqualTo(0);
    }
}

public class UpdateRoleDtoValidator : AbstractValidator<UpdateRoleDto>
{
    public UpdateRoleDtoValidator()
    {
        Include(new CreateRoleDtoValidator());
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("角色ID不合法");
    }
}

// ── Menu ──────────────────────────────────────────────────
public class CreateMenuDtoValidator : AbstractValidator<CreateMenuDto>
{
    public CreateMenuDtoValidator()
    {
        RuleFor(x => x.MenuName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.MenuType).NotEmpty().MaximumLength(1);
        RuleFor(x => x.Sort).GreaterThanOrEqualTo(0);
    }
}

public class UpdateMenuDtoValidator : AbstractValidator<UpdateMenuDto>
{
    public UpdateMenuDtoValidator()
    {
        Include(new CreateMenuDtoValidator());
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("菜单ID不合法");
    }
}

// ── Dept ──────────────────────────────────────────────────
public class CreateDeptDtoValidator : AbstractValidator<CreateDeptDto>
{
    public CreateDeptDtoValidator()
    {
        RuleFor(x => x.DeptName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Sort).GreaterThanOrEqualTo(0);
    }
}

public class UpdateDeptDtoValidator : AbstractValidator<UpdateDeptDto>
{
    public UpdateDeptDtoValidator()
    {
        Include(new CreateDeptDtoValidator());
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("部门ID不合法");
    }
}

// ── Dict Type ─────────────────────────────────────────────
public class CreateDictTypeDtoValidator : AbstractValidator<CreateDictTypeDto>
{
    public CreateDictTypeDtoValidator()
    {
        RuleFor(x => x.DictName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DictType).NotEmpty().MaximumLength(50)
            .Matches("^[A-Za-z0-9_]+$").WithMessage("字典类型只能包含字母、数字和下划线");
    }
}

public class UpdateDictTypeDtoValidator : AbstractValidator<UpdateDictTypeDto>
{
    public UpdateDictTypeDtoValidator()
    {
        Include(new CreateDictTypeDtoValidator());
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("字典类型ID不合法");
    }
}

// ── Dict Data ─────────────────────────────────────────────
public class CreateDictDataDtoValidator : AbstractValidator<CreateDictDataDto>
{
    public CreateDictDataDtoValidator()
    {
        RuleFor(x => x.DictType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DictLabel).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DictValue).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Sort).GreaterThanOrEqualTo(0);
    }
}

public class UpdateDictDataDtoValidator : AbstractValidator<UpdateDictDataDto>
{
    public UpdateDictDataDtoValidator()
    {
        Include(new CreateDictDataDtoValidator());
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("字典数据ID不合法");
    }
}
