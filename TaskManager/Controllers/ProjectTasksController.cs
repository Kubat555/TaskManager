using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManager;
using TaskManager.Migrations;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    public class ProjectTasksController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectTasksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ProjectTasks
        public IActionResult Index(int? projectId)
        {
            var tasksQuery = _context.ProjectTasks
                .Include(t => t.Author)
                .Include(t => t.Executor)
                .Include(t => t.Project)
                .AsQueryable();

            if (projectId.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.ProjectId == projectId);
                ViewBag.ProjectId = projectId;
            }
            else
            {
                ViewBag.ProjectId = new SelectList(_context.Projects, "Id", "ProjectName");
            }

            var tasks = tasksQuery.ToList();
            return View(tasks);
        }

        // GET: ProjectTasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ProjectTasks == null)
            {
                return NotFound();
            }

            var projectTask = await _context.ProjectTasks
                .Include(p => p.Author)
                .Include(p => p.Executor)
                .Include(p => p.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (projectTask == null)
            {
                return NotFound();
            }

            return View(projectTask);
        }

        // GET: ProjectTasks/Create
        public IActionResult Create( int id)
        {
            ViewData["AuthorId"] = new SelectList(_context.Employees, "Id", "FirstName");
            var project = _context.Projects
                .Include(p => p.Employees)
                .ThenInclude(ep => ep.Employee)
                .FirstOrDefault(p => p.Id == id);
            ViewData["ExecutorId"] = new SelectList(project.Employees.Select(ep => ep.Employee), "Id", "FirstName");
            ViewData["ProjectName"] = _context.Projects.Find(id)?.ProjectName;
            ViewData["ProjectId"] = id;

            return View(new ProjectTask() { ProjectId = id});
        }

        // POST: ProjectTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TaskName,Comment,Priority,Status,AuthorId,ExecutorId,ProjectId")] ProjectTask projectTask)
        {
            if (ModelState.IsValid)
            {
                _context.Add(projectTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new {projectId = projectTask.ProjectId});
            }
            ViewData["AuthorId"] = new SelectList(_context.Employees, "Id", "FirstName", projectTask.AuthorId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "ProjectName", projectTask.ProjectId);
            var project = _context.Projects
                .Include(p => p.Employees)
                .ThenInclude(ep => ep.Employee)
                .FirstOrDefault(p => p.Id == projectTask.ProjectId);
            ViewData["ExecutorId"] = new SelectList(project.Employees.Select(ep => ep.Employee), "Id", "FirstName", projectTask.ExecutorId);
            return View(projectTask);
        }

        // GET: ProjectTasks/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectTask = _context.ProjectTasks
                .Include(pt => pt.Author)
                .Include(pt => pt.Executor)
                .Include(pt => pt.Project)  
                .FirstOrDefault(pt => pt.Id == id);

            if (projectTask == null)
            {
                return NotFound();
            }

            // Подготавливаем список для выпадающего списка сотрудников, тут будут только те сотрудники которые есть в проекте задачи

            var project = _context.Projects
                .Include(p => p.Employees)
                .ThenInclude(ep => ep.Employee)
                .FirstOrDefault(p => p.Id == projectTask.ProjectId);
            ViewData["ExecutorId"] = new SelectList(project.Employees.Select(ep => ep.Employee), "Id", "FirstName", projectTask.ExecutorId);

            return View(projectTask);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TaskName,Comment,Priority,Status,ExecutorId,ProjectId")] ProjectTask projectTask)
        {
            if (id != projectTask.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Получаем существующую задачу с подробными данными
                    var existingTask = _context.ProjectTasks
                        .Include(pt => pt.Author)
                        .Include(pt => pt.Executor)
                        .Include(pt => pt.Project)
                        .FirstOrDefault(pt => pt.Id == id);
                    ViewData["projectId"] = existingTask.ProjectId;
                    // Копируем значения, которые необходимо сохранить
                    existingTask.TaskName = projectTask.TaskName;
                    existingTask.Comment = projectTask.Comment;
                    existingTask.Priority = projectTask.Priority;
                    existingTask.Status = projectTask.Status;
                    existingTask.ExecutorId = projectTask.ExecutorId;

                    // Сохраняем изменения
                    _context.Update(existingTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectTaskExists(projectTask.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { projectId = ViewData["projectId"] });
            }

            // Подготавливаем списки для выпадающих списков
            var project = _context.Projects
                .Include(p => p.Employees)
                .ThenInclude(ep => ep.Employee)
                .FirstOrDefault(p => p.Id == projectTask.ProjectId);
            ViewData["ExecutorId"] = new SelectList(project.Employees.Select(ep => ep.Employee), "Id", "FirstName", projectTask.ExecutorId);

            return View(projectTask);
        }



        // GET: ProjectTasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ProjectTasks == null)
            {
                return NotFound();
            }

            var projectTask = await _context.ProjectTasks
                .Include(p => p.Author)
                .Include(p => p.Executor)
                .Include(p => p.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (projectTask == null)
            {
                return NotFound();
            }

            return View(projectTask);
        }

        // POST: ProjectTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ProjectTasks == null)
            {
                return Problem("Entity set 'AppDbContext.ProjectTasks'  is null.");
            }
            var projectTask = await _context.ProjectTasks.FindAsync(id);
            int projId = (int)projectTask.ProjectId;
            _context.ProjectTasks.Remove(projectTask);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { projectId = projId });
        }

        private bool ProjectTaskExists(int id)
        {
          return (_context.ProjectTasks?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
