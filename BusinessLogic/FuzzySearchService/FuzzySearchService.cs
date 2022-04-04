using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess;
using Repositories;

namespace BusinessLogic.FuzzySearchService
{
    /// <summary>
    /// FuzzySearchService
    /// </summary>
    public class FuzzySearchService
    {
        /// <summary>
        /// FuzzySearchDBRepository
        /// </summary>
        public FuzzySearchDBRepository repo;

        /// <summary>
        /// Initializes a new instance of the <see cref="FuzzySearchService"/> class.
        /// </summary>
        public FuzzySearchService()
        {
            this.repo = new FuzzySearchDBRepository();
        }

        /// <summary>
        /// 比較關鍵字
        /// </summary>
        /// <param name="keyword">關鍵字</param>
        /// <returns></returns>
        public Dictionary<long, int> FindSimilarKeywords(string keyword)
        {
            var tags = this.repo.GetTags();
            var tagDistanceDict = new Dictionary<long, int>();

            foreach (var tag in tags)
            {
                var distance = this.MinDistance(keyword, tag.Tag_Keyword);

                if (distance <= 2)
                {
                    tagDistanceDict.Add(tag.Tag_Id, distance);
                }
            }

            return tagDistanceDict;
        }

        /// <summary>
        /// 計算分數
        /// </summary>
        /// <param name="tagDistanceDict">Tag</param>
        /// <param name="tagPosts">Posts</param>
        /// <returns></returns>
        public List<IGrouping<long, Entity>> Calculate(Dictionary<long, int> tagDistanceDict, IEnumerable<TagPost> tagPosts)
        {
            var score = this.GetTfIdfScore(tagPosts, tagDistanceDict)
                            .GroupBy(i => i.TagId).ToList();

            score.ForEach(i => i.OrderBy(x => x.TagDistance).ThenByDescending(x => x.TfIdf));

            return score;
        }

        /// <summary>
        /// 取得分數
        /// </summary>
        /// <param name="tagPosts">有該關鍵字的貼文</param>
        /// <param name="tagDistance">關鍵字-距離</param>
        /// <returns>計算結果</returns>
        public IEnumerable<Entity> GetTfIdfScore(IEnumerable<TagPost> tagPosts, Dictionary<long, int> tagDistance)
        {
            var result = new List<Entity>();

            var tagIds = tagPosts.Select(i => i.TagPost_TagId).Distinct();
            var postIds = tagPosts.Select(i => i.TagPost_PostId).Distinct();
            var totalCount = tagPosts.Count();

            foreach (var tagId in tagIds)
            {
                var count = tagPosts.Where(i => i.TagPost_TagId == tagId).Count();
                var idf = Math.Log10(totalCount / count);

                foreach (var postId in postIds)
                {
                    var wordsCount = this.repo.GetPost(postId).Post_WordsCount;
                    var tagPost = tagPosts.Where(i => i.TagPost_TagId == tagId && i.TagPost_PostId == postId)
                                          .FirstOrDefault();

                    if (tagPost != null)
                    {
                        var tf = (double)tagPost.TagPost_Frequency / (double)wordsCount;
                        var tfIdf = tf * idf;

                        result.Add(new Entity()
                        {
                            TagId = tagId,
                            TagDistance = tagDistance[tagId],
                            PostId = postId,
                            TfIdf = tfIdf
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Levenshtein Distance
        /// </summary>
        /// <param name="word1">The word1.</param>
        /// <param name="word2">The word2.</param>
        /// <returns>Distance</returns>
        private int MinDistance(string word1, string word2)
        {
            var n1 = word1.Length;
            var n2 = word2.Length;

            var dp = new int[n1 + 1, n2 + 1];

            for (int i = 0; i < n1 + 1; i++)
            {
                for (int j = 0; j < n2 + 1; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        dp[i, j] = 0;
                    }
                    else if (i == 0)
                    {
                        dp[i, j] = j;
                    }
                    else if (j == 0)
                    {
                        dp[i, j] = i;
                    }
                    else
                    {
                        var word1Index = i - 1;
                        var word2Index = j - 1;

                        if (word1[word1Index] == word2[word2Index])
                        {
                            dp[i, j] = dp[i - 1, j - 1];
                        }
                        else
                        {
                            // 1. change one letter
                            // 2. remove one letter from word2
                            // 3. add one letter to word2
                            dp[i, j] = Math.Min(dp[i - 1, j - 1], Math.Min(dp[i, j - 1], dp[i - 1, j])) + 1;
                        }
                    }
                }
            }

            return dp[n1, n2];
        }

        public class Entity
        {
            public long TagId { get; set; }

            public long PostId { get; set; }

            public int TagDistance { get; set; }

            public double TfIdf { get; set; }
        }
    }
}
