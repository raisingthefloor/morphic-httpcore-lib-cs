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
using Morphic.Http.Core.Utils;
using System.Collections;
using System.Collections.Generic;

namespace Morphic.Http.Core.Rfc2616;

public class Rfc2616Headers : IEnumerable<KeyValuePair<string, string>>
{
     private List<CaseInvariantString> _protectedKeys;
     private System.Collections.Generic.Dictionary<CaseInvariantString, string> _elements { get; set; }

     internal Rfc2616Headers(string[] protectedKeys)
     {
          var invariantProtectedKeys = new List<CaseInvariantString>();
          foreach (var protectedKey in protectedKeys)
          {
               var invariantProtectedKey = new CaseInvariantString(protectedKey);
               invariantProtectedKeys.Add(invariantProtectedKey);
          }

          _protectedKeys = invariantProtectedKeys;
          _elements = new();
     }

     //

     public record AddOrSetHeaderError : MorphicAssociatedValueEnum<AddOrSetHeaderError.Values>
     {
          // enum members
          public enum Values
          {
               HeaderIsReadOnly,
          }

          // functions to create member instances
          public static AddOrSetHeaderError HeaderIsReadOnly => new(Values.HeaderIsReadOnly);

          // associated values

          // verbatim required constructor implementation for MorphicAssociatedValueEnums
          private AddOrSetHeaderError(Values value) : base(value) { }
     }

     //

     private MorphicResult<MorphicUnit, AddOrSetHeaderError> Set(string key, string value)
     {
          var invariantKey = new CaseInvariantString(key);

          if (_protectedKeys.Contains(invariantKey) == true)
          {
               return MorphicResult.ErrorResult(AddOrSetHeaderError.HeaderIsReadOnly);
          }

          this.InternalSet(invariantKey, value);

          return MorphicResult.OkResult();
     }
     //
     internal MorphicResult<MorphicUnit, AddOrSetHeaderError> SetWithoutProtectedKeyCheck(string key, string value)
     {
          var invariantKey = new CaseInvariantString(key);
          
          return this.InternalSet(invariantKey, value);
     }
     //
     private MorphicResult<MorphicUnit, AddOrSetHeaderError> InternalSet(CaseInvariantString key, string value)
     {
          // NOTE: if we want to do any validation on individual headers, we can do so here (and then return validation failures via a SetHeaderError validation error>

          // set the element
          _elements[key] = value;

          return MorphicResult.OkResult();
     }

     //

     private string Get(string key)
     {
          var invariantKey = new CaseInvariantString(key);
          return _elements[invariantKey];
     }

     //

     public MorphicResult<MorphicUnit, AddOrSetHeaderError> Add(string key, string value)
     {
          var invariantKey = new CaseInvariantString(key);

          if (_protectedKeys.Contains(invariantKey) == true)
          {
               return MorphicResult.ErrorResult(AddOrSetHeaderError.HeaderIsReadOnly);
          }

          return this.InternalAdd(invariantKey, value);
     }
     //
     private MorphicResult<MorphicUnit, AddOrSetHeaderError> InternalAdd(CaseInvariantString key, string value)
     {
          _elements.Add(key, value);

          return MorphicResult.OkResult();
     }

     //

     public record RemoveHeaderError : MorphicAssociatedValueEnum<RemoveHeaderError.Values>
     {
          // enum members
          public enum Values
          {
               HeaderIsReadOnly,
          }

          // functions to create member instances
          public static RemoveHeaderError HeaderIsReadOnly => new(Values.HeaderIsReadOnly);

          // associated values

          // verbatim required constructor implementation for MorphicAssociatedValueEnums
          private RemoveHeaderError(Values value) : base(value) { }
     }
     //
     public MorphicResult<MorphicUnit, RemoveHeaderError> Remove(string key)
     {
          var invariantKey = new CaseInvariantString(key);

          if (_protectedKeys.Contains(invariantKey) == true)
          {
               return MorphicResult.ErrorResult(RemoveHeaderError.HeaderIsReadOnly);
          }

          return this.InternalRemove(invariantKey);
     }
     //
     private MorphicResult<MorphicUnit, RemoveHeaderError> InternalRemove(CaseInvariantString key)
     {
          _elements.Remove(key);

          return MorphicResult.OkResult();
     }

     //

     public bool ContainsKey(string key)
     {
          var invariantKey = new CaseInvariantString(key);
          return _elements.ContainsKey(invariantKey);
     }

     //

     public string this[string key]
     {
          get
          {
               return this.Get(key);
          }
          set
          {
               this.Set(key, value);
          }
     }

     //

     #region IEnumerable

     public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
     {
          foreach(var element in _elements)
          {
               yield return new KeyValuePair<string, string>(element.Key.Value, element.Value);
          }
     }

     IEnumerator IEnumerable.GetEnumerator()
     {
          return this.GetEnumerator();
     }

     #endregion IEnumerable
}
