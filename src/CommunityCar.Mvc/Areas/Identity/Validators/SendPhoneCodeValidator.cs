using FluentValidation;
using CommunityCar.Mvc.Areas.Identity.ViewModels;

namespace CommunityCar.Mvc.Areas.Identity.Validators;

public class SendPhoneCodeValidator : AbstractValidator<SendPhoneCodeViewModel>
{
    public SendPhoneCodeValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format");
    }
}
