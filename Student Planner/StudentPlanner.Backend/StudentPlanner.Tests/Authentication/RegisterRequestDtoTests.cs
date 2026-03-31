using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using StudentPlanner.Core.Application.Authentication;
using Xunit;

namespace StudentPlanner.Tests.Authentication;

public class RegisterRequestDtoTests
{
    [Fact]
    public void RegisterRequestDto_ShouldFailValidation_WhenDomainIsNotPwEduPl()
    {
        var request = new RegisterRequestDto
        {
            Email = "test@gmail.com",
            Password = "Password123!"
        };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        isValid.Should().BeFalse();
        results.Should().Contain(x => x.ErrorMessage != null && x.ErrorMessage.Contains("@pw.edu.pl"));
    }


}
