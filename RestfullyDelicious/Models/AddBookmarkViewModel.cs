using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfullyDelicious.Models
{
	public class AddBookmarkViewModel
	{
		public string Url { get; set; }

		public string Description { get; set; }

		public List<TagSummary> Tags { get; set; }
	}
}