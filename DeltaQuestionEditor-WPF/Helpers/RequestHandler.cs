using CefSharp;
using CefSharp.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeltaQuestionEditor_WPF.Helpers
{
    class ExternalRequestHandler : RequestHandler
    {
        protected override bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            if (!request.Url.Equals("http://localhtml/"))
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
