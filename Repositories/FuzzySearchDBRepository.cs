using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DataAccess;

namespace Repositories
{
    /// <summary>
    /// FuzzySearchDBRepository
    /// </summary>
    public class FuzzySearchDBRepository
    {
        /// <summary>
        /// FuzzySearchDBEntities1
        /// </summary>
        FuzzySearchDBEntities1 DbContext;

        /// <summary>
        /// FuzzySearchDBRepository
        /// </summary>
        public FuzzySearchDBRepository()
        {
            this.DbContext = new FuzzySearchDBEntities1();
        }

        /// <summary>
        /// GetPost
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public Post GetPost(long postId)
        {
            var result = from post in this.DbContext.Post
                         where post.Post_Id == postId
                         select post;

            return result.FirstOrDefault();
        }

        public IEnumerable<string> GetPostNames(IEnumerable<long> postIds)
        {
            var result = from post in this.DbContext.Post
                         where postIds.Contains(post.Post_Id)
                         select post.Post_Title;

            return result;
        }

        /// <summary>
        /// InsertPost
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public long InsertPost(Post post)
        {
            this.DbContext.Post.Add(post);
            this.DbContext.SaveChanges();

            return post.Post_Id;
        }

        /// <summary>
        /// GetTag
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public Tag GetTag(string keyword)
        {
            var result = from tag in this.DbContext.Tag
                         where tag.Tag_Keyword == keyword
                         select tag;

            return result.FirstOrDefault();
        }

        /// <summary>
        /// GetTags
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tag> GetTags()
        {
            var result = from tag in this.DbContext.Tag
                         select tag;

            return result.ToList();
        }

        /// <summary>
        /// InsertTag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public long InsertTag(Tag tag)
        {
            this.DbContext.Tag.Add(tag);
            this.DbContext.SaveChanges();

            return tag.Tag_Id;
        }

        /// <summary>
        /// UpdateTag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public long UpdateTag(Tag tag)
        {
            this.DbContext.Tag.Attach(tag);
            this.DbContext.Entry(tag).State = EntityState.Modified;
            this.DbContext.SaveChanges();

            return tag.Tag_Id;
        }

        /// <summary>
        /// GetTagPosts
        /// </summary>
        /// <param name="tagIds"></param>
        /// <returns></returns>
        public IEnumerable<TagPost> GetTagPosts(IEnumerable<long> tagIds)
        {
            var result = from tagPost in this.DbContext.TagPost
                         where tagIds.Contains(tagPost.TagPost_TagId)
                         select tagPost;

            return result.ToList();
        }

        /// <summary>
        /// InsertTagPost
        /// </summary>
        /// <param name="tagPost"></param>
        /// <returns></returns>
        public long InsertTagPost(TagPost tagPost)
        {
            this.DbContext.TagPost.Add(tagPost);
            this.DbContext.SaveChanges();

            return tagPost.TagPost_Id;
        }
    }
}
