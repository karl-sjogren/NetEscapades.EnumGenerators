using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using VerifyXunit;
using Xunit;

#if INTERCEPTORS
namespace NetEscapades.EnumGenerators.Tests;
#else
namespace NetEscapades.EnumGenerators.Tests.Roslyn4_04;
#endif

[UsesVerify]
public class InterceptorTests
{
    private readonly Dictionary<string, string> _interceptionEnabled =
        new() { { "build_property.EnableEnumGeneratorInterceptor", "true" } };

#if INTERCEPTORS
    [Fact]
    public Task CanInterceptToString()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                }
                
                public class InnerClass
                {
                    public MyEnum _field = default;
                    public MyEnum Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = MyEnum.Second;
                        var var1 = myValue.ToString();
                        var var2 = MyEnum.Second.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                    }
                }
            }
            """;
        var (diagnostics, output) =
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_interceptionEnabled, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanInterceptHasFlag()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;
            using System;

            namespace MyTestNameSpace
            {
                [EnumExtensions]
                [Flags]
                internal enum MyEnum
                {
                    First = 1,
                    Second = 2,
                    Third = 4,
                }
                
                public class InnerClass
                {
                    public MyEnum _field = default;
                    public MyEnum Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = MyEnum.Second;
                        var var1 = myValue.HasFlag(MyEnum.First);
                        var var2 = MyEnum.Second.HasFlag(myValue);
                        var var3 = Property.HasFlag(MyEnum.First);
                        var var4 = _field.HasFlag(MyEnum.First);
                    }
                }
            }
            """;
        var (diagnostics, output) =
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_interceptionEnabled, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanInterceptMultipleEnumsToString()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                    Third = 2,
                }

                [EnumExtensions]
                internal enum AnotherEnum
                {
                    First = 0,
                    Second = 1,
                    Third = 2,
                }

                internal enum YetAnotherEnum
                {
                    First = 0,
                    Second = 1,
                    Third = 2,
                }
                
                public class InnerClass
                {
                    public void MyTest()
                    {
                        var result = AnotherEnum.First.ToString();
                        AssertValue(MyEnum.First);
                        AssertValue(AnotherEnum.Second);
                        AssertValue(MyEnum.Third);
                        var result2 = YetAnotherEnum.First.ToString();
                        AssertValue(YetAnotherEnum.Second);
                        
                        void AssertValue(MyEnum value)
                        {
                            var toString = value.ToString();
                        }

                        void AssertValue(AnotherEnum value)
                        {
                            var toString = value.ToString();
                        }

                        void AssertValue(YetAnotherEnum value)
                        {
                            var toString = value.ToString();
                        }
                    }
                }
            }
            """;
        var (diagnostics, output) = 
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_interceptionEnabled, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task DoesNotInterceptEnumMarkedAsNotInterceptable()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions(IsInterceptable = false)]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                    Third = 2,
                }

                [EnumExtensions]
                [Flags]
                internal enum AnotherEnum
                {
                    First = 0,
                    Second = 1,
                    Third = 2,
                }

                internal enum YetAnotherEnum
                {
                    First = 0,
                    Second = 1,
                    Third = 2,
                }
                
                public class InnerClass
                {
                    public void MyTest()
                    {
                        var result = AnotherEnum.First.ToString();
                        AssertValue(MyEnum.First);
                        AssertValue(AnotherEnum.Second);
                        AssertValue(MyEnum.Third);
                        var result2 = YetAnotherEnum.First.ToString();
                        AssertValue(YetAnotherEnum.Second);
                        
                        var hasValue = AnotherEnum.Second.HasFlag(AnotherEnum.First);
                        
                        void AssertValue(MyEnum value)
                        {
                            var toString = value.ToString();
                        }

                        void AssertValue(AnotherEnum value)
                        {
                            var toString = value.ToString();
                        }

                        void AssertValue(YetAnotherEnum value)
                        {
                            var toString = value.ToString();
                        }
                    }
                }
            }
            """;
        var (diagnostics, output) = 
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_interceptionEnabled, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanInterceptExternalEnumToString()
    {
        const string input =
            """
            using System;
            using NetEscapades.EnumGenerators;
            
            [assembly:EnumExtensions<StringComparison>()]

            namespace MyTestNameSpace
            {
                public class InnerClass
                {
                    public StringComparison _field = default;
                    public StringComparison Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = StringComparison.Ordinal;
                        var var1 = myValue.ToString();
                        var var2 = StringComparison.Ordinal.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                        AssertValue(StringComparison.OrdinalIgnoreCase);

                        void AssertValue(StringComparison value)
                        {
                            var toString = value.ToString();
                        }
                    }
                }
            }
            """;
        var (diagnostics, output) = 
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_interceptionEnabled, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanInterceptUsingInterceptableAttributeToString()
    {
        const string input =
            """
            using System;
            using NetEscapades.EnumGenerators;
            
            [assembly:EnumExtensions<StringComparison>(IsInterceptable = false)]
            [assembly:Interceptable<StringComparison>]

            namespace MyTestNameSpace
            {
                public class InnerClass
                {
                    public StringComparison _field = default;
                    
                    public StringComparison Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = StringComparison.Ordinal;
                        var var1 = myValue.ToString();
                        var var2 = StringComparison.Ordinal.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                        AssertValue(StringComparison.OrdinalIgnoreCase);

                        void AssertValue(StringComparison value)
                        {
                            var toString = value.ToString();
                        }
                    }
                }
            }
            """;
        var (diagnostics, output) = 
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_interceptionEnabled, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task DoesNotInterceptExternalEnumMarkedAsNotInterceptable()
    {
        const string input =
            """
            using System;
            using NetEscapades.EnumGenerators;
            
            [assembly:EnumExtensions<StringComparison>()]
            [assembly:NetEscapades.EnumGenerators.EnumExtensions<DateTimeKind>(IsInterceptable = false)]

            namespace MyTestNameSpace
            {
                public class InnerClass
                {
                    public StringComparison _field = default;
                    public StringComparison Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = StringComparison.Ordinal;
                        var var1 = myValue.ToString();
                        var var2 = StringComparison.Ordinal.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                        AssertValue(StringComparison.OrdinalIgnoreCase);
                        
                        var var5 = DateTimeKind.Unspecified.ToString();

                        void AssertValue(StringComparison value)
                        {
                            var toString = value.ToString();
                        }
                    }
                }
            }
            """;
        var (diagnostics, output) = 
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_interceptionEnabled, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanInterceptToStringWhenCsharp11()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                }
                
                public class InnerClass
                {
                    public MyEnum _field = default;
                    public MyEnum Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = MyEnum.Second;
                        var var1 = myValue.ToString();
                        var var2 = MyEnum.Second.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                    }
                }
            }
            """;
        var opts = new TestHelpers.Options(LanguageVersion.CSharp11, _interceptionEnabled, input);
        var (diagnostics, output) =
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(opts);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task DoesNotInterceptToStringWhenDisabled()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                }
                
                public class InnerClass
                {
                    public MyEnum _field = default;
                    public MyEnum Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = MyEnum.Second;
                        var var1 = myValue.ToString();
                        var var2 = MyEnum.Second.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                    }
                }
            }
            """;
        var (diagnostics, output) =
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task DoesNotInterceptToStringWhenOldCsharp()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                }
                
                public class InnerClass
                {
                    public MyEnum _field = default;
                    public MyEnum Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = MyEnum.Second;
                        var var1 = myValue.ToString();
                        var var2 = MyEnum.Second.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                    }
                }
            }
            """;
        var opts = new TestHelpers.Options(LanguageVersion.CSharp10, _interceptionEnabled, input);
        var (diagnostics, output) =
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(opts);

        diagnostics.Should().ContainSingle(x => x.Id == DiagnosticHelper.CsharpVersionLooLow.Id);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

#else
    [Fact]
    public Task CanNotInterceptEnumToStringInOldSDK()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                }
                
                public class InnerClass
                {
                    public MyEnum _field = default;
                    public MyEnum Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = MyEnum.Second;
                        var var1 = myValue.ToString();
                        var var2 = MyEnum.Second.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                    }
                }
            }
            """;
        var (diagnostics, output) =
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_interceptionEnabled, input));

        diagnostics.Should().ContainSingle(x => x.Id == DiagnosticHelper.SdkVersionTooLow.Id);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

#endif
}