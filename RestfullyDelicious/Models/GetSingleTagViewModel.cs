using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfullyDelicious.Models
{
	public class GetSingleTagViewModel
	{
		public string Name { get; set; }

		public List<BookmarkSummary> Bookmarks { get; set; }
	}
}