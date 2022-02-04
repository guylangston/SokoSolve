using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace TextRenderZ.Tests;

public class StringUtilTests
{
    [Theory]
    [InlineData("HelloWorld", "Hello World")]
    [InlineData("ABC", "ABC")]
    public void UnCamel(string inp, string oup)
    {
        var res = StringUtil.UnCamel(inp);
        Assert.Equal(oup, res);
    }
    
    [Theory]
    [InlineData("Hello {World}!", "Hello XXX!")]
    public void ParseAndReplaceVariables(string inp, string oup)
    {
        var res = StringUtil.ParseAndReplaceVariables(inp, x=>"XXX");
        Assert.Equal(oup, res);
    }
}

public class GenDocs
{
    private ITestOutputHelper outp;

    public GenDocs(ITestOutputHelper outp)
    {
        this.outp = outp;
    }

    [Fact]
    public void Strings()
    {
        var type = typeof(StringUtil);
        foreach (var method in DescribeMethod(type))
        {
            outp.WriteLine(method.ToString());
        }    
    }

    public static IEnumerable<string> DescribeMethod(Type type)
    {
        foreach (var method in type.GetMethods())
        {
            yield return method.ToString();
        }    
    }

    
}