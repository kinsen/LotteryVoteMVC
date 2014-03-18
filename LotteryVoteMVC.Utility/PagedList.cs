using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Utility
{
    /// <summary>
    /// 分页容器类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedList<T> : IEnumerable<T>
    {
        #region Properties
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public bool HasPreviousPage
        {
            get
            {
                return PageIndex > 1;
            }
        }
        public bool HasNextPage
        {
            get
            {
                return PageIndex * PageSize < TotalCount;
            }
        }
        public IEnumerable<T> Items { get; set; }
        #endregion
        public PagedList(IEnumerable<T> source, int index, int pageSize, int totalCount)
        {
            this.Items = source;
            this.PageIndex = index;
            this.PageSize = pageSize;
            this.TotalCount = totalCount;
        }
        public static PagedList<T> Empty = new PagedList<T>(Enumerable.Empty<T>(), 1, 1, 0);
        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
