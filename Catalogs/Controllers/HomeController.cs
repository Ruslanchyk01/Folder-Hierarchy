using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Catalogs.Models;
using Catalogs.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace Catalogs.Controllers;

public class HomeController : Controller
{
    private readonly FoldersContext _context;

    public HomeController(FoldersContext context)
    {
        _context = context;

        if (!_context.Folders.Any())
        {

            Folder folder1 = new Folder { Name = "Creating Digital Images", Description = "Folder", FolderId = null };
            Folder folder2 = new Folder { Name = "Resources", Description = "Folder", FolderId = 1 };
            Folder folder3 = new Folder { Name = "Evidence", Description = "Folder", FolderId = 1 };
            Folder folder4 = new Folder { Name = "Graphic Products", Description = "Folder", FolderId = 1 };
            Folder folder5 = new Folder { Name = "Primary Sources", Description = "Folder", FolderId = 2 };
            Folder folder6 = new Folder { Name = "Secondary Sources", Description = "Folder", FolderId = 2 };
            Folder folder7 = new Folder { Name = "Process", Description = "Folder", FolderId = 4 };
            Folder folder8 = new Folder { Name = "Final Product", Description = "Folder", FolderId = 4 };

            _context.Folders.AddRange(folder1, folder2, folder3, folder4, folder5, folder6, folder7, folder8);
            _context.SaveChanges();
        }
    }


    //Головна сторінка
    public IActionResult Index(string name)
    {
        var mainFolder = _context.Folders.Include(p => p.Folders).FirstOrDefault(f => f.FolderId == null);

        FoldersViewModel viewModel = new FoldersViewModel
        {
            Folder = mainFolder,
            Folders = mainFolder?.Folders?.ToList(),
        };

        return View(viewModel);
    }

    //Дочірні Елементи
    public IActionResult ChildFolders(int id)
    {
        var folder = _context.Folders
            .Include(f => f.Folders)
            .FirstOrDefault(f => f.Id == id);

        FoldersViewModel viewModel = new FoldersViewModel
        {
            Folder = folder,
            Folders = folder?.Folders?.ToList(),
        };

        return View(viewModel);
    }


    //Викачування структури
    public async Task<IActionResult> Export()
    {
        var folders = await _context.Folders.ToListAsync();
        var sb = new StringBuilder();

        foreach (var directory in folders)
        {
            sb.AppendLine($"Directory Name: {directory.Name}");
            sb.AppendLine($"Directory Description: {directory.Description}");
            sb.AppendLine($"Directory ParentID: {directory.FolderId}");
            sb.AppendLine();
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());

        return File(new MemoryStream(bytes), "text/plain", "folders.txt");
    }


    //Завантаження структури з ОС
    [HttpPost]
    public async Task<IActionResult> ImportFromOS(string path)
    {
        _context.Folders.RemoveRange(_context.Folders);

        await _context.SaveChangesAsync();
        await ImportFolder(path);

        return RedirectToAction("Index");
    }

    private async Task ImportFolder(string path, int? parentId = null)
    {
        var directoryInfo = new DirectoryInfo(path);

        var folder = new Folder
        {
            Name = directoryInfo.Name,
            Description = "Folder",
            FolderId = parentId
        };

        _context.Folders.Add(folder);
        await _context.SaveChangesAsync();

        foreach (var subdirectoryInfo in directoryInfo.GetDirectories())
        {
            await ImportFolder(subdirectoryInfo.FullName, folder.Id);
        }
    }

    public IActionResult ImportFromOS()
    {
        return View();
    }

    //Завантаження структури з файлу.!!!!!!!!ПРАЦЮЄ ПОГАНО - НЕ ЗБЕРІГАЄ КЛЮЧ БАТЬКА!!!!!!!!!!
    [HttpPost]
    public async Task<IActionResult> ImportFromFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("file", "Please select a file.");
            return View();
        }

        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            var lines = await reader.ReadToEndAsync();

            var linesArray = lines.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            _context.Folders.RemoveRange(_context.Folders);
            await _context.SaveChangesAsync();

            Folder folder = null;

            foreach (var line in linesArray)
            {
                var keyValue = line.Split(": ", StringSplitOptions.RemoveEmptyEntries);

                if (keyValue.Length != 2)
                {
                    continue;
                }

                var key = keyValue[0];
                var value = keyValue[1];

                if (key == "Directory Name")
                {
                    folder = new Folder { Name = value };
                }
                else if (key == "Directory Description")
                {
                    if (folder != null)
                    {
                        folder.Description = value;
                    }
                }
                else if (key == "Directory ParentID" && int.TryParse(value, out var parentId))
                {
                    if (folder != null) 
                    {
                        folder.FolderId = parentId;
                    }
                }
                else
                {
                    continue;
                }

                if (folder != null && folder.Name != null && folder.Description != null)
                {
                    _context.Folders.Add(folder);
                    await _context.SaveChangesAsync();
                    folder = null;
                }
            }
        }

        return RedirectToAction("Index");
    }

    public IActionResult ImportFromFile()
    {
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}