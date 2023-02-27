using System;

namespace Catalogs.Models
{
	public class FoldersViewModel
	{
		public IEnumerable<Folder> Folders { get; set; } = new List<Folder>();

		public Folder Folder { get; set; }
	}
}

