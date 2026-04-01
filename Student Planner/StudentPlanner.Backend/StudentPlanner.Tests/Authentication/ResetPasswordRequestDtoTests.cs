using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using StudentPlanner.Core.Application.Authentication;
using Xunit;

namespace StudentPlanner.Tests.Authentication;

public class ResetPasswordRequestDtoTests
{
    [Fact]
    public void ResetPasswordRequestDto_ShouldFailValidation_WhenPasswordsDoNotMatch()
    {
        var request = new ResetPasswordRequestDto
        {
            Email = "user@pw.edu.pl",
            Token = "mocked-token",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "DifferentPassword!"
        };

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        isValid.Should().BeFalse();
        results.Should().Contain(x => x.ErrorMessage != null && x.ErrorMessage.Contains("match"));
    }
}
