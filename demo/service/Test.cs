using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TFramework.Web
{
    public class Test : ITest, IHttpContext
    {
        public HttpContext Context { get; set; }

        public Task<string> Invoke(string s, int i, int[] a, TestData data)
        {
            int sum = 0;
            foreach (var item in a)
            {
                sum += item;
            }
            return Task.FromResult($"{i}:{s}:{sum}:{data.s1}:{data.s2}");
        }
    }
}
