using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataAccess;
using Repositories;

namespace BusinessLogic.PostService
{
    /// <summary>
    /// PostService
    /// </summary>
    public class PostService
    {
        /// <summary>
        /// Words
        /// </summary>
        public string[] Words;

        /// <summary>
        /// PostFolderPath
        /// </summary>
        public string PostFolderPath;

        /// <summary>
        /// MinWordCount
        /// </summary>
        public int MinWordCount;

        /// <summary>
        /// MaxWordCount
        /// </summary>
        public int MaxWordCount;

        /// <summary>
        /// Random
        /// </summary>
        public Random Random;

        /// <summary>
        /// FuzzySearchDBRepository
        /// </summary>
        public FuzzySearchDBRepository repo;

        /// <summary>
        /// PostService
        /// </summary>
        public PostService()
        {
            this.Words = this.SetWords();
            this.PostFolderPath = Path.Combine(@"D:\Files\FuzzySearch", DateTime.Now.ToString("yyyyMMdd"));
            this.MinWordCount = 2;
            this.MaxWordCount = 10;
            this.Random = new Random();
            this.repo = new FuzzySearchDBRepository();
        }

        /// <summary>
        /// 文本分析
        /// </summary>
        /// <param name="postIds"></param>
        public void ProcessPosts(IEnumerable<long> postIds)
        {
            foreach (var postId in postIds)
            {
                Console.WriteLine($"Now PostId:{postId}");

                string contents = this.GetPostContent(postId);

                //// 斷詞
                var list = contents.Split(' ').ToList();
                list.RemoveAt(list.Count - 1);

                //// 去掉大小寫之分
                list.ForEach(i => i.ToLower());

                //// 寫進 DB
                var groups = list.GroupBy(x => x).Select(y => new { Term = y.Key, Freq = y.Count() });
                foreach (var group in groups)
                {
                    var tagId = this.InsertOrUpdateTag(group.Term, group.Freq);
                    this.InsertTagPost(tagId, postId, group.Freq);
                }
            }
        }

        /// <summary>
        /// 隨機產生文本
        /// </summary>
        /// <param name="postCount"></param>
        /// <returns></returns>
        public IEnumerable<long> RandomGeneratePosts(int postCount)
        {
            var result = new List<long>();

            var now = DateTime.Now;
            string folderPath = this.CreateDirectory();

            for (var i = 1; i <= postCount; i++)
            {
                var fileName = now.ToString("HHmmss_") + i + ".txt";
                var post = this.GetRandomPost();

                using (StreamWriter sw = new StreamWriter(Path.Combine(folderPath, fileName)))
                {
                    sw.WriteLine(post);
                }

                //// 寫進 DB
                var wordsCount = post.Split(' ').Count();
                var postId = this.InsertPost(fileName, wordsCount);
                result.Add(postId);
            }

            return result;
        }

        /// <summary>
        /// GetTagPosts
        /// </summary>
        /// <param name="tagIds">tagIds</param>
        /// <returns>TagPosts</returns>
        public IEnumerable<TagPost> GetTagPosts(IEnumerable<long> tagIds)
        {
            var result = this.repo.GetTagPosts(tagIds);

            return result;
        }

        /// <summary>
        /// GetPostNames
        /// </summary>
        /// <param name="postIds">postIds</param>
        /// <returns>PostNames</returns>
        public IEnumerable<string> GetPostNames(IEnumerable<long> postIds)
        {
            var result = this.repo.GetPostNames(postIds);

            return result;
        }

        /// <summary>
        /// InsertOrUpdateTag
        /// </summary>
        /// <param name="term"></param>
        /// <param name="freq"></param>
        /// <returns></returns>
        private long InsertOrUpdateTag(string term, int freq)
        {
            var originTag = this.repo.GetTag(term);

            long tagId;
            Tag tag;

            //// 若 DB 存在該關鍵字，則更新
            if (originTag != null)
            {
                originTag.Tag_Frequency = originTag.Tag_Frequency + freq;

                tagId = this.repo.UpdateTag(originTag);
            }
            //// 若 DB 不存在該關鍵字，則新增
            else
            {
                tag = new Tag
                {
                    Tag_Keyword = term.ToLower(),
                    Tag_Frequency = freq
                };

                tagId = this.repo.InsertTag(tag);
            }

            return tagId;
        }

        /// <summary>
        /// GetPostContent
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        private string GetPostContent(long postId)
        {
            var result = string.Empty;
            
            var fileName = this.repo.GetPost(postId).Post_Title;
            var filePath = Path.Combine(PostFolderPath, fileName);

            using(StreamReader sr = new StreamReader(filePath))
            {
                result = sr.ReadToEnd();
            }

            return result;
        }

        /// <summary>
        /// InsertPost
        /// </summary>
        /// <param name="title"></param>
        /// <param name="wordsCount"></param>
        /// <returns></returns>
        private long InsertPost(string title, int wordsCount)
        {
            var post = new Post()
            {
                Post_PersonId = this.Random.Next(10),
                Post_Title = title,
                Post_WordsCount = wordsCount,
                Post_UpdatedDateTime = DateTime.Now
            };

            var postId = this.repo.InsertPost(post);

            return postId;
        }

        /// <summary>
        /// InsertTagPost
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="postId"></param>
        /// <param name="freq"></param>
        private void InsertTagPost(long tagId, long postId, int freq)
        {
            var tagPost = new TagPost()
            {
                TagPost_PostId = postId,
                TagPost_TagId = tagId,
                TagPost_Frequency = freq
            };

            var tagPostId = this.repo.InsertTagPost(tagPost);
        }

        /// <summary>
        /// CreateDirectory
        /// </summary>
        /// <returns></returns>
        private string CreateDirectory()
        {
            Directory.CreateDirectory(this.PostFolderPath);
            return this.PostFolderPath;
        }

        /// <summary>
        /// SetWords
        /// </summary>
        /// <returns></returns>
        private string[] SetWords()
        {
            return new string[]
                { 
                    "apple",
                    "book",
                    "acat", // 這三個會被搜尋到
                    "cat",  // 這三個會被搜尋到
                    "cad",  // 這三個會被搜尋到
                    "dog",
                    "egg",
                    "fish",
                    "gym",
                    "house",
                    "illness",
                    "joy",
                    "key",
                    "lion",
                    "monkey",
                    "nose",
                    "operation",
                    "piano",
                    "question",
                    "rabbit",
                    "supermarket",
                    "teacher",
                    "user",
                    "video",
                    "window",
                    "xray",
                    "yellow",
                    "zip"
                };

            //string FileToRead = @"D:\Files\FuzzySearch\words.txt";
            //// Creating enumerable object  
            //IEnumerable<string> line = File.ReadLines(FileToRead);

            //return line.ToArray();
        }

        /// <summary>
        /// GetRandomPost
        /// </summary>
        /// <returns></returns>
        private string GetRandomPost()
        {
            var wordList = new List<string>();
            var wordsCount = this.Words.Count();

            //// 隨機決定該貼文有幾個字
            var count = this.Random.Next(MinWordCount, this.MaxWordCount);

            for (var i = 1; i <= count; i++)
            {
                var index = this.Random.Next(wordsCount);
                var word = this.Words[index];

                wordList.Add(word);
            }

            var result = string.Join(" ", wordList);

            return result;
        }
    }
}
