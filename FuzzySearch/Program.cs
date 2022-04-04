using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.FuzzySearchService;
using BusinessLogic.PostService;

namespace FuzzySearch
{
    class Program
    {
        static void Main(string[] args)
        {
            var postService = new PostService();
            var fuzzySearchService = new FuzzySearchService();

            Console.WriteLine("是否要產假貼文?");
            var needToGeneratePosts = Convert.ToBoolean(Console.ReadLine());
            
            if (needToGeneratePosts)
            {
                Console.WriteLine("要產幾篇?");                
                var count = Convert.ToInt32(Console.ReadLine());

                var postIds = postService.RandomGeneratePosts(count);
                postService.ProcessPosts(postIds);
            }

            Console.WriteLine("請輸入關鍵字:");
            var keyword = Console.ReadLine();

            //// 找出相似的關鍵字
            var keywords = fuzzySearchService.FindSimilarKeywords(keyword.ToLower());

            //// 找出貼文
            var posts = postService.GetTagPosts(keywords.Keys);

            var r = fuzzySearchService.Calculate(keywords, posts);
        }
    }
}
