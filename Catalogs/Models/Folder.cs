using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Catalogs.Models
{
	public class Folder
	{
		public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Description { get; set; }

		public int? FolderId { get; set; }

		public ICollection<Folder> Folders { get; set; } = new List<Folder>();
    }
}

