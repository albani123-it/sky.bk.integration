using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using sky.bk.integration.Libs;
using System.Data.SqlClient;

namespace CustomTokenAuthProvider
{

    public class TokenProviderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenProviderOptions _options;
        private readonly JsonSerializerSettings _serializerSettings;

        public TokenProviderMiddleware(
            RequestDelegate next,
            IOptions<TokenProviderOptions> options)
        {
            _next = next;

            _options = options.Value;
            ThrowIfInvalidOptions(_options);

            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        public Task Invoke(HttpContext context)
        {

            if (!context.Request.Path.Equals(_options.Path, StringComparison.Ordinal))
            {
                bool condition = (context.User.Identity.IsAuthenticated);
                lDbConn dbconn = new lDbConn();
                //lServiceLogs lsl = new lServiceLogs();
                JObject joReturn = new JObject();

                if (context.Response.StatusCode==200)
                {
                    var authHeader = context.Request.Headers["Authorization"].ToString();
                    var authBits = authHeader.Split(" ");
                   
                    //return _next(context);
                    if (authBits.Length > 1)
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var Audiences = dbconn.domainGetTokenCredential("Audience");
                        var Issuer = dbconn.domainGetTokenCredential("Issuer");

                        try
                        {
                            var decoded_token = handler.ReadJwtToken(authBits[1]);
                            var tokentime = decoded_token.ValidTo.AddHours(7);
                            List<string> tokenaudience = (from string str in decoded_token.Audiences
                                                          select str.ToString()).ToList();

                            if (tokentime < DateTime.Now)
                            {
                                condition = false;
                                var errresponse = new
                                {
                                    authenticated = false,
                                    message = "Invalid access, token expired. Authenticated" + condition,
                                    date = DateTime.Now
                                };
                                context.Response.ContentType = "application/json";

                                var strJTkn = (JToken.FromObject(errresponse)).ToString();
                                joReturn.Add("success", false);
                                joReturn.Add("data", JObject.Parse(strJTkn));
                               // lsl.ServiceRecordLogs(context.Request.Method.ToString(), context.Request.Path.Value.ToString(), "Authenticated Failed", joReturn.ToString());

                                return context.Response.WriteAsync(JsonConvert.SerializeObject(errresponse, _serializerSettings));
                            }
                            else if (tokenaudience[0].ToString() != Audiences)
                            {
                                condition = false;
                                var errresponse = new
                                {
                                    authenticated = false,
                                    message = "Invalid Audiences. Authenticated " + condition,
                                    date = DateTime.Now
                                };
                                context.Response.ContentType = "application/json";
                                var strJTkn = (JToken.FromObject(errresponse)).ToString();
                                joReturn.Add("success", false);
                                joReturn.Add("data", JObject.Parse(strJTkn));
                                //lsl.ServiceRecordLogs(context.Request.Method.ToString(), context.Request.Path.Value.ToString(), "Authenticated Failed", joReturn.ToString());

                                return context.Response.WriteAsync(JsonConvert.SerializeObject(errresponse, _serializerSettings));
                            }
                            else if (decoded_token.Issuer.ToString() != Issuer)
                            {
                                condition = false;
                                var errresponse = new
                                {
                                    authenticated = false,
                                    message = "Invalid Issuer. Authenticated " + condition,
                                    date = DateTime.Now
                                };
                                context.Response.ContentType = "application/json";
                                var strJTkn = (JToken.FromObject(errresponse)).ToString();
                                joReturn.Add("success", false);
                                joReturn.Add("data", JObject.Parse(strJTkn));
                                //lsl.ServiceRecordLogs(context.Request.Method.ToString(), context.Request.Path.Value.ToString(), "Authenticated Failed", joReturn.ToString());

                                return context.Response.WriteAsync(JsonConvert.SerializeObject(errresponse, _serializerSettings));
                            }
                            else
                            {
                                return _next(context);
                            }
                        }
                        catch (Exception ex)
                        {
                            condition = false;
                            var errresponse = new
                            {
                                authenticated = false,
                                message = "Invalid token access. Authenticated" + condition,
                                date = DateTime.Now
                            };

                            context.Response.ContentType = "application/json";
                            var strJTkn = (JToken.FromObject(errresponse)).ToString();
                            joReturn.Add("success", false);
                            joReturn.Add("data", JObject.Parse(strJTkn));
                           // lsl.ServiceRecordLogs(context.Request.Method.ToString(), context.Request.Path.Value.ToString(), "Authenticated Failed", joReturn.ToString());

                            return context.Response.WriteAsync(JsonConvert.SerializeObject(errresponse, _serializerSettings));
                        }
                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                    var errresponse = new
                    {
                        authenticated = false,
                        message = "Bad request. Authenticated " + condition,
                        date = DateTime.Now
                    };

                    context.Response.ContentType = "application/json";
                    var strJTkn = (JToken.FromObject(errresponse)).ToString();
                    joReturn.Add("success", false);
                    joReturn.Add("data", JObject.Parse(strJTkn));
                   // lsl.ServiceRecordLogs(context.Request.Method.ToString(), context.Request.Path.Value.ToString(), "Authenticated Failed", joReturn.ToString());

                    return context.Response.WriteAsync(JsonConvert.SerializeObject(errresponse, _serializerSettings));
                }

            }

            if (!context.Request.Method.Equals("POST") || !context.Request.HasFormContentType)
            {
                context.Response.StatusCode = 400;
                var errresponse = new
                {
                    success = false,
                    message = "Bad request.",
                    date = DateTime.Now
                };

                // Serialize and return the response
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(errresponse, _serializerSettings));
            }


            return GenerateToken(context);
        }

        private async Task GenerateToken(HttpContext context)
        {
            var username = context.Request.Headers["username"];
            var password = context.Request.Headers["password"];

            var identity = await _options.IdentityResolver(username, password);
            if (identity == null)
            {
                context.Response.StatusCode = 400;
                var errresponse = new
                {
                    success = false,
                    message = "Invalid username or token.",
                    date = DateTime.Now
                };

                // Serialize and return the response
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(errresponse, _serializerSettings));
                return;
            }

            var now = DateTime.UtcNow;

          
            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, await _options.NonceGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Create the JWT and write it to a string
            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(_options.Expiration),
                signingCredentials: _options.SigningCredentials);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                success=true,
                access_token = encodedJwt,
                expires_in = (int)_options.Expiration.TotalSeconds
            };

            // Serialize and return the response
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response, _serializerSettings));
            return;
        }

        private static void ThrowIfInvalidOptions(TokenProviderOptions options)
        {
            if (string.IsNullOrEmpty(options.Path))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Path));
            }

            if (string.IsNullOrEmpty(options.Issuer))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Issuer));
            }

            if (string.IsNullOrEmpty(options.Audience))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Audience));
            }

            if (options.Expiration == TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(TokenProviderOptions.Expiration));
            }

            if (options.IdentityResolver == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.IdentityResolver));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.SigningCredentials));
            }

            if (options.NonceGenerator == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.NonceGenerator));
            }
        }

    }
}