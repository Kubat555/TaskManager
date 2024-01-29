using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManager;
using TaskManager.DTO;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Projects
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? priority, string sortOrder)
        {
            List<Project> projectFiltr;
            if (startDate != null && endDate != null && priority != null)
            {
                projectFiltr = await _context.Projects
                    .Include(p => p.ProjectManager)
                    .Where(p => p.StartDate >= startDate.Value && p.EndDate <= endDate.Value && p.Priority == priority)
                    .ToListAsync();      
            }
            else if (startDate != null && endDate != null)
            {
                projectFiltr = await _context.Projects
                    .Include(p => p.ProjectManager)
                    .Where(p => p.StartDate >= startDate.Value && p.EndDate <= endDate.Value)
                    .ToListAsync();
            }
            else if(priority != null)
            {
                projectFiltr = await _context.Projects
                    .Include(p => p.ProjectManager)
                    .Where(p => p.Priority == priority)
                    .ToListAsync();
                
            }
            else
            {
                projectFiltr = await _context.Projects.Include(p => p.ProjectManager).ToListAsync();
                    
            }
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["Priority"] = priority != null ? priority?.ToString(): "Все";

            return View(projectFiltr);
        }

        // GET: Projects/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = _context.Projects
                .Include(p => p.ProjectManager)
                .Include(p => p.Employees)
                .ThenInclude(ep => ep.Employee)
                .FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            var viewModel = new ProjectDetailsViewModel
            {
                Project = project,
                ProjectManager = project.ProjectManager,
                Employees = project.Employees.Select(ep => ep.Employee).ToList()
            };

            return View(viewModel);
        }


        // GET: Projects/Create
        public IActionResult Create()
        {
            var viewModel = new ProjectCreateViewModel
            {
                Project = new Project(),
                Employees = _context.Employees.Select(e => new SelectListItem { Value = e.Id.ToString(), Text = $"{e.FirstName} {e.LastName}" }).ToList()
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var project = viewModel.Project;

                // Устанавливаем руководителя проекта
                if (viewModel.SelectedManagerId != 0)
                {
                    var manager = await _context.Employees.FindAsync(viewModel.SelectedManagerId);
                    project.ProjectManager = manager;

                }

                _context.Add(project);
                await _context.SaveChangesAsync();

                AddEmployeeToProject(new EmployeeProject() { ProjectId = project.Id, EmployeeId = viewModel.SelectedManagerId});

                return RedirectToAction(nameof(Index));
            }

            // Если ModelState недействителен, нужно обновить список сотрудников в модели представления
            viewModel.Employees = _context.Employees
        .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = $"{e.FirstName} {e.LastName}" })
        .ToList();

            return View(viewModel);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = _context.Projects
                .Include(p => p.ProjectManager)
                .Include(p => p.Employees)
                .ThenInclude(ep => ep.Employee)
                .FirstOrDefault(p => p.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            var viewModel = new ProjectCreateViewModel
            {
                Project = project,
                Employees = _context.Employees.Select(e => new SelectListItem { Value = e.Id.ToString(), Text = $"{e.FirstName} {e.LastName}" }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectCreateViewModel viewModel)
        {
            if (id != viewModel.Project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var project = viewModel.Project;

                    _context.Update(project);
                    await _context.SaveChangesAsync();
                    AddEmployeeToProject(new EmployeeProject() { EmployeeId = (int)project.ProjectManagerId, ProjectId = project.Id });

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(viewModel.Project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = id});
            }

            // Если ModelState недействителен, нужно обновить список сотрудников в модели представления
            viewModel.Employees = await _context.Employees
                .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = $"{e.FirstName} {e.LastName}" })
                .ToListAsync();

            return View(viewModel);
        }


        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'AppDbContext.Projects'  is null.");
            }
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
          return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [NonAction]
        private void AddEmployeeToProject(EmployeeProject employeeProject)
        {
            try
            {
                if(!(_context.EmployeeProjects?.
                    Any(e => e.EmployeeId == employeeProject.EmployeeId && e.ProjectId == employeeProject.ProjectId))
                    .GetValueOrDefault())
                {
                    _context.EmployeeProjects.Add(employeeProject);
                    _context.SaveChanges();
                }
                
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveEmployee(int projectId, int employeeId)
        {
            var employeeProject = await _context.EmployeeProjects
                .FirstOrDefaultAsync(ep => ep.ProjectId == projectId && ep.EmployeeId == employeeId);

            if (employeeProject != null)
            {
                _context.EmployeeProjects.Remove(employeeProject);
                var project = _context.Projects.Find(projectId);
                if (project != null && project.ProjectManagerId != null && project.ProjectManagerId == employeeId)
                {
                    project.ProjectManagerId = null;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        public IActionResult AddEmployee(int id)
        {
            var project = _context.Projects
                .FirstOrDefault(p => p.Id == id);
            var employeesProject = _context.EmployeeProjects.Where(ep => ep.ProjectId == id);

            if (project == null)
            {
                return NotFound();
            }

            var viewModel = new AddEmployeeViewModel
            {
                ProjectId = id,
                ProjectName = project.ProjectName,
                AvailableEmployees = _context.Employees
                    .Where(e => !employeesProject.Any(ep => ep.EmployeeId == e.Id))
                    .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = $"{e.FirstName} {e.LastName}" })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmployee(AddEmployeeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                _context.EmployeeProjects.Add(new EmployeeProject() { EmployeeId = viewModel.SelectedEmployeeId, ProjectId = viewModel.ProjectId });
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = viewModel.ProjectId });
            }

            viewModel.AvailableEmployees = _context.Employees
                .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = $"{e.FirstName} {e.LastName}" })
                .ToList();

            return View(viewModel);
        }
    }
}
