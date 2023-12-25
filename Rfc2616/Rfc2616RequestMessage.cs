// Copyright 2023 Raising the Floor - US, Inc.
//
// Licensed under the New BSD license. You may not use this file except in
// compliance with this License.
//
// You may obtain a copy of the License at
// https://github.com/raisingthefloor/morphic-httpcore-lib-cs/blob/main/LICENSE
//
// The R&D leading to these results received funding from the:
// * Rehabilitation Services Administration, US Dept. of Education under
//   grant H421A150006 (APCP)
// * National Institute on Disability, Independent Living, and
//   Rehabilitation Research (NIDILRR)
// * Administration for Independent Living & Dept. of Education under grants
//   H133E080022 (RERC-IT) and H133E130028/90RE5003-01-00 (UIITA-RERC)
// * European Union's Seventh Framework Programme (FP7/2007-2013) grant
//   agreement nos. 289016 (Cloud4all) and 610510 (Prosperity4All)
// * William and Flora Hewlett Foundation
// * Ontario Ministry of Research and Innovation
// * Canadian Foundation for Innovation
// * Adobe Foundation
// * Consumer Electronics Association Foundation

using Morphic.Core;
using System;
using System.Diagnostics;

namespace Morphic.Http.Core.Rfc2616;

public struct Rfc2616RequestMessage
{
     public Rfc2616Headers Headers { get; }

     public string Method { get; init; }
     public string Host { get; }
     public string RequestUri { get; init; }

     private const uint HTTP_MAJOR_VERSION = 1;
     private const uint HTTP_MINOR_VERSION = 1;

     public Rfc2616RequestMessage(string method, Uri absoluteRequestUri) : this(method, absoluteRequestUri.Host + (absoluteRequestUri.Port >= 0 ? ":" + absoluteRequestUri.Port.ToString() : ""), absoluteRequestUri.PathAndQuery) { }

     public Rfc2616RequestMessage(string method, string host, string requestUriAsString)
     {
          // validate method
          Debug.Assert(method == method.ToUpperInvariant(), "Argument 'method' is uppercase by convention.");

          // validate host
          if (Rfc2616RequestMessage.IsWellFormedHost(host) == false)
          {
               throw new ArgumentException("Host is not a valid hostname or is not a valid hostname:port concatenation", nameof(host));
          }

          // validate requestUri
          // see: RFC 2616, section 5.1.2
          var requestUriIsValid = false;
          if (requestUriAsString == "*")
          {
               // valid
               //
               // NOTE: technically, this Request-URI form may only be valid with some HTTP methods
               requestUriIsValid = true;
          } 
          else if (System.Uri.IsWellFormedUriString(requestUriAsString, UriKind.Absolute))
          {
               // absolute uris are valid
               // NOTE: in this implementation, we are restricting absolute URI schemes to HTTP and HTTPS
               var absoluteRequestUri = new Uri(requestUriAsString, UriKind.Absolute);
               var schemeIsAllowed = absoluteRequestUri.Scheme.ToUpperInvariant() switch
               {
                    "HTTP" or "HTTPS" => true,
                    _ => false,
               };

               requestUriIsValid = schemeIsAllowed;
          }
          else if (System.Uri.IsWellFormedUriString(requestUriAsString, UriKind.Relative))
          {
               // absolute paths (i.e. without scheme or host) are valid
               requestUriIsValid = true;
          }
          // NOTE: we do not support the authority form of Request-URI in this implementation (e.g. for the "CONNECT" method)
          //
          if (requestUriIsValid == false)
          {
               throw new ArgumentException("Request-URI must be '*', an absolute uri or an absolute path." + nameof(requestUriAsString));
          }

          // if requestUriAsString is an absolute uri, verify that the host matches
          if (System.Uri.IsWellFormedUriString(requestUriAsString, UriKind.Absolute))
          {
               var hostnameAndPortMatch = false;

               var absoluteRequestUri = new Uri(requestUriAsString, UriKind.Absolute);
               var copyOfHost = host;
               //
               var absoluteRequestUriHost = absoluteRequestUri.Host.ToUpperInvariant();
               if (copyOfHost.Length >= absoluteRequestUriHost.Length && copyOfHost.Substring(0, absoluteRequestUriHost.Length).ToUpperInvariant() == absoluteRequestUriHost)
               {
                    // hostname matches; now check for port
                    copyOfHost = copyOfHost.Substring(absoluteRequestUriHost.Length);
                    if (copyOfHost[0] == ':')
                    {
                         // contains port number
                         copyOfHost = copyOfHost.Substring(1);
                         ushort hostPortNumber;
                         var parseAsUInt16Success = UInt16.TryParse(copyOfHost, out hostPortNumber);
                         if (parseAsUInt16Success == true)
                         {
                              if (absoluteRequestUri.Port == hostPortNumber)
                              {
                                   hostnameAndPortMatch = true;
                              }
                         }
                    }
                    else
                    {
                         // does not contain port number
                         ushort? defaultPortForScheme;
                         switch (absoluteRequestUri.Scheme.ToUpperInvariant())
                         {
                              case "HTTP":
                                   defaultPortForScheme = 80;
                                   break;
                              case "HTTPS":
                                   defaultPortForScheme = 443;
                                   break;
                              default:
                                   defaultPortForScheme = null;
                                   break;
                         }
                         if (defaultPortForScheme is not null)
                         {
                              if (absoluteRequestUri.Port >= 0 && defaultPortForScheme!.Value == (ushort)absoluteRequestUri.Port)
                              {
                                   hostnameAndPortMatch = true;
                              }
                         }
                    }
               }

               if (hostnameAndPortMatch == false)
               {
                    throw new ArgumentException("If Request-URI is an absolute URI, its hostname and port must match argument 'host'", nameof(requestUriAsString));
               }
          }

          this.Method = method;
          this.Host = host;
          this.RequestUri = requestUriAsString;

          // set up with empty headers (except for "Host" header)
          var protectedHeaderKeys = new string[]
          {
               "Host",
          };
          var headers = new Rfc2616Headers(protectedHeaderKeys);
          headers.SetWithoutProtectedKeyCheck("Host", host);
          this.Headers = headers;
     }

     private static bool IsWellFormedHost(string host)
     {
          if (host.Length == 0)
          {
               return false;
          }

          var copyOfHost = host;
          
          if (copyOfHost[0] == '-' || copyOfHost[0] == ':')
          {
               return false;
          }

          // split host into hostname and optional port
          string hostname;
          string? portAsString;
          //
          if (host.Contains(":") == true)
          {
               hostname = host.Substring(0, host.IndexOf(':'));
               portAsString = host.Substring(hostname.Length + 1);
          }
          else
          {
               hostname = host;
               portAsString = null;
          }

          // validate host
          foreach(char ch in host)
          {
               switch (ch)
               {
                    case char lowercaseCh when (lowercaseCh >= 'a' && lowercaseCh <= 'z'):
                    case char uppercaseCh when (uppercaseCh >= 'A' && uppercaseCh <= 'Z'):
                    case char numberCh when (numberCh >= '0' && numberCh <= '9'):
                    case '-':
                         // allowed
                         break;
                    default:
                         // not allowed
                         return false;
               }
          }

          // validate port
          if (portAsString is not null)
          {
               var portIsInRange = ushort.TryParse(portAsString, out _);
               if (portIsInRange == false)
               {
                    return false;
               }
          }

          // if we reach here, the host was validated as well-formed
          return true;
     }
}
