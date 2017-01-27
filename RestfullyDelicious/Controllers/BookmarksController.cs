using Microsoft.AspNet.Identity.Owin;
using RestfullyDelicious.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace RestfullyDelicious.Controllers
{
	public class BookmarksController : ApiController
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

		public BookmarksController()
		{
			_dbContext = new ApplicationDbContext();
		}

		//GET api/username/Bookmarks
		public BookmarksResponse Get(string username, string tag = null, string dt = null, int? start = 1, int? length = 2)
		{
			BookmarksResponse response = new BookmarksResponse();
			response.Start = start.Value;
			response.Length = length.Value;
			string absolute = RequestContext.Url.Request.RequestUri.AbsoluteUri;
			string nextUrl = absolute.Split('?')[0];
			nextUrl += "?start=" + (start + length);
			nextUrl += "&length=" + length;

			var query = (from b in _dbContext.Bookmarks
						 join u in _dbContext.Users on b.UserId equals u.Id
						 where u.UserName == username
						 select b);
			if (tag != null)
			{
				nextUrl += "&tag=" + tag;
				query = query.Where(x => x.Tags.Exists(y => y.Name == tag));
			}
			if (dt != null)
			{
				nextUrl += "&dt=" + dt;
				DateTime date = DateTime.Parse(dt);
				query = query.Where(x => DbFunctions.TruncateTime(x.Time).Value == date.Date);
			}
			query = query.OrderBy(x => x.BookmarkId);
			if (start > 1)
			{
				query = query.Skip(start.Value - 1);
			}

			query = query.Take(length.Value);
			response.Bookmarks = query.ToList();
			if (response.Bookmarks.Count >= length)
			{
				response.Next = nextUrl;
			}
			foreach (var bookmark in response.Bookmarks)
			{
				bookmark.Href = absolute.Split('?')[0] + "/" + bookmark.BookmarkId;
			}

			return response;
		}

		//GET api/username/Bookmarks/1
		public IHttpActionResult Get(string username, int id)
		{
			var viewModel = new GetSingleBookmarkViewModel();
			var bookmark = (from b in _dbContext.Bookmarks
						 join u in _dbContext.Users on b.UserId equals u.Id
						 where u.UserName == username
						 && b.BookmarkId == id
						 select new { Bookmark = b, Tags = b.Tags }).SingleOrDefault();
			if (bookmark == null)
			{
				return NotFound();
			}
			else
			{
				string authority = RequestContext.Url.Request.RequestUri.Authority;
				viewModel.Description = bookmark.Bookmark.Description;
				viewModel.Time = bookmark.Bookmark.Time;
				viewModel.Url = bookmark.Bookmark.Url;
				viewModel.Tags = new List<TagSummary>();
				foreach (var tag in bookmark.Tags)
				{
					viewModel.Tags.Add(new TagSummary() { Name = tag.Name, Href = "http://" + authority + $"/api/{username}/Tags/{tag.TagId}" });
				}

				return Ok(viewModel);
			}
		}

		//GET api/username/Bookmarks/Count
		[Route("api/{username}/Bookmarks/Count")]
		[HttpGet]
		public int GetCount(string username, string tag = null)
		{
			var query = (from b in _dbContext.Bookmarks
						 join u in _dbContext.Users on b.UserId equals u.Id
						 where u.UserName == username
						 select b);
			if (tag != null)
			{
				query = query.Where(x => x.Tags.Exists(y => y.Name == tag));
			}

			return query.Count();
		}

		//GET api/username/Bookmarks/Count
		[Route("api/{username}/Bookmarks/Update")]
		[HttpGet]
		public DateTime GetUpdate(string username)
		{
			var result = (from b in _dbContext.Bookmarks
						  join u in _dbContext.Users on b.UserId equals u.Id
						  where u.UserName == username
						  orderby b.Time descending
						  select b.Time).First();

			return result;
		}

		//POST api/username/Bookmarks
		[HttpPost]
		public async Task<IHttpActionResult> Post(string username, AddBookmarkViewModel viewModel)
		{
			if (ModelState.IsValid)
			{
				Bookmark bookmark = new Bookmark();
				bookmark.Description = viewModel.Description;
				bookmark.Url = viewModel.Url;
				bookmark.Time = DateTime.Now;
				var user = await UserManager.FindByNameAsync(username);
				if (viewModel.Tags.Count > 0)
				{
					bookmark.Tags = new List<Tag>();
					foreach (var tag in viewModel.Tags)
					{
						Tag dbTag = _dbContext.Tags.FirstOrDefault(x => x.UserId == user.Id && x.Name == tag.Name);
						if (dbTag != null)
						{
							bookmark.Tags.Add(dbTag);
						}
					}
				}
				_dbContext.Bookmarks.Add(bookmark);
				try
				{
					await _dbContext.SaveChangesAsync();
				}
				catch (Exception ex)
				{
					throw;
				}
				string absolute = RequestContext.Url.Request.RequestUri.AbsoluteUri;
				string nextUrl = absolute.Split('?')[0];

				return Created(nextUrl + "/" + bookmark.BookmarkId, string.Empty);
			}
			else
			{
				return BadRequest(ModelState);
			}
		}

		//PUT api/username/Bookmarks
		[HttpPut]
		public async Task<IHttpActionResult> Put(string username, int id, AddBookmarkViewModel viewModel)
		{
			if (ModelState.IsValid)
			{
				Bookmark bookmark = _dbContext.Bookmarks.SingleOrDefault(x => x.BookmarkId == id);
				if (bookmark != null)
				{
					bookmark.Description = viewModel.Description;
					bookmark.Url = viewModel.Url;
					bookmark.Time = DateTime.Now;
					var user = await UserManager.FindByNameAsync(username);
					if (viewModel.Tags.Count > 0)
					{
						bookmark.Tags = new List<Tag>();
						foreach (var tag in viewModel.Tags)
						{
							Tag dbTag = _dbContext.Tags.FirstOrDefault(x => x.UserId == user.Id && x.Name == tag.Name);
							if (dbTag != null)
							{
								bookmark.Tags.Add(dbTag);
							}
						}
					}
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

		//POST api/username/Bookmarks
		[HttpDelete]
		public async Task<IHttpActionResult> Delete(string username, int id)
		{
			Bookmark bookmark = _dbContext.Bookmarks.SingleOrDefault(x => x.BookmarkId == id);
			if (bookmark != null)
			{
				try
				{
					_dbContext.Bookmarks.Remove(bookmark);
					await _dbContext.SaveChangesAsync();
				}
				catch (Exception ex)
				{
					throw;
				}

				return StatusCode(HttpStatusCode.NoContent);
			}
			else
			{
				return NotFound();
			}
		}
	}
}
