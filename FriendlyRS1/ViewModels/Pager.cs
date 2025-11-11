using System;

namespace FriendlyRS1.ViewModels
{
    public class Pager
    {
        public int TotalItems { get; private set; }
        public int CurrentPage { get; private set; }
        public int PageSize { get; private set; }
        public int TotalPages { get; private set; }
        public int StartPage { get; private set; }
        public int EndPage { get; private set; }

        public Pager(int totalItems, int currentPage, int pageSize = 10)
        {
            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;

            // calculate total pages
            TotalPages = (int)Math.Ceiling((decimal)TotalItems / PageSize);

            // calculate start and end pages for pager display
            StartPage = CurrentPage - 5;
            EndPage = CurrentPage + 4;

            if (StartPage <= 0)
            {
                EndPage = EndPage - (StartPage - 1);
                StartPage = 1;
            }

            if (EndPage > TotalPages)
            {
                EndPage = TotalPages;
                if (EndPage > 10)
                {
                    StartPage = EndPage - 9;
                }
            }
        }
    }
}
