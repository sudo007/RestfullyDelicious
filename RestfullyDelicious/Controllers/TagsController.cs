using Microsoft.AspNet.Identity.Owin;
using RestfullyDelicious.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RestfullyDelicious.Controllers
{
	public class TagsController : ApiController
	{
		ApplicationDbContext _dbContext;
		ApplicationUserManager _userManager;

		public ApplicationUserManager UserManager
		{
			get
			{
				return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
			}
			private set
			{
				_userManager = value;
			}
		}

		public TagsController()
		{
			_dbContext = new ApplicationDbContext();
		}

		//GET api/username/Tags
		public GetTagsResponse Get(string username, int? start = 1, int? length = 2)
		{
			GetTagsResponse response = new GetTagsResponse();
			response.Tags = new List<TagViewModel>();
			response.Start = start.Value;
			response.Length = length.Value;
			string absolute = RequestContext.Url.Request.RequestUri.AbsoluteUri;
			string nextUrl = absolute.Split('?')[0];
			nextUrl += "?start=" + (start + length);
			nextUrl += "&length=" + length;

			var query = (from t in _dbContext.Tags
						 join u in _dbContext.Users on t.UserId equals u.Id
						 where u.UserName == username
						 select t);
			query = query.OrderBy(x => x.TagId);
			if (start > 1)
			{
				query = query.Skip(start.Value - 1);
			}

			query = query.Take(length.Value);
			List<Tag> dbTags = query.ToList();
			foreach (var tag in dbTags)
			{
				response.Tags.Add(new TagViewModel()
				{
					Id = tag.TagId,
					Name = tag.Name
				});
			}
			if (response.Tags.Count >= length)
			{
				response.Next = nextUrl;
			}

			return response;
		}

		//GET api/username/Tags/id
		public IHttpActionResult Get(string username, int id)
		{
			GetSingleTagViewModel viewModel = new GetSingleTagViewModel();
			viewModel.Bookmarks = new List<BookmarkSummary>();

			var tag = (from t in _dbContext.Tags
						 join u in _dbContext.Users on t.UserId equals u.Id
						 where u.UserName == username
						 select new { Tag = t, Bookmarks = t.Bookmarks }).SingleOrDefault();

			if (tag == null)
			{
				return NotFound();
			}
			else
			{
				string authority = RequestContext.Url.Request.RequestUri.Authority;
				viewModel.Name = tag.Tag.Name;
				foreach (var bookmark in tag.Bookmarks)
				{
					viewModel.Bookmarks.Add(new BookmarkSummary() { Url = bookmark.Url, Href = "http://" + authority + $"/api/{username}/Bookmarks/{bookmark.BookmarkId}" });
				}
				return Ok(viewModel);
			}
		}

		//Note - for now all this does is update the "Name" property
		//PUT api/username/Tags/id
		[HttpPut]
		public async Task<IHttpActionResult> Put(string username, int id, AddTagViewModel viewModel)
		{
			if (ModelState.IsValid)
			{
				Tag tag = _dbContext.Tags.SingleOrDefault(x => x.TagId == id);
				if (tag != null)
				{
					tag.Name = viewModel.Name;
					
					try
					{
						await _dbContext.SaveChangesAsync();
					}
					catch (Exception ex)
					{
						throw;
					}

					return Ok();
				}
				else
				{
					return NotFound();
				}
			}
			else
			{
				return BadRequest(ModelState);
			}
		}
	}
}
