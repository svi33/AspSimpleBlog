using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Blog_CP_7.Models
{
    public class Post
    {
        public int Id { get; set; }
        [Display(Name = "Автор")]
        public string Author { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Длина {0} не менее{2}, не более{1}", MinimumLength = 5)]
        [Display(Name = "Заголовок")]
        public string Title { get; set; }

        [Display(Name = "Путь к изображению")]
        public string ImagePath { get; set; }
        public string Links { get; set; }
        public string Res { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Длина {0} не менее{2},", MinimumLength = 50)]
        [Display(Name = "Текст поста")]
        public string Content { get; set; }

        [Display(Name = "UrlSeo")]
        public string UrlSeo { get; set; }

        [DefaultValue(0)]
        public int NetLikeCount { get; set; }

        public DateTime? PostedOn { get; set; }
        public string UsId { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<TagMap> PostTags { get; set; }

        public Post()
        {
            Comments = new List<Comment>();
            PostTags = new List<TagMap>();
        }

    }

    public class Comment
    {
        public int Id { get; set; }

        public DateTime? DateTime { get; set; }
        public string UserName { get; set; }
        [Required]
        public string Body { get; set; }
        [DefaultValue(true)]
        public bool NetLikeCount { get; set; }

        
        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public Post Post { get; set; }
    }

    public class TagMap
    {
        public int Id { get; set; }

        public int PostId { get; set; }
       // [ForeignKey("PostId")]
       // public Post Post { get; set; }

        public int TagId { get; set; }
      //  [ForeignKey("TagId")]
       // public Tag Tag { get; set; }
    }

    public class Tag
    {
        public int Id { get; set; }
        public string UrlSlug { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<TagMap> PostTags { get; set; }
        public Tag()
        {
            PostTags = new List<TagMap>();
        }
    }
}