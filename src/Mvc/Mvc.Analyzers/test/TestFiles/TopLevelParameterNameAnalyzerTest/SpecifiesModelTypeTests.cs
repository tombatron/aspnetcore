﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Microsoft.AspNetCore.Mvc.Analyzers.TopLevelParameterNameAnalyzerTestFiles
{
    public class SpecifiesModelTypeTests
    {
        public void SpecifiesModelType_ReturnsFalse_IfModelBinderDoesNotSpecifyType(
            [ModelBinder(Name = "Name")] object model) { }

        public void SpecifiesModelType_ReturnsTrue_IfModelBinderSpecifiesTypeFromConstructor(
            [ModelBinder(typeof(SimpleTypeModelBinder))] object model) { }

        public void SpecifiesModelType_ReturnsTrue_IfModelBinderSpecifiesTypeFromProperty(
            [ModelBinder(BinderType = typeof(SimpleTypeModelBinder))] object model) { }
    }
}
