using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AutomatonLib;

namespace AutomatonServerLib
{
    class DigestAuthenticationHelper
    {
        public static string Realm = "Automaton Server";

        public static string GenerateAuthenticateChallenge(HTTPRequestStruct request, bool stale)
        {
            return "Digest realm=\""+Realm+"\", algorithm=MD5, qop=\"auth\",nonce=\"" + GenerateNonce(request) + "\", opaque=\"" + Config.Config.Current.SessionId.ToByteArray().ConvertToBase16() + "\"" + (stale ? ", stale=true" : string.Empty);
        }

        private static string GenerateNonce(HTTPRequestStruct request)
        {
            return Encoding.UTF8.GetBytes(GenerateTimestamp(DateTime.Now) + "|" + request.UserHostAddress).ConvertToBase16();
        }

        public static bool CheckAuthentication(HTTPRequestStruct request, out bool stale)
        {
            if (request.Headers.ContainsKey("Authorization"))
            {
                string rawAuthHeader = request.Headers["Authorization"].Trim();

                if (rawAuthHeader.StartsWith("Digest"))
                {
                    string[] authComponents = rawAuthHeader.Substring(6).Split(new[] { ',' });
                    string method = request.Method;
                    string username = string.Empty;
                    string realm = string.Empty;
                    string uri = string.Empty;
                    string nonce = string.Empty;
                    string nc = string.Empty;
                    string cnonce = string.Empty;
                    string qop = string.Empty;
                    string response = string.Empty;

                    foreach (var authComponent in authComponents)
                    {
                        string key = authComponent.Substring(0, authComponent.IndexOf('=')).Trim();
                        string value = authComponent.Substring(authComponent.IndexOf('=') + 1).Trim().Replace("\"", string.Empty);

                        switch (key)
                        {
                            case "username":
                                username = value;
                                break;
                            case "realm":
                                realm = value;
                                break;
                            case "uri":
                                uri = value;
                                break;
                            case "nonce":
                                nonce = value;
                                break;
                            case "nc":
                                nc = value;
                                break;
                            case "cnonce":
                                cnonce = value;
                                break;
                            case "qop":
                                qop = value;
                                break;
                            case "response":
                                response = value;
                                break;
                        }
                    }

                    if (IsValidNonce(nonce, request, out stale))
                    {
                        if (username == Config.Config.Current.ServiceUserName && realm==Realm)
                        {
                            //now verify that the password supplied in the digest result is correct
                            return CheckAuthentication(username, realm, method, uri, nonce, nc, cnonce, qop, response);
                        }
                    }
                }
            }
            stale = false;
            return false;
        }

        /// <summary>
        /// valid nonce values must be generated within the last 15 minutes to prevent replay attacks.
        /// </summary>
        /// <param name="nonce"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        private static bool IsValidNonce(string nonce, HTTPRequestStruct request, out bool stale)
        {
            try
            {
                string convertedNonce = Encoding.UTF8.GetString(nonce.ConvertFromBase16());
                string[] nonceComponents = convertedNonce.Split(new[] { '|' });
                DateTime nonceTimeStamp = DateTime.ParseExact(nonceComponents[0], "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();
                string remoteIp = nonceComponents[1];

                TimeSpan diff = new TimeSpan(Math.Abs(DateTime.Now.Ticks - nonceTimeStamp.Ticks));
                stale = diff.TotalMinutes > 1;
                return remoteIp == request.UserHostAddress;
            }
            catch (Exception)
            {
                stale = false;
                return false;
            }
        }

        private static string GenerateTimestamp(DateTime messageTimestamp)
        {
            return messageTimestamp.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }

        private static bool CheckAuthentication(string username,string realm, string method, string uri, string nonce, string nc, string cnonce, string qop, string response)
        {
            using (HashAlgorithm hashAlg = MD5.Create())
            {
                string a1 = username+":"+realm+":"+Config.Config.Current.ServicePassword;
                string ha1 = a1.ComputeHash(hashAlg);

                string a2 = method + ":" + uri;
                string ha2 = a2.ComputeHash(hashAlg);

                string generatedResponse = ha1 + ":" + nonce + ":" + nc + ":" + cnonce + ":" + qop + ":" + ha2;
                string hGeneratedResponse = generatedResponse.ComputeHash(hashAlg);

                //if the client and server generated vlaues agree, then the user is authenticated
                return hGeneratedResponse == response;
            }
        }
    }
}
