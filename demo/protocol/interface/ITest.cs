using System.Threading.Tasks;

namespace TFramework.Web
{
    [WebRPC]
    public interface ITest
    {
        Task<string> Invoke(string s, int i, int[] a, TestData data);
    }
}
