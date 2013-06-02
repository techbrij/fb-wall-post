using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WallPostByTechBrij.Filters;
using WallPostByTechBrij.Models;
using WebMatrix.WebData;

namespace WallPostByTechBrij.Controllers
{
    [Authorize]
    public class WallPostController : ApiController
    {
        private string imgFolder = "/Images/profileimages/";
        private string defaultAvatar = "user.png";
        private WallEntities db = new WallEntities();

        // GET api/WallPost
        public dynamic GetPosts()
        {
           
            var ret = (from post in db.Posts.ToList() 
                      orderby post.PostedDate descending
                      select new
                      {
                          Message = post.Message,
                          PostedBy = post.PostedBy,
                          PostedByName = post.UserProfile.UserName,
                          PostedByAvatar =imgFolder +(String.IsNullOrEmpty(post.UserProfile.AvatarExt) ? defaultAvatar : post.PostedBy + "." + post.UserProfile.AvatarExt), 
                          PostedDate = post.PostedDate,
                          PostId = post.PostId,
                          PostComments = from comment in post.PostComments.ToList() 
                                         orderby comment.CommentedDate
                                         select new
                                         {
                                             CommentedBy = comment.CommentedBy,
                                             CommentedByName = comment.UserProfile.UserName,
                                             CommentedByAvatar = imgFolder +(String.IsNullOrEmpty(comment.UserProfile.AvatarExt) ? defaultAvatar :  comment.CommentedBy + "." + comment.UserProfile.AvatarExt), 
                                             CommentedDate = comment.CommentedDate,
                                             CommentId = comment.CommentId,
                                             Message = comment.Message,
                                             PostId = comment.PostId

                                         }
                      }).AsEnumerable();
            return ret;
        }

        // GET api/WallPost/5
        public Post GetPost(int id)
        {
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return post;
        }

        // PUT api/WallPost/5
        public HttpResponseMessage PutPost(int id, Post post)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != post.PostId)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(post).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/WallPost
        public HttpResponseMessage PostPost(Post post)
        {
            post.PostedBy = WebSecurity.CurrentUserId;
            post.PostedDate = DateTime.UtcNow;
            ModelState.Remove("post.PostedBy");
            ModelState.Remove("post.PostedDate");

            if (ModelState.IsValid)
            {
                db.Posts.Add(post);
                db.SaveChanges();
                var usr = db.UserProfiles.FirstOrDefault(x => x.UserId == post.PostedBy);
                var ret = new
                      {
                          Message = post.Message,
                          PostedBy = post.PostedBy,
                          PostedByName = usr.UserName,
                          PostedByAvatar = imgFolder +(String.IsNullOrEmpty(usr.AvatarExt) ? defaultAvatar : post.PostedBy + "." + post.UserProfile.AvatarExt),
                          PostedDate = post.PostedDate,
                          PostId = post.PostId
                      };
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, ret);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = post.PostId }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/WallPost/5
        public HttpResponseMessage DeletePost(int id)
        {
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Posts.Remove(post);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, post);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}