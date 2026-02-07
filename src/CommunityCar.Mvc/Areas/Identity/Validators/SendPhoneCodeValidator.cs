using FluentValidation;
using CommunityCar.Web.Areas.Identity.ViewModels;

namespace CommunityCar.Web.Areas.Identity.Validators;

public class SendPhoneCodeValidator : AbstractValidator<SendPhoneCodeViewModel>
{
    public SendPhoneCodeValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format");
    }
}
