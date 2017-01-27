using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RestfullyDelicious.Models
{
	public class Bookmark
	{
		[Key]
		public int BookmarkId { get; set; }

		public string Url { get; set; }

		public List<Tag> Tags { get; set; }

		public string Href { get; set; }

		public DateTime Time { get; set; }

		public string Description { get; set; }

		public string UserId { get; set; }
	}
}