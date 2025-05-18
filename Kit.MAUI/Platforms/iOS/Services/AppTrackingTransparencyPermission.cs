using Kit.Forms.Services.Interfaces;
using Kit.iOS.Services;

[assembly: Dependency(typeof(AppTrackingTransparencyPermission))]
namespace Kit.iOS.Services
{
    [Preserve(AllMembers = true)]
    public class AppTrackingTransparencyPermission : Permissions.BasePlatformPermission, IAppTrackingTransparencyPermission
    {
        protected override Func<IEnumerable<string>> RequiredInfoPlistKeys => () => new string[] { "NSUserTrackingUsageDescription" };

        // Requests the user to accept or deny a permission
        public void RequestAsync(Action<PermissionStatus> completion)
        {
            AppTrackingTransparency.ATTrackingManager.RequestTrackingAuthorization((result) => completion(Convert(result)));
        }

        // This method checks if current status of the permission
        public override Task<PermissionStatus> CheckStatusAsync()
        {
            return Task.FromResult(Convert(AppTrackingTransparency.ATTrackingManager.TrackingAuthorizationStatus));
        }

        private PermissionStatus Convert(AppTrackingTransparency.ATTrackingManagerAuthorizationStatus status)
        {
            switch (status)
            {
                case AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.NotDetermined:
                    return PermissionStatus.Disabled;
                case AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.Restricted:
                    return PermissionStatus.Restricted;
                case AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.Denied:
                    return PermissionStatus.Denied;
                case AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.Authorized:
                    return PermissionStatus.Granted;
                default:
                    return PermissionStatus.Unknown;
            }
        }


    }
}