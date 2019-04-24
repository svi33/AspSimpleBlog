using Blog_CP_7.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Blog_CP_7.Controllers
{
    public class BlogController : Controller
    {
        ApplicationContext db = new ApplicationContext();
        public ActionResult Index()
        {
            // стартовый вывод постов
            var Allblog = db.Blogs.OrderByDescending(u => u.PostedOn);
            ViewBag.Com_t = db.Comments.ToList();
            ViewBag.AllTags = db.Tags.ToList();
            return View(Allblog);
        }

        [Authorize]
        public ActionResult Create()
        {
            // создание поста
            ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            ViewBag.Author = "";
            if (user != null) ViewBag.Author = user.UserName;
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult Create(Post bl)
        {
            //запись поста в базу
            if (ModelState.IsValid)
            {
                ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
                bl.PostedOn = DateTime.Now.Date;
                bl.UsId = user.Id;
                if (bl.Author == null || bl.Author == "") bl.Author = "Unknown";
                db.Blogs.Add(bl);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }

        
        public ActionResult Generic()
        {
            //вывод поста с коментариями, админ и автор могут редактировать
            Post blog = db.Blogs.Find(Convert.ToInt32(Url.RequestContext.RouteData.Values["id"]));
            ViewBag.Com_t = db.Comments.Where(t=>t.PostId==blog.Id).ToList();
            ViewBag.Kom_r = "Anonimus";
            ViewBag.UsIdr = -1;
            ViewBag.Likes = true;
            ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            if (user != null)
            {
                ViewBag.Kom_r = user.UserName;
                if (blog.UsId == user.Id) ViewBag.UsIdr = 0;
                if (User.IsInRole("admin")) ViewBag.UsIdr = 1;
            }
            var MT = db.TagMaps.Where(u => u.PostId == blog.Id).ToList();
            string TagsPost = "";
            if (MT != null)
            {
                foreach (TagMap i in MT)
                {
                    Tag blad = db.Tags.Where(u => u.Id == i.TagId).First();

                    //string temp =db.Tags.Where(u => u.Id == i.TagId).Select(x => x.Name).First();
                    TagsPost += "   #" + blad.Name;
                }
            }
            ViewBag.TagP = TagsPost;
            return View(blog);
        }

        [HttpPost]
        public ActionResult Generic(Comment com)
        { 
            if (com == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(com.Body))
            {
                ModelState.AddModelError("Body", "Некорректный коментарий");
            }
            else if (com.Body.Length < 12)
            {
                ModelState.AddModelError("Body", "Недопустимая длина строки");
            }

            com.Post = db.Blogs.Find(com.PostId);
            // dopil 

            if (ModelState.IsValid)
            {
                Comment pr = new Comment { UserName = com.UserName, NetLikeCount=true, DateTime = DateTime.Now.Date, Body=com.Body, PostId=com.PostId, Post=db.Blogs.Find(com.PostId) };
                db.Comments.Add(pr);
                db.SaveChanges();
                return RedirectToAction("Generic", "Blog", new { @id = com.PostId });
            }
            ViewBag.Message = "Запрос не прошел валидацию";
            return RedirectToAction("Generic", "Blog", new { @id = com.PostId });
        }
        
        public PartialViewResult Likes(int id)
        {
            Post post = db.Blogs.Find(id); 

            if (post != null)
            {
                post.NetLikeCount++;
                db.SaveChanges();
            }
            return PartialView();
        }

        
        public PartialViewResult UnLikes(int id)
        {
            Post post = db.Blogs.Find(id);
            if (post != null)
            {
                //if (post.NetLikeCount>0) post.NetLikeCount--;
                post.NetLikeCount--;
                db.SaveChanges();
            }
            return PartialView();
        }

        [Authorize]
        public ActionResult EditPost(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post prodacts = db.Blogs.Find(id);

            if (prodacts == null)
            {
                return HttpNotFound();
            }
            ViewBag.PI = id;
            var MT = db.TagMaps.Where(u => u.PostId == id).ToList();
            string TagsPost = "";
            if (MT != null) { 
            foreach (TagMap i in MT)
            {
                    Tag blad = db.Tags.Where(u => u.Id == i.TagId).First();
                    
                //string temp =db.Tags.Where(u => u.Id == i.TagId).Select(x => x.Name).First();
                    TagsPost += "   #" + blad.Name;
            }
            }
            ViewBag.TagP = TagsPost;
            return View(prodacts);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(Post PM)
        {
            // Добавленно по Рекомендациям *отредактировано модером
            if (PM == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(PM.Author))
            {
                ModelState.AddModelError("Name", "Некорректное имя");
            }
            else if (PM.Content.Length < 2)
            {
                ModelState.AddModelError("Content", "Недопустимая длина строки");
            }

            if (ModelState.IsValid)
            {
                ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
               //if (User.IsInRole("admin"))
                if (user.UserName == "admin") PM.Content = PM.Content + " \n *отредактированно модератором*";
                db.Entry(PM).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Generic", "Blog", new { @id = PM.Id });
            }
            ViewBag.Message = "Запрос не прошел валидацию";
            return View(PM);
        }

        [Authorize]
        public ActionResult DelPost(int id)
        {
            Post b = new Post { Id = id };
            db.Entry(b).State = EntityState.Deleted;
            db.SaveChanges();
            return RedirectToAction("Index", "Blog");
        }

        [Authorize]
        public ActionResult DelComent(int id, int PI)
        {
            Comment b = db.Comments.Where(u=>u.PostId==PI&& u.Id==id).FirstOrDefault();
            ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            if (b==null) return RedirectToAction("Generic", "Blog", new { @id = PI });
            if (user.UserName == "admin") b.Body="*удалено модератором*";
            else b.Body = "*удалено автором поста*";
            //db.Entry(b).State = EntityState.Deleted;
            db.SaveChanges();
            return RedirectToAction("Generic", "Blog", new { @id = PI });
        }

        [HttpPost]
        public ActionResult Index (string param)
        {
            var s1 = db.Blogs.Where(u=>u.Title.Contains(param));
            var s2 = db.Blogs.Where(u => u.Author.Contains(param));
            var s3 = db.Blogs.Where(u => u.Content.Contains(param));
            var P_log = s1.Union(s2);
            P_log = P_log.Union(s3);
            ViewBag.Com_t = db.Comments.ToList();
            ViewBag.AllTags = db.Tags.ToList();
            return View(P_log);
        }


        public ActionResult Upload()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase upload)
        {
            int MyId = (int)TempData["prod"];
            if (upload != null)
            {
                string fileName = System.IO.Path.GetFileName(upload.FileName);
                string dir = Directory.CreateDirectory(Server.MapPath("~/foto/" + MyId.ToString() + "/" + fileName)).FullName;
                upload.SaveAs(dir + "/" + fileName);
                Post prodacts = db.Blogs.Find(MyId);

                prodacts.ImagePath = ("~/foto/" + MyId.ToString() + "/" + fileName + "/" + fileName);
                db.SaveChanges();


                return RedirectToAction("ImageAdd", new { id = MyId });
            }
            else return RedirectToAction("Index");
        }

        public ActionResult ImageAdd(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post prodacts = db.Blogs.Find(id);
            if (prodacts == null)
            {
                return HttpNotFound();
            }
            TempData["prod"] = id;
            return View(prodacts);
        }

        public ActionResult ImageDel(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post prodacts = db.Blogs.Find(id);
            if (prodacts == null)
            {
                return HttpNotFound();
            }
            if (prodacts.ImagePath == "") return RedirectToAction("EditPost", "Blog", id );
            string dir = prodacts.ImagePath;
            FileInfo fileInf = new FileInfo(dir);
            if (fileInf.Exists)
            {
                fileInf.Delete();
            }
            prodacts.ImagePath = "";
            db.Entry(prodacts).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("EditPost", "Blog", id); 
        }

        public ActionResult AutPost (string autor)
        {
            var P_log = db.Blogs.Where(u => u.Author.Contains(autor));
            TempData["PostData"] = P_log;
            return RedirectToAction("PostNeed", "Blog");
        }

        public ActionResult SortLike ()
        {
            var P_log = db.Blogs.OrderByDescending(u => u.NetLikeCount); 
            TempData["PostData"] = P_log;
            return RedirectToAction("PostNeed", "Blog");
        }

        public ActionResult SortComent()
        {
            var P_log = db.Blogs.OrderByDescending(u => u.Comments.Count);
            TempData["PostData"] = P_log;
            return RedirectToAction("PostNeed", "Blog");
        }

        public ActionResult SortPostTag(string tag)
        {
            Tag first = db.Tags.Where(u => u.Name == tag).FirstOrDefault();
            if (first == null)
            { return HttpNotFound(); }
            var sec = db.TagMaps.Where(u => u.TagId == first.Id);
            if (sec == null)
            { return HttpNotFound(); }



            var result = db.Blogs.Join(sec, // второй набор
             p => p.Id, // свойство-селектор объекта из первого набора
             t => t.PostId, // свойство-селектор объекта из второго набора
             (p, t) => new  Post());




            List<Post> thir = new List<Post>();
            var Allblog = db.Blogs.ToList();

            foreach (TagMap tm in sec)
            {
                Post temp = Allblog.Where(u => u.Id==tm.PostId).First();
                if (temp != null) thir.Add(temp);
            }
            var P_log = thir;
            TempData["PostData"] = P_log;
            return RedirectToAction("PostNeed", "Blog");
        }

        public ActionResult PostNeed ()
        {
            var viewpost = TempData["PostData"];
            ViewBag.Com_t = db.Comments.ToList();
            ViewBag.AllTags = db.Tags.ToList();
            return View(viewpost);
        }

        public ActionResult TagAdd(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Blogs.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            SelectList tags = new SelectList(db.Tags, "Id", "Name");
            ViewBag.AllTags = tags;

            return View(post);
        }

        [HttpPost]
        //public string TagAdd(int pid, int selTag)
        public ActionResult TagAdd(int pid, int[] selTag)
        {

           var MT = db.TagMaps.Where(u => u.PostId == pid).ToList();
           if (MT!=null)
           {
               foreach (TagMap II in MT)
                   db.Entry(II).State = EntityState.Deleted;

                db.SaveChanges();
            }

            if (selTag != null)
            {

                //получаем выбранные tag
                foreach (int i in selTag)
                {
                    TagMap newTM = new TagMap();
                    newTM.PostId = pid; newTM.TagId = i;
                    db.TagMaps.Add(newTM);
                }
                db.SaveChanges();
            }


            return RedirectToAction("EditPost", "Blog", new { @id = pid });

        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

    }
}

//&& blog.PostTags.Count>0