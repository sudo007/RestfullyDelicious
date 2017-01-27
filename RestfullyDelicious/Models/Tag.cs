using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RestfullyDelicious.Models
{
	public class Tag
	{
		[Key]
		public int TagId { get; set; }

		public string UserId { get; set; }

		public string Name { get; set; }
		
		public List<Bookmark> Bookmarks { get; set; }
	}
}