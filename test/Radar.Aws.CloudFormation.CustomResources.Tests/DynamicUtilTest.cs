using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Radar.Aws.CloudFormation.CustomResources.Util;

namespace Radar.Aws.CloudFormation.CustomResources.Tests
{
    public class DynamicUtilTest
    {
        private readonly ITestOutputHelper Output;
        public DynamicUtilTest(ITestOutputHelper output)
        {
            Output = output;
        }

        public class Data
        {
            public string This { get; set; }
            public string That { get; set; }
        }

        [Fact]
        public void TestIsNullTrue()
        {
            dynamic x = null;
            Assert.True(DynamicUtil.IsNull(x));
        }
        
        [Fact]
        public void TestIsNullFalse()
        {
            dynamic x = 1;
            Assert.False(DynamicUtil.IsNull(x));
        }

        [Fact]
        public void TestToDynamic()
        {
            Data data = new Data
            {
                This = "this",
                That = "that"
            };

            dynamic dyn = DynamicUtil.ToDynamic(data);
            Assert.Equal("this", dyn.This);
            Assert.Equal("that", dyn.That);
        }

        [Fact]
        public void TestFromDynamic()
        {
            dynamic dyn = new System.Dynamic.ExpandoObject();
            dyn.This = "this";
            dyn.That = "that";
            Data data = DynamicUtil.FromDynamic<Data>(dyn);
            Assert.Equal("this", data.This);
            Assert.Equal("that", data.That);
        }
        
    }
}