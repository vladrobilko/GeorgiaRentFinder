using Google.Apis.Services;
using Google.Apis.Translate.v2;
using Google.Cloud.Translation.V2;

namespace WebScraper
{
    public static class GoogleTranslator
    {
        public static string Translate(this string textToTranslate, string toLanguage, string apiToken)
        {
            var service = new TranslateService(new BaseClientService.Initializer { ApiKey = apiToken });

            var client = new TranslationClientImpl(service, TranslationModel.ServiceDefault);

            var result = client.TranslateText(textToTranslate, toLanguage);

            return result.TranslatedText;
        }
    }
}