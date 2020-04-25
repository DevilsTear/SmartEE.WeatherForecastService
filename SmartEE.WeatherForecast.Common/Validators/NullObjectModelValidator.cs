using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SmartEE.WeatherForecast.Common.Validators
{
    /// <summary>
    /// Uygulamada Razor pages üzerinden gönderilen model verisinin validasyonunu iptal eder
    /// </summary>
    public class NullObjectModelValidator : IObjectModelValidator
    {
        /// <summary>
        /// Gönderilen nesneyi valide eder
        /// </summary>
        /// <param name="actionContext">Uygulama context i ile ilişkili istek context i</param>
        /// <param name="validationState">Model için validasyon davranışını özelleştirmek amacıyla validasyon durumunu izlemek için kullanılır</param>
        /// <param name="prefix">Model öneki. Model nesnesini validationState içindeki girişlerle eşlemek için kullanılır</param>
        /// <param name="model">Model nesnesi</param>
        public void Validate(
            ActionContext actionContext,
            ValidationStateDictionary validationState,
            string prefix,
            object model)
        {
        }
    }
}
