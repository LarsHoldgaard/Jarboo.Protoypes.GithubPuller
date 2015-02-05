using System.Web.Mvc;

namespace Jarboo.Protoypes.GithubPuller
{
    public static class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
