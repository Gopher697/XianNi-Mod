using System.Threading.Tasks;
namespace xn.voice
{
    public interface ITTSProvider
    {
        Task<bool> GenerateSpeech(string text, string voiceId, string outputPath);
        string GetProviderName();
    }
}