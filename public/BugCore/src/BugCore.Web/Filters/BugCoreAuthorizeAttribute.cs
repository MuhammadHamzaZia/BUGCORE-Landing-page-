using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BugCore.Core.Enums;
using BugCore.Core.Interfaces;

namespace BugCore.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class BugCoreAuthorizeAttribute : TypeFilterAttribute
{
    public BugCoreAuthorizeAttribute(AccessLevel requiredLevel, bool globalOnly = false) : base(typeof(BugCoreAuthorizeFilter))
    {
        Arguments = [requiredLevel, globalOnly];
    }

    private class BugCoreAuthorizeFilter : IAsyncAuthorizationFilter
    {
        private readonly AccessLevel _requiredLevel;
        private readonly bool _globalOnly;
        private readonly IUserContextService _userContext;
        private readonly IAccessControlService _accessControl;

        public BugCoreAuthorizeFilter(
            AccessLevel requiredLevel, 
            bool globalOnly,
            IUserContextService userContext, 
            IAccessControlService accessControl)
        {
            _requiredLevel = requiredLevel;
            _globalOnly = globalOnly;
            _userContext = userContext;
            _accessControl = accessControl;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userId = _userContext.GetCurrentUserId();
            if (!userId.HasValue)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            int? activeProjectId = _globalOnly ? null : _userContext.GetActiveProjectId();
            var hasAccess = await _accessControl.HasAccessLevelAsync(userId.Value, _requiredLevel, activeProjectId);

            if (!hasAccess)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
