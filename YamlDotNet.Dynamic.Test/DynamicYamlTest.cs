﻿//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013 Antoine Aubry

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

//  Credits for this class: https://github.com/imgen

using System;
using System.Collections.Generic;
using Xunit;

namespace YamlDotNet.Dynamic.Test
{
    public class DynamicYamlTest
    {
        [Fact]
        public void TestMappingNode()
        {
            dynamic dynamicYaml = new DynamicYaml(MappingYaml);

            var receipt = (string)(dynamicYaml.Receipt);
            var firstPartNo = dynamicYaml.Items[0].part_no;
            Assert.Equal(receipt, "Oz-Ware Purchase Invoice");
        }

        [Fact]
        public void TestSequenceNode()
        {
            dynamic dynamicYaml = new DynamicYaml(SequenceYaml);

            string firstName = dynamicYaml[0].name;
            Assert.Equal(firstName, "Me");
        }

        [Fact]
        public void TestNestedSequenceNode()
        {
            dynamic dynamicYaml = new DynamicYaml(NestedSequenceYaml);

            string firstNumberAsString = dynamicYaml[0, 0];
            Assert.Equal(firstNumberAsString, "1");
            int firstNumberAsInt = dynamicYaml[0, 0];
            Assert.Equal(firstNumberAsInt, 1);
        }

        [Fact]
        public void TestEnumConvert()
        {
            dynamic dynamicYaml = new DynamicYaml(EnumYaml);

            StringComparison stringComparisonMode = dynamicYaml[0].stringComparisonMode;
            Assert.Equal(StringComparison.CurrentCultureIgnoreCase, stringComparisonMode);
        }

        [Fact]
        public void TestArrayConvert()
        {
            dynamic dynamicYaml = new DynamicYaml(NestedSequenceYaml);
            dynamic[] dynamicArray = null;

            Assert.DoesNotThrow(() =>
            {
                dynamicArray = dynamicYaml;
                Assert.NotNull(dynamicArray);
                Assert.NotEmpty(dynamicArray);
            });

            Assert.DoesNotThrow(() =>
            {
                int[] intArray = dynamicArray[0];
                Assert.NotNull(intArray);
                Assert.NotEmpty(intArray);
            });
        }

        [Fact]
        public void TestEnumArrayConvert()
        {
            dynamic dynamicYaml = new DynamicYaml(EnumSequenceYaml);

            Assert.DoesNotThrow(() =>
            {
                StringComparison[] enumArray = dynamicYaml;
                Assert.NotNull(enumArray);
                Assert.NotEmpty(enumArray);
            });
        }

        [Fact]
        public void TestCollectionConvert()
        {
            dynamic dynamicYaml = new DynamicYaml(NestedSequenceYaml);
            List<dynamic> dynamicList = null;
            Assert.DoesNotThrow(() =>
            {
                dynamicList = dynamicYaml;
                Assert.NotNull(dynamicList);
                Assert.NotEmpty(dynamicList);
            });

            Assert.DoesNotThrow(() =>
            {
                List<int> intList = dynamicList[0];
                Assert.NotNull(intList);
                Assert.NotEmpty(intList);
            });
        }

        [Fact]
        public void TestEnumCollectionConvert()
        {
            dynamic dynamicYaml = new DynamicYaml(EnumSequenceYaml);

            Assert.DoesNotThrow(() =>
            {
                List<StringComparison> enumList = dynamicYaml;
                Assert.NotNull(enumList);
                Assert.NotEmpty(enumList);
            });
        }

        [Fact]
        public void TestDictionaryConvert()
        {
            dynamic dynamicYaml = new DynamicYaml(MappingYaml);

            Dictionary<string, dynamic> dynamicDictionary = null;
            Assert.DoesNotThrow(() =>
            {
                dynamicDictionary = dynamicYaml;
                Assert.NotNull(dynamicDictionary);
                Assert.NotEmpty(dynamicDictionary);
            });

            Assert.DoesNotThrow(() =>
            {
                Dictionary<string, string> stringDictonary = dynamicDictionary["customer"];
                Assert.NotNull(stringDictonary);
                Assert.NotEmpty(stringDictonary);
            });
        }

        [Fact]
        public void TestEnumDictonaryConvert()
        {
            dynamic dynamicYaml = new DynamicYaml(EnumMappingYaml);

            Assert.DoesNotThrow(() =>
            {
                IDictionary<StringComparison, string> enumDict = dynamicYaml;
                Assert.NotNull(enumDict);
                Assert.NotEmpty(enumDict);
            });
        }

        [Fact]
        public void TestNonexistingMember()
        {
            dynamic dynamicYaml = new DynamicYaml(SequenceYaml);
            var title = (string)(dynamicYaml[0].Title);
            Assert.Null(title);
            var id = (int?)(dynamicYaml[0].Id);
            Assert.Null(id);
        }

        private const string EnumYaml = @"---
            - stringComparisonMode: CurrentCultureIgnoreCase
            - stringComparisonMode: Ordinal";

        private const string EnumMappingYaml = @"---
            CurrentCultureIgnoreCase: on
            Ordinal: off";

        private const string EnumSequenceYaml = @"---
            - CurrentCultureIgnoreCase
            - Ordinal";

        private const string SequenceYaml = @"---
            - name: Me
            - name: You";

        private const string NestedSequenceYaml = @"---
            - [1, 2, 3]
            - [4, 5, 6]";

        private const string MappingYaml = @"---
            receipt:    Oz-Ware Purchase Invoice
            date:        2007-08-06
            customer:
                given:   Dorothy
                family:  Gale

            items:
                - part_no:   A4786
                  descrip:   Water Bucket (Filled)
                  price:     1.47
                  quantity:  4

                - part_no:   E1628
                  descrip:   High Heeled ""Ruby"" Slippers
                  price:     100.27
                  quantity:  1

            bill-to:  &id001
                street: |
                        123 Tornado Alley
                        Suite 16
                city:   East Westville
                state:  KS

            ship-to:  *id001

            specialDelivery:  >
                Follow the Yellow Brick
                Road to the Emerald City.
                Pay no attention to the
                man behind the curtain.
            
...";
    }
}
