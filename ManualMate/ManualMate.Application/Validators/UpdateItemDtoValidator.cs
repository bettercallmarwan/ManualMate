using FluentValidation;
using ManualMate.Application.DTOs;

namespace ManualMate.Application.Validators;

public class UpdateItemDtoValidator : AbstractValidator<UpdateItemDto>
{
    public UpdateItemDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(50).WithMessage("Item name cannot exceed 50 characters");
        RuleFor(x => x.Description)
            .MaximumLength(600).WithMessage("Item description cannot exceed 600 characters");
    }
}