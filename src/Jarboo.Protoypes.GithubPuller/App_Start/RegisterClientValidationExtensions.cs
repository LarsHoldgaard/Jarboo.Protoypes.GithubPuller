using DataAnnotationsExtensions.ClientValidation;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Jarboo.Protoypes.GithubPuller.App_Start.RegisterClientValidationExtensions), "Start")]
 
namespace Jarboo.Protoypes.GithubPuller.App_Start {
    public static class RegisterClientValidationExtensions {
        public static void Start() {
            DataAnnotationsModelValidatorProviderExtensions.RegisterValidationExtensions();            
        }
    }
}