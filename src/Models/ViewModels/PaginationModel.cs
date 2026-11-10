namespace MiaraEyeglasses.Models.ViewModels
{
    // Model phân trang dùng chung cho tất cả các màn hình danh sách (Reusable Pagination)
    public class PaginationModel
    {
        // Trang hiện tại (1-based)
        public int CurrentPage { get; set; } = 1;

        // Tổng số trang
        public int TotalPages { get; set; } = 1;

        // Số phần tử trên mỗi trang
        public int PageSize { get; set; } = 10;

        // Tổng số bản ghi tìm thấy
        public int TotalItems { get; set; } = 0;

        // Action điều hướng (mặc định là Index)
        public string Action { get; set; } = "Index";

        // Controller điều hướng
        public string Controller { get; set; } = "";

        // Các tham số lọc cần duy trì trên URL (search, categoryId, brandId, status...)
        public Dictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>();

        // Thuộc tính tiện ích
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public int StartItemIndex => TotalItems == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
        public int EndItemIndex => Math.Min(CurrentPage * PageSize, TotalItems);

        // Hàm tạo link URL cho một trang cụ thể, giữ nguyên tất cả tham số bộ lọc
        public Dictionary<string, string> GetRouteValuesForPage(int page)
        {
            var dict = new Dictionary<string, string>(RouteValues);
            dict["page"] = page.ToString();
            dict["pageSize"] = PageSize.ToString();
            return dict;
        }
    }
}

