using CefSharp;
using CefSharp.Handler;

namespace DeltaEditor.Helpers
{
    class ExternalRequestHandler : RequestHandler
    {
        protected override bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            if (!request.Url.Equals("http://localhtml/") && !request.Url.StartsWith("file:///"))
            {
                System.Diagnostics.Process.Start(request.Url);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
