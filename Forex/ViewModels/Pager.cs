namespace Forex.ViewModels
{
    public class Pager : NotifyPropertyChanged
    {
        #region Notify Properties

        private int _currentPage;
        public int CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                if (value == _currentPage)
                {
                    return;
                }

                _currentPage = value;
                OnPropertyChanged();
            }
        }

        private int _pageSize;
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                if (value == _pageSize)
                {
                    return;
                }

                _pageSize = value;
                OnPropertyChanged();

                CalculatePageCount();
            }
        }

        private int _itemCount;
        public int ItemCount
        {
            get
            {
                return _itemCount;
            }
            set
            {
                if (value == _itemCount)
                {
                    return;
                }

                _itemCount = value;
                OnPropertyChanged();

                CalculatePageCount();
            }
        }

        private int _pageCount;
        public int PageCount
        {
            get
            {
                return _pageCount;
            }
            private set
            {
                if (value == _pageCount)
                {
                    return;
                }

                _pageCount = value;
                OnPropertyChanged();
            }
        }

        private void CalculatePageCount()
        {
            PageCount = PageSize == 0 ? 0 : (ItemCount - 1) / PageSize + 1;
        }

        #endregion
    }
}
