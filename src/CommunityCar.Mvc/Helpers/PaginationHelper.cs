namespace CommunityCar.Web.Helpers;

public static class PaginationHelper
{
    public static int CalculateTotalPages(int totalCount, int pageSize)
    {
        return (int)Math.Ceiling((double)totalCount / pageSize);
    }
}
