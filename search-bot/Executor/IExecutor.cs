using OpenQA.Selenium;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kijiji_searchbot.Executor
{
    public interface IExecutor
    {
        Task Run();
        void CleanUpHistory();
        IReadOnlyCollection<IWebElement> GetAllUrlElements(string homeUrl);
        Task ScanPage(string homeUrl);
    }
}