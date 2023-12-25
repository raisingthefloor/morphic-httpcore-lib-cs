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

using System;

namespace Morphic.Http.Core.Rfc2616;

public struct Rfc2616ResponseMessage
{
     public Rfc2616Headers Headers { get; }

     public (ushort StatusCode, string ReasonPhrase) Status { get; }
     private (byte MajorVersion, byte MinorVersion) HttpVersion { get; }

     private const uint MAX_HTTP_MAJOR_VERSION = 1;
     private const uint MAX_HTTP_MINOR_VERSION = 1;

     public Rfc2616ResponseMessage(byte httpMajorVersion, byte httpMinorVersion, KnownStatusCode statusCode) : this(httpMajorVersion, httpMinorVersion, (ushort)statusCode, Rfc2616ResponseMessage.ConvertKnownStatusCodeToReasonPhrase(statusCode)) { }

     public Rfc2616ResponseMessage(byte httpMajorVersion, byte httpMinorVersion, ushort statusCode, string reasonPhrase)
     {
          if (httpMajorVersion > MAX_HTTP_MAJOR_VERSION)
          {
               throw new ArgumentOutOfRangeException(nameof(httpMajorVersion), "HTTP version cannot be higher than HTTP/" + MAX_HTTP_MAJOR_VERSION + "." + MAX_HTTP_MINOR_VERSION);
          }
          else if (httpMajorVersion == MAX_HTTP_MAJOR_VERSION && httpMinorVersion > MAX_HTTP_MINOR_VERSION)
          {
               throw new ArgumentOutOfRangeException(nameof(httpMajorVersion), "HTTP version cannot be higher than HTTP/" + MAX_HTTP_MAJOR_VERSION + "." + MAX_HTTP_MINOR_VERSION);
          }

          // see RFC 2616, section 6.1.1
          if (statusCode > 999)
          {
               throw new ArgumentOutOfRangeException(nameof(statusCode));
          }
          if (reasonPhrase.Contains((char)0x13) || reasonPhrase.Contains((char)0x10))
          {
               throw new ArgumentException("Reason phrase cannot contain line terminator characters", nameof(reasonPhrase));
          }

          this.HttpVersion = (httpMajorVersion, httpMinorVersion);
          this.Status = (statusCode, reasonPhrase);

          // set up with empty headers
          var protectedHeaderKeys = new string[]
          {
          };
          var headers = new Rfc2616Headers(protectedHeaderKeys);
          this.Headers = headers;
     }

     //

     public enum KnownStatusCode : ushort
     {
          _100_Continue                      = 100,
          _101_SwitchingProtocols            = 101,
          //
          _200_OK                            = 200,
          _201_Created                       = 201,
          _202_Accepted                      = 202,
          _203_NonauthoritativeInformation   = 203,
          _204_NoContent                     = 204,
          _205_ResetContent                  = 205,
          _206_PartialContent                = 206,
          //
          _300_MultipleChoices               = 300,
          _301_MovedPermanently              = 301,
          _302_Found                         = 302,
          _303_SeeOther                      = 303,
          _304_NotModified                   = 304,
          _305_UseProxy                      = 305,
          _307_TemporaryRedirect             = 306,
          //
          _400_BadRequest                    = 400,
          _401_Unauthorized                  = 401,
          _402_PaymentRequired               = 402,
          _403_Forbidden                     = 403,
          _404_NotFound                      = 404,
          _405_MethodNotAllowed              = 405,
          _406_NotAcceptable                 = 406,
          _407_ProxyAuthenticationRequired   = 407,
          _408_RequestTimeout                = 408,
          _409_Conflict                      = 409,
          _410_Gone                          = 410,
          _411_LengthRequired                = 411,
          _412_PreconditionFailed            = 412,
          _413_RequestEntityTooLarge         = 413,
          _414_RequesturiTooLarge            = 414,
          _415_UnsupportedMediaType          = 415,
          _416_RequestedRangeNotSatisfiable  = 416,
          _417_ExpectationFailed             = 417,
          //
          _500_InternalServerError           = 500,
          _501_NotImplemented                = 501,
          _502_BadGateway                    = 502,
          _503_ServiceUnavailable            = 503,
          _504_GatewayTimeout                = 504,
          _505_HttpVersionNotSupported       = 505,
     }

     private static string? ConvertKnownStatusCodeToReasonPhraseOrNull(KnownStatusCode value)
     {
          return value switch
          {
               KnownStatusCode._100_Continue => "Continue",
               KnownStatusCode._101_SwitchingProtocols => "Switching Protocols",
               KnownStatusCode._200_OK => "OK",
               KnownStatusCode._201_Created => "Created",
               KnownStatusCode._202_Accepted => "Accepted",
               KnownStatusCode._203_NonauthoritativeInformation => "Non-Authoritative Information",
               KnownStatusCode._204_NoContent => "No Content",
               KnownStatusCode._205_ResetContent => "Reset Content",
               KnownStatusCode._206_PartialContent => "Partial Content",
               KnownStatusCode._300_MultipleChoices => "Multiple Choices",
               KnownStatusCode._301_MovedPermanently => "Moved Permanently",
               KnownStatusCode._302_Found => "Found",
               KnownStatusCode._303_SeeOther => "See Other",
               KnownStatusCode._304_NotModified => "Not Modified",
               KnownStatusCode._305_UseProxy => "Use Proxy",
               KnownStatusCode._307_TemporaryRedirect => "Temporary Redirect",
               KnownStatusCode._400_BadRequest => "Bad Request",
               KnownStatusCode._401_Unauthorized => "Unauthorized",
               KnownStatusCode._402_PaymentRequired => "Payment Required",
               KnownStatusCode._403_Forbidden => "Forbidden",
               KnownStatusCode._404_NotFound => "Not Found",
               KnownStatusCode._405_MethodNotAllowed => "Method Not Allowed",
               KnownStatusCode._406_NotAcceptable => "Not Acceptable",
               KnownStatusCode._407_ProxyAuthenticationRequired => "Proxy Authentication Required",
               KnownStatusCode._408_RequestTimeout => "Request Time-out",
               KnownStatusCode._409_Conflict => "Conflict",
               KnownStatusCode._410_Gone => "Gone",
               KnownStatusCode._411_LengthRequired => "Length Required",
               KnownStatusCode._412_PreconditionFailed => "Precondition Failed",
               KnownStatusCode._413_RequestEntityTooLarge => "Request Entity Too Large",
               KnownStatusCode._414_RequesturiTooLarge => "Request-URI Too Large",
               KnownStatusCode._415_UnsupportedMediaType => "Unsupported Media Type",
               KnownStatusCode._416_RequestedRangeNotSatisfiable => "Requested range not satisfiable",
               KnownStatusCode._417_ExpectationFailed => "Expectation Failed",
               KnownStatusCode._500_InternalServerError => "Internal Server Error",
               KnownStatusCode._501_NotImplemented => "Not Implemented",
               KnownStatusCode._502_BadGateway => "Bad Gateway",
               KnownStatusCode._503_ServiceUnavailable => "Service Unavailable",
               KnownStatusCode._504_GatewayTimeout => "Gateway Time-out",
               KnownStatusCode._505_HttpVersionNotSupported => "HTTP Version not supported",
               _ => null,
          };
     }

     private static string ConvertKnownStatusCodeToReasonPhrase(KnownStatusCode value)
     {
          var result = Rfc2616ResponseMessage.ConvertKnownStatusCodeToReasonPhraseOrNull(value);
          if (result is not null)
          {
               return result!;
          }
          else
          {
               throw new ArgumentOutOfRangeException(nameof(value));
          }
     }

}
