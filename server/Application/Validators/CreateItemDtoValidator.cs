using FluentValidation;
using ManualMate.Application.DTOs;

namespace ManualMate.Application.Validators
{
    public class CreateItemDtoValidator : AbstractValidator<CreateItemDto>
    {
        public CreateItemDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Item name is required")
                .MaximumLength(50).WithMessage("Item name cannot exceed 50 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Item description is required")
                .MaximumLength(600).WithMessage("Item description cannot exceed 600 characters");
        }
    }
}