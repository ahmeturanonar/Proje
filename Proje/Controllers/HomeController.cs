using PagedList;
using Proje.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Proje.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home

        [AllowAnonymous]
        public ActionResult Index(int? pagedNo)
        {
            int _PagedNo = pagedNo ?? 1;

            ProjeContext db = new ProjeContext();
            ViewModel viewModel = new ViewModel();
            viewModel.Posts = db.Post.OrderByDescending(x => x.PostId).ToPagedList(_PagedNo, 2);
            foreach (var item in viewModel.Posts)
            {

                var categoryName = db.Category.Where(x => x.CategoryId == item.CategoryId).FirstOrDefault();

                item.Category.CategoryName = categoryName.CategoryName;

                var comment = db.Comment.Where(x => x.PostId == item.PostId).ToList();

                item.Comment_Count = comment.Count();
            }

            viewModel.Announcement = db.Announcement.FirstOrDefault();

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                string user = HttpContext.User.Identity.Name;

                var cUser = db.User.Where(x => x.Username == user).FirstOrDefault();

                ViewBag.cRole = cUser.Role;


            }

            Setting setting = db.Settings.Where(x => x.Id == 1).FirstOrDefault();
            viewModel.Setting = setting;

            return View(viewModel);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Index(string cpname, string cpemail, string cpphone, string cpmessage)
        {
            ProjeContext db = new ProjeContext();
            Message message = new Message()
            {
                Name = cpname,
                Email = cpemail,
                Phone = cpphone,
                Content = cpmessage
            };

            db.Messages.Add(message);

            db.SaveChanges();

            return RedirectToAction("Index");
        }


        [AllowAnonymous]
        public ActionResult Login()
        {
            if (string.IsNullOrEmpty(HttpContext.User.Identity.Name))
            {
                FormsAuthentication.SignOut();
                return View();
            }
            else
            {
                ProjeContext db = new ProjeContext();
                var role = db.User.Where(x => x.Username == HttpContext.User.Identity.Name).FirstOrDefault();
                if (role.Role == "Admin")
                {
                    return RedirectToAction("Panel", "Home");
                }
                else if (role.Role == "Company")
                {
                    return RedirectToAction("CompanyPanel", "Home");
                }
                else if (role.Role == "Member")
                {
                    return RedirectToAction("Index", "Home");
                }
                return View();
            }


        }


        [HttpPost]
        [AllowAnonymous]

        public ActionResult Login(User user)
        {
            ProjeContext db = new ProjeContext();
            var u = db.User.FirstOrDefault(x => x.Username == user.Username && x.Password == user.Password);
            if (u != null)
            {
                FormsAuthentication.SetAuthCookie(u.Username, true);
                u.IsActive = true;
                db.SaveChanges();
                if (u.Role == "Admin")
                {
                    return RedirectToAction("Panel", "Home");
                }
                else if (u.Role == "Company")
                {
                    return RedirectToAction("CompanyPanel", "Home");
                }

                else if (string.IsNullOrEmpty(u.Role))
                {
                    return RedirectToAction("Index", "Home");
                }

            }
            else
            {
                ViewBag.LoginError = "Hatalı Kullanıcı Adı veya Şifre";
            }
            return View();
        }


        [Authorize]
        [LoginControl(Roles = "Admin")]
        public ActionResult Panel()
        {
            var model = new User();
            return View(model);

        }



        [Authorize]
        [LoginControl(Roles = "Company")]

        public ActionResult CompanyPanel()
        {
            var model = new User();
            return View(model);
        }

        public ActionResult Logout()
        {
            ProjeContext db = new ProjeContext();
            var current_user = db.User.Where(x => x.Username.Contains(HttpContext.User.Identity.Name)).FirstOrDefault();
            current_user.IsActive = false;
            db.SaveChanges();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            ProjeContext db = new ProjeContext();
            ViewModel viewModel = new ViewModel();
            viewModel.Setting = db.Settings.Where(x => x.Id == 1).FirstOrDefault();
            if (viewModel.Setting.AnyoneCanRegister == true)
            {
                return View();
            }
            else if (viewModel.Setting.AnyoneCanRegister == false)
            {
                return RedirectToAction("NotFound", "Error");
            }

            return View();

        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Register(User user, HttpPostedFileBase file)
        {
            ProjeContext db = new ProjeContext();

            Setting setting = db.Settings.Where(x => x.Id == 1).FirstOrDefault();

            var role = setting.NewUserDefaultRole;

            User rs = db.User.Where(x => x.Username.Contains(user.Username)).FirstOrDefault();
            if (rs == null)
            {
                if (file != null && file.ContentLength > 0)
                {
                    var newFileName = Guid.NewGuid() + System.IO.Path.GetExtension(file.FileName);
                    var serverPath = Server.MapPath("/ProfilesPhotos");
                    var path = Path.Combine(serverPath, newFileName).Replace("\\", "/");
                    file.SaveAs(path);
                    var userProfilePhotoPath = $@"/ProfilesPhotos/{newFileName}";
                    user.ProfilePhoto = userProfilePhotoPath;
                }
                user.Role = role;
                db.User.Add(user);
                db.SaveChanges();
                FormsAuthentication.SetAuthCookie(user.Username, false);
                return RedirectToAction("Index", "Home");


            }
            else
            {
                ViewBag.Result = "Bu kullanıcı adı daha önceden alınmış!";
                return View();
            }

        }


        [LoginControl(Roles = "Admin")]
        [Authorize]
        public ActionResult ListUsers()
        {
            ProjeContext db = new ProjeContext();
            List<User> user = db.User.ToList();
            return View(user);
        }

        [LoginControl(Roles = "Admin")]
        [Authorize]

        public ActionResult EditUsers(int? user_id)
        {
            if (user_id != 0)
            {
                ProjeContext db = new ProjeContext();
                User user = db.User.Where(x => x.UserId == user_id).FirstOrDefault();
                return View(user);

            }

            return RedirectToAction("NotFound", "Error");

        }


        [LoginControl(Roles = "Admin")]
        [Authorize]
        [HttpPost]

        public ActionResult EditUsers(int? user_id, User model, HttpPostedFileBase file)
        {
            if (user_id != 0 && model != null)
            {
                ProjeContext db = new ProjeContext();
                User user = db.User.Where(x => x.UserId == user_id).FirstOrDefault();
                user.Username = model.Username;
                user.Password = model.Password;
                user.Role = model.Role;
                if (file != null && file.ContentLength > 0)
                {
                    var profilePicturePath = Server.MapPath(user.ProfilePhoto);
                    if (System.IO.File.Exists(profilePicturePath))
                    {
                        System.IO.File.Delete(profilePicturePath);

                    }

                    var newFileName = Guid.NewGuid() + System.IO.Path.GetExtension(file.FileName);
                    var serverPath = Server.MapPath("/ProfilesPhotos");
                    var path = Path.Combine(serverPath, newFileName).Replace("\\", "/");
                    file.SaveAs(path);
                    var userProfilePhotoPath = $@"/ProfilesPhotos/{newFileName}";
                    user.ProfilePhoto = userProfilePhotoPath;
                }
                db.SaveChanges();
                return RedirectToAction("ListUsers", "Home");

            }

            return RedirectToAction("NotFound", "Error");

        }



        [LoginControl(Roles = "Admin")]
        [Authorize]
        [HttpGet]
        public ActionResult DeleteUser(int? user_id)
        {
            if (user_id != 0)
            {
                ProjeContext db = new ProjeContext();
                User user = db.User.Where(x => x.UserId == user_id).FirstOrDefault();
                return View(user);
            }
            return RedirectToAction("NotFound", "Error");
        }


        [LoginControl(Roles = "Admin")]
        [Authorize]
        [HttpPost, ActionName("DeleteUser")]

        public ActionResult DeleteUserOK(int? user_id)
        {

            if (user_id != 0)
            {
                ProjeContext db = new ProjeContext();
                User user = db.User.Where(x => x.UserId == user_id).FirstOrDefault();
                var profilePicturePath = Server.MapPath(user.ProfilePhoto);
                if (System.IO.File.Exists(profilePicturePath))
                {
                    System.IO.File.Delete(profilePicturePath);

                }
                db.User.Remove(user);
                db.SaveChanges();
                return RedirectToAction("ListUsers", "Home");
            }

            return RedirectToAction("Index", "Home");

        }


        [LoginControl(Roles = "Admin")]
        [Authorize]
        [HttpGet]
        public ActionResult ListCategories()
        {
            ProjeContext db = new ProjeContext();
            List<Category> categories = db.Category.ToList();
            if (categories != null)
            {
                return View(categories);
            }
            return View();

        }


        [LoginControl(Roles = "Admin")]
        [Authorize]
        [HttpGet]
        public ActionResult CreateCategory()
        {

            return View();
        }

        [LoginControl(Roles = "Admin")]
        [Authorize]
        [HttpPost]
        public ActionResult CreateCategory(Category category)
        {
            if (category != null)
            {
                ProjeContext db = new ProjeContext();
                db.Category.Add(category);
                db.SaveChanges();
                return RedirectToAction("ListCategories", "Home");
            }
            return RedirectToAction("NotFound", "Error");
        }



        [LoginControl(Roles = "Admin")]
        [Authorize]
        [HttpGet]
        public ActionResult EditCategory(int? category_id)
        {
            ProjeContext db = new ProjeContext();
            Category category = db.Category.Where(x => x.CategoryId == category_id).FirstOrDefault();
            if (category != null && category_id != 0)
            {
                return View(category);

            }
            return View();
        }


        [LoginControl(Roles = "Admin")]
        [Authorize]
        [HttpPost]
        public ActionResult EditCategory(int? category_id, Category model)
        {
            if (category_id != 0 && model != null)
            {
                ProjeContext db = new ProjeContext();
                Category category = db.Category.Where(x => x.CategoryId == category_id).FirstOrDefault();
                category.CategoryName = model.CategoryName;
                db.SaveChanges();
                return RedirectToAction("ListCategories", "Home");
            }
            return View();

        }


        [LoginControl(Roles = "Admin")]
        [Authorize]
        [HttpGet]
        public ActionResult DeleteCategory(int? category_id)
        {
            if (category_id != 0)
            {
                ProjeContext db = new ProjeContext();
                Category category = db.Category.Where(x => x.CategoryId == category_id).FirstOrDefault();
                return View(category);
            }
            return View();
        }


        [LoginControl(Roles = "Admin")]
        [Authorize]
        [HttpPost, ActionName("DeleteCategory")]
        public ActionResult DeleteCategoryOK(int? category_id)
        {
            if (category_id != 0)
            {
                ProjeContext db = new ProjeContext();
                Category category = db.Category.Where(x => x.CategoryId == category_id).FirstOrDefault();
                db.Category.Remove(category);
                db.SaveChanges();
                return RedirectToAction("ListCategories", "Home");
            }
            return View();

        }



        [LoginControl(Roles = "Admin")]
        [Authorize]
        [HttpGet]
        public ActionResult CreateUser()
        {
            List<SelectListItem> roles = new List<SelectListItem>()
            {
                new SelectListItem() {Text = "Admin"},
                new SelectListItem() {Text = "Company"}
            }.ToList();

            ViewBag.Roles = roles;

            return View();
        }

        [LoginControl(Roles = "Admin")]
        [Authorize]
        [HttpPost]
        public ActionResult CreateUser(User user, HttpPostedFileBase file)
        {
            if (user != null)
            {
                ProjeContext db = new ProjeContext();
                if (file != null && file.ContentLength > 0)
                {
                    var newFileName = Guid.NewGuid() + System.IO.Path.GetExtension(file.FileName);
                    var serverPath = Server.MapPath("/ProfilesPhotos");
                    var path = Path.Combine(serverPath, newFileName).Replace("\\", "/");
                    file.SaveAs(path);
                    var userProfilePhotoPath = $@"/ProfilesPhotos/{newFileName}";
                    user.ProfilePhoto = userProfilePhotoPath;
                }
                db.User.Add(user);
                db.SaveChanges();
                return RedirectToAction("ListUsers", "Home");

            }
            return View();
        }



        [LoginControl(Roles = "Admin,Company")]
        [Authorize]
        public ActionResult CreateArticle()
        {
            ProjeContext db = new ProjeContext();
            List<SelectListItem> Categories = (from categories in db.Category.ToList()
                                               select new SelectListItem()
                                               {

                                                   Text = categories.CategoryName,
                                                   Value = categories.CategoryId.ToString()


                                               }).ToList();
            ViewBag.Category = Categories;

            return View();
        }

        [LoginControl(Roles = "Admin,Company")]
        [Authorize]
        [HttpPost]
        public ActionResult CreateArticle(Post model, HttpPostedFileBase file)
        {
            if (model != null)
            {
                ProjeContext db = new ProjeContext();
                db.Post.Add(model);

                if (file != null && file.ContentLength > 0)
                {
                    var newFileName = Guid.NewGuid() + System.IO.Path.GetExtension(file.FileName);
                    var serverPath = Server.MapPath("/PostsPhotos");
                    var path = Path.Combine(serverPath, newFileName).Replace("\\", "/");
                    file.SaveAs(path);
                    var postPhotoPath = $@"/PostsPhotos/{newFileName}";
                    model.PostImage = postPhotoPath;
                }


                db.SaveChanges();
                return RedirectToAction("CreateArticle", "Home");

            }
            return View();

        }



        [Authorize]
        [LoginControl(Roles = "Admin,Company")]
        public ActionResult ListArticles(int? postId)
        {
            if (postId == null)
            {
                ProjeContext db = new ProjeContext();
                List<Post> post = db.Post.ToList();
                foreach (var item in post)
                {
                    var categoryName = db.Category.Where(x => x.CategoryId == item.CategoryId).FirstOrDefault();
                    item.Category.CategoryName = categoryName.CategoryName;
                }
                return View(post);
            }
            else
            {

                ProjeContext db = new ProjeContext();
                List<Post> post = db.Post.Where(x => x.PostId == postId).ToList();
                foreach (var item in post)
                {
                    var categoryName = db.Category.Where(x => x.CategoryId == item.CategoryId).FirstOrDefault();
                    item.Category.CategoryName = categoryName.CategoryName;
                    return View(post);
                }

            }

            return View();

        }

        [AllowAnonymous]
        public ActionResult ReadMore(int? post_id)
        {
            ProjeContext db = new ProjeContext();
            Post post = db.Post.Where(x => x.PostId == post_id).FirstOrDefault();
            post.TotalViews = post.TotalViews + 1;
            List<Comment> ct = db.Comment.Where(x => x.PostId == post_id).ToList();
            post.Comments = ct;
            db.SaveChanges();
            return View(post);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ReadMore(string txtTextArea, int post_id, string returnUrl)
        {
            if (!string.IsNullOrEmpty(txtTextArea))
            {
                ProjeContext db = new ProjeContext();
                Comment comment = new Comment();
                comment.CommentText = txtTextArea.ToString();
                comment.Username = HttpContext.User.Identity.Name;
                comment.PostId = post_id;
                comment.PublishDate = DateTime.Now;
                db.Comment.Add(comment);
                db.SaveChanges();
                return Redirect(returnUrl);
            }

            return View();
        }

        [Authorize]
        [LoginControl(Roles = "Admin,Company")]
        public ActionResult EditArticle(int? post_id)
        {
            if (post_id != 0)
            {
                ProjeContext db = new ProjeContext();
                Post post = db.Post.Where(x => x.PostId == post_id).FirstOrDefault();

                List<SelectListItem> Categories = (from categories in db.Category.ToList()
                                                   select new SelectListItem()
                                                   {

                                                       Text = categories.CategoryName,
                                                       Value = categories.CategoryId.ToString()


                                                   }).ToList();
                ViewBag.Category = Categories;

                return View(post);
            }
            return View();
        }


        [Authorize]
        [LoginControl(Roles = "Admin,Company")]
        [HttpPost]

        public ActionResult EditArticle(int? post_id, Post model, HttpPostedFileBase file)
        {
            if (post_id != 0)
            {
                ProjeContext db = new ProjeContext();
                Post post = db.Post.Where(x => x.PostId == post_id).FirstOrDefault();
                post.PostTitle = model.PostTitle;
                post.PostText = model.PostText;
                post.CategoryId = model.CategoryId;
                if (file != null && file.ContentLength > 0)
                {
                    var postPicturePath = Server.MapPath(post.PostImage);
                    if (System.IO.File.Exists(postPicturePath))
                    {
                        System.IO.File.Delete(postPicturePath);

                    }

                    var newFileName = Guid.NewGuid() + System.IO.Path.GetExtension(file.FileName);
                    var serverPath = Server.MapPath("/PostsPhotos");
                    var path = Path.Combine(serverPath, newFileName).Replace("\\", "/");
                    file.SaveAs(path);
                    var postPhotoPath = $@"/PostsPhotos/{newFileName}";
                    post.PostImage = postPhotoPath;
                }

                db.SaveChanges();
                return RedirectToAction("ListArticles", "Home");
            }
            return View();
        }



        [Authorize]
        [LoginControl(Roles = "Admin,Company")]
        public ActionResult DeleteArticle(int? post_id)
        {
            if (post_id != 0)
            {
                ProjeContext db = new ProjeContext();
                Post post = db.Post.Where(x => x.PostId == post_id).FirstOrDefault();
                var categoryName = db.Category.Where(x => x.CategoryId == post.CategoryId).FirstOrDefault();
                post.Category.CategoryName = categoryName.CategoryName;
                return View(post);
            }
            return View();
        }


        [Authorize]
        [LoginControl(Roles = "Admin,Company")]
        [HttpPost, ActionName("DeleteArticle")]
        public ActionResult DeleteArticleOK(int? post_id)
        {
            if (post_id != 0)
            {
                ProjeContext db = new ProjeContext();
                Post post = db.Post.Where(x => x.PostId == post_id).FirstOrDefault();
                var postPicturePath = Server.MapPath(post.PostImage);
                if (System.IO.File.Exists(postPicturePath))
                {
                    System.IO.File.Delete(postPicturePath);

                }
                db.Post.Remove(post);
                db.SaveChanges();
                return RedirectToAction("ListArticles", "Home");
            }
            return View();
        }

        [Authorize]
        [LoginControl(Roles = "Admin")]
        public ActionResult AnnouncementAdd()
        {
            return View();
        }


        [Authorize]
        [LoginControl(Roles = "Admin")]
        [HttpPost]
        public ActionResult AnnouncementAdd(Announcement model)
        {
            if (model != null)
            {
                ProjeContext db = new ProjeContext();
                db.Announcement.Add(model);
                db.SaveChanges();
                return RedirectToAction("ListsAnnouncement", "Home");
            }
            return View();
        }

        [Authorize]
        [LoginControl(Roles = "Admin")]
        public ActionResult ListsAnnouncement()
        {
            ProjeContext db = new ProjeContext();
            List<Announcement> announcement = db.Announcement.ToList();
            return View(announcement);
        }

        [Authorize]
        [LoginControl(Roles = "Admin")]
        public ActionResult EditAnnouncement(int announcement_id)
        {
            ProjeContext db = new ProjeContext();
            Announcement announcement = db.Announcement.Where(x => x.AnnouncementId == announcement_id).FirstOrDefault();
            return View(announcement);
        }


        [Authorize]
        [LoginControl(Roles = "Admin")]
        [HttpPost]
        public ActionResult EditAnnouncement(int announcement_id, Announcement model)
        {
            ProjeContext db = new ProjeContext();
            Announcement announcement = db.Announcement.Where(x => x.AnnouncementId == announcement_id).FirstOrDefault();
            announcement.AnnouncementText = model.AnnouncementText;
            db.SaveChanges();
            return RedirectToAction("ListsAnnouncement", "Home");
        }

        [Authorize]
        [LoginControl(Roles = "Admin")]
        public ActionResult DeleteAnnouncement(int announcement_id)
        {
            ProjeContext db = new ProjeContext();
            Announcement announcement = db.Announcement.Where(x => x.AnnouncementId == announcement_id).FirstOrDefault();
            return View(announcement);
        }


        [Authorize]
        [LoginControl(Roles = "Admin")]
        [HttpPost, ActionName("DeleteAnnouncement")]
        public ActionResult DeleteAnnouncementOK(int announcement_id)
        {
            ProjeContext db = new ProjeContext();
            Announcement announcement = db.Announcement.Where(x => x.AnnouncementId == announcement_id).FirstOrDefault();
            db.Announcement.Remove(announcement);
            db.SaveChanges();
            return RedirectToAction("ListsAnnouncement", "Home");
        }

        [Authorize]
        public ActionResult DeleteComment(string userName, string currentName, int? commentId, int? cId)
        {
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(currentName) && commentId > 0)
            {
                ProjeContext db = new ProjeContext();
                Comment comment = db.Comment.Where(x => x.Username == currentName && x.CommentId == commentId).FirstOrDefault();
                db.Comment.Remove(comment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            else if (cId != null)
            {
                ProjeContext db = new ProjeContext();
                Comment comment = db.Comment.Where(x => x.CommentId == cId).FirstOrDefault();
                db.Comment.Remove(comment);
                db.SaveChanges();
                return RedirectToAction("ListComments");
            }

            return RedirectToAction("NotFound");



        }

        [Authorize]
        [LoginControl(Roles = "Admin")]
        public ActionResult ListComments()
        {
            ProjeContext db = new ProjeContext();
            List<Comment> comments = db.Comment.ToList();
            return View(comments);
        }


        [Authorize]
        [LoginControl(Roles = "Admin")]

        public ActionResult EditComment(int? commentId)
        {
            if (commentId != null)
            {
                ProjeContext db = new ProjeContext();
                Comment comment = db.Comment.Where(x => x.CommentId == commentId).FirstOrDefault();
                return View(comment);
            }
            return RedirectToAction("NotFound");
        }

        [Authorize]
        [LoginControl(Roles = "Admin")]
        [HttpPost]
        public ActionResult EditComment(int? commentId, Comment model)
        {
            if (commentId != null)
            {
                ProjeContext db = new ProjeContext();
                Comment comment = db.Comment.Where(x => x.CommentId == commentId).FirstOrDefault();
                comment.CommentText = model.CommentText;
                comment.PublishDate = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("ListComments");
            }
            return RedirectToAction("NotFound");
        }


        [Authorize]
        [LoginControl(Roles = "Admin")]
        public ActionResult ListMessages()
        {
            ProjeContext db = new ProjeContext();
            List<Message> messages = db.Messages.ToList();
            return View(messages);
        }

        [Authorize]
        [LoginControl(Roles = "Admin")]

        public ActionResult DeleteMessage(int? messageId)
        {
            ProjeContext db = new ProjeContext();
            Message message = db.Messages.Where(x => x.Id == messageId).FirstOrDefault();
            db.Messages.Remove(message);
            db.SaveChanges();
            return RedirectToAction("ListMessages");
        }



        [Authorize]
        [LoginControl(Roles = "Admin")]
        public ActionResult CommentAdd()
        {
            ProjeContext db = new ProjeContext();

            List<SelectListItem> selectListItems = (from posts in db.Post.ToList()
                                                    select new SelectListItem()
                                                    {
                                                        Text = posts.PostTitle,
                                                        Value = posts.PostId.ToString()

                                                    }).ToList();


            List<SelectListItem> selectListItems1 = (from users in db.User.ToList()
                                                     select new SelectListItem()
                                                     {
                                                         Text = users.Username,
                                                         Value = users.Username


                                                     }).ToList();

            ViewBag.Posts = selectListItems;

            ViewBag.Users = selectListItems1;

            return View();
        }


        [Authorize]
        [LoginControl(Roles = "Admin")]
        [HttpPost]
        public ActionResult CommentAdd(Comment model, string User)
        {
            ProjeContext db = new ProjeContext();
            model.PublishDate = DateTime.Now;
            model.Username = User;
            db.Comment.Add(model);
            db.SaveChanges();
            return RedirectToAction("ListComments", "Home");
        }


        [Authorize]
        public ActionResult MyProfile()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                ProjeContext db = new ProjeContext();
                string cUser = HttpContext.User.Identity.Name;
                User user = db.User.Where(x => x.Username == cUser).FirstOrDefault();

                var userMessages = db.UserMessages.Where(x => x.UserId == user.UserId);

                ViewBag.MessageCount = userMessages.Count();

                return View(user);
            }
            return RedirectToAction("NotFound");

        }


        [Authorize]
        [HttpPost]
        public ActionResult MyProfile(HttpPostedFileBase file, User model)
        {
            ProjeContext db = new ProjeContext();

            User user = db.User.Where(x => x.Username == HttpContext.User.Identity.Name).FirstOrDefault();



            user.Password = model.Password;


            if (file != null && file.ContentLength > 0)
            {
                var newFileName = Guid.NewGuid() + System.IO.Path.GetExtension(file.FileName);
                var serverPath = Server.MapPath("/ProfilesPhotos");
                var path = Path.Combine(serverPath, newFileName).Replace("\\", "/");
                file.SaveAs(path);
                var userProfilePhotoPath = $@"/ProfilesPhotos/{newFileName}";

                user.ProfilePhoto = userProfilePhotoPath;

            }

            db.SaveChanges();

            return RedirectToAction("MyProfile");

        }


        [Authorize]
        [LoginControl(Roles = "Admin")]
        public ActionResult AddMessage()
        {
            ProjeContext db = new ProjeContext();
            var users = (from data in db.User.ToList()
                         select new SelectListItem()
                         {
                             Text = data.Username,
                             Value = data.UserId.ToString()

                         }).ToList();

            ViewBag.Users = users;

            return View();
        }


        [Authorize]
        [LoginControl(Roles = "Admin")]
        [HttpPost]
        public ActionResult AddMessage(UserMessage Model)
        {
            ProjeContext db = new ProjeContext();
            db.UserMessages.Add(Model);
            db.SaveChanges();
            return RedirectToAction("ListMessages", "Home");
        }



        [Authorize]
        [LoginControl(Roles = "Admin")]
        [HttpGet]
        public ActionResult SentMessages()
        {
            ProjeContext db = new ProjeContext();
            List<UserMessage> userMessages = db.UserMessages.ToList();
            foreach (var item in userMessages)
            {
                var incomingData = db.User.Where(x => x.UserId == item.UserId).FirstOrDefault();
                item.User.Username = incomingData.Username;
            }
            return View(userMessages);
        }


        [Authorize]
        public ActionResult MyProfileListMessages(int? id)
        {
            ProjeContext db = new ProjeContext();

            var incomingData = db.User.Where(x => x.UserId == id).FirstOrDefault();

            if (HttpContext.User.Identity.Name == incomingData.Username && id != null)
            {
                ViewBag.userName = incomingData.Username;


                List<UserMessage> userMessages = db.UserMessages.Where(x => x.UserId == id).ToList();

                return View(userMessages);
            }


            return RedirectToAction("NotFound", "Error");

        }


        [Authorize]
        public ActionResult MyProfileDeleteMessage(int? messageId)
        {
            ProjeContext db = new ProjeContext();
            UserMessage userMessage = db.UserMessages.Where(x => x.Id == messageId).FirstOrDefault();
            db.UserMessages.Remove(userMessage);
            db.SaveChanges();
            var data = db.User.Where(x => x.UserId == userMessage.UserId).FirstOrDefault();
            int? id = data.UserId;

            return RedirectToAction("MyProfileListMessages", new { id });
        }


        [Authorize]
        [LoginControl(Roles = "Admin")]
        public ActionResult Setting()
        {
            ProjeContext db = new ProjeContext();

            Setting setting = db.Settings.Where(x => x.Id == 1).FirstOrDefault();



            List<SelectListItem> selectListItem = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Text = "Admin",
                    Value = "Admin"
                },

                new SelectListItem()
                {
                    Text = "Company",
                    Value = "Company"
                },

                new SelectListItem()
                {
                    Text = "Member",
                    Value = "Member"

                }

            }.ToList();


            ViewBag.Roles = selectListItem;

            return View(setting);
        }



        [Authorize]
        [LoginControl(Roles = "Admin")]
        [HttpPost]
        public ActionResult Setting(Setting Model)
        {
            ProjeContext db = new ProjeContext();

            Setting setting = db.Settings.Where(x => x.Id == 1).FirstOrDefault();

            setting.SiteTitle = Model.SiteTitle;

            setting.TagLine = Model.TagLine;

            setting.AnyoneCanRegister = Model.AnyoneCanRegister;

            setting.NewUserDefaultRole = Model.NewUserDefaultRole;

            db.SaveChanges();

            return RedirectToAction("Setting", "Home");
        }


        [Authorize]

        public ActionResult Chat(string uName)
        {
            ProjeContext db = new ProjeContext();

            ChatViewModel chatViewModel = new ChatViewModel();

            if (string.IsNullOrEmpty(uName))
            {
                chatViewModel.Users = db.User.ToList();

                return View(chatViewModel);
            }

            else
            {
                chatViewModel.Users = db.User.ToList();

                chatViewModel.User = db.User.Where(x => x.Username == uName).FirstOrDefault();

                return View(chatViewModel);
            }
           

        }
    }
}