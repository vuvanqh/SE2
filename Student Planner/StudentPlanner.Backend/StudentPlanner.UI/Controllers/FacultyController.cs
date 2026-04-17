using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Core.Application.ServiceContracts;
using StudentPlanner.Core.Entities;

namespace StudentPlanner.UI.Controllers;

/// <summary>
/// Provides endpoints for managing and retrieving faculty data.
/// </summary>
/// <remarks>
/// All endpoints are restricted to users with the <see cref="UserRoleOptions.Admin"/> role.
/// This controller exposes read-only operations for accessing faculty information.
/// </remarks>
[Route("api/[controller]")]
[Authorize(Roles = nameof(UserRoleOptions.Admin))]
[ApiController]
public class FacultyController : ControllerBase
{
    private readonly IFacultyService _facultyService;
    /// <summary>
    /// Initializes a new instance of the <see cref="FacultyController"/> class.
    /// </summary>
    /// <param name="facultyService">
    /// Service responsible for handling faculty-related business logic.
    /// </param>
    public FacultyController(IFacultyService facultyService)
    {
        _facultyService = facultyService;
    }

    /// <summary>
    /// Retrieves all faculties available in the system.
    /// </summary>
    /// <returns>A collection of faculties.</returns>
    /// <response code="200">Returns the list of faculties.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required admin role.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _facultyService.GetAllFaculties());
    }

    /// <summary>
    /// Retrieves a specific faculty by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the faculty.</param>
    /// <returns>The faculty matching the specified identifier.</returns>
    /// <response code="200">Returns the requested faculty.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required admin role.</response>
    /// <response code="404">If the faculty does not exist.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFacultyById(Guid id)
    {
        return Ok(await _facultyService.GetFacultyByIdAsync(id));
    }
}
