using Microsoft.EntityFrameworkCore;
using StudentPlanner.Core.Domain.Entities;
using StudentPlanner.Core.Domain.RepositoryContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Infrastructure.Repositories;

public class FacultyRepository : IFacultyRepository
{
    private readonly ApplicationDbContext _context;
    public FacultyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Faculty>> GetAllFacultiesAsync()
    {
        return await _context.Faculties.Select(f => f.ToFaculty()).ToListAsync();
    }

    public async Task<Faculty?> GetFacultyByIdAsync(Guid facultyId)
    {
        return (await _context.Faculties.FirstOrDefaultAsync(f => f.Id == facultyId))?.ToFaculty();
    }

    public async Task<Faculty?> GetFacultyByUsosIdAsync(string facultyId)
    {
        return (await _context.Faculties.FirstOrDefaultAsync(f => f.FacultyId == facultyId))?.ToFaculty();
    }
    public async Task<Faculty?> GetFacultyByFacultyCodeAsync(string facultycode)
    {
        return (await _context.Faculties.FirstOrDefaultAsync(f => f.FacultyCode == facultycode))?.ToFaculty();
    }
}
