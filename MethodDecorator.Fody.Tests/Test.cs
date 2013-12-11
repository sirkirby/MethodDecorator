﻿using System.Reflection;

namespace MethodDecorator.Fody.Tests
{
    public class DecoratedSimpleTest
    {
        public DecoratedSimpleTest()
        {
            var weaverHelper = new WeaverHelper(@"SimpleTest\SimpleTest.csproj");
            Assembly = weaverHelper.Weave();
        }

        public Assembly Assembly { get; private set; }
    }

    public class DecorateWithExternalTest : DecoratedSimpleTest {
        public DecorateWithExternalTest() {
            var path = base.Assembly.Location.Replace("SimpleTest2.dll", "AnotherAssemblyAttributeContainer.dll");
            this.ExternalAssembly = Assembly.LoadFile(path);
        }

        public Assembly ExternalAssembly { get; private set; }
        
    }
}