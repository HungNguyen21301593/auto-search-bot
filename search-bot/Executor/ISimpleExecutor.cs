using OpenQA.Selenium;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kijiji_searchbot.Executor
{
    public interface ISimpleExecutor
    {
        Task Run();
        void CleanUpHistory();
    }
}