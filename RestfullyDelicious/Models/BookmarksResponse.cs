using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfullyDelicious.Models
{
	public class BookmarksResponse
	{
		public List<Bookmark> Bookmarks { get; set; }

		public int Start { get; set; }

		public int Length { get; set; }

		public string Next { get; set; }
	}
}