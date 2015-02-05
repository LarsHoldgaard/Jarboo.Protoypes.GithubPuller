using DataAnnotationsExtensions.ClientValidation;
using Jarboo.Protoypes.GithubPuller.App_Start;
using WebActivator;

[assembly: PreApplicationStartMethod(typeof(RegisterClientValidationExtensions), "Start")]
 
namespace Jarboo.Protoypes.GithubPuller.App_Start {
    public static class RegisterClientValidationExtensions {
        public static void Start() {
            DataAnnotationsModelValidatorProviderExtensions.RegisterValidationExtensions();            
        }
    }
}