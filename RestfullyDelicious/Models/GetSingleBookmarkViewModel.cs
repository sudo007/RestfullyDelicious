using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfullyDelicious.Models
{
	public class GetSingleBookmarkViewModel
	{
		public string Url { get; set; }

		public string Description { get; set; }

		public DateTime Time { get; set; }

		public List<TagSummary> Tags { get; set; }
	}
}