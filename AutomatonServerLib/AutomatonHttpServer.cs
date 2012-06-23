using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AutomatonContract.Entities;
using AutomatonLib;

namespace AutomatonServerLib
{
    public class AutomatonHttpServer:HttpServerBase
    {
        public AutomatonHttpServer(int port): base(port)
        {
        }

        public override void OnStart()
        {
        }

        public override void OnResponse(ref HTTPRequestStruct request, ref HTTPResponseStruct response)
        {
            if (request.Method=="GET")
            {
                if (Authenticated(ref request,ref response))
                {
                    if (request.URL.Equals("/favicon.ico", StringComparison.InvariantCultureIgnoreCase))
                    {
                        response.Headers.Add("Content-Type", "image/x-icon");
                        ShowFile("favicon.ico", ref response);
                    }
                    else if (request.URL.Equals("/style.css", StringComparison.InvariantCultureIgnoreCase))
                    {
                        response.Headers.Add("Content-Type", "text/css; charset=utf-8");
                        ShowFile("style.css", ref response);
                    }
                    else if (request.URL.Equals("/mobile-style.css", StringComparison.InvariantCultureIgnoreCase))
                    {
                        response.Headers.Add("Content-Type", "text/css; charset=utf-8");
                        ShowFile("mobile-style.css", ref response);
                    }
                    else if (request.URL.Equals("/apple-touch-icon.png", StringComparison.InvariantCultureIgnoreCase))
                    {
                        response.Headers.Add("Content-Type", "image/png");
                        ShowFile("apple-touch-icon.png", ref response);
                    }
                    else if (request.URL.Equals("/"))
                    {
                        if (!string.IsNullOrEmpty(request.QueryString) && request.QueryString.StartsWith("?taskid=", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Guid id = new Guid(request.QueryString.ToLowerInvariant().Replace("?taskid=", string.Empty));
                            RunTask(id, ref response);
                        }
                        else
                        {
                            ShowAllTasks(ref response);
                        }
                    }
                }
            }
        }

        private bool Authenticated(ref HTTPRequestStruct request, ref HTTPResponseStruct response)
        {
            bool stale;
            if (!DigestAuthenticationHelper.CheckAuthentication(request,out stale))
            {
                response.status = 401;
                response.Headers.Add("WWW-Authenticate", DigestAuthenticationHelper.GenerateAuthenticateChallenge(request,false));
                return false;
            }
            else if (stale)//auth passed, but the nonce is stale
            {
                response.status = 401;
                response.Headers.Add("WWW-Authenticate", DigestAuthenticationHelper.GenerateAuthenticateChallenge(request, true));
                return false;
            }
            else
            {
                return true;
            }
        }

        private void RunTask(Guid id, ref HTTPResponseStruct response)
        {
            AgentManager.Current.RunTask(id);

            response.status = 302;
            response.Headers.Add("Location","/");
            response.Headers.Add("Content-Type", "text/html; charset=utf-8");
            StringBuilder sb = new StringBuilder();

            sb.Append(
                @"<html>
<head>
<title>Moved</title>
</head>
<body>
<h1>Moved</h1>
<p>This page has moved to <a href=""/"">here</a>.</p>
</body>
</html>");
            response.BodyData = Encoding.UTF8.GetBytes(sb.ToString());
            response.BodySize = response.BodyData.Length;
        }

        private static void ShowFile(string filename,ref HTTPResponseStruct response)
        {
            if (File.Exists(Path.Combine(Resources.ApplicationDirectory, filename)))
            {
                List<byte> responseData = new List<byte>();

                using (FileStream fs = new FileStream(Path.Combine(Resources.ApplicationDirectory, filename), FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[255];
                    int bytesRead = fs.Read(buffer, 0, 255);

                    while (bytesRead > 0)
                    {
                        byte[] tempBuffer = new byte[bytesRead];
                        Array.Copy(buffer, tempBuffer, bytesRead);
                        responseData.AddRange(tempBuffer);

                        bytesRead = fs.Read(buffer, 0, 255);
                    }
                }
                response.BodyData = responseData.ToArray();
            }
            else
            {
                response.status = 404;
            }
        }

        private void ShowAllTasks(ref HTTPResponseStruct response)
        {
            response.Headers.Add("Content-Type", "text/html; charset=utf-8");

            StringBuilder sb = new StringBuilder();

            sb.Append(
                @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"" lang=""en"" xml:lang=""en"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8"" />
    <title>Automaton</title>
    <link href=""/style.css"" type=""text/css"" rel=""stylesheet"" />
    <link media=""only screen and (max-device-width: 480px)"" href=""/mobile-style.css"" type=""text/css"" rel=""stylesheet"" />
    <meta name=""viewport"" content=""width = 320"" />
</head>
<body>
<h1>Automaton Server</h1>
");
            bool alt = false;
            foreach (Agent agent in AgentManager.Current.GetAgents())
            {
                sb.Append(@"<div class=""agent"">");
                sb.Append("<p>" + agent.Name + "</p>");
                sb.Append(@"</div>");
                foreach (Task task in agent.Tasks)
                {
                    sb.Append(@"<div class=""task" + (alt ? " alt" : string.Empty) + @""">");
                    sb.Append(@"<a href=""/?taskid=" + task.Id + @""">" + task.Name + @"</a>");
                    sb.Append("<p>" + task.Description + "</p>");
                    sb.Append(@"</div>");
                    alt = !alt;
                }
            }

            sb.Append(
                @"</body>
</html>");

            response.status = 200;
            response.BodyData = Encoding.UTF8.GetBytes(sb.ToString());
            response.BodySize = response.BodyData.Length;
        }
    }
}